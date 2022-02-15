namespace DistributedTasksOnTime.Client.Readers;

public class ProcessTaskReader : Ariane.MessageReaderBase<DistributedTasksOnTime.ProcessTask>
{
	public ProcessTaskReader(Ariane.IServiceBus bus,
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

	public override Task ProcessMessageAsync(DistributedTasksOnTime.ProcessTask message)
	{
		var type = Type.GetType(message.FullTypeName);
		Host.Enqueue(message.Id, type, force : message.IsForced);

		var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
		taskInfo.Id = message.Id;
		taskInfo.HostKey = Settings.HostKey;
		taskInfo.State = DistributedTasksOnTime.TaskState.Enqueued;

		Bus.Send(Settings.TaskInfoQueueName, taskInfo);

		return Task.CompletedTask;
	}
}