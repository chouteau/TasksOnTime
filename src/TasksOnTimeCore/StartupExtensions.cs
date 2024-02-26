using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TasksOnTimeCore.Tests", AllInternalsVisible = true)]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TasksOnTimeCore.Scheduling", AllInternalsVisible = true)]

namespace TasksOnTime
{
	public static class StartupExtensions
	{
		public static IServiceCollection AddTasksOnTimeServices(this IServiceCollection services, Action<TasksOnTimeSettings> config = null)
		{
			var defaultSettings = new TasksOnTimeSettings();
			config?.Invoke(defaultSettings);
			services.AddSingleton(defaultSettings);

			services.AddSingleton<ITasksHost, TasksHost>();
			services.TryAddTransient<IProgressReporter, DefaultProgressReporter>();

			return services;
		}

		public static void AddOrUpdateParameter(this ExecutionContext ctx, string key, object value)
		{
			ctx.Parameters.AddOrUpdateParameter(key, value);
		}

		public static void AddOrUpdateParameter(this Dictionary<string, object> parameters, string key, object value)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return;
			}
			if (parameters.ContainsKey(key))
			{
				parameters[key] = value;
			}
			else
			{
				parameters.Add(key, value);
			}
		}

		public static object GetParameter(this ExecutionContext ctx, string key)
		{
			return ctx.Parameters.GetParameter(key);
		}

		public static object GetParameter(this Dictionary<string, object> parameters, string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}

			if (parameters.ContainsKey(key))
			{
				return parameters[key];
			}

			return null;
		}

	}
}
