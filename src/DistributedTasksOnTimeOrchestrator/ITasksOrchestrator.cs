namespace DistributedTasksOnTime.Orchestrator;

public interface ITasksOrchestrator
{
	event Action<string> OnHostRegistered;

	void Start();
	void Stop();
	void RegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo);
	void UnRegisterHost(DistributedTasksOnTime.HostRegistrationInfo hostInfo);
	Task EnqueueNextTasks(DateTime now);
	void NotifyRunningTask(DistributedTasksOnTime.DistributedTaskInfo distributedTaskInfo);
	bool ContainsTask(string taskName);
	Task CancelTask(string taskName);
	Task ForceTask(string taskName);
	int GetScheduledTaskCount();
	int GetRunningTaskCount();
	IEnumerable<Models.ScheduledTask> GetScheduledTaskList();
}

