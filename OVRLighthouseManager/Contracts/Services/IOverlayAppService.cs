using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace OVRLighthouseManager.Contracts.Services;
public interface IOverlayAppService
{
    public bool IsVRMonitorConnected
    {
        get;
    }

    public event EventHandler<VREvent_t> OnVRMonitorConnected;
    public event EventHandler<VREvent_t> OnVRSystemQuit;

    public void Shutdown();
}
