namespace DistributedTasksOnTime.Orchestrator;

public static class StartupExtensions
{
	public static IHostBuilder AddDistributedTasksOnTimeOrchestrator(this IHostBuilder builder, Action<DistributedTasksOnTimeServerSettings> config)
	{
		var settings = new DistributedTasksOnTimeServerSettings();
		config(settings);
		builder.AddDistributedTasksOnTimeOrchestrator(settings);

		return builder;
	}

	public static IHostBuilder AddDistributedTasksOnTimeOrchestrator(this IHostBuilder builder, DistributedTasksOnTimeServerSettings settings)
	{
		var currentFolder = System.IO.Path.GetDirectoryName(typeof(StartupExtensions).Assembly.Location);

		builder
			.ConfigureServices((ctx, services) =>
			{
				services.AddSingleton<ITasksOrchestrator, TasksOrchestrator>();
				services.AddTransient<Repository.IDbRepository, Repository.FileDbRepository>();
				services.AddTransient<QueueSender>();
				services.AddSingleton(new ExistingQueues());

				services.AddHostedService<MainWorker>();

				services.AddSingleton(settings);

				if (settings.StoreFolder.StartsWith(@".\"))
				{
					settings.StoreFolder = System.IO.Path.Combine(currentFolder, settings.StoreFolder.Replace(@".\", ""));
				}
				if (!System.IO.Directory.Exists(settings.StoreFolder))
				{
					System.IO.Directory.CreateDirectory(settings.StoreFolder);
				}
			});

		return builder;
	}

	public static void SetupArianeRegisterDistributedTasksOnTimeOrchestrator(this IRegister register, DistributedTasksOnTimeServerSettings settings)
	{
		register.AddAzureQueueReader<Readers.TaskInfoReader>(settings.TaskInfoQueueName);
		register.AddAzureQueueReader<Readers.HostRegistrationReader>(settings.HostRegistrationQueueName);
		register.AddAzureTopicWriter(settings.CancelTaskQueueName);
	}
}

