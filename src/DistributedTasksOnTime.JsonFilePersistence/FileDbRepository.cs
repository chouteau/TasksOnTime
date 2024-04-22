using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace DistributedTasksOnTime.JsonFilePersistence;

internal class FileDbRepository : IDbRepository
{
    readonly JsonSerializerOptions _options = new JsonSerializerOptions();
    readonly string hostRegistrationFileName;
    readonly string scheduledTaskFileName;

    public FileDbRepository(PersistenceSettings settings,
        ILogger<FileDbRepository> logger)
    {
        this.Settings = settings;
        this.Logger = logger;
        _options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        _options.WriteIndented = true;
        hostRegistrationFileName = System.IO.Path.Combine(Settings.StoreFolder, "HostRegistrationInfoList.json");
        scheduledTaskFileName = System.IO.Path.Combine(Settings.StoreFolder, "ScheduledTaskList.json");

        try
        {
            Initialize();
        }
        catch(Exception ex)
        {
           logger.LogError(ex, ex.Message);
        }
    }

    protected ConcurrentDictionary<string, DistributedTasksOnTime.HostRegistrationInfo> HostList { get; set; } = new();
    protected ConcurrentDictionary<string, ScheduledTask> ScheduledTaskList { get; set; } = new();
    protected ConcurrentDictionary<Guid, RunningTask> RunningTaskList { get; set; } = new();

    protected PersistenceSettings Settings { get; }
    protected ILogger Logger { get; }

    public Task<List<HostRegistrationInfo>> GetHostRegistrationList()
    {
        return Task.FromResult(HostList!.Select(i => i.Value).ToList());
    }

    public Task<List<ScheduledTask>> GetScheduledTaskList()
    {
        return Task.FromResult(ScheduledTaskList!.Select(i => i.Value).ToList());
    }

    public async Task SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo)
    {
        HostList.AddOrUpdate(hostRegistrationInfo.Key, hostRegistrationInfo, (k, old) => hostRegistrationInfo);
        await PersistHostRegistrationList();
    }

    public async Task DeleteHostRegistration(string key)
    {
        HostList.TryRemove(key, out var item);
        await PersistHostRegistrationList();
    }

    public async Task PersistHostRegistrationList()
    {
        try
        {
            var list = HostList.Select(i => i.Value).ToList();
            if (System.IO.File.Exists(hostRegistrationFileName))
            {
                System.IO.File.Copy(hostRegistrationFileName, $"{hostRegistrationFileName}.bak", true);
                System.IO.File.Delete(hostRegistrationFileName);
            }
            var content = JsonSerializer.Serialize(list, _options);
            await File.WriteAllTextAsync(hostRegistrationFileName, content);
            Logger.LogTrace("{0} Host persisted in {1}", list.Count, hostRegistrationFileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
    }

    public async Task SaveScheduledTask(ScheduledTask scheduledTask)
    {
        ScheduledTaskList.AddOrUpdate(scheduledTask.Name, scheduledTask, (k, old) => scheduledTask);
        await PersistScheduledTaskList();
    }

    public async Task DeleteScheduledTask(string taskName)
    {
        ScheduledTaskList.TryRemove(taskName, out var scheduledTask); 
        await PersistScheduledTaskList();
    }

    public async Task PersistScheduledTaskList()
    {
        try
        {
            var list = ScheduledTaskList.Select(i => i.Value).ToList();
            if (System.IO.File.Exists(scheduledTaskFileName))
            {
                System.IO.File.Copy(scheduledTaskFileName, $"{scheduledTaskFileName}.bak", true);
                System.IO.File.Delete(scheduledTaskFileName);
            }
            var content = JsonSerializer.Serialize(list, _options);
            await File.WriteAllTextAsync(scheduledTaskFileName, content);
            Logger.LogTrace("{0} Scheduled Task persisted in {1}", list.Count, scheduledTaskFileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
    }

    public Task<List<RunningTask>> GetRunningTaskList(bool withHistory = false)
	{
		return Task.FromResult(RunningTaskList!.Select(i => i.Value).ToList());
    }

    public Task SaveRunningTask(RunningTask task)
	{
		RunningTaskList.AddOrUpdate(task.Id, task, (key, oldValue) => task);
        return Task.CompletedTask;
	}

    public Task ResetRunningTasks()
    {
        RunningTaskList.Clear();
        return Task.CompletedTask;
    }

	public async Task PersistAll()
	{
        await PersistScheduledTaskList();
        await PersistHostRegistrationList();
    }

    private void Initialize()
    {
        if (System.IO.File.Exists(hostRegistrationFileName))
        {
            var content = File.ReadAllText(hostRegistrationFileName);
            var list = JsonSerializer.Deserialize<List<HostRegistrationInfo>>(content, _options);
            foreach (var item in list!)
            {
                HostList.TryAdd(item.Key, item);
            }
            Logger.LogTrace("Load {0} existing host", list.Count);
        }

        if (System.IO.File.Exists(scheduledTaskFileName))
        {
            var content = File.ReadAllText(scheduledTaskFileName);
            var list = JsonSerializer.Deserialize<List<ScheduledTask>>(content, _options);
            foreach (var item in list!)
            {
                ScheduledTaskList.TryAdd(item.Name, item);
            }

            Logger.LogTrace("Load {0} existing scheduled task", list.Count);
        }

    }

	public async Task<List<ProgressInfo>> GetProgressInfoList(Guid RunningTaskId)
	{
        await Task.Yield();
        throw new NotImplementedException();
	}

	public Task SaveProgressInfo(ProgressInfo progressInfo)
    {
        // Do nothing
        return Task.CompletedTask;
    }
}

