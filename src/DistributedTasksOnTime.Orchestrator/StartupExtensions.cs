namespace DistributedTasksOnTime.Orchestrator;

public static class StartupExtensions
{
	public static IHostBuilder AddDistributedTasksOnTimeOrchestrator(this IHostBuilder builder, Action<Ariane.IRegister> arianeRegister = null)
	{
		var currentFolder = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);

		builder
			.ConfigureAppConfiguration((ctx, configurationBuilder) =>
			{
				configurationBuilder.SetBasePath(currentFolder)
						.AddJsonFile("appSettings.json", false, true)
						.AddJsonFile($"appSettings.{ctx.HostingEnvironment.EnvironmentName}.json", true, true)
						.AddEnvironmentVariables();

				var localConfig = System.IO.Path.Combine(currentFolder, "localconfig", "appsettings.json");
				if (System.IO.File.Exists(localConfig))
				{
					configurationBuilder.AddJsonFile(localConfig, true, false);
				}
			})
			.ConfigureServices((ctx, services) =>
			{
				services.AddSingleton<ITasksOrchestrator, TasksOrchestrator>();
				services.AddTransient<Repository.IDbRepository, Repository.FileDbRepository>();
				services.AddTransient<QueueSender>();

				services.AddHostedService<MainWorker>();

				var settings = new DistributedTasksOnTimeServerSettings();
				var section = ctx.Configuration.GetSection("DistributedTasksOnTime");
				section.Bind(settings);
				services.AddSingleton(settings);

				if (settings.StoreFolder.StartsWith(@".\"))
				{
					settings.StoreFolder = System.IO.Path.Combine(currentFolder, settings.StoreFolder.Replace(@".\", ""));
					if (!System.IO.Directory.Exists(settings.StoreFolder))
					{
						System.IO.Directory.CreateDirectory(settings.StoreFolder);
					}
				}

				services.ConfigureArianeAzure();
				Action<Ariane.ArianeSettings> arianeConfig = (cfg) =>
				{
					cfg.DefaultAzureConnectionString = settings.AzureBusConnectionString;
				};
				if (arianeRegister != null)
				{
					services.ConfigureAriane(arianeRegister, arianeConfig);
				}
				else
				{
					services.ConfigureAriane(register =>
					{
						register.AddAzureQueueReader<Readers.TaskInfoReader>(settings.TaskInfoQueueName);
						register.AddAzureQueueReader<Readers.HostRegistrationReader>(settings.HostRegistrationQueueName);
						register.AddAzureTopicWriter(settings.CancelTaskQueueName);
					}, arianeConfig);
				}

			});

		return builder;
	}
}

