using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public interface ITasksHost
	{
		event EventHandler<Guid> TaskFailed;
		event EventHandler<Guid> TaskStarted;
		event EventHandler<Guid> TaskTerminated;

		void Cancel(Guid key);
		void Cleanup();

		void Enqueue(Guid key, Type taskType, Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null, bool force = false);
		void Enqueue(Type taskType, Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null);
		void Enqueue<T>(Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null, bool force = false) where T : class, ITask;
		void Enqueue<T>(Guid key, Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null, bool force = false) where T : class, ITask;

		Task ExecuteSubTask<T>(ExecutionContext ctx, Dictionary<string, object> parameters = null);

		bool Exists(Guid key);
		TaskHistory GetHistory(Guid id);
		IEnumerable<TaskHistory> GetHistory(string scheduledTaskName);
		bool IsRunning();
		bool IsRunning(Guid key);
		void Stop();
	}
}