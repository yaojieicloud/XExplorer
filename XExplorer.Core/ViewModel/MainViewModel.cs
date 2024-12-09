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
/// The MainViewModel class serves as the primary ViewModel in the MVVM architecture
/// for the application, extending the functionality provided by the
/// ObservableObject from the CommunityToolkit.Mvvm.ComponentModel namespace.
/// This class is responsible for maintaining the state and logic required for
/// the UI, facilitating the data binding between the view and the model layers.
/// </summary>
public partial class MainViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    #region Fields

    /// <summary>
    /// 一个私有变量，与 <see cref="DataService"/> 类关联，负责
    /// 提供数据操作服务，由 <see cref="MainViewModel"/> 使用以实现
    /// 应用程序的核心数据逻辑。这一服务可能涉及对视频、
    /// 快照和密码数据的管理。
    /// </summary>
    private DataService dataService;

    /// <summary>
    /// 一个私有布尔变量，表示是否正在进行处理操作。
    /// 在 <see cref="MainViewModel"/> 类中用于跟踪当前处理状态，
    /// 常用于用户界面的交互逻辑中，以在执行长时间操作时
    /// 向用户提供反馈，如显示加载动画或阻止重复操作。
    /// </summary>
    [ObservableProperty]
    private bool processing = false;

    /// <summary>
    /// 一个私有的可观察集合，包含 <see cref="VideoMode"/> 对象。
    /// 此集合用于在应用程序的主视图模型中管理和表示多个视频实体，
    /// 方便进行数据绑定和界面更新。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VideoMode> videos = new();

    [ObservableProperty]
    private string selectedDir;
    #endregion

    #region Command

    /// <summary>
    /// 异步加载选定目录中的视频数据。
    /// </summary>
    /// <returns>返回一个执行加载操作的任务。</returns>
    [RelayCommand]
    public async Task LoadDirAsync()
    {
        try
        {
            this.Processing = true;
            this.Videos.Clear();
            var enties = await this.dataService.VideosService.QueryAsync(this.SelectedDir);
            var modes = enties.ToModes();
            this.Videos = new ObservableCollection<VideoMode>(modes);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error"); 
        }
        finally
        {
            this.Processing = false;
        }
    }
    
    #endregion

    /// <summary>
    /// MainViewModel 类是应用程序中 MVVM 架构的主要 ViewModel。
    /// 此类扩展了 CommunityToolkit.Mvvm.ComponentModel 命名空间中的
    /// ObservableObject，实现了应用程序 UI 所需的状态和逻辑管理，
    /// 促进了视图层和模型层之间的数据绑定。
    /// </summary>
    public MainViewModel()
    {
        this.dataService = new DataService();
    }
}