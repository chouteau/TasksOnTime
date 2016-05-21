using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.MonitorApi
{
	public class ScheduledTaskViewModel
	{
		public string Name { get; set; }

		public DateTime NextRunningDate { get; set; }
		public TasksOnTime.Scheduling.ScheduledTaskTimePeriod Period { get; set; }
		public int Interval { get; set; }

		public Exception Exception { get; set; }

		public DateTime CreationDate { get; internal set; }

		public int StartedCount { get; set; }
		public bool Enabled { get; set; }

		public IEnumerable<TaskHistoryViewModel> HistoryList { get; set; }
	}
}
