using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TasksOnTime;

namespace DistributedTasksOnTime.DemoClient
{
	public class CancelableTask : ITask
	{
		public async Task ExecuteAsync(TasksOnTime.ExecutionContext context)
		{
			for (int i = 0; i < 30; i++)
			{
				if (context.IsCancelRequested)
				{
					context.WriteNotification("test", $"Canceled by user at {i} loop");
					context.CancelNotification("test");
					break;
				}
				await Task.Delay(1 * 1000);
			}
		}
	}
}
