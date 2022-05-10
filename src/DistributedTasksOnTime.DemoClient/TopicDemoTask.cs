using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime;

namespace DistributedTasksOnTime.DemoClient
{
	internal class TopicDemoTask : TasksOnTime.ITask
	{
		public async Task ExecuteAsync(TasksOnTime.ExecutionContext context)
		{
			context.StartNotification("", "Topic DemoTask Started");
			await Task.Delay(10 * 1000);
			Console.WriteLine(DateTime.Now);
			context.WriteNotification("", "Date", $"{DateTime.Now}");
			await Task.Delay(5 * 1000);
			context.CompletedNotification("");
		}
	}
}
