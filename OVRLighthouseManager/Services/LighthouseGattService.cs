using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    private static readonly Guid V1ControlService = new("0000cb00-0000-1000-8000-00805f9b34fb");
    private static readonly Guid V1PowerCharacteristic = new("0000cb01-0000-1000-8000-00805f9b34fb");

    private static readonly Guid V2ControlService = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid V2PowerCharacteristic = new("00001525-1212-efde-1523-785feabcd124");

    private static readonly Regex _v1SerialRegex = new(@"HTC BS \w\w(\w\w\w\w)", RegexOptions.Compiled);

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
            await ControlV1Async(lighthouse, true);
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
            await ControlV1Async(lighthouse, false);
        }
        else
        {
            await WriteV2PowerCharacteristic(lighthouse, 0x00);
        }
    }

    public async Task StandbyAsync(Lighthouse lighthouse)
    {
        if (lighthouse.Version == LighthouseVersion.V1)
        {
            throw new LighthouseGattException("Standby is not supported on V1 lighthouses");
        }
        await WriteV2PowerCharacteristic(lighthouse, 0x02);
    }

    private async Task ControlV1Async(Lighthouse lighthouse, bool powerOn)
    {
        byte[] bytes;
        if (powerOn)
        {
            bytes = new byte[] { 0x12, 0x00, 0x00, 0x00 };
        }
        else
        {
            bytes = new byte[] { 0x12, 0x02, 0x00, 0x01 };
        }

        if (string.IsNullOrEmpty(lighthouse.Id))
        {
            throw new LighthouseGattException("ID is missing in lighthouse settings");
        }
        if (lighthouse.Id.Length != 8)
        {
            throw new LighthouseGattException($"Invalid ID length: {lighthouse.Id} ({lighthouse.Id.Length})");
        }

        // convert id to byte array
        var idBytes = Enumerable.Range(0, lighthouse.Id.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(lighthouse.Id.Substring(x, 2), 16));

        bytes = bytes
            .Concat(idBytes.Reverse())
            .Concat(Enumerable.Repeat<byte>(0x00, 12))
            .ToArray();

        if (bytes.Length != 20)
        {
            throw new LighthouseGattException("Invalid byte array length");
        }

        await WriteV1PowerCharacteristic(lighthouse, bytes);
    }

    private async Task WriteV1PowerCharacteristic(Lighthouse lighthouse, byte[] data)
    {
        var device = await GetBluetoothLEDeviceAsync(lighthouse.BluetoothAddressValue);
        using var service = await GetService(device, V1ControlService);
        var characteristic = await GetCharacteristic(service, V1PowerCharacteristic);
        await WriteCharacteristicAsync(characteristic, data);
    }


    private async Task WriteV2PowerCharacteristic(Lighthouse lighthouse, byte data)
    {
        var device = await GetBluetoothLEDeviceAsync(lighthouse.BluetoothAddressValue);
        using var service = await GetService(device, V2ControlService);
        var characteristic = await GetCharacteristic(service, V2PowerCharacteristic);
        await WriteCharacteristicAsync(characteristic, new byte[] { data });
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

    private static async Task<GattDeviceService> GetService(BluetoothLEDevice device, Guid serviceGuid)
    {
        const int retryCount = 5;
        var status = GattCommunicationStatus.Success;
        for (var i = 0; i < retryCount; i++)
        {
            var result = await device.GetGattServicesForUuidAsync(serviceGuid, BluetoothCacheMode.Uncached);
            status = result.Status;
            switch (status)
            {
                case GattCommunicationStatus.Success:
                    if (result.Services.Count > 0)
                    {
                        return result.Services[0];
                    }
                    _log.Error($"Failed to get service ({device.Name}): No services found");
                    break;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    _log.Error($"Failed to get service ({device.Name}): {result.Status}");
                    break;
            }
            await Task.Delay(200);
        }
        throw new Exception($"Failed to get service ({device.Name}): {status}");
    }

    private static async Task<GattCharacteristic> GetCharacteristic(GattDeviceService service, Guid characteristicGuid)
    {
        const int retryCount = 5;
        var status = GattCommunicationStatus.Success;
        for (var i = 0; i < retryCount; i++)
        {
            var characteristic = await service.GetCharacteristicsForUuidAsync(characteristicGuid, BluetoothCacheMode.Cached);
            status = characteristic.Status;
            switch (status)
            {
                case GattCommunicationStatus.Success:
                    if (characteristic.Characteristics.Count > 0)
                    {
                        return characteristic.Characteristics[0];
                    }
                    _log.Error($"Failed to get characteristic ({service.Device.Name}): No characteristics found");
                    break;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    _log.Error($"Failed to get characteristic ({service.Device.Name}): {status}");
                    break;
            }
            await Task.Delay(200);
        }
        throw new LighthouseGattException($"Failed to get characteristic ({service.Device.Name}): {status}");
    }

    private static async Task WriteCharacteristicAsync(GattCharacteristic characteristic, byte[] data)
    {
        const int retryCount = 5;
        GattCommunicationStatus status = GattCommunicationStatus.Success;
        for (var i = 0; i < retryCount; i++)
        {
            status = await characteristic.WriteValueAsync(data.AsBuffer());
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
