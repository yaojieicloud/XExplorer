using System.ComponentModel.DataAnnotations;

namespace XExplorer.Core.Modes;

/// <summary>
/// 实体基类
/// </summary>
public abstract class ModeBase
{
    [Key]
    public long Id { get; set; }
}