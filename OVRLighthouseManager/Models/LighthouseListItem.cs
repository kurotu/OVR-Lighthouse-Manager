using System.Text;

namespace OVRLighthouseManager.Models;

public class LighthouseListItem
{
    public required string Name
    {
        get; set;
    }

    public ulong BluetoothAddress
    {
        get; set;
    }

    public string BluetoothAddressString
    {
        get
        {
            // 0x0123456789ab -> 01:23:45:67:89:ab
            var str = BluetoothAddress.ToString("x012");
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i += 2)
            {
                sb.Append(str[i]);
                sb.Append(str[i + 1]);
                if (i < str.Length - 2)
                {
                    sb.Append(':');
                }
            }
            return sb.ToString();
        }
    }

    public bool IsManaged
    {
        get; set;
    }
}
