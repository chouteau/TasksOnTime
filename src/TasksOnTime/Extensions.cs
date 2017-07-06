using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public static class Extensions
	{
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

		public static object GetParameter(this Dictionary<string, object> paremeters, string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}

			if (paremeters.ContainsKey(key))
			{
				return paremeters[key];
			}

			return null;
		}
	}
}
