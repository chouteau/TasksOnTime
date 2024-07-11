namespace DistributedTasksOnTime;

public class TaskRegistrationInfo
{
    public string? TaskName { get; set; } = default!;
    public string? AssemblyQualifiedName { get; set; } = default!;
    public bool AllowMultipleInstances { get; set; } = false;
    public bool AllowLocalMultipleInstances { get; set; } = false;
    public string? Description { get; set; } = default!;
    public ScheduledTaskTimePeriod DefaultPeriod { get; set; }
    public int DelayedStartInSecond { get; set; } = 0;
    public int DefaultInterval { get; set; }
    public int DefaultStartDay { get; set; } = 1;
    public int DefaultStartHour { get; set; }
    public int DefaultStartMinute { get; set; }
    public ProcessMode ProcessMode { get; set; } = ProcessMode.Exclusive;
    public bool Enabled { get; set; } = true;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
