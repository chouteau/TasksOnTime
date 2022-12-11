using Microsoft.AspNetCore.Builder;

namespace DistributedTasksOnTime.BlazorComponent;

public static class StartupExtensions
{
	public static WebApplicationBuilder AddDistributedTasksOnTimeBlazor(this WebApplicationBuilder builder, DistributedTasksOnTimeServerSettings settings)
	{
		builder.AddDistributedTasksOnTimeOrchestrator(settings);
		return builder;
	}

	public static WebApplicationBuilder AddDistributedTasksOnTimeBlazor(this WebApplicationBuilder builder, Action<DistributedTasksOnTimeServerSettings> config)
	{
		builder.AddDistributedTasksOnTimeOrchestrator(config);
		return builder;
	}
}
