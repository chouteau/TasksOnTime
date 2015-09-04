using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime.Scheduling
{
	public class ScheduledTask : IDisposable
	{
		internal ScheduledTask()
		{
		}

        public string Name { get; set; }

        internal DateTime NextRunningDate { get; set; }
		public ScheduledTaskTimePeriod Period { get; set; }
		internal int Interval { get; set; }
        internal int StartDay { get; set; }
        internal int StartHour { get; set; }
        internal int StartMinute { get; set; }
        internal int DelayedStartInMillisecond { get; set; }
        internal bool IsQueued { get; set; }

		public Exception Exception { get; set; }

		public Action Started { get; set; }
		public Action Completed { get; set; }
		public Action Terminated { get; set; }

		public DateTime CreationDate { get; internal set; }
		// public DateTime StartDate { get; set; }
		// public DateTime? TerminatedDate { get; set; }
		internal Type TaskType { get; set; }

		public int StartedCount { get; set; }
		public bool Enabled { get; set; }
		internal bool AllowMultipleInstance { get; set; }

		#region IDisposable Members

		public void Dispose()
		{
			Started = null;
			Completed = null;
			Terminated = null;
			TaskType = null;
		}

		#endregion
	}
}
