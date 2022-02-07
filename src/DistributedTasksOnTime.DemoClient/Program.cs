// See https://aka.ms/new-console-template for more information
using DistributedTasksOnTime.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, TasksOnTime DemoClient!");

var currentFolder = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);

var host= Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(configurationBuilder =>
				{
					configurationBuilder
						.AddJsonFile("appSettings.json");

					var localConfig = System.IO.Path.Combine(currentFolder, "localconfig", "appsettings.json");
					if (System.IO.File.Exists(localConfig))
					{
						configurationBuilder.AddJsonFile(localConfig, true, false);
					}

				})
				.ConfigureServices((ctx, services) =>
				{
					var clientSection = ctx.Configuration.GetSection("DistributedTasksOnTime");
					var clientSettings = new DistributedTasksOnTimeSettings();
					clientSection.Bind(clientSettings);

					clientSettings.RegisterScheduledTask(new DistributedTasksOnTime.TaskRegistrationInfo
					{
						AllowMultipleInstances = false,
						TaskName = "Demo",
						AssemblyQualifiedName = typeof(DistributedTasksOnTime.DemoClient.DemoTask).AssemblyQualifiedName,
						Enabled = true,
					});

					services.AddDistributedTasksOnTimeClient(clientSettings, arianeConfig =>
					{
						arianeConfig.DefaultAzureConnectionString = clientSettings.AzureBusConnectionString;
					});
				})
				.Build();

await host.Services.UseDistributedTasksOnTime();

await host.RunAsync();

