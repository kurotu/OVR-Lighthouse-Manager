using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;

namespace OVRLighthouseManager.Views;

public sealed partial class LighthouseRenameDialog : ContentDialog, INotifyPropertyChanged
{
    private string _newName = "";
    public string NewName
    {
        get => _newName;
        set
        {
            _newName = value;
            OnPropertyChanged();
        }
    }

    private string _realName = "";
    public string RealName
    {
        get => _realName;
        set
        {
            _realName = value;
            OnPropertyChanged();
        }
    }

    public LighthouseRenameDialog()
    {
        this.InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
