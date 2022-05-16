namespace DistributedTasksOnTime.BlazorComponent;

public static class StartupExtensions
{
	public static IHostBuilder AddDistributedTasksOnTimeBlazor(this IHostBuilder builder, DistributedTasksOnTimeServerSettings settings)
	{
		builder.AddDistributedTasksOnTimeOrchestrator(settings);
		return builder;
	}

	public static IHostBuilder AddDistributedTasksOnTimeBlazor(this IHostBuilder builder, Action<DistributedTasksOnTimeServerSettings> config)
	{
		builder.AddDistributedTasksOnTimeOrchestrator(config);
		return builder;
	}
}
