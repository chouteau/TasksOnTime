using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace TasksOnTime
{
	public class SynchronizedTaskHost : TasksOnTime.TasksHost
	{
		public SynchronizedTaskHost(ILogger<TasksHost> logger, IServiceProvider provider, TasksOnTime.Settings settings, IProgressReporter reporter)
			: base(logger, provider, settings, reporter)
		{

		}

		protected override void RunTask(ExecutionContext context, int? delayInMillisecond = null)
		{
			ExecuteTask(context, null).Wait();
		}
	}
}
