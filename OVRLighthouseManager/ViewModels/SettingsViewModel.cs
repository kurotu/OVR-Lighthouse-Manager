using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using Windows.ApplicationModel;

namespace OVRLighthouseManager.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IMiscSettingsService _miscSettingsService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

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

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IMiscSettingsService miscSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _miscSettingsService = miscSettingsService;
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
            var path = Path.Combine(AppContext.BaseDirectory, "ThirdPartyLicenses.html");
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true,
            });
        });
        _miscSettingsService = miscSettingsService;
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
