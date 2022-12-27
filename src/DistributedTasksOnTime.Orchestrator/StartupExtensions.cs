using Microsoft.AspNetCore.Builder;

namespace DistributedTasksOnTime.Orchestrator;

public static class StartupExtensions
{
	public static WebApplicationBuilder AddDistributedTasksOnTimeOrchestrator(this WebApplicationBuilder builder, Action<DistributedTasksOnTimeServerSettings> config)
	{
		var settings = new DistributedTasksOnTimeServerSettings();
		config(settings);
		builder.AddDistributedTasksOnTimeOrchestrator(settings);

		return builder;
	}

	public static WebApplicationBuilder AddDistributedTasksOnTimeOrchestrator(this WebApplicationBuilder builder, DistributedTasksOnTimeServerSettings settings)
	{
		var currentFolder = System.IO.Path.GetDirectoryName(typeof(StartupExtensions).Assembly.Location);

		builder.Services.AddSingleton<ITasksOrchestrator, TasksOrchestrator>();
        builder.Services.AddTransient<QueueSender>();
        builder.Services.AddSingleton(new ExistingQueues());

        builder.Services.AddHostedService<MainWorker>();

        builder.Services.AddSingleton(settings);

		if (settings.StoreFolder.StartsWith(@".\")
			&& System.Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			settings.StoreFolder = System.IO.Path.Combine(currentFolder, settings.StoreFolder.Replace(@".\", ""));
		}
		if (!System.IO.Directory.Exists(settings.StoreFolder))
		{
			System.IO.Directory.CreateDirectory(settings.StoreFolder);
		}

		return builder;
	}

	public static void SetupArianeRegisterDistributedTasksOnTimeOrchestrator(this IRegister register, DistributedTasksOnTimeServerSettings settings)
	{
		register.AddAzureQueueReader<Readers.TaskInfoReader>(settings.TaskInfoQueueName);
		register.AddAzureQueueReader<Readers.HostRegistrationReader>(settings.HostRegistrationQueueName);
		register.AddAzureQueueReader<Readers.ForceTaskReader>(settings.ForceTaskQueueName);
		register.AddAzureTopicWriter(settings.CancelTaskQueueName);
	}
}

