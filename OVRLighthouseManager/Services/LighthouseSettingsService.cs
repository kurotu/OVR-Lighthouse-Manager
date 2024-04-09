using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.Services;

class LighthouseSettingsService : ILighthouseSettingsService
{
    private const string SettingsKey_PowerManagement = "PowerManagement";
    private const string SettingsKey_PowerDownMode = "PowerDownMode";
    private const string SettingsKey_Devices = "Devices";

    public bool PowerManagement
    {
        get; set;
    }

    public PowerDownMode PowerDownMode
    {
        get; set;
    }

    public List<Lighthouse> Devices
    {
        get; set;
    } = new List<Lighthouse>();

    private readonly ILocalSettingsService _localSettingsService;

    public LighthouseSettingsService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        PowerManagement = await LoadPowerManagementFromSettingsAsync();
        PowerDownMode = await LoadPowerDownModeFromSettingsAsync();
        Devices = await LoadDevicesFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetPowerManagementAsync(bool powerManagement)
    {
        PowerManagement = powerManagement;
        await SavePowerManagementInSettingsAsync(PowerManagement);
    }

    public async Task SetPowerDownModeAsync(PowerDownMode powerDownMode)
    {
        PowerDownMode = powerDownMode;
        await SavePowerDownModeInSettingsAsync(PowerDownMode);
    }

    public async Task SetDevicesAsync(Lighthouse[] devices)
    {
        Devices = devices.ToList();
        await SaveDevicesInSettingsAsync(Devices);
    }

    #region PowerManagement

    private async Task SavePowerManagementInSettingsAsync(bool powerManagement)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey_PowerManagement, powerManagement);
    }

    public async Task<bool> LoadPowerManagementFromSettingsAsync()
    {
        var powerManagement = await _localSettingsService.ReadSettingAsync<bool?>(SettingsKey_PowerManagement);
        if (!powerManagement.HasValue)
        {
            return true;
        }
        return powerManagement.Value;
    }

    #endregion

    #region PowerDownMode

    private async Task SavePowerDownModeInSettingsAsync(PowerDownMode powerDownMode)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey_PowerDownMode, powerDownMode.ToString());
    }

    private async Task<PowerDownMode> LoadPowerDownModeFromSettingsAsync()
    {
        var powerDownModeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey_PowerDownMode);
        if (Enum.TryParse(powerDownModeName, out PowerDownMode powerDownMode))
        {
            return powerDownMode;
        }
        return PowerDownMode.Sleep;
    }

    #endregion

    #region Devices

    private async Task SaveDevicesInSettingsAsync(List<Lighthouse> devices)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey_Devices, devices);
    }

    private async Task<List<Lighthouse>> LoadDevicesFromSettingsAsync()
    {
        var devices = await _localSettingsService.ReadSettingAsync<List<Lighthouse>>(SettingsKey_Devices);
        if (devices != null)
        {
            return devices;
        }
        return new List<Lighthouse>();
    }

    #endregion
}
