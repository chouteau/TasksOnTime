using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TasksOnTimeCore.Tests", AllInternalsVisible = true)]

namespace TasksOnTime.Scheduling
{
	public static class StartupExtentions
	{
		public static IServiceCollection AddTasksOnTimeScheduledServices(this IServiceCollection services, Action<TasksOnTimeSettings> config, Action<TasksOnTimeSchedulingSettings> configScheduling = null)
		{
			var defaultSettings = new TasksOnTimeSchedulingSettings();
			configScheduling?.Invoke(defaultSettings);
			services.AddSingleton(defaultSettings);

			services.AddTasksOnTimeServices(config);
			services.AddSingleton<ITaskScheduler, TaskScheduler>();
			return services;
		}
	}
}
