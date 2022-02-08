[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DistributedTaskOnTime.Tests")]

IHost host = Host.CreateDefaultBuilder(args)
				.AddDistributedTasksOnTimeOrchestrator()
				.Build();

await host.RunAsync();
