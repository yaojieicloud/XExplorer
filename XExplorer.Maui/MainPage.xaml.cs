using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using XExplorer.Core.ViewModel;
using ValueChangedEventArgs = Syncfusion.Maui.Inputs.ValueChangedEventArgs;

namespace XExplorer.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void MainGrid_OnSizeChanged(object? sender, EventArgs e)
    {
        if (this.BindingContext is MainViewModel vm)
        {
            vm.MainViewHeight = this.Height - 30;
            vm.MainContHeight = vm.MainViewHeight - this.MainTool.Height - 60;
        }
    }
}