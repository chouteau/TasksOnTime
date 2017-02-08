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
			Parameters = new Dictionary<string, object>();
		}

        public string Name { get; set; }

        public DateTime NextRunningDate { get; set; }
		public ScheduledTaskTimePeriod Period { get; set; }
		public int Interval { get; internal set; }
        public int StartDay { get; internal set; }
        public int StartHour { get; internal set; }
        public int StartMinute { get; internal set; }
        public int DelayedStartInMillisecond { get; internal set; }
        public bool IsQueued { get; internal set; }

		public Exception Exception { get; set; }

		public Action Started { get; set; }
		public Action<Dictionary<string, object>> Completed { get; set; }
		public Action Terminated { get; set; }

		public DateTime CreationDate { get; internal set; }
		// public DateTime StartDate { get; set; }
		// public DateTime? TerminatedDate { get; set; }
		public Type TaskType { get; internal set; }

		public int StartedCount { get; set; }
		public bool Enabled { get; set; }
		public bool AllowMultipleInstance { get; internal set; }
		public Dictionary<string, object> Parameters { get; set; }

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
