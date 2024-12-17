using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using XExplorer.Core.Modes;
using XExplorer.Core.Service;
using XExplorer.Core.Utils;

namespace XExplorer.Core.ViewModel;

/// <summary>
///     The MainViewModel class serves as the primary ViewModel in the MVVM architecture
///     for the application, extending the functionality provided by the
///     ObservableObject from the CommunityToolkit.Mvvm.ComponentModel namespace.
///     This class is responsible for maintaining the state and logic required for
///     the UI, facilitating the data binding between the view and the model layers.
/// </summary>
partial class MainViewModel
{
    #region Static

    /// <summary>
    /// 支持的图片扩展名列表
    /// </summary>
    private static readonly List<string> picExts = new List<string> { ".jpg", ".png", ".gif", ".bmp" };

    /// <summary>
    /// 支持的视频扩展名列表
    /// </summary>
    private static readonly List<string> videoExts = new List<string>
        { ".mp4", ".avi", ".mkv", ".rmvb", ".wmv", ".ts", ".m4v", ".mov", ".flv" };

    /// <summary>
    /// 视频最小大小（单位：MB）
    /// </summary>
    private readonly decimal videoMiniSize = 110 * 1024 * 1024;
    
    #endregion
    
    #region Fields

    /// <summary>
    ///     一个私有变量，与 <see cref="DataService" /> 类关联，负责
    ///     提供数据操作服务，由 <see cref="MainViewModel" /> 使用以实现
    ///     应用程序的核心数据逻辑。这一服务可能涉及对视频、
    ///     快照和密码数据的管理。
    /// </summary>
    private readonly DataService dataService;

    /// <summary>
    ///     一个私有布尔变量，表示是否正在进行处理操作。
    ///     在 <see cref="MainViewModel" /> 类中用于跟踪当前处理状态，
    ///     常用于用户界面的交互逻辑中，以在执行长时间操作时
    ///     向用户提供反馈，如显示加载动画或阻止重复操作。
    /// </summary>
    [ObservableProperty]
    private bool processing;

    /// <summary>
    ///     一个私有的可观察集合，包含 <see cref="VideoMode" /> 对象。
    ///     此集合用于在应用程序的主视图模型中管理和表示多个视频实体，
    ///     方便进行数据绑定和界面更新。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VideoMode> videos = new();

    /// <summary>
    ///     一个私有变量，代表用户当前选择的目录路径。用于
    ///     记录和管理用户导航的文件夹，支持界面中目录
    ///     相关操作的实现。当目录加载时，
    ///     此变量会更新以反映用户的新选择。
    /// </summary>
    [ObservableProperty]
    private DirRecord selectedDir;

    /// <summary>
    ///     一个私有变量，用于存储目录路径的集合。该集合是
    ///     <see cref="ObservableCollection{T}" /> 类型，支持在 UI 中动态更新和绑定。
    ///     <see cref="dirs" /> 变量由 <see cref="MainViewModel" /> 管理，负责跟踪应用程序
    ///     中需要显示的所有目录路径。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DirRecord> dirs = new();

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
    /// 主视图的高度
    /// </summary>
    [ObservableProperty]
    private double mainViewHeight;

    #endregion
}