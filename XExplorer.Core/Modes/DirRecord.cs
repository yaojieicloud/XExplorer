using CommunityToolkit.Mvvm.ComponentModel;
using XExplorer.Core.Dictionaries;

namespace XExplorer.Core.Modes;

/// <summary>
/// DirInfo类用于表示目录信息。
/// </summary>
public class DirRecord
{
    /// <summary>
    /// 表示一个目录的信息记录。
    /// 包含目录名称、合法名称以及完整路径的属性。
    /// </summary>
    public DirRecord()
    {
    }

    /// <summary>
    /// 表示一个目录的信息记录。
    /// 包含目录名称、合法名称以及完整路径的属性。
    /// 提供路径调整功能以适配不同操作系统的路径格式。
    /// </summary>
    public DirRecord(string fullName)
    {
        this.FullName = this.AdjustPath(fullName);
        this.Name = Path.GetFileName(this.FullName);
        this.ValidName = Path.GetRelativePath(AppSettingsUtils.Default.Current.Volume,this.FullName);
    }
    
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
    
    /// <summary>
    /// 调整路径格式，根据运行平台将路径在 Windows 和 Mac 之间进行转换。
    /// </summary>
    /// <param name="path">需要调整的路径字符串。</param>
    /// <returns>调整后的路径字符串。</returns>
    private string AdjustPath(string path)
    {
        if (AppSettingsUtils.Default.OS == OS.MacCatalyst)
        {
            // 将 Windows 路径转换成 Mac 的路径
            path = path.Replace(AppSettingsUtils.Default.Windows.Volume, AppSettingsUtils.Default.Mac.Volume);
            path = path.Replace('\\', '/');
        }
        else
        {
            // 将 Mac 路径转换成 Windows 的路径
            path = path.Replace(AppSettingsUtils.Default.Mac.Volume, AppSettingsUtils.Default.Windows.Volume);
            path = path.Replace('/', '\\');
        }

        return path;
    }
}