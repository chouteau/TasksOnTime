namespace DistributedTasksOnTime.Orchestrator.Readers;

internal class HostRegistrationReader : Ariane.MessageReaderBase<HostRegistrationInfo>
{
	public HostRegistrationReader(ITasksOrchestrator tasksOrchestrator)
	{
		TasksOrchestrator = tasksOrchestrator;
	}

	protected ITasksOrchestrator TasksOrchestrator { get; }

	public override Task ProcessMessageAsync(HostRegistrationInfo message)
	{
		if (message.State == HostRegistrationState.Started)
		{
			TasksOrchestrator.RegisterHost(message);
		}
		else if (message.State == HostRegistrationState.Stopped)
		{
			TasksOrchestrator.UnRegisterHost(message);
		}

		return Task.CompletedTask;
	}
}

