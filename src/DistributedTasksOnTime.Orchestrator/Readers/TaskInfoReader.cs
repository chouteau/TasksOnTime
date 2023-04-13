namespace DistributedTasksOnTime.Orchestrator.Readers;

public class TaskInfoReader : ArianeBus.MessageReaderBase<DistributedTasksOnTime.DistributedTaskInfo>
{
	public TaskInfoReader(ILogger<TaskInfoReader> logger,
		ITasksOrchestrator tasksOrchestrator)
	{
		this.Logger = logger;
		this.TasksOrchestrator = tasksOrchestrator;
	}

	protected ILogger Logger { get; }
	protected ITasksOrchestrator TasksOrchestrator { get; }

	public override async Task ProcessMessageAsync(DistributedTaskInfo message, CancellationToken cancellationToken)
	{
		await TasksOrchestrator.NotifyRunningTask(message);
	}
}

