// See https://aka.ms/new-console-template for more information
using ArianeBus;
using DistributedTasksOnTime.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, TasksOnTime DemoClient!");

var currentFolder = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);

var host= Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(configurationBuilder =>
				{
                    var localConfig = System.IO.Path.Combine(currentFolder, "localconfig", "appsettings.json");
                    configurationBuilder
                        .AddJsonFile("appSettings.json")
						.AddJsonFile(localConfig, true, false);

				})
				.ConfigureServices((ctx, services) =>
				{
					var clientSection = ctx.Configuration.GetSection("DistributedTasksOnTime");
					var clientSettings = new DistributedTasksOnTimeSettings();
					clientSection.Bind(clientSettings);

					clientSettings.RegisterScheduledTask<DistributedTasksOnTime.DemoClient.DemoTask>(new DistributedTasksOnTime.TaskRegistrationInfo
					{
						TaskName = "DemoTask",
						Description = "Demo task description",
						DefaultPeriod = DistributedTasksOnTime.ScheduledTaskTimePeriod.Second,
						DefaultInterval = 30
					});

					clientSettings.RegisterScheduledTask<DistributedTasksOnTime.DemoClient.CancelableTask>(new DistributedTasksOnTime.TaskRegistrationInfo
					{
						TaskName = "CancelableDemoTask",
						Description = "Cancelable Demo task description",
						DefaultPeriod = DistributedTasksOnTime.ScheduledTaskTimePeriod.Minute,
						DefaultInterval = 1
					});

					clientSettings.RegisterScheduledTask<DistributedTasksOnTime.DemoClient.CancelableTask>(new DistributedTasksOnTime.TaskRegistrationInfo
					{
						TaskName = "CancelableDemoTask4",
						Description = "Cancelable Demo4 task description",
						DefaultPeriod = DistributedTasksOnTime.ScheduledTaskTimePeriod.Minute,
						DefaultInterval = 1
					});

					clientSettings.RegisterScheduledTask<DistributedTasksOnTime.DemoClient.TopicDemoTask>(new DistributedTasksOnTime.TaskRegistrationInfo
					{
						TaskName = "TopicDemoTask",
						Description = "Topic Demo task description",
						DefaultPeriod = DistributedTasksOnTime.ScheduledTaskTimePeriod.Minute,
						DefaultInterval = 1,
						ProcessMode = DistributedTasksOnTime.ProcessMode.AllInstances
					});

					clientSettings.RegisterScheduledTask<DistributedTasksOnTime.DemoClient.FailTask>(new DistributedTasksOnTime.TaskRegistrationInfo
					{
						TaskName = "FailTask",
						Description = "task fail after 10 secondes",
						DefaultPeriod = DistributedTasksOnTime.ScheduledTaskTimePeriod.Minute,
						DefaultInterval = 1,
						ProcessMode = DistributedTasksOnTime.ProcessMode.AllInstances
					});
					services.AddDistributedTasksOnTimeClient(clientSettings);

					services.AddArianeBus(config =>
					{ 
						config.BusConnectionString = clientSettings.AzureBusConnectionString;
					});

				})
				.Build();

await host.Services.UseDistributedTasksOnTimeClient();

await Task.Delay(15 * 1000);

await host.RunAsync();



