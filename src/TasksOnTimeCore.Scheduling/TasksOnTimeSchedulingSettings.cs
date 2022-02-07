using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime.Scheduling
{
	public class TasksOnTimeSchedulingSettings : TasksOnTime.TasksOnTimeSettings
	{
		public TasksOnTimeSchedulingSettings()
		{
			TaskNameList = new List<string>();
			IntervalInSeconds = 60; // 1 minute
		}

		public string this[string name]
		{
			get
			{
				return TaskNameList.FirstOrDefault(i => i.Equals(name, StringComparison.InvariantCultureIgnoreCase));
			}
		}

		public int IntervalInSeconds { get; set; }
		public IList<string> TaskNameList { get; set; }
		public bool ScheduledTaskDisabledByDefault { get; set; } = true;
	}
}
