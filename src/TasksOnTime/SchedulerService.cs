using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Threading;

namespace TasksOnTime
{
	internal class SchedulerService 
	{
		private ManualResetEvent m_EventStop;
		private ManualResetEvent m_ForceTask;
		private bool m_Terminated = false;
		private Thread m_Thread;

		public SchedulerService()
		{
			ScheduledTaskList = new SynchronizedCollection<ScheduledTask>();
		}

		public event Action<string,Exception> TaskFailed;
		public event Action<string> TaskStarted;
		public event Action<string> TaskFinished;
        protected SynchronizedCollection<ScheduledTask> ScheduledTaskList { get; set; }

        public virtual void Stop()
		{
            m_Terminated = true;
            if (m_EventStop != null)
            {
                m_EventStop.Set();
            }

			// Waiting 5 secondes before kill process
			if (m_Thread != null && !m_Thread.Join(TimeSpan.FromSeconds(5)))
			{
				m_Thread.Abort();
			}

            foreach (var item in ScheduledTaskList)
            {
                item.Dispose();
            }
		}

		public virtual void Start()
		{
            m_EventStop = new ManualResetEvent(false);
			m_ForceTask = new ManualResetEvent(false);
			m_Thread = new Thread(new ThreadStart(ProcessNextTasks));
			m_Thread.Name = "TasksOnTime.SchedulerService";
			m_Thread.Start();
		}

		public bool Contains(string taskName)
		{
			bool result = false;
			if (ScheduledTaskList == null || ScheduledTaskList.Count == 0)
			{
				return false;
			}
			lock (ScheduledTaskList.SyncRoot)
			{
				result = ScheduledTaskList.Where(i => i.Name != null).Any(i => i.Name.Equals(taskName));
			}
			return result;
		}

		public ScheduledTask CreateTask(string name)
		{
            if (name == null || name.Trim() == string.Empty)
            {
                throw new ArgumentException("the name of task does not be null");
            }

			var task = new ScheduledTask();
			task.Name = name;
			task.Enabled = !GlobalConfiguration.Settings.DisabledByDefault;
			return task;
		}

		internal virtual void Add(ScheduledTask task)
		{
            lock (ScheduledTaskList.SyncRoot)
            {
                if (ScheduledTaskList.Any(i => i.Name.Equals(task.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new Exception(string.Format("this task with name {0} is already registered", task.Name));
                }

                if (task.TaskType == null)
                {
                    throw new Exception(string.Format("task {0} must contains GetTask delegate", task.Name));
                }

                if (task.Interval <= 0)
                {
                    throw new Exception(string.Format("interval for task {0} must be greater than zero", task.Name));
                }

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

                if (task.Period == ScheduledTaskTimePeriod.Second)
                {
                    GlobalConfiguration.Settings.IntervalInSeconds = Math.Min(GlobalConfiguration.Settings.IntervalInSeconds, task.Interval);
                }

                ScheduledTaskList.Add(task);
            }
		}

        public void Remove(string taskName)
        {
            lock(ScheduledTaskList.SyncRoot)
            {
                var task = ScheduledTaskList.SingleOrDefault(i => i.Name.Equals(taskName, StringComparison.InvariantCultureIgnoreCase));
                if (task != null)
                {
                    ScheduledTaskList.Remove(task);
                }
            }
        }

		public virtual void ForceTask(string taskName)
		{
            ScheduledTask task = null;
            lock(ScheduledTaskList.SyncRoot)
            {
                task = ScheduledTaskList.FirstOrDefault(i => i.Name == taskName);
                if (task == null)
                {
                    return;
                }
            }
            task.DelayedStartInMillisecond = 0;
			task.NextRunningDate = DateTime.MinValue;
			if (m_ForceTask != null)
			{
				m_ForceTask.Set();
			}
		}

        public void ResetScheduledTaskList()
        {
            lock(ScheduledTaskList.SyncRoot)
            {
                ScheduledTaskList.Clear();
            }
        }

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
			}
		}

