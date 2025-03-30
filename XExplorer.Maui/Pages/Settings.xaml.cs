using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XExplorer.Core.ViewModel.Settings;

namespace XExplorer.Maui.Pages;

public partial class Settings : ContentPage
{
    public Settings()
    {
        InitializeComponent();
    }
    
    private void MainGrid_OnSizeChanged(object? sender, EventArgs e)
    {
        if (this.BindingContext is SettingsViewModel vm)
        {
            vm.MainViewHeight = this.Height;
        }
    }
}