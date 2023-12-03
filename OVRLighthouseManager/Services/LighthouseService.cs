using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using Serilog;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Custom;
using Windows.Devices.Enumeration;
using static OVRLighthouseManager.Models.LighthouseDevice;

namespace OVRLighthouseManager.Services;
public class LighthouseService : ILighthouseService
{
    private class KnownDevice
    {
        public string Name
        {
            get; set;
        } = "";
        public ulong BluetoothAddress
        {
            get; set;
        }
        public string Id
        {
            get; set;
        } = "";
        public KnownDeviceType DeviceType
        {
            get; set;
        }

        public enum KnownDeviceType
        {
            Identifying,
            Unknown,
            NotLighthouse,
            Lighthouse,
        }
    }

    public event EventHandler<LighthouseDevice> OnFound = delegate { };
    public IReadOnlyList<LighthouseDevice> KnownLighthouses => _knownLighthouses;
    private readonly List<LighthouseDevice> _knownLighthouses = new();
    private readonly List<KnownDevice> _knownDevices = new();

    private readonly DeviceWatcher _watcher;

    public bool IsScanning => _isScanning;
    private bool _isScanning;

    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    private readonly ILogger _log = LogHelper.ForContext<LighthouseService>();

    public LighthouseService()
    {
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
        _watcher = DeviceInformation.CreateWatcher
            (BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
            requestedProperties,
            DeviceInformationKind.AssociationEndpoint);
        _watcher.Added += DeviceWatcher_Added;
        _watcher.Updated += DeviceWatcher_Updated;
        _watcher.Removed += DeviceWatcher_Removed;
        _watcher.EnumerationCompleted += (sender, arg) =>
        {
            _log.Debug("DeviceWatcher EnumerationCompleted");
            StopScan();
        };
        _watcher.Stopped += (sender, arg) =>
        {
            _log.Debug("DeviceWatcher Stopped");
        };

        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }

    public void StartScan()
    {
        _log.Debug("StartScan");
        CheckBluetoothAdapter();
        if (_isScanning)
        {
            return;
        }
        _isScanning = true;
        _watcher.Start();
    }

    public void StopScan()
    {
        _log.Debug("StopScan");
        if (!_isScanning)
        {
            return;
        }
        _watcher.Stop();
        _isScanning = false;
    }

    public LighthouseDevice? GetLighthouse(ulong bluetoothAddress)
    {
        var lighthouse = _knownLighthouses.FirstOrDefault(l => l.BluetoothAddress == bluetoothAddress);
        if (lighthouse != null)
        {
            return lighthouse;
        }
        return null;
    }

    public LighthouseDevice? GetLighthouse(string bluetoothAddress)
    {
        var address = AddressToStringConverter.StringToAddress(bluetoothAddress);
        return GetLighthouse(address);
    }

    public void RemoveLighthouse(string bluetoothAddress)
    {
        var address = AddressToStringConverter.StringToAddress(bluetoothAddress);
        var device = _knownLighthouses.FirstOrDefault(l => l.BluetoothAddress == address);
        if (device != null)
        {
            _knownLighthouses.Remove(device);
            device.Dispose();
        }
        var known = _knownDevices.FirstOrDefault(d => d.BluetoothAddress == address);
        if (known != null)
        {
            _knownDevices.Remove(known);
        }
    }

    public bool HasBluetoothLEAdapter()
    {
        var adaper = BluetoothAdapter.GetDefaultAsync().AsTask().Result;
        if (adaper == null)
        {
            return false;
        }
        return adaper.IsLowEnergySupported;
    }

    internal void CheckBluetoothAdapter()
    {
        if (!HasBluetoothLEAdapter())
        {
            throw new Exception("Bluetooth is not available");
        }
    }

    private static ulong GetBluetoothAddress(DeviceInformation device)
    {
        if (device.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out var addressString))
        {
            return AddressToStringConverter.StringToAddress((string)addressString);
        }
        throw new Exception("Failed to get Bluetooth address");
    }

    private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation device)
    {
        _dispatcherQueue.TryEnqueue(async () =>
        {
            if (device.Name.StartsWith("LHB-"))
            {
                var address = GetBluetoothAddress(device);
                if (device.Name == "")
                {
                    return;
                }
                var known = _knownDevices.FirstOrDefault(d => d.Id == device.Id);
                if (known != null && known.DeviceType != KnownDevice.KnownDeviceType.Unknown)
                {
                    return;
                }

                if (_knownLighthouses.Any(l => l.BluetoothAddress == address))
                {
                    return;
                }

                _log.Information($"Found possible lighthouse: {device.Name} ({address:X012})");
                var knownDevice = known ?? new KnownDevice()
                {
                    Name = device.Name,
                    BluetoothAddress = address,
                    Id = device.Id,
                };
                await OnFoundPossibleLighthouse(knownDevice);
            }
        });
    }

    private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate device)
    {
        _dispatcherQueue.TryEnqueue(async () =>
        {
            var known = _knownDevices.FirstOrDefault(d => d.Id == device.Id);
            if (known == null)
            {
                return;
            }
            if (known.DeviceType != KnownDevice.KnownDeviceType.Unknown)
            {
                return;
            }

            if (known.Name.StartsWith("LHB-"))
            {
                _log.Information($"Updated possible lighthouse: {known.Name} ({known.BluetoothAddress:X012})");
                await OnFoundPossibleLighthouse(known);
            }
        });
    }

    private async Task OnFoundPossibleLighthouse(KnownDevice device)
    {
        try
        {
            var lighthouse = await LighthouseDevice.FromBluetoothAddressAsync(device.BluetoothAddress);
            lighthouse.OnDisconnected += (sender, args) =>
            {
                _log.Information("{$name} has been disconnected", lighthouse.Name);
            };
            if (!_knownLighthouses.Any(l => l.BluetoothAddress == lighthouse.BluetoothAddress))
            {
                _knownDevices.Add(device);
            }
            device.DeviceType = KnownDevice.KnownDeviceType.Identifying;

            var result = await lighthouse.Identify();
            switch (result)
            {
                case LighthouseDevice.DeviceType.Lighthouse:
                    _log.Information($"{device.Name} is a lighthouse");
                    _knownLighthouses.Add(lighthouse);
                    OnFound(this, lighthouse);
                    _knownDevices.Remove(device);
                    break;
                case LighthouseDevice.DeviceType.NotLighthouse:
                    _log.Debug($"{device.Name} is not a lighthouse");
                    device.DeviceType = KnownDevice.KnownDeviceType.NotLighthouse;
                    break;
                case LighthouseDevice.DeviceType.Unknown:
                    _log.Debug($"{device.Name} is unknown device");
                    device.DeviceType = KnownDevice.KnownDeviceType.Unknown;
                    break;
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to identify {device.Name} ({device.BluetoothAddress:X012})");
            device.DeviceType = KnownDevice.KnownDeviceType.Unknown;
        }
    }

    private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate device)
    {
    }
}
