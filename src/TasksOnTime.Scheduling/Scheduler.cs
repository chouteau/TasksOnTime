using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TasksOnTime.Scheduling
{
	public class Scheduler 
	{
        public static Lazy<Scheduler> m_LazyInstance = new Lazy<Scheduler>(() =>
        {
            return new Scheduler();
        });

		private Scheduler()
		{
			ScheduledTaskList = new SynchronizedCollection<ScheduledTask>();
            Terminated = false;
		}

        internal static Scheduler Current
        {
            get
            {
                return m_LazyInstance.Value;
            }
        }

        protected ManualResetEvent EventStop { get; set; }
        protected ManualResetEvent EventForceTask { get; set; }
        protected bool Terminated { get; set;}
        protected Thread TimerThread { get; set; }
        protected SynchronizedCollection<ScheduledTask> ScheduledTaskList { get; set; }

        public event Action<string,Exception> TaskFailed;
		public event Action<string> TaskStarted;
		public event Action<string> TaskFinished;

        public static void Stop()
		{
            Current.Terminated = true;
            if (Current.EventStop != null)
            {
                Current.EventStop.Set();
            }

			// Waiting 5 secondes before kill process
			if (Current.TimerThread != null 
				&& !Current.TimerThread.Join(TimeSpan.FromSeconds(5)))
			{
                Current.TimerThread.Abort();
			}

            foreach (var item in Current.ScheduledTaskList)
            {
                item.Dispose();
            }
		}

		public static void Start()
		{
			if (Current.TimerThread != null)
			{
				return;
			}
            Current.EventStop = new ManualResetEvent(false);
            Current.EventForceTask = new ManualResetEvent(false);
            Current.TimerThread = new Thread(new ThreadStart(Current.ProcessNextTasks));
            Current.TimerThread.Name = "TasksOnTime.SchedulerService";
            Current.TimerThread.Start();
		}

        public static ScheduledTask CreateScheduledTask<T>(string name)
            where T : class
        {
            if (!typeof(T).GetInterfaces().Contains(typeof(ITask)))
            {
                throw new Exception("task must implements ITask");
            }
            var task = new ScheduledTask();
            task.Name = name;
            task.Enabled = !GlobalConfiguration.Settings.ScheduledTaskDisabledByDefault;
            task.TaskType = typeof(T);
            task.NextRunningDate = DateTime.MinValue;
            task.CreationDate = DateTime.Now;
            task.StartedCount = 0;
            task.Enabled = true;
            task.AllowMultipleInstance = true;

            return task;
        }

        #region Static

        public static bool Contains(string taskName)
        {
            bool result = false;
            if (Current.ScheduledTaskList == null || Current.ScheduledTaskList.Count == 0)
            {
                return false;
            }
            lock (Current.ScheduledTaskList.SyncRoot)
            {
                result = Current.ScheduledTaskList.Where(i => i.Name != null).Any(i => i.Name.Equals(taskName));
            }
            return result;
        }

        public static void Add(ScheduledTask task)
		{
            lock (Current.ScheduledTaskList.SyncRoot)
            {
                if (Current.ScheduledTaskList.Any(i => i.Name.Equals(task.Name, StringComparison.InvariantCultureIgnoreCase)))
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
			    if (GlobalConfiguration.Settings.ScheduledTaskDisabledByDefault)
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

                Current.ScheduledTaskList.Add(task);
                Current.EventForceTask.Set();
            }
		}

        public static void Remove(string taskName)
        {
            lock(Current.ScheduledTaskList.SyncRoot)
            {
                var task = Current.ScheduledTaskList.SingleOrDefault(i => i.Name.Equals(taskName, StringComparison.InvariantCultureIgnoreCase));
                if (task != null)
                {
					if (TasksHost.IsRunning(task.Name))
                    {
						var h = TasksHost.GetHistory(task.Name).LastOrDefault();
						if (h != null && h.Context != null)
						{
							h.Context.IsCancelRequested = true;
						}
					}
                    Current.ScheduledTaskList.Remove(task);
                }
            }
        }

		public static void ForceTask(string taskName)
		{
            ScheduledTask task = null;
            lock(Current.ScheduledTaskList.SyncRoot)
            {
                task = Current.ScheduledTaskList.FirstOrDefault(i => i.Name == taskName);
                if (task == null)
                {
                    return;
                }
            }
            task.DelayedStartInMillisecond = 0;
			task.NextRunningDate = DateTime.MinValue;
			if (Current.EventForceTask != null)
			{
                Current.EventForceTask.Set();
			}
		}

        public static void ResetScheduledTaskList()
        {
            lock(Current.ScheduledTaskList.SyncRoot)
            {
                Current.ScheduledTaskList.Clear();
            }
        }

        public static int GetScheduledTaskCount()
        {
            return Current.ScheduledTaskList.Count;
        }

        public static IEnumerable<ScheduledTask> GetList()
        {
            return Current.ScheduledTaskList;
        }

        #endregion  

        internal virtual void ProcessNextTasks()
		{
			while (!Terminated)
			{
				ProcessNextTasks(DateTime.Now);
				var waitHandles = new WaitHandle[] { EventStop, EventForceTask };
				int result = ManualResetEvent.WaitAny(waitHandles, GlobalConfiguration.Settings.IntervalInSeconds * 1000, true);
				if (result == 0)
				{
					Terminated = true;
					break;
				}
				EventForceTask.Reset();
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
