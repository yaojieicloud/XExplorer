using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using XExplorer.Core.Context;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Service;

/// <summary>
///     The DataService class acts as a central component for managing
///     various service-related operations. It is designed to facilitate interactions
///     with different datasets and contexts, including video and password management,
///     by leveraging underlying database contexts.
/// </summary>
partial class DataService
{
    /// <summary>
    ///     The VideoService class provides functionality for managing passwords within
    ///     the application. It includes methods to add new passwords asynchronously to
    ///     the database and retrieve all existing passwords.
    /// </summary>
    public class VideoService
    {
        /// <summary>
        ///     数据上下文
        /// </summary>
        private readonly SQLiteContext dataContext;

        /// <summary>
        ///     Provides functionality for managing passwords within the application.
        /// </summary>
        public VideoService(SQLiteContext context)
        {
            dataContext = context;
        }

        /// <summary>
        ///     创建一个新的 <see cref="Video" /> 实例。
        /// </summary>
        /// <param name="path">视频文件的路径。</param>
        /// <returns>包含指定路径的 <see cref="Video" /> 对象。</returns>
        public Video Create(string path)
        {
            return dataContext.CreateVideo(path);
        }

        /// <summary>
        ///     将单个视频对象添加到数据库中。
        /// </summary>
        /// <param name="video">要添加到数据库的视频对象。</param>
        /// <remarks>
        ///     此方法将单个视频对象添加到数据上下文中，并保存更改到数据库。
        ///     在使用此方法之前，请确保视频对象已经正确设置了所有必要的属性。
        /// </remarks>
        public async Task AddAsync(Video video)
        {
            if (!dataContext.Videos.Any(m => m.Id == video.Id))
            {
                dataContext.Videos.Add(video);
                await dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        ///     将一系列视频对象添加到数据库中。
        /// </summary>
        /// <param name="videos">包含要添加到数据库的视频对象的列表。</param>
        /// <remarks>
        ///     此方法批量将视频对象添加到数据上下文中，并保存更改到数据库。
        ///     请确保传入的视频列表不为空，以避免不必要的数据库操作。
        /// </remarks>
        public async Task AddAsync(List<Video> videos)
        {
            var newVideos = videos.Where(v => !dataContext.Videos.Any(m => m.Id == v.Id)).ToList();
            if (newVideos?.Any() ?? false)
            {
                dataContext.Videos.AddRange(newVideos);
                await dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        ///     更新数据库中的视频对象。
        /// </summary>
        /// <param name="video">要更新的视频对象。</param>
        /// <remarks>
        ///     此方法用于更新数据上下文中的视频对象，并保存更改到数据库。
        ///     在调用此方法之前，请确保视频对象的 ID 对应于数据库中已存在的记录，并且所有需要更新的属性都已正确设置。
        /// </remarks>
        public async Task UpdateAsync(Video video)
        {
            var existingVideo = dataContext.Videos
                .Include(v => v.Snapshots)
                .FirstOrDefault(v => v.Id == video.Id);

            if (existingVideo != null)
            {
                var entry = dataContext.Entry(existingVideo);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(video);
                var delSnapshots = dataContext.Snapshots
                    .Where(s => s.VideoId == video.Id).ToList();

                for (var i = delSnapshots.Count - 1; i >= 0; i--)
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
                    dataContext.Snapshots.RemoveRange(delSnapshots);

                if (video.Snapshots?.Any() ?? false)
                    dataContext.Snapshots.AddRange(video.Snapshots);

                await dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        ///     更新数据库中的视频对象。
        /// </summary>
        /// <param name="video">要更新的视频对象。</param>
        /// <remarks>
        ///     此方法用于更新数据上下文中的视频对象，并保存更改到数据库。
        ///     在调用此方法之前，请确保视频对象的 ID 对应于数据库中已存在的记录，并且所有需要更新的属性都已正确设置。
        /// </remarks>
        public async Task UpdateOnlyAsync(Video video)
        {
            var existingVideo = dataContext.Videos
                .FirstOrDefault(v => v.Id == video.Id);

            if (existingVideo != null)
            {
                var entry = dataContext.Entry(existingVideo);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(video);
                await dataContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 异步更新指定视频的部分字段。
        /// </summary>
        /// <param name="video">待更新的单个视频对象。</param>
        /// <returns>表示异步操作的 <see cref="Task"/>。</returns>
        public async Task UpdateOnlyAsync(List<Video> videos)
        {
            foreach (var video in videos)
            {
                var entry = dataContext.Entry(video);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(video);
            }

            await dataContext.SaveChangesAsync();
        }

        /// <summary>
        ///     从数据库中删除指定的视频对象。
        /// </summary>
        /// <param name="video">要从数据库中删除的视频对象。</param>
        /// <remarks>
        ///     此方法从数据上下文中移除指定的视频对象，并保存更改到数据库。
        ///     在调用此方法之前，请确保视频对象已经存在于数据上下文中。
        ///     需要注意的是，如果视频对象在数据库中有关联的数据（如快照），则可能需要先删除或处理这些关联数据。
        /// </remarks>
        public async Task DeleteAsync(Video video)
        {
            await DeleteAsync(video.Id);
        }

        /// <summary>
        ///     异步删除指定 ID 的实体。
        /// </summary>
        /// <param name="id">要删除的实体的唯一标识符。</param>
        /// <returns>无返回值的 Task，表示异步操作。</returns>
        /// <remarks>
        ///     如果找不到指定的实体，此方法可能会抛出异常或执行特定的错误处理逻辑，
        ///     具体取决于实现的细节。
        /// </remarks>
        public async Task DeleteAsync(long id)
        {
            var video = dataContext.Videos.FirstOrDefault(v => v.Id == id);
            var delSnapshots = dataContext.Snapshots.Where(s => s.VideoId == video.Id).ToList();
            if (video != null)
                dataContext.Videos.Remove(video);

            if (delSnapshots?.Any() ?? false)
                dataContext.Snapshots.RemoveRange(delSnapshots);

            await dataContext.SaveChangesAsync();
        }

        /// <summary>
        ///     从数据库中批量删除视频对象列表。
        /// </summary>
        /// <param name="videos">要从数据库中删除的视频对象列表。</param>
        /// <remarks>
        ///     此方法用于从数据上下文中批量移除视频对象，并保存更改到数据库。
        ///     请确保传入的视频列表中的每个视频对象都已经存在于数据上下文中。
        ///     如果列表中的某些视频对象在数据库中有关联的数据（如快照），则可能需要先处理这些关联数据。
        /// </remarks>
        public async Task DeleteVideosAsync(List<Video> videos)
        {
            dataContext.Videos.RemoveRange(videos);
            await dataContext.SaveChangesAsync();
        }

        /// <summary>
        ///     异步删除指定目录下的所有视频记录。
        /// </summary>
        /// <param name="dir">要删除的视频文件所在的目录。</param>
        /// <remarks>
        ///     此方法查询指定目录下的所有视频记录，并从数据库中删除这些记录。
        ///     它使用异步操作来确保数据库操作的效率。
        ///     在调用此方法之前，请确保提供的目录是有效的，并且考虑到删除操作是不可逆的。
        /// </remarks>
        public async Task DeleteDirAsync(string dir)
        {
            var delVideos = dataContext.Videos.Where(v => v.VideoDir == dir).ToList();
            var delSnapshots = dataContext.Snapshots.Where(s => delVideos.Any(v => v.Id == s.VideoId)).ToList();

            if (delVideos?.Any() ?? false)
                dataContext.Videos.RemoveRange(delVideos);

            if (delSnapshots?.Any() ?? false)
                dataContext.Snapshots.RemoveRange(delSnapshots);

            await dataContext.SaveChangesAsync();
        }

        /// <summary>
        ///     执行视频数据查询的异步方法。
        /// </summary>
        /// <param name="dir">目标目录，可选。</param>
        /// <param name="caption">视频标题或描述的部分文本，可选。</param>
        /// <param name="evaluate">最小评价分数，可选。</param>
        /// <param name="isDesc">排序方式，true 表示按降序，false 表示按升序。</param>
        /// <param name="skip">跳过的记录数，默认为 0。</param>
        /// <param name="take">要返回的最大记录数，默认为 int.MaxValue。</param>
        /// <param name="status">视频状态过滤条件，默认为 1。</param>
        /// <param name="wideScrenn">是否为宽屏视频，布尔值，可选。</param>
        /// <returns>符合条件的视频列表。</returns>
        public async Task<List<Video>> QueryAsync(string? dir = null,
            string? caption = null, int? evaluate = null,
            bool isDesc = true, int skip = 0, int take = int.MaxValue, decimal status = 1, bool? wideScrenn = null)
        {
            var query = dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            if (!string.IsNullOrWhiteSpace(dir))
                query = query.Where(m => m.RootDir.Replace(@"\", "/").EndsWith(dir.Replace(@"\", "/")));

            if (!string.IsNullOrWhiteSpace(caption))
                query = query.Where(m => m.Caption.Contains(caption));

            if (evaluate.HasValue)
                query = query.Where(m => m.Evaluate >= evaluate.Value);

            if (wideScrenn.HasValue)
                query = query.Where(m => m.WideScrenn == wideScrenn.Value);

            query = isDesc
                ? query.OrderByDescending(m => m.Evaluate).ThenByDescending(v => v.ModifyTime).ThenBy(m => m.RootDir)
                : (IQueryable<Video>)query.OrderByDescending(m => m.Evaluate).ThenBy(v => v.ModifyTime)
                    .ThenBy(m => m.RootDir);

            var sql = query.ToQueryString();
            Log.Information($"[SQL] {sql}");
            return await query.Skip(skip).Take(take).ToListAsync();
        }

        /// <summary>
        ///     异步查询视频的方法，根据指定的谓词条件从数据库中检索视频列表。
        /// </summary>
        /// <param name="predicate">用于筛选视频的谓词表达式。</param>
        /// <returns>满足条件的视频列表。</returns>
        public async Task<List<Video>> QueryAsync(Expression<Func<Video, bool>> predicate)
        {
            var query = dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            return await query.Where(predicate).ToListAsync();
        }

        /// <summary>
        ///     异步查询视频的方法，根据指定的谓词条件从数据库中检索视频列表。
        /// </summary>
        /// <param name="predicate">用于筛选视频的谓词表达式。</param>
        /// <returns>满足条件的视频列表。</returns>
        public async Task<List<Video>> QueryOnlyAsync(Expression<Func<Video, bool>> predicate = null)
        {
            var query = dataContext.Videos.AsQueryable();
            return predicate == null ? await query.ToListAsync() : await query.Where(predicate).ToListAsync();
        }

        /// <summary>
        ///     异步查询视频的方法，根据指定的谓词条件从数据库中检索视频列表。
        /// </summary>
        /// <param name="predicate">用于筛选视频的谓词表达式。</param>
        /// <returns>满足条件的视频列表。</returns>
        public async Task<Video> FirstAsync(Expression<Func<Video, bool>> predicate)
        {
            var query = dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            return await query.Where(predicate).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     异步查询视频的方法，根据指定的谓词条件从数据库中检索视频列表。
        /// </summary>
        /// <param name="predicate">用于筛选视频的谓词表达式。</param>
        /// <returns>满足条件的视频列表。</returns>
        public Video First(Expression<Func<Video, bool>> predicate)
        {
            var query = dataContext.Videos
                .Include(v => v.Snapshots).AsQueryable();

            return query.Where(predicate).FirstOrDefault();
        }
        
        /// <summary>
        /// 异步查询重复视频的方法，根据指定的字段分组，返回重复视频列表。
        /// </summary>
        /// <typeparam name="TKey">用于分组的字段类型。</typeparam>
        /// <param name="keySelector">用于指定分组依据的表达式。</param>
        /// <returns>包含重复视频的列表。</returns>
        public async Task<List<Video>> QueryDuplicatesAsync<TKey>(Expression<Func<Video, TKey>> keySelector)
        {
            // 查询所有视频数据
            var query = dataContext.Videos.Include(v => v.Snapshots).AsQueryable();

            // 查找重复的视频
            var duplicateKeys = await query
                .GroupBy(keySelector) // 按指定字段分组
                .Where(g => g.Count() > 1) // 找出组内数量大于1的
                .Select(g => g.Key) // 提取重复的Key
                .ToListAsync();

            // 获取所有满足重复条件的视频列表
            var duplicates = await query
                .Where(video => duplicateKeys.Contains(keySelector.Compile().Invoke(video)))
                .ToListAsync();

            return duplicates;
        }
        
        /// <summary>
        /// 根据 MD5 查询所有 MD5 相同的视频集合。
        /// </summary>
        /// <returns>包含所有 MD5 相同的视频集合的列表。</returns>
        public async Task<List<Video>> QueryMD5DuplicateAsync()
        {
            // 查询视频数据
            var query = dataContext.Videos.Include(v => v.Snapshots).AsQueryable();

            // 查找重复的 MD5 值
            var duplicateMD5s = await query
                .GroupBy(video => video.MD5) // 按 MD5 分组
                .Where(group => group.Count() > 1) // 只保留重复的
                .Select(group => group.Key) // 提取 MD5 值
                .ToListAsync();

            // 找出所有 MD5 重复的视频
            var duplicateVideos = await query
                .Where(video => duplicateMD5s.Contains(video.MD5))
                .ToListAsync();

            return duplicateVideos;
        }
    }
}