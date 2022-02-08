﻿namespace DistributedTasksOnTime.Orchestrator.Models;

public enum ScheduledTaskTimePeriod
{
	None,
	Month,
	Day,
	WorkingDay,
	Hour,
	Minute,
    Second,
	Custom
}
