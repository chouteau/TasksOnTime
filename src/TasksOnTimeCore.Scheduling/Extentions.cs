﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TasksOnTimeCore.Tests", AllInternalsVisible = true)]

namespace TasksOnTime.Scheduling
{
	public static class Extentions
	{
		public static IServiceCollection AddTasksOnTimeScheduledServices([NotNull] this IServiceCollection services, [NotNull] IConfiguration configuration, Action<ScheduleSettings> settingsExpression = null)
		{
			var defaultSettings = new ScheduleSettings();
			configuration.GetSection("TasksOnTime").Bind(defaultSettings);
			services.AddSingleton(defaultSettings);
			services.AddTasksOnTimeServices(configuration);
			services.AddSingleton<ITaskScheduler, TaskScheduler>();
			services.AddLogging();
			return services;
		}
	}
}
