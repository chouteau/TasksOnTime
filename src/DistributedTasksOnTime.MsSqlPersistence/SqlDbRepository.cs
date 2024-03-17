using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedTasksOnTime.MsSqlPersistence;

internal class SqlDbRepository(
	IDbContextFactory<MsSqlDbContext> dbContextFactory,
	IMemoryCache cache
	)
	: IDbRepository
{
	private const string CACHE_REGISTRATIONS = "hostregistrations";
	private const string CACHE_SCHEDULEDTASKS = "scheduledtasks";

	public async Task SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo)
	{
		using var db = await dbContextFactory.CreateDbContextAsync();

		var data = db.HostRegistrations.SingleOrDefault(i => (i.MachineName + "||" + i.HostName).Equals(hostRegistrationInfo.Key));
		if (data == null)
		{
			db.HostRegistrations!.Add(hostRegistrationInfo);
		}
		else
		{
			data.State = hostRegistrationInfo.State;
			db.HostRegistrations!.Attach(data);
			db.Entry(data!).State = EntityState.Modified;
		}

		// TODO: Supprimer les taches planifiées qui existent mais ne sont plus référencées

		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			cache.Remove(CACHE_REGISTRATIONS);
		}
	}

	public async Task DeleteHostRegistration(string key)
	{
		using var db = dbContextFactory.CreateDbContext();
		var existing = await db.HostRegistrations.SingleOrDefaultAsync(i => $"{i.MachineName}||{i.HostName}".Equals(key));
		if (existing != null)
		{
			db.Remove(existing!);
			db.Entry(existing).State = EntityState.Deleted;
		}
		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			cache.Remove(CACHE_REGISTRATIONS);
		}
	}

	public async Task<List<HostRegistrationInfo>> GetHostRegistrationList()
	{
		cache.TryGetValue(CACHE_REGISTRATIONS, out List<HostRegistrationInfo>? list);
		if (list != null)
		{
			return list;
		}
		var db = dbContextFactory.CreateDbContext();
		list = await db.HostRegistrations.ToListAsync();
		if (list != null)
		{
			cache.Set(CACHE_REGISTRATIONS, list);
		}
		return list!;
	}

	public async Task SaveScheduledTask(ScheduledTask scheduledTask)
	{
		var db = dbContextFactory.CreateDbContext();

		var name = scheduledTask.Name;
		var data = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name.Equals(name));
		if (data == null)
		{
			db.ScheduledTasks!.Add(scheduledTask);
		}
		else if (scheduledTask.FromEditor)
		{
			data.AssemblyQualifiedName = scheduledTask.AssemblyQualifiedName;
			data.StartMinute = scheduledTask.StartMinute;
			data.StartHour = scheduledTask.StartHour;
			data.StartDay = scheduledTask.StartDay;
			data.Interval = scheduledTask.Interval;
			data.Period = scheduledTask.Period;
			data.Enabled = scheduledTask.Enabled;
			data.AllowMultipleInstance = scheduledTask.AllowMultipleInstance;
			data.AllowLocalMultipleInstances = scheduledTask.AllowLocalMultipleInstances;
			data.Parameters = scheduledTask.Parameters;
			data.Description = scheduledTask.Description;
			data.ProcessMode = scheduledTask.ProcessMode;
			data.LastDurationInSeconds = scheduledTask.LastDurationInSeconds;

			db.ScheduledTasks!.Attach(data);
			db.Entry(data).State = EntityState.Modified;
		}
		else if (scheduledTask.NextRunningDate != DateTime.MinValue)
		{
			data.NextRunningDate = scheduledTask.NextRunningDate;
			data.StartedCount = scheduledTask.StartedCount;
			db.ScheduledTasks!.Attach(data);
			db.Entry(data).State = EntityState.Modified;
		}

		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			cache.Remove(CACHE_SCHEDULEDTASKS);
		}
	}

	public async Task DeleteScheduledTask(string name)
	{
		var db = dbContextFactory.CreateDbContext();
		var existing = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name.Equals(name));
		if (existing != null)
		{
			db.Remove(existing!);
			db.Entry(existing).State = EntityState.Deleted;
		}
		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			cache.Remove(CACHE_SCHEDULEDTASKS);
		}
	}

	public async Task<List<ScheduledTask>> GetScheduledTaskList()
	{
		cache.TryGetValue(CACHE_SCHEDULEDTASKS, out List<ScheduledTask>? list);
		if (list != null)
		{
			return list;
		}
		var db = dbContextFactory.CreateDbContext();
		list = await db.ScheduledTasks.ToListAsync();
		if (list != null)
		{
			cache.Set(CACHE_SCHEDULEDTASKS, list);
		}
		return list!;
	}

	public async Task<List<RunningTask>> GetRunningTaskList(bool withProgress = false, bool withHistory = false)
	{
		var db = dbContextFactory.CreateDbContext();
		var query = from rt in db.RunningTasks
					select rt;

		if (!withHistory)
		{
			query = query.Where(i => !i.TerminatedDate.HasValue);
		}

		var list = await query.ToListAsync();
		if (withProgress)
		{
			var taskIdList = list.Select(i => i.Id).Distinct().ToList();
			var progressList = await db.ProgressInfos
										.Where(i => taskIdList.Contains(i.TaskId))
										.OrderBy(i => i.CreationDate)
										.ToListAsync();

			foreach (var progressData in progressList)
			{
				var runningTask = list.Single(i => i.Id == progressData.TaskId);
				runningTask.ProgressLogs.Add(progressData);
			}
		}
		return list;
	}

	public async Task SaveRunningTask(RunningTask task)
	{
		var db = dbContextFactory.CreateDbContext();
		var data = await db.RunningTasks.SingleOrDefaultAsync(i => i.Id == task.Id);
		if (data == null)
		{
			db.RunningTasks!.Add(task);
		}
		else
		{
			data.RunningDate = task.RunningDate;
			data.TerminatedDate = task.TerminatedDate;
			data.CanceledDate = task.CanceledDate;
			data.CancelingDate = task.CancelingDate;
			data.ErrorStack = task.ErrorStack;
			data.IsForced = task.IsForced;
			data.FailedDate = task.FailedDate;

			db.RunningTasks!.Attach(data);
			db.Entry(data).State = EntityState.Modified;
		}
		await db.SaveChangesAsync();
	}

	public async Task ResetRunningTasks()
	{
		var db = dbContextFactory.CreateDbContext();
		await db.RunningTasks.ExecuteDeleteAsync();
		await db.SaveChangesAsync();
	}

	public async Task SaveProgressInfo(ProgressInfo progressInfo)
	{
		var db = dbContextFactory.CreateDbContext();
		db.ProgressInfos.Add(progressInfo);
		await db.SaveChangesAsync();
	}

	public Task PersistAll()
	{
		// Do nothing
		return Task.CompletedTask;
	}
}
