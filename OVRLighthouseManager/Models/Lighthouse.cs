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
}
