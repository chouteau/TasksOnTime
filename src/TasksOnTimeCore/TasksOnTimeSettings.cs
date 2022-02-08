using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public class TasksOnTimeSettings
	{
		public TasksOnTimeSettings()
		{
			CleanupTimeoutInSeconds = 60 * 10; // 10 minutes
			ProgresReporterType = typeof(DefaultProgressReporter);
        }

		public int CleanupTimeoutInSeconds { get; set; }
		public Type ProgresReporterType { get; set; }
	}
}
