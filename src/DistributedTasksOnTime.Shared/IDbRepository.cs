namespace DistributedTasksOnTime;

public interface IDbRepository
{
	public List<DistributedTasksOnTime.HostRegistrationInfo> GetHostRegistrationList();
	public void SaveHostRegistration(DistributedTasksOnTime.HostRegistrationInfo hostRegistrationInfo);
    public void DeleteHostRegistration(string key);
    public List<ScheduledTask> GetScheduledTaskList();
	public void SaveScheduledTask(ScheduledTask scheduledTask);
    public void DeleteScheduledTask(string name);
    public List<RunningTask> GetRunningTaskList(bool withProgress = false);
    public void SaveRunningTask(RunningTask task);
    public void ResetRunningTasks();
    public void PersistAll();
    void SaveProgressInfo(ProgressInfo progressInfo);
}

