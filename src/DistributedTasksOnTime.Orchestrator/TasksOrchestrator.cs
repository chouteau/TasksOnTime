using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace DistributedTasksOnTime.Orchestrator;

internal class TasksOrchestrator : ITasksOrchestrator
{
    public event Action<string> OnHostRegistered;
    public event Action<TaskState, RunningTask> OnRunningTaskChanged;
    public event Action<string> OnScheduledTaskStarted;

    private readonly Dictionary<Guid, CheckTaskIsRunning> _checkTaskIsRunningList = new();

    public TasksOrchestrator(DistributedTasksOnTimeServerSettings scheduleSettings,
        ILogger<TasksOrchestrator> logger,
        IDbRepository dbRepository,
        ArianeBus.IServiceBus bus)
    {
        this.Settings = scheduleSettings;
        this.Logger = logger;
        this.DbRepository = dbRepository; 
        this.Bus = bus;
    }

    protected DistributedTasksOnTimeServerSettings Settings { get; }
    protected ILogger Logger { get; }
    protected IDbRepository DbRepository { get; }
    protected ArianeBus.IServiceBus Bus { get; }

    public Task Start()
	{
        return Task.CompletedTask;
    }

    public async Task Stop()
	{
        Logger.LogWarning("TasksOrchestrator stopping");
        await DbRepository.PersistAll();
	}

