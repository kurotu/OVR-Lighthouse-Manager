using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly ILighthouseService _lighthouseService;

    [ObservableProperty]
    private bool _canStartScan = true;

    [ObservableProperty]
    private LighthouseListItem[] _devices = { };

    public ICommand ClickScanCommand
    {
        get;
    }

    public MainViewModel(ILighthouseService lighthouseService)
    {
        _lighthouseService = lighthouseService;
        _lighthouseService.OnFound += (sender, arg) =>
        {
            if (Devices.FirstOrDefault(d => d.BluetoothAddress == arg.BluetoothAddress) == null)
            {
                var item = new LighthouseListItem()
                {
                    Name = arg.Name,
                    BluetoothAddress = arg.BluetoothAddress,
                    IsManaged = false
                };
                Devices = Devices.Append(item).ToArray();
            }
            System.Diagnostics.Debug.WriteLine($"Found: {arg.Name} ({arg.BluetoothAddress.ToString("x012")})");
        };

        ClickScanCommand = new RelayCommand(OnClickScan);
    }

    public async void OnClickScan()
    {
        System.Diagnostics.Debug.WriteLine("Clicked Scan");
        CanStartScan = false;
        _lighthouseService.StartScan();
        await Task.Delay(10000);
        _lighthouseService.StopScan();
        CanStartScan = true;
    }

    public void OnClickDevice(object sender, ItemClickEventArgs e)
    {
        var device = e.ClickedItem as LighthouseListItem;
        System.Diagnostics.Debug.WriteLine($"Clicked: {device?.Name} ({device?.BluetoothAddressString})");
    }
}
