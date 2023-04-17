namespace DistributedTasksOnTime.Orchestrator;

public class MainWorker : BackgroundService
{
	private DateTime _garbadgeTimeout = DateTime.Now;

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
		await TasksOrchestrator.Start();
		await base.StartAsync(cancellationToken);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			if (Settings.Enable)
			{
				await TasksOrchestrator.EnqueueNextTasks(DateTime.Now);
			}

			if (_garbadgeTimeout < DateTime.Now)
			{
                await TasksOrchestrator.TerminateOldTasks();
				_garbadgeTimeout = DateTime.Now.AddMinutes(1);
            }
            await Task.Delay(Settings.TimerInSecond * 1000, stoppingToken);
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		await TasksOrchestrator.Stop();
		await base.StopAsync(cancellationToken);
	}

}
