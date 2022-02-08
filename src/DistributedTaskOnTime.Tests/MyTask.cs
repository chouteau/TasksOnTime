using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TasksOnTime;

namespace DistributedTaskOnTime.Tests
{
	internal class MyTask : TasksOnTime.ITask
	{
		public Task ExecuteAsync(ExecutionContext context)
		{
			context.StartNotification("test", "Start MyTask");
			Console.WriteLine(DateTime.Now);
			context.CompletedNotification("test", "MyTask Completed");
			return Task.CompletedTask;
		}
	}
}
