namespace DistributedTasksOnTime.Orchestrator.Readers;

internal class HostRegistrationReader : ArianeBus.MessageReaderBase<HostRegistrationInfo>
{
	public HostRegistrationReader(ITasksOrchestrator tasksOrchestrator)
	{
		TasksOrchestrator = tasksOrchestrator;
	}

	protected ITasksOrchestrator TasksOrchestrator { get; }

	public override async Task ProcessMessageAsync(HostRegistrationInfo message, CancellationToken cancellationToken)
	{
		if (message.State == HostRegistrationState.Started)
		{
			await TasksOrchestrator.RegisterHost(message);
		}
		else if (message.State == HostRegistrationState.Stopped)
		{
			await TasksOrchestrator.UnRegisterHost(message);
		}
	}
}

