using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using File = System.IO.File;
using ReaderOptions = SharpCompress.Readers.ReaderOptions;

namespace XExplorer.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ZipController : ControllerBase
{
    private const string ROOT_DIR = "/volume1/99_资源收藏";
    private readonly ILogger<ZipController> _logger;

    public ZipController(ILogger<ZipController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UnzipAsync([FromBody] UnzipRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                var path = string.IsNullOrWhiteSpace(request.Dir) ? ROOT_DIR : Path.Combine(ROOT_DIR, request.Dir);
                var zipFiles = this.GetPaths(path);
                foreach (var file in zipFiles)
                    this.ExtractArchive(file, request.Passwords, path);

                return this.Ok();
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return this.BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<List<string>> GetAsync(string path = null)
    {
        path = string.IsNullOrWhiteSpace(path) ? ROOT_DIR : Path.Combine(ROOT_DIR, path);
        return await Task.Run(() => this.GetPaths(path));
    }

    private void ExtractArchive(string archivePath, List<string> passwords, string outputDirectory)
    {
        try
        {
            if (string.IsNullOrEmpty(archivePath))
                throw new ArgumentException("Archive path cannot be null or empty.", nameof(archivePath));

            if (!System.IO.File.Exists(archivePath))
                throw new FileNotFoundException("The archive file does not exist.", archivePath);

            if (string.IsNullOrEmpty(outputDirectory))
                throw new ArgumentException("Output directory cannot be null or empty.", nameof(outputDirectory));

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory); // Ensure the output directory exists

            var success = false;
            var stb = new StringBuilder();
            var name = Path.GetFileNameWithoutExtension(archivePath);
            var outDir = Path.Combine(outputDirectory, name);
            stb.AppendLine();
            stb.AppendLine(
                "----------------------------------------------------------------------------------------------------------------------------------------------");
            stb.AppendLine($"Start extracting archive [{archivePath}], target directory [{outputDirectory}]...");
            foreach (var password in passwords)
            {
                try
                {
                    if (!Directory.Exists(outDir))
                        Directory.CreateDirectory(outDir);

                    stb.AppendLine($"Trying password: {password}");

                    var opt = string.IsNullOrWhiteSpace(password)
                        ? new ReaderOptions()
                        : new ReaderOptions() { Password = password };
                    using var archive = ArchiveFactory.Open(System.IO.File.OpenRead(archivePath), opt);
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            stb.AppendLine($"Extracting: {entry.Key}");
                            entry.WriteToDirectory(outDir, new ExtractionOptions
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
                stb.AppendLine(
                    $"Failed to extract the archive with the provided passwords.[{archivePath}] ---> [{outDir}]");
            else
                System.IO.File.Delete(archivePath);

            stb.AppendLine(
                "----------------------------------------------------------------------------------------------------------------------------------------------");
            Log.Information(stb.ToString());
        }
        catch (Exception e)
        {
            Log.Error(e, $"failed to extract the archive with the provided passwords.[{archivePath}]");
        }
    }

    public List<string> GetPaths(string path)
    {
        var zipExts = new HashSet<string>()
            { ".zip", ".7z", ".rar", ".tar", ".gz", ".bz2", ".xz", ".iso", ".cab", ".tgz", ".lz", ".z" };
        List<string> paths = new List<string>();

        try
        {
            var dir = new DirectoryInfo(path);
            var compressedFiles = dir.GetFiles()
                .Where(file => zipExts.Contains(file.Extension.ToLower()))
                .ToList();
            foreach (var file in compressedFiles)
            {
                paths.Add(file.FullName); // 添加文件路径
                Log.Information($"Found file: {file.FullName}");
            }

            // 获取当前目录中的子目录
            foreach (var subDir in dir.GetDirectories())
            {
                paths.Add(subDir.FullName); // 添加子目录路径
                Log.Information($"Found sub dir: {subDir.FullName}");

                // 递归查找子目录中的文件和目录
                paths.AddRange(GetPaths(subDir.FullName));
            }
        }
        catch (UnauthorizedAccessException exx)
        {
            Log.Error(exx, $"访问被拒绝: {path}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"处理目录时发生错误: {path}, 错误信息: {ex.Message}");
        }

        return paths;
    }
}

public class UnzipRequest
{
    public string Dir { get; set; }
    public List<string> Passwords { get; set; }
}