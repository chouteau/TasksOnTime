using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Threading;

namespace TasksOnTime
{
	public class TasksService : ITasksService
	{
		private ManualResetEvent m_EventStop;
		private ManualResetEvent m_ForceTask;
		private bool m_Terminated = false;
		private Thread m_Thread;
		protected SynchronizedCollection<TaskEntry> m_ScheduledTaskList;

		public TasksService()
		{
			m_ScheduledTaskList = new SynchronizedCollection<TaskEntry>();
		}

		public event Action TimerElapsed;
		public event Action<string,Exception> TaskFailed;
		public event Action<string> TaskStarted;
		public event Action<string> TaskFinished;

		public virtual void Stop()
		{
			Terminate();
			// Waiting 5 secondes before kill process
			if (m_Thread != null && !m_Thread.Join(TimeSpan.FromSeconds(5)))
			{
				m_Thread.Abort();
			}
		}

		public void Terminate()
		{
			m_Terminated = true;
			if (m_EventStop != null)
			{
				m_EventStop.Set();
			}
			ActivityHoster.Current.Stop();
		}

		public virtual void Start()
		{
			m_EventStop = new ManualResetEvent(false);
			m_ForceTask = new ManualResetEvent(false);
			m_Thread = new Thread(new ThreadStart(ProcessNextTasks));
			m_Thread.Name = "TasksOnTime.TasksService";
			m_Thread.Start();
		}

		public bool Contains(string taskName)
		{
			bool result = false;
			if (m_ScheduledTaskList == null || m_ScheduledTaskList.Count == 0)
			{
				return false;
			}
			lock (m_ScheduledTaskList.SyncRoot)
			{
				result = m_ScheduledTaskList.Where(i => i.Name != null).Any(i => i.Name.Equals(taskName));
			}
			return result;
		}

		public TaskEntry CreateTask(string name)
		{
			var task = new TaskEntry();
			task.Name = name;
			task.Enabled = !GlobalConfiguration.Settings.DisabledByDefault;
			return task;
		}

		public virtual void Add(TaskEntry task)
		{
			var runnable = GlobalConfiguration.Settings[task.Name];
			if (GlobalConfiguration.Settings.DisabledByDefault)
			{
				if (runnable == null)
				{
					return;
				}

				if (runnable != "enabled")
				{
					return;
				}
				task.Enabled = true;
			}
			else if (runnable != null)
			{
				task.Enabled = runnable != "disabled";
			}
			if (!task.Enabled)
			{
				return;
			}
			GlobalConfiguration.Logger.Info("Add task {0} scheduling",  task.Name);
			m_ScheduledTaskList.Add(task);
		}

		/*
		public virtual IDictionary<string, object> ExecuteScheduledTask(string taskName)
		{
			var task = m_ScheduledTaskList.SingleOrDefault(i => taskName.Equals(i.Name, StringComparison.InvariantCultureIgnoreCase));
			if (task == null)
			{
				throw new KeyNotFoundException();
			}

			return ExecuteTask(task);
		}

		private IDictionary<string, object> ExecuteTask(TaskEntry task)
		{
			var activity = task.GetActivityInstance();
			var wi = new WorkflowInvoker(activity);
			if (TaskStarted != null)
			{
				TaskStarted.Invoke(task.Name);
			}
			var output = wi.Invoke(task.WorkflowProperties);
			if (TaskFinished != null)
			{
				TaskFinished.Invoke(task.Name);
			}
			return output;
		}
		 */

		public virtual void ForceTask(string taskName)
		{
			var task = m_ScheduledTaskList.FirstOrDefault(i => i.Name == taskName);
			if (task == null)
			{
				return;
			}
			task.DelayedStart = 0;
			task.NextRunningDate = DateTime.MinValue;
			if (m_ForceTask != null)
			{
				m_ForceTask.Set();
			}
		}

		// For tests
		internal virtual void ProcessNextTasks()
		{
			while (!m_Terminated)
			{
				ProcessNextTasks(DateTime.Now);
				var waitHandles = new WaitHandle[] { m_EventStop, m_ForceTask };
				int result = ManualResetEvent.WaitAny(waitHandles, GlobalConfiguration.Settings.IntervalInSeconds * 1000, true);
				if (result == 0)
				{
					m_Terminated = true;
					break;
				}
				m_ForceTask.Reset();
				ActivityHoster.Current.Cleanup();
				if (TimerElapsed != null)
				{
					TimerElapsed.Invoke();
				}
			}
		}

