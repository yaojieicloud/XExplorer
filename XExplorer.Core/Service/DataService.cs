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
    /// Gets the PasswordService instance used for managing password-related
    /// operations within the data service. This includes adding and retrieving
    /// passwords stored in the database.
    /// </summary>
    public PasswordService PwdService { get; private set; }

    /// <summary>
    /// Provides functionality for managing video-related operations within the application.
    /// This includes methods to add new video content, retrieve existing videos, and other
    /// video management tasks.
    /// </summary>
    public VideoService VideosService { get; private set; }

    /// <summary>
    /// The DataService class is responsible for managing the data operations
    /// within the application. It provides methods to interact with the data
    /// layer, allowing for the retrieval, modification, and processing of data
    /// used by the application.
    /// </summary>
    public DataService()
    {
        var dbfile = this.GetSqlitePath();
        this.dataContext = new SQLiteContext(dbfile.FullName);
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