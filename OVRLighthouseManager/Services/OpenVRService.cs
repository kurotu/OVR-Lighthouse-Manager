using OVRLighthouseManager.Contracts.Services;
using OVRSharp;
using Valve.VR;

namespace OVRLighthouseManager.Services;
public class OpenVRService : IOpenVRService
{
    private Application? _application;

    public void Initialize()
    {
        _application = new Application(Application.ApplicationType.Utility);
    }

    public void Shutdown()
    {
        _application?.Shutdown();
        _application = null;
    }

    public void AddApplicationManifest(string applicationManifestPath)
    {
        if (_application == null)
        {
            throw new Exception("OpenVRService not initialized");
        }
        OpenVR.Applications.AddApplicationManifest(applicationManifestPath, false);
    }

    public void SetApplicationAutoLaunch(string applicationKey, bool autoLaunch)
    {
        if (_application == null)
        {
            throw new Exception("OpenVRService not initialized");
        }
        OpenVR.Applications.SetApplicationAutoLaunch(applicationKey, autoLaunch);
    }

    public void RemoveApplicationManifest(string applicationManifestPath)
    {
        if (_application == null)
        {

            throw new Exception("OpenVRService not initialized");
        }
        OpenVR.Applications.RemoveApplicationManifest(applicationManifestPath);
    }
}
