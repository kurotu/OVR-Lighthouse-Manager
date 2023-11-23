using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OVRLighthouseManager.Contracts.Services;

namespace OVRLighthouseManager.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly ILighthouseService _lighthouseService;

    [ObservableProperty]
    private bool _canStartScan = true;

    public ICommand ClickScanCommand
    {
        get;
    }

    public MainViewModel(ILighthouseService lighthouseService)
    {
        _lighthouseService = lighthouseService;
        _lighthouseService.OnFound += (sender, arg) =>
        {
            System.Diagnostics.Debug.WriteLine($"Found: {arg.Name} ({arg.BluetoothAddress.ToString("x012")})");
        };

        ClickScanCommand = new RelayCommand(OnClickScan);
    }

    public async void OnClickScan()
    {
        CanStartScan = false;
        _lighthouseService.StartScan();
        await Task.Delay(10000);
        _lighthouseService.StopScan();
        CanStartScan = true;
    }
}
