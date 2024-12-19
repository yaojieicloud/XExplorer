using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace XExplorer.Core.Modes;

/// <summary>
/// 表示视频快照的实体类。
/// </summary> 
public partial class SnapshotMode : ObservableObject
{
    /// <summary>
    /// 获取或设置快照的唯一标识符。
    /// </summary>
    [ObservableProperty]
    public long id;
    
    /// <summary>
    /// 获取或设置与此快照关联的视频的标识符。
    /// </summary>
    [ObservableProperty]
    public long videoId;

    /// <summary>
    /// 获取或设置快照的文件路径。
    /// </summary>
    [ObservableProperty]
    public string path;

    /// <summary>
    /// 获取或设置快照完整的路径。
    /// </summary>
    [ObservableProperty]
    public string fullPath;
}