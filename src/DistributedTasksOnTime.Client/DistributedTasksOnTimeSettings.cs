namespace DistributedTasksOnTime.Client;

public class DistributedTasksOnTimeSettings
{
	public DistributedTasksOnTimeSettings()
	{
		ScheduledTaskList = new List<DistributedTasksOnTime.TaskRegistrationInfo>();
	}

	public string TaskInfoQueueName { get; set; }
	public string HostRegistrationQueueName { get; set; }
	public string CancelTaskQueueName { get; set; }
    public string ForceTaskQueueName { get; set; }
	public string CheckTaskIsRunningQueueName { get; set; }

    public string PrefixQueueName { get; set; } = "DistributedTasksOnTime";
	public string AzureBusConnectionString { get; set; }

	public string HostName { get; set; }
	
	public Type ExistingProgressReporter { get; set; }

	public string HostKey => $"{System.Environment.MachineName}.{HostName}";

	public LogLevel ProgressReporterLogLevel { get; set; } = LogLevel.Trace;

    internal IList<DistributedTasksOnTime.TaskRegistrationInfo> ScheduledTaskList { get; }

	public DistributedTasksOnTimeSettings RegisterScheduledTask<T>(DistributedTasksOnTime.TaskRegistrationInfo taskInfo)
		where T : ITask
	{
		if (!ScheduledTaskList.Any(i => i.TaskName.Equals(taskInfo.TaskName, StringComparison.InvariantCultureIgnoreCase)))
		{
			var assemblyQualifiedName = typeof(T).AssemblyQualifiedName;
			var parts = assemblyQualifiedName.Split(',');
			taskInfo.AssemblyQualifiedName = $"{parts[0]},{parts[1]}";
			ScheduledTaskList.Add(taskInfo);
		}
		return this;
	}
}

