using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;

namespace OVRLighthouseManager.Services;
public class LighthouseService : ILighthouseService
{
    public event EventHandler<LighthouseDevice> OnFound = delegate { };

    public List<LighthouseDevice> KnownDevices => _knownDevices;

    private readonly BluetoothLEAdvertisementWatcher _watcher = new();
    private readonly List<LighthouseDevice> _knownDevices = new();
    private readonly List<ulong> _knownAddresses = new();
    private readonly List<Task> _checkingTasks = new();

    public LighthouseService()
    {
        _watcher.ScanningMode = BluetoothLEScanningMode.Active;
        _watcher.Received += (sender, arg) =>
        {
            if (_knownAddresses.Contains(arg.BluetoothAddress))
            {
                return;
            }
            _knownAddresses.Add(arg.BluetoothAddress);
            var task = Task.Run(async () =>
            {
                var device = await LighthouseDevice.FromBluetoothAddressAsync(arg.BluetoothAddress);
                if (device == null)
                {
                    return;
                }
                if (!_knownDevices.Any(d => d.BluetoothAddress == arg.BluetoothAddress))
                {
                    _knownDevices.Add(device);
                }
                OnFound(this, device);
            });
            _checkingTasks.Add(task);
        };
    }

    public void StartScan()
    {
        CheckBluetoothAdapter();
        _knownAddresses.Clear();
        _checkingTasks.Clear();
        _watcher.Start();
    }

    public void StopScan()
    {
        _watcher.Stop();
        Task.WaitAll(_checkingTasks.ToArray());
    }

    public async Task StopScanAsync()
    {
        _watcher.Stop();
        await Task.WhenAll(_checkingTasks.ToArray());
    }

    public void AddDevice(LighthouseDevice device)
    {
        if (!_knownDevices.Any(d => d.BluetoothAddress == device.BluetoothAddress))
        {
            _knownDevices.Add(device);
        }
    }

    public async Task<LighthouseDevice> GetDeviceAsync(ulong bluetoothAddress)
    {
        var device = _knownDevices.FirstOrDefault(d => d.BluetoothAddress == bluetoothAddress);
        if (device == null)
        {
            device = await LighthouseDevice.FromBluetoothAddressAsync(bluetoothAddress);
            if (device == null)
            {
                throw new Exception($"Could not get device {bluetoothAddress:X012}");
            }
            AddDevice(device);
        }
        return device;
    }

    public async Task<LighthouseDevice> GetDeviceAsync(string bluetoothAddress)
    {
        var address = AddressToStringConverter.StringToAddress(bluetoothAddress);
        return await GetDeviceAsync(address);
    }

    internal static void CheckBluetoothAdapter()
    {
        if (BluetoothAdapter.GetDefaultAsync().AsTask().Result == null)
        {
            throw new Exception("Bluetooth is not available");
        }
    }
}
