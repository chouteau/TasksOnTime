using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public class Settings
	{
		public Settings()
		{
			CleanupTimeoutInSeconds = 60 * 10; // 10 minutes
        }

		public int CleanupTimeoutInSeconds { get; set; }
	}
}
