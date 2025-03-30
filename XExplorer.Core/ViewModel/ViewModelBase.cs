using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Serilog;
using Syncfusion.Maui.Popup;
using XExplorer.Core.Modes;
using XExplorer.Core.Service;

namespace XExplorer.Core.ViewModel;

public partial class ViewModelBase : ObservableObject
{
    #region Properties
    
    /// <summary>
    ///     一个私有变量，与 <see cref="DataService" /> 类关联，负责
    ///     提供数据操作服务，由 <see cref="MainViewModel" /> 使用以实现
    ///     应用程序的核心数据逻辑。这一服务可能涉及对视频、
    ///     快照和密码数据的管理。
    /// </summary>
    protected readonly DataService dataService;
    
    /// <summary>
    ///     一个私有布尔变量，表示是否正在进行处理操作。
    ///     在 <see cref="MainViewModel" /> 类中用于跟踪当前处理状态，
    ///     常用于用户界面的交互逻辑中，以在执行长时间操作时
    ///     向用户提供反馈，如显示加载动画或阻止重复操作。
    /// </summary>
    [ObservableProperty]
    private bool processing;
    
    /// <summary>
    /// 一个私有字段，用于存储与应用程序状态或用户界面相关的消息内容。
    /// 该字段通过实现数据绑定的机制，被 UI 动态显示或更新，以实现对
    /// 消息通知、信息提示或用户交互内容的支持。
    /// </summary>
    [ObservableProperty]
    private string message;

    /// <summary>
    /// 一个私有布尔变量，表示当前界面元素的显示状态。通常与
    /// 界面的可见性或动画效果相关联，支持对 UI 层的动态更新。
    /// 此变量对 UI 状态的变化响应至关重要，绑定到对应的视图属性以确保
    /// 状态同步。
    /// </summary>
    [ObservableProperty]
    private bool isShow;
    
    /// <summary>
    /// 控制弹出按钮的外观模式
    /// </summary>
    [ObservableProperty]
    private PopupButtonAppearanceMode appearanceMode = PopupButtonAppearanceMode.OneButton;
    
    /// <summary>
    /// 表示取消操作的状态
    /// </summary>
    [ObservableProperty]
    private bool isCancel = true;
    
    /// <summary>
    /// 主视图的高度
    /// </summary>
    [ObservableProperty]
    private double mainViewHeight;
    
    #endregion
    public ViewModelBase()
    {
        dataService = new DataService();
    }

    /// <summary>
    ///     Decline 方法用于执行用户拒绝操作，处理相关的业务逻辑。
    /// </summary>
    [RelayCommand]
    public void Decline()
    {
        IsCancel = true;
        IsShow = false;
    }

    /// <summary>
    ///     Accept 方法用于处理对话框的接受操作。
    ///     执行逻辑将更改相关标志状态以关闭用户界面提示或通知。
    /// </summary>
    [RelayCommand]
    public void Accept()
    {
        IsCancel = false;
        IsShow = false;
    }
    
    /// <summary>
    ///     通知方法，用于在应用程序中显示消息通知。
    ///     根据输入参数设置消息内容，并控制消息显示状态。
    /// </summary>
    /// <param name="param">
    ///     消息内容的参数。如果参数为字符串类型，则其内容将被设置为通知消息。
    /// </param>
    [RelayCommand]
    public void Notification(object param)
    {
        if (param is string msg)
        {
            Message = msg;
            IsShow = true;
            AppearanceMode = PopupButtonAppearanceMode.OneButton;
        }
    }
    
    /// <summary>
    ///     Ask方法用于处理参数，并根据传入的消息内容更新相关的UI显示和交互模式。
    /// </summary>
    /// <param name="param">传入的参数，期望为字符串类型的消息内容。</param>
    [RelayCommand]
    public async Task<bool> Ask(object param)
    {
        if (param is string msg)
        {
            Message = msg;
            IsShow = true;
            IsCancel = true;
            AppearanceMode = PopupButtonAppearanceMode.TwoButton;

            while (IsShow)
                await Task.Delay(500);

            await Task.Delay(500);
            return IsCancel;
        }

        return true;
    }
    
    /// <summary>
    ///     CloseNotification 是一个用于关闭通知的方法。
    ///     此方法通过将 Message 属性设置为 null 和 IsShow 属性设置为 false，
    ///     清除当前通知信息并隐藏通知界面。
    /// </summary>
    [RelayCommand]
    public void CloseNotification()
    {
        Message = null;
        IsShow = false;
        IsCancel = true;
    }
    
    /// <summary>
    /// 显示信息消息的方法。
    /// 此方法记录信息消息，并更新 UI 线程中的消息字段。
    /// </summary>
    /// <param name="message">需要显示或记录的消息内容。</param>
    /// <param name="title">可选的标题，用于覆盖主消息字段的显示值。</param>
    protected void Information(string message, string title = null)
    {
        Log.Information(message);
        if (MainThread.IsMainThread)
            this.Message = title ?? message;
        else
            MainThread.BeginInvokeOnMainThread(() => this.Message = title ?? message);
    }
}