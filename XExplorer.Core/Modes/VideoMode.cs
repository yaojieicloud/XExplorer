using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using XExplorer.Core.Service;
using XExplorer.Core.Utils;

namespace XExplorer.Core.Modes;

/// <summary>
/// 表示一个视频实体
/// </summary>
public partial class VideoMode : ObservableObject
{
    /// <summary>
    /// 获取或设置视频的唯一标识符。
    /// </summary>
    [ObservableProperty]
    private long id;

    /// <summary>
    /// 获取或设置视频的标题。
    /// </summary>
    [ObservableProperty]
    private string caption;

    /// <summary>
    /// 获取或设置视频文件的存储根目录。
    /// </summary>
    [ObservableProperty]
    private string? rootDir;

    /// <summary>
    /// 获取或设置视频文件的存储目录。
    /// </summary>
    [ObservableProperty]
    private string? videoDir;

    /// <summary>
    /// 获取或设置视频文件的快照存储目录。
    /// </summary>
    [ObservableProperty]
    private string? dataDir;

    /// <summary>
    /// 获取或设置视频文件的完整路径。
    /// </summary>
    [ObservableProperty]
    private string videoPath;

    /// <summary>
    /// 获取或设置视频的长度（单位：秒）。
    /// </summary>
    [ObservableProperty]
    private long length;

    /// <summary>
    /// 获取或设置视频的播放次数。
    /// </summary>
    [ObservableProperty]
    private long playCount = 0;

    /// <summary>
    /// 获取或设置视频的最后修改时间。
    /// </summary>
    [ObservableProperty]
    private DateTime? modifyTime;

    /// <summary>
    /// 获取或设置视频评价分数。
    /// </summary>
    [ObservableProperty]
    private int evaluate;

    /// <summary>
    /// MD5
    /// </summary>
    [ObservableProperty]
    private string? md5;

    /// <summary>
    /// 获取或设置视频的时长（单位：秒）。
    /// </summary>
    [ObservableProperty]
    private long? times;

    /// <summary>
    /// 获取或设置视频的宽度。
    /// </summary>
    [ObservableProperty]
    private int width;

    /// <summary>
    /// 获取或设置视频的高度。
    /// </summary>
    [ObservableProperty]
    private int height;

    /// <summary>
    /// 指示视频是否为宽屏格式。
    /// </summary>
    [ObservableProperty]
    private bool wideScrenn;

    /// <summary>
    /// 获取视频的时长最小值（单位：分钟）。
    /// </summary>
    [ObservableProperty]
    private long? minute;

    /// <summary>
    /// Status
    /// </summary>
    [ObservableProperty]
    private decimal status = 1;

    /// <summary>
    /// 获取或设置视频分组的编号。
    /// </summary>
    [ObservableProperty]
    private int groupNo;

    /// <summary>
    /// 快照列表.
    /// </summary>
    private ObservableCollection<SnapshotMode> snapshots = new();

    public ObservableCollection<SnapshotMode> Snapshots
    {
        get => snapshots;
        set
        { 
            this.SetProperty(ref snapshots, value);
            var imgs = value?.Select(m => m.FullPath).ToList();
            this.Images = new ObservableCollection<string>(imgs ?? new List<string>());
        }
    }

    private object last;

    /// <summary>
    /// 获取视频快照的完整路径列表。
    /// </summary>
    [ObservableProperty]
    public ObservableCollection<string> images = new();

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
                if (enty.Evaluate == 0)
                    return;

                if (this.last != null && Convert.ToInt32(last) == enty.Evaluate)
                    return;

                var video = enty.ToVideo();
                var dataService = new DataService();
                await dataService.VideosService.UpdateOnlyAsync(video);
                Log.Information($"视频 [{enty.VideoPath}] 更新完成。");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
    }
}