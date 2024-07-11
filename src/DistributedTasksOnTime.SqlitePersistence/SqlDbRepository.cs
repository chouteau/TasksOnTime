using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedTasksOnTime.SqlitePersistence
{
	internal class SqlDbRepository : IDbRepository
	{
		private const string CACHE_REGISTRATIONS = "hostregistrations";
		private const string CACHE_SCHEDULEDTASKS = "scheduledtasks";

		private readonly IDbContextFactory<SqliteDbContext> _dbContextFactory;
		private readonly IMemoryCache _cache;
		private readonly IMapper _mapper;

		public SqlDbRepository(IDbContextFactory<SqliteDbContext> dbContextFactory,
			IMemoryCache cache,
			AutoMapper.IMapper mapper)
		{
			_dbContextFactory = dbContextFactory;
			_cache = cache;
			_mapper = mapper;
		}

		public async Task SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

			var existing = await db.HostRegistrations!.SingleOrDefaultAsync(i => i.UniqueKey.Equals(hostRegistrationInfo.Key), cancellationToken);
			if (existing == null)
			{
				var ri = _mapper.Map<Datas.HostRegistrationData>(hostRegistrationInfo);
				db.HostRegistrations!.Add(ri);
			}
			else
			{
				existing = _mapper.Map(hostRegistrationInfo, existing);
				db.HostRegistrations!.Attach(existing!);
				db.Entry(existing!).State = EntityState.Modified;
			}

			// Supprimer les taches planifiées qui existent mais ne sont plus référencées

			var updatecount = await db.SaveChangesAsync(cancellationToken);
			if (updatecount > 0)
			{
				_cache.Remove(CACHE_REGISTRATIONS);
			}
		}

		public async Task DeleteHostRegistration(string key, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var existing = await db.HostRegistrations!.SingleOrDefaultAsync(i => i.UniqueKey.Equals(key), cancellationToken);
			if (existing != null)
			{
				db.Remove(existing!);
				db.Entry(existing).State = EntityState.Deleted;
			}
			var updatecount = await db.SaveChangesAsync(cancellationToken);
			if (updatecount > 0)
			{
				_cache.Remove(CACHE_REGISTRATIONS);
			}
		}

		public async Task<List<HostRegistrationInfo>> GetHostRegistrationList(CancellationToken cancellationToken = default)
		{
			_cache.TryGetValue(CACHE_REGISTRATIONS, out List<HostRegistrationInfo>? list);
			if (list != null)
			{
				return list;
			}
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var datas = await db.HostRegistrations!.ToListAsync(cancellationToken);
			list = _mapper.Map<List<HostRegistrationInfo>>(datas);
			if (list != null)
			{
				_cache.Set(CACHE_REGISTRATIONS, list);
			}
			return list!;
		}

		public async Task SaveScheduledTask(ScheduledTask scheduledTask, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

			var name = scheduledTask.Name;
			var existing = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name.Equals(name), cancellationToken);
			if (existing == null)
			{
				var st = _mapper.Map<Datas.ScheduledTaskData>(scheduledTask);
				db.ScheduledTasks!.Add(st);
			}
			else if (scheduledTask.FromEditor)
			{
				var nextRunningDate = existing.NextRunningDate;
				var startedCount = existing.StartedCount;
				existing = _mapper.Map(scheduledTask, existing);
				existing.NextRunningDate = nextRunningDate;
				existing.StartedCount = startedCount;
				existing.LastUpdate = DateTime.Now;
				db.ScheduledTasks!.Attach(existing);
				db.Entry(existing).State = EntityState.Modified;
			}
			else
			{
				existing.NextRunningDate = scheduledTask.NextRunningDate;
				existing.LastUpdate = DateTime.Now;
				existing.StartedCount = scheduledTask.StartedCount;
				db.ScheduledTasks!.Attach(existing);
				db.Entry(existing).State = EntityState.Modified;
			}

			var updatecount = await db.SaveChangesAsync(cancellationToken);
			if (updatecount > 0)
			{
				_cache.Remove(CACHE_SCHEDULEDTASKS);
			}
		}

		public async Task DeleteScheduledTask(string name, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var existing = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name.Equals(name), cancellationToken);
			if (existing != null)
			{
				db.Remove(existing!);
				db.Entry(existing).State = EntityState.Deleted;
			}
			var updatecount = await db.SaveChangesAsync(cancellationToken);
			if (updatecount > 0)
			{
				_cache.Remove(CACHE_SCHEDULEDTASKS);
			}
		}

		public async Task<List<ScheduledTask>> GetScheduledTaskList(CancellationToken cancellationToken = default)
		{
			_cache.TryGetValue(CACHE_SCHEDULEDTASKS, out List<ScheduledTask>? list);
			if (list != null)
			{
				return list;
			}
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var datas = await db.ScheduledTasks.ToListAsync(cancellationToken);
			list = _mapper.Map<List<ScheduledTask>>(datas);
			if (list != null)
			{
				_cache.Set(CACHE_SCHEDULEDTASKS, list);
			}
			return list!;
		}

		public async Task<List<RunningTask>> GetRunningTaskList(bool withHistory = false, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var query = from rt in db.RunningTasks
						select rt;

			if (!withHistory)
			{
				query = query.Where(i => !i.TerminatedDate.HasValue);
			}

			var data = await query.ToListAsync(cancellationToken);
			var result = _mapper.Map<List<RunningTask>>(data);
			return result;
		}

		public async Task<RunningTask?> GetLastRunningTask(string taskName, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var query = from rt in db.RunningTasks
						where taskName.Equals(rt.TaskName)
						orderby rt.RunningDate descending
						select rt;

			var last = await query.FirstOrDefaultAsync(cancellationToken);
			var result = _mapper.Map<RunningTask>(last);
			return result;
		}

		public async Task SaveRunningTask(RunningTask task, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var existing = await db.RunningTasks.SingleOrDefaultAsync(i => i.Id == task.Id, cancellationToken);
			if (existing == null)
			{
				var data = _mapper.Map<Datas.RunningTaskData>(task);
				db.RunningTasks!.Add(data);
			}
			else
			{
				existing = _mapper.Map(task, existing);
				db.RunningTasks!.Attach(existing);
				db.Entry(existing).State = EntityState.Modified;
			}
			await db.SaveChangesAsync(cancellationToken);
		}

		public async Task ResetRunningTasks(CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			FormattableString script = $"Delete from RunningTask";
			await db.Database.ExecuteSqlAsync(script,cancellationToken);
			await db.SaveChangesAsync(cancellationToken);
		}

		public async Task SaveProgressInfo(ProgressInfo progressInfo, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var data = _mapper.Map<Datas.ProgressInfoData>(progressInfo);
			db.ProgressInfos.Add(data);
			await db.SaveChangesAsync(cancellationToken);
		}

		public async Task<List<ProgressInfo>> GetProgressInfoList(Guid RunningTaskId, CancellationToken cancellationToken = default)
		{
			using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
			var data = await db.ProgressInfos.Where(i => i.TaskId == RunningTaskId).ToListAsync(cancellationToken);
			var result = _mapper.Map<List<ProgressInfo>>(data);
			return result.OrderByDescending(i => i.CreationDate).ToList();
		}

		public Task PersistAll(CancellationToken cancellationToken = default)
		{
			// Do nothing
			return Task.CompletedTask;
		}
	}
}