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
    /**
     * Real device name, as reported over BLE. Used for version detection and commands.
     */
    public string Name => _lighthouse.Name;

    /**
     * Name shown in the UI: the custom name when set, otherwise the real device name.
     */
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(CustomName) ? CustomName : _lighthouse.Name;

    public string BluetoothAddress => _lighthouse.BluetoothAddress;

    public string? CustomName
    {
        get => _lighthouse.CustomName;
        set
        {
            if (_lighthouse.CustomName != value)
            {
                _lighthouse.CustomName = value;
                OnPropertyChanged(nameof(CustomName));
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(IsCustomNamed));
            }
        }
    }

    public bool IsCustomNamed => !string.IsNullOrWhiteSpace(CustomName);

    public LighthouseVersion Version => _lighthouse.Version;

    public string VersionText => _lighthouse.Version switch
    {
        LighthouseVersion.V1 => "V1",
        LighthouseVersion.V2 => "V2",
        _ => "",
    };

    public bool IsVersionKnown => _lighthouse.Version != LighthouseVersion.Unknown;

    public int? Channel
    {
        get => _lighthouse.Channel;
        set
        {
            if (_lighthouse.Channel != value)
            {
                _lighthouse.Channel = value;
                OnPropertyChanged(nameof(Channel));
                OnPropertyChanged(nameof(ChannelText));
                OnPropertyChanged(nameof(IsChannelKnown));
            }
        }
    }

    public string ChannelText => _lighthouse.Channel?.ToString() ?? "";

    public bool IsChannelKnown => _lighthouse.Channel.HasValue;

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

    public ICommand RenameCommand
    {
        get;
    }

    public ICommand RemoveCommand
    {
        get;
    }

    public event EventHandler OnClickRemove = delegate { };
    public event EventHandler OnEditId = delegate { };
    public event EventHandler OnRename = delegate { };

    public Lighthouse Lighthouse => _lighthouse;
    private readonly Lighthouse _lighthouse;

    public LighthouseObject(Lighthouse device, bool isFound)
    {
        _lighthouse = device;
        IsFound = isFound;
        EditIdCommand = new AsyncRelayCommand(async () =>
        {
            var dialog = new LighthouseV1IdInputDialog();
            dialog.Id = Id ?? "";
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Id = dialog.Id;
                OnEditId(this, EventArgs.Empty);
            }
        });
        RenameCommand = new AsyncRelayCommand(async () =>
        {
            var dialog = new LighthouseRenameDialog();
            dialog.NewName = CustomName ?? "";
            dialog.RealName = Lighthouse.Name;
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newName = dialog.NewName.Trim();
                CustomName = string.IsNullOrEmpty(newName) || newName == Lighthouse.Name ? null : newName;
                OnRename(this, EventArgs.Empty);
            }
        });
        RemoveCommand = new RelayCommand(() =>
        {
            OnClickRemove(this, EventArgs.Empty);
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
