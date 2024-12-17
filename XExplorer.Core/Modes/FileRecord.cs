using XExplorer.Core.Dictionaries;

namespace XExplorer.Core.Modes;

/// <summary>
/// 表示文件记录的类，包含文件的相关信息和功能。
/// </summary>
public class FileRecord
{
    /// <summary>
    /// 表示文件记录的类，用于保存文件的基本信息，包括名称、有效名称和完整路径。
    /// </summary>
    public FileRecord()
    {
    }

    /// <summary>
    /// 表示文件记录的类，包含文件的相关信息和功能。
    /// </summary>
    public FileRecord(string fullName)
    {
        this.FullName = this.AdjustPath(fullName);
        this.Name = Path.GetFileName(this.FullName);
        this.ValidName = Path.GetRelativePath(AppSettingsUtils.Default.Current.Volume,this.FullName);
        this.RelativePath = this.ValidName;
        this.Ext = Path.GetExtension(this.FullName);
        this.NameWithoutExt = Path.GetFileNameWithoutExtension(this.FullName);
        this.Dir = new DirRecord(Path.GetDirectoryName(this.FullName));
    }
    
    /// <summary>
    /// 表示文件记录的类，包含文件的相关信息和功能。
    /// </summary>
    public FileRecord(string fullName, string rootDir):this(fullName)
    {
        this.RelativePath = Path.GetRelativePath(rootDir,this.FullName);
    }

    /// <summary>
    /// 获取或设置文件的标题或说明文本。
    /// </summary>
    public string NameWithoutExt { get; set; }
    
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
    /// 获取或设置文件的相对路径。
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// 获取或设置文件的扩展名。
    /// </summary>
    public string Ext { get; set; }

    /// <summary>
    /// 获取或设置与文件关联的目录信息。
    /// </summary>
    public DirRecord Dir { get; set; }
    
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