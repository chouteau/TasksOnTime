using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TasksOnTime
{
	public class TasksHost
	{
		public event EventHandler<Guid> TaskStarted;
		public event EventHandler<Guid> TaskTerminated;
		public event EventHandler<Guid> TaskFailed;

		public TasksHost(ILogger<TasksHost> logger,
			IServiceProvider serviceProvider,
			Settings settings,
			IProgressReporter progressReporter)
		{
            TaskHistoryList = new ConcurrentDictionary<Guid,TaskHistory>();
			this.Logger = logger;
			this.ServiceProvider = serviceProvider;
			this.Settings = settings;
			this.ProgressReporter = progressReporter;
        }

		protected ILogger<TasksHost> Logger { get;  }
		protected IServiceProvider ServiceProvider { get; }
		internal ConcurrentDictionary<Guid,TaskHistory> TaskHistoryList { get; set; }
		protected Settings Settings { get; }
		protected IProgressReporter ProgressReporter { get; }

		public void Enqueue(
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null)
		{
			Enqueue(Guid.NewGuid(), null, taskType, inputParameters, completed, failed, delayInMillisecond);
		}

		public void Enqueue<T>(
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null,
			bool force = false)
			where T : class, ITask
		{
            Enqueue(Guid.NewGuid(), null, typeof(T), inputParameters, completed, failed, delayInMillisecond, IsForced : force);
        }

		public void Enqueue(Guid key,
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null,
			bool force = false)
		{
			Enqueue(key, null, taskType, inputParameters, completed, failed, delayInMillisecond, IsForced : force);
		}

		public void Enqueue<T>(Guid key,
                Dictionary<string, object> inputParameters = null,
                Action<Dictionary<string, object>> completed = null,
                Action<Exception> failed = null,
                int? delayInMillisecond = null,
				bool force = false)
            where T : class, ITask
        {
            Enqueue(key, null, typeof(T), inputParameters, completed, failed, delayInMillisecond, IsForced : force);
        }

		public void ExecuteSubTask<T>(ExecutionContext ctx, Dictionary<string, object> parameters = null)
		{
			var clone = (ExecutionContext)ctx.Clone();
			clone.Parameters = ctx.Parameters ?? new Dictionary<string, object>();
			if (parameters != null)
			{
				foreach (var item in parameters)
				{
					clone.Parameters.AddOrUpdateParameter(item.Key, item.Value);
				}
			}
			clone.TaskType = typeof(T);
			clone.IsSubTask = true;
			ExecuteTask(clone);
			if (ctx.Exception != null)
			{
				throw ctx.Exception;
			}
			ctx.Parameters = clone.Parameters;
		}


		internal void Enqueue(Guid key,
			string name,
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null,
			Action started = null,
			bool IsScheduled = false,
			bool IsForced = false)
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
			context.Force = IsForced;
			context.Progress = ProgressReporter;

            var history = new TaskHistory();
            history.Context = context;
            history.Id = context.Id;
			history.IsScheduled = IsScheduled;
            history.Name = name ?? taskType.FullName;
			var loop = 0;

			while(true)
			{
				if (loop > 5)
				{
					break;
				}
				if (!TaskHistoryList.TryAdd(context.Id, history))
				{
					loop++;
					System.Threading.Thread.Sleep(500);
					continue;
				}
				break;
			}

			Action<object> executeTask = (state) =>
            {
				ExecuteTask((ExecutionContext)state);
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

		internal void ExecuteTask(ExecutionContext ctx)
		{
			var h = TaskHistoryList.RetryGetValue(ctx.Id) 
								?? new TaskHistory();

			if (ctx.Started != null)
			{
				ctx.Started.Invoke();
			}
			if (TaskStarted != null)
			{
				try
				{
					TaskStarted(ctx, ctx.Id);
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, ex.Message);
				}
			}


			ITask taskInstance = null;
			try
			{
				if (ctx.TaskType != null)
				{
					using(var scope = ServiceProvider.CreateScope())
					{
						taskInstance = (ITask)ActivatorUtilities.CreateInstance(scope.ServiceProvider, ctx.TaskType);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, ex.Message);
			}

			if (taskInstance == null)
			{
				return;
			}

			try
			{
				h.StartedDate = DateTime.Now;
				taskInstance.Execute(ctx);
				h.TerminatedDate = DateTime.Now;
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

					if (TaskFailed != null)
					{
						try
						{
							TaskFailed(ctx, ctx.Id);
						}
						catch (Exception gex)
						{
							Logger.LogError(gex, gex.Message);
						}
					}

				}
				Logger.LogError(ex, ex.Message);
			}
			finally
			{
				h.TerminatedDate = DateTime.Now;
				h.Context = null;
				h.Parameters = ctx.Parameters;

				if (ctx.Completed != null)
				{
					try
					{
						ctx.Completed(ctx.Parameters);
					}
					catch { }
				}
				if (TaskTerminated != null)
				{
					try
					{
						TaskTerminated(ctx, ctx.Id);
					}
					catch (Exception ex)
					{
						Logger.LogError(ex, ex.Message);
					}
				}
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

		}

		#region Task Management

		public bool IsRunning(Guid key)
		{
			TaskHistory task = TaskHistoryList.RetryGetValue(key);
            if (task != null)
            {
                return task.StartedDate.HasValue && !task.TerminatedDate.HasValue;
            }
            return false;
		}

        public bool IsRunning()
        {
            bool result = false;
			foreach (var key in TaskHistoryList.Keys)
			{
				var item = TaskHistoryList.RetryGetValue(key);
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

        internal bool IsRunning(string taskName)
        {
			bool result = false;
			foreach (var key in TaskHistoryList.Keys)
			{
				var item = TaskHistoryList.RetryGetValue(key);
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

        public void Cancel(Guid key)
		{
			var existing = TaskHistoryList.RetryGetValue(key);
            if (existing == null)
            {
                return;
            }
            if (existing.TerminatedDate.HasValue)
            {
                return;
            }
            Logger.LogDebug("Cancel activity {0} requested", key);
            existing.Context.IsCancelRequested = true;
        }

		public bool Exists(Guid key)
		{
			return TaskHistoryList.RetryGetValue(key) != null;
        }

		public void Cleanup()
		{
			var removeList = new ConcurrentBag<Guid>();
			foreach (var key in TaskHistoryList.Keys)
			{
				var item = TaskHistoryList.RetryGetValue(key);
				if (item == null)
				{
					continue;
				}
				if (item.TerminatedDate.HasValue
					&& item.TerminatedDate.Value.AddSeconds(Settings.CleanupTimeoutInSeconds) > DateTime.Now)
				{
					removeList.Add(key);
				}
			}
			foreach (var key in removeList)
			{
				TaskHistory item = null;
				TaskHistoryList.TryRemove(key, out item);
			}
		}

        public TaskHistory GetHistory(Guid id)
        {
            var ai = TaskHistoryList.RetryGetValue(id);
            return ai;
        }

        public IEnumerable<TaskHistory> GetHistory(string scheduledTaskName)
        {
			var result = new ConcurrentBag<TaskHistory>(); ;
			if (string.IsNullOrWhiteSpace(scheduledTaskName))
			{
				return result;
			}
			foreach (var key in TaskHistoryList.Keys)
			{
				var item = TaskHistoryList.RetryGetValue(key);
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

        public void Stop()
		{
			foreach (var key in TaskHistoryList.Keys)
			{
				var item = TaskHistoryList.RetryGetValue(key);
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
    }
}
