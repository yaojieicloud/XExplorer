using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XExplorer.Core.Modes;

/// <summary>
/// 表示一个视频实体，用于数据库操作。
/// </summary>
[Table("Videos")]
public class Video : ModeBase
{
    /// <summary>
    /// 获取或设置视频的标题。
    /// </summary>
    public string Caption { get; set; }

    /// <summary>
    /// 获取或设置视频文件的存储目录。
    /// </summary>
    public string? RootDir { get; set; }

    /// <summary>
    /// 获取或设置视频文件的存储目录。
    /// </summary>
    public string? VideoDir { get; set; }
    
    /// <summary>
    /// 获取或设置视频文件的快照存储目录。
    /// </summary>
    public string? DataDir { get; set; }

    /// <summary>
    /// 获取或设置视频文件的完整路径。
    /// </summary>
    public string VideoPath { get; set; }

    /// <summary>
    /// 获取或设置视频的长度（单位：秒）。
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// 获取或设置视频的播放次数。
    /// </summary>
    public long PlayCount { get; set; } = 0;

    /// <summary>
    /// 获取或设置视频的最后修改时间。
    /// </summary>
    public DateTime? ModifyTime { get; set; }

    /// <summary>
    /// 获取或设置视频评价分数。
    /// </summary>
    public int Evaluate { get; set; } = 0;

    /// <summary>
    /// MD5
    /// </summary>
    public string? MD5 { get; set; }

    /// <summary>
    /// 获取或设置视频的时长（单位：秒）。
    /// </summary>
    public long? Times { get; set; }

    /// <summary>
    /// 获取或设置视频的宽度。
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 获取或设置视频的高度。
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// 指示视频是否为宽屏格式。
    /// </summary>
    public bool WideScrenn { get; set; }

    /// <summary>
    /// 获取或设置最小值，该属性未映射到数据库。
    /// </summary>
    [NotMapped]
    public long? Minute => this.Times / 60;
    
    /// <summary>
    /// Status
    /// </summary>
    public decimal Status { get; set; } = 1;

    /// <summary>
    /// 快照列表.
    /// </summary>
    public List<Snapshot> Snapshots { get; set; } = [];
}