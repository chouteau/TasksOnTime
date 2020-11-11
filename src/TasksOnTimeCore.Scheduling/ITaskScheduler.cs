using System;
using System.Collections.Generic;

namespace TasksOnTime.Scheduling
{
	public interface ITaskScheduler
	{
		DateTime LastSignal { get; set; }

		event Action<string, Exception> TaskFailed;
		event Action<string> TaskFinished;
		event Action<string> TaskStarted;

		void Add(ScheduledTask task);
		bool Contains(string taskName);
		ScheduledTask CreateScheduledTask<T>(string name, Dictionary<string, object> parameters = null) where T : class;
		void ForceTask(string taskName);
		IEnumerable<ScheduledTask> GetList();
		int GetScheduledTaskCount();
		void Remove(string taskName);
		void RemoveAll();
		void ResetScheduledTaskList();
		void Start();
		void Stop();
	}
}