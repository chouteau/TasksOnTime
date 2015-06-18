using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class Settings
	{
		public Settings()
		{
			var intervalInSecond = 60;
			int.TryParse(Configuration.ConfigurationSettings.AppSettings["intervalInSeconds"] ?? "60", out intervalInSecond);
			IntervalInSeconds = intervalInSecond;
			DisabledByDefault = true;
			CleanupTimeoutInSeconds = 60 * 10; // 10 minutes
			SynchronizedMode = false;
		}

		public string this[string taskName]
		{
			get
			{
				return Configuration.ConfigurationSettings.AppSettings[taskName];
			}
		}

		public int IntervalInSeconds { get; set; }
		public bool DisabledByDefault { get; set; }
		public int CleanupTimeoutInSeconds { get; set; }

		public bool SynchronizedMode { get; set; }
	}
}