		/// <summary>
		/// Recupération de la première tache executable 
		/// dans l'ordre FIFO
		/// </summary>
		/// <returns></returns>
		internal virtual void ProcessNextTasks(DateTime now)
		{
			IEnumerable<TaskEntry> list = null;
			lock (m_ScheduledTaskList.SyncRoot)
			{
				list = m_ScheduledTaskList
					.Where(i => !i.IsRunning && CanRun(now, i))
					.OrderBy(i => i.CreationDate)
					.ToList();

			}
			if (list != null && list.Count() > 0)
			{
				foreach (var item in list)
				{
					if (item.IsParallelizable)
					{
						ProcessTaskAsync(item);
					}
					else
					{
						try
						{
							ProcessTask(item);
						}
						catch (Exception ex)
						{
							ex.Data.Add("TaskName", item.Name);
							GlobalConfiguration.Logger.Error(ex);
						}
					}
				}
			}
		}

		internal virtual void ProcessTaskAsync(TaskEntry taskEntry)
		{
			var callback = new System.Threading.WaitCallback((object state) =>
			{
				var te = state as TaskEntry;
				try
				{
					ProcessTask(te);
				}
				catch (Exception ex)
				{
					ex.Data.Add("TaskName", te.Name);
					GlobalConfiguration.Logger.Error(ex);
				}
			});
			System.Threading.ThreadPool.QueueUserWorkItem(callback, taskEntry);
		}

		internal virtual void ProcessTask(TaskEntry taskEntry)
		{
			if (taskEntry.IsRunning 
				&& !taskEntry.AllowMultipleInstance)
			{
				return;
			}

			var activityId = Guid.NewGuid().ToString();
			if (!taskEntry.AllowMultipleInstance)
			{
				activityId = taskEntry.Name;
				if (ActivityHoster.Current.IsRunning(activityId))
				{
					return;
				}
			}
			var activity = taskEntry.GetActivityInstance();

			taskEntry.IsRunning = true;
			taskEntry.StartDate = DateTime.Now;
			taskEntry.StartedCount += 1;
			if (taskEntry.Started != null)
			{
				taskEntry.Started.Invoke();
			}
			taskEntry.ActivityTrackerId = activityId;

			if (TaskStarted != null)
			{
				TaskStarted.Invoke(taskEntry.Name);
			}

			ActivityHoster.Current.Run(
				activityId
				, activity
				, taskEntry.WorkflowProperties
				, (dic) =>
				{
					try
					{
						foreach (var outputParameter in dic)
						{
							taskEntry.WorkflowProperties[outputParameter.Key] = outputParameter.Value;
						}

						if (taskEntry.Completed != null)
						{
							taskEntry.Completed(dic);
						}
					}
					catch (Exception ex)
					{
						GlobalConfiguration.Logger.Error(ex);
					}
					finally
					{
						taskEntry.IsRunning = false;
					}
				}
				, (ex) =>
				{
					taskEntry.Exception = ex;
					GlobalConfiguration.Logger.Error(ex);
					if (TaskFailed != null)
					{
						TaskFailed.Invoke(taskEntry.Name, ex);
					}
				}
				, (ex) =>
				{
					taskEntry.IsRunning = false;
				});
		}

		public int GetScheduledTaskCount()
		{
			return m_ScheduledTaskList.Count;
		}

		public IEnumerable<TaskEntry> GetList()
		{
			return m_ScheduledTaskList;
		}

		internal bool CanRun(DateTime now, TaskEntry taskEntry)
		{
			if (taskEntry.IsRunning)
			{
				return false;
			}

			// Delayed start
			if (now < taskEntry.CreationDate.AddSeconds(taskEntry.DelayedStart))
			{
				return false;
			}

			if (now >= taskEntry.NextRunningDate
				|| taskEntry.StartedCount == 0)
			{
				switch (taskEntry.Period)
				{
					case ScheduledTaskTimePeriod.Month:

						taskEntry.NextRunningDate = new DateTime(now.Year, now.Month, taskEntry.StartDay, taskEntry.StartHour, taskEntry.StartMinute, 0).AddMonths(taskEntry.Interval);

						break;
					case ScheduledTaskTimePeriod.Day:

						taskEntry.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, taskEntry.StartHour, taskEntry.StartMinute, 0).AddDays(taskEntry.Interval);
						break;

					case ScheduledTaskTimePeriod.WorkingDay:

						while (true)
						{
							taskEntry.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(taskEntry.Interval);

							if (taskEntry.NextRunningDate.DayOfWeek == DayOfWeek.Saturday
								|| taskEntry.NextRunningDate.DayOfWeek == DayOfWeek.Sunday)
							{
								taskEntry.NextRunningDate = taskEntry.NextRunningDate.AddDays(1);
								break;
							}
						}

						break;
					case ScheduledTaskTimePeriod.Hour:

						taskEntry.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(taskEntry.Interval);

						break;
					case ScheduledTaskTimePeriod.Minute:

						taskEntry.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(taskEntry.Interval);

						break;
				}

				return true;
			}
			return false;
		}
	}
}
