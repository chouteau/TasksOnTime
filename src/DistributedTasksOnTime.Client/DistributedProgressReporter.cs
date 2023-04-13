namespace DistributedTasksOnTime.Client;

public class DistributedProgressReporter : TasksOnTime.IProgressReporter
{
	public DistributedProgressReporter(ILogger<DistributedProgressReporter> logger,
		ArianeBus.IServiceBus bus,
		DistributedTasksOnTimeSettings settings)
	{
		this.Logger = logger;	
		this.Bus = bus;
		this.Settings = settings;
	}

	protected ILogger Logger { get; }
	protected ArianeBus.IServiceBus Bus { get; }
	protected DistributedTasksOnTimeSettings Settings { get; }

	public async Task Notify(TasksOnTime.ProgressInfo info)
	{
		Logger.Log(Settings.ProgressReporterLogLevel, info.Subject);

		var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
		taskInfo.Id = info.TaskId;
		taskInfo.State = DistributedTasksOnTime.TaskState.Progress;
		taskInfo.ProgressInfo = new DistributedTasksOnTime.ProgressInfo
		{
			Body = info.Body,
			Entity = info.Entity,
			EntityId = info.EntityId,
			EntityName = info.EntityName,
			GroupName = info.GroupName,
			Subject = info.Subject,
			Index = info.Index,
			TaskId = info.TaskId,
			TotalCount = info.TotalCount,
			Type = (DistributedTasksOnTime.ProgressType)Enum.Parse(typeof(DistributedTasksOnTime.ProgressType), $"{info.Type}"),
		};

		await Bus.EnqueueMessage(Settings.TaskInfoQueueName, taskInfo);
	}
}

