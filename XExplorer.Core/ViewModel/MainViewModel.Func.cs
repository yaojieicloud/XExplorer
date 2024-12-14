using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using LibVLCSharp.Shared;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;
using XExplorer.Core.Service;
using XExplorer.Core.Utils;

namespace XExplorer.Core.ViewModel;

/// <summary>
///     The MainViewModel class serves as the primary ViewModel in the MVVM architecture
///     for the application, extending the functionality provided by the
///     ObservableObject from the CommunityToolkit.Mvvm.ComponentModel namespace.
///     This class is responsible for maintaining the state and logic required for
///     the UI, facilitating the data binding between the view and the model layers.
/// </summary>
partial class MainViewModel
{
    /// <summary>
    /// 初始化目录信息的方法。
    /// 此方法会从配置的根目录中获取所有子目录，并将其信息转化为 <see cref="DirInfo"/> 对象集合，
    /// 最终赋值给当前 ViewModel 的 <c>Dirs</c> 属性。
    /// </summary>
    /// <remarks>
    /// 具体步骤如下：
    /// 1. 调用 <see cref="Directory.GetDirectories(string)"/> 方法获取根目录中的所有子目录路径。
    /// 2. 遍历每一个子目录路径，将其转换为 <see cref="DirInfo"/> 对象实例，包含目录的名称、完整路径和有效名称信息。
    /// 3. 将所有的 <see cref="DirInfo"/> 对象存入 <see cref="ObservableCollection{T}"/> 集合中，并更新 <c>Dirs</c> 属性。
    /// </remarks>
    /// <example>
    /// 假设配置的根目录包括如下子目录：
    /// <list type="bullet">
    /// <item>C:\Root\Dir1</item>
    /// <item>C:\Root\Dir2</item>
    /// </list>
    /// 调用本方法后，<c>Dirs</c> 属性会包含以下内容：
    /// <list type="bullet">
    /// <item>目录名称：Dir1，有效名称：\Dir1，完整路径：C:\Root\Dir1</item>
    /// <item>目录名称：Dir2，有效名称：\Dir2，完整路径：C:\Root\Dir2</item>
    /// </list>
    /// </example>
    private void InitDirs()
    {
        var videoDirs = new List<DirInfo>();
        var allDirs = Directory.GetDirectories(AppSettingsUtils.Default.Current.RootDir);
        for (int i = 0; i < allDirs.Length; i++)
        {
            var dir = allDirs[i];
            var dirInfo = new DirectoryInfo(dir);
            var valid = dir.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty);
            videoDirs.Add(new() { Name = dirInfo.Name, FullName = dir, ValidName = valid });
        }

