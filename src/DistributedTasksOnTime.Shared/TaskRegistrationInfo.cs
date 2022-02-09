using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime
{
	public class TaskRegistrationInfo
	{
		public string TaskName { get; set; }
		public string AssemblyQualifiedName { get; set; }
		public bool AllowMultipleInstances { get; set; } = false;
		public bool AllowLocalMultipleInstances { get; set; } = false;
		public string Description { get; set; }
		public ScheduledTaskTimePeriod DefaultPeriod { get; set; }
		public int DefaultInterval { get; set; }
		public int DefaultStartDay { get; set; } = 1;
		public int DefaultStartHour { get; set; }
		public int DefaultStartMinute { get; set; }

		public bool Enabled { get; set; } = true;
		public Dictionary<string, string> Parameters { get; set; }
	}
}
