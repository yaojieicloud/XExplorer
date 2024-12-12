using CommunityToolkit.Mvvm.ComponentModel;

namespace XExplorer.Core.Modes;

/// <summary>
/// DirInfo类用于表示目录信息。
/// </summary>
public class DirInfo
{
    /// <summary>
    /// 获取或设置目录的名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置目录的有效名称。
    /// </summary>
    public string ValidName { get; set; }
    
    /// <summary>
    /// 获取或设置目录的完整路径。
    /// </summary>
    public string FullName { get; set; }
}