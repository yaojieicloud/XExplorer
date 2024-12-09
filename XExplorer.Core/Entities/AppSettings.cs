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
    ///     DBPath
    /// </summary>
    public string DBPath { get; set; } = "";

    /// <summary>
    ///     VLCPath
    /// </summary>
    public string VLCPath { get; set; } = "/Applications/VLC.app/Contents/MacOS/VLC";
}