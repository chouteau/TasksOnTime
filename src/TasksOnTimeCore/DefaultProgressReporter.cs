using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
	public class DefaultProgressReporter : IProgressReporter
	{
		public DefaultProgressReporter(ILogger<DefaultProgressReporter> logger)
		{
			this.Logger = logger;
		}

		protected ILogger<DefaultProgressReporter> Logger { get; }

		public void Notify(ProgressInfo info)
		{
			Logger.LogDebug(info.Subject);
		}
	}
}