    public async Task RegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo)
	{
        foreach (var task in hostInfo.TaskList)
		{
            var scheduledTask = CreateScheduledTask(task);
            await DbRepository.SaveScheduledTask(scheduledTask);
        }
        await DbRepository.SaveHostRegistration(hostInfo);

        OnHostRegistered?.Invoke(hostInfo.Key);
    }

    public async Task UnRegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo)
	{
        await DbRepository.DeleteHostRegistration(hostInfo.Key);
    }

    public async Task NotifyRunningTask(DistributedTasksOnTime.DistributedTaskInfo distributedTaskInfo)
	{
        var runningTask = (await DbRepository.GetRunningTaskList(true)).SingleOrDefault(i => i.Id == distributedTaskInfo.Id);
        if (runningTask == null) // <- pas normal
		{
            Logger.LogWarning("Running task not found with id {Id} {TaskName} {State} {Subject}", 
                distributedTaskInfo.Id, 
                distributedTaskInfo.TaskName,
                distributedTaskInfo.State,
                distributedTaskInfo?.ProgressInfo?.Subject);
            runningTask = new RunningTask();
            runningTask.Id = distributedTaskInfo.Id;
            runningTask.TaskName = "unknown";
		}

        var scheduledTask = (await DbRepository.GetScheduledTaskList()).SingleOrDefault(i => i.Name.Equals(runningTask.TaskName, StringComparison.InvariantCultureIgnoreCase));
        if (scheduledTask == null)
		{
            // Pas normal du tout
            Logger.LogTrace("Running task without scheduledTask {0}", distributedTaskInfo.Id);
            return;
        }

        if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Enqueued)
		{
            Logger.LogTrace("Task {0} enqueued", runningTask.TaskName);
            runningTask.HostKey = distributedTaskInfo.HostKey;
            runningTask.EnqueuedDate = distributedTaskInfo.EventDate;
		}
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Started)
		{
            Logger.LogTrace("Task {0} started", runningTask.TaskName);
            runningTask.HostKey = distributedTaskInfo.HostKey;
            runningTask.RunningDate = distributedTaskInfo.EventDate;

            scheduledTask.StartedCount = scheduledTask.StartedCount + 1;
            await DbRepository.SaveScheduledTask(scheduledTask);

            OnScheduledTaskStarted?.Invoke(runningTask.TaskName);
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Terminated)
		{
            Logger.LogTrace("Task {0} terminated", runningTask.TaskName);
            runningTask.HostKey = distributedTaskInfo.HostKey;
            runningTask.TerminatedDate = distributedTaskInfo.EventDate;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Canceling)
        {
            Logger.LogTrace("Task {0} canceling", runningTask.TaskName);
            runningTask.HostKey = distributedTaskInfo.HostKey;
            runningTask.CancelingDate = distributedTaskInfo.EventDate;
        }
        else if (distributedTaskInfo.State ==  DistributedTasksOnTime.TaskState.Canceled)
		{
            Logger.LogTrace("Task {0} canceled", runningTask.TaskName);
            runningTask.HostKey = distributedTaskInfo.HostKey;
            runningTask.CanceledDate = distributedTaskInfo.EventDate;
            runningTask.TerminatedDate = DateTime.Now;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Failed)
        {
            Logger.LogTrace("Task {0} failed with stack {1}", runningTask.TaskName, distributedTaskInfo.ErrorStack);
            runningTask.HostKey = distributedTaskInfo.HostKey;
            runningTask.FailedDate = distributedTaskInfo.EventDate;
            runningTask.TerminatedDate = DateTime.Now;
            runningTask.ErrorStack = distributedTaskInfo.ErrorStack;
        }
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.Progress)
		{
            Logger.LogTrace("Task {0} progress", runningTask.TaskName);
            if (distributedTaskInfo.ProgressInfo == null)
			{
                Logger.LogWarning("Progress event without info");
			}
            else
			{
                SanitizeProgressInfo(distributedTaskInfo.ProgressInfo);
                runningTask.ProgressLogs.Add(distributedTaskInfo.ProgressInfo);
                await DbRepository.SaveProgressInfo(distributedTaskInfo.ProgressInfo);
            }
		}
        else if (distributedTaskInfo.State == DistributedTasksOnTime.TaskState.RunningConfirmed)
        {
            Logger.LogTrace("Task {0} running confirmed", runningTask.TaskName);
            // On supprime l'entrée dans la demande de vérification
            _checkTaskIsRunningList.Remove(distributedTaskInfo.Id, out var checkTaskIsRunning);
        }

        await DbRepository.SaveRunningTask(runningTask);

        OnRunningTaskChanged?.Invoke(distributedTaskInfo.State, runningTask);
    }

    private void SanitizeProgressInfo(ProgressInfo progressInfo)
    {
        if (progressInfo.Subject is not null)
        {
            progressInfo.Subject = progressInfo.Subject.Trim();
            progressInfo.Subject = progressInfo.Subject.Trim().Substring(0, Math.Min(progressInfo.Subject.Length, 500));
        }
        if (progressInfo.Body is not null)
        {
            progressInfo.Body = progressInfo.Body.Trim();
            progressInfo.Body = progressInfo.Body.Trim().Substring(0, Math.Min(progressInfo.Body.Length, 1024));
        }
        if (progressInfo.GroupName is not null)
        {
            progressInfo.GroupName = progressInfo.GroupName.Trim();
            progressInfo.GroupName = progressInfo.GroupName.Trim().Substring(0, Math.Min(progressInfo.GroupName.Length, 100));
        }
    }

    public async Task<bool> ContainsTask(string taskName)
    {
        if (taskName == null)
		{
            throw new NullReferenceException("taskName is null");
		}

        bool result = false;
        result = (await DbRepository.GetScheduledTaskList()).SingleOrDefault(i => i.Name.Equals(taskName, StringComparison.InvariantCultureIgnoreCase)) != null;
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
        var task = (await DbRepository.GetRunningTaskList()).Find(i => taskName.Equals(i.TaskName, StringComparison.InvariantCultureIgnoreCase)
                            && !i.TerminatedDate.HasValue);

        if (task != null)
		{
            var cancelTask = new DistributedTasksOnTime.CancelTask();
            cancelTask.Id = task.Id;
            await Bus.PublishTopic(Settings.CancelTaskQueueName, cancelTask);
        }
    }

	public async Task TerminateTask(string taskName)
	{
		if (taskName == null)
		{
			throw new NullReferenceException("task name is null");
		}

		taskName = taskName.ToLower();

		Logger.LogInformation("Try to Cancel task {0}", taskName);
		var runningTask = (await DbRepository.GetRunningTaskList()).Find(i => taskName.Equals(i.TaskName, StringComparison.InvariantCultureIgnoreCase)
							&& !i.TerminatedDate.HasValue);

        if (runningTask is null)
        {
            return;
        }
		runningTask.TerminatedDate = DateTime.Now;
		runningTask.IsForced = true;

		await DbRepository.SaveRunningTask(runningTask);

		OnRunningTaskChanged?.Invoke(TaskState.Terminated, runningTask);
	}


	public async Task DeleteTask(string taskName)
    {
        if (taskName == null)
        {
            throw new NullReferenceException("task name is null");
        }

        taskName = taskName.ToLower();

        Logger.LogInformation("Try to Delete task {0}", taskName);
        await DbRepository.DeleteScheduledTask(taskName);
    }

    public async Task ForceTask(string taskName, Dictionary<string,string> parameters)
    {
        if (string.IsNullOrWhiteSpace(taskName))
        {
            throw new NullReferenceException("task name is null or empty");
        }

        Logger.LogInformation("Try to force task {0}", taskName);
            
        var scheduledTask = (await DbRepository.GetScheduledTaskList()).SingleOrDefault(i => i.Name.Equals(taskName, StringComparison.InvariantCultureIgnoreCase));
        if (scheduledTask == null)
        {
            Logger.LogWarning("force unknown task {0}", taskName);
            return;
        }
		var runningTask = (await DbRepository.GetRunningTaskList()).Find(i => i.TaskName.Equals(taskName, StringComparison.InvariantCultureIgnoreCase));
        if (runningTask!= null
            && !runningTask.TerminatedDate.HasValue
            && !scheduledTask.AllowMultipleInstance)
		{
            Logger.LogWarning("force task {taskName} canceled because task is already started and not completed ", taskName);
            return;
        }

        await EnqueueTask(new EnqueueTaskItem
        {
            ScheduledTask = scheduledTask,
            Parameters = parameters,
            Force = true
        });
    }

    public async Task<int> GetScheduledTaskCount()
    {
        return (await DbRepository.GetScheduledTaskList()).Count;
    }

    public async Task<int> GetRunningTaskCount()
    {
        return (await DbRepository.GetRunningTaskList()).Count(i => !i.TerminatedDate.HasValue);
    }

    public async Task<IEnumerable<ScheduledTask>> GetScheduledTaskList()
    {
        return await DbRepository.GetScheduledTaskList();
    }

    public async Task<IEnumerable<RunningTask>> GetRunningTaskList(string taskName = null, bool withProgress = false, bool withHistory = false)
    {
        if (taskName == null)
        {
            return await DbRepository.GetRunningTaskList(withProgress, withHistory);
        }
        return (await DbRepository.GetRunningTaskList(withProgress, withHistory)).Where(i => i.TaskName.Equals(taskName, StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task ResetRunningTasks()
    {
        await DbRepository.ResetRunningTasks();
    }

    public async Task SaveScheduledTask(ScheduledTask scheduledTask)
	{
        await DbRepository.SaveScheduledTask(scheduledTask);
	}

    /// <summary>
    /// Recupération de la première tache executable 
    /// dans l'ordre FIFO
    /// </summary>
    /// <returns></returns>
    public async virtual Task EnqueueNextTasks(DateTime now)
    {
        var taskList = await DbRepository.GetScheduledTaskList();
		var query = from t in taskList
                    select t;

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
                if (!await CanRun(now, task))
                {
                    continue;
                }
                Logger.LogDebug("Try to start scheduled task {0}", task.Name);
                await EnqueueTask(new EnqueueTaskItem
                {
                    ScheduledTask = task,
                    Parameters = null,
                    Force = false
                });
                SetNextRuningDate(DateTime.Now, task);
                await DbRepository.SaveScheduledTask(task);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskName", task.Name);
                Logger.LogError(ex, ex.Message);
            }
        }
    }

    public async Task TerminateOldTasks()
    {
		var runningTasks = await DbRepository.GetRunningTaskList(false, true);

        // 1 - Recherche des taches en cours non terminées en doublons 
        var oldNotTerminatedTask = from rt in runningTasks
                                   where !rt.TerminatedDate.HasValue
                                   group rt by new { rt.TaskName } into g
                                   where g.Count() > 1
                                   select g.Key.TaskName;

        foreach (var task in oldNotTerminatedTask)
        { 
            // On termine toutes les taches sauf la plus récente
            var oldTask = runningTasks.Where(i => i.TaskName == task 
                                            && !i.TerminatedDate.HasValue)
                            .OrderByDescending(i => i.CreationDate)
                            .Skip(1)
                            .ToList();

            foreach (var item in oldTask)
            {
                item.TerminatedDate = DateTime.Now;
                item.IsForced = true;

                await DbRepository.SaveRunningTask(item);
            }
        }

        var scheduledTasks = await DbRepository.GetScheduledTaskList();
        runningTasks = await DbRepository.GetRunningTaskList(true, false);

        foreach (var runningTask in runningTasks)
        {
            // On verifie que la tache en cours à bien de l'activité
            if (runningTask.ProgressLogs.Exists(i => i.CreationDate > DateTime.Now.AddMinutes(-1)))
            {
                continue;
            }

            _checkTaskIsRunningList.TryGetValue(runningTask.Id, out var checkTaskIsRunning);
            if (checkTaskIsRunning is not null)
            {
                // Si la demande est encore présente et que sa date d'expiration est passée 
                // C'est qu'aucun client ne traite la tache, on la termine
                if (checkTaskIsRunning.Timeout < DateTime.Now)
                {
                    await TerminateTask(runningTask.TaskName);
                }
                continue;
            }

            var scheduledTask = scheduledTasks.Single(i => i.Name == runningTask.TaskName);
            var verifyTask = new ScheduledTask()
            {
                Period = scheduledTask.Period,
                Interval = scheduledTask.Interval,
                StartDay = scheduledTask.StartDay,
                StartHour = scheduledTask.StartHour,
                StartMinute = scheduledTask.StartMinute,
            };
            // 2 Cycles
            SetNextRuningDate(runningTask.CreationDate, verifyTask);
            SetNextRuningDate(verifyTask.NextRunningDate, verifyTask);
            if (verifyTask.NextRunningDate < DateTime.Now)
            {
                // Envoyer aux clients une vérification de tache en cours
                checkTaskIsRunning = new DistributedTasksOnTime.CheckTaskIsRunning();
                checkTaskIsRunning.TaskId = runningTask.Id;
                checkTaskIsRunning.ScheduledTaskName = runningTask.TaskName;

                await Bus.PublishTopic(Settings.CheckTaskIsRunningQueueName, checkTaskIsRunning);
                _checkTaskIsRunningList.TryAdd(checkTaskIsRunning.TaskId, checkTaskIsRunning);
            }
        }

	}

	internal async Task EnqueueTask(EnqueueTaskItem enqueueTaskItem)
    {
        var procesTask = new DistributedTasksOnTime.ProcessTask();
        procesTask.Id = Guid.NewGuid();
        procesTask.CreationDate = DateTime.Now;
        procesTask.TaskName = enqueueTaskItem.ScheduledTask.Name;
        procesTask.FullTypeName = enqueueTaskItem.ScheduledTask.AssemblyQualifiedName;
        procesTask.AllowMultipleInstances = enqueueTaskItem.ScheduledTask.AllowLocalMultipleInstances;
        procesTask.IsForced = enqueueTaskItem.Force;
        procesTask.Parameters = enqueueTaskItem.Parameters ?? enqueueTaskItem.ScheduledTask.Parameters;

        var queueName = $"{Settings.PrefixQueueName}.{enqueueTaskItem.ScheduledTask.Name}";
        if (enqueueTaskItem.ScheduledTask.ProcessMode == ProcessMode.Exclusive)
        {
			await Bus.EnqueueMessage(queueName, procesTask);
		}
        else
        {
            await Bus.PublishTopic(queueName, procesTask);
        }

		var runningTask = new RunningTask();
        runningTask.Id = procesTask.Id;
        runningTask.TaskName = enqueueTaskItem.ScheduledTask.Name;
        runningTask.IsForced = enqueueTaskItem.Force;

        await DbRepository.SaveRunningTask(runningTask);
    }

    internal async Task<bool> CanRun(DateTime now, ScheduledTask scheduledTask)
    {
        if (!scheduledTask.Enabled)
		{
            return false;
		}

        var runningTask = (await DbRepository.GetRunningTaskList())
                            .OrderByDescending(i => i.CreationDate)
                            .FirstOrDefault(i => i.TaskName == scheduledTask.Name);

        if (!scheduledTask.AllowMultipleInstance
            && runningTask != null
            && !runningTask.TerminatedDate.HasValue)
        {
            return false;
        }

        if (scheduledTask.Period ==  ScheduledTaskTimePeriod.None)
		{
            return false;
		}

        if (scheduledTask.Period == ScheduledTaskTimePeriod.WorkingDay
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

    internal void SetNextRuningDate(DateTime now, ScheduledTask scheduledTask)
    {
        switch (scheduledTask.Period)
        {
            case ScheduledTaskTimePeriod.None:

                scheduledTask.NextRunningDate = DateTime.MinValue;

                break;
            case ScheduledTaskTimePeriod.Month:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, scheduledTask.StartDay, scheduledTask.StartHour, scheduledTask.StartMinute, 0).AddMonths(scheduledTask.Interval);

                break;
            case ScheduledTaskTimePeriod.Day:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, scheduledTask.StartHour, scheduledTask.StartMinute, 0).AddDays(scheduledTask.Interval);
                break;

            case ScheduledTaskTimePeriod.WorkingDay:

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
            case ScheduledTaskTimePeriod.Hour:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(scheduledTask.Interval);

                break;
            case ScheduledTaskTimePeriod.Minute:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(scheduledTask.Interval);

                break;

            case ScheduledTaskTimePeriod.Second:

                scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second).AddSeconds(scheduledTask.Interval);
                break;
        }
    }

    private ScheduledTask CreateScheduledTask(DistributedTasksOnTime.TaskRegistrationInfo taskInfo)
    {
        var task = new ScheduledTask();
        task.Name = taskInfo.TaskName;
        task.Enabled = taskInfo.Enabled;
        task.AssemblyQualifiedName = taskInfo.AssemblyQualifiedName;
        task.StartedCount = 0;
        task.AllowMultipleInstance = taskInfo.AllowMultipleInstances;
        task.AllowLocalMultipleInstances = taskInfo.AllowLocalMultipleInstances;
        task.Period = taskInfo.DefaultPeriod;
        task.Interval = taskInfo.DefaultInterval;
        task.Description = taskInfo.Description;
        task.Parameters = taskInfo.Parameters;
        task.StartDay = taskInfo.DefaultStartDay;
        task.StartHour = taskInfo.DefaultStartHour;
        task.StartMinute = taskInfo.DefaultStartMinute;
        task.ProcessMode = taskInfo.ProcessMode;

        return task;
    }

}