        this.Dirs = new ObservableCollection<DirInfo>(videoDirs);
    }

    /// <summary>
    /// 调整指定路径以适应当前操作系统环境的方法。
    /// 此方法根据配置的系统平台，将路径在 Windows 格式和 Mac 格式之间进行转换，
    /// 确保路径在不同平台上能够正确解析。
    /// </summary>
    /// <param name="path">需要调整的原始路径字符串。</param>
    /// <returns>适配当前操作系统后的路径字符串。</returns>
    /// <remarks>
    /// 本方法通过检测当前操作系统类型（如 Windows 或 MacCatalyst），
    /// 将路径中的卷标和分隔符进行相应替换：
    /// - 若当前系统为 MacCatalyst，则将路径中的 Windows 卷标替换为 Mac 卷标，并将路径分隔符从反斜杠 ('\\') 替换为正斜杠 ('/')。
    /// - 若当前系统非 MacCatalyst（假定为 Windows），则将路径中的 Mac 卷标替换为 Windows 卷标，并将路径分隔符从正斜杠 ('/') 替换为反斜杠 ('\\')。
    /// </remarks>
    /// <example>
    /// 假设当前系统为 MacCatalyst：
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
    /// 获取相对目录路径的方法。
    /// 此方法根据 AppSettings 的定义，移除 Windows 或 Mac 平台特定的卷标信息后，
    /// 返回处理后的相对目录路径。
    /// </summary>
    /// <param name="path">完整的目录路径，包含平台相关的卷标前缀。</param>
    /// <returns>移除卷标信息后的相对目录路径。</returns>
    /// <remarks>
    /// 此方法的主要用途是将平台相关的绝对路径转换为统一的相对路径表示形式，
    /// 以便后续的路径处理逻辑使用。
    /// 具体步骤如下：
    /// 1. 将路径中的 Windows 卷标（例如 "C:\"）移除。
    /// 2. 将路径中的 Mac 卷标（例如 "/Volumes/"）移除。
    /// 3. 使用 <see cref="Path.GetDirectoryName(string)"/> 从处理后的路径中提取上级目录路径。
    /// </remarks>
    /// <example>
    /// 假设输入路径分别为：
    /// <list type="bullet">
    /// <item>Windows: "C:\Root\Dir1\File.txt"</item>
    /// <item>Mac: "/Volumes/Data/Root/Dir1/File.txt"</item>
    /// </list>
    /// 方法返回值为：
    /// <list type="bullet">
    /// <item>Windows: "\Root\Dir1"</item>
    /// <item>Mac: "Data/Root/Dir1"</item>
    /// </list>
    /// </example>
    private string GetRelativeDir(string path)
    {
        path = path.Replace(AppSettingsUtils.Default.Windows.Volume, string.Empty);
        path = path.Replace(AppSettingsUtils.Default.Mac.Volume, string.Empty); 
        path = Path.GetDirectoryName(path);
        return path;
    }

    /// <summary>
    /// 打开指定目录的方法。
    /// 根据输入目录路径尝试进行路径调整，并在不同平台上调用对应的文件管理器（例如 Windows 的资源管理器）
    /// 打开目录界面，便于用户浏览目录内容。
    /// </summary>
    /// <param name="dir">需要打开的目录路径。可以是目录路径或文件路径。</param>
    /// <remarks>
    /// 此方法将执行以下步骤：
    /// 1. 调用 <c>AdjustPath(string)</c> 方法对提供的路径做标准化处理，确保路径有效。
    /// 2. 如果路径指向一个文件，将自动获取文件所在的目录。
    /// 3. 根据系统平台（通过 <see cref="AppSettingsUtils.Default.OS"/> 判断），
    /// 在 Windows 上通过调用 `explorer.exe`，在其他平台（例如 macOS）调用对应的目录打开命令。
    /// 4. 在发生异常情况下，会记录错误日志，并向用户展示错误通知。
    /// </remarks>
    /// <example>
    /// <list type="number">
    /// <item>假设输入路径为 "C:\Root\Dir1\File1.txt"（文件路径），调用后将打开 "C:\Root\Dir1"。</item>
    /// <item>假设输入路径为 "/User/Documents/Dir2"，调用后将在 macOS 上打开 Finder 并展示该目录。</item>
    /// </list>
    /// </example>
    private void OpenFolder(string dir)
    {
        try
        {
            var path = this.AdjustPath(dir);

            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            // Running on Windows
            if (AppSettingsUtils.Default.OS == OS.Windows)
            {
                Process.Start("explorer.exe", path);
            }
            else
            {
                Process.Start("open", path);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            this.Notification($"{ex}");
        }
    }

    #region 视频处理

    /// <summary>
    /// 无损压缩图片并保存为PNG格式
    /// </summary>
    /// <param name="inputPath">输入图片的路径</param>
    private void CompressAsPng(string inputPath)
    {
        if (!File.Exists(inputPath))
            return;

        // 加载原始图片
        // Load the original image
        using (var image = Image.Load(inputPath))
        {
            // 设置PNG编码器选项
            // Set PNG encoder options
            var options = new JpegEncoder()
            {
                Quality = 90
            };

            if (File.Exists(inputPath))
                File.Delete(inputPath);

            // 保存为PNG格式
            // Save as PNG format
            var stream = File.OpenWrite(inputPath);
            image.Save(stream, options);
            stream.Close();
            stream.Dispose();
            image.Dispose();
        }
    }

    /// <summary>
    /// 判断图像是否全黑
    /// </summary>
    /// <param name="imageFile">要检查的图像文件</param>
    /// <returns>如果图像全黑则返回true，否则返回false</returns>
    private bool IsImageBlack(string imageFile)
    {
        var image = new Mat(imageFile);

        // 获取图像的宽度和高度
        int width = image.Width;
        int height = image.Height;

        // 将图像转换为灰度图像
        Mat grayImage = new Mat();
        CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

        // 遍历每一个像素点
        Image<Gray, byte> img = grayImage.ToImage<Gray, byte>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Gray pixel = img[y, x];
                if (pixel.Intensity > 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 异步解析视频文件并返回其长度。
    /// </summary>
    /// <param name="libVLC">LibVLC库的实例。</param>
    /// <param name="item">代表视频文件的对象。</param>
    /// <returns>
    /// 返回一个任务，该任务表示异步操作，任务的结果是视频文件的长度。
    /// </returns>
    /// <remarks>
    /// 此方法首先创建一个 'Media' 对象，并以异步方式解析这个媒体。解析完成后，它会停止解析并获取媒体的长度。
    /// </remarks> 
    private async Task<long> ParseMediaAsync(LibVLC libVLC, FileInfo item)
    {
        var media = new Media(libVLC, item.FullName, FromType.FromPath);
        await media.Parse(MediaParseOptions.ParseNetwork);
        media.ParseStop();
        var length = media.Duration;
        return length;
    }

    /// <summary>
    /// 异步方法，用于计算指定文件的 MD5 哈希值。
    /// 该方法读取文件内容并生成其对应的 MD5 哈希，以字符串形式返回。
    /// </summary>
    /// <param name="filePath">目标文件的完整路径。</param>
    /// <returns>
    /// 返回文件的 MD5 哈希值，以小写字母形式的字符串表示。
    /// 如果文件路径无效或文件读取失败，可能会抛出相应的异常。
    /// </returns>
    /// <remarks>
    /// 此方法通过流式读取文件内容，利用 <see cref="System.Security.Cryptography.MD5"/>
    /// 对象计算文件的 MD5 哈希，适用于检查文件的完整性或进行文件比对等场景。
    /// </remarks>
    /// <example>
    /// 示例代码：
    /// <code>
    /// string filePath = @"C:\example.txt";
    /// string md5Hash = await GetMd5CodeAsync(filePath);
    /// Console.WriteLine($"文件的 MD5 值为: {md5Hash}");
    /// </code>
    /// 假设文件内容为 "Hello World"，则输出结果类似于：
    /// 文件的 MD5 值为: b10a8db164e0754105b7a99be72e3fe5
    /// </example>
    private async Task<string> GetMd5CodeAsync(string filePath)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = await Task.Run(() => md5.ComputeHash(stream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    private async Task ProcessVideoAsync(Video enty, CancellationToken cancellationToken)
    {
        var picCount = 10;
        using var libVLC = new LibVLC();
        using var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC);
        var isNew = this.dataService.VideosService.FirstAsync(m => m.Id == enty.Id) == null;
        Log.Information($"Video {enty.Caption} is {(isNew ? "New" : "already exists")}.");

        try
        {
            var item = new FileInfo(this.AdjustPath(enty.VideoPath)); // 视频文件
            var times = new List<long>(); // 截图时间点
            var images = new List<Snapshot>(); // 截图文件
            var length = await this.ParseMediaAsync(libVLC, item);

            Log.Information($"Video {enty.Caption} parse times {length / 1000} s.");

            // 使用cancellationToken.ThrowIfCancellationRequested来检查取消请求
            cancellationToken.ThrowIfCancellationRequested();

            var media = new Media(libVLC, item.FullName, FromType.FromPath); // 视频文件
            var interval = length / picCount; // 截图时间间隔 
            mediaPlayer.Media = media; // 设置视频文件
            mediaPlayer.EncounteredError += (s, e) => { Log.Error($"Error: {e}"); };

            for (int i = 0; i < picCount; i++)
                times.Add(interval * i); // 添加播放时间  

            times.RemoveAt(0); // 移除第一个时间点
            times.RemoveAt(times.Count - 1); // 移除最后一个时间点

            mediaPlayer.Play();
            mediaPlayer.ToggleMute(); // 静音
            await Task.Delay(300);
            Log.Information($"Video {enty.Caption} start paly.");
            // 使用cancellationToken.ThrowIfCancellationRequested来检查取消请求
            cancellationToken.ThrowIfCancellationRequested();

            while (mediaPlayer.State != VLCState.Playing)
            {
                await Task.Delay(500);
                cancellationToken.ThrowIfCancellationRequested();
            }

            enty.Caption = Path.GetFileNameWithoutExtension(enty.VideoPath); // 视频标题
            enty.Length = item.Length / 1024 / 1024; // 视频大小
            enty.ModifyTime = item.LastWriteTime; // 修改时间
            enty.VideoDir = this.SelectedDir.ValidName;
            enty.Dir = this.GetRelativeDir(enty.VideoPath); 

            foreach (var time in times)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var picName = $"{Guid.NewGuid()}.jpg";
                var snapshot = Path.Combine(AppSettingsUtils.Default.Current.DataDir, picName);
                mediaPlayer.Time = time; // 设置播放时间
                await Task.Delay(300); // 等待截图完成
                mediaPlayer.TakeSnapshot(0, snapshot, 0, 0); // 截图
                await Task.Delay(300);
                this.CompressAsPng(snapshot);

                if (File.Exists(snapshot))
                {
                    var snap = this.dataService.SnapshotsService.Create(snapshot, enty.Id);
                    images.Add(snap);
                }
            }

            Log.Information($"Video {enty.Caption} get images completed.");
            this.DelOriginalPictures(enty);
            enty.Snapshots = images; // 截图文件 

            Log.Information($"Video {enty.Caption} delete images completed.");
            //enty.MD5 = await this.GetMd5CodeAsync(enty.VideoPath); // MD5
            cancellationToken.ThrowIfCancellationRequested();

            if (isNew)
                await this.dataService.VideosService.AddAsync(enty); // 添加视频实体
            else
                await this.dataService.VideosService.UpdateAsync(enty); // 更新视频实体

            Log.Information($"Video {enty.Caption} {(isNew ? "add" : "update")} completed.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error: {enty.VideoPath}{Environment.NewLine}{ex}");
        }
        finally
        {
            mediaPlayer.Stop();
            mediaPlayer.Dispose();
        }
    }

    /// <summary>
    /// 删除视频图片
    /// </summary>
    /// <param name="enty">视频实体</param>
    private void DelOriginalPictures(Video enty)
    {
        var imgs = enty.Snapshots.ToList();
        enty.Snapshots.Clear();
        foreach (var item in imgs)
        {
            try
            {
                File.Delete(item.Path);
            }
            catch (Exception ex)
            {
                Log.Error($"File Del Error:{item}{Environment.NewLine}{ex}");
            }
        }
    }
    
    #endregion
}