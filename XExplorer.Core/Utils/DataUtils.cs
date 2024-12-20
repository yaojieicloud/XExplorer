using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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
        result.RootDir = mode.RootDir;
        result.VideoDir = mode.VideoDir;
        result.DataDir = mode.DataDir;
        result.VideoPath = mode.VideoPath;
        result.Length = mode.Length;
        result.PlayCount = mode.PlayCount;
        result.ModifyTime = mode.ModifyTime;
        result.Evaluate = mode.Evaluate;
        result.MD5 = mode.Md5;
        result.Times = mode.Times;
        result.Width = mode.Width;
        result.Height = mode.Height;
        result.WideScrenn = mode.WideScrenn;
        result.Status = mode.Status;
        result.Snapshots = mode.Snapshots.ToSnapshots();
        return result;
    }

    /// <summary>
    /// 将 SnapshotMode 转换为 Snapshot 实体。
    /// </summary>
    /// <param name="mode">源 SnapshotMode 实例。</param>
    /// <param name="snapshot">可选的目标 Snapshot 实例，若为 null 则创建一个新的实例。</param>
    /// <returns>Snapshot 实例。</returns>
    public static Snapshot ToSnapshot(this SnapshotMode mode, Snapshot snapshot = null)
    {
        var result = snapshot ?? new Snapshot();
        result.Id = mode.Id;
        result.Path = mode.Path;
        return result;
    }

    /// <summary>
    /// 将 SnapshotMode 列表转换为 Snapshot 实例列表。
    /// </summary>
    /// <param name="modes">包含 SnapshotMode 的列表。</param>
    /// <returns>转换后的 Snapshot 实例列表。</returns>
    public static List<Snapshot> ToSnapshots(this IList<SnapshotMode> modes) =>
        modes.Select(m => m.ToSnapshot()).ToList();
 
    /// <summary>
    /// 将 VideoMode 列表转换为 Video 实体的列表。
    /// </summary>
    /// <param name="modes">要转换的 VideoMode 对象列表。</param>
    /// <returns>一个包含转换后的 Video 实例的列表。</returns>
    public static List<Video> ToVideos(this IList<VideoMode> modes) => modes.Select(m => m.ToVideo()).ToList();

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
        result.RootDir = video.RootDir;
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
        result.Minute = video.Minute;
        result.WideScrenn = video.WideScrenn;
        result.Width = video.Width;
        result.Height = video.Height;
        result.Snapshots = new ObservableCollection<SnapshotMode>(video.Snapshots.ToModes());

        return result;
    }

    /// <summary>
    /// 将 Snapshot 转换为 SnapshotMode 实体。
    /// </summary>
    /// <param name="snapshot">源 Snapshot 实例。</param>
    /// <param name="mode">可选的目标 SnapshotMode 实例，若为 null 则创建一个新的实例。</param>
    /// <returns>SnapshotMode 实例。</returns>
    public static SnapshotMode ToMode(this Snapshot snapshot, SnapshotMode mode = null)
    {
        var result = mode ?? new SnapshotMode();
        result.Id = snapshot.Id;
        result.Path = snapshot.Path;
        result.FullPath = snapshot.FullPath;
        return result;
    }

    /// <summary>
    /// 将快照列表转换为快照模式列表。
    /// </summary>
    /// <param name="snapshots">要转换的快照列表。</param>
    /// <returns>转换后的快照模式列表。</returns>
    public static List<SnapshotMode> ToModes(this IList<Snapshot> snapshots) =>
        snapshots.Select(m => m.ToMode()).ToList();

    /// <summary>
    /// 将 Video 实例列表转换为 VideoMode 实例列表。
    /// </summary>
    /// <param name="videos">要转换的 Video 实例列表。</param>
    /// <returns>转换后的 VideoMode 实例列表。</returns>
    public static List<VideoMode> ToModes(this IList<Video> videos) => videos.Select(m => m.ToMode()).ToList();
}