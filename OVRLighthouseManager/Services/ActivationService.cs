using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using OVRLighthouseManager.Activation;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Views;
using Serilog;

namespace OVRLighthouseManager.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILighthouseSettingsService _lighthouseSettingsService;
    private readonly IMiscSettingsService _miscSettingsService;
    private readonly IAppLifecycleService _appLifecycleService;
    private readonly IOpenVRService _openVRService;
    private readonly IUpdaterService _updaterService;
    private UIElement? _shell = null;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IThemeSelectorService themeSelectorService,
        ILighthouseSettingsService lighthouseSettingsService,
        IMiscSettingsService miscSettingsService,
        IAppLifecycleService appLifecycleService,
        IOpenVRService openVRService,
        IUpdaterService updaterService
    )
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _lighthouseSettingsService = lighthouseSettingsService;
        _miscSettingsService = miscSettingsService;
        _appLifecycleService = appLifecycleService;
        _openVRService = openVRService;
        _updaterService = updaterService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
            App.MainWindow.AppWindow.Closing += (_, __) =>
            {
                _appLifecycleService.OnBeforeAppExit();
            };
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        await _lighthouseSettingsService.InitializeAsync().ConfigureAwait(false);
        await _miscSettingsService.InitializeAsync().ConfigureAwait(false);
        LogHelper.InitializeLogger(_miscSettingsService.OutputDebug);
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        var scanCommand = App.GetService<ScanCommand>();
        if (scanCommand!.CanExecute(null))
        {
            scanCommand.Execute(null);
        }
        await _themeSelectorService.SetRequestedThemeAsync();
        await _updaterService.FetchLatestVersion();
        await _appLifecycleService.OnLaunch();
        await Task.CompletedTask;
    }
}
