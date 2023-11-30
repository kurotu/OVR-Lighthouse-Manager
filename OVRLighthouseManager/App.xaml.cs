using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using OVRLighthouseManager.Activation;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Core.Contracts.Services;
using OVRLighthouseManager.Core.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Services;
using OVRLighthouseManager.ViewModels;
using OVRLighthouseManager.Views;
using Serilog;
using Valve.VR;

namespace OVRLighthouseManager;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        LogHelper.InitializeLogger();
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            OnCommandLineArgs(args);
        }
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<ILighthouseService, LighthouseService>();
            services.AddSingleton<ILighthouseSettingsService, LighthouseSettingsService>();
            services.AddSingleton<IOverlayAppService, OverlayAppService>();
            services.AddSingleton<IAppLifecycleService, AppLifeCycleService>();
            services.AddSingleton<INotificationService, NotificationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));

            // Commands
            services.AddSingleton<ScanCommand>();
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        Log.Error(e.Exception, "UnhandledException");
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private void OnCommandLineArgs(string[] args)
    {
        var key = "com.github.kurotu.ovr-lighthouse-manager";
        var manifestPath = Path.Combine(AppContext.BaseDirectory, "manifest.vrmanifest");
        if (args[1] == "install")
        {
            try
            {
                var openvr = new OpenVRService();
                openvr.Initialize();
                openvr.AddApplicationManifest(manifestPath);
                openvr.SetApplicationAutoLaunch(key, true);
                openvr.Shutdown();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to install vrmanifest");
                Environment.Exit(1);
            }
            Environment.Exit(0);
        }

        if (args[1] == "uninstall")
        {
            try
            {
                var openvr = new OpenVRService();
                openvr.Initialize();
                openvr.SetApplicationAutoLaunch(key, false);
                openvr.RemoveApplicationManifest(manifestPath);
                openvr.Shutdown();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to uninstall vrmanifest");
                Environment.Exit(1);
            }
            Environment.Exit(0);
        }
    }
}
