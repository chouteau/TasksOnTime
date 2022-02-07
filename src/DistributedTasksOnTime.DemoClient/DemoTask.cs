using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.DemoClient
{
	internal class DemoTask : TasksOnTime.ITask
	{
		public Task ExecuteAsync(TasksOnTime.ExecutionContext context)
		{
			Console.WriteLine(DateTime.Now);
			return Task.CompletedTask;
		}
	}
}
