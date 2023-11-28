
using System.Runtime.InteropServices.WindowsRuntime;
using Serilog;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace OVRLighthouseManager.Models;

public class LighthouseDevice : IDisposable
{
    public string Name => _device?.Name ?? "(Unknown)";
    public ulong BluetoothAddress => _device?.BluetoothAddress ?? 0;

    private BluetoothLEDevice? _device;
    private GattDeviceService? _controlService;
    private GattCharacteristic? _powerCharacteristic;

    private static readonly Guid ControlService = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid PowerCharacteristic = new("00001525-1212-efde-1523-785feabcd124");

    internal static async Task<LighthouseDevice?> FromBluetoothAddressAsync(ulong bluetoothAddress)
    {
        var device = new LighthouseDevice();

        const int retryCount = 5;

        device._device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
        if (device._device == null)
        {
            Log.Information($"Failed to get device from address {bluetoothAddress:X012})");
            return null;
        }

        {
            GattDeviceServicesResult? servicesResult = null;
            for (var i = 0; i < retryCount; i++)
            {
                var shouldBreak = false;
                servicesResult = await device._device.GetGattServicesForUuidAsync(ControlService);
                switch (servicesResult.Status)
                {
                    case GattCommunicationStatus.Success:
                        {
                            shouldBreak = true;
                            var services = servicesResult.Services;
                            if (services.Count == 0)
                            {
                                Log.Information($"No control services found for {device.Name} ({bluetoothAddress:X012})");
                                return null;
                            }
                            device._controlService = services[0];
                        }
                        break;
                    case GattCommunicationStatus.Unreachable:
                        continue;
                    case GattCommunicationStatus.ProtocolError:
                    case GattCommunicationStatus.AccessDenied:
                        Log.Information($"Unknown error getting control service for {device.Name} ({bluetoothAddress:X012}) : {servicesResult.Status}");
                        return null;
                }
                if (shouldBreak)
                {
                    break;
                }
            }
            if (device._controlService == null)
            {
                Log.Information($"Failed to get control service for {device.Name} ({bluetoothAddress:X012}) : {servicesResult?.Status}");
                device.Dispose();
                return null;
            }
        }

        {
            GattCharacteristicsResult? characteristicsResult = null;
            for (var i = 0; i < retryCount; i++)
            {
                var shouldBreak = false;
                characteristicsResult = await device._controlService.GetCharacteristicsForUuidAsync(PowerCharacteristic);
                switch (characteristicsResult.Status)
                {
                    case GattCommunicationStatus.Success:
                        {
                            shouldBreak = true;
                            var characteristics = characteristicsResult.Characteristics;
                            if (characteristics.Count == 0)
                            {
                                Log.Information($"No power characteristics found for {device.Name} ({bluetoothAddress:X012})");
                                return null;
                            }
                            device._powerCharacteristic = characteristics[0];
                        }
                        break;
                    case GattCommunicationStatus.Unreachable:
                        continue;
                    case GattCommunicationStatus.ProtocolError:
                    case GattCommunicationStatus.AccessDenied:
                        Log.Information($"Unknown error getting power characteristic for {device.Name} ({bluetoothAddress:X012}) : {characteristicsResult?.Status}");
                        return null;
                }
                if (shouldBreak)
                {
                    break;
                }
            }
            if (device._powerCharacteristic == null)
            {
                Log.Information($"Failed to get power characteristic for {device.Name} ({bluetoothAddress:X012}) : {characteristicsResult?.Status}");
                device.Dispose();
                return null;
            }
            return device;
        }
    }

    public void Dispose()
    {
        _controlService?.Dispose();
        _device?.Dispose();
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
