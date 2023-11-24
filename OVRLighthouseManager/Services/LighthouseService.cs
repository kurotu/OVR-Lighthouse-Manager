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

    private readonly SynchronizationContext? _context;

    public LighthouseService()
    {
        _context = SynchronizationContext.Current;
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
                try
                {
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(arg.BluetoothAddress);
                    if (device == null)
                    {
                        _knownAddresses.Remove(arg.BluetoothAddress);
                        return;
                    }

                    if (device.Name == string.Empty)
                    {
                        return;
                    }

                    if (await IsLightHouse(device))
                    {
                        var d = new LighthouseDevice()
                        {
                            BluetoothAddress = arg.BluetoothAddress,
                            Name = device.Name
                        };
                        _context?.Post((d2) =>
                        {
                            OnFound(this, d2 as LighthouseDevice);
                        }, d);
                    }
                    else
                    {
                        _knownAddresses.Remove(arg.BluetoothAddress);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            });
            _checkingTasks.Add(task);
        };
    }

    public void StartScan()
    {
        /*
        OnFound(this, new LighthouseDevice() { Name = "Test", BluetoothAddress = 0x123456789AB });
        OnFound(this, new LighthouseDevice() { Name = "Test2", BluetoothAddress = 0x23456789ABC });
        OnFound(this, new LighthouseDevice() { Name = "Test3", BluetoothAddress = 0x3456789ABCD });
        OnFound(this, new LighthouseDevice() { Name = "Test4", BluetoothAddress = 0x456789ABCDE });
        OnFound(this, new LighthouseDevice() { Name = "Test5", BluetoothAddress = 0x56789ABCDEF });
        OnFound(this, new LighthouseDevice() { Name = "Test6", BluetoothAddress = 0x6789ABCDEF0 });
        OnFound(this, new LighthouseDevice() { Name = "Test7", BluetoothAddress = 0x789ABCDEF01 });
        OnFound(this, new LighthouseDevice() { Name = "Test8", BluetoothAddress = 0x89ABCDEF012 });
        OnFound(this, new LighthouseDevice() { Name = "Test9", BluetoothAddress = 0x9ABCDEF0123 });
        return;
        */
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
