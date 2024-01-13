namespace DistributedTasksOnTime.Client.Readers;

public class ProcessTaskReader : ArianeBus.MessageReaderBase<DistributedTasksOnTime.ProcessTask>
{
	public ProcessTaskReader(ArianeBus.IServiceBus bus,
		TasksOnTime.ITasksHost host,
		DistributedTasksOnTimeSettings settings,
		ILogger<ProcessTaskReader> logger)
	{
		this.Bus = bus;
		this.Host = host;	
		this.Settings = settings;
		this.Logger = logger;
	}

	protected ArianeBus.IServiceBus Bus { get; }
	protected TasksOnTime.ITasksHost Host { get; }
	protected DistributedTasksOnTimeSettings Settings { get; }
	protected ILogger<ProcessTaskReader> Logger { get; }

	public override async Task ProcessMessageAsync(DistributedTasksOnTime.ProcessTask message, CancellationToken cancellationToken)
	{
		var type = Type.GetType(message.FullTypeName);
		if (type is null)
		{
			throw new ArgumentException($"The type is not found {message.FullTypeName}");
		}
		Dictionary<string, object> parameters = new();
		if (message.Parameters != null)
		{
			foreach (var item in message.Parameters)
			{
				parameters.Add(item.Key, item.Value);
			}
		}
		await Host.Enqueue(message.Id, type, force: message.IsForced, inputParameters: parameters);

		var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
		taskInfo.Id = message.Id;
		taskInfo.HostKey = Settings.HostKey;
		taskInfo.State = DistributedTasksOnTime.TaskState.Enqueued;

		await Bus.EnqueueMessage(Settings.TaskInfoQueueName, taskInfo);
	}
}