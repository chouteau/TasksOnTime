using System.Reflection.Emit;

namespace DistributedTasksOnTime.Client.Readers;
internal class CheckTaskIsRunningReader : MessageReaderBase<CheckTaskIsRunning>
{
    public CheckTaskIsRunningReader(ArianeBus.IServiceBus bus,
        TasksOnTime.ITasksHost host,
        DistributedTasksOnTimeSettings settings,
        ILogger<CheckTaskIsRunningReader> logger)
    {
        this.Bus = bus;
        this.Host = host;
        this.Settings = settings;
        this.Logger = logger;
    }

    protected ArianeBus.IServiceBus Bus { get; }
    protected TasksOnTime.ITasksHost Host { get; }
    protected DistributedTasksOnTimeSettings Settings { get; }
    protected ILogger Logger { get; }

    public override async Task ProcessMessageAsync(CheckTaskIsRunning message, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Check if {taskName} is running on {machineName}", message.ScheduledTaskName, System.Environment.MachineName);
        var task = Host.GetHistory(message.TaskId);

        // On regarde s'il y en a au moins une en cours
        if (task != null
            && !task.TerminatedDate.HasValue)
        {
            var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
            taskInfo.Id = message.TaskId;
            taskInfo.State = DistributedTasksOnTime.TaskState.RunningConfirmed;
            await Bus.EnqueueMessage(Settings.TaskInfoQueueName, taskInfo);

            Logger.LogInformation("{taskName} started on {machineName}", message.ScheduledTaskName, System.Environment.MachineName);
        }
        else
        {
            Logger.LogInformation("{taskName} not running on {machineName}", message.ScheduledTaskName, System.Environment.MachineName);

        }
    }
}
