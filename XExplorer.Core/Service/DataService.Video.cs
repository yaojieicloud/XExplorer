using System.Linq.Expressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using XExplorer.Core.Context;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Service;

/// <summary>
/// The DataService class acts as a central component for managing
/// various service-related operations. It is designed to facilitate interactions
/// with different datasets and contexts, including video and password management,
/// by leveraging underlying database contexts.
/// </summary>
partial class DataService
{
    /// <summary>
    /// The VideoService class provides functionality for managing passwords within
    /// the application. It includes methods to add new passwords asynchronously to
    /// the database and retrieve all existing passwords.
    /// </summary>
    public partial class VideoService
    {
        /// <summary>
        /// 数据上下文
        /// </summary>
        private SQLiteContext dataContext;

        /// <summary>
        /// Provides functionality for managing passwords within the application.
        /// </summary>
        public VideoService(SQLiteContext context)
        {
            this.dataContext = context;
        }

        /// <summary>
        /// 将单个视频对象添加到数据库中。
        /// </summary>
        /// <param name="video">要添加到数据库的视频对象。</param>
        /// <remarks>
        /// 此方法将单个视频对象添加到数据上下文中，并保存更改到数据库。
        /// 在使用此方法之前，请确保视频对象已经正确设置了所有必要的属性。
        /// </remarks>
        public async Task AddAsync(Video video)
        {
            if (!this.dataContext.Videos.Any(m => m.Id == video.Id))
            {
                this.dataContext.Videos.Add(video);
                await this.dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 将一系列视频对象添加到数据库中。
        /// </summary>
        /// <param name="videos">包含要添加到数据库的视频对象的列表。</param>
        /// <remarks>
        /// 此方法批量将视频对象添加到数据上下文中，并保存更改到数据库。
        /// 请确保传入的视频列表不为空，以避免不必要的数据库操作。
        /// </remarks>
        public async Task AddAsync(List<Video> videos)
        {
            var newVideos = videos.Where(v => !this.dataContext.Videos.Any(m => m.Id == v.Id)).ToList();
            if (newVideos?.Any() ?? false)
            {
                this.dataContext.Videos.AddRange(newVideos);
                await this.dataContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// 更新数据库中的视频对象。
        /// </summary>
        /// <param name="video">要更新的视频对象。</param>
        /// <remarks>
        /// 此方法用于更新数据上下文中的视频对象，并保存更改到数据库。
        /// 在调用此方法之前，请确保视频对象的 ID 对应于数据库中已存在的记录，并且所有需要更新的属性都已正确设置。
        /// </remarks>
        public async Task UpdateAsync(Video video)
        {
            var existingVideo = this.dataContext.Videos
                .Include(v => v.Snapshots)
                .FirstOrDefault(v => v.Id == video.Id);

            if (existingVideo != null)
            {
                this.dataContext.Entry(existingVideo).CurrentValues.SetValues(video);
                var delSnapshots = this.dataContext.Snapshots
                    .Where(s => s.VideoId == video.Id).ToList();

                for (int i = delSnapshots.Count - 1; i >= 0; i--)
                {
                    var snap = delSnapshots[i];
                    if (video.Snapshots.Any(m => m.Id == snap.Id))
                    {
                        var newSnap = video.Snapshots.FirstOrDefault(s => s.Id == snap.Id);
                        video.Snapshots.Remove(newSnap);
                        delSnapshots.Remove(snap);
                    }
                }

                if (delSnapshots?.Any() ?? false)
                    this.dataContext.Snapshots.RemoveRange(delSnapshots);

                if (video.Snapshots?.Any() ?? false)
                    this.dataContext.Snapshots.AddRange(video.Snapshots);

                await this.dataContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// 更新数据库中的视频对象。
        /// </summary>
        /// <param name="video">要更新的视频对象。</param>
        /// <remarks>
        /// 此方法用于更新数据上下文中的视频对象，并保存更改到数据库。
        /// 在调用此方法之前，请确保视频对象的 ID 对应于数据库中已存在的记录，并且所有需要更新的属性都已正确设置。
        /// </remarks>
        public async Task UpdateOnlyAsync(Video video)
        {
            var existingVideo = this.dataContext.Videos
                .Include(v => v.Snapshots)
                .FirstOrDefault(v => v.Id == video.Id);

            if (existingVideo != null)
            {
                this.dataContext.Entry(existingVideo).CurrentValues.SetValues(video);
                await this.dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 从数据库中删除指定的视频对象。
        /// </summary>
        /// <param name="video">要从数据库中删除的视频对象。</param>
        /// <remarks>
        /// 此方法从数据上下文中移除指定的视频对象，并保存更改到数据库。
        /// 在调用此方法之前，请确保视频对象已经存在于数据上下文中。
        /// 需要注意的是，如果视频对象在数据库中有关联的数据（如快照），则可能需要先删除或处理这些关联数据。
        /// </remarks>
        public async Task DeleteAsync(Video video) => await this.DeleteAsync(video.Id);

        /// <summary>
        /// 异步删除指定 ID 的实体。
        /// </summary>
        /// <param name="id">要删除的实体的唯一标识符。</param>
        /// <returns>无返回值的 Task，表示异步操作。</returns>
        /// <remarks>
        /// 如果找不到指定的实体，此方法可能会抛出异常或执行特定的错误处理逻辑，
        /// 具体取决于实现的细节。
        /// </remarks>
        public async Task DeleteAsync(long id)
        {
            var video = this.dataContext.Videos.FirstOrDefault(v => v.Id == id);
            var delSnapshots = this.dataContext.Snapshots.Where(s => s.VideoId == video.Id).ToList();
            if (video != null)
                this.dataContext.Videos.Remove(video);

            if (delSnapshots?.Any() ?? false)
                this.dataContext.Snapshots.RemoveRange(delSnapshots);

            await this.dataContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 从数据库中批量删除视频对象列表。
        /// </summary>
        /// <param name="videos">要从数据库中删除的视频对象列表。</param>
        /// <remarks>
        /// 此方法用于从数据上下文中批量移除视频对象，并保存更改到数据库。
        /// 请确保传入的视频列表中的每个视频对象都已经存在于数据上下文中。
        /// 如果列表中的某些视频对象在数据库中有关联的数据（如快照），则可能需要先处理这些关联数据。
        /// </remarks>
        public async Task DeleteVideosAsync(List<Video> videos)
        {
            this.dataContext.Videos.RemoveRange(videos);
            await this.dataContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 异步删除指定目录下的所有视频记录。
        /// </summary>
        /// <param name="dir">要删除的视频文件所在的目录。</param>
        /// <remarks>
        /// 此方法查询指定目录下的所有视频记录，并从数据库中删除这些记录。
        /// 它使用异步操作来确保数据库操作的效率。
        /// 在调用此方法之前，请确保提供的目录是有效的，并且考虑到删除操作是不可逆的。
        /// </remarks>
        public async Task DeleteDirAsync(string dir)
        {
            var delVideos = this.dataContext.Videos.Where(v => v.VideoDir == dir).ToList();
            var delSnapshots = this.dataContext.Snapshots.Where(s => delVideos.Any(v => v.Id == s.VideoId)).ToList();

            if (delVideos?.Any() ?? false)
                this.dataContext.Videos.RemoveRange(delVideos);

            if (delSnapshots?.Any() ?? false)
                this.dataContext.Snapshots.RemoveRange(delSnapshots);

            await this.dataContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 根据指定条件查询视频列表。
        /// </summary>
        /// <param name="dir">视频文件存储的目录，可为空。</param>
        /// <param name="caption">视频标题的部分内容，用于模糊匹配，可为空。</param>
        /// <param name="evaluate">视频评价的最低分数，可为空。</param>
        /// <param name="isDesc">指定返回结果是否按修改时间降序排列，默认为true。</param>
        /// <param name="skip">跳过结果集中的前N个视频，默认为0。</param>
        /// <param name="take">返回结果集中的视频数量，默认为10。</param> 
        /// <returns>根据条件筛选的视频对象列表。</returns>
        /// <remarks>
        /// 此方法允许通过目录、标题关键字和评价分数进行筛选，并支持分页和排序。
        /// </remarks>
        public async Task<List<Video>> QueryAsync(string? dir = null, string? caption = null, int? evaluate = null,
            bool isDesc = true, int skip = 0, int take = int.MaxValue, decimal status = 1)
        {
            var query = this.dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            query = query.Where(m => m.Status == status);

            if (!string.IsNullOrWhiteSpace(dir))
                query = query.Where(m => m.VideoDir.Replace(@"\","/").EndsWith(dir.Replace(@"\","/")));

            if (!string.IsNullOrWhiteSpace(caption))
                query = query.Where(m => m.Caption.Contains(caption));

            if (evaluate.HasValue)
                query = query.Where(m => m.Evaluate >= evaluate.Value);

            query = isDesc
                ? query.OrderByDescending(m => m.Evaluate).ThenByDescending(v => v.ModifyTime).ThenBy(m => m.Dir)
                : (IQueryable<Video>)query.OrderByDescending(m => m.Evaluate).ThenBy(v => v.ModifyTime).ThenBy(m => m.Dir);

            var sql = query.ToQueryString();
            
            return await query.Skip(skip).Take(take).ToListAsync();
        }

        /// <summary>
        /// 异步查询视频的方法，根据指定的谓词条件从数据库中检索视频列表。
        /// </summary>
        /// <param name="predicate">用于筛选视频的谓词表达式。</param>
        /// <returns>满足条件的视频列表。</returns>
        public async Task<List<Video>> QueryAsync(Expression<Func<Video, bool>> predicate)
        {
            var query = this.dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            return await query.Where(predicate).ToListAsync();
        }
        
        /// <summary>
        /// 异步查询视频的方法，根据指定的谓词条件从数据库中检索视频列表。
        /// </summary>
        /// <param name="predicate">用于筛选视频的谓词表达式。</param>
        /// <returns>满足条件的视频列表。</returns>
        public async Task<Video> FirstAsync(Expression<Func<Video, bool>> predicate)
        {
            var query = this.dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            return await query.Where(predicate).FirstOrDefaultAsync();
        }
    }
}