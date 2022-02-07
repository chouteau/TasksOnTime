namespace DistributedTasksOnTime.Orchestrator;

internal class TasksOrchestrator : ITasksOrchestrator
{
    public event Action<string> OnHostRegistered;

    public TasksOrchestrator(DistributedTasksOnTimeServerSettings scheduleSettings,
        ILogger<TasksOrchestrator> logger,
        Repository.IDbRepository dbRepository,
        QueueSender queueSender)
    {
        this.Settings = scheduleSettings;
        this.Logger = logger;
        this.DbRepository = dbRepository; 
        this.QueueSender = queueSender;
    }

    protected ConcurrentDictionary<string, DistributedTasksOnTime.HostRegistrationInfo> HostList { get; set; }
    protected ConcurrentDictionary<string, Models.ScheduledTask> ScheduledTaskList { get; set; }
    protected ConcurrentDictionary<string, DistributedTasksOnTime.ProcessTask> TaskOrderList { get; set; }
    protected ConcurrentDictionary<Guid, Models.RunningTask> RunningTaskList { get; set; }

    protected DistributedTasksOnTimeServerSettings Settings { get; }
    protected ILogger Logger { get; }
    protected Repository.IDbRepository DbRepository { get; }
    protected QueueSender QueueSender { get; }

    public void Start()
	{
        var hostList = DbRepository.GetHostRegistrationList();
        this.HostList = new ConcurrentDictionary<string, DistributedTasksOnTime.HostRegistrationInfo>();
        foreach (var item in hostList)
		{
            this.HostList.TryAdd(item.Key, item);
		}
        Logger.LogInformation("Start with {0} existing host", hostList.Count);

        var taskList = DbRepository.GetScheduledTaskList();
        this.ScheduledTaskList = new ConcurrentDictionary<string, Models.ScheduledTask>();
        foreach (var item in taskList)
		{
            this.ScheduledTaskList.TryAdd(item.Name, item);
        }

        Logger.LogInformation("Start with {0} existing scheduled task", taskList.Count);

        RunningTaskList = new ConcurrentDictionary<Guid, Models.RunningTask>();
    }

    public void Stop()
	{
        Logger.LogWarning("TasksOrchestrator stopping");

        var taskList = ScheduledTaskList.Select(i => i.Value).ToList();
        DbRepository.PersistScheduledTaskList(taskList);

        var hostList = HostList.Select(i => i.Value).ToList();
        DbRepository.PersistHostRegistrationList(hostList);
	}

