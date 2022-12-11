namespace DistributedTasksOnTime.BlazorComponent;

public class TaskInfo
{
	public DistributedTasksOnTime.ScheduledTask ScheduledTask { get; set; }
	public DistributedTasksOnTime.RunningTask LastRunningTask { get; set; }
}
