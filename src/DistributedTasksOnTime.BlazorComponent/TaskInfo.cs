namespace DistributedTasksOnTime.BlazorComponent;

public class TaskInfo
{
	public Orchestrator.Models.ScheduledTask ScheduledTask { get; set; }
	public Orchestrator.Models.RunningTask LastRunningTask { get; set; }
}
