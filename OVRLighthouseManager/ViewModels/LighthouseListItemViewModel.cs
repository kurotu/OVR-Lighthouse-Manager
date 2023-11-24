using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Services;

namespace OVRLighthouseManager.ViewModels;
public partial class LighthouseListItemViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _bluetoothAddress;

    [ObservableProperty]
    private bool _isManaged;

    public string Glyph => IsManaged ? "\uE73D" : "\uE739";

    public event EventHandler OnClickRemove = delegate { };

    public ICommand ClickRemoveDeviceCommand
    {
        get;
    }

    public ICommand ClickPowerOn
    {
        get;
    }

    public ICommand ClickSleep
    {
        get;
    }

    public ICommand ClickStandby
    {
        get;
    }

    public LighthouseListItemViewModel(LighthouseListItem item)
    {
        _name = item.Name;
        _bluetoothAddress = item.BluetoothAddress;
        _isManaged = item.IsManaged;
        ClickRemoveDeviceCommand = new RelayCommand(OnClickRemoveDevice);
        ClickPowerOn = new RelayCommand(OnClickPowerOn);
        ClickSleep = new RelayCommand(OnClickSleep);
        ClickStandby = new RelayCommand(OnClickStandby);
    }

    public LighthouseListItemViewModel(LighthouseDevice device)
    {
        _name = device.Name;
        _bluetoothAddress = AddressToStringConverter.AddressToString(device.BluetoothAddress);
        _isManaged = false;
        ClickRemoveDeviceCommand = new RelayCommand(OnClickRemoveDevice);
        ClickPowerOn = new RelayCommand(OnClickPowerOn);
        ClickSleep = new RelayCommand(OnClickSleep);
        ClickStandby = new RelayCommand(OnClickStandby);
    }

    public LighthouseListItem ToListItem()
    {
        return new LighthouseListItem()
        {
            Name = Name,
            BluetoothAddress = BluetoothAddress,
            IsManaged = IsManaged
        };
    }

    public void SetManaged(bool managed)
    {
        IsManaged = managed;
        OnPropertyChanged(nameof(Glyph));
    }

    private void OnClickRemoveDevice()
    {
        OnClickRemove(this, EventArgs.Empty);
    }

    private async void OnClickPowerOn()
    {
        System.Diagnostics.Debug.WriteLine($"Powering on {Name}");
        await LighthouseService.PowerOn(AddressToStringConverter.StringToAddress(BluetoothAddress));
    }

    private async void OnClickSleep()
    {
        System.Diagnostics.Debug.WriteLine($"Sleeping {Name}");
        await LighthouseService.Sleep(AddressToStringConverter.StringToAddress(BluetoothAddress));
    }

    private async void OnClickStandby()
    {
        System.Diagnostics.Debug.WriteLine($"Standing by {Name}");
        await LighthouseService.Standby(AddressToStringConverter.StringToAddress(BluetoothAddress));
    }
}
