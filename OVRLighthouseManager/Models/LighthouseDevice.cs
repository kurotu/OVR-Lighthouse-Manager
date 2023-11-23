namespace OVRLighthouseManager.Models;

public class LighthouseDevice
{
    public ulong BluetoothAddress
    {
        get; set;
    }

    public required string Name
    {

        get; set;
    }
}
