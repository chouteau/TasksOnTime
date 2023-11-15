using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TasksOnTime
{
	public class SynchronizedTaskHost : TasksOnTime.TasksHost
	{
		public SynchronizedTaskHost(ILogger<TasksHost> logger, IServiceScopeFactory serviceScopeFactory, TasksOnTime.TasksOnTimeSettings settings, IProgressReporter reporter)
			: base(logger, serviceScopeFactory, settings, reporter)
		{

		}

		protected override void RunTask(ExecutionContext context, int? delayInMillisecond = null)
		{
			ExecuteTask(context, null).Wait();
		}
	}
}
