using System.ComponentModel.DataAnnotations.Schema;

namespace XExplorer.Core.Modes;

/// <summary>
/// 表示视频快照的实体类。
/// </summary>
[Table("Snapshots")]
public class Snapshot : ModeBase
{
    /// <summary>
    /// 获取或设置与此快照关联的视频的标识符。
    /// </summary>
    [ForeignKey("Video")]
    public long VideoId { get; set; }

    /// <summary>
    /// 获取或设置快照的文件路径。
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 获取或设置快照完整的路径。
    /// </summary>
    [NotMapped]
    public string FullPath => System.IO.Path.Combine(AppSettingsUtils.Default.Current.SnapshotsDir, this.Path);
}