namespace DistributedTasksOnTime;

public class DistributedTaskInfo
{
    public Guid Id { get; set; }
    public string? TaskName { get; set; } = default!;
    public string? HostKey { get; set; } = default!;
    public TaskState State { get; set; }
    public string? ErrorStack { get; set; } = default!;
    public DateTime EventDate { get; set; } = DateTime.Now;
    public ProgressInfo ProgressInfo { get; set; } = new();
}
