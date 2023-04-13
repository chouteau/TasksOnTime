using Microsoft.AspNetCore.Builder;
using Microsoft.Win32;

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

		builder.Services.AddArianeBus(config =>
		{
			config.RegisterQueueReader<Readers.TaskInfoReader>(new QueueName(settings.TaskInfoQueueName));
			config.RegisterQueueReader<Readers.HostRegistrationReader>(new QueueName(settings.HostRegistrationQueueName));
			config.RegisterQueueReader<Readers.ForceTaskReader>(new QueueName(settings.ForceTaskQueueName));
		});

		return builder;
	}
}

