using ArianeBus;
using DistributedTasksOnTime.Client;
using DistributedTasksOnTime.JsonFilePersistence;
using DistributedTasksOnTime.Orchestrator;

using Microsoft.AspNetCore.Builder;
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

			var localConfig = System.IO.Path.Combine(currentFolder, "localconfig", "appsettings.json");
			var configurationBuilder = new ConfigurationBuilder()
						.AddJsonFile("appSettings.json")
						.AddJsonFile(localConfig, false, false);

			var configuration = configurationBuilder.Build();

			var clientSection = configuration.GetSection("DistributedTasksOnTime");
			var clientSettings = new DistributedTasksOnTimeSettings();
			var serverSettings = new DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings();
			clientSection.Bind(clientSettings);
			clientSection.Bind(serverSettings);

			config.Invoke(clientSettings);

			var builder = WebApplication.CreateBuilder();

            builder.Services.AddSingleton(clientSettings);
            builder.Services.AddArianeBus(config =>
			{
				config.BusConnectionString = clientSettings.AzureBusConnectionString;
			});

			builder.Services.AddDistributedTasksOnTimeClient(clientSettings);

			serverSettings.TimerInSecond = 2;
			builder.AddDistributedTasksOnTimeOrchestrator(serverSettings);
			builder.Services.AddTasksOnTimeJsonFilePersistence(config =>
			{
				config.StoreFolder = @".\";
			});

			var host = builder.Build();
			return host;
		}
	}
}
