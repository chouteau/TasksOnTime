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

	public string PrefixQueueName { get; set; } = "DistributedTasksOnTime";
	public string AzureBusConnectionString { get; set; }

	public string HostName { get; set; }
	public Type ExistingProgressReporter { get; set; }

	public string HostKey => $"{System.Environment.MachineName}.{HostName}";

	internal IList<DistributedTasksOnTime.TaskRegistrationInfo> ScheduledTaskList { get; }

	public IEnumerable<DistributedTasksOnTime.TaskRegistrationInfo> RegisterScheduledTask(DistributedTasksOnTime.TaskRegistrationInfo taskInfo)
	{
		if (!ScheduledTaskList.Any(i => i.TaskName.Equals(taskInfo.TaskName, StringComparison.InvariantCultureIgnoreCase)))
		{
			ScheduledTaskList.Add(taskInfo);
		}
		return ScheduledTaskList;
	}
}

