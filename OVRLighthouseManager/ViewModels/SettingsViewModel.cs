using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Contracts.ViewModels;
using OVRLighthouseManager.Helpers;
using Windows.ApplicationModel;

namespace OVRLighthouseManager.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IMiscSettingsService _miscSettingsService;
    private readonly ILighthouseSettingsService _lighthouseSettingsService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private bool _minimizeOnLaunchedByOpenVR;

    [ObservableProperty]
    private bool _minimizeToTray;

    [ObservableProperty]
    private bool _sendSimultaneously;

    [ObservableProperty]
    private bool _outputDebug;

    [ObservableProperty]
    private string _versionDescription;

    public ICommand ThirdPartyLicensesLinkCommand
    {
        get;
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IMiscSettingsService miscSettingsService, ILighthouseSettingsService lighthouseSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;

        _miscSettingsService = miscSettingsService;
        _lighthouseSettingsService = lighthouseSettingsService;

        _minimizeOnLaunchedByOpenVR = _miscSettingsService.MinimizeOnLaunchedByOpenVR;
        _sendSimultaneously = _lighthouseSettingsService.SendSimultaneously;
        _outputDebug = _miscSettingsService.OutputDebug;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        ThirdPartyLicensesLinkCommand = new RelayCommand(() =>
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Documents", "ThirdPartyLicenses.html");
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true,
            });
        });
    }

    public void OnNavigatedTo(object parameter)
    {
        MinimizeOnLaunchedByOpenVR = _miscSettingsService.MinimizeOnLaunchedByOpenVR;
        OutputDebug = _miscSettingsService.OutputDebug;
        MinimizeToTray = _miscSettingsService.MinimizeToTray;
    }

    public void OnNavigatedFrom()
    {
    }

    public async void OnToggleMinimizeOnLaunchedByOpenVR(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {

            MinimizeOnLaunchedByOpenVR = toggleSwitch.IsOn;
            await _miscSettingsService.SetMinimizeOnLaunchedByOpenVRAsync(MinimizeOnLaunchedByOpenVR);
        }
    }

    public async void OnToggleMinimizeToTray(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            MinimizeToTray = toggleSwitch.IsOn;
            await _miscSettingsService.SetMinimizeToTray(MinimizeToTray);
        }
    }

    public async void OnToggleSendSimultaneously(object sender, RoutedEventArgs e)
    {
        if(sender is ToggleSwitch toggleSwitch)
        {
            SendSimultaneously = toggleSwitch.IsOn;
            await _lighthouseSettingsService.SetSendSimultaneously(SendSimultaneously);
        }
    }

    public async void OnToggleOutputDebug(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            OutputDebug = toggleSwitch.IsOn;
            await _miscSettingsService.SetOutputDebugAsync(OutputDebug);
        }
    }

    private static string GetVersionDescription()
    {
        string versionString;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            var version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            versionString = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        else
        {
            var commit = VersionHelper.GetCommit();
            versionString = commit == null ? VersionHelper.GetVersion() : $"{VersionHelper.GetVersion()} ({commit[..6]})";
        }

        return $"{"AppDisplayName".GetLocalized()} - {versionString}";
    }
}
