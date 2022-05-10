namespace DistributedTasksOnTime.Orchestrator.Repository;

internal class FileDbRepository : IDbRepository
{
	readonly JsonSerializerOptions _options = new JsonSerializerOptions();

	public FileDbRepository(DistributedTasksOnTimeServerSettings settings,
		ILogger<FileDbRepository> logger)
	{
		this.Settings = settings;
		this.Logger = logger;
		_options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
		_options.WriteIndented = true;
	}

	protected DistributedTasksOnTimeServerSettings Settings { get; }
	protected ILogger Logger { get; }

	public List<HostRegistrationInfo> GetHostRegistrationList()
	{
		var fileName = System.IO.Path.Combine(Settings.StoreFolder, "HostRegistrationInfoList.json");
		if (!System.IO.File.Exists(fileName))
		{
			return new List<HostRegistrationInfo>();
		}
		var content = File.ReadAllText(fileName);
		var result = JsonSerializer.Deserialize<List<HostRegistrationInfo>>(content, _options);
		return result;
	}

	public List<Models.ScheduledTask> GetScheduledTaskList()
	{
		var fileName = System.IO.Path.Combine(Settings.StoreFolder, "ScheduledTaskList.json");
		if (!System.IO.File.Exists(fileName))
		{
			return new List<Models.ScheduledTask>();
		}
		var content = File.ReadAllText(fileName);
		var result = JsonSerializer.Deserialize<List<Models.ScheduledTask>>(content, _options);
		return result;
	}

	public void PersistHostRegistrationList(List<HostRegistrationInfo> list)
	{
		var fileName = System.IO.Path.Combine(Settings.StoreFolder, "HostRegistrationInfoList.json");
		if (System.IO.File.Exists(fileName))
		{
			System.IO.File.Copy(fileName, $"{fileName}.bak", true);
			System.IO.File.Delete(fileName);
		}
		var content = JsonSerializer.Serialize(list, _options);
		File.WriteAllText(fileName, content);
		Logger.LogTrace("{0} Host persisted in {1}", list.Count, fileName);
	}

	public void PersistScheduledTaskList(List<Models.ScheduledTask> list)
	{
		var fileName = System.IO.Path.Combine(Settings.StoreFolder, "ScheduledTaskList.json");
		if (System.IO.File.Exists(fileName))
		{
			System.IO.File.Copy(fileName, $"{fileName}.bak", true);
			System.IO.File.Delete(fileName);
		}
		var content = JsonSerializer.Serialize(list, _options);
		File.WriteAllText(fileName, content);
		Logger.LogTrace("{0} Scheduled Task persisted in {1}", list.Count, fileName);
	}
}

