using ArianeBus;

using DistributedTasksOnTime.Client.Readers;

using Microsoft.Win32;

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

		services.AddArianeBus(config =>
		{
			var subscriptionName = $"{System.Environment.MachineName}.{settings.HostName}";
			config.RegisterTopicReader<Readers.CancelTaskReader>(new TopicName(settings.CancelTaskQueueName), new SubscriptionName(subscriptionName));

			foreach (var item in settings.ScheduledTaskList)
			{
				var queueOrTopicName = $"{settings.PrefixQueueName}.{item.TaskName}";
				if (item.ProcessMode == ProcessMode.Exclusive)
				{
					config.RegisterQueueReader<Readers.ProcessTaskReader>(new QueueName(queueOrTopicName));
				}
				else
				{
					config.RegisterTopicReader<Readers.ProcessTaskReader>(new TopicName(queueOrTopicName), new SubscriptionName(subscriptionName));
				}
			}
		});

		return services;
	}

	public static async Task<IServiceProvider> UseDistributedTasksOnTimeClient(this IServiceProvider serviceProvider)
	{
		var settings = serviceProvider.GetRequiredService<DistributedTasksOnTimeSettings>();
		var logger = serviceProvider.GetRequiredService<ILogger<ProcessTaskReader>>();


		var registrationInfo = new DistributedTasksOnTime.HostRegistrationInfo();
		registrationInfo.MachineName = System.Environment.MachineName;
		registrationInfo.HostName = settings.HostName;
		registrationInfo.State = DistributedTasksOnTime.HostRegistrationState.Started;
		registrationInfo.TaskList = settings.ScheduledTaskList;

		var bus = serviceProvider.GetRequiredService<ArianeBus.IServiceBus>();
		await bus.EnqueueMessage(settings.HostRegistrationQueueName, registrationInfo);

		var taskHost = serviceProvider.GetRequiredService<TasksOnTime.ITasksHost>();

		taskHost.TaskStarted += async (s, taskId) =>
		{
			try
			{
				var taskInfo = new DistributedTasksOnTime.DistributedTaskInfo();
				taskInfo.Id = taskId;
				taskInfo.State = DistributedTasksOnTime.TaskState.Started;
				taskInfo.HostKey = settings.HostKey;
				await bus.EnqueueMessage(settings.TaskInfoQueueName, taskInfo);
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

				await bus.EnqueueMessage(settings.TaskInfoQueueName, taskInfo);
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

				await bus.EnqueueMessage(settings.TaskInfoQueueName, taskInfo);
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

				await bus.EnqueueMessage(settings.TaskInfoQueueName, taskInfo);
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
		var settings = serviceProvider.GetRequiredService<DistributedTasksOnTimeSettings>();

		var registrationInfo = new DistributedTasksOnTime.HostRegistrationInfo();
		registrationInfo.MachineName = System.Environment.MachineName;
		registrationInfo.HostName = settings.HostName;
		registrationInfo.State = DistributedTasksOnTime.HostRegistrationState.Stopped;

		var bus = serviceProvider.GetRequiredService<ArianeBus.IServiceBus>();
		await bus.EnqueueMessage(settings.HostRegistrationQueueName, registrationInfo);
		await Task.Delay(1 * 1000);

		return serviceProvider;
	}
}