    public void RegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo)
	{
        if (HostList.ContainsKey(hostInfo.Key))
		{
            Logger.LogInformation("Host already registered {0}", hostInfo.Key);
            return;
		}
        HostList.TryAdd(hostInfo.Key, hostInfo);
        var list = HostList.Select(i => i.Value).ToList();
        DbRepository.PersistHostRegistrationList(list);

		foreach (var task in hostInfo.TaskList)
		{
            if (ScheduledTaskList.ContainsKey(task.TaskName))
            {
                continue;
            }

            var scheduledTask = CreateScheduledTask(task);
            this.ScheduledTaskList.TryAdd(scheduledTask.Name, scheduledTask);

        }
        var taskList = ScheduledTaskList.Select(i => i.Value).ToList();
        DbRepository.PersistScheduledTaskList(taskList);

        OnHostRegistered?.Invoke(hostInfo.Key);
    }

    public void UnRegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo)
	{
        if (!HostList.ContainsKey(hostInfo.Key))
        {
            return;
        }
        HostList.TryRemove(hostInfo.Key, out var remove);
        var list = HostList.Select(i => i.Value).ToList();
        DbRepository.PersistHostRegistrationList(list);
        Logger.LogWarning("Host {0} unregistered", hostInfo.Key);
    }

    public void NotifyRunningTask(DistributedTasksOnTime.DistributedTaskInfo distributedTaskInfo)
	{
        RunningTaskList.TryGetValue(distributedTaskInfo.Id, out var existing);
        if (existing == null) // <- pas normal
		{
            Logger.LogWarning("Running task not found with id {0}", distributedTaskInfo.Id);
            existing = new Models.RunningTask();
            existing.Id = distributedTaskInfo.Id;
		}

        if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Enqueued)
		{
            Logger.LogTrace("Task {0} enqueued", existing.TaskName);
            existing.HostKey = distributedTaskInfo.HostKey;
            existing.EnqueuedDate = distributedTaskInfo.EventDate;
		}
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Started)
		{
            Logger.LogTrace("Task {0} started", existing.TaskName);
            existing.HostKey = distributedTaskInfo.HostKey;
            existing.RunningDate = distributedTaskInfo.EventDate;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Terminated)
		{
            Logger.LogTrace("Task {0} terminated", existing.TaskName);
            existing.HostKey = distributedTaskInfo.HostKey;
            existing.TerminatedDate = distributedTaskInfo.EventDate;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Canceling)
        {
            Logger.LogTrace("Task {0} canceling", existing.TaskName);
            existing.HostKey = distributedTaskInfo.HostKey;
            existing.CancelingDate = distributedTaskInfo.EventDate;
            existing.TerminatedDate = DateTime.Now;
        }
        else if (distributedTaskInfo.State ==  DistributedTasksOnTime.TaskState.Canceled)
		{
            Logger.LogTrace("Task {0} canceled", existing.TaskName);
            existing.HostKey = distributedTaskInfo.HostKey;
            existing.CanceledDate = distributedTaskInfo.EventDate;
            existing.TerminatedDate = DateTime.Now;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Failed)
        {
            Logger.LogTrace("Task {0} failed with stack {1}", existing.TaskName, distributedTaskInfo.ErrorStack);
            existing.HostKey = distributedTaskInfo.HostKey;
            existing.FailedDate = distributedTaskInfo.EventDate;
            existing.TerminatedDate = DateTime.Now;
            existing.ErrorStack = distributedTaskInfo.ErrorStack;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Progress)
		{
            Logger.LogTrace("Task {0} progress", existing.TaskName);
            if (distributedTaskInfo.ProgressInfo == null)
			{
                Logger.LogWarning("Progress event without info");
			}
            switch (distributedTaskInfo.ProgressInfo.Type)
			{
				case DistributedTasksOnTime.ProgressType.Start:
                    existing.Logs.Add("Start");
					break;
				case DistributedTasksOnTime.ProgressType.Write:
                    existing.Logs.Add(distributedTaskInfo.ProgressInfo.Subject);
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.StartProgress:
                    existing.Logs.Add($"StartProgress:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
					{
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.StartContinuousProgress:
                    existing.Logs.Add($"StartContinuousProgress:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.Progress:
                    existing.Logs.Add($"Progress:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.EndProgress:
                    existing.Logs.Add($"EndProgress:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.EndContinuousProgress:
                    existing.Logs.Add($"EndContinousProgress:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.EntityChanged:
                    existing.Logs.Add($"EntityChanged:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.Failed:
                    existing.Logs.Add($"Failed:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.Cancel:
                    existing.Logs.Add($"Cancel:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				case DistributedTasksOnTime.ProgressType.Completed:
                    existing.Logs.Add($"Comleted:{distributedTaskInfo.ProgressInfo.Subject}");
                    if (!string.IsNullOrWhiteSpace(distributedTaskInfo.ProgressInfo.Body))
                    {
                        existing.Logs.Add(distributedTaskInfo.ProgressInfo.Body);
                    }
                    break;
				default:
					break;
			}
		}
    }

    public bool ContainsTask(string taskName)
    {
        if (taskName == null)
		{
            throw new NullReferenceException("taskName is null");
		}

        taskName = taskName.ToLower();

        bool result = false;
        if (ScheduledTaskList == null || ScheduledTaskList.Count == 0)
        {
            return false;
        }
        result = ScheduledTaskList.ContainsKey(taskName);
        return result;
    }

    public async Task CancelTask(string taskName)
    {
        if (taskName == null)
        {
            throw new NullReferenceException("task name is null");
        }

        taskName = taskName.ToLower();

        Logger.LogInformation("Try to Cancel task {0}", taskName);
        var task = RunningTaskList.FirstOrDefault(i => i.Value.TaskName == taskName 
                            && !i.Value.TerminatedDate.HasValue);

        if (task.Value != null)
		{
            var cancelTask = new DistributedTasksOnTime.CancelTask();
            cancelTask.Id = task.Key;
            await QueueSender.SendMessage(Settings.CancelTaskQueueName, cancelTask);
        }
    }

    public async Task ForceTask(string taskName)
    {
        if (taskName == null)
        {
            throw new NullReferenceException("task name is null");
        }

        taskName = taskName.ToLower();

        Logger.LogInformation("Try to force task {0}", taskName);
            
        var getResult = ScheduledTaskList.TryGetValue(taskName, out Models.ScheduledTask task);
        if (!getResult)
		{
            throw new Exception($"try to get task {taskName} failed");
		}
        if (task == null)
        {
            Logger.LogWarning("force unknown task {0}", taskName);
            return;
        }
        await EnqueueTask(task);
    }

    public int GetScheduledTaskCount()
    {
        return ScheduledTaskList.Count;
    }

    public int GetRunningTaskCount()
    {
        return RunningTaskList.Count(i => !i.Value.TerminatedDate.HasValue);
    }

    public IEnumerable<Models.ScheduledTask> GetScheduledTaskList()
    {
        var result = new List<Models.ScheduledTask>();
        if (ScheduledTaskList == null)
		{
            return result;
		}
        foreach (var item in ScheduledTaskList)
        {
            result.Add(item.Value);
        }
        return result;
    }

    /// <summary>
    /// Recupération de la première tache executable 
    /// dans l'ordre FIFO
    /// </summary>
    /// <returns></returns>
    public async virtual Task EnqueueNextTasks(DateTime now)
    {
        var query = from t in ScheduledTaskList
                    where CanRun(now, t.Value)
                    select t.Value;

        var list = query.ToList();

        Logger.LogTrace("Found {0} task to enqueue", list.Count);

        while (true)
        {
            var task = list.FirstOrDefault();
            if (task == null)
            {
                break;
            }
            list.Remove(task);
            try
            {
                Logger.LogDebug("Try to start scheduled task {0}", task.Name);
                await EnqueueTask(task);
                SetNextRuningDate(DateTime.Now, task);
                var currentList = ScheduledTaskList.Select(i => i.Value).ToList();
                DbRepository.PersistScheduledTaskList(currentList);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskName", task.Name);
                Logger.LogError(ex, ex.Message);
            }
        }
    }

    internal async virtual Task EnqueueTask(Models.ScheduledTask scheduledTask, bool isForced = false)
    {
        var procesTask = new DistributedTasksOnTime.ProcessTask();
        procesTask.Id = Guid.NewGuid();
        procesTask.CreationDate = DateTime.Now;
        procesTask.TaskName = scheduledTask.Name;
        procesTask.FullTypeName = scheduledTask.AssemblyQualifiedName;

        var queueName = $"{Settings.PrefixQueueName}.{scheduledTask.Name}";
        await QueueSender.SendMessage(queueName, procesTask);

        var runningTask = new Models.RunningTask();
        runningTask.Id = procesTask.Id;
        runningTask.TaskName = scheduledTask.Name;
        runningTask.IsForced = isForced;

        RunningTaskList.TryAdd(runningTask.Id, runningTask);
    }

    internal bool CanRun(DateTime now, Models.ScheduledTask scheduledTask)
    {
        var runningTask = RunningTaskList.FirstOrDefault(
                        i => i.Value.TaskName == scheduledTask.Name
                        && !i.Value.TerminatedDate.HasValue);

        if (!scheduledTask.AllowMultipleInstance
            && runningTask.Value != null)
        {
            return false;
        }

        if (scheduledTask.Period == Models.ScheduledTaskTimePeriod.WorkingDay
                && (scheduledTask.NextRunningDate.DayOfWeek == DayOfWeek.Saturday
                || scheduledTask.NextRunningDate.DayOfWeek == DayOfWeek.Sunday))
        {
            return false;
        }

        if (now >= scheduledTask.NextRunningDate
            || scheduledTask.StartedCount == 0)
        {
            return true;
        }

        return false;
    }

    internal void SetNextRuningDate(DateTime now, Models.ScheduledTask scheduledTask)
    {
        switch (scheduledTask.Period)
        {
            case Models.ScheduledTaskTimePeriod.None:

                scheduledTask.NextRunningDate = DateTime.MinValue;

                break;
            case Models.ScheduledTaskTimePeriod.Month:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, scheduledTask.StartDay, scheduledTask.StartHour, scheduledTask.StartMinute, 0).AddMonths(scheduledTask.Interval);

                break;
            case Models.ScheduledTaskTimePeriod.Day:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, scheduledTask.StartHour, scheduledTask.StartMinute, 0).AddDays(scheduledTask.Interval);
                break;

            case Models.ScheduledTaskTimePeriod.WorkingDay:

                while (true)
                {
                    scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(scheduledTask.Interval);
                    if (scheduledTask.NextRunningDate.DayOfWeek != DayOfWeek.Saturday
                        && scheduledTask.NextRunningDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        break;
                    }
                    now = now.AddDays(1);
                }
                break;
            case Models.ScheduledTaskTimePeriod.Hour:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(scheduledTask.Interval);

                break;
            case Models.ScheduledTaskTimePeriod.Minute:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(scheduledTask.Interval);

                break;

            case Models.ScheduledTaskTimePeriod.Second:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second).AddSeconds(scheduledTask.Interval);
                break;
        }
    }

    private Models.ScheduledTask CreateScheduledTask(DistributedTasksOnTime.TaskRegistrationInfo taskInfo)
    {
        var task = new Models.ScheduledTask();
        task.Name = taskInfo.TaskName;
        task.Enabled = taskInfo.Enabled;
        task.AssemblyQualifiedName = taskInfo.AssemblyQualifiedName;
        task.StartedCount = 0;
        task.AllowMultipleInstance = taskInfo.AllowMultipleInstances;

        return task;
    }

}

