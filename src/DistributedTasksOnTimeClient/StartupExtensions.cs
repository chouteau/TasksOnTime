[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DistributedTaskOnTime.Tests")]

namespace DistributedTasksOnTime.Client;
public static class StartupExtensions
{
	public static IServiceCollection AddDistributedTasksOnTimeClient(this IServiceCollection services,
		Action<DistributedTasksOnTimeSettings> config,
		Action<ArianeSettings> arianeConfig = null,
		Action<Ariane.IRegister> arianeRegister = null)
	{
		var settings = new DistributedTasksOnTimeSettings();
		config.Invoke(settings);

		services.AddDistributedTasksOnTimeClient(settings, arianeConfig, arianeRegister);
		return services;
	}

	public static IServiceCollection AddDistributedTasksOnTimeClient(this IServiceCollection services, 
				DistributedTasksOnTimeSettings settings, 
				Action<ArianeSettings> arianeConfig = null,
				Action<Ariane.IRegister> arianeRegister = null)
	{
		services.AddSingleton(settings);
		services.AddTransient<DistributedProgressReporter>();

		services.ConfigureArianeAzure();
		if (arianeRegister == null)
		{
			services.ConfigureAriane(register =>
			{
				register.AddAzureQueueWriter(settings.TaskInfoQueueName);
				register.AddAzureQueueWriter(settings.HostRegistrationQueueName);
				var topicName = $"{System.Environment.MachineName}.{settings.HostName}";
				register.AddAzureTopicReader<Readers.CancelTaskReader>(settings.CancelTaskQueueName, topicName);

				foreach (var item in settings.ScheduledTaskList)
				{
					var queueName = $"{settings.PrefixQueueName}.{item.TaskName}";
					register.AddAzureQueueReader<Readers.ProcessTaskReader>(queueName);
				}
			}, arianeConfig);
		}
		else
		{
			services.ConfigureAriane(arianeRegister, arianeConfig);
		}

		services.AddTasksOnTimeServices(tasksOnTimeConfig =>
		{
			tasksOnTimeConfig.ProgresReporterType = typeof(DistributedProgressReporter);
		});

		return services;
	}

	public static async Task<IServiceProvider> UseDistributedTasksOnTime(this IServiceProvider serviceProvider)
	{
		var bus = serviceProvider.GetRequiredService<Ariane.IServiceBus>();
		var settings = serviceProvider.GetRequiredService<DistributedTasksOnTimeSettings>();

		await bus.StartReadingAsync();

		var registrationInfo = new DistributedTasksOnTime.HostRegistrationInfo();
		registrationInfo.MachineName = System.Environment.MachineName;
		registrationInfo.HostName = settings.HostName;
		registrationInfo.State = DistributedTasksOnTime.HostRegistrationState.Started;
		registrationInfo.TaskList = settings.ScheduledTaskList;

		bus.Send(settings.HostRegistrationQueueName, registrationInfo);

		var taskHost = serviceProvider.GetRequiredService<TasksOnTime.ITasksHost>();

		taskHost.TaskStarted += (s, taskId) =>
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.State = DistributedTasksOnTime.TaskState.Started;
			taskInfo.HostKey = settings.HostKey;

			bus.Send(settings.TaskInfoQueueName, taskInfo);
		};
		taskHost.TaskTerminated += (s, taskId) =>
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.HostKey = settings.HostKey;
			taskInfo.State = DistributedTasksOnTime.TaskState.Terminated;

			bus.Send(settings.TaskInfoQueueName, taskInfo);
		};
		taskHost.TaskFailed += (s, taskId) =>
		{
			var history = taskHost.GetHistory(taskId);

			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.State = DistributedTasksOnTime.TaskState.Failed;
			taskInfo.HostKey = settings.HostKey;
			taskInfo.ErrorStack = history.Exception.StackTrace;

			bus.Send(settings.TaskInfoQueueName, taskInfo);
		};

		return serviceProvider;
	}

	public static async Task<IServiceProvider> StopDistributedTasksOnTime(this IServiceProvider serviceProvider)
	{
		var bus = serviceProvider.GetRequiredService<Ariane.IServiceBus>();
		var settings = serviceProvider.GetRequiredService<DistributedTasksOnTimeSettings>();

		var registrationInfo = new DistributedTasksOnTime.HostRegistrationInfo();
		registrationInfo.MachineName = System.Environment.MachineName;
		registrationInfo.HostName = settings.HostName;
		registrationInfo.State = DistributedTasksOnTime.HostRegistrationState.Stopped;

		bus.Send(settings.HostRegistrationQueueName, registrationInfo);
		await Task.Delay(1 * 1000);

		await bus.StopReadingAsync();

		return serviceProvider;
	}

}

