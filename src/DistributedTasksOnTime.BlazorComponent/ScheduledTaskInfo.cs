namespace DistributedTasksOnTime.BlazorComponent;

public class ScheduledTaskInfo
{
	public DistributedTasksOnTime.ScheduledTask ScheduledTask { get; set; }
	public DistributedTasksOnTime.RunningTask LastRunningTask { get; set; }
}
