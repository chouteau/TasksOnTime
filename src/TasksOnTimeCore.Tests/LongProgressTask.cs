using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
	public class LongProgressTask : ITask
	{
		public async Task ExecuteAsync(ExecutionContext context)
		{
			context.StartNotification("test", "test task started");

			var total = 20;
			var index = 0;
			context.StartProgressNotification("test", "process items", total);
			while(true)
			{
				index++;
				context.ProgressNotification("test", $"progress {index}/{total}");
				if (index > total)
				{
					break;
				}
				await Task.Delay(1 * 1000);
			}
			context.EndProgressNotification("test");
			context.CompletedNotification("test", "test task completed");

			context.Parameters.AddOrUpdateParameter("completed", true);
		}
	}
}
