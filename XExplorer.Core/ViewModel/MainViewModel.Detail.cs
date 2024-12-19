using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;
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
    #region Command

    /// <summary>
    ///     播放指定路径的视频文件。
    /// </summary>
    /// <param name="param">表示文件路径的对象。</param>
    /// <remarks>
    ///     此方法首先将传入的参数转换为字符串路径，然后检查路径是否为空。如果路径不为空，那么它会使用PotPlayer播放器打开并播放该路径的视频文件，然后增加该视频的播放次数。
    /// </remarks>
    [RelayCommand]
    public async Task PlayAsync(object param)
    {
        try
        {
            var path = param as string;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var currPath = AdjustPath(path);
                currPath = Path.Combine(AppSettingsUtils.Default.Current.Volume, currPath);
                if (AppSettingsUtils.Default.OS == OS.Windows)
                    Process.Start(AppSettingsUtils.Default.Current.VLCPath, $"--no-one-instance \"{currPath}\"");
                else
                    Process.Start(AppSettingsUtils.Default.Current.VLCPath, $"\"{currPath}\"");

                var entry = await dataService.VideosService.FirstAsync(m => m.VideoPath == path);
                if (entry != null)
                {
                    entry.PlayCount++;
                    await dataService.VideosService.UpdateOnlyAsync(entry);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
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
    public async Task OpenFolderAsync(object param)
    {
        if (param is VideoMode mode)
        {
            var path = Path.Combine(AppSettingsUtils.Default.Current.Volume, mode.VideoDir);
            this.OpenFolder($"{path}");
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 删除指定视频文件及其相关数据。
    /// </summary>
    /// <param name="param">表示视频文件信息的对象，该对象应为 <see cref="VideoMode"/> 类型。</param>
    /// <returns>异步任务对象，表示该删除操作完成的状态。</returns>
    [RelayCommand]
    public async Task DelFolderAsync(object param)
    {
        if (param is VideoMode mode)
        {
            this.Processing = true; 
            
            try
            {
                if (await this.Ask($"确认删除视频 「{mode.VideoPath}」?"))
                    return;
                
                var fullDir = Path.Combine(AppSettingsUtils.Default.Current.Volume, mode.VideoDir);
                var fullPath = Path.Combine(AppSettingsUtils.Default.Current.Volume, mode.VideoPath);
                var videos =
                    await this.dataService.VideosService.QueryAsync(m =>
                        m.VideoDir == mode.VideoDir && m.Id != mode.Id);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Log.Information($"视频文件 [{fullPath}] 已删除。");
                }
                else
                {
                    Log.Information($"视频文件 [{fullPath}] 不存在，无需删除。");
                }

                if (!(videos?.Any() ?? false))
                {
                    var files = Directory.GetFiles(fullDir);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                        Log.Information($"视频文件 [{file}] 已删除。");
                    }
                }

                foreach (var snapshot in mode.Snapshots)
                {
                    if (File.Exists(snapshot.FullPath))
                    {
                        File.Delete(snapshot.FullPath);
                        Log.Information($"视频快照 [{snapshot.FullPath}] 已删除。");
                    }
                    else
                    {
                        Log.Information($"视频快照 [{snapshot.FullPath}] 不存在，无需删除。");
                    }
                }

                this.dataService.VideosService.DeleteAsync(mode.Id);
                this.Videos.Remove(mode);
                Log.Information($"视频数据 [{mode.Caption}] 已删除。");
                //this.Notification($"视频数据 [{mode.Caption}] 已删除。");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{MethodBase.GetCurrentMethod().Name} Is Error");
                this.Notification($"{e}");
            }
            finally
            {
                this.Processing = false; 
            }
        }
    }

    /// <summary>
    ///     异步处理单个视频文件。
    /// </summary>
    /// <param name="param">表示视频实体的对象。</param>
    /// <returns>
    ///     表示异步操作的任务。
    /// </returns>
    /// <remarks>
    ///     此方法首先检查传入的参数是否为`VideoEntry`类型，如果是，它就会调用`ProcessVideoAsync`方法来处理这个视频实体。
    /// </remarks>
    [RelayCommand]
    public async Task ResetVideoAsync(object param)
    {
        try
        {
            if (param is VideoMode enty)
            {
                var st = Stopwatch.StartNew();
                var fullName = Path.Combine(AppSettingsUtils.Default.Current.Volume, enty.VideoPath);
                var video = await ProcessVideoAsync(new FileRecord(fullName));
                await dataService.VideosService.UpdateAsync(video);
                video.ToMode(enty);
                st.Stop();
                Notification($"视频重置完成，耗时 {st.Elapsed.TotalSeconds} 秒");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
        }
    }

    /// <summary>
    /// 更新视频评分信息。
    /// </summary>
    /// <param name="param">表示包含视频信息的对象参数。</param>
    /// <returns>一个异步任务，用于表示操作的完成状态。</returns>
    [RelayCommand]
    public async Task RatingChangedAsync(object param)
    {
        try
        {
            if (param is VideoMode enty)
            {
                var video = enty.ToVideo();
                await dataService.VideosService.UpdateOnlyAsync(video);
                this.Notification($"视频更新完成。");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
        }
    }

    /// <summary>
    /// 处理图片点击事件，触发与特定逻辑相关的操作。
    /// </summary>
    /// <param name="param">传递的与点击事件相关的参数对象。</param>
    /// <returns>表示异步操作的任务。</returns>
    [RelayCommand]
    public async Task ImageTappedAsync(object param)
    {
        if (param is SnapshotMode snapshot)
        {
            this.SelectedImg = snapshot.FullPath;
            this.ShowImg = true;
        }

        await Task.CompletedTask;
    }

    #endregion
}