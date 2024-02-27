using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace OVRLighthouseManager.Views;

public sealed partial class LighthouseV1IdInputDialog : ContentDialog
{
    public string Id = "";

    public LighthouseV1IdInputDialog()
    {
        this.InitializeComponent();
    }
}
