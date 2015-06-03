using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public interface ITasksService
	{
		event Action TimerElapsed;
		event Action<string, Exception> TaskFailed;
		event Action<string> TaskStarted;
		event Action<string> TaskFinished;

		void Start();
		void Stop();
		TaskEntry CreateTask(string name);
		void Add(TaskEntry task);
		int GetScheduledTaskCount();
		bool Contains(string taskName);
		// IDictionary<string, object> ExecuteScheduledTask(string taskName);
		void ForceTask(string taskName);
		void ReplaceActivityHoster(IActivityHoster hoster);
	}
}
