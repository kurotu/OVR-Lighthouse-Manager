using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace OVRLighthouseManager.Contracts.Services;
public interface IOpenVRService
{
    public bool IsVRMonitorConnected
    {
        get;
    }

    public event EventHandler<VREvent_t> OnVRMonitorConnected;
    public event EventHandler<VREvent_t> OnVRSystemQuit;

    public void Shutdown();
    public void AddApplicationManifest(string applicationManifestPath);
    public void RemoveApplicationManifest(string applicationManifestPath);
    public void SetApplicationAutoLaunch(string applicationKey, bool autoLaunch);
}