		/// <summary>
		/// Recupération de la première tache executable 
		/// dans l'ordre FIFO
		/// </summary>
		/// <returns></returns>
		internal virtual void ProcessNextTasks(DateTime now)
		{
			IEnumerable<ScheduledTask> list = null;
			lock (ScheduledTaskList.SyncRoot)
			{
				list = ScheduledTaskList
					.Where(i => CanRun(now, i))
					.OrderBy(i => i.CreationDate)
					.ToList();

			}
			if (list != null && list.Count() > 0)
			{
				foreach (var item in list)
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

		internal virtual void ProcessTask(ScheduledTask scheduledTask)
		{
			if (scheduledTask.IsQueued 
				&& !scheduledTask.AllowMultipleInstance)
			{
				return;
			}

			if (!scheduledTask.AllowMultipleInstance)
			{
				if (TasksHost.IsRunning(scheduledTask.Name))
				{
					return;
				}
			}

            var id = Guid.NewGuid();
            scheduledTask.IsQueued = true;

            TasksHost.Enqueue(
                id
                , scheduledTask.Name
                , scheduledTask.TaskType
                , null
                , (dic) =>
                {
                    scheduledTask.IsQueued = false;
                    try
                    {
                        if (scheduledTask.Completed != null)
                        {
                            scheduledTask.Completed();
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalConfiguration.Logger.Error(ex);
                    }
                }
                , (ex) =>
                {
                    scheduledTask.Exception = ex;
                    if (TaskFailed != null)
                    {
                        try
                        {
                            TaskFailed.Invoke(scheduledTask.Name, ex);
                        }
                        catch { }
                    }
                },
                null,
                () =>
                {
                    scheduledTask.StartedCount += 1;
                    if (TaskStarted != null)
                    {
                        try
                        {
                            TaskStarted.Invoke(scheduledTask.Name);
                        }
                        catch { }
                    }
                });
		}

		public int GetScheduledTaskCount()
		{
			return ScheduledTaskList.Count;
		}

		public IEnumerable<ScheduledTask> GetList()
		{
			return ScheduledTaskList;
		}

		internal bool CanRun(DateTime now, ScheduledTask scheduledTask)
		{
			if (scheduledTask.IsQueued)
			{
                GlobalConfiguration.Logger.Debug("task {0} with only instance is already queued", scheduledTask.Name);
				return false;
			}

			// Delayed start
			if (scheduledTask.DelayedStartInMillisecond > 0
                && now < scheduledTask.CreationDate.AddSeconds(scheduledTask.DelayedStartInMillisecond))
			{
				return false;
			}

			if (now >= scheduledTask.NextRunningDate
				|| scheduledTask.StartedCount == 0)
			{
				switch (scheduledTask.Period)
				{
					case ScheduledTaskTimePeriod.Month:

						scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, scheduledTask.StartDay, scheduledTask.StartHour, scheduledTask.StartMinute, 0).AddMonths(scheduledTask.Interval);

						break;
					case ScheduledTaskTimePeriod.Day:

						scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, scheduledTask.StartHour, scheduledTask.StartMinute, 0).AddDays(scheduledTask.Interval);
						break;

					case ScheduledTaskTimePeriod.WorkingDay:

                        if (scheduledTask.NextRunningDate.DayOfWeek == DayOfWeek.Saturday
                                || scheduledTask.NextRunningDate.DayOfWeek == DayOfWeek.Sunday)
                        {
                            return false;
                        }

                        scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(scheduledTask.Interval);
                        break;
					case ScheduledTaskTimePeriod.Hour:

						scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(scheduledTask.Interval);

						break;
					case ScheduledTaskTimePeriod.Minute:

						scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(scheduledTask.Interval);

						break;

                    case ScheduledTaskTimePeriod.Second:

                        scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second).AddSeconds(scheduledTask.Interval);
                        break;
				}

				return true;
			}
			return false;
		}
	}
}
