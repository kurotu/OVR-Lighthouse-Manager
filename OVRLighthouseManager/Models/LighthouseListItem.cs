using System.Text;

namespace OVRLighthouseManager.Models;

public class LighthouseListItem
{
    public required string Name
    {
        get; set;
    }

    public required string BluetoothAddress
    {
        get; set;
    }

    public bool IsManaged
    {
        get; set;
    }
}
