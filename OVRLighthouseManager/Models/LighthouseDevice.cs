
using System.Runtime.InteropServices.WindowsRuntime;
using Serilog;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace OVRLighthouseManager.Models;

public class LighthouseDevice
{
    public enum DeviceType
    {
        Unknown,
        Lighthouse,
        NotLighthouse,
    };

    public string Name => _device?.Name ?? "(Unknown)";
    public ulong BluetoothAddress => _device?.BluetoothAddress ?? 0;

    public bool IsInitialized => _powerCharacteristic != null;

    private readonly BluetoothLEDevice _device;
    private GattDeviceService? _controlService;
    private GattCharacteristic? _powerCharacteristic;

    private static readonly Guid ControlService = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid PowerCharacteristic = new("00001525-1212-efde-1523-785feabcd124");

    private LighthouseDevice(BluetoothLEDevice device)
    {
        _device = device;
    }

    internal static async Task<LighthouseDevice> FromBluetoothAddressAsync(ulong bluetoothAddress)
    {
        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
        return new LighthouseDevice(device);
    }

    internal static async Task<LighthouseDevice> FromIdAsync(string deviceId)
    {
        var device = await BluetoothLEDevice.FromIdAsync(deviceId);
        return new LighthouseDevice(device);
    }

    public async Task<DeviceType> Identify()
    {
        if (_controlService == null)
        {
            var result = await _device.GetGattServicesAsync(BluetoothCacheMode.Cached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                _controlService = result.Services.FirstOrDefault(s => s.Uuid == ControlService);
            } else
            {
                return DeviceType.Unknown;
            }

            if (_controlService == null)
            {
                return DeviceType.NotLighthouse;
            }
        }

        if (_powerCharacteristic == null)
        {
            var result = await _controlService.GetCharacteristicsAsync(BluetoothCacheMode.Cached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                _powerCharacteristic = result.Characteristics.FirstOrDefault(c => c.Uuid == PowerCharacteristic);
            }
            else
            {
                return DeviceType.Unknown;
            }

            if (_powerCharacteristic == null)
            {
                return DeviceType.NotLighthouse;
            }
        }

        return DeviceType.Lighthouse;
    }

    public async Task<bool> PowerOnAsync()
    {
        if (_powerCharacteristic == null)
        {
            throw new Exception("Power characteristic is null");
        }
        return await WriteCharacteristicAsync(_powerCharacteristic, 0x01);
    }

    public async Task<bool> SleepAsync()
    {
        if (_powerCharacteristic == null)
        {
            throw new Exception("Power characteristic is null");
        }
        return await WriteCharacteristicAsync(_powerCharacteristic, 0x00);
    }

    public async Task<bool> StandbyAsync()
    {
        if (_powerCharacteristic == null)
        {
            throw new Exception("Power characteristic is null");
        }
        return await WriteCharacteristicAsync(_powerCharacteristic, 0x02);
    }


    private async Task<bool> WriteCharacteristicAsync(GattCharacteristic characteristic, byte data)
    {
        const int retryCount = 5;
        for (var i = 0; i < retryCount; i++)
        {
            var result = await characteristic.WriteValueAsync(new byte[] { data }.AsBuffer());
            switch (result)
            {
                case GattCommunicationStatus.Success:
                    return true;
                case GattCommunicationStatus.Unreachable:
                    Log.Information($"Failed to write characteristic for {Name} ({BluetoothAddress:X012}) : {result}");
                    continue;
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    Log.Information($"Failed to write characteristic for {Name} ({BluetoothAddress:X012}) : {result}");
                    return false;
            }
        }
        return false;
    }
}
