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
			var serverSettings = new DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings();
			clientSection.Bind(clientSettings);
			clientSection.Bind(serverSettings);

			config.Invoke(clientSettings);

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
					services.ConfigureArianeAzure();
					services.ConfigureAriane(register =>
					{
						register.SetupArianeRegisterDistributedTasksOnTimeClient(clientSettings);
						register.SetupArianeRegisterDistributedTasksOnTimeOrchestrator(serverSettings);

					}, arianeConfig);

					services.AddDistributedTasksOnTimeClient(clientSettings);
				});

			builder.AddDistributedTasksOnTimeOrchestrator(serverSettings);

			var host = builder.Build();
			return host;
		}
	}
}
