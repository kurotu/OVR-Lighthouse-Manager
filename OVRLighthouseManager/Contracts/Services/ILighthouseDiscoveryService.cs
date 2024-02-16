using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.Contracts.Services;
public interface ILighthouseDiscoveryService
{
    public bool IsDiscovering
    {
        get;
    }
    public IReadOnlyCollection<Lighthouse> FoundLighthouses
    {
        get;
    }

    public event EventHandler<Lighthouse> Found;

    void StartDiscovery();
    void StopDiscovery();
}
