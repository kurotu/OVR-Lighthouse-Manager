using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.Contracts.Services;

public interface ILighthouseSettingsService
{
    public bool PowerManagement
    {
        get;
    }

    public PowerDownMode PowerDownMode
    {
        get;
    }

    public List<Lighthouse> Devices
    {
        get;
    }

    public Task InitializeAsync();
    public Task SetPowerManagementAsync(bool powerManagement);
    public Task SetPowerDownModeAsync(PowerDownMode powerDownMode);
    public Task SetDevicesAsync(Lighthouse[] device);
}
