using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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

	public async Task SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var data = await db.HostRegistrations.SingleOrDefaultAsync(i => (i.MachineName + "||" + i.HostName).Equals(hostRegistrationInfo.Key), cancellationToken);
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

		var updatecount = await db.SaveChangesAsync(cancellationToken);
		if (updatecount > 0)
		{
			cache.Remove(CACHE_REGISTRATIONS);
		}
	}

	public async Task DeleteHostRegistration(string key, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var existing = await db.HostRegistrations.SingleOrDefaultAsync(i => $"{i.MachineName}||{i.HostName}".Equals(key), cancellationToken);
		if (existing != null)
		{
			db.Remove(existing!);
			db.Entry(existing).State = EntityState.Deleted;
		}
		var updatecount = await db.SaveChangesAsync(cancellationToken);
		if (updatecount > 0)
		{
			cache.Remove(CACHE_REGISTRATIONS);
		}
	}

	public async Task<List<HostRegistrationInfo>> GetHostRegistrationList(CancellationToken cancellationToken = default)
	{
		cache.TryGetValue(CACHE_REGISTRATIONS, out List<HostRegistrationInfo>? list);
		if (list != null)
		{
			return list;
		}
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		list = await db.HostRegistrations.ToListAsync(cancellationToken);
		if (list != null)
		{
			cache.Set(CACHE_REGISTRATIONS, list);
		}
		return list!;
	}

	public async Task SaveScheduledTask(ScheduledTask scheduledTask, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var name = scheduledTask.Name;
		var data = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name!.Equals(name), cancellationToken);
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

		var updatecount = await db.SaveChangesAsync(cancellationToken);
		if (updatecount > 0)
		{
			cache.Remove(CACHE_SCHEDULEDTASKS);
		}
	}

	public async Task DeleteScheduledTask(string name, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var existing = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name!.Equals(name), cancellationToken);
		if (existing != null)
		{
			db.Remove(existing!);
			db.Entry(existing).State = EntityState.Deleted;
		}
		var updatecount = await db.SaveChangesAsync(cancellationToken);
		if (updatecount > 0)
		{
			cache.Remove(CACHE_SCHEDULEDTASKS);
		}
	}

	public async Task<List<ScheduledTask>> GetScheduledTaskList(CancellationToken cancellationToken = default)
	{
		cache.TryGetValue(CACHE_SCHEDULEDTASKS, out List<ScheduledTask>? list);
		if (list != null)
		{
			return list;
		}
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		list = await db.ScheduledTasks.ToListAsync(cancellationToken);
		if (list != null)
		{
			cache.Set(CACHE_SCHEDULEDTASKS, list);
		}
		return list!;
	}

	public async Task<List<RunningTask>> GetRunningTaskList(bool withHistory = false, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var query = from rt in db.RunningTasks
					select rt;

		if (!withHistory)
		{
			query = query.Where(i => !i.TerminatedDate.HasValue);
		}

		var list = await query.ToListAsync(cancellationToken);
		return list;
	}

    public async Task<RunningTask?> GetLastRunningTask(string taskName, CancellationToken cancellationToken = default)
    {
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = from rt in db.RunningTasks
					where taskName.Equals(rt.TaskName)
					orderby rt.RunningDate descending
                    select rt;

        var last = await query.FirstOrDefaultAsync(cancellationToken);
        return last;
    }

    public async Task SaveRunningTask(RunningTask task, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var data = await db.RunningTasks.SingleOrDefaultAsync(i => i.Id == task.Id, cancellationToken);
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
		await db.SaveChangesAsync(cancellationToken);
	}

	public async Task ResetRunningTasks(CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		await db.RunningTasks.ExecuteDeleteAsync(cancellationToken);
	}

	public async Task SaveProgressInfo(ProgressInfo progressInfo, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		db.ProgressInfos.Add(progressInfo);
		await db.SaveChangesAsync(cancellationToken);
	}

	public async Task<List<ProgressInfo>> GetProgressInfoList(Guid RunningTaskId, CancellationToken cancellationToken = default)
	{
		using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var result = await db.ProgressInfos.Where(i => i.TaskId == RunningTaskId).ToListAsync(cancellationToken);
		return result.OrderByDescending(i => i.CreationDate).ToList();
	}

	public Task PersistAll(CancellationToken cancellationToken = default)
	{
		// Do nothing
		return Task.CompletedTask;
	}
}
