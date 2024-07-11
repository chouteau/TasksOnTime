namespace DistributedTasksOnTime;

public class RunningTask
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string? TaskName { get; set; } = default!;
	public string? HostKey { get; set; } = default!;
	public DateTime CreationDate { get; set; } = DateTime.Now;
	public DateTime? EnqueuedDate { get; set; }
	public DateTime? RunningDate { get; set; }
	public DateTime? CancelingDate { get; set; }
	public DateTime? CanceledDate { get; set; }
	public DateTime? TerminatedDate { get; set; }
	public DateTime? FailedDate { get; set; }
	public DateTime? LastUpdate { get; set; }
	public string? ErrorStack { get; set; } = default!;
	public bool IsForced { get; set; }
}

