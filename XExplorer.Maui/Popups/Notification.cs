using CommunityToolkit.Maui.Views;
namespace XExplorer.Maui.Popups;

public class Notification:Popup
{
    public Notification(string message)
    {
        Content = new Frame
        {
            BackgroundColor = Colors.Gray,
            CornerRadius = 10,
            Padding = new Thickness(10),
            Content = new Label
            {
                Text = message,
                TextColor = Colors.White,
                FontSize = 14
            },
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.End,
            TranslationX = -20, // 调整水平偏移位置
            TranslationY = 20  // 调整垂直偏移位置
        };
    }
}