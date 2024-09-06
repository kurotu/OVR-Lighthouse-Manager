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
    public string Name => _lighthouse.Name;

    public string BluetoothAddress => _lighthouse.BluetoothAddress;

    public bool IsManaged
    {
        get => _lighthouse.IsManaged;
        set
        {
            _lighthouse.IsManaged = value;
            OnPropertyChanged(nameof(IsManaged));
        }
    }

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

    public ICommand RemoveCommand
    {
        get;
    }

    public event EventHandler OnClickRemove = delegate { };

    public Lighthouse Lighthouse => _lighthouse;
    private readonly Lighthouse _lighthouse;

    public LighthouseObject(Lighthouse device, bool isFound)
    {
        _lighthouse = device;
        IsFound = isFound;
        RemoveCommand = new RelayCommand<LighthouseObject>((parameter) =>
        {
            parameter?.OnClickRemove(parameter, EventArgs.Empty);
        });
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
