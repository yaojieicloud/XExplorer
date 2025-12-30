using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;
using XExplorer.Core.Utils;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace XExplorer.Core.ViewModel.Browser;

public partial class BrowserViewModel
{
    /// <summary>
    /// Gets or sets the collection of video download information items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VodeoDownloadInfo> vodeoDownloadInfos = new();

    /// <summary>
    /// 一个私有变量，用于存储目录路径的集合。该集合是
    /// <see cref="ObservableCollection{T}" /> 类型，支持在 UI 中动态更新和绑定。
    /// <see cref="dirs" /> 变量由 <see cref="MainViewModel" /> 管理，负责跟踪应用程序中需要显示的所有目录路径。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DirRecord> dirs = new();
     /// <summary>
    ///     一个私有变量，代表用户当前选择的目录路径。用于
    ///     记录和管理用户导航的文件夹，支持界面中目录
    ///     相关操作的实现。当目录加载时，
    ///     此变量会更新以反映用户的新选择。
    /// </summary>
    [ObservableProperty]
    private DirRecord selectedDir;



    /// <summary>
    /// Downloads a video from the specified URL and saves it to the given directory asynchronously.
    /// </summary> 
    /// <returns>A task that represents the asynchronous download operation.</returns>
    [RelayCommand]
    public async Task DownloadVideoAsync()
    {
        var ydl = new YoutubeDL();
        ydl.YoutubeDLPath = VideoUtils.YoutubeDLPath;
        ydl.FFmpegPath = VideoUtils.FFmpegPath;
        ydl.OutputFolder = this.SelectedDir.FullName;
        var fileName = string.Empty;
        var url = this.WebUrl;
        var dir = this.SelectedDir.FullName;

        // 1. 获取视频元数据
        var res = await ydl.RunVideoDataFetch(this.WebUrl);

        if (res.Success)
            fileName = res.Data.Title + "." + res.Data.Extension;

        var info = new VodeoDownloadInfo()
        {
            WebUrl = url,
            Dir = dir,
            FileName = fileName,
        };

        this.VodeoDownloadInfos.Add(info);

        // 配置下载选项
        var options = new OptionSet()
        {
            // 指定下载路径
            Output = Path.Combine(dir, "%(title)s.%(ext)s"),
            // 开启 10 线程加速（针对 HLS/m3u8 极其有效）
            ConcurrentFragments = 10,
            // 如果需要，可以指定浏览器 Cookie（例如 chrome, edge, firefox）
            // CookiesFromBrowser = Browser.Chrome 
        };

        // 进度回调
        var progress = new Progress<DownloadProgress>(p =>
        {
            info.Progress = (p.Progress * 100).ToString("F2") + "%";
            info.Speed = p.DownloadSpeed;
        });

        // var result = await ydl.RunVideoDownload(url, progress: progress, overrideOptions: options);
        info.State = "Downloading";
        info.Progress = "10%";
        var result = await Task.Run(async () => await ydl.RunVideoDownload(url, progress: progress, overrideOptions: options));

        if (result.Success)
        {
            info.State = "Completed";
            info.Progress = "100%";
        }
        else
        {
            info.State = "Failed";
            info.Progress = "0%";
            info.Msg = string.Join("\n", result.ErrorOutput);
        }
    }

    /// <summary>
    ///     初始化目录列表的方法。
    ///     此方法从应用程序配置的根目录中加载所有子目录，
    ///     并将其转换为包含名称、完整路径和有效名称的目录记录集合。
    /// </summary>
    /// <remarks>
    ///     本方法使用应用程序设定的根目录路径（RootDir）读取所有子目录，
    ///     并生成用于绑定显示的 ObservableCollection「DirRecord」对象，
    ///     以便供界面或后续逻辑使用。
    /// </remarks>
    private void InitDirs()
    {
        Processing = true;
        try
        {
            var videoDirs = new List<DirRecord>();
            var allDirs = Directory.GetDirectories(AppSettingsUtils.Default.Current.RootDir);
            var list = SortUtils.Sort(allDirs.ToList(), true);

            for (var i = 0; i < list.Count; i++)
            {
                var dir = list[i];
                var dirInfo = new DirectoryInfo(dir);
                var valid = this.GetValid(dir);
                videoDirs.Add(new DirRecord { Name = dirInfo.Name, FullName = dir, ValidName = valid, Sort = i + 1 });
            }

            Dirs = new ObservableCollection<DirRecord>(videoDirs);
            this.SelectedDir = Dirs.FirstOrDefault();
        }
        finally
        {
            Processing = false;
        }
    }

    /// <summary>
    /// 根据提供的路径生成有效标识符的方法。
    /// 此方法通过移除路径中与应用程序配置相关的不必要部分，
    /// 返回一个适合用于显示或处理的有效路径名称。
    /// </summary>
    /// <param name="path">
    /// 原始路径字符串，需要转换为有效名称的目标路径。
    /// </param>
    /// <returns>
    /// 返回一个处理后的字符串，此字符串是原始路径中去除了特定卷名信息后的有效名称。
    /// </returns>
    private string GetValid(string path)
    {
        var valid = path.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty);

        foreach (var invalid in AppSettingsUtils.Default.Current.Volumes)
            valid = valid.Replace(invalid, string.Empty);

        return valid;
    }
}

public partial class VodeoDownloadInfo() : ObservableObject
{
    /// <summary>
    /// Gets the web address associated with the resource.
    /// </summary>
    [ObservableProperty]
    private string webUrl;

    /// <summary>
    /// Gets the URL from which the resource can be downloaded.
    /// </summary>
    [ObservableProperty]
    private string dir;

    /// <summary>
    /// Gets the name of the file associated with this instance.
    /// </summary>
    [ObservableProperty]
    private string fileName;

    /// <summary>
    /// Gets or sets the current progress as a decimal value.
    /// </summary>
    [ObservableProperty]
    private string progress;

    /// <summary>
    /// Gets or sets the current state of the operation.
    /// </summary>
    [ObservableProperty]
    private string state = "Waiting";

    /// <summary>
    /// Gets or sets the message content associated with this instance.
    /// </summary>
    [ObservableProperty]
    private string msg;

    /// <summary>
    /// Gets or sets the speed value.
    /// </summary>
    [ObservableProperty]
    private string speed;
}