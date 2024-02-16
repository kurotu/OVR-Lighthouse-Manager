using Windows.Devices.Bluetooth;

namespace OVRLighthouseManager.Helpers;
internal static class BluetoothLEHelper
{
    public static bool HasBluetoothLEAdapter()
    {
        var adaper = BluetoothAdapter.GetDefaultAsync().AsTask().Result;
        if (adaper == null)
        {
            return false;
        }
        return adaper.IsLowEnergySupported;
    }
}
