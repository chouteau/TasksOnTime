using DistributedTasksOnTime.Orchestrator;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent
{
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
}
