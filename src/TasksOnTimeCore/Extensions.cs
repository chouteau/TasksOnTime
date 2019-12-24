using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TasksOnTimeCore.Tests", AllInternalsVisible = true)]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TasksOnTimeCore.Scheduling", AllInternalsVisible = true)]

namespace TasksOnTime
{
	public static class Extensions
	{
		public static IServiceCollection AddTasksOnTimeServices([NotNull] this IServiceCollection services, [NotNull] IConfiguration configuration, Action<Settings> settingsExpression = null)
		{
			var defaultSettings = new Settings();
			configuration.GetSection("TasksOnTime").Bind(defaultSettings);
			if (settingsExpression != null)
			{
				var s = new Settings();
				settingsExpression.Invoke(s);
			}
			services.AddSingleton(defaultSettings);
			services.AddSingleton<TasksHost>();
			services.AddLogging();
			return services;
		}

		public static U RetryGetValue<T, U>(this ConcurrentDictionary<T, U> dictionary, T key, int loopMax = 3)
		{
			U result = default(U);
			var loop = 0;
			while (true)
			{
				if (loop >= loopMax)
				{
					break;
				}

				if (!dictionary.ContainsKey(key))
				{
					break;
				}
				if (!dictionary.TryGetValue(key, out result))
				{
					System.Threading.Thread.Sleep(500);
					loop++;
					continue;
				}

				break;
			}
			return result;
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
