using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Contracts.Services;

namespace OVRLighthouseManager.Services;
public class LighthouseService : ILighthouseService
{
    public event EventHandler<LighthouseDevice> OnFound = delegate { };

    private static readonly Guid ControlService = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid PowerCharacteristic = new("00001525-1212-efde-1523-785feabcd124");

    private readonly BluetoothLEAdvertisementWatcher _watcher = new();
    private readonly HashSet<ulong> _knownAddresses = new();
    private readonly List<Task> _checkingTasks = new();

    public void StartScan()
    {
        CheckBluetoothAdapter();
        _knownAddresses.Clear();
        _checkingTasks.Clear();
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
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(arg.BluetoothAddress);
                if (device == null)
                {
                    _knownAddresses.Remove(arg.BluetoothAddress);
                    return;
                }

                if (await IsLightHouse(device))
                {
                    var d = new LighthouseDevice()
                    {
                        BluetoothAddress = arg.BluetoothAddress,
                        Name = device.Name
                    };
                    OnFound(arg.BluetoothAddress, d);
                }
                else
                {
                    _knownAddresses.Remove(arg.BluetoothAddress);
                }
            });
            _checkingTasks.Add(task);
        };
        _watcher.Start();
    }

    public void StopScan()
    {
        _watcher.Stop();
        Task.WaitAll(_checkingTasks.ToArray());
    }

    internal static async Task<bool> PowerOn(ulong address)
    {
        CheckBluetoothAdapter();
        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
        return await WritePowerCharacteristic(device, 1);
    }

    internal static async Task<bool> Sleep(ulong address)
    {
        CheckBluetoothAdapter();
        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
        return await WritePowerCharacteristic(device, 0);
    }

    internal static async Task<bool> Standby(ulong address)
    {
        CheckBluetoothAdapter();
        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
        return await WritePowerCharacteristic(device, 2);
    }

    internal static void CheckBluetoothAdapter()
    {
        if (BluetoothAdapter.GetDefaultAsync().AsTask().Result == null)
        {
            throw new Exception("Bluetooth is not available");
        }
    }

    private static async Task<bool> IsLightHouse(BluetoothLEDevice device)
    {
        var services = await device.GetGattServicesForUuidAsync(ControlService);
        if (services.Status != GattCommunicationStatus.Success)
        {
            return false;
        }
        if (services.Services.Count == 0)
        {
            return false;
        }

        var characteristics = await services.Services[0].GetCharacteristicsForUuidAsync(PowerCharacteristic);
        if (characteristics.Status != GattCommunicationStatus.Success)
        {
            return false;
        }
        if (characteristics.Characteristics.Count == 0)
        {
            return false;
        }

        return true;
    }

    private static async Task<bool> WritePowerCharacteristic(BluetoothLEDevice device, byte value)
    {
        var services = await device.GetGattServicesForUuidAsync(ControlService);
        if (services.Status != GattCommunicationStatus.Success)
        {
            return false;
        }
        if (services.Services.Count == 0)
        {
            return false;
        }

        var characteristics = await services.Services[0].GetCharacteristicsForUuidAsync(PowerCharacteristic);
        if (characteristics.Status != GattCommunicationStatus.Success)
        {
            return false;
        }
        if (characteristics.Characteristics.Count == 0)
        {
            return false;
        }

        var characteristic = characteristics.Characteristics[0];
        await characteristic.WriteValueAsync(new byte[1] { value }.AsBuffer());
        return true;
    }
}
