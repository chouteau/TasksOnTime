[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DistributedTaskOnTime.Tests")]

namespace DistributedTasksOnTime.Client;
public static class StartupExtensions
{
	public static IServiceCollection AddDistributedTasksOnTimeClient(this IServiceCollection services, 
				DistributedTasksOnTimeSettings settings)
	{
		services.AddSingleton(settings);
		services.AddTransient<DistributedProgressReporter>();

		services.AddTasksOnTimeServices(tasksOnTimeConfig =>
		{
			tasksOnTimeConfig.ProgresReporterType = typeof(DistributedProgressReporter);
		});

		return services;
	}

	public static async Task<IServiceProvider> UseDistributedTasksOnTimeClient(this IServiceProvider serviceProvider)
	{
		var bus = serviceProvider.GetRequiredService<Ariane.IServiceBus>();
		var settings = serviceProvider.GetRequiredService<DistributedTasksOnTimeSettings>();

		await bus.StartReadingAsync();

		var registrationInfo = new DistributedTasksOnTime.HostRegistrationInfo();
		registrationInfo.MachineName = System.Environment.MachineName;
		registrationInfo.HostName = settings.HostName;
		registrationInfo.State = DistributedTasksOnTime.HostRegistrationState.Started;
		registrationInfo.TaskList = settings.ScheduledTaskList;

		await bus.SendAsync(settings.HostRegistrationQueueName, registrationInfo);

		var taskHost = serviceProvider.GetRequiredService<TasksOnTime.ITasksHost>();

		taskHost.TaskStarted += async (s, taskId) =>
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.State = DistributedTasksOnTime.TaskState.Started;
			taskInfo.HostKey = settings.HostKey;

			await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
		};
		taskHost.TaskCanceled += async (s, taskId) =>
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.State = DistributedTasksOnTime.TaskState.Canceled;
			taskInfo.HostKey = settings.HostKey;

			await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
		};
		taskHost.TaskTerminated += async (s, taskId) =>
		{
			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.HostKey = settings.HostKey;
			taskInfo.State = DistributedTasksOnTime.TaskState.Terminated;

			await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
		};
		taskHost.TaskFailed += async (s, taskId) =>
		{
			var history = taskHost.GetHistory(taskId);

			var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
			taskInfo.Id = taskId;
			taskInfo.State = DistributedTasksOnTime.TaskState.Failed;
			taskInfo.HostKey = settings.HostKey;
			taskInfo.ErrorStack = history.Exception.StackTrace;

			await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
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

		await bus.SendAsync(settings.HostRegistrationQueueName, registrationInfo);
		await Task.Delay(1 * 1000);

		await bus.StopReadingAsync();

		return serviceProvider;
	}

	public static void SetupArianeRegisterDistributedTasksOnTimeClient(this IRegister register, DistributedTasksOnTimeSettings settings)
	{
		register.AddAzureQueueWriter(settings.TaskInfoQueueName);
		register.AddAzureQueueWriter(settings.HostRegistrationQueueName);
		var topicName = $"{System.Environment.MachineName}.{settings.HostName}";
		register.AddAzureTopicReader<Readers.CancelTaskReader>(settings.CancelTaskQueueName, topicName);

		foreach (var item in settings.ScheduledTaskList)
		{
			var queueName = $"{settings.PrefixQueueName}.{item.TaskName}";
			if (item.ProcessMode == ProcessMode.Exclusive)
			{
				register.AddAzureQueueReader<Readers.ProcessTaskReader>(queueName);
			}
			else
			{
				register.AddAzureTopicReader<Readers.ProcessTaskReader>(queueName,topicName);
			}
		}
	}


}

