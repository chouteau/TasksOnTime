namespace DistributedTasksOnTime;

public class ProgressInfo
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public DateTime CreationDate { get; set; } = DateTime.Now;
    public Guid TaskId { get; set; }
    public ProgressType Type { get; set; }
    public string? GroupName { get; set; } = default!;
    public string? Subject { get; set; } = default!;
    public string? Body { get; set; } = default!;
    public string? EntityName { get; set; } = default!;
    public string? EntityId { get; set; } = default!;
    public object Entity { get; set; } = default!;
    public int? TotalCount { get; set; }
    public int? Index { get; set; }
}
