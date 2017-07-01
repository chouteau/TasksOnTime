using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TasksOnTime.Scheduling
{
	[DataContract]
	public class ScheduledTask : IDisposable
	{
		internal ScheduledTask()
		{
			Parameters = new Dictionary<string, object>();
		}

		[DataMember]
        public string Name { get; set; }
		[DataMember]
		public DateTime NextRunningDate { get; set; }
		[IgnoreDataMember]
		public Func<DateTime> NextRunningDateFactory { get; set; }
		[DataMember]
		public ScheduledTaskTimePeriod Period { get; set; }
		[DataMember]
		public int Interval { get; internal set; }
		[DataMember]
		public int StartDay { get; internal set; }
		[DataMember]
		public int StartHour { get; internal set; }
		[DataMember]
		public int StartMinute { get; internal set; }
		[DataMember]
		public int DelayedStartInMillisecond { get; internal set; }
		[DataMember]
		public bool IsQueued { get; internal set; }

		public Exception Exception { get; set; }

		public Action Started { get; set; }
		public Action<Dictionary<string, object>> Completed { get; set; }
		public Action Terminated { get; set; }

		[DataMember]
		public DateTime CreationDate { get; internal set; }
		// public DateTime StartDate { get; set; }
		// public DateTime? TerminatedDate { get; set; }
		[DataMember]
		public Type TaskType { get; internal set; }

		[DataMember]
		public int StartedCount { get; set; }
		[DataMember]
		public bool Enabled { get; set; }
		[DataMember]
		public bool AllowMultipleInstance { get; internal set; }
		[DataMember]
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
