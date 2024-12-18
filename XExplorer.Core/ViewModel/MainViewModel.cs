using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
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
            await Task.Delay(5000);
            var enties = await dataService.VideosService.QueryAsync(this.SelectedDir.ValidName);
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
    /// 异步处理选定文件夹中的视频文件。
    /// 此方法通过命令模式触发，会记录处理耗时并显示通知。
    /// 若在处理过程中出现异常，将捕获并记录错误信息。
    /// </summary>
    /// <returns>表示异步操作完成的任务。</returns>
    [RelayCommand]
    public async Task ProcessVideosAsync()
    {
        var st = Stopwatch.StartNew();
        
        try
        {
            await this.ProcessVideosAsync(this.SelectedDir);
            st.Stop();
            this.Notification($"文件夹 [{this.selectedDir.FullName}] 全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
        }
        catch (Exception e)
        {
            this.Notification($"{e}");
            Log.Error(e, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
        finally
        {
            st.Stop();
            Log.Information($"文件夹 [{this.selectedDir.FullName}] 全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
        }
    }
     
    /// <summary>
    /// 异步方法，用于通过文件夹选择器功能让用户选择一个文件夹并将其信息加载到应用状态中。
    /// 如果选择了文件夹，该方法会生成一个 <see cref="DirRecord"/> 对象，
    /// 包含文件夹的名称、完整路径以及格式化后的路径信息（相对于配置中的卷路径）。
    /// 此方法在选择文件夹后会更新 ViewModel 中的 <c>SelectedDir</c> 属性。
    /// </summary>
    /// <returns>
    /// 返回一个 <see cref="Task"/>，表示异步操作的执行状态。
    /// 如果用户成功选择了文件夹，则更新模型状态；否则，无状态变化。
    /// </returns>
    [RelayCommand]
    public async Task SelectFolderAsync()
    { 
        var result = await FolderPicker.PickAsync(default);
        if (result != null)
        {
            var dir = new DirRecord()
            {
                Name = result.Folder.Name,
                FullName = result.Folder.Path,
                ValidName = result.Folder.Path.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty)
            };
            this.SelectedDir = dir;
        }
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
    /// 异步方法 DownloadRuntimeAsync 用于下载和配置 FFmpeg 的运行时文件。
    /// 此方法将 FFmpeg 的运行时文件下载到指定的路径，并设置可执行文件路径，
    /// 确保在应用程序中可以正确使用 FFmpeg 功能。
    /// </summary>
    /// <returns>表示异步操作的 Task。</returns>
    [RelayCommand]
    public async Task DownloadRuntimeAsync()
    {
        var ffmpegDir = Path.Combine(AppContext.BaseDirectory, "ffmpeg", AppSettingsUtils.Default.OS);

        if (!Directory.Exists(ffmpegDir))
            Directory.CreateDirectory(ffmpegDir);

        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Shared, ffmpegDir);
        FFmpeg.SetExecutablesPath(ffmpegDir);
        Log.Information($"已经将 FFmpeg 运行时下载到 [{ffmpegDir}]");
        this.Notification($"已经将 FFmpeg 运行时下载到 [{ffmpegDir}]");
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