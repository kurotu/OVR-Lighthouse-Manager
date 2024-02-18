using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVRLighthouseManager.Models;
public class Lighthouse
{
    public string Name { get; set; } = "";
    public ulong BluetoothAddress
    {
        get; set;
    }

    /**
     * Used to control Lighthouse V1.
     */
    public string? Id
    {
        get; set;
    }

    public LighthouseVersion Version
    {
        get
        {
            if (Name.StartsWith("HTC BS"))
            {
                return LighthouseVersion.V1;
            }
            if (Name.StartsWith("LHB-"))
            {
                return LighthouseVersion.V2;
            }
            return LighthouseVersion.Unknown;
        }
    }
}
