using OVRLighthouseManager.Contracts.Services;

namespace OVRLighthouseManager.Services;
internal class MiscSettingsService : IMiscSettingsService
{
    private const string SettingsKey_MinimizeOnLaunchedByOpenVR = "MinimizeOnLaunchedByOpenVR";
    private const string SettingsKey_MinimizeToTray = "MinimizeToTray";
    private const string SettingsKey_OutputDebug = "OutputDebug";

    public bool MinimizeOnLaunchedByOpenVR
    {
        get; set;
    }

    public bool MinimizeToTray
    {
        get; set;
    }

    public bool OutputDebug
    {
        get; set;
    }

    private readonly ILocalSettingsService _localSettingsService;

    public MiscSettingsService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task SetMinimizeOnLaunchedByOpenVRAsync(bool minimizeOnLaunchedByOpenVR)
    {
        MinimizeOnLaunchedByOpenVR = minimizeOnLaunchedByOpenVR;
        await SaveMinimizeOnLaunchedByOpenVRInSettingsAsync(MinimizeOnLaunchedByOpenVR);
    }

    public async Task SetMinimizeToTray(bool minimizeToTray)
    {
        MinimizeToTray = minimizeToTray;
        await SaveMinimizeToTrayInSettingsAsync(MinimizeToTray);
    }

    public async Task SetOutputDebugAsync(bool outputDebug)
    {
        OutputDebug = outputDebug;
        await SaveOutputDebugInSettingsAsync(OutputDebug);
    }

    public async Task InitializeAsync()
    {
        OutputDebug = await LoadOutputDebugFromSettingsAsync();
        MinimizeOnLaunchedByOpenVR = await LoadMinimizeOnLaunchedByOpenVRFromSettingsAsync();
        MinimizeToTray = await LoadMinimizeToTrayFromSettingsAsync();
        await Task.CompletedTask;
    }

    private async Task SaveMinimizeOnLaunchedByOpenVRInSettingsAsync(bool minimizeOnLaunchedByOpenVR)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey_MinimizeOnLaunchedByOpenVR, minimizeOnLaunchedByOpenVR);
    }

    private async Task<bool> LoadMinimizeOnLaunchedByOpenVRFromSettingsAsync()
    {
        return await _localSettingsService.ReadSettingAsync<bool>(SettingsKey_MinimizeOnLaunchedByOpenVR);
    }

    private async Task SaveMinimizeToTrayInSettingsAsync(bool minimizeToTray)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey_MinimizeToTray, minimizeToTray);
    }

    private async Task<bool> LoadMinimizeToTrayFromSettingsAsync()
    {
        return await _localSettingsService.ReadSettingAsync<bool>(SettingsKey_MinimizeToTray);
    }

    private async Task SaveOutputDebugInSettingsAsync(bool outputDebug)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey_OutputDebug, outputDebug);
    }

    public async Task<bool> LoadOutputDebugFromSettingsAsync()
    {
        return await _localSettingsService.ReadSettingAsync<bool>(SettingsKey_OutputDebug);
    }
}
