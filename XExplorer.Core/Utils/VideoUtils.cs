using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Utils;

public static class VideoUtils
{
    /// <summary>
    /// Gets or sets the file system path to the youtube-dl executable.
    /// </summary>
    public static string YoutubeDLPath = string.Empty;

    /// <summary>
    /// Specifies the file system path to the FFmpeg executable used by the application.
    /// </summary>
    /// <remarks>Set this field to the full path of the FFmpeg binary if it is not available in the system's
    /// PATH environment variable. This value is used to locate and invoke FFmpeg for media processing tasks.</remarks>
    public static string FFmpegPath = string.Empty;

    /// <summary>
    /// Specifies the directory path where the youtube-dl executable is located.
    /// </summary>
    /// <remarks>Set this field to the full path of the directory containing the youtube-dl binary if it is
    /// not available in the system PATH. This field is intended for configuration purposes and should be set before
    /// invoking any functionality that depends on youtube-dl.</remarks>
    public static string YoutubeDLDir = string.Empty;

    /// <summary>
    /// Initializes the yt-dlp and FFmpeg components by ensuring their executables are present in the application
    /// directory. Downloads the required files if they do not already exist.
    /// </summary>
    /// <remarks>This method deletes any existing yt-dlp directory before recreating it and downloading the
    /// latest versions of yt-dlp and FFmpeg if necessary. Call this method before performing any operations that depend
    /// on these components.</remarks>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public static async Task InitYtDlpAsync()
    {
        var ytdlpDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppSettingsUtils.Default.OS, "Ytdlp");
        var ytdlpExePath = Path.Combine(ytdlpDirPath, "yt-dlp.exe");
        var ffmpegExePath = Path.Combine(ytdlpDirPath, "ffmpeg.exe");

        try
        {
            if (File.Exists(ytdlpExePath) && File.Exists(ffmpegExePath))
                return;

            if (Directory.Exists(ytdlpDirPath))
                Directory.Delete(ytdlpDirPath, true);

            Directory.CreateDirectory(ytdlpDirPath);

            // 确保组件存在
            if (!File.Exists(ytdlpExePath))
                await YoutubeDLSharp.Utils.DownloadYtDlp(ytdlpDirPath);
            if (!File.Exists(ffmpegExePath))
                await YoutubeDLSharp.Utils.DownloadFFmpeg(ytdlpDirPath);
        }
        finally
        {
            YoutubeDLPath = ytdlpExePath;
            FFmpegPath = ffmpegExePath;
            YoutubeDLDir = ytdlpDirPath;
        }
    }
}
