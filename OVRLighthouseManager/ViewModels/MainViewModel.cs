using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using Serilog;

namespace OVRLighthouseManager.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly ILighthouseService _lighthouseService;
    private readonly ILighthouseSettingsService _lighthouseSettingsService;

    [ObservableProperty]
    private bool _powerManagement;

    [ObservableProperty]
    private bool _canStartScan = true;

    public ObservableCollection<LighthouseObject> Devices = new();

    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    public ICommand ClickScanCommand
    {
        get;
    }

    public MainViewModel(ILighthouseService lighthouseService, ILighthouseSettingsService lighthouseSettingsService)
    {
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _lighthouseService = lighthouseService;
        _lighthouseSettingsService = lighthouseSettingsService;

        _lighthouseService.OnFound += async (sender, arg) =>
        {
            var existing = Devices.FirstOrDefault(d => AddressToStringConverter.StringToAddress(d.BluetoothAddress) == arg.BluetoothAddress);
            if (existing == null)
            {
                dispatcherQueue.TryEnqueue(async () =>
                {
                    var item = LighthouseObject.FromLighthouseDevice(arg);
                    Devices.Add(item);
                    var devices = Devices.Select(d => d.ToListItem()).ToArray();
                    await _lighthouseSettingsService.SetDevicesAsync(devices);
                });
            }
            else
            {
                existing.Name = arg.Name;
            }
            Log.Information($"Found: {arg.Name} ({AddressToStringConverter.AddressToString(arg.BluetoothAddress)})");
        };

        PowerManagement = _lighthouseSettingsService.PowerManagement;
        var devices = _lighthouseSettingsService.Devices.Select(d =>
        {
            var vm = LighthouseObject.FromLighthouseListItem(d);
            return vm;
        }).ToArray();
        Devices = new(devices);

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
        Log.Information("Clicked Scan");
        CanStartScan = false;
        _lighthouseService.StartScan();
        await Task.Delay(10000);
        await _lighthouseService.StopScanAsync();
        CanStartScan = true;
    }

    public async void OnClickDevice(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is LighthouseObject device)
        {
            device.SetManaged(!device.IsManaged);
            Log.Information($"Clicked: {device.Name} ({device.BluetoothAddress}) : {device.IsManaged}");
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.ToListItem()).ToArray());
        }
        else
        {
            throw new InvalidProgramException("Clicked item is not a LighthouseListItemViewModel");
        }
    }

    public async void OnClickRemoveDevice(object? sender, EventArgs args)
    {
        if (sender is LighthouseObject device)
        {
            Log.Information($"Remove: {device.Name} ({device.BluetoothAddress})");
            Devices.Remove(device);
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.ToListItem()).ToArray());
        }
        else
        {
            throw new InvalidProgramException("Clicked item is not a LighthouseListItemViewModel");
        }
    }
}
