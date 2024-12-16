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
    public static Video ToVideo(this VideoMode mode, Video video = null)
    {
        var result = video ?? new Video();
        result.Id = video == null ? mode.Id : video.Id;
        result.Caption = mode.Caption;
        result.Dir = mode.Dir;
        result.VideoDir = mode.VideoDir;
        result.DataDir = mode.DataDir;
        result.VideoPath = mode.VideoPath;
        result.Length = mode.Length;
        result.PlayCount = mode.PlayCount;
        result.ModifyTime = mode.ModifyTime;
        result.Evaluate = mode.Evaluate;
        result.MD5 = mode.Md5;
        result.Times = mode.Times;
        result.Status = mode.Status;
        result.Snapshots = mode.Snapshots.ToList();
        return result;
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
    public static VideoMode ToMode(this Video video, VideoMode mode = null)
    {
        var result = mode ?? new VideoMode();
        result.Id = mode == null ? video.Id : mode.Id;
        result.Caption = video.Caption;
        result.Dir = video.Dir;
        result.VideoDir = video.VideoDir;
        result.DataDir = video.DataDir;
        result.VideoPath = video.VideoPath;
        result.Length = video.Length;
        result.PlayCount = video.PlayCount;
        result.ModifyTime = video.ModifyTime;
        result.Evaluate = video.Evaluate;
        result.Md5 = video.MD5;
        result.Times = video.Times;
        result.Status = video.Status;
        result.Snapshots = new ObservableCollection<Snapshot>(video.Snapshots);

        return result;
    }

    /// <summary>
    /// 将 Video 实例列表转换为 VideoMode 实例列表。
    /// </summary>
    /// <param name="videos">要转换的 Video 实例列表。</param>
    /// <returns>转换后的 VideoMode 实例列表。</returns>
    public static List<VideoMode> ToModes(this List<Video> videos) => videos.Select(m => m.ToMode()).ToList();
}