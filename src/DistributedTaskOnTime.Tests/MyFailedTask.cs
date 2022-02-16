using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TasksOnTime;

namespace DistributedTaskOnTime.Tests
{
	internal class MyFailedTask : ITask
	{
		public Task ExecuteAsync(ExecutionContext context)
		{
			context.StartNotification("test", "Start MyTask");
			StaticCounter.Increment();
			throw new Exception("Failed");
		}
	}
}
