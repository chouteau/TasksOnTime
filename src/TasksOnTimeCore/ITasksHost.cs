﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public interface ITasksHost
	{
		event EventHandler<Guid> TaskFailed;
		event EventHandler<Guid> TaskStarted;
		event EventHandler<Guid> TaskTerminated;
		event EventHandler<Guid> TaskCanceled;

		void Cancel(Guid key);
		void Cleanup();

		Task Enqueue(Guid key, Type taskType, Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null, bool force = false);
        Task Enqueue(Type taskType, Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null);
        Task Enqueue<T>(Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null, bool force = false) where T : class, ITask;
        Task Enqueue<T>(Guid key, Dictionary<string, object> inputParameters = null, Action<Dictionary<string, object>> completed = null, Action<Exception> failed = null, int? delayInMillisecond = null, bool force = false) where T : class, ITask;

		Task ExecuteSubTask<T>(ExecutionContext ctx, Dictionary<string, object> parameters = null);

		bool Exists(Guid key);
		TaskHistory GetHistory(Guid id);
		IEnumerable<TaskHistory> GetHistory(string scheduledTaskName);
		bool IsRunning();
		bool IsRunning(Guid key);
		void Stop();
	}
}