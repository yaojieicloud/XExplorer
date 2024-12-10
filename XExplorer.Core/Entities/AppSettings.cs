namespace XExplorer.Core.Modes;

/// <summary>
///     应用程序设置类，包括 Windows 和 Mac 平台的路径信息。
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets the current configuration settings based on the specified operating system.
    /// Returns the Mac settings if the operating system is MacCatalyst,
    /// otherwise returns the Windows settings by default.
    /// </summary>
    public Conf Current
    {
        get
        {
            switch (this.OS)
            {
                case Dictionaries.OS.MacCatalyst:
                    return this.Mac;
                case Dictionaries.OS.Windows:
                default:
                    return this.Windows;
            }
        }
    }

    /// <summary>
    /// Gets or sets the operating system identifier for the application settings,
    /// used to determine the current configuration.
    /// </summary>
    public string OS { get; set; }

    /// <summary>
    /// Gets or sets the configuration settings specific to the Mac platform.
    /// </summary>
    public Conf Mac { get; set; }

    /// <summary>
    /// Gets or sets the configuration settings specific to the Windows platform.
    /// </summary>
    public Conf Windows { get; set; }

    /// <summary>
    /// TaskLimit
    /// </summary>
    public int TaskLimit { get; set; } = 1;
}

public class Conf
{
    /// <summary>
    /// 获取或设置数据存储目录路径，以便应用程序存放其相关的文件或数据。
    /// 根据当前操作系统的配置（Windows 或 MacCatalyst）可以有不同的路径设定。
    /// </summary>
    public string DataDir { get; set; }

    /// <summary>
    /// 获取或设置数据库文件的路径。
    /// DBPath 属性用于指定应用程序使用的数据库文件的存储位置，
    /// 以确保数据存储和访问的便捷性。
    /// </summary>
    public string DBPath { get; set; } = "";

    /// <summary>
    /// 获取或设置VLC播放器的路径。
    /// 默认值为 "/Applications/VLC.app/Contents/MacOS/VLC"。
    /// 该属性用于指定VLC播放器的可执行文件路径，以便在系统上运行或与其他应用程序进行交互。
    /// </summary>
    public string VLCPath { get; set; } = "/Applications/VLC.app/Contents/MacOS/VLC";

    /// <summary>
    /// 获取或设置应用程序的根目录路径。
    /// 根目录是配置文件、数据库及其他应用程序资源的基础位置，通常根据操作系统的不同而有所变化。
    /// </summary>
    public string RootDir { get; set; }
}