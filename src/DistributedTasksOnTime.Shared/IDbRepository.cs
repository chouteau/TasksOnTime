namespace DistributedTasksOnTime;

public interface IDbRepository
{
	Task<List<DistributedTasksOnTime.HostRegistrationInfo>> GetHostRegistrationList();
	Task SaveHostRegistration(DistributedTasksOnTime.HostRegistrationInfo hostRegistrationInfo);
    Task DeleteHostRegistration(string key);
    Task<List<ScheduledTask>> GetScheduledTaskList();
	Task SaveScheduledTask(ScheduledTask scheduledTask);
    Task DeleteScheduledTask(string name);
    Task<List<RunningTask>> GetRunningTaskList(bool withProgress = false);
    Task SaveRunningTask(RunningTask task);
    Task ResetRunningTasks();
    Task PersistAll();
    Task SaveProgressInfo(ProgressInfo progressInfo);
}

