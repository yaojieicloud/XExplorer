using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Serilog;
using Syncfusion.Maui.Popup;
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
        dataService = new DataService();
        InitDirs();
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
            var dir = SelectedDir.ValidName == Screnn.All ? null : SelectedDir.ValidName;
            bool? wideScrenn = ScrennMode == Screnn.None
                ? null
                : ScrennMode == Screnn.Wide
                    ? true
                    : false;
            var enties = await dataService.VideosService.QueryAsync(
                dir,
                wideScrenn: wideScrenn);
            var modes = enties.ToModes();
            Videos = new ObservableCollection<VideoMode>(modes);
            Message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 目录 {SelectedDir.FullName} 加载完成。";
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
            Message = ex.Message;
        }
        finally
        {
            Processing = false;
        }
    }

    /// <summary>
    ///     异步处理选定文件夹中的视频文件。
    ///     此方法通过命令模式触发，会记录处理耗时并显示通知。
    ///     若在处理过程中出现异常，将捕获并记录错误信息。
    /// </summary>
    /// <returns>表示异步操作完成的任务。</returns>
    [RelayCommand]
    public async Task ProcessVideosAsync()
    {
        var st = Stopwatch.StartNew();
        Processing = true;

        try
        {
            var result = await FolderPicker.PickAsync(default);
            if (result != null)
            {
                var dir = new DirRecord
                {
                    Name = result.Folder.Name,
                    FullName = result.Folder.Path,
                    ValidName = result.Folder.Path.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty)
                };

                await WithVideosAsync(dir);
                st.Stop();
                Notification($"文件夹 [{selectedDir.FullName}] 全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
            }
        }
        catch (Exception e)
        {
            Notification($"{e}");
            Log.Error(e, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
        finally
        {
            st.Stop();
            Log.Information($"文件夹 [{selectedDir.FullName}] 全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
            Processing = false;
        }
    }

    /// <summary>
    ///     异步批量处理视频文件的方法，允许用户选择文件夹并对文件夹内的视频进行批处理操作。
    ///     方法完成后会提供处理状态的通知及耗时信息。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    [RelayCommand]
    public async Task BatchProcessVideosAsync()
    {
        var st = Stopwatch.StartNew();
        Processing = true;

        try
        {
            var result = await FolderPicker.PickAsync(default);
            if (result != null)
            {
                var dir = new DirRecord
                {
                    Name = result.Folder.Name,
                    FullName = result.Folder.Path,
                    ValidName = result.Folder.Path.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty)
                };

                await BatchProcessVideosAsync(dir);
                st.Stop();
                Notification($"文件夹 [{selectedDir.FullName}] 全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
            }
        }
        catch (Exception e)
        {
            Notification($"{e}");
            Log.Error(e, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
        finally
        {
            st.Stop();
            Log.Information($"文件夹 [{selectedDir.FullName}] 全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
            Processing = false;
        }
    }

    /// <summary>
    ///     异步方法，用于通过文件夹选择器功能让用户选择一个文件夹并将其信息加载到应用状态中。
    ///     如果选择了文件夹，该方法会生成一个 <see cref="DirRecord" /> 对象，
    ///     包含文件夹的名称、完整路径以及格式化后的路径信息（相对于配置中的卷路径）。
    ///     此方法在选择文件夹后会更新 ViewModel 中的 <c>SelectedDir</c> 属性。
    /// </summary>
    /// <returns>
    ///     返回一个 <see cref="Task" />，表示异步操作的执行状态。
    ///     如果用户成功选择了文件夹，则更新模型状态；否则，无状态变化。
    /// </returns>
    [RelayCommand]
    public async Task SelectFolderAsync()
    {
        var result = await FolderPicker.PickAsync(default);
        if (result != null)
        {
            var dir = new DirRecord
            {
                Name = result.Folder.Name,
                FullName = result.Folder.Path,
                ValidName = result.Folder.Path.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty)
            };
            SelectedDir = dir;
        }
    }

    /// <summary>
    ///     打开日志目录。
    /// </summary>
    /// <remarks>
    ///     此方法首先获取当前应用程序域的基目录，然后构造日志目录的路径。最后，它使用Windows资源管理器打开日志目录。
    /// </remarks>
    [RelayCommand]
    public async Task OpenLogDirAsync()
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            if (AppSettingsUtils.Default.OS == OS.MacCatalyst)
            {
                // Adjust the path for macOS to get the app bundle root directory
                var path = Path.GetDirectoryName(AppSettingsUtils.Default.Current.LogFile);
                OpenFolder(path);
            }
            else
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                OpenFolder(path);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Md5Async 方法是一个异步任务，用于计算文件的 MD5 哈希值。
    ///     此方法通常用于文件校验操作，确保文件的完整性。
    /// </summary>
    /// <returns>返回表示异步计算完成的任务。</returns>
    [RelayCommand]
    public async Task Md5Async()
    {
        var st = Stopwatch.StartNew();

        try
        {
            Information("开始MD5处理。。。");
            await WithMd5Async();
        }
        catch (Exception e)
        {
            Notification($"{e}");
        }
        finally
        {
            st.Stop();
            Information($"MD5处理结束，耗时 「{st.Elapsed.TotalSeconds}」 秒");
        }
    }

    /// <summary>
    ///     KillAsync 方法会查找并终止当前操作系统上的指定名称的进程。
    ///     在 Windows 上，它会终止名为 "vlc" 的进程，
    ///     在 macOS 上也会查找并终止同名进程。
    ///     如果在终止过程中发生异常，方法将记录相关错误信息。
    /// </summary>
    /// <returns>返回一个已完成的 Task，表示该操作的异步结果。</returns>
    [RelayCommand]
    public async Task KillAsync()
    {
        // 检查当前操作系统
        if (AppSettingsUtils.Default.OS == OS.Windows)
        {
            // Windows平台
            foreach (var process in Process.GetProcessesByName("vlc"))
                try
                {
                    process.Kill();
                    Information($"Killed process {process.Id} on Windows.");
                }
                catch (Exception ex)
                {
                    Information($"Error killing process {process.Id}: {ex}", ex.Message);
                }
        }
        else if (AppSettingsUtils.Default.OS == OS.MacCatalyst)
        {
            foreach (var process in Process.GetProcessesByName("vlc"))
                try
                {
                    process.Kill();
                    Information($"Killed process {process.Id} {process.ProcessName} on macOS.");
                }
                catch (Exception ex)
                {
                    Information($"Error killing process {process.Id}: {ex}", ex.Message);
                }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 异步查询具有重复 MD5 值的视频条目，并更新 UI 中的视频列表。
    /// </summary>
    /// <returns>返回一个表示异步操作的 Task。</returns>
    [RelayCommand]
    public async Task QueryDuplicateMD5Async()
    {
        try
        {
            Processing = true;
            Videos.Clear();

            var enties = await dataService.VideosService.QueryMD5DuplicateAsync();
            var modes = enties.ToModes();
            Videos = new ObservableCollection<VideoMode>(modes);
            Message = $"重复数据[{enties?.Count}]行加载完成。";
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
            Message = ex.Message;
        }
        finally
        {
            Processing = false;
        }
    }


    /// <summary>
    ///     异步方法 DownloadRuntimeAsync 用于下载和配置 FFmpeg 的运行时文件。
    ///     此方法将 FFmpeg 的运行时文件下载到指定的路径，并设置可执行文件路径，
    ///     确保在应用程序中可以正确使用 FFmpeg 功能。
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
        Notification($"已经将 FFmpeg 运行时下载到 [{ffmpegDir}]");
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
    /// 将指定的 CollectionView 滚动到顶部位置。
    /// </summary>
    /// <param name="param">需要传递的参数，应为 CollectionView 类型。</param>
    [RelayCommand]
    public void ScrollToTop(object param)
    {
        if (param is CollectionView view)
        {
            view.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
        }
    }

    /// <summary>
    /// ScrollToBottom 方法用于将指定的 CollectionView 滚动到其内容的末尾。
    /// </summary>
    /// <param name="param">类型为 CollectionView 的对象，用于执行滚动操作。</param>
    [RelayCommand]
    public void ScrollToBottom(object param)
    {
        if (param is CollectionView view)
        {
            var itemsSource = view.ItemsSource as IList;
            if (itemsSource != null && itemsSource.Count > 0)
            {
                view.ScrollTo(itemsSource.Count - 1, position: ScrollToPosition.End, animate: true);
            }
        }
    }

    /// <summary>
    /// 在 CollectionView 中滚动到数据源的中间位置。
    /// </summary>
    /// <param name="param">CollectionView 对象，用于执行滚动操作。</param>
    [RelayCommand]
    public void ScrollToMiddle(object param)
    {
        if (param is CollectionView view)
        {
            var itemsSource = view.ItemsSource as IList;
            if (itemsSource != null && itemsSource.Count > 0)
            {
                view.ScrollTo(itemsSource.Count / 2 + 1, position: ScrollToPosition.End, animate: true);
            }
        }
    }

    #endregion
}