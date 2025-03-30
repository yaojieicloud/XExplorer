using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.Popup;
using XExplorer.Core.Service;

namespace XExplorer.Core.ViewModel.Settings;

public partial class SettingsViewModel
{ 
    /// <summary>
    /// password
    /// </summary>
    [ObservableProperty]
    private string password = "1024";
}