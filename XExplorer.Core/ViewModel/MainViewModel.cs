using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using XExplorer.Core.Dictionaries;
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
public partial class MainViewModel : ObservableObject
{
    /// <summary>
    ///     MainViewModel 类是应用程序中 MVVM 架构的主要 ViewModel。
    ///     此类扩展了 CommunityToolkit.Mvvm.ComponentModel 命名空间中的
    ///     ObservableObject，实现了应用程序 UI 所需的状态和逻辑管理，
    ///     促进了视图层和模型层之间的数据绑定。
    /// </summary>
    public MainViewModel()
    {
        this.dataService = new DataService();
        this.InitDirs();
    }

    #region Command

    /// <summary>
    ///     异步加载选定目录中的视频数据。
    /// </summary>
    /// <returns>返回一个执行加载操作的任务。</returns>
    [RelayCommand]
    public async Task LoadDirAsync()
    {
        try
        {
            Processing = true;
            Videos.Clear();
            var enties = await dataService.VideosService.QueryAsync(this.SelectedDir.ValidName);

            Parallel.ForEach(enties, m =>
            {
                m.Snapshots.ForEach(s =>
                {
                    s.Path = Path.Combine(AppSettingsUtils.Default.Current.DataDir, s.Path);
                    s.Path = AppSettingsUtils.Default.OS == OS.MacCatalyst
                        ? s.Path.Replace('\\', '/')
                        : s.Path.Replace('/', '\\');
                });
            });

            var modes = enties.ToModes();
            Videos = new ObservableCollection<VideoMode>(modes);
            this.Notification($"数据加载完成！");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            this.Notification($"{ex}");
        }
        finally
        {
            Processing = false;
        }
    }

    /// <summary>
    /// 打开包含指定文件的文件夹。
    /// </summary>
    /// <param name="param">表示文件路径的对象。</param>
    /// <remarks>
    /// 此方法首先将传入的参数转换为字符串，然后获取该路径的目录名。最后，如果路径不为空，它会使用Windows资源管理器打开该目录。
    /// </remarks>
    [RelayCommand]
    public async Task FolderAsync(object param)
    {
        this.OpenFolder($"{param}");
    }

    /// <summary>
    /// 打开日志目录。
    /// </summary>
    /// <remarks>
    /// 此方法首先获取当前应用程序域的基目录，然后构造日志目录的路径。最后，它使用Windows资源管理器打开日志目录。
    /// </remarks>
    [RelayCommand]
    public async Task OpenLogDirAsync()
    {
        try
        {
            string baseDirectory = AppContext.BaseDirectory;
            if (AppSettingsUtils.Default.OS == OS.MacCatalyst)
            {
                // Adjust the path for macOS to get the app bundle root directory
                var path = Path.GetFullPath(Path.Combine(baseDirectory, "..", ".."));
                path = Path.Combine(path, "logs");
                this.OpenFolder(path);
            }
            else
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                this.OpenFolder(path);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            this.Notification($"{ex}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 通知方法，用于在应用程序中显示消息通知。
    /// 根据输入参数设置消息内容，并控制消息显示状态。
    /// </summary>
    /// <param name="param">
    /// 消息内容的参数。如果参数为字符串类型，则其内容将被设置为通知消息。
    /// </param>
    [RelayCommand]
    public void Notification(object param)
    {
        if (param is string msg)
        {
            this.Message = msg;
            this.IsShow = true;
        }
    }

    /// <summary>
    /// CloseNotification 是一个用于关闭通知的方法。
    /// 此方法通过将 Message 属性设置为 null 和 IsShow 属性设置为 false，
    /// 清除当前通知信息并隐藏通知界面。
    /// </summary>
    [RelayCommand]
    public void CloseNotification()
    {
        this.Message = null;
        this.IsShow = false;
    }

    #endregion
}