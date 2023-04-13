namespace DistributedTasksOnTime.Client.Readers;

public class CancelTaskReader : ArianeBus.MessageReaderBase<DistributedTasksOnTime.CancelTask>
{
	public CancelTaskReader(ArianeBus.IServiceBus bus,
		TasksOnTime.ITasksHost host,
		DistributedTasksOnTimeSettings settings)
	{
		this.Bus = bus;
		this.Host = host;
		this.Settings = settings;
	}

	protected ArianeBus.IServiceBus Bus { get; }
	protected TasksOnTime.ITasksHost Host { get; }
	protected DistributedTasksOnTimeSettings Settings { get; }

	public override async Task ProcessMessageAsync(DistributedTasksOnTime.CancelTask message, CancellationToken cancellationToken)
	{
		var history = Host.GetHistory(message.Id);
		if (history != null
			&& !history.TerminatedDate.HasValue)
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = message.Id;
			taskInfo.State = DistributedTasksOnTime.TaskState.Canceling;

			await Bus.EnqueueMessage(Settings.TaskInfoQueueName, taskInfo);

			Host.Cancel(message.Id);
		}
	}
}

