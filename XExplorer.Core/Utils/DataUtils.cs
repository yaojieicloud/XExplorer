using System.Collections.ObjectModel;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Utils;

/// <summary>
/// 提供用于数据处理的静态工具方法集合。
/// </summary>
public static class DataUtils
{
    /// <summary>
    /// 将 VideoMode 转换为 Video 实体。
    /// </summary>
    /// <returns>Video 实例</returns>
    public static Video ToVideo(this VideoMode mode)
    {
        return new Video
        {
            Caption = mode.Caption,
            Dir = mode.Dir,
            VideoDir = mode.VideoDir,
            DataDir = mode.DataDir,
            VideoPath = mode.VideoPath,
            Length = mode.Length,
            PlayCount = mode.PlayCount,
            ModifyTime = mode.ModifyTime,
            Evaluate = mode.Evaluate,
            MD5 = mode.Md5,
            Status = mode.Status,
            Snapshots = new List<Snapshot>(mode.Snapshots)
        };
    }

    /// <summary>
    /// 将 VideoMode 列表转换为 Video 实体的列表。
    /// </summary>
    /// <param name="modes">要转换的 VideoMode 对象列表。</param>
    /// <returns>一个包含转换后的 Video 实例的列表。</returns>
    public static List<Video> ToVideos(this List<VideoMode> modes) => modes.Select(m => m.ToVideo()).ToList(); 
    
    /// <summary>
    /// 将 Video 实体转换为 VideoMode。
    /// </summary>
    /// <param name="video">Video 实例</param>
    /// <returns>VideoMode 实例</returns>
    public static VideoMode ToMode(this Video video)
    {
        return new VideoMode
        {
            Caption = video.Caption,
            Dir = video.Dir,
            VideoDir = video.VideoDir,
            DataDir = video.DataDir,
            VideoPath = video.VideoPath,
            Length = video.Length,
            PlayCount = video.PlayCount,
            ModifyTime = video.ModifyTime,
            Evaluate = video.Evaluate,
            Md5 = video.MD5,
            Status = video.Status,
            Snapshots = new ObservableCollection<Snapshot>(video.Snapshots)
        };
    }

    /// <summary>
    /// 将 Video 实例列表转换为 VideoMode 实例列表。
    /// </summary>
    /// <param name="videos">要转换的 Video 实例列表。</param>
    /// <returns>转换后的 VideoMode 实例列表。</returns>
    public static List<VideoMode> ToModes(this List<Video> videos) => videos.Select(m => m.ToMode()).ToList();
}