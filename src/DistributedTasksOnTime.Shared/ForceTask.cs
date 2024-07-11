namespace DistributedTasksOnTime;

public class ForceTask
{
    public string? TaskName { get; set; } = default!;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
