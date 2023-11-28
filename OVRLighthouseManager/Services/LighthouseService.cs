using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using Windows.Devices.Enumeration;
using Serilog;
using System.Collections.Generic;

namespace OVRLighthouseManager.Services;
public class LighthouseService : ILighthouseService
{
    public event EventHandler<LighthouseDevice> OnFound = delegate { };
    public IReadOnlyList<LighthouseDevice> KnownLighthouses => _knownLighthouses;
    private readonly List<LighthouseDevice> _knownLighthouses = new();
    private readonly List<DeviceInformation> _identifyingDevices = new();
    private readonly List<DeviceInformation> _notLighthouses = new();

    private readonly DeviceWatcher _watcher;
    private bool _isScanning;

    public LighthouseService()
    {
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
        Log.Debug(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
        _watcher = DeviceInformation.CreateWatcher
            (BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
            requestedProperties,
            DeviceInformationKind.AssociationEndpoint);
        _watcher.Added += DeviceWatcher_Added;
        _watcher.Updated += DeviceWatcher_Updated;
        _watcher.Removed += DeviceWatcher_Removed;
        _watcher.EnumerationCompleted += (sender, arg) =>
        {
            Log.Debug("EnumerationCompleted");
            sender.Stop();
        };
        _watcher.Stopped += (sender, arg) =>
        {
            Log.Debug("Stopped");
        };
    }

    public void StartScan()
    {
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

    internal static void CheckBluetoothAdapter()
    {
        if (BluetoothAdapter.GetDefaultAsync().AsTask().Result == null)
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

    private bool IsKnownDevice(DeviceInformation device)
    {
        var address = GetBluetoothAddress(device);
        return _knownLighthouses.Any(l => l.BluetoothAddress == address);
    }

    private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation device)
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
            Log.Debug($"{device.Name} is known lighthouse");
            return;
        }

        Log.Debug("Found BLE Device: {@id}", device.Name);

        var lighthouse = await LighthouseDevice.FromBluetoothAddressAsync(address);

        var result = await lighthouse.Identify();
        switch (result)
        {
            case LighthouseDevice.DeviceType.Lighthouse:
                _knownLighthouses.Add(lighthouse);
                OnFound(this, lighthouse);
                _identifyingDevices.Remove(device);
                break;
            case LighthouseDevice.DeviceType.NotLighthouse:
                Log.Debug($"{device.Name} is not a lighthouse");
                _notLighthouses.Add(device);
                break;
            case LighthouseDevice.DeviceType.Unknown:
                Log.Debug($"{device.Name} is unknown");
                _identifyingDevices.Remove(device);
                break;
        }
    }

    private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate device)
    {
        if (_notLighthouses.Any(d => d.Id == device.Id))
        {
            return;
        }
        var identifying = _identifyingDevices.FirstOrDefault(d => d.Id == device.Id);
        if (identifying == null)
        {
            return;
        }

        Log.Debug("Updated {@id}", device.Id);

        var lighthouse = await LighthouseDevice.FromIdAsync(device.Id);
        var result = await lighthouse.Identify();
        switch (result)
        {
            case LighthouseDevice.DeviceType.Lighthouse:
                _knownLighthouses.Add(lighthouse);
                OnFound(this, lighthouse);
                _identifyingDevices.Remove(identifying);
                break;
            case LighthouseDevice.DeviceType.NotLighthouse:
                Log.Debug($"{lighthouse.Name} is not a lighthouse");
                _notLighthouses.Add(identifying);
                break;
            case LighthouseDevice.DeviceType.Unknown:
                Log.Debug($"{lighthouse.Name} is unknown");
                _identifyingDevices.Remove(identifying);
                break;
        }
    }

    private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate device)
    {
    }
}
