using Ariane;
using DistributedTasksOnTime.Client;
using DistributedTasksOnTime.Orchestrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTaskOnTime.Tests
{
	internal static class TestsHelper
	{
		public static IHost CreateTestHostWith1Client(Action<DistributedTasksOnTimeSettings> config)
		{
			var currentFolder = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);

			var configurationBuilder = new ConfigurationBuilder()
						.AddJsonFile("appSettings.json");

			var localConfig = System.IO.Path.Combine(currentFolder, "localconfig", "appsettings.json");
			if (System.IO.File.Exists(localConfig))
			{
				configurationBuilder.AddJsonFile(localConfig, true, false);
			}

			var configuration = configurationBuilder.Build();

			var clientSection = configuration.GetSection("DistributedTasksOnTime");
			var clientSettings = new DistributedTasksOnTimeSettings();
			clientSection.Bind(clientSettings);

			config.Invoke(clientSettings);

			Action<Ariane.IRegister> arianeRegister = (register) =>
			{
				var topicName = $"{System.Environment.MachineName}.{clientSettings.HostName}";
				register.AddAzureTopicReader<DistributedTasksOnTime.Client.Readers.CancelTaskReader>(clientSettings.CancelTaskQueueName, topicName);

				register.AddAzureQueueReader<DistributedTasksOnTime.Orchestrator.Readers.TaskInfoReader>(clientSettings.TaskInfoQueueName);
				register.AddAzureQueueReader<DistributedTasksOnTime.Orchestrator.Readers.HostRegistrationReader>(clientSettings.HostRegistrationQueueName);

				foreach (var item in clientSettings.ScheduledTaskList)
				{
					var queueName = $"{clientSettings.PrefixQueueName}.{item.TaskName}";
					register.AddAzureQueueReader<DistributedTasksOnTime.Client.Readers.ProcessTaskReader>(queueName);
				}
			};

			Action<Ariane.ArianeSettings> arianeConfig = (cfg) =>
			{
				cfg.DefaultAzureConnectionString = clientSettings.AzureBusConnectionString;
				cfg.UniqueTopicNameForTest = "test";
				cfg.UniquePrefixName = "test.";
				cfg.WorkSynchronized = true;
			};

			var builder = Host.CreateDefaultBuilder()
				.ConfigureServices(services =>
				{
					services.AddSingleton(clientSettings);
					services.AddDistributedTasksOnTimeClient(clientSettings, arianeConfig, arianeRegister);
				});

			builder.AddDistributedTasksOnTimeOrchestrator(arianeRegister);

			var host = builder.Build();
			return host;
		}
	}
}
