using DistributedTasksOnTime.Client.Readers;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DistributedTaskOnTime.Tests")]

namespace DistributedTasksOnTime.Client;
public static class StartupExtensions
{
	public static IServiceCollection AddDistributedTasksOnTimeClient(this IServiceCollection services, 
				DistributedTasksOnTimeSettings settings)
	{
		services.AddSingleton(settings);
		services.AddTransient<DistributedProgressReporter>();
		services.AddTransient<IForceTaskService, ForceTaskService>();

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
		var logger = serviceProvider.GetRequiredService<ILogger<ProcessTaskReader>>();

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
			try
			{
				var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
				taskInfo.Id = taskId;
				taskInfo.State = DistributedTasksOnTime.TaskState.Started;
				taskInfo.HostKey = settings.HostKey;
				await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
		};
		taskHost.TaskCanceled += async (s, taskId) =>
		{
			try
			{
				var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
				taskInfo.Id = taskId;
				taskInfo.State = DistributedTasksOnTime.TaskState.Canceled;
				taskInfo.HostKey = settings.HostKey;

				await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
		};
		taskHost.TaskTerminated += async (s, taskId) =>
		{
			try
			{
				var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
				taskInfo.Id = taskId;
				taskInfo.HostKey = settings.HostKey;
				taskInfo.State = DistributedTasksOnTime.TaskState.Terminated;

				await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}

		};
		taskHost.TaskFailed += async (s, taskId) =>
		{
			try
			{
				var history = taskHost.GetHistory(taskId);

				var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
				taskInfo.Id = taskId;
				taskInfo.State = DistributedTasksOnTime.TaskState.Failed;
				taskInfo.HostKey = settings.HostKey;
				taskInfo.ErrorStack = history.Exception.ToString();

				await bus.SendAsync(settings.TaskInfoQueueName, taskInfo);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}

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
		register.AddAzureQueueWriter(settings.ForceTaskQueueName);
		
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

