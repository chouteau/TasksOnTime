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

    public List<HostRegistrationInfo> GetHostRegistrationList()
    {
        return HostList!.Select(i => i.Value).ToList();
    }

    public List<ScheduledTask> GetScheduledTaskList()
    {
        return ScheduledTaskList!.Select(i => i.Value).ToList();
    }

    public void SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo)
    {
        HostList.AddOrUpdate(hostRegistrationInfo.Key, hostRegistrationInfo, (k, old) => hostRegistrationInfo);
        PersistHostRegistrationList();
    }

    public void DeleteHostRegistration(string key)
    {
        HostList.TryRemove(key, out var item);
        PersistHostRegistrationList();
    }

    public void PersistHostRegistrationList()
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
            File.WriteAllText(hostRegistrationFileName, content);
            Logger.LogTrace("{0} Host persisted in {1}", list.Count, hostRegistrationFileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
    }

    public void SaveScheduledTask(ScheduledTask scheduledTask)
    {
        ScheduledTaskList.AddOrUpdate(scheduledTask.Name, scheduledTask, (k, old) => scheduledTask);
        PersistScheduledTaskList();
    }

    public void DeleteScheduledTask(string taskName)
    {
        ScheduledTaskList.TryRemove(taskName, out var scheduledTask); 
        PersistScheduledTaskList();
    }

    public void PersistScheduledTaskList()
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
            File.WriteAllText(scheduledTaskFileName, content);
            Logger.LogTrace("{0} Scheduled Task persisted in {1}", list.Count, scheduledTaskFileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
    }

    public List<RunningTask> GetRunningTaskList(bool withProgress = false)
	{
		return RunningTaskList!.Select(i => i.Value).ToList();
    }

    public void SaveRunningTask(RunningTask task)
	{
		RunningTaskList.AddOrUpdate(task.Id, task, (key, oldValue) => task);
	}

    public void ResetRunningTasks()
    {
        RunningTaskList.Clear();
    }

	public void PersistAll()
	{
        PersistScheduledTaskList();
        PersistHostRegistrationList();
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

    public void SaveProgressInfo(ProgressInfo progressInfo)
    {
        // Do nothing
    }
}

