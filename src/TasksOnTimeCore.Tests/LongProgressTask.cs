using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime.Tests
{
	public class LongProgressTask : ITask
	{
		public void Execute(ExecutionContext context)
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
				System.Threading.Thread.Sleep(1 * 1000);
			}
			context.EndProgressNotification("test");
			context.CompletedNotification("test", "test task completed");

			context.Parameters.AddOrUpdateParameter("completed", true);
		}
	}
}
