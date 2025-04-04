using System.Diagnostics;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;

namespace XExplorer.Core.ViewModel.Settings;

public partial class SettingsViewModel
{
    #region ZIP

    private void ExtractArchive(string archivePath, List<string> passwords, string outputDirectory)
    {
        if (string.IsNullOrEmpty(archivePath))
            throw new ArgumentException("Archive path cannot be null or empty.", nameof(archivePath));

        if (!File.Exists(archivePath))
            throw new FileNotFoundException("The archive file does not exist.", archivePath);

        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty.", nameof(outputDirectory));

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory); // Ensure the output directory exists

        var success = false;
        var stb = new StringBuilder();
        stb.AppendLine();
        stb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------");
        stb.AppendLine($"Start extracting archive [{archivePath}], target directory [{outputDirectory}]...");
        foreach (var password in passwords)
        {
            try
            {
                stb.AppendLine($"Trying password: {password}");

                var opt = string.IsNullOrWhiteSpace(password)
                    ? new ReaderOptions()
                    : new ReaderOptions() { Password = password };
                using var archive = ArchiveFactory.Open(File.OpenRead(archivePath), opt);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        stb.AppendLine($"Extracting: {entry.Key}");
                        entry.WriteToDirectory(outputDirectory, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                stb.AppendLine($"Successfully extracted the archive using password: {password}");
                success = true;
                break; // Exit loop if extraction is successful
            }
            catch (InvalidOperationException ex)
            {
                stb.AppendLine($"Password '{password}' failed. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                stb.AppendLine($"An error occurred: {ex.Message}");
            }
        }

        if (!success)
            stb.AppendLine($"Failed to extract the archive with the provided passwords.[{archivePath}]");
        else
            File.Delete(archivePath);

        stb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------");
        Log.Information(stb.ToString());
    }

    #endregion
    
    /// <summary>
    ///     打开指定目录的方法。
    ///     根据输入目录路径尝试进行路径调整，并在不同平台上调用对应的文件管理器（例如 Windows 的资源管理器）
    ///     打开目录界面，便于用户浏览目录内容。
    /// </summary>
    /// <param name="dir">需要打开的目录路径。可以是目录路径或文件路径。</param>
    private void OpenFolder(string dir)
    {
        try
        {
            var path = AdjustPath(dir);

            if (File.Exists(path)) path = Path.GetDirectoryName(path);

            // Running on Windows
            if (AppSettingsUtils.Default.OS == OS.Windows)
                Process.Start("explorer.exe", path);
            else
                Process.Start("open", path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
        }
    }
    
        /// <summary>
    ///     调整指定路径以适应当前操作系统环境的方法。
    ///     此方法根据配置的系统平台，将路径在 Windows 格式和 Mac 格式之间进行转换，
    ///     确保路径在不同平台上能够正确解析。
    /// </summary>
    /// <param name="path">需要调整的原始路径字符串。</param>
    /// <returns>适配当前操作系统后的路径字符串。</returns>
    /// <remarks>
    ///     本方法通过检测当前操作系统类型（如 Windows 或 MacCatalyst），
    ///     将路径中的卷标和分隔符进行相应替换：
    ///     - 若当前系统为 MacCatalyst，则将路径中的 Windows 卷标替换为 Mac 卷标，并将路径分隔符从反斜杠 ('\\') 替换为正斜杠 ('/')。
    ///     - 若当前系统非 MacCatalyst（假定为 Windows），则将路径中的 Mac 卷标替换为 Windows 卷标，并将路径分隔符从正斜杠 ('/') 替换为反斜杠 ('\\')。
    /// </remarks>
    /// <example>
    ///     假设当前系统为 MacCatalyst：
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

    /// <summary>
    ///     获取相对目录路径的方法。
    ///     此方法根据 AppSettings 的定义，移除 Windows 或 Mac 平台特定的卷标信息后，
    ///     返回处理后的相对目录路径。
    /// </summary>
    /// <param name="path">完整的目录路径，包含平台相关的卷标前缀。</param>
    /// <returns>移除卷标信息后的相对目录路径。</returns>
    /// <remarks>
    ///     此方法的主要用途是将平台相关的绝对路径转换为统一的相对路径表示形式，
    ///     以便后续的路径处理逻辑使用。
    ///     具体步骤如下：
    ///     1. 将路径中的 Windows 卷标（例如 "C:\"）移除。
    ///     2. 将路径中的 Mac 卷标（例如 "/Volumes/"）移除。
    ///     3. 使用 <see cref="Path.GetDirectoryName(string)" /> 从处理后的路径中提取上级目录路径。
    /// </remarks>
    /// <example>
    ///     假设输入路径分别为：
    ///     <list type="bullet">
    ///         <item>Windows: "C:\Root\Dir1\File.txt"</item>
    ///         <item>Mac: "/Volumes/Data/Root/Dir1/File.txt"</item>
    ///     </list>
    ///     方法返回值为：
    ///     <list type="bullet">
    ///         <item>Windows: "\Root\Dir1"</item>
    ///         <item>Mac: "Data/Root/Dir1"</item>
    ///     </list>
    /// </example>
    private string GetRelativeDir(string path)
    {
        path = AdjustPath(path);
        path = path.Replace(AppSettingsUtils.Default.Windows.Volume, string.Empty);
        path = path.Replace(AppSettingsUtils.Default.Mac.Volume, string.Empty);
        path = Path.GetDirectoryName(path);
        return path;
    }

    /// <summary>
    ///     获取相对路径的方法。
    ///     此方法根据当前应用的设置，将传入的路径去掉 Windows 或 Mac 的根目录前缀，返回去除前缀后的相对路径。
    /// </summary>
    /// <param name="path">需要处理的绝对路径。</param>
    /// <returns>去掉根目录前缀后的相对路径字符串。</returns>
    private string GetRelativePath(string path)
    {
        path = AdjustPath(path);
        path = path.Replace(AppSettingsUtils.Default.Windows.Volume, string.Empty);
        path = path.Replace(AppSettingsUtils.Default.Mac.Volume, string.Empty);
        return path;
    }
}

public class UnzipRequest
{
    public string Dir { get; set; }
    public List<string> Passwords { get; set; }
}