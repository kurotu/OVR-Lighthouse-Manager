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

    public bool RequiresId => _lighthouse.Version == LighthouseVersion.V1;
    public bool IsMissingId => RequiresId && string.IsNullOrEmpty(_lighthouse.Id);

    public bool SupportsIdentify => _lighthouse.Version == LighthouseVersion.V2;

    public string? Id
    {
        get => _lighthouse.Id;
        set
        {
            _lighthouse.Id = value;
            OnPropertyChanged(nameof(RequiresId));
        }
    }

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

    public Lighthouse Lighthouse => _lighthouse;
    private readonly Lighthouse _lighthouse;

    public LighthouseObject(Lighthouse device, bool isFound)
    {
        _lighthouse = device;
        IsFound = isFound;
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
