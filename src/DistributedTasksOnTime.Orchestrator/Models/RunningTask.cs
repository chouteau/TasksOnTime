﻿namespace DistributedTasksOnTime.Orchestrator.Models;

public class RunningTask
{
	public Guid Id { get; set; }
	public string TaskName { get; set; }

	public string HostKey { get; set; }
	public DateTime CreationDate { get; set; } = DateTime.Now;
	public DateTime? EnqueuedDate { get; set; }
	public DateTime? RunningDate { get; set; }
	public DateTime? CancelingDate { get; set; }
	public DateTime? CanceledDate { get; set; }
	public DateTime? TerminatedDate { get; set; }
	public DateTime? FailedDate { get; set; }
	public string ErrorStack { get; set; }
	public bool IsForced { get; set; }
	public List<ProgressInfo> ProgressLogs { get; set; } = new();
}

