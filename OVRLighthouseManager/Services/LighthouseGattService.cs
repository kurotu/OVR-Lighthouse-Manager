using System.Runtime.InteropServices.WindowsRuntime;
using Serilog;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Exceptions;
using OVRLighthouseManager.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OVRLighthouseManager.Services;

class LighthouseGattService : ILighthouseGattService
{
    private static readonly Guid V1ControlService = new("0000cb00-0000-1000-8000-00805f9b34fb");
    private static readonly Guid V1PowerCharacteristic = new("0000cb01-0000-1000-8000-00805f9b34fb");

    private static readonly Guid V2ControlService = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid V2PowerCharacteristic = new("00001525-1212-efde-1523-785feabcd124");

    private static readonly ILogger _log = LogHelper.ForContext<LighthouseGattService>();

    private readonly ILighthouseDiscoveryService _lighthouseDiscoveryService;

    public LighthouseGattService(ILighthouseDiscoveryService lighthouseDiscoveryService)
    {
        _lighthouseDiscoveryService = lighthouseDiscoveryService;
    }

    public async Task PowerOnAsync(Lighthouse lighthouse)
    {
        if (lighthouse.Version == LighthouseVersion.V1)
        {
            await WriteV1PowerCharacteristic(lighthouse, 0x00, 0x00);
        }
        else
        {
            await WriteV2PowerCharacteristic(lighthouse, 0x01);
        }
    }

    public async Task SleepAsync(Lighthouse lighthouse)
    {
        if (lighthouse.Version == LighthouseVersion.V1)
        {
            await WriteV1PowerCharacteristic(lighthouse, 0x02, 0x01);
        }
        else
        {
            await WriteV2PowerCharacteristic(lighthouse, 0x00);
        }
    }

    public async Task StandbyAsync(Lighthouse lighthouse)
    {
        if ( lighthouse.Version == LighthouseVersion.V1 )
        {
            await WriteV1PowerCharacteristic(lighthouse, 0x01, 0x04);
        }
        else
        {
            await WriteV2PowerCharacteristic(lighthouse, 0x02);
        }
    }

    private async Task WriteV1PowerCharacteristic(Lighthouse lighthouse, byte cmd, byte val)
    {
        // Lighthouse 1.0 commands are of the following form:
        // 0x12 : HTC MAGIC
        // command ::
        //     0x00 : Power On
        //     0x02 : Sleep
        //     0x01 : Standby
        // 0x00
        // value ::
        //     0x00 : Power On
        //     0x01 : Sleep
        //     0x04 : Standby
        // 4 bytes representing base station ID. If all are set to 0xFF it'll execute.
        // 12 bytes set to 0
        await WritePowerCharacteristicAsync(lighthouse, V1ControlService, V1PowerCharacteristic, new byte[] { 0x12, cmd, 0x00, val, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } );
    }

    private async Task WriteV2PowerCharacteristic(Lighthouse lighthouse, byte data)
    {
        await WritePowerCharacteristicAsync(lighthouse, V2ControlService, V2PowerCharacteristic, new byte[] { data });
    }

    private async Task WritePowerCharacteristicAsync(Lighthouse lighthouse, Guid controlService, Guid powerCharacteristic, byte[] data)
    {
        if (!BluetoothLEHelper.HasBluetoothLEAdapter())
        {
            throw new LighthouseGattException("Bluetooth LE adapter not found");
        }

        const int retryCount = 10;
        Exception? lastException = null;
        for (var i = 0; i < retryCount; i++)
        {
            try
            {
                using var device = await GetBluetoothLEDeviceAsync(lighthouse.BluetoothAddressValue);
                using var service = await GetService(device, controlService);
                var characteristic = await GetCharacteristic(service, powerCharacteristic);
                await WriteCharacteristicAsync(characteristic, data);
                _log.Information($"Succeeded to write power characteristic for {lighthouse.Name}");
                return;
            }
            catch (InvalidProgramException)
            {
                throw;
            }
            catch (Exception e)
            {
                lastException = e;
                _log.Error(e, "Failed to write power characteristic");
                await Task.Delay(500);
            }
        }
        _log.Error($"Failed to write power characteristic in {retryCount} retries");
        throw lastException!;
    }

    private async Task<BluetoothLEDevice> GetBluetoothLEDeviceAsync(ulong address)
    {
        const int retryCount = 10;
        for (var i = 0; i < retryCount; i++)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            if (device != null)
            {
                return device;
            }
            _lighthouseDiscoveryService.StartDiscovery();
            await Task.Delay(500);
        }
        throw new LighthouseGattException("Lighthouse not found");
    }

    private static async Task<GattDeviceService> GetService(BluetoothLEDevice device, Guid serviceGuid)
    {
        var result = await device.GetGattServicesForUuidAsync(serviceGuid, BluetoothCacheMode.Cached);
        switch (result.Status)
        {
            case GattCommunicationStatus.Success:
                if (result.Services.Count > 0)
                {
                    return result.Services[0];
                }
                throw new LighthouseGattException($"Failed to get service ({device.Name}): No services found");
            case GattCommunicationStatus.Unreachable:
            case GattCommunicationStatus.ProtocolError:
            case GattCommunicationStatus.AccessDenied:
                throw new LighthouseGattException($"Failed to get service ({device.Name}): {result.Status}");
        }
        throw new InvalidProgramException($"Unexpected status: {result.Status}");
    }

    private static async Task<GattCharacteristic> GetCharacteristic(GattDeviceService service, Guid characteristicGuid)
    {
        var characteristic = await service.GetCharacteristicsForUuidAsync(characteristicGuid, BluetoothCacheMode.Cached);
        switch (characteristic.Status)
        {
            case GattCommunicationStatus.Success:
                if (characteristic.Characteristics.Count > 0)
                {
                    return characteristic.Characteristics[0];
                }
                throw new LighthouseGattException($"Failed to get characteristic ({service.Device.Name}): No characteristics found");
            case GattCommunicationStatus.Unreachable:
            case GattCommunicationStatus.ProtocolError:
            case GattCommunicationStatus.AccessDenied:
                throw new LighthouseGattException($"Failed to get characteristic ({service.Device.Name}): {characteristic.Status}");
        }
        throw new InvalidProgramException($"Unexpected status: {characteristic.Status}");
    }

    private static async Task WriteCharacteristicAsync(GattCharacteristic characteristic, byte[] data)
    {
        var status = await characteristic.WriteValueAsync(data.AsBuffer());
        switch (status)
        {
            case GattCommunicationStatus.Success:
                return;
            case GattCommunicationStatus.Unreachable:
            case GattCommunicationStatus.ProtocolError:
            case GattCommunicationStatus.AccessDenied:
                throw new LighthouseGattException($"Failed to write characteristic ({characteristic.Service.Device.Name}): {status}");
        }
        throw new InvalidProgramException($"Unexpected status: {status}");
    }
}
