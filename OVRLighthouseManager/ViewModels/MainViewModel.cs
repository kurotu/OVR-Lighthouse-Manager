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
    private readonly ILighthouseSettingsService _lighthouseSettingsService;

    [ObservableProperty]
    private bool _canStartScan = true;

    [ObservableProperty]
    private LighthouseListItem[] _devices = Array.Empty<LighthouseListItem>();

    public ICommand ClickScanCommand
    {
        get;
    }

    public MainViewModel(ILighthouseService lighthouseService, ILighthouseSettingsService lighthouseSettingsService)
    {
        _lighthouseService = lighthouseService;
        _lighthouseSettingsService = lighthouseSettingsService;

        _lighthouseService.OnFound += async (sender, arg) =>
        {
            var existing = Devices.FirstOrDefault(d => d.BluetoothAddress == arg.BluetoothAddress);
            if (existing == null)
            {
                var item = new LighthouseListItem()
                {
                    Name = arg.Name,
                    BluetoothAddress = arg.BluetoothAddress,
                    IsManaged = false
                };
                Devices = Devices.Append(item).ToArray();
            }
            else
            {
                existing.Name = arg.Name;
            }
            await _lighthouseSettingsService.SetDevicesAsync(Devices);
            System.Diagnostics.Debug.WriteLine($"Found: {arg.Name} ({arg.BluetoothAddress.ToString("x012")})");
        };

        Devices = _lighthouseSettingsService.Devices.ToArray();

        ClickScanCommand = new RelayCommand(OnClickScan);
    }

    public async void OnClickScan()
    {
        System.Diagnostics.Debug.WriteLine("Clicked Scan");
        CanStartScan = false;
        _lighthouseService.StartScan();
        await Task.Delay(10000);
        await _lighthouseService.StopScanAsync();
        CanStartScan = true;
    }

    public void OnClickDevice(object sender, ItemClickEventArgs e)
    {
        var device = e.ClickedItem as LighthouseListItem;
        System.Diagnostics.Debug.WriteLine($"Clicked: {device?.Name} ({device?.BluetoothAddressString})");
    }
}
