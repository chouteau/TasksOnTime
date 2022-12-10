namespace DistributedTasksOnTime.BlazorComponent;

public class TaskInfo
{
	public Persistence.Models.ScheduledTask ScheduledTask { get; set; }
	public Persistence.Models.RunningTask LastRunningTask { get; set; }
}
