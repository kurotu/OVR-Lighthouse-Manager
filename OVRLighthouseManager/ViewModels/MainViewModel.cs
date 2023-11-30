using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Behaviors;
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

    public ObservableCollection<LighthouseObject> Devices = new();

    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    public bool CannotUseBluetooth => !_lighthouseService.HasBluetoothLEAdapter();

    public readonly ICommand ScanCommand;

    public MainViewModel(
        ILighthouseService lighthouseService,
        ILighthouseSettingsService lighthouseSettingsService,
        ScanCommand scanCommand
        )
    {
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _lighthouseService = lighthouseService;
        _lighthouseSettingsService = lighthouseSettingsService;

        _lighthouseService.OnFound += (sender, arg) =>
        {
            var address = arg.BluetoothAddress;
            var existing = Devices.FirstOrDefault(d => AddressToStringConverter.StringToAddress(d.BluetoothAddress) == address);
            if (existing == null)
            {
                dispatcherQueue.TryEnqueue(async () =>
                {
                    var d = _lighthouseService.GetLighthouse(address);
                    if (d == null)
                    {
                        Log.Information($"{arg.Name} is not a Lighthouse");
                        return;
                    }
                    var item = LighthouseObject.FromLighthouseDevice(d);
                    Devices.Add(item);
                    var devices = Devices.Select(d => d.ToListItem()).ToArray();
                    await _lighthouseSettingsService.SetDevicesAsync(devices);
                    Log.Information($"Found: {arg.Name} ({AddressToStringConverter.AddressToString(address)})");
                });
            }
            else
            {
                Log.Information($"Updated: {arg.Name} ({AddressToStringConverter.AddressToString(address)})");
                existing.Name = arg.Name;
            }
        };

        PowerManagement = _lighthouseSettingsService.PowerManagement;
        var devices = _lighthouseSettingsService.Devices.Select(d =>
        {
            var vm = LighthouseObject.FromLighthouseListItem(d);
            return vm;
        }).ToArray();
        Devices = new(devices);

        ScanCommand = scanCommand;
        if (ScanCommand.CanExecute(null))
        {
            ScanCommand.Execute(null);
        }
    }

    public async void OnTogglePowerManagement(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            PowerManagement = toggleSwitch.IsOn;
            await _lighthouseSettingsService.SetPowerManagementAsync(PowerManagement);
        }
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
