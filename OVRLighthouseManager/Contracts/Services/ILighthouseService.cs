using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.Contracts.Services;
public interface ILighthouseService
{
    public event EventHandler<LighthouseDevice> OnFound;

    public void StartScan();
    public void StopScan();
}
