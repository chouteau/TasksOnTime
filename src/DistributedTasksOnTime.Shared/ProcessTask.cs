namespace DistributedTasksOnTime;

public class ProcessTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public string? FullTypeName { get; set; } = default!;
    public string? TaskName { get; set; } = default!;
    public bool AllowMultipleInstances { get; set; }
    public bool IsForced { get; set; } = false;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
