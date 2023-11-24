using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly ILighthouseService _lighthouseService;
    private readonly ILighthouseSettingsService _lighthouseSettingsService;

    [ObservableProperty]
    private bool _powerManagement;

    [ObservableProperty]
    private bool _canStartScan = true;

    [ObservableProperty]
    private LighthouseListItemViewModel[] _devices = Array.Empty<LighthouseListItemViewModel>();

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
            var existing = Devices.FirstOrDefault(d => AddressToStringConverter.StringToAddress(d.BluetoothAddress) == arg.BluetoothAddress);
            if (existing == null)
            {
                var item = new LighthouseListItemViewModel(arg);
                item.OnClickRemove += OnClickRemoveDevice;
                Devices = Devices.Append(item).OrderBy(i => i.BluetoothAddress).ToArray();
            }
            else
            {
                existing.Name = arg.Name;
            }
            var devices = Devices.Select(d => d.ToListItem()).ToArray();
            await _lighthouseSettingsService.SetDevicesAsync(devices);
            System.Diagnostics.Debug.WriteLine($"Found: {arg.Name} ({AddressToStringConverter.AddressToString(arg.BluetoothAddress)})");
        };

        PowerManagement = _lighthouseSettingsService.PowerManagement;
        Devices = _lighthouseSettingsService.Devices.Select(d =>
        {
            var vm = new LighthouseListItemViewModel(d);
            vm.OnClickRemove += OnClickRemoveDevice;
            return vm;
        }).ToArray();

        ClickScanCommand = new RelayCommand(OnClickScan);
    }

    public async void OnTogglePowerManagement(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            PowerManagement = toggleSwitch.IsOn;
            await _lighthouseSettingsService.SetPowerManagementAsync(PowerManagement);
        }
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

    public async void OnClickDevice(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is LighthouseListItemViewModel device)
        {
            System.Diagnostics.Debug.WriteLine($"Clicked: {device.Name} ({device.BluetoothAddress}) : {device.IsManaged}");
            device.SetManaged(!device.IsManaged);
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.ToListItem()).ToArray());
        }
        else
        {
            throw new InvalidProgramException("Clicked item is not a LighthouseListItemViewModel");
        }
    }

    public async void OnClickRemoveDevice(object? sender, EventArgs args)
    {
        if (sender is LighthouseListItemViewModel device)
        {
            System.Diagnostics.Debug.WriteLine($"Remove: {device.Name} ({device.BluetoothAddress})");
            Devices = Devices.Where(d => d != device).ToArray();
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.ToListItem()).ToArray());
        }
        else
        {
            throw new InvalidProgramException("Clicked item is not a LighthouseListItemViewModel");
        }
    }
}
