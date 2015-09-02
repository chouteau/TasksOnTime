using System;
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
		}, true);

		private TasksHost()
		{
            TaskHistoryList = new SynchronizedCollection<TaskHistory>();
        }

        internal static TasksHost Current 
		{
			get
			{
				return m_LazyActivityHoster.Value;
			}
		}

        internal SynchronizedCollection<TaskHistory> TaskHistoryList { get; set; }

        public static void Enqueue<T>(
            Dictionary<string, object> inputParameters = null,
            Action<Dictionary<string, object>> completed = null,
            Action<Exception> failed = null,
            int? delayInMillisecond = null)
        {
            Enqueue(Guid.NewGuid(), null, typeof(T), inputParameters, completed, failed, delayInMillisecond);
        }

        public static void Enqueue<T>(Guid key,
                Dictionary<string, object> inputParameters = null,
                Action<Dictionary<string, object>> completed = null,
                Action<Exception> failed = null,
                int? delayInMillisecond = null)
            where T : class
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

            lock (Current.TaskHistoryList.SyncRoot)
            {
                var history = new TaskHistory();
                history.Context = context;
                history.Id = context.Id;
                history.Name = name;
                Current.TaskHistoryList.Add(history);
            }

            Action<object> executeTask = (state) =>
            {
                var ctx = (ExecutionContext)state;
                var h = Current.TaskHistoryList.Single(i => i.Id == ctx.Id);
                if (ctx.Started != null)
                {
                    ctx.Started.Invoke();
                }

                ITask taskInstance = null;
                try
                {
                    taskInstance = (ITask) GlobalConfiguration.DependencyResolver.GetService(ctx.TaskType);
                }
                catch(Exception ex)
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
			lock (Current.TaskHistoryList.SyncRoot)
			{
                var task = Current.TaskHistoryList.SingleOrDefault(i => i.Id == key);
                if (task != null)
                {
                    return task.StartedDate.HasValue && !task.TerminatedDate.HasValue;
                }
                return false;
			}
		}

        public static bool IsRunning()
        {
            bool result = false;
            lock (Current.TaskHistoryList.SyncRoot)
            {
                result = Current.TaskHistoryList.Any(i => !i.TerminatedDate.HasValue);
            }
            return result;
        }

        internal static bool IsRunning(string taskName)
        {
            lock (Current.TaskHistoryList.SyncRoot)
            {
                var task = Current.TaskHistoryList.OrderBy(i => i.CreationDate).FirstOrDefault(i => i.Name == taskName);
                if (task != null)
                {
                    return task.StartedDate.HasValue && !task.TerminatedDate.HasValue;
                }
                return false;
            }
        }

        public static void Cancel(Guid key)
		{
            lock(Current.TaskHistoryList.SyncRoot)
            {
                var existing = Current.TaskHistoryList.SingleOrDefault(i => i.Id == key);
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
        }

		public static bool Exists(Guid key)
		{
            lock(Current.TaskHistoryList.SyncRoot)
            {
                return Current.TaskHistoryList.Any(i => i.Id == key);
            }
        }

		public static void Cleanup()
		{
			lock (Current.TaskHistoryList.SyncRoot)
			{
				var cleanupList = (from instance in Current.TaskHistoryList
                                   where instance.TerminatedDate.HasValue
										&& instance.TerminatedDate.Value.AddSeconds(GlobalConfiguration.Settings.CleanupTimeoutInSeconds) > DateTime.Now
								  select instance.Id).ToList();

				foreach (var item in cleanupList)
				{
					var first = Current.TaskHistoryList.FirstOrDefault(i => i.Id == item);
					if (first == null)
					{
						continue;
					}
					// first.Dispose();
                    Current.TaskHistoryList.Remove(first);
				}
			}
		}

        public static TaskHistory GetHistory(Guid id)
        {
            TaskHistory ai = null;
            lock (Current.TaskHistoryList.SyncRoot)
            {
                ai = Current.TaskHistoryList.SingleOrDefault(i => i.Id == id);
            }

            if (ai == null)
            {
                return null;
            }

            return ai;
        }

        public static IEnumerable<TaskHistory> GetHistory(string scheduledTaskName)
        {
            IEnumerable<TaskHistory> result = null;
            lock (Current.TaskHistoryList.SyncRoot)
            {
                result = Current.TaskHistoryList.Where(i => i.Name.Equals(scheduledTaskName, StringComparison.InvariantCultureIgnoreCase));
            }
            return result;
        }

        #endregion

        public static void Stop()
		{
			lock (Current.TaskHistoryList.SyncRoot)
			{
				foreach (var item in Current.TaskHistoryList)
				{
                    if (item.Context == null || item.TerminatedDate.HasValue)
                    {
                        continue;
                    }
                    item.Context.IsCancelRequested = true;
                    System.Threading.Thread.Sleep(200);
					item.Dispose();
				}
			}
        }

    }
}
