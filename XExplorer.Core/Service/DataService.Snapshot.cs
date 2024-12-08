using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using XExplorer.Core.Context;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Service;

/// <summary>
/// The DataService class encompasses services that facilitate interaction with
/// various data-related operations within the application. It serves as a partial class,
/// suggesting composition with additional functionalities in separate implementations.
/// </summary>
partial class DataService
{
    /// <summary>
    /// The SnapshotService class provides methods for managing snapshots within
    /// the application. It leverages the database context to perform operations
    /// such as adding new passwords and retrieving all passwords asynchronously
    /// from the database.
    /// </summary>
    public partial class SnapshotService
    {
        /// <summary>
        /// 数据上下文
        /// </summary>
        private SQLiteContext dataContext;

        /// <summary>
        /// Provides functionality for managing passwords within the application.
        /// </summary>
        public SnapshotService(SQLiteContext context)
        {
            this.dataContext = context;
        }

        /// <summary>
        /// 从数据库中批量删除视频快照列表。
        /// </summary>
        /// <param name="snapshots">要从数据库中删除的视频快照对象列表。</param> 
        public async Task DelAsync(List<Snapshot> snapshots)
        {
            var delSnaps = await this.dataContext.Snapshots.Where(s => snapshots.Any(s1 => s1.Id == s.Id)).ToListAsync();

            if (delSnaps?.Any() ?? false)
            {
                this.dataContext.Snapshots.RemoveRange(delSnaps);
                await this.dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Asynchronously deletes video snapshots from the database based on the specified video ID.
        /// </summary>
        /// <param name="vid">The video ID whose associated snapshots are to be deleted from the database.</param>
        /// <returns>A task representing the asynchronous deletion operation.</returns>
        public async Task DelAsync(long vid)
        {
            var delSnaps = this.dataContext.Snapshots.Where(s => s.VideoId == vid);
            if (delSnaps?.Any() ?? false)
            {
                this.dataContext.Snapshots.RemoveRange(delSnaps);
                await this.dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of all passwords from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of passwords as its result.</returns>
        public async Task<List<Snapshot>> GetAsync() => await this.dataContext.Snapshots.ToListAsync();

        /// <summary>
        /// Asynchronously retrieves a list of all snapshots from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of all snapshot objects from the database as its result.</returns>
        public List<Snapshot> GetAsync(Func<Snapshot, bool> predicat) =>
            this.dataContext.Snapshots.Where(predicat).ToList();
    }
}