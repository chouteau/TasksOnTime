namespace DistributedTasksOnTime.Orchestrator;

public interface ITasksOrchestrator
{
	event Action<string> OnHostRegistered;
	event Action<TaskState, Persistence.Models.RunningTask> OnRunningTaskChanged;
	event Action<string> OnScheduledTaskStarted;

	void Start();
	void Stop();
	void RegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo);
	void UnRegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo);
	Task EnqueueNextTasks(DateTime now);
	void NotifyRunningTask(DistributedTasksOnTime.DistributedTaskInfo distributedTaskInfo);
	bool ContainsTask(string taskName);
	Task CancelTask(string taskName);
	Task DeleteTask(string taskName);

	Task ForceTask(string taskName);
	int GetScheduledTaskCount();
	void SaveScheduledTaskList(Persistence.Models.ScheduledTask scheduledTask = null);
	int GetRunningTaskCount();
	IEnumerable<Persistence.Models.RunningTask> GetRunningTaskList(string taskName = null);
	IEnumerable<Persistence.Models.ScheduledTask> GetScheduledTaskList();
}

