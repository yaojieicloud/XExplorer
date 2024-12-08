namespace XExplorer.Core.Modes;

/// <summary>
///     应用程序设置类，包括 Windows 和 Mac 平台的路径信息。
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the database directory path.
    /// </summary>
    public string DBDir { get; set; }
    
    /// <summary>
    ///     获取或设置 Windows 平台的数据路径。
    /// </summary>
    public string WinDataPath { get; set; } = string.Empty;

    /// <summary>
    ///     获取或设置 Windows 平台的播放器路径。
    /// </summary>
    public string WinPlayerPath { get; set; } = string.Empty;

    /// <summary>
    ///     获取或设置 Mac 平台的数据路径。
    /// </summary>
    public string MacDataPath { get; set; } = string.Empty;

    /// <summary>
    ///     获取或设置 Mac 平台的播放器路径。
    /// </summary>
    public string MacPlayerPath { get; set; } = string.Empty;

    /// <summary>
    ///     获取或设置是否启用多线程下载。
    /// </summary>
    public int TaskCount { get; set; } = 1;

    /// <summary>
    ///     备份目录
    /// </summary>
    public string BackupPath { get; set; }
}