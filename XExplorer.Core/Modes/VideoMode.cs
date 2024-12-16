using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

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
    /// 获取或设置视频文件的存储目录。
    /// </summary>
    [ObservableProperty]
    private string? dir;

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
    private int evaluate = 0;

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
    /// Status
    /// </summary>
    [ObservableProperty]
    private decimal status = 1;

    /// <summary>
    /// 快照列表.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Snapshot> snapshots = new ObservableCollection<Snapshot>();
}