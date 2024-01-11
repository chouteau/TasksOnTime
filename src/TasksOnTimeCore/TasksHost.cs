using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class TasksHost : ITasksHost
	{
		public event EventHandler<Guid> TaskStarted;
		public event EventHandler<Guid> TaskTerminated;
		public event EventHandler<Guid> TaskFailed;
		public event EventHandler<Guid> TaskCanceled;

		public TasksHost(ILogger<TasksHost> logger,
			IServiceScopeFactory serviceScopeFactory,
			TasksOnTimeSettings settings,
			IProgressReporter progressReporter)
		{
			TaskHistoryList = new ConcurrentDictionary<Guid, TaskHistory>();
			this.Logger = logger;
			this.ServiceScopeFactory = serviceScopeFactory;
			this.Settings = settings;
			this.ProgressReporter = progressReporter;
		}

		protected ILogger<TasksHost> Logger { get; }
		protected IServiceScopeFactory ServiceScopeFactory { get; }
		internal ConcurrentDictionary<Guid, TaskHistory> TaskHistoryList { get; set; }
		protected TasksOnTimeSettings Settings { get; }
		protected IProgressReporter ProgressReporter { get; }

		public async Task Enqueue(
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null)
		{
			await Enqueue(Guid.NewGuid(), null, taskType, inputParameters, completed, failed, delayInMillisecond);
		}

		public async Task Enqueue<T>(
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null,
			bool force = false)
			where T : class, ITask
		{
            await Enqueue(Guid.NewGuid(), null, typeof(T), inputParameters, completed, failed, delayInMillisecond, IsForced: force);
		}

		public async Task Enqueue(Guid key,
			Type taskType,
			Dictionary<string, object> inputParameters = null,
			Action<Dictionary<string, object>> completed = null,
			Action<Exception> failed = null,
			int? delayInMillisecond = null,
			bool force = false)
		{
            await Enqueue(key, null, taskType, inputParameters, completed, failed, delayInMillisecond, IsForced: force);
		}

		public async Task Enqueue<T>(Guid key,
				Dictionary<string, object> inputParameters = null,
				Action<Dictionary<string, object>> completed = null,
				Action<Exception> failed = null,
				int? delayInMillisecond = null,
				bool force = false)
			where T : class, ITask
		{
            await Enqueue(key, null, typeof(T), inputParameters, completed, failed, delayInMillisecond, IsForced: force);
		}

		public async System.Threading.Tasks.Task ExecuteSubTask<T>(ExecutionContext ctx, Dictionary<string, object> parameters = null)
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
			await ExecuteTask(clone);
			if (ctx.Exception != null)
			{
				throw ctx.Exception;
			}
			ctx.Parameters = clone.Parameters;
		}

		internal async Task Enqueue(Guid key,
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
				&& delayInMillisecond.Value < 0)
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

			while (true)
			{
				if (loop > 5)
				{
					break;
				}
                loop++;
                if (!TaskHistoryList.TryAdd(context.Id, history))
				{
					await Task.Delay(500);
					continue;
				}
				break;
			}

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ExecuteTask(context, delayInMillisecond);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await Task.Yield();
        }

        protected virtual void RunTask(ExecutionContext context, int? delayInMillisecond = null)
        {
            ExecuteTask(context, null).Wait();
        }

        internal async System.Threading.Tasks.Task ExecuteTask(ExecutionContext ctx, int? delayInMillisecond = null)
		{
			if (delayInMillisecond.HasValue)
			{
				await System.Threading.Tasks.Task.Delay(delayInMillisecond.Value);
			}

			TaskHistoryList.TryGetValue(ctx.Id, out TaskHistory h);
			h = h ?? new TaskHistory();

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
					var scope = ServiceScopeFactory.CreateScope();
					var sp = scope.ServiceProvider;
					taskInstance = (ITask)ActivatorUtilities.GetServiceOrCreateInstance(sp, ctx.TaskType);
				}
			}
			catch (Exception ex)
			{
				ex.Data.Add("Task", ctx.TaskType);
				Logger.LogError(ex, ex.Message);
				return;
			}

			if (taskInstance == null)
			{
				Logger.LogWarning($"taskInstance is null for {ctx.TaskType}");
				return;
			}

			try
			{
				h.StartedDate = DateTime.Now;
				await taskInstance.ExecuteAsync(ctx);
				h.TerminatedDate = DateTime.Now;
				if (ctx.IsCancelRequested)
				{
					h.CanceledDate = DateTime.Now;
					TaskCanceled?.Invoke(this, ctx.Id);
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

					var disposable = taskInstance as IDisposable;
					if (disposable != null)
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
			TaskHistoryList.TryGetValue(key, out TaskHistory task);
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
				TaskHistoryList.TryGetValue(key, out TaskHistory item);
				if (item != null
					&& !item.TerminatedDate.HasValue)
				{ 
					result = true;
					break;
				}
			}
			return result;
		}

		internal bool IsRunning(string taskName)
		{
			bool result = false;
			foreach (var key in TaskHistoryList.Keys)
			{
				TaskHistoryList.TryGetValue(key, out TaskHistory item);
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

		public void Cancel(string taskName)
		{
			foreach (var key in TaskHistoryList.Keys)
			{
				TaskHistoryList.TryGetValue(key, out TaskHistory item);
				if (item != null
					&& taskName.Equals(item.Name)
					&& item.StartedDate.HasValue
					&& !item.TerminatedDate.HasValue)
				{
					Logger.LogDebug("Cancel activity {0} requested named {1}", key, taskName);
					item.Context.IsCancelRequested = true;
					break;
				}
			}
		}

		public void Cancel(Guid key)
		{
			if (TaskHistoryList.TryGetValue(key, out TaskHistory existing))
			{
				if (existing == null)
				{
					Logger.LogDebug("Activity {0} not found", key);
					return;
				}
				if (existing.TerminatedDate.HasValue)
				{
					Logger.LogDebug("Activity {0} is terminated", key);
					return;
				}
				Logger.LogDebug("Cancel activity {0} requested", key);
				existing.Context.IsCancelRequested = true;
			}
			else
			{
				Logger.LogDebug("Try to get activity {0} failed", key);
			};
		}

		public bool Exists(Guid key)
		{
			TaskHistoryList.TryGetValue(key, out TaskHistory history);
			return history != null;
		}

		public void Cleanup()
		{
			var removeList = new ConcurrentBag<Guid>();
			foreach (var key in TaskHistoryList.Keys)
			{
				TaskHistoryList.TryGetValue(key, out TaskHistory item);
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
			TaskHistoryList.TryGetValue(id, out TaskHistory history);
			return history;
		}

		public IEnumerable<TaskHistory> GetHistory(string scheduledTaskName)
		{
			var result = new ConcurrentBag<TaskHistory>(); 
			if (string.IsNullOrWhiteSpace(scheduledTaskName))
			{
				return result;
			}
			foreach (var key in TaskHistoryList.Keys)
			{
				TaskHistoryList.TryGetValue(key, out TaskHistory item);
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
				TaskHistoryList.TryGetValue(key, out TaskHistory item);
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
