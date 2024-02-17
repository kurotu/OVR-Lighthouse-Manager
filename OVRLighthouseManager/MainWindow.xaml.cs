using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using Serilog;
using Windows.UI.ViewManagement;

namespace OVRLighthouseManager;

public sealed partial class MainWindow : WindowEx
{
    public bool IsMinimizedToTray = false;

    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(500, 500));

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        AppWindow.Changed += (_, __) =>
        {
            var mainWindow = (MainWindow)App.MainWindow;
            if (App.MainWindow.WindowState != WindowState.Minimized)
            {
                mainWindow.IsMinimizedToTray = false;
            }
            if (mainWindow.IsMinimizedToTray)
            {
                return;
            }
            if (App.MainWindow.WindowState == WindowState.Minimized && App.GetService<IMiscSettingsService>().MinimizeToTray)
            {
                Log.Information("Minimize to tray");
                mainWindow.IsMinimizedToTray = true;
                this.Hide();
            }
        };
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}
