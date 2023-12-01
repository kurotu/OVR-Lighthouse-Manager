using System.ComponentModel;
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
public partial class LighthouseObject : INotifyPropertyChanged
{
    public string Name
    {
        get; set;
    }

    public string BluetoothAddress
    {
        get; set;
    }

    public bool IsManaged
    {
        get => _isManaged;
        set
        {
            _isManaged = value;
            OnPropertyChanged(nameof(IsManaged));
        }
    }
    private bool _isManaged;

    public string Glyph => IsManaged ? "\uE73D" : "\uE739";

    public ICommand RemoveCommand
    {
        get;
    }

    public event EventHandler OnClickRemove = delegate { };

    public LighthouseListItem ListItem
    {
        set
        {
            Name = value.Name;
            BluetoothAddress = value.BluetoothAddress;
            IsManaged = value.IsManaged;
        }
    }

    public LighthouseObject()
    {
        Name = "";
        BluetoothAddress = "";
        RemoveCommand = new RelayCommand<LighthouseObject>((parameter) =>
        {
            parameter?.OnClickRemove(parameter, EventArgs.Empty);
        });
    }

    public static LighthouseObject FromLighthouseDevice(LighthouseDevice device)
    {
        var obj = new LighthouseObject
        {
            Name = device.Name,
            BluetoothAddress = AddressToStringConverter.AddressToString(device.BluetoothAddress),
            IsManaged = false
        };
        return obj;
    }

    public static LighthouseObject FromLighthouseListItem(LighthouseListItem item)
    {
        var obj = new LighthouseObject
        {
            Name = item.Name,
            BluetoothAddress = item.BluetoothAddress,
            IsManaged = item.IsManaged,
        };
        return obj;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
