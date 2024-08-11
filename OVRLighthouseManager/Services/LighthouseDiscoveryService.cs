using System.Diagnostics;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using Serilog;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace OVRLighthouseManager.Services;
public class LighthouseDiscoveryService : ILighthouseDiscoveryService
{
    public bool IsDiscovering => _isDiscovering;
    public IReadOnlyCollection<Lighthouse> FoundLighthouses => _foundLighthouses.Values;

    public event EventHandler<Lighthouse> Found = delegate { };

    private bool _isDiscovering = false;
    private readonly DeviceWatcher _watcher;
    private readonly DeviceWatcher _pairedDeviceWatcher;

    private readonly Dictionary<string, Lighthouse> _foundLighthouses = new();
    private readonly ILogger _log = LogHelper.ForContext<LighthouseDiscoveryService>();

    public LighthouseDiscoveryService()
    {
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

        _watcher = DeviceInformation.CreateWatcher(
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
            requestedProperties,
            DeviceInformationKind.AssociationEndpoint);

        _watcher.Added += DeviceWatcher_Added;
        _watcher.Updated += (sender, arg) => { };
        _watcher.Removed += (sender, arg) => { };
        _watcher.EnumerationCompleted += (sender, arg) =>
        {
            _log.Debug("DeviceWatcher EnumerationCompleted");
            StopDiscovery();
        };
        _watcher.Stopped += (sender, arg) =>
        {
            _log.Debug("DeviceWatcher Stopped");
        };

        _pairedDeviceWatcher = DeviceInformation.CreateWatcher(
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(true),
            requestedProperties,
            DeviceInformationKind.Device);
        _pairedDeviceWatcher.Added += DeviceWatcher_Added;
    }

    public void StartDiscovery()
    {
        if (_isDiscovering)
        {
            return;
        }
        _isDiscovering = true;
        _foundLighthouses.Clear();
        _watcher.Start();
        _pairedDeviceWatcher.Start();
    }

    public void StopDiscovery()
    {
        _watcher.Stop();
        _pairedDeviceWatcher.Stop();
        _isDiscovering = false;
    }

    private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
    {
        _log.Debug("DeviceWatcher Added: {Name} ({Id})", args.Name, args.Id);
        var isLighthouse = new Lighthouse { Name = args.Name }.Version != LighthouseVersion.Unknown;
        if (isLighthouse && !_foundLighthouses.ContainsKey(args.Id))
        {
            using var device = BluetoothLEDevice.FromIdAsync(args.Id).AsTask().Result;
            if (device == null)
            {
                _log.Error("Failed to get BluetoothLEDevice for {Name} ({Id})", args.Name, args.Id);
                return;
            }
            var lighthouse = new Lighthouse { Name = args.Name, BluetoothAddress = AddressToStringConverter.AddressToString(device.BluetoothAddress) };
            _foundLighthouses[args.Id] = lighthouse;
            _log.Information($"Found: {lighthouse.Name} ({AddressToStringConverter.AddressToString(lighthouse.BluetoothAddressValue)})");
            Found.Invoke(this, lighthouse);
        }
    }
}
