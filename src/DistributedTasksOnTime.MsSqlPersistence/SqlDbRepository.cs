using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedTasksOnTime.MsSqlPersistence;

internal class SqlDbRepository : IDbRepository
{
	private const string CACHE_REGISTRATIONS = "hostregistrations";
	private const string CACHE_SCHEDULEDTASKS = "scheduledtasks";

	private readonly IDbContextFactory<MsSqlDbContext> _dbContextFactory;
	private readonly IMemoryCache _cache;
	private readonly IMapper _mapper;

	public SqlDbRepository(IDbContextFactory<MsSqlDbContext> dbContextFactory,
		IMemoryCache cache,
		AutoMapper.IMapper mapper)
	{
		_dbContextFactory = dbContextFactory;
		_cache = cache;
		_mapper = mapper;
	}

	public async Task SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo)
	{
		using var db = await _dbContextFactory.CreateDbContextAsync();

		var existing = db.HostRegistrations.SingleOrDefault(i => i.UniqueKey.Equals(hostRegistrationInfo.Key));
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

		// TODO: Supprimer les taches planifiées qui existent mais ne sont plus référencées

		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			_cache.Remove(CACHE_REGISTRATIONS);
		}
	}

	public async Task DeleteHostRegistration(string key)
	{
		using var db = _dbContextFactory.CreateDbContext();
		var existing = await db.HostRegistrations.SingleOrDefaultAsync(i => i.UniqueKey.Equals(key));
		if (existing != null)
		{
			db.Remove(existing!);
			db.Entry(existing).State = EntityState.Deleted;
		}
		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			_cache.Remove(CACHE_REGISTRATIONS);
		}
	}

	public async Task<List<HostRegistrationInfo>> GetHostRegistrationList()
	{
		_cache.TryGetValue(CACHE_REGISTRATIONS, out List<HostRegistrationInfo>? list);
		if (list != null)
		{
			return list;
		}
		var db = _dbContextFactory.CreateDbContext();
		var datas = await db.HostRegistrations.ToListAsync();
		list = _mapper.Map<List<HostRegistrationInfo>>(datas);
		if (list != null)
		{
			_cache.Set(CACHE_REGISTRATIONS, list);
		}
		return list!;
	}

	public async Task SaveScheduledTask(ScheduledTask scheduledTask)
	{
		var db = _dbContextFactory.CreateDbContext();

		var name = scheduledTask.Name;
		var existing = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name.Equals(name));
		if (existing == null)
		{
			var st = _mapper.Map<Datas.ScheduledTaskData>(scheduledTask);
			db.ScheduledTasks!.Add(st);
		}
		else
		{
			existing = _mapper.Map(scheduledTask, existing);
			existing.LastUpdate = DateTime.Now;
			db.ScheduledTasks!.Attach(existing);
			db.Entry(existing).State = EntityState.Modified;
		}

		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			_cache.Remove(CACHE_SCHEDULEDTASKS);
		}
	}

	public async Task DeleteScheduledTask(string name)
	{
		var db = _dbContextFactory.CreateDbContext();
		var existing = await db.ScheduledTasks.SingleOrDefaultAsync(i => i.Name.Equals(name));
		if (existing != null)
		{
			db.Remove(existing!);
			db.Entry(existing).State = EntityState.Deleted;
		}
		var updatecount = await db.SaveChangesAsync();
		if (updatecount > 0)
		{
			_cache.Remove(CACHE_SCHEDULEDTASKS);
		}
	}

	public async Task<List<ScheduledTask>> GetScheduledTaskList()
	{
		_cache.TryGetValue(CACHE_SCHEDULEDTASKS, out List<ScheduledTask>? list);
		if (list != null)
		{
			return list;
		}
		var db = _dbContextFactory.CreateDbContext();
		var datas = await db.ScheduledTasks.ToListAsync();
		list = _mapper.Map<List<ScheduledTask>>(datas);
		if (list != null)
		{
			_cache.Set(CACHE_SCHEDULEDTASKS, list);
		}
		return list!;
	}

	public async Task<List<RunningTask>> GetRunningTaskList(bool withProgress = false, bool withHistory = false)
	{
		var db = _dbContextFactory.CreateDbContext();
		var query = from rt in db.RunningTasks
					select rt;

		if (!withHistory)
		{
			query = query.Where(i => !i.TerminatedDate.HasValue);
		}
		
		var data =  await query.ToListAsync();
		var result = _mapper.Map<List<RunningTask>>(data);
		if (withProgress)
		{
			var taskIdList = result.Select(i => i.Id).Distinct().ToList();
			var progressList = await db.ProgressInfos
										.Where(i => taskIdList.Contains(i.TaskId))
										.OrderBy(i => i.CreationDate)
										.ToListAsync();

			foreach (var progressData in progressList)
			{
				var runningTask = result.Single(i => i.Id == progressData.TaskId);
				var progress = _mapper.Map<ProgressInfo>(progressData);
				runningTask.ProgressLogs.Add(progress);
			}
		}
		return result;
	}

	public async Task SaveRunningTask(RunningTask task)
	{
		var db = _dbContextFactory.CreateDbContext();
		var existing = await db.RunningTasks.SingleOrDefaultAsync(i => i.Id == task.Id);
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
		await db.SaveChangesAsync();
	}

	public async Task ResetRunningTasks()
	{
		var db = _dbContextFactory.CreateDbContext();
		await db.RunningTasks.ExecuteDeleteAsync();
		await db.SaveChangesAsync();
	}

	public async Task SaveProgressInfo(ProgressInfo progressInfo)
	{
		var db = _dbContextFactory.CreateDbContext();
		var data = _mapper.Map<Datas.ProgressInfoData>(progressInfo);
		db.ProgressInfos.Add(data);
		await db.SaveChangesAsync();
	}

	public Task PersistAll()
	{
		// Do nothing
		return Task.CompletedTask;
	}
}
