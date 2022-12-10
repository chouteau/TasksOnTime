namespace DistributedTasksOnTime.Persistence;

public interface IDbRepository
{
	public List<DistributedTasksOnTime.HostRegistrationInfo> GetHostRegistrationList();
	public void PersistHostRegistrationList(List<DistributedTasksOnTime.HostRegistrationInfo> list);
	public List<Models.ScheduledTask> GetScheduledTaskList();
	public void PersistScheduledTaskList(List<Models.ScheduledTask> list);
}

