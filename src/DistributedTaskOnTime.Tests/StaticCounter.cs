using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTaskOnTime.Tests
{
	internal static class StaticCounter
	{
		private static int counter = 0;
		public static void Increment()
		{
			counter++;
		}

		public static int CounterValue => counter;

		public static void Reset()
		{
			counter = 0;
		}
	}
}
