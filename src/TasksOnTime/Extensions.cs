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
	}
}
