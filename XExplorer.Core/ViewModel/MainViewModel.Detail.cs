using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;

namespace XExplorer.Core.ViewModel;

/// <summary>
/// The MainViewModel class serves as the primary ViewModel in the MVVM architecture
/// for the application, extending the functionality provided by the
/// ObservableObject from the CommunityToolkit.Mvvm.ComponentModel namespace.
/// This class is responsible for maintaining the state and logic required for
/// the UI, facilitating the data binding between the view and the model layers.
/// </summary>
partial class MainViewModel
{
    #region Command

    /// <summary>
    /// 播放指定路径的视频文件。
    /// </summary>
    /// <param name="param">表示文件路径的对象。</param>
    /// <remarks>
    /// 此方法首先将传入的参数转换为字符串路径，然后检查路径是否为空。如果路径不为空，那么它会使用PotPlayer播放器打开并播放该路径的视频文件，然后增加该视频的播放次数。
    /// </remarks> 
    [RelayCommand]
    public async Task PlayAsync(object param)
    {
        try
        {
            string path = param as string;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var currPath = this.AdjustPath(path);
                if (AppSettingsUtils.Default.OS == OS.Windows)
                    Process.Start(AppSettingsUtils.Default.Current.VLCPath, $"--no-one-instance \"{currPath}\"");
                else
                    Process.Start(AppSettingsUtils.Default.Current.VLCPath, $"\"{currPath}\"");
                
                var entry = await this.dataService.VideosService.FirstAsync(m => m.VideoPath == path);
                if (entry != null)
                {
                    entry.PlayCount++;
                    await this.dataService.VideosService.UpdateOnlyAsync(entry);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
    }

    /// <summary>
    /// 异步处理单个视频文件。
    /// </summary>
    /// <param name="param">表示视频实体的对象。</param>
    /// <returns>
    /// 表示异步操作的任务。
    /// </returns>
    /// <remarks>
    /// 此方法首先检查传入的参数是否为`VideoEntry`类型，如果是，它就会调用`ProcessVideoAsync`方法来处理这个视频实体。
    /// </remarks>
    [RelayCommand]
    public async Task ResetVideoAsync(object param)
    {
        try
        {
            if (param is VideoMode enty)
            {
                var video = await this.dataService.VideosService.FirstAsync(m=>m.Id == enty.Id);
                await this.ProcessVideoAsync(video, default);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            this.Notification($"{ex}");
        }
    }
    
    #endregion
}