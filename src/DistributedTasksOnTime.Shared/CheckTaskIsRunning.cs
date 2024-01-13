namespace DistributedTasksOnTime;

public class CheckTaskIsRunning
{
    public Guid TaskId { get; set; }
    public string ScheduledTaskName { get; set; } = null!;
    public DateTime Timeout { get; set; } = DateTime.Now.AddSeconds(30);
}
