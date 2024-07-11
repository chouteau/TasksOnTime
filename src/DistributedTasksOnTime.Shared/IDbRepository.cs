namespace DistributedTasksOnTime;

public interface IDbRepository
{
	Task<List<DistributedTasksOnTime.HostRegistrationInfo>> GetHostRegistrationList(CancellationToken cancellationToken = default);
	Task SaveHostRegistration(DistributedTasksOnTime.HostRegistrationInfo hostRegistrationInfo, CancellationToken cancellationToken = default);
    Task DeleteHostRegistration(string key, CancellationToken cancellationToken = default);
    Task<List<ScheduledTask>> GetScheduledTaskList(CancellationToken cancellationToken = default);
	Task SaveScheduledTask(ScheduledTask scheduledTask, CancellationToken cancellationToken = default);
    Task DeleteScheduledTask(string name, CancellationToken cancellationToken = default);
    Task<List<RunningTask>> GetRunningTaskList(bool withHistory = false, CancellationToken cancellationToken = default);
    Task<RunningTask?> GetLastRunningTask(string taskName, CancellationToken cancellationToken = default);
    Task SaveRunningTask(RunningTask task, CancellationToken cancellationToken = default);
    Task ResetRunningTasks(CancellationToken cancellationToken = default);
    Task PersistAll(CancellationToken cancellationToken = default);
    Task SaveProgressInfo(ProgressInfo progressInfo, CancellationToken cancellationToken = default);
    Task<List<ProgressInfo>> GetProgressInfoList(Guid RunningTaskId, CancellationToken cancellationToken = default);
}

