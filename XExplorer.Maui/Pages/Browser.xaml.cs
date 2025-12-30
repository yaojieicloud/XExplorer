using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XExplorer.Core.ViewModel.Browser;
using XExplorer.Core.ViewModel.Settings;

namespace XExplorer.Maui.Pages;

public partial class Browser : ContentPage
{
    public Browser()
    {
        InitializeComponent();
    }

    private void MainGrid_OnSizeChanged(object? sender, EventArgs e)
    {
        if (this.BindingContext is BrowserViewModel vm)
        {
            vm.MainViewHeight = this.Height;
        }
    }
}