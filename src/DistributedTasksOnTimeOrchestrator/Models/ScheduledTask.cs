namespace DistributedTasksOnTime.Orchestrator.Models;

public class ScheduledTask 
{
	public ScheduledTask()
	{
		Parameters = new Dictionary<string, string>();
	}

    public string Name { get; set; }
	public ScheduledTaskTimePeriod Period { get; set; }
	public int Interval { get; set; }
	public int StartDay { get; set; }
	public int StartHour { get; set; }
	public int StartMinute { get; set; }
	public string AssemblyQualifiedName { get; set; }
	public int StartedCount { get; set; }
	public bool Enabled { get; set; }
	public bool AllowMultipleInstance { get; internal set; }
	public DateTime NextRunningDate { get; set; }
	public Dictionary<string, string> Parameters { get; set; }
}

