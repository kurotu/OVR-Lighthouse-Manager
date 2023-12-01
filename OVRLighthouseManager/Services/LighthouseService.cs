using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using Serilog;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace OVRLighthouseManager.Services;
public class LighthouseService : ILighthouseService
{
    public event EventHandler<LighthouseDevice> OnFound = delegate { };
    public IReadOnlyList<LighthouseDevice> KnownLighthouses => _knownLighthouses;
    private readonly List<LighthouseDevice> _knownLighthouses = new();
    private readonly List<DeviceInformation> _identifyingDevices = new();
    private readonly List<DeviceInformation> _notLighthouses = new();

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
            var address = GetBluetoothAddress(device);
            if (device.Name == "")
            {
                return;
            }
            if (_notLighthouses.Any(d => d.Id == device.Id))
            {
                return;
            }
            if (_identifyingDevices.Any(l => l.Id == device.Id))
            {
                return;
            }

            if (_knownLighthouses.Any(l => l.BluetoothAddress == address))
            {
                return;
            }

            if (device.Name.StartsWith("LHB-"))
            {
                try
                {
                    _log.Information($"Found possible lighthouse: {device.Name} ({address:X012})");

                    var lighthouse = await LighthouseDevice.FromBluetoothAddressAsync(address);
                    lighthouse.OnDisconnected += (sender, args) =>
                    {
                        _log.Information("{$name} has been disconnected", lighthouse.Name);
                        if (_knownLighthouses.Contains(lighthouse))
                        {
                            _knownLighthouses.Remove(lighthouse);
                        }
                        lighthouse.Dispose();
                    };

                    var result = await lighthouse.Identify();
                    switch (result)
                    {
                        case LighthouseDevice.DeviceType.Lighthouse:
                            _log.Information($"{device.Name} is a lighthouse");
                            _knownLighthouses.Add(lighthouse);
                            OnFound(this, lighthouse);
                            _identifyingDevices.Remove(device);
                            break;
                        case LighthouseDevice.DeviceType.NotLighthouse:
                            _log.Debug($"{device.Name} is not a lighthouse");
                            _notLighthouses.Add(device);
                            break;
                        case LighthouseDevice.DeviceType.Unknown:
                            _log.Debug($"{device.Name} is unknown device");
                            _identifyingDevices.Remove(device);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"Failed to identify {device.Name} ({address:X012})");
                }
            }
        });
    }

    private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate device)
    {
    }

    private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate device)
    {
    }
}
