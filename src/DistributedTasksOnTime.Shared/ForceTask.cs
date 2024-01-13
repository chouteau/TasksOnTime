namespace DistributedTasksOnTime;

public class ForceTask
{
    public string TaskName { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}
