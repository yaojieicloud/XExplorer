using XExplorer.Core.Context;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Service;

/// <summary>
/// The DataService class is responsible for managing the data operations
/// within the application. It provides methods to interact with the data
/// layer, allowing for the retrieval, modification, and processing of data
/// used by the application.
/// </summary>
public partial class DataService
{
    /// <summary>
    /// 数据上下文
    /// </summary>
    private SQLiteContext dataContext;

    /// <summary>
    /// 密码服务，用于管理应用中的密码操作。提供添加密码、新增密码或从数据库检索所有密码的功能。
    /// </summary>
    public PasswordService PwdService { get; private set; }

    /// <summary>
    /// 视频服务，用于管理与视频相关的数据操作，包括查询、更新及统计播放计数等功能。
    /// </summary>
    public VideoService VideosService { get; private set; }

    /// <summary>
    /// 用于管理快照服务的属性。
    /// 提供对 <see cref="SnapshotService"/> 的访问，以便应用程序能够执行快照相关的操作，
    /// 例如添加、删除以及检索快照数据。
    /// </summary>
    public SnapshotService SnapshotsService { get; private set; }

    /// <summary>
    /// 提供数据处理相关服务的核心类
    /// </summary>
    /// <remarks>
    /// 该类包含对数据库上下文的管理，并初始化与其相关的服务，例如密码服务和视频服务。
    /// </remarks>
    public DataService()
    {
        var dbfile = this.GetSqlitePath();
        this.dataContext = new SQLiteContext(dbfile.FullName);
        this.VideosService = new(this.dataContext);
        this.PwdService = new(this.dataContext);
        this.SnapshotsService = new(this.dataContext);
    }

    /// <summary>
    /// 获取数据目录路径
    /// </summary>
    /// <returns>包含目录路径、文件路径和名称的元组</returns>
    private DBFile GetSqlitePath()
    {
        var dbfile = new DBFile();
        dbfile.Name = Path.GetFileName(AppSettingsUtils.Default.Current.DBPath);
        dbfile.FullName = AppSettingsUtils.Default.Current.DBPath;
        dbfile.Dir = Path.GetDirectoryName(AppSettingsUtils.Default.Current.DBPath);
        return dbfile;
    }
}