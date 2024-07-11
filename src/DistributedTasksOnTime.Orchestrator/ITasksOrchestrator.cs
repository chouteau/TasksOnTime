namespace DistributedTasksOnTime.Orchestrator;

public interface ITasksOrchestrator
{
	event Action<string> OnHostRegistered;
	event Action<TaskState, RunningTask> OnRunningTaskChanged;
	event Action<string> OnScheduledTaskStarted;

	Task Start(CancellationToken cancellationToken = default);
	Task Stop(CancellationToken cancellationToken = default);
	Task RegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo, CancellationToken cancellationToken = default);
	Task UnRegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo, CancellationToken cancellationToken = default);
	Task EnqueueNextTasks(DateTime now, CancellationToken cancellationToken = default);
	Task NotifyRunningTask(DistributedTasksOnTime.DistributedTaskInfo distributedTaskInfo, CancellationToken cancellationToken = default);
	Task<bool> ContainsTask(string taskName, CancellationToken cancellationToken = default);
	Task CancelTask(string taskName, CancellationToken cancellationToken = default);
	Task DeleteTask(string taskName, CancellationToken cancellationToken = default);
	Task TerminateTask(string taskName, CancellationToken cancellationToken = default);
	Task TerminateOldTasks(CancellationToken cancellationToken = default);

	Task ForceTask(string taskName, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
	Task<int>  GetScheduledTaskCount(CancellationToken cancellationToken = default);
	Task SaveScheduledTask(ScheduledTask scheduledTask, CancellationToken cancellationToken = default);
	Task<int> GetRunningTaskCount(CancellationToken cancellationToken = default);
	Task<IEnumerable<RunningTask>> GetRunningTaskList(string taskName = null, bool withHistory = false, CancellationToken cancellationToken = default);
    Task<RunningTask> GetLastRunningTask(string taskName, CancellationToken cancellationToken = default);
    Task ResetRunningTasks(CancellationToken cancellationToken = default);
	Task<IEnumerable<ScheduledTask>> GetScheduledTaskList(CancellationToken cancellationToken = default);
}

