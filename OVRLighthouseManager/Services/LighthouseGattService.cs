using System.Runtime.InteropServices.WindowsRuntime;
using Serilog;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Exceptions;
using OVRLighthouseManager.Helpers;

namespace OVRLighthouseManager.Services;

class LighthouseGattService : ILighthouseGattService
{
    private static readonly Guid ControlService = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid PowerCharacteristic = new("00001525-1212-efde-1523-785feabcd124");
    private static readonly ILogger _log = LogHelper.ForContext<LighthouseGattService>();

    private readonly ILighthouseDiscoveryService _lighthouseDiscoveryService;

    public LighthouseGattService(ILighthouseDiscoveryService lighthouseDiscoveryService)
    {
        _lighthouseDiscoveryService = lighthouseDiscoveryService;
    }

    public async Task PowerOnAsync(Lighthouse lighthouse)
    {
        await WritePowerCharacteristic(lighthouse, 0x01);
    }

    public async Task SleepAsync(Lighthouse lighthouse)
    {
        await WritePowerCharacteristic(lighthouse, 0x00);
    }

    public async Task StandbyAsync(Lighthouse lighthouse)
    {
        await WritePowerCharacteristic(lighthouse, 0x02);
    }

    private async Task WritePowerCharacteristic(Lighthouse lighthouse, byte data)
    {
        var device = await GetBluetoothLEDeviceAsync(lighthouse.BluetoothAddress);
        using var service = await GetControlService(device);
        var characteristic = await GetPowerCharacteristic(service);
        await WriteCharacteristicAsync(characteristic, data);
    }

    private async Task<BluetoothLEDevice> GetBluetoothLEDeviceAsync(ulong address)
    {
        const int retryCount = 5;
        for (var i = 0; i < retryCount; i++)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            if (device != null)
            {
                return device;
            }
            _lighthouseDiscoveryService.StartDiscovery();
            await Task.Delay(200);
        }
        throw new LighthouseGattException("Lighthouse not found");
    }

    private static async Task<GattDeviceService> GetControlService(BluetoothLEDevice device)
    {
        const int retryCount = 5;
        var status = GattCommunicationStatus.Success;
        for (var i = 0; i < retryCount; i++)
        {
            var result = await device.GetGattServicesForUuidAsync(ControlService, BluetoothCacheMode.Uncached);
            status = result.Status;
            switch (status)
            {
                case GattCommunicationStatus.Success:
                    if (result.Services.Count > 0)
                    {
                        return result.Services[0];
                    }
                    _log.Error($"Failed to get control service ({device.Name}): No services found");
                    break;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    _log.Error($"Failed to get control service ({device.Name}): {result.Status}");
                    break;
            }
            await Task.Delay(200);
        }
        throw new Exception($"Failed to get control service ({device.Name}): {status}");
    }

    private static async Task<GattCharacteristic> GetPowerCharacteristic(GattDeviceService service)
    {
        const int retryCount = 5;
        var status = GattCommunicationStatus.Success;
        for (var i = 0; i < retryCount; i++)
        {
            var characteristic = await service.GetCharacteristicsForUuidAsync(PowerCharacteristic, BluetoothCacheMode.Cached);
            status = characteristic.Status;
            switch (status)
            {
                case GattCommunicationStatus.Success:
                    if (characteristic.Characteristics.Count > 0)
                    {
                        return characteristic.Characteristics[0];
                    }
                    _log.Error($"Failed to get power characteristic ({service.Device.Name}): No characteristics found");
                    break;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    _log.Error($"Failed to get power characteristic ({service.Device.Name}): {status}");
                    break;
            }
            await Task.Delay(200);
        }
        throw new LighthouseGattException($"Failed to get power characteristic ({service.Device.Name}): {status}");
    }

    private static async Task WriteCharacteristicAsync(GattCharacteristic characteristic, byte data)
    {
        const int retryCount = 5;
        GattCommunicationStatus status = GattCommunicationStatus.Success;
        for (var i = 0; i < retryCount; i++)
        {
            status = await characteristic.WriteValueAsync(new byte[] { data }.AsBuffer());
            switch (status)
            {
                case GattCommunicationStatus.Success:
                    return;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    _log.Error($"Failed to write characteristic ({characteristic.Service.Device.Name}): {status}");
                    break;
            }
            await Task.Delay(200);
        }
        throw new LighthouseGattException($"Failed to write characteristic ({characteristic.Service.Device.Name}): {status}");
    }
}
