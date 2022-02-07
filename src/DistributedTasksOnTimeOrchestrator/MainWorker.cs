namespace DistributedTasksOnTime.Orchestrator;

public class MainWorker : BackgroundService
{
	public MainWorker(ILogger<MainWorker> logger,
		ITasksOrchestrator tasksOrchestrator,
		DistributedTasksOnTimeServerSettings settings)
	{
		this.Logger = logger;
		this.TasksOrchestrator = tasksOrchestrator;
		this.Settings = settings;
	}

	protected ILogger Logger { get; }
	protected ITasksOrchestrator TasksOrchestrator { get; }
	protected DistributedTasksOnTimeServerSettings Settings { get; }

	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		TasksOrchestrator.Start();
		await base.StartAsync(cancellationToken);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await TasksOrchestrator.EnqueueNextTasks(DateTime.Now);
			await Task.Delay(Settings.TimerInSecond * 1000, stoppingToken);
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		TasksOrchestrator.Stop();
		await base.StopAsync(cancellationToken);
	}

}
