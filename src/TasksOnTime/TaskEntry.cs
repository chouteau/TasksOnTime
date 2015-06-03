using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public delegate System.Activities.Activity GetActivityInstance();

	public class TaskEntry : IDisposable
	{
		internal TaskEntry()
		{
			NextRunningDate = DateTime.MinValue;
			IsRunning = false;
			CreationDate = DateTime.Now;
			StartedCount = 0;
			Id = Guid.NewGuid();
			IsParallelizable = false;
			Enabled = true;
			AllowMultipleInstance = true;
			WorkflowProperties = new Dictionary<string, object>();
		}

		public Guid Id { get; set; }
		public DateTime NextRunningDate { get; set; }
		public IDictionary<string, object> WorkflowProperties { get; set; }
		public ScheduledTaskTimePeriod Period { get; set; }
		public int Interval { get; set; }
		public int StartDay { get; set; }
		public int StartHour { get; set; }
		public int StartMinute { get; set; }
		public int DelayedStart { get; set; } 
		public bool IsRunning { get; set; }
		public bool IsParallelizable { get; set; }
		public Exception Exception { get; set; }
		public Action Started { get; set; }
		public Action<IDictionary<string, object>> Completed { get; set; }
		public Action Terminated { get; set; }
		public DateTime CreationDate { get; private set; }
		public DateTime StartDate { get; set; }
		public DateTime? TerminatedDate { get; set; }
		public string Name { get; set; }
		public GetActivityInstance GetActivityInstance { get; set; }
		public int StartedCount { get; set; }
		public bool Enabled { get; set; }
		public string ActivityTrackerId { get; set; }
		public bool AllowMultipleInstance { get; set; }

		#region IDisposable Members

		public void Dispose()
		{
			Started = null;
			Completed = null;
			Terminated = null;
			WorkflowProperties = null;
			GetActivityInstance = null;
		}

		#endregion
	}
}
