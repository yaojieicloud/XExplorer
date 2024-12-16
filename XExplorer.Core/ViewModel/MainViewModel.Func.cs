using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using Xabe.FFmpeg;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;

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
    ///     初始化目录信息的方法。
    ///     此方法会从配置的根目录中获取所有子目录，并将其信息转化为 <see cref="DirInfo" /> 对象集合，
    ///     最终赋值给当前 ViewModel 的 <c>Dirs</c> 属性。
    /// </summary>
    /// <remarks>
    ///     具体步骤如下：
    ///     1. 调用 <see cref="Directory.GetDirectories(string)" /> 方法获取根目录中的所有子目录路径。
    ///     2. 遍历每一个子目录路径，将其转换为 <see cref="DirInfo" /> 对象实例，包含目录的名称、完整路径和有效名称信息。
    ///     3. 将所有的 <see cref="DirInfo" /> 对象存入 <see cref="ObservableCollection{T}" /> 集合中，并更新 <c>Dirs</c> 属性。
    /// </remarks>
    /// <example>
    ///     假设配置的根目录包括如下子目录：
    ///     <list type="bullet">
    ///         <item>C:\Root\Dir1</item>
    ///         <item>C:\Root\Dir2</item>
    ///     </list>
    ///     调用本方法后，<c>Dirs</c> 属性会包含以下内容：
    ///     <list type="bullet">
    ///         <item>目录名称：Dir1，有效名称：\Dir1，完整路径：C:\Root\Dir1</item>
    ///         <item>目录名称：Dir2，有效名称：\Dir2，完整路径：C:\Root\Dir2</item>
    ///     </list>
    /// </example>
    private void InitDirs()
    {
        var videoDirs = new List<DirInfo>();
        var allDirs = Directory.GetDirectories(AppSettingsUtils.Default.Current.RootDir);
        for (var i = 0; i < allDirs.Length; i++)
        {
            var dir = allDirs[i];
            var dirInfo = new DirectoryInfo(dir);
            var valid = dir.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty);
            videoDirs.Add(new DirInfo { Name = dirInfo.Name, FullName = dir, ValidName = valid });
        }

        Dirs = new ObservableCollection<DirInfo>(videoDirs);
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
        path = path.Replace(AppSettingsUtils.Default.Windows.Volume, string.Empty);
        path = path.Replace(AppSettingsUtils.Default.Mac.Volume, string.Empty);
        path = Path.GetDirectoryName(path);
        return path;
    }

    /// <summary>
    ///     打开指定目录的方法。
    ///     根据输入目录路径尝试进行路径调整，并在不同平台上调用对应的文件管理器（例如 Windows 的资源管理器）
    ///     打开目录界面，便于用户浏览目录内容。
    /// </summary>
    /// <param name="dir">需要打开的目录路径。可以是目录路径或文件路径。</param>
    /// <remarks>
    ///     此方法将执行以下步骤：
    ///     1. 调用 <c>AdjustPath(string)</c> 方法对提供的路径做标准化处理，确保路径有效。
    ///     2. 如果路径指向一个文件，将自动获取文件所在的目录。
    ///     3. 根据系统平台（通过 <see cref="AppSettingsUtils.Default.OS" /> 判断），
    ///     在 Windows 上通过调用 `explorer.exe`，在其他平台（例如 macOS）调用对应的目录打开命令。
    ///     4. 在发生异常情况下，会记录错误日志，并向用户展示错误通知。
    /// </remarks>
    /// <example>
    ///     <list type="number">
    ///         <item>假设输入路径为 "C:\Root\Dir1\File1.txt"（文件路径），调用后将打开 "C:\Root\Dir1"。</item>
    ///         <item>假设输入路径为 "/User/Documents/Dir2"，调用后将在 macOS 上打开 Finder 并展示该目录。</item>
    ///     </list>
    /// </example>
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

    #region 视频处理

    /// <summary>
    /// 异步处理视频的方法。
    /// 此方法根据传入的视频路径生成一个 <see cref="Video" /> 对象，并返回该对象。
    /// </summary>
    /// <param name="path">
    /// 视频文件的路径，用于创建对应的 <see cref="Video" /> 对象。
    /// </param>
    /// <returns>
    /// 一个任务，表示异步操作的结果。任务完成时返回一个 <see cref="Video" /> 对象，
    /// 其中包含生成的视频信息（如标题、路径等）。
    /// </returns>
    /// <remarks>
    /// 具体流程如下：
    /// 1. 调用 <see cref="DataService.VideoService.Create(string)" /> 方法，根据提供的路径创建一个 <see cref="Video" /> 对象。
    /// 2. 通过 <c>await</c> 保证异步任务的执行流程。
    /// 3. 返回该 <see cref="Video" /> 对象作为结果。
    /// </remarks>
    /// <example>
    /// 假设输入路径为 "C:\Videos\Sample.mp4"，调用此方法后将生成一个具有以下信息的 <see cref="Video" /> 对象：
    /// <list type="bullet">
    /// <item>Caption：自动生成的标题（可能基于文件名）。</item>
    /// <item>Dir：文件所在的主目录。</item>
    /// <item>VideoDir：文件的完整路径。</item>
    /// </list>
    /// </example>
    private async Task<Video> ProcessVideoAsync(string path)
    {
        path = AdjustPath(path);

        if (!File.Exists(path))
            return null;

        var fileName = Path.GetFileName(path);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var file = new FileInfo(path);
        var dataDir = Path.Combine(AppSettingsUtils.Default.Current.DataDir, this.GetRelativeDir(path));
        var video = await this.dataService.VideosService.FirstAsync(m => m.VideoPath == path);
        var times = await this.GetVideoTimes(path);
        var md5Task = this.GetMd5CodeAsync(path);
        var timestamps = this.GetTimestamps(times);
        var images = await this.GetVideoImages(path, dataDir, timestamps);

        video ??= this.dataService.VideosService.Create(path);
        video.Status = 1;
        await this.DelOriginalImagesAsync(video);
        video.VideoDir = this.GetRelativeDir(path);
        video.VideoPath = this.GetRelativeDir(path);
        video.Dir = Path.GetDirectoryName(path)?.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty);
        video.Caption = fileNameWithoutExtension;
        video.Times = times;
        video.Length = file.Length / 1024 / 1024;
        video.DataDir = video.VideoPath;
        video.Snapshots = images.Select(m => this.dataService.SnapshotsService.Create(m, video.Id)).ToList();
        video.MD5 = await md5Task;
        video.ModifyTime = DateTime.Now;
        return video;
    }

    /// <summary>
    ///     从指定的视频文件中提取多个时间点的截图，并将其保存到指定的输出文件夹中。
    ///     此方法通过 <see cref="Xabe.FFmpeg" /> 库实现视频文件的处理和截图功能。
    /// </summary>
    /// <param name="videoPath">待处理的视频文件的路径。</param>
    /// <param name="outputFolderPath">保存截图的目标文件夹路径。如果文件夹不存在，将自动创建。</param>
    /// <param name="timestamps">一个时间点集合，表示在视频中需截取画面的时间点。</param>
    /// <remarks>
    ///     具体步骤如下：
    ///     1. 调用 <see cref="Directory.CreateDirectory(string)" /> 确保输出文件夹已存在。
    ///     2. 创建一个新的 FFmpeg 转换实例，通过 <see cref="Xabe.FFmpeg.FFmpeg.Conversions.New()" /> 方法完成。
    ///     3. 遍历传入的时间点集合，为每个时间点构建截图参数，并将其添加到转换实例中。
    ///     4. 调用 <c>conversion.Start()</c> 执行转换任务，生成截图文件。
    ///     5. 通过 <see cref="Serilog.Log.Information(string)" /> 记录操作日志以标识任务完成。
    /// </remarks>
    /// <returns>异步任务 (<see cref="Task" />)，用于表示截取过程的完成状态。</returns>
    private async Task<List<string>> GetVideoImages(string videoPath, string outputFolderPath,
        List<TimeSpan> timestamps)
    {
        var images = new List<string>();

        // 确保输出文件夹存在
        if (!Directory.Exists(outputFolderPath))
            Directory.CreateDirectory(outputFolderPath);

        // 创建一个 FFmpeg 转换实例
        var conversion = FFmpeg.Conversions.New();

        // 遍历时间点并添加截图参数
        foreach (var timestamp in timestamps)
        {
            var img = $"{Guid.NewGuid():N}.jpg";
            var outputPath = Path.Combine(outputFolderPath, img);
            conversion.AddParameter($"-ss {timestamp} -i \"{videoPath}\" -frames:v 1 \"{outputPath}\"");
            this.CompressAsPng(outputPath);
        }

        // 执行转换任务
        await conversion.Start();

        Log.Information($"视频 「{videoPath}」截图已保存到: [{outputFolderPath}] ");
        return images;
    }

    /// <summary>
    /// 根据总时长（单位为秒）计算出若干个关键时间点的时间戳。
    /// 本方法会将输入的时长分为 10 个时间段，提取中间 8 个时间点的时间戳（剔除第一个和最后一个时间点）。
    /// </summary>
    /// <param name="times">
    /// 视频的总时长（单位为秒）。该值必须大于 1，否则返回空集合。
    /// </param>
    /// <returns>
    /// 一个包含 8 个时间戳 (<see cref="TimeSpan" />) 的集合列表。这些时间戳为中间的
    /// 分段点时间值，依次从第 2 段到第 9 段的计算结果。
    /// 如果输入的时长不足以分为 10 个时间点，返回空集合。
    /// </returns>
    /// <remarks>
    /// 具体实现过程为：
    /// 1. 首先检查输入的时长。如果时长小于或等于 1，直接返回空集合。
    /// 2. 计算时间间隔，即将总时长除以 10，作为每段的距离。
    /// 3. 遍历第 2 到第 9 段，生成对应的时间戳；若因误差导致时间点超出总时长，则取总时长作为限制。
    /// 4. 将剔除的第一个和最后一个段时间点外的时间戳加入集合后返回。
    /// </remarks>
    private List<TimeSpan> GetTimestamps(long times)
    {
        // 创建一个列表来存储时间戳
        var timestamps = new List<TimeSpan>();
        if (times <= 1)
            return timestamps;

        // 计算时间间隔，确保分为10段
        double interval = (double)times / 10;
        for (int i = 1; i <= 10; i++)
        {
            double timeInSeconds = i == 10 ? times : i * interval;
            TimeSpan timeStamp = TimeSpan.FromSeconds(timeInSeconds);
            if (!timestamps.Contains(timeStamp))
                timestamps.Add(timeStamp);
        }

        // 删除第一个时间点
        if (timestamps.Count > 1)
            timestamps.RemoveAt(0);

        // 删除最后一个时间点
        if (timestamps.Count > 1)
            timestamps.RemoveAt(timestamps.Count - 1);

        return timestamps;
    }

    /// <summary>
    ///     异步获取视频文件的时长（以分钟为单位）。
    ///     此方法使用指定的文件路径检查视频文件的存在性，
    ///     并调用 <see cref="FFmpeg.GetMediaInfo(string)" /> 获得视频的媒体信息以提取时长。
    ///     如果文件不存在，则返回 <c>double.NaN</c>。
    /// </summary>
    /// <param name="videoPath">视频文件的完整路径。</param>
    /// <returns>以分钟为单位的视频时长。如果文件不存在，则返回 <c>double.NaN</c>。</returns>
    /// <remarks>
    ///     此功能依赖于 FFmpeg 库及其 <see cref="FFmpeg.GetMediaInfo(string)" /> 方法，
    ///     请确保视频路径指向有效且存在的媒体文件。
    /// </remarks>
    /// <example>
    ///     假设提供的视频文件路径为 <c>C:\Videos\Sample.mp4</c> 且文件有效：
    ///     <code>
    /// double length = await GetVideoLength(@"C:\Videos\Sample.mp4");
    /// Console.WriteLine($"视频时长: {length} 秒");
    /// </code>
    ///     如果文件不存在：
    ///     <code>
    /// double length = await GetVideoLength(@"C:\Videos\NonExistent.mp4");
    /// Console.WriteLine($"返回值为 {length}，表示文件无效");
    /// </code>
    /// </example>
    private async Task<long> GetVideoTimes(string videoPath)
    {
        if (File.Exists(videoPath))
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

            // 获取时长
            var duration = mediaInfo.Duration;
            Log.Information($"视频[{videoPath}]时长: {duration.TotalSeconds} 秒");
            return Convert.ToInt64(duration.TotalSeconds);
        }

        return 0l;
    }

    /// <summary>
    ///     无损压缩图片并保存为PNG格式
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
            var options = new JpegEncoder
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
    ///     判断图像是否全黑
    /// </summary>
    /// <param name="imageFile">要检查的图像文件</param>
    /// <returns>如果图像全黑则返回true，否则返回false</returns>
    private bool IsImageBlack(string imageFile)
    {
        var image = new Mat(imageFile);

        // 获取图像的宽度和高度
        var width = image.Width;
        var height = image.Height;

        // 将图像转换为灰度图像
        var grayImage = new Mat();
        CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

        // 遍历每一个像素点
        var img = grayImage.ToImage<Gray, byte>();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var pixel = img[y, x];
            if (pixel.Intensity > 0) return false;
        }

        return true;
    }

    /// <summary>
    ///     异步方法，用于计算指定文件的 MD5 哈希值。
    ///     该方法读取文件内容并生成其对应的 MD5 哈希，以字符串形式返回。
    /// </summary>
    /// <param name="filePath">目标文件的完整路径。</param>
    /// <returns>
    ///     返回文件的 MD5 哈希值，以小写字母形式的字符串表示。
    ///     如果文件路径无效或文件读取失败，可能会抛出相应的异常。
    /// </returns>
    /// <remarks>
    ///     此方法通过流式读取文件内容，利用 <see cref="System.Security.Cryptography.MD5" />
    ///     对象计算文件的 MD5 哈希，适用于检查文件的完整性或进行文件比对等场景。
    /// </remarks>
    /// <example>
    ///     示例代码：
    ///     <code>
    /// string filePath = @"C:\example.txt";
    /// string md5Hash = await GetMd5CodeAsync(filePath);
    /// Console.WriteLine($"文件的 MD5 值为: {md5Hash}");
    /// </code>
    ///     假设文件内容为 "Hello World"，则输出结果类似于：
    ///     文件的 MD5 值为: b10a8db164e0754105b7a99be72e3fe5
    /// </example>
    private async Task<string> GetMd5CodeAsync(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = await Task.Run(() => md5.ComputeHash(stream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    /// <summary>
    ///     删除视频图片
    /// </summary>
    /// <param name="enty">视频实体</param>
    private async Task DelOriginalImagesAsync(Video enty)
    {
        var imgs = enty.Snapshots?.ToList();

        if (!(imgs?.Any() ?? false))
            return;

        await this.dataService.SnapshotsService.DelAsync(imgs);
        for (int i = imgs.Count - 1; i >= 0; i--)
        {
            var fullPath = Path.Combine(AppSettingsUtils.Default.Current.DataDir, imgs[i]?.Path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            imgs.RemoveAt(i);
        }
    }

    #endregion
}