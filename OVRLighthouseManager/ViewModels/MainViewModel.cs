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
    private readonly ILighthouseDiscoveryService _lighthouseService;
    private readonly ILighthouseSettingsService _lighthouseSettingsService;
    private readonly IOpenVRService _openVRService;

    [ObservableProperty]
    private bool _hasUpdate;

    [ObservableProperty]
    private string? _hasUpdateMessage;

    private string? _latestVersion;

    [ObservableProperty]
    private bool _powerManagement;

    [ObservableProperty]
    private int _powerDownModeIndex;

    public ObservableCollection<LighthouseObject> Devices = new();

    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    public bool CannotUseOpenVR => !_openVRService.IsInitialized;
    public bool CannotUseBluetooth => !BluetoothLEHelper.HasBluetoothLEAdapter();

    public bool IsScanning => _lighthouseService.IsDiscovering;
    public readonly ICommand ScanCommand;

    public MainViewModel(
        ILighthouseDiscoveryService lighthouseService,
        ILighthouseSettingsService lighthouseSettingsService,
        IOpenVRService openVRService,
        IUpdaterService updaterService,
        ScanCommand scanCommand
        )
    {
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _lighthouseService = lighthouseService;
        _lighthouseSettingsService = lighthouseSettingsService;
        _openVRService = openVRService;

        _lighthouseService.Found += (sender, arg) =>
        {
            Log.Debug($"Found: {arg.Name} ({AddressToStringConverter.AddressToString(arg.BluetoothAddressValue)})");
            var address = arg.BluetoothAddressValue;
            var existing = Devices.FirstOrDefault(d => AddressToStringConverter.StringToAddress(d.BluetoothAddress) == address);
            if (existing == null)
            {
                dispatcherQueue.TryEnqueue(async () =>
                {
                    var item = new LighthouseObject(arg, true);
                    item.OnClickRemove += OnClickRemoveDevice;
                    item.IsFound = true;
                    Devices.Add(item);
                    var devices = Devices.Select(d => d.Lighthouse).ToArray();
                    await _lighthouseSettingsService.SetDevicesAsync(devices);
                    Log.Information($"Found: {arg.Name} ({AddressToStringConverter.AddressToString(address)})");
                });
            }
            else
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    Log.Information($"Updated: {arg.Name} ({AddressToStringConverter.AddressToString(address)})");
                    existing.IsFound = true;
                });
            }
        };

        _latestVersion = updaterService.LatestVersion;
        HasUpdate = updaterService.HasUpdate;
        HasUpdateMessage = GetHasUpdateMessage();
        updaterService.FoundUpdate += (sender, version) =>
        {
            _latestVersion = version;
            HasUpdate = true;
            HasUpdateMessage = GetHasUpdateMessage();
        };

        PowerManagement = _lighthouseSettingsService.PowerManagement;
        PowerDownModeIndex = (int)_lighthouseSettingsService.PowerDownMode;

        var devices = _lighthouseSettingsService.Devices.Select(d =>
        {
            var vm = new LighthouseObject(d, true);
            vm.OnClickRemove += OnClickRemoveDevice;
            vm.OnEditId += OnEditId;
            vm.IsFound = _lighthouseService.FoundLighthouses.Any(l => l.BluetoothAddressValue == AddressToStringConverter.StringToAddress(d.BluetoothAddress));
            return vm;
        }).ToArray();
        Devices = new(devices);

        ScanCommand = scanCommand;
        ScanCommand.CanExecuteChanged += (sender, args) =>
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(IsScanning));
            });
        };
    }

    public async void OnTogglePowerManagement(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            PowerManagement = toggleSwitch.IsOn;
            await _lighthouseSettingsService.SetPowerManagementAsync(PowerManagement);
        }
    }

    public async void OnSelectPowerDownMode(object sender, SelectionChangedEventArgs e)
    {
        if (sender is RadioButtons radioButtons)
        {
            PowerDownModeIndex = radioButtons.SelectedIndex;
            await _lighthouseSettingsService.SetPowerDownModeAsync((PowerDownMode)PowerDownModeIndex);
        }
    }

    public async void OnClickDevice(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is LighthouseObject device)
        {
            device.SetManaged(!device.IsManaged);
            Log.Information($"Clicked: {device.Name} ({device.BluetoothAddress}) : {device.IsManaged}");
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.Lighthouse).ToArray());
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
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.Lighthouse).ToArray());
        }
        else
        {
            throw new InvalidProgramException("Clicked item is not a LighthouseListItemViewModel");
        }
    }

    public async void OnEditId(object? sender, EventArgs args)
    {
        if (sender is LighthouseObject lh)
        {
            Devices.First(d => d.BluetoothAddress == lh.BluetoothAddress).Id = lh.Id;
            await _lighthouseSettingsService.SetDevicesAsync(Devices.Select(d => d.Lighthouse).ToArray());
        }
        else
        {
            throw new InvalidProgramException("Sender is not a LighthouseObject");
        }
    }

    private string? GetHasUpdateMessage()
    {
        return HasUpdate ? string.Format("InfoBar_UpdateFound_Message".GetLocalized(), _latestVersion) : null;
    }
}
