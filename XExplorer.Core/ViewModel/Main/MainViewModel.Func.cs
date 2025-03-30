using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Maui.ApplicationModel;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
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
    ///     初始化目录列表的方法。
    ///     此方法从应用程序配置的根目录中加载所有子目录，
    ///     并将其转换为包含名称、完整路径和有效名称的目录记录集合。
    /// </summary>
    /// <remarks>
    ///     本方法使用应用程序设定的根目录路径（RootDir）读取所有子目录，
    ///     并生成用于绑定显示的 ObservableCollection
    ///     <DirRecord>
    ///         对象，
    ///         以便供界面或后续逻辑使用。
    /// </remarks>
    private void InitDirs()
    {
        var videoDirs = new List<DirRecord>();
        ;
        var allDirs = Directory.GetDirectories(AppSettingsUtils.Default.Current.RootDir);
        for (var i = 0; i < allDirs.Length; i++)
        {
            var dir = allDirs[i];
            var dirInfo = new DirectoryInfo(dir);
            var valid = dir.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty);
            videoDirs.Add(new DirRecord { Name = dirInfo.Name, FullName = dir, ValidName = valid });
        }

        videoDirs = videoDirs.OrderByDescending(m => m.ValidName).ToList();
        videoDirs.Insert(0, new DirRecord() { Name = Screnn.All, FullName = Screnn.All, ValidName = Screnn.All });
        Dirs = new ObservableCollection<DirRecord>(videoDirs);
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
    /// 显示信息消息的方法。
    /// 此方法记录信息消息，并更新 UI 线程中的消息字段。
    /// </summary>
    /// <param name="message">需要显示或记录的消息内容。</param>
    /// <param name="title">可选的标题，用于覆盖主消息字段的显示值。</param>
    private void Information(string message, string title = null)
    {
        Log.Information(message);
        if (MainThread.IsMainThread)
            this.Message = title ?? message;
        else
            MainThread.BeginInvokeOnMainThread(() => this.Message = title ?? message);
    }

    #region 批处理视频

    /// <summary>
    /// 异步处理给定目录下符合条件的视频文件，并将处理结果保存到数据服务中。
    /// </summary>
    /// <param name="dir">待处理的目录记录对象，包含目录的名称和路径等信息。</param>
    /// <returns>返回一个异步任务，表示处理完成后的状态。</returns>
    /// <exception cref="FileNotFoundException">当目录内没有任何符合条件的文件时抛出此异常。</exception>
    public async Task WithVideosAsync(DirRecord dir)
    {
        var dirInfo = new DirectoryInfo(dir.FullName);
        var newVideos = new List<Video>();
        var files = dirInfo.GetFiles(string.Empty, SearchOption.AllDirectories);

        if (!(files?.Any() ?? false))
            throw new FileNotFoundException($"目录内没有任何文件: {dir.FullName}");

        // 筛选符合条件的视频文件
        var videoFiles = files.Where(f => videoExts.Contains(f.Extension.ToLower())).ToList();
        var videoStoreFiles = videoFiles?.Where(m => m.Length >= videoMiniSize).ToList() ?? new List<FileInfo>();

        // 查询已有的视频数据
        var videoData = await dataService.VideosService.QueryAsync(m => m.RootDir == dir.ValidName);
        var videoDict = videoData.ToDictionary(m => m.VideoPath, m => m);

        // 使用 Parallel.ForEachAsync 并发处理所有视频文件
        await Parallel.ForEachAsync(videoStoreFiles, async (fileInfo, cancellationToken) =>
        {
            try
            {
                var videoRecord = new FileRecord(fileInfo.FullName);

                // 判断是否已经存在
                if (videoDict.TryGetValue(videoRecord.ValidName, out var _))
                {
                    this.Information($"视频 [{fileInfo.FullName}] 已存在，无需处理。");
                    return;
                }

                this.Information($"视频 [{fileInfo.FullName}] 处理中。。。");

                // 处理视频并返回结果
                var video = await WithVideoAsync(dir, new FileRecord(fileInfo.FullName));

                // 使用锁保护对共享列表的访问
                lock (newVideos)
                {
                    if (!(newVideos?.Any(m => m.VideoPath == video.VideoPath) ?? false))
                        newVideos.Add(video);
                }

                this.Information($"视频数据 [{fileInfo.FullName}] 处理完成。");
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception e)
            {
                this.Information($"视频 [{fileInfo.FullName}] 处理失败: {e.Message}");
            }
        });

        if (newVideos?.Any() ?? false)
        {
            this.Information($"目录：{dir.FullName} 开始写入数据...");
            await this.dataService.VideosService.AddAsync(newVideos);
        }
        else
        {
            this.Information($"目录：{dir.FullName} 无需写入数据...");
        }

        this.Information($"目录：{dir.FullName} 写入数据完成，目录处理结束。");
    }

    /// <summary>
    /// 异步处理视频文件的方法。
    /// 该方法接收目录记录和文件记录对象，通过特定的逻辑处理视频文件，
    /// 并返回处理完成的 <see cref="Video"/> 对象。
    /// </summary>
    /// <param name="dir">包含视频文件所属目录信息的 <see cref="DirRecord"/> 对象。</param>
    /// <param name="record">表示待处理视频文件的 <see cref="FileRecord"/> 对象。</param>
    /// <returns>返回处理完成的 <see cref="Video"/> 对象。</returns>
    private async Task<Video> WithVideoAsync(DirRecord dir, FileRecord record, Video video = null)
    {
        var st = Stopwatch.StartNew();

        try
        {
            if (!File.Exists(record.FullName))
                return null;

            var file = new FileInfo(record.FullName);
            var dataDir = Path.Combine(AppSettingsUtils.Default.Current.DataDir, record.Dir.ValidName);
            var info = await GetVideoInfo(record.FullName);
            var timestamps = GetTimestamps(info.times);
            var images = await GetVideoImagesAsync(record.FullName, timestamps);

            video ??= dataService.VideosService.Create(record.ValidName);
            video.Status = 1;
            this.DelOriginalImages(video);
            video.VideoDir = record.Dir.ValidName;
            video.VideoPath = record.ValidName;
            video.RootDir = dir.ValidName;
            video.Caption = record.NameWithoutExt;
            video.Times = info.times;
            video.Length = file.Length / 1024 / 1024;
            video.DataDir = record.Dir.ValidName;
            video.Snapshots = images.Select(m => dataService.SnapshotsService.Create(m.Name, video.Id))
                .ToList();
            video.Width = info.width;
            video.Height = info.height;
            video.WideScrenn = info.widescreen;
            video.ModifyTime = DateTime.Now;
            return video;
        }
        finally
        {
            st.Stop();
            this.Information($"视频 [{record.FullName}] 解析完成，耗时 [{st.Elapsed.TotalSeconds}] 秒");
        }
    }

    /// <summary>
    /// 异步处理视频文件的 MD5 计算并更新视频数据的方法。
    /// 此方法查询现有视频数据，为每个视频文件计算 MD5 值，并将结果写入数据库。
    /// </summary>
    /// <remarks>
    /// 使用并行处理（Parallel.ForEachAsync）高效地计算视频文件的 MD5 值，同时在失败时提供详细的错误信息。
    /// 在所有处理完成后，调用数据服务接口更新视频数据。
    /// </remarks>
    /// <returns>返回一个表示异步操作的任务。</returns>
    private async Task WithMd5Async()
    {
        // 查询已有的视频数据
        var videoData = await dataService.VideosService.QueryOnlyAsync(m => string.IsNullOrWhiteSpace(m.MD5));

        // 使用 Parallel.ForEachAsync 并发处理所有视频文件
        await Parallel.ForEachAsync(videoData, async (fileInfo, cancellationToken) =>
        {
            try
            {
                this.Information($"视频 [{fileInfo.VideoPath}] MD5处理中。。。");
                fileInfo.MD5 =
                    await this.GetMd5CodeAsync(
                        Path.Combine(AppSettingsUtils.Default.Current.Volume, fileInfo.VideoPath));
                this.Information($"视频数据 [{fileInfo.VideoPath}] MD5处理完成。");
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception e)
            {
                this.Information($"视频 [{fileInfo.VideoPath}] 处理失败: {e.Message}");
            }
        });

        this.Information($"MD5处理开始写入数据...");
        await this.dataService.VideosService.UpdateOnlyAsync(videoData);
        this.Information($"MD5处理写入数据完成。");
    }

    #endregion

    #region 视频处理

    /// <summary>
    /// 异步批量处理指定目录及其子目录中的视频文件。
    /// 此方法遍历指定目录下的所有子目录，依次调用视频处理逻辑。
    /// </summary>
    /// <param name="dir">要处理的目录信息，包含目录路径及相关属性的 <see cref="DirRecord"/> 对象。</param>
    /// <returns>
    /// 返回一个表示异步操作的任务对象。
    /// 处理完成后，所有子目录的视频文件均完成批量处理。
    /// </returns>
    public async Task BatchProcessVideosAsync(DirRecord dir)
    {
        var subDirs = Directory.GetDirectories(dir.FullName);
        if (subDirs?.Any() ?? false)
        {
            foreach (var sub in subDirs)
            {
                this.Information($"目录 [{sub}] 开始处理。。。");
                var subRecord = new DirRecord(sub);
                await this.WithVideosAsync(subRecord);
                this.Information($"目录 [{sub}] 处理完成。");
            }
        }
    }

    /// <summary>
    /// 异步处理视频文件的方法。
    /// 此方法扫描指定目录中的视频文件，根据条件筛选后将其处理并写入数据库。
    /// </summary>
    /// <param name="dir">
    /// 表示目录的对象，包含目录的完整路径、名称和有效名称等信息。
    /// </param>
    /// <returns>
    /// 表示异步操作的任务对象。
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// 如果目录中未找到任何文件时，抛出此异常。
    /// </exception>
    public async Task ProcessVideosAsync(DirRecord dir)
    {
        var dirInfo = new DirectoryInfo(dir.FullName);
        var files = dirInfo.GetFiles(string.Empty, SearchOption.AllDirectories);
        if (!(files?.Any() ?? false))
            throw new FileNotFoundException(dir.FullName);

        var videoFiles = files.Where(f => videoExts.Contains(f.Extension.ToLower())).ToList();
        var videoStoreFiles = videoFiles?.Where(m => m.Length >= videoMiniSize).ToList() ?? new List<FileInfo>();

        var videoData = await dataService.VideosService.QueryAsync(m => m.VideoDir == dir.ValidName);
        var videoDict = videoData.ToDictionary(m => m.VideoPath, m => m);

        foreach (var fileInfo in videoStoreFiles)
            try
            {
                var videoRecord = new FileRecord(fileInfo.FullName);
                if (videoDict.ContainsKey(videoRecord.RelativePath))
                {
                    Log.Information($"视频 [{fileInfo.FullName}] 已存在，无需处理。");
                    Message = $"视频 [{fileInfo.FullName}] 已存在，无需处理。";
                    continue;
                }

                Message = $"视频 [{fileInfo.FullName}] 处理中。。。";
                var video = await ProcessVideoAsync(dir, new FileRecord(fileInfo.FullName));
                await this.dataService.VideosService.AddAsync(video);
                Log.Information($"视频数据 [{fileInfo.FullName}] 写入完成。");
                Message = $"视频数据 [{fileInfo.FullName}] 写入完成。";
            }
            catch (Exception e)
            {
                Log.Information($"视频 [{fileInfo.FullName}] 处理报错： {e}。");
                Message = $"视频 [{fileInfo.FullName}] 处理出错：{e.Message}";
            }
    }

    /// <summary>
    /// 异步处理视频文件的方法。
    /// 该方法接收目录记录和文件记录对象，通过特定的逻辑处理视频文件，
    /// 并返回处理完成的 <see cref="Video"/> 对象。
    /// </summary>
    /// <param name="dir">包含视频文件所属目录信息的 <see cref="DirRecord"/> 对象。</param>
    /// <param name="record">表示待处理视频文件的 <see cref="FileRecord"/> 对象。</param>
    /// <returns>返回处理完成的 <see cref="Video"/> 对象。</returns>
    private async Task<Video> ProcessVideoAsync(DirRecord dir, FileRecord record)
    {
        var st = Stopwatch.StartNew();

        try
        {
            if (!File.Exists(record.FullName))
                return null;

            var file = new FileInfo(record.FullName);
            var video = await dataService.VideosService.FirstAsync(m => m.VideoPath == record.ValidName);
            var dataDir = Path.Combine(AppSettingsUtils.Default.Current.DataDir, record.Dir.ValidName);
            var info = await GetVideoInfo(record.FullName);
            //var md5Task = GetMd5CodeAsync(record.FullName);
            var timestamps = GetTimestamps(info.times);
            var images = await GetVideoImagesAsync(record.FullName, timestamps);

            video ??= dataService.VideosService.Create(record.ValidName);
            video.Status = 1;
            this.DelOriginalImages(video);
            video.VideoDir = record.Dir.ValidName;
            video.VideoPath = record.ValidName;
            video.RootDir = dir.ValidName;
            video.Caption = record.NameWithoutExt;
            video.Times = info.times;
            video.Length = file.Length / 1024 / 1024;
            video.DataDir = record.Dir.ValidName;
            video.Snapshots = images.Select(m => dataService.SnapshotsService.Create(m.Name, video.Id))
                .ToList();
            //video.MD5 = await md5Task;
            video.Width = info.width;
            video.Height = info.height;
            video.WideScrenn = info.widescreen;
            video.ModifyTime = DateTime.Now;
            return video;
        }
        finally
        {
            st.Stop();
            Log.Information($"视频 [{record.FullName}] 处理完成，耗时 [{st.Elapsed.TotalSeconds}] 秒");
            Message = $"视频 [{record.FullName}] 处理完成，耗时 [{st.Elapsed.TotalSeconds}] 秒";
        }
    }

    /// <summary>
    ///     异步获取视频指定时间点的截图文件集合。
    ///     此方法通过提供的视频路径与多个时间戳点，截取对应的视频帧作为图片并保存到指定目录。
    /// </summary>
    /// <param name="videoPath">
    ///     视频文件的完整路径。
    /// </param>
    /// <param name="timestamps">
    ///     视频需要截取的多个时间点列表。
    /// </param>
    /// <returns>
    ///     返回包含所有保存的截图文件信息的集合。
    /// </returns>
    private async Task<List<FileRecord>> GetVideoImagesAsync(string videoPath, List<TimeSpan> timestamps)
    {
        var images = new List<FileRecord>();
        var outputFolderPath = AppSettingsUtils.Default.Current.SnapshotsDir;
        // 确保输出文件夹存在
        if (!Directory.Exists(outputFolderPath))
            Directory.CreateDirectory(outputFolderPath);

        // 遍历时间点并添加截图参数
        foreach (var timestamp in timestamps)
        {
            var name = $"{Guid.NewGuid():N}.png";
            var fullName = Path.Combine(outputFolderPath, name);
            var record = new FileRecord(fullName, AppSettingsUtils.Default.Current.DataDir);
            //conversion.AddParameter($"-ss {timestamp} -i \"{videoPath}\" -frames:v 1 \"{fullName}\"");
            await CaptureFrameAtTime(videoPath, fullName, timestamp);
            images.Add(record);
        }

        Log.Information($"视频 「{videoPath}」截图已保存到: [{outputFolderPath}] ");
        return images;
    }

    /// <summary>
    ///     从视频文件中指定时间戳截图并保存为图像文件。
    ///     此方法利用FFmpeg库提取视频帧并保存到指定输出目录。
    /// </summary>
    /// <param name="videoPath">视频文件的路径。</param>
    /// <param name="outputFolderPath">保存截图的输出文件夹路径。</param>
    /// <param name="timestamp">截图的时间戳位置。</param>
    /// <returns>一个表示异步操作的 Task。</returns>
    private async Task CaptureFrameAtTime(string videoPath, string outputFolderPath, TimeSpan timestamp)
    {
        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-ss {timestamp}")
            .AddParameter($"-i \"{videoPath}\"")
            .AddParameter("-frames:v 1")
            .AddParameter("-q:v 1") // 设置JPEG图片质量（数字越小质量越高）
            .AddParameter("-vf \"scale=iw:ih\"")
            .AddParameter($"\"{outputFolderPath}\"");
        await conversion.Start();
    }

    /// <summary>
    ///     根据总时长（单位为秒）计算出若干个关键时间点的时间戳。
    ///     本方法会将输入的时长分为 10 个时间段，提取中间 8 个时间点的时间戳（剔除第一个和最后一个时间点）。
    /// </summary>
    /// <param name="times">
    ///     视频的总时长（单位为秒）。该值必须大于 1，否则返回空集合。
    /// </param>
    /// <returns>
    ///     一个包含 8 个时间戳 (<see cref="TimeSpan" />) 的集合列表。这些时间戳为中间的
    ///     分段点时间值，依次从第 2 段到第 9 段的计算结果。
    ///     如果输入的时长不足以分为 10 个时间点，返回空集合。
    /// </returns>
    /// <remarks>
    ///     具体实现过程为：
    ///     1. 首先检查输入的时长。如果时长小于或等于 1，直接返回空集合。
    ///     2. 计算时间间隔，即将总时长除以 10，作为每段的距离。
    ///     3. 遍历第 2 到第 9 段，生成对应的时间戳；若因误差导致时间点超出总时长，则取总时长作为限制。
    ///     4. 将剔除的第一个和最后一个段时间点外的时间戳加入集合后返回。
    /// </remarks>
    private List<TimeSpan> GetTimestamps(long times)
    {
        // 创建一个列表来存储时间戳
        var timestamps = new List<TimeSpan>();
        if (times <= 1)
            return timestamps;

        // 计算时间间隔，确保分为10段
        var interval = (double)times / 10;
        for (var i = 1; i <= 10; i++)
        {
            var timeInSeconds = i == 10 ? times : i * interval;
            var timeStamp = TimeSpan.FromSeconds(timeInSeconds);
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
    /// 获取指定视频文件的基本信息（时长、分辨率、宽高比例等）。
    /// 此方法使用 FFmpeg 库解析视频文件并返回相关数据。
    /// </summary>
    /// <param name="videoPath">视频文件的完整路径。</param>
    /// <returns>
    /// 一个包含视频时长（秒）、宽度、高度以及是否为宽屏的元组数据；
    /// 如果文件不存在则返回默认值。
    /// </returns>
    private async Task<(long times, int width, int height, bool widescreen)> GetVideoInfo(string videoPath)
    {
        if (File.Exists(videoPath))
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var videoStream = mediaInfo.VideoStreams.First(); // 获取视频流 
            var width = videoStream.Width; // 获取视频宽度和高度
            var height = videoStream.Height; // 获取视频宽度和高度 
            var widescrenn = width > height; // 判断视频是宽屏还是窄屏  
            var duration = mediaInfo.Duration; // 获取时长
            Log.Information($"视频[{videoPath}]时长: {duration.TotalSeconds} 秒");
            return (Convert.ToInt64(duration.TotalSeconds), width, height, widescrenn);
        }

        return default;
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
    /// 异步获取文件的 MD5 校验码。
    /// 此方法读取指定文件，并计算其 MD5 哈希值，返回小写的十六进制字符串格式。
    /// </summary>
    /// <param name="filePath">文件的完整路径。</param>
    /// <returns>返回文件的 MD5 哈希值，格式为小写的十六进制字符串。</returns>
    private async Task<string> GetMd5CodeAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        });
    }

    /// <summary>
    /// 删除视频图片
    /// </summary>
    /// <param name="enty">视频实体</param>
    private void DelOriginalImages(Video enty)
    {
        var imgs = enty.Snapshots?.ToList();

        if (!(imgs?.Any() ?? false))
            return;

        for (var i = imgs.Count - 1; i >= 0; i--)
        {
            var fullPath = Path.Combine(AppSettingsUtils.Default.Current.DataDir, imgs[i]?.Path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            imgs.RemoveAt(i);
        }
    }

    /// <summary>
    /// 删除视频图片
    /// </summary>
    /// <param name="enty">视频实体</param>
    private async Task DelOriginalImagesAsync(Video enty)
    {
        var imgs = enty.Snapshots?.ToList();

        if (!(imgs?.Any() ?? false))
            return;

        await dataService.SnapshotsService.DelAsync(imgs);
        for (var i = imgs.Count - 1; i >= 0; i--)
        {
            var fullPath = Path.Combine(AppSettingsUtils.Default.Current.DataDir, imgs[i]?.Path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            imgs.RemoveAt(i);
        }
    }

    #endregion

}