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
    
    // File 菜单的二级菜单
    private async void OnFileMenuClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("File Menu", "Cancel", null, "New File", "Open File", "Save File");
        if (action != "Cancel")
        {
            await DisplayAlert("You Selected", action, "OK");
        }
    }

    // Edit 菜单的二级菜单
    private async void OnEditMenuClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Edit Menu", "Cancel", null, "Copy", "Paste", "Undo");
        if (action != "Cancel")
        {
            await DisplayAlert("You Selected", action, "OK");
        }
    }

    private void Button_OnClicked(object? sender, EventArgs e)
    {
        //this.ShowPopup(DirPopup);
    }

    private void MainGrid_OnSizeChanged(object? sender, EventArgs e)
    {
        if (this.BindingContext is MainViewModel vm)
        {
            vm.MainViewHeight = this.MainGrid.Height - this.MainTool.Height - 30;
        }
    }
}