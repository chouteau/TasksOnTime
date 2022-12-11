using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime;

namespace DistributedTasksOnTime.DemoClient
{
	internal class FailTask : TasksOnTime.ITask
	{
		public async Task ExecuteAsync(TasksOnTime.ExecutionContext context)
		{
			context.StartNotification("", "FailTask Started");
			await Task.Delay(10 * 1000);
			throw new ApplicationException("failed");
		}
	}
}
