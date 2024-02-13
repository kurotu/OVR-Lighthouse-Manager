using OVRLighthouseManager.Contracts.Services;

namespace OVRLighthouseManager.Services;
internal class MiscSettingsService : IMiscSettingsService
{
    private const string SettingsKey_OutputDebug = "OutputDebug";

    public bool OutputDebug
    {
        get; set;
    }

    private readonly ILocalSettingsService _localSettingsService;

    public MiscSettingsService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task SetOutputDebugAsync(bool outputDebug)
    {
        OutputDebug = outputDebug;
        await SaveOutputDebugInSettingsAsync(OutputDebug);
    }

    public async Task InitializeAsync()
    {
        OutputDebug = await LoadOutputDebugFromSettingsAsync();
        await Task.CompletedTask;
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
