﻿namespace DistributedTasksOnTime.Orchestrator;

public class DistributedTasksOnTimeServerSettings
{
	public string TaskInfoQueueName { get; set; }
	public string HostRegistrationQueueName { get; set; }
	public string CancelTaskQueueName { get; set; }

	public string PrefixQueueName { get; set; } = "DistributedTasksOnTime";
	public int TimerInSecond { get; set; } = 15;
	public string AzureBusConnectionString { get; set; }
	public string StoreFolder { get; set; } = @".\DistributedTasksOnTimeServer";
	public string ScheduledTaskListBlazorPage { get; set; } = "/scheduledtasklist";
}

