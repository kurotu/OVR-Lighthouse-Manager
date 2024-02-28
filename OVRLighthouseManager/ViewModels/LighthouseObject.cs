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
using OVRLighthouseManager.Views;

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

    public bool RequiresId
    {
        get
        {
            var l = new Lighthouse()
            {
                Name = Name,
            };
            return l.Version == LighthouseVersion.V1 && string.IsNullOrEmpty(Id);
        }
    }

    public string? Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(nameof(RequiresId));
        }
    }
    private string? _id;

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

    public bool IsFound
    {
        get => _isFound;
        set
        {
            _isFound = value;
            OnPropertyChanged(nameof(IsFound));
        }
    }
    private bool _isFound;

    public string Glyph => IsManaged ? "\uE73D" : "\uE739";

    public ICommand EditIdCommand
    {
        get;
    }

    public ICommand RemoveCommand
    {
        get;
    }

    public event EventHandler OnClickRemove = delegate { };
    public event EventHandler OnEditId = delegate { };

    public Lighthouse ListItem
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
        EditIdCommand = new RelayCommand<LighthouseObject>(async (parameter) =>
        {
            var dialog = new LighthouseV1IdInputDialog();
            dialog.Id = parameter?.Id ?? "";
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                parameter!.Id = dialog.Id;
                OnEditId(parameter, EventArgs.Empty);
            }
        });
        RemoveCommand = new RelayCommand<LighthouseObject>((parameter) =>
        {
            parameter?.OnClickRemove(parameter, EventArgs.Empty);
        });
    }

    public static LighthouseObject FromLighthouse(Lighthouse device)
    {
        var obj = new LighthouseObject
        {
            Name = device.Name,
            BluetoothAddress = AddressToStringConverter.AddressToString(device.BluetoothAddressValue),
            IsManaged = false,
            IsFound = true,
        };
        return obj;
    }

    public static LighthouseObject FromLighthouseListItem(Lighthouse item)
    {
        var obj = new LighthouseObject
        {
            Name = item.Name,
            BluetoothAddress = item.BluetoothAddress,
            Id = item.Id,
            IsManaged = item.IsManaged,
        };
        return obj;
    }

    public Lighthouse ToListItem()
    {
        return new Lighthouse()
        {
            Name = Name,
            BluetoothAddress = BluetoothAddress,
            Id = Id,
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
