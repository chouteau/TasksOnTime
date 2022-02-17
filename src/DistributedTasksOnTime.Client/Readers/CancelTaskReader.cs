namespace DistributedTasksOnTime.Client.Readers;

public class CancelTaskReader : Ariane.MessageReaderBase<DistributedTasksOnTime.CancelTask>
{
	public CancelTaskReader(Ariane.IServiceBus bus,
		TasksOnTime.ITasksHost host,
		DistributedTasksOnTimeSettings settings)
	{
		this.Bus = bus;
		this.Host = host;
		this.Settings = settings;
	}

	protected Ariane.IServiceBus Bus { get; }
	protected TasksOnTime.ITasksHost Host { get; }
	protected DistributedTasksOnTimeSettings Settings { get; }

	public override async Task ProcessMessageAsync(DistributedTasksOnTime.CancelTask message)
	{
		var history = Host.GetHistory(message.Id);
		if (history != null
			&& !history.TerminatedDate.HasValue)
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = message.Id;
			taskInfo.State = DistributedTasksOnTime.TaskState.Canceling;

			await Bus.SendAsync(Settings.TaskInfoQueueName, taskInfo);

			Host.Cancel(message.Id);
		}
	}
}

