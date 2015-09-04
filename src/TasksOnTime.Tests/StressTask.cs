using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
	public class StressTask : ITask
	{
		public void Execute(ExecutionContext context)
		{
			GlobalConfiguration.Logger.Debug("Start StressTask with thread {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
			context.Parameters["ThreadId"] = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var primeNumber = new List<bool?>(10000);
			for (int i = 0; i < 10000; i++)
			{
				primeNumber.Add(null);
			}
			primeNumber[0] = true;
			primeNumber[1] = true;
			primeNumber[2] = true;
			primeNumber[3] = true;

			int nextPrime = 2;
            while (true)
			{
				if (context.IsCancelRequested)
				{
					break;
				}
				for (int i = nextPrime +1; i < primeNumber.Count; i++)
				{
					if (context.IsCancelRequested)
					{
						break;
					}
					var item = primeNumber[i];
					if (item.HasValue)
					{
						continue;
					}
					var isMultiple = i % nextPrime == 0;
                    if (isMultiple)
					{
						primeNumber[i] = false;
					}
				}

				for (int i = 0; i < primeNumber.Count; i++)
				{
					if (primeNumber[i] == null)
					{
						nextPrime = i;
						break;
					}
				}
				if (primeNumber.Count(i => !i.HasValue) == 0)
				{
					break;
				}
				primeNumber[nextPrime] = true;
			}

			while(true)
			{
				var notPrime = primeNumber.FirstOrDefault(i => !i.Value);
				if (notPrime == null)
				{
					break;
				}
				primeNumber.Remove(notPrime);
			}
			GlobalConfiguration.Logger.Debug("Prime number count {0}", primeNumber.Count);
		}
	}
}
