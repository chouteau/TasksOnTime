namespace DistributedTasksOnTime.Orchestrator.Readers;

public class TaskInfoReader : Ariane.MessageReaderBase<DistributedTasksOnTime.DistributedTaskInfo>
{
	public TaskInfoReader(ILogger<TaskInfoReader> logger,
		ITasksOrchestrator tasksOrchestrator)
	{
		this.Logger = logger;
		this.TasksOrchestrator = tasksOrchestrator;
	}

	protected ILogger Logger { get; }
	protected ITasksOrchestrator TasksOrchestrator { get; }

	public override Task ProcessMessageAsync(DistributedTaskInfo message)
	{
		TasksOrchestrator.NotifyRunningTask(message);
		return Task.CompletedTask;
	}
}

