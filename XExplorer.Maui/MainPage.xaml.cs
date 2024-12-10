using XExplorer.Core.ViewModel;

namespace XExplorer.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
        this.BindingContext = new MainViewModel();
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
}