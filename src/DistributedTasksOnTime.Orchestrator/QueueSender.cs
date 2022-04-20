using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace DistributedTasksOnTime.Orchestrator;

internal class QueueSender 
{
	public QueueSender(DistributedTasksOnTimeServerSettings settings,
		ILogger<QueueSender> logger,
		ExistingQueues existingQueues)
	{
		this.Settings = settings;
		this.Logger	= logger;
		this.ExistingQueues = existingQueues;

		ServiceBusClient = new ServiceBusClient(settings.AzureBusConnectionString, new ServiceBusClientOptions()
		{
			TransportType = ServiceBusTransportType.AmqpTcp,
			RetryOptions = new ServiceBusRetryOptions()
			{
				Mode = ServiceBusRetryMode.Exponential,
				MaxRetries = 3,
				MaxDelay = TimeSpan.FromSeconds(10)
			}
		});
	}

	protected DistributedTasksOnTimeServerSettings Settings { get; }
	protected ServiceBusClient ServiceBusClient { get; }
	protected ILogger Logger { get; }
	protected ExistingQueues ExistingQueues { get; }

	public async Task SendMessage<T>(string queueName, T message)
	{
		ServiceBusMessage busMessage = null;
		string data = null;
		try
		{
			data = System.Text.Json.JsonSerializer.Serialize(message);
			busMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(data));
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, ex.Message);
			return;
		}

		await using var sender = ServiceBusClient.CreateSender(queueName);
		try
		{
			await sender.SendMessageAsync(busMessage);
			Logger.LogTrace("message sent to azure bus in queue {0}", queueName);
		}
		catch (Exception ex)
		{
			ex.Data.Add("QueueName", queueName);
			ex.Data.Add("Message", data);
			Logger.LogError(ex, ex.Message);
			await Task.Delay(200);
		}
	}
}

