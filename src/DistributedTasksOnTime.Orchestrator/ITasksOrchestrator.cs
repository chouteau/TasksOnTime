namespace DistributedTasksOnTime.Orchestrator;

public interface ITasksOrchestrator
{
	event Action<string> OnHostRegistered;
	event Action<TaskState, RunningTask> OnRunningTaskChanged;
	event Action<string> OnScheduledTaskStarted;

	Task Start();
	Task Stop();
	Task RegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo);
	Task UnRegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo);
	Task EnqueueNextTasks(DateTime now);
	Task NotifyRunningTask(DistributedTasksOnTime.DistributedTaskInfo distributedTaskInfo);
	Task<bool> ContainsTask(string taskName);
	Task CancelTask(string taskName);
	Task DeleteTask(string taskName);
	Task TerminateTask(string taskName);
	Task TerminateOldTasks();

	Task ForceTask(string taskName, Dictionary<string, string> parameters);
	Task<int>  GetScheduledTaskCount();
	Task SaveScheduledTask(ScheduledTask scheduledTask);
	Task<int> GetRunningTaskCount();
	Task<IEnumerable<RunningTask>> GetRunningTaskList(string taskName = null, bool withHistory = false);
    Task<RunningTask> GetLastRunningTask(string taskName);
    Task ResetRunningTasks();
	Task<IEnumerable<ScheduledTask>> GetScheduledTaskList();
}

