﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public class TasksHost
	{
        private static Lazy<TasksHost> m_LazyActivityHoster = new Lazy<TasksHost>(() =>
		{
			return new TasksHost();
		});

		private TasksHost()
		{
            TaskHistoryList = new ConcurrentDictionary<Guid,TaskHistory>();
        }

        internal static TasksHost Current 
		{
			get
			{
				return m_LazyActivityHoster.Value;
			}
		}

        internal ConcurrentDictionary<Guid,TaskHistory> TaskHistoryList { get; set; }

		public static void Enqueue(
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null)
		{
			Enqueue(Guid.NewGuid(), null, taskType, inputParameters, completed, failed, delayInMillisecond);
		}

		public static void Enqueue<T>(
            Dictionary<string, object> inputParameters = null,
            Action<Dictionary<string, object>> completed = null,
            Action<Exception> failed = null,
            int? delayInMillisecond = null)
			where T : class, ITask
		{
            Enqueue(Guid.NewGuid(), null, typeof(T), inputParameters, completed, failed, delayInMillisecond);
        }

		public static void Enqueue(Guid key,
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null)
		{
			Enqueue(key, null, taskType, inputParameters, completed, failed, delayInMillisecond);
		}

		public static void Enqueue<T>(Guid key,
                Dictionary<string, object> inputParameters = null,
                Action<Dictionary<string, object>> completed = null,
                Action<Exception> failed = null,
                int? delayInMillisecond = null)
            where T : class, ITask
        {
            Enqueue(key, null, typeof(T), inputParameters, completed, failed, delayInMillisecond);
        }

        internal static void Enqueue(Guid key,
            string name,
            Type taskType,
            Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null, 
			Action<Exception> failed = null,
            int? delayInMillisecond = null,
            Action started = null)
        {
			if (key == Guid.Empty)
			{
				throw new ArgumentNullException("key parameter required");
			}

			if (!taskType.GetInterfaces().Contains(typeof(ITask)))
			{
				throw new Exception("task must implement ITask");
			}

            if (delayInMillisecond.HasValue 
                && delayInMillisecond.Value <0)
            {
                delayInMillisecond = 0;
            }

			var context = ExecutionContext.Create();
			context.Id = key;
			context.Completed = completed ?? context.Completed;
			context.Failed = failed ?? context.Failed;
            context.Started = started;
            context.TaskType = taskType;
            context.Parameters = inputParameters ?? context.Parameters;

            var history = new TaskHistory();
            history.Context = context;
            history.Id = context.Id;
            history.Name = name;
			var loop = 0;
			while(true)
			{
				if (loop > 5)
				{
					break;
				}
				if (!Current.TaskHistoryList.TryAdd(context.Id, history))
				{
					loop++;
					System.Threading.Thread.Sleep(500);
					continue;
				}
				break;
			}

			Action<object> executeTask = (state) =>
            {
                var ctx = (ExecutionContext)state;
				TaskHistory h = RetryGetValue(ctx.Id) ?? new TaskHistory();
				if (ctx.Started != null)
				{
					ctx.Started.Invoke();
				}

				ITask taskInstance = null;
				try
				{
					if (ctx.TaskType != null)
					{
						taskInstance = (ITask)GlobalConfiguration.DependencyResolver.GetService(ctx.TaskType);
					}
				}
				catch (Exception ex)
				{
					GlobalConfiguration.Logger.Error(ex);
				}

				if (taskInstance == null)
				{
					return;
				}

				try
				{
					h.StartedDate = DateTime.Now;
					taskInstance.Execute(ctx);
					if (ctx.IsCancelRequested)
					{
						h.CanceledDate = DateTime.Now;
					}
				}
				catch (Exception ex)
				{
					ctx.Exception = ex;
					h.Exception = ex;
					if (ctx.Failed != null)
					{
						try
						{
							ctx.Failed(ex);
						}
						catch { }
					}
					GlobalConfiguration.Logger.Error(ex);
				}
				finally
				{
					if (ctx.Completed != null)
					{
						try
						{
							ctx.Completed(ctx.Parameters);
							h.Parameters = ctx.Parameters;
						}
						catch { }
					}
					h.TerminatedDate = DateTime.Now;
					h.Context = null;
					try
					{
						ctx.Dispose();

						if (taskInstance is IDisposable)
						{
							((IDisposable)taskInstance).Dispose();
						}
					}
					catch { }
				}
			};

            if (!delayInMillisecond.HasValue)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(
                    new System.Threading.WaitCallback(executeTask), 
                    context);
            }
            else
            {
                Action<object, bool> timeoutCallback = (state, timeout) =>
                {
                    executeTask.Invoke(context);
                };

                System.Threading.ThreadPool.RegisterWaitForSingleObject(
                    new System.Threading.AutoResetEvent(false),
                    new System.Threading.WaitOrTimerCallback(timeoutCallback), 
                    context, 
                    delayInMillisecond.Value, 
                    true);
            }
        }

        #region Task Management

        public static bool IsRunning(Guid key)
		{
			TaskHistory task = RetryGetValue(key);
            if (task != null)
            {
                return task.StartedDate.HasValue && !task.TerminatedDate.HasValue;
            }
            return false;
		}

        public static bool IsRunning()
        {
            bool result = false;
			foreach (var key in Current.TaskHistoryList.Keys)
			{
				var item = RetryGetValue(key);
				if (item != null)
				{
					if (!item.TerminatedDate.HasValue)
					{
						result = true;
						break;
					}
				}
			}
            return result;
        }

        internal static bool IsRunning(string taskName)
        {
			bool result = false;
			foreach (var key in Current.TaskHistoryList.Keys)
			{
				var item = RetryGetValue(key);
				if (item != null
					&& taskName.Equals(item.Name)
					&& item.StartedDate.HasValue 
					&& !item.TerminatedDate.HasValue)
				{
					result = true;
					break;
				}
			}
			return result;
        }

        public static void Cancel(Guid key)
		{
			var existing = RetryGetValue(key);
            if (existing == null)
            {
                return;
            }
            if (existing.TerminatedDate.HasValue)
            {
                return;
            }
            GlobalConfiguration.Logger.Debug("Cancel activity {0} requested", key);
            existing.Context.IsCancelRequested = true;
        }

		public static bool Exists(Guid key)
		{
			return RetryGetValue(key) != null;
        }

		public static void Cleanup()
		{
			var removeList = new ConcurrentBag<Guid>();
			foreach (var key in Current.TaskHistoryList.Keys)
			{
				var item = RetryGetValue(key);
				if (item == null)
				{
					continue;
				}
				if (item.TerminatedDate.HasValue
					&& item.TerminatedDate.Value.AddSeconds(GlobalConfiguration.Settings.CleanupTimeoutInSeconds) > DateTime.Now)
				{
					removeList.Add(key);
				}
			}
			foreach (var key in removeList)
			{
				TaskHistory item = null;
				Current.TaskHistoryList.TryRemove(key, out item);
			}
		}

        public static TaskHistory GetHistory(Guid id)
        {
            var ai = RetryGetValue(id);
            return ai;
        }

        public static IEnumerable<TaskHistory> GetHistory(string scheduledTaskName)
        {
			var result = new ConcurrentBag<TaskHistory>(); ;
			if (string.IsNullOrWhiteSpace(scheduledTaskName))
			{
				return result;
			}
			foreach (var key in Current.TaskHistoryList.Keys)
			{
				var item = RetryGetValue(key);
				if (item == null)
				{
					continue;
				}
				if (scheduledTaskName.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					result.Add(item);
				}
			}
			return result;
        }

        #endregion

        public static void Stop()
		{
			foreach (var key in Current.TaskHistoryList.Keys)
			{
				var item = RetryGetValue(key);
				if (item == null)
				{
					continue;
				}
                if (item.Context == null || item.TerminatedDate.HasValue)
                {
                    continue;
                }
                item.Context.IsCancelRequested = true;
                System.Threading.Thread.Sleep(200);
				item.Dispose();
			}
        }

		private static TaskHistory RetryGetValue(Guid id)
		{
			TaskHistory result = null;
			var loop = 0;
			while(true)
			{
				if (!Current.TaskHistoryList.ContainsKey(id))
				{
					break;
				}
				if (!Current.TaskHistoryList.TryGetValue(id, out result))
				{
					System.Threading.Thread.Sleep(500);
					loop++;
					continue;
				}
				break;
			}
			return result;
		}
    }
}
