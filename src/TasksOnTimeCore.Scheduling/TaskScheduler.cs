﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Logging;


namespace TasksOnTime.Scheduling
{
    public class TaskScheduler : ITaskScheduler
    {
        public event Action<string, Exception> TaskFailed;
        public event Action<string> TaskStarted;
        public event Action<string> TaskFinished;

        public TaskScheduler(TasksOnTimeSchedulingSettings scheduleSettings,
            ILogger<TaskScheduler> logger,
            ITasksHost tasksHost)
        {
            this.ScheduledTaskList = new ConcurrentDictionary<string,ScheduledTask>();
            this.Terminated = false;
            this.LastSignal = DateTime.MinValue;
            this.Settings = scheduleSettings;
            this.Logger = logger;
            this.TasksHost = tasksHost;
        }

        public DateTime LastSignal { get; set; }

        protected ManualResetEvent EventStop { get; set; }
        protected ManualResetEvent EventForceTask { get; set; }
        protected bool Terminated { get; set; }
        protected Thread TimerThread { get; set; }
        protected ConcurrentDictionary<string, ScheduledTask> ScheduledTaskList { get; set; }
        protected TasksOnTimeSchedulingSettings Settings { get; }
        protected ILogger<TaskScheduler> Logger { get; }
        protected ITasksHost TasksHost { get; }

        public void Start()
        {
            Terminated = false;
            if (TimerThread != null)
            {
                return;
            }

            EventStop = new ManualResetEvent(false);
            EventForceTask = new ManualResetEvent(false);

            TimerThread = new Thread(new ThreadStart(ProcessNextTasks));
            TimerThread.Name = $"ToTCore.Scheduler.{Guid.NewGuid()}";
            TimerThread.Start();
        }

        public void Stop()
        {
            Terminated = true;
            if (EventStop != null)
            {
                EventStop.Set();
            }

            var loop = ScheduledTaskList.Count;
            while (true)
            {
                var task = ScheduledTaskList.FirstOrDefault();
                if (task.Value == null)
                {
                    break;
                }
                Remove(task.Key);
                loop--;
                if (loop <= 0)
                {
                    break;
                }
            }

            // Waiting 5 secondes before kill process
            if (TimerThread != null
                && !TimerThread.Join(TimeSpan.FromSeconds(5)))
            {
                // TimerThread.Abort();
            }
        }

        public ScheduledTask CreateScheduledTask<T>(string name, Dictionary<string, object> parameters = null)
            where T : class
        {
            if (!typeof(T).GetInterfaces().Contains(typeof(ITask)))
            {
                throw new NotImplementedException("task must implements ITask");
            }
            var task = new ScheduledTask();
            task.Name = name;
            task.Enabled = !Settings.ScheduledTaskDisabledByDefault;
            task.TaskType = typeof(T);
            task.NextRunningDate = DateTime.MinValue;
            task.CreationDate = DateTime.Now;
            task.StartedCount = 0;
            task.AllowMultipleInstance = true;
            if (parameters != null)
            {
                task.Parameters = parameters;
            }

            return task;
        }

        #region Static

        public bool Contains(string taskName)
        {
            if (taskName == null)
			{
                throw new NullReferenceException("taskName is null");
			}

            taskName = taskName.ToLower();

            bool result = false;
            if (ScheduledTaskList == null || ScheduledTaskList.Count == 0)
            {
                return false;
            }
            result = ScheduledTaskList.ContainsKey(taskName);
            return result;
        }

        public void Add(ScheduledTask task)
        {
            if (task == null)
			{
                throw new NullReferenceException();
			}
            if (task.Name == null)
			{
                throw new ArgumentNullException("task name is null");
			}

            var taskName = task.Name.ToLower();
            if (ScheduledTaskList.ContainsKey(taskName))
            {
                throw new Exception($"this task with name {taskName} is already registered");
            }

            if (task.TaskType == null)
            {
                throw new Exception($"task {taskName} must contains GetTask delegate");
            }

            if (task.Period != ScheduledTaskTimePeriod.Custom
                && task.Interval <= 0)
            {
                throw new Exception($"interval for task {taskName} must be greater than zero");
            }

            var taskFound = Settings[taskName];
            if (Settings.ScheduledTaskDisabledByDefault)
            {
                if (taskFound == null)
                {
                    return;
                }

                task.Enabled = true;
            }
            else if (taskFound != null)
            {
                task.Enabled = true;
            }

            if (!task.Enabled)
            {
                return;
            }

            Logger.LogInformation($"Add task {taskName} scheduling");

            if (task.Period == ScheduledTaskTimePeriod.Second)
            {
                Settings.IntervalInSeconds = Math.Min(Settings.IntervalInSeconds, task.Interval);
            }

            var addResult = ScheduledTaskList.TryAdd(taskName, task);
            if (!addResult)
			{
                Logger.LogWarning($"Try to add task {taskName} failed");
			}

            if (EventForceTask != null)
            {
                Logger.LogDebug($"try to force task {taskName}");
                EventForceTask.Set();
            }
        }

        public void RemoveAll()
		{
			foreach (var item in ScheduledTaskList.Keys)
			{
                Remove(item);
			} 
		}

        public void Remove(string taskName)
        {
            if (taskName == null)
			{
                throw new NullReferenceException("task name is null");
			}

            taskName = taskName.ToLower();

            var getResult = ScheduledTaskList.TryGetValue(taskName, out ScheduledTask task);
            if (!getResult)
			{
                throw new Exception($"Try to get task {taskName} failed");
			}

            if (task != null)
            {
                if (((TasksHost)TasksHost).IsRunning(task.Name))
                {
                    var h = TasksHost.GetHistory(task.Name).LastOrDefault();
                    if (h != null && h.Context != null)
                    {
                        h.Context.IsCancelRequested = true;
                    }
                }
                var removeResult = ScheduledTaskList.TryRemove(taskName, out ScheduledTask removedTask);
                if (!removeResult)
				{
                    throw new Exception($"Try to remove task {taskName} failed");
				}
            }
        }

        public void CancelTask(string taskName)
        {
            if (taskName == null)
            {
                throw new NullReferenceException("task name is null");
            }

            taskName = taskName.ToLower();

            Logger.LogInformation("Try to Cancel task {0}", taskName);
            var getResult = ScheduledTaskList.TryGetValue(taskName, out ScheduledTask task);
            if (!getResult)
			{
                throw new Exception($"Try to get task {taskName} failed");
			}

            if (task != null)
            {
                if (((TasksHost)TasksHost).IsRunning(taskName))
                {
                    var h = TasksHost.GetHistory(task.Name).LastOrDefault();
                    if (h != null && h.Context != null)
                    {
                        h.Context.IsCancelRequested = true;
                    }
                }
            }

        }

        public void ForceTask(string taskName)
        {
            if (taskName == null)
            {
                throw new NullReferenceException("task name is null");
            }

            taskName = taskName.ToLower();

            Logger.LogInformation("Try to force task {0}", taskName);
            
            var getResult = ScheduledTaskList.TryGetValue(taskName, out ScheduledTask task);
            if (!getResult)
			{
                throw new Exception($"try to get task {taskName} failed");
			}
            if (task == null)
            {
                Logger.LogWarning("force unknown task {0}", taskName);
                return;
            }
            task.DelayedStartInMillisecond = 0;
            task.NextRunningDate = DateTime.MinValue;
            task.IsForced = true;
            if (EventForceTask != null)
            {
                EventForceTask.Set();
                Logger.LogInformation("Task {0} was forced", taskName);
            }
        }

        public void ResetScheduledTaskList()
        {
            ScheduledTaskList.Clear();
        }

        public int GetScheduledTaskCount()
        {
            return ScheduledTaskList.Count;
        }

        public IEnumerable<ScheduledTask> GetList()
        {
            var result = new List<ScheduledTask>();
            foreach (var item in ScheduledTaskList)
            {
                result.Add(item.Value);
            }
            return result;
        }

        #endregion

        internal virtual void ProcessNextTasks()
        {
            Logger.LogInformation("Task scheduler started");
            int loop = 0;
            while (!Terminated)
            {
                Logger.LogDebug($"Try to process NextTask loop {loop}");
                ProcessNextTasks(DateTime.Now);
                Logger.LogDebug($"Task process processed loop {loop}");
                var waitHandles = new WaitHandle[] { EventStop, EventForceTask };
                int result = ManualResetEvent.WaitAny(waitHandles, Settings.IntervalInSeconds * 1000, true);
                LastSignal = DateTime.Now;
                if (result == 0)
                {
                    Terminated = true;
                    break;
                }
                else if (result == 1)
                {
                    Logger.LogDebug("scheduled task : handle force");
                }
                else if (result == 258)
                {
                    Logger.LogDebug("scheduled task : timeout");
                }
                EventForceTask.Reset();
                loop++;
            }
            Logger.LogInformation("Task scheduler stopped");
        }

        /// <summary>
        /// Recupération de la première tache executable 
        /// dans l'ordre FIFO
        /// </summary>
        /// <returns></returns>
        internal virtual void ProcessNextTasks(DateTime now)
        {
            var query = from t in ScheduledTaskList
                        where CanRun(now, t.Value)
                        orderby t.Value.CreationDate
                        select t.Value;

            var list = query.ToList();

            while (true)
            {
                var task = list.FirstOrDefault();
                if (task == null)
                {
                    break;
                }
                list.Remove(task);
                try
                {
                    Logger.LogDebug("Try to start scheduled task {0}", task.Name);
                    ProcessTask(task);
                    SetNextRuningDate(DateTime.Now, task);
                }
                catch (Exception ex)
                {
                    ex.Data.Add("TaskName", task.Name);
                    Logger.LogError(ex, ex.Message);
                }
            }
        }

        internal virtual void ProcessTask(ScheduledTask scheduledTask)
        {
            if (scheduledTask.IsQueued
                && !scheduledTask.AllowMultipleInstance)
            {
                Logger.LogWarning("scheduled task {0} is in queue and not allow multiple instance", scheduledTask.Name);
                return;
            }

            if (!scheduledTask.AllowMultipleInstance)
            {
                if (((TasksHost)TasksHost).IsRunning(scheduledTask.Name))
                {
                    Logger.LogWarning("scheduled task {0} is already running and not allow multiple instance", scheduledTask.Name);
                    return;
                }
            }

            var id = Guid.NewGuid();
            scheduledTask.IsQueued = true;

            var taskHost  = (TasksHost)TasksHost;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            taskHost.Enqueue(
                id
                , scheduledTask.Name
                , scheduledTask.TaskType
                , scheduledTask.Parameters
                , (dic) =>
                {
                    Logger.LogDebug("scheduled task {0} completed", scheduledTask.Name);
                    scheduledTask.IsQueued = false;
                    try
                    {
                        scheduledTask.Completed?.Invoke(dic);
                        TaskFinished?.Invoke(scheduledTask.Name);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        scheduledTask.IsForced = false;
                        if (scheduledTask.NextRunningDateFactory != null)
                        {
                            try
                            {
                                scheduledTask.NextRunningDate = scheduledTask.NextRunningDateFactory.Invoke();
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, ex.Message);
                            }
                        }
                    }
                }
                , (ex) =>
                {
                    scheduledTask.Exception = ex;
                    Logger.LogError(ex, ex.Message);
                    try
                    {
                        TaskFailed?.Invoke(scheduledTask.Name, ex);
                    }
                    catch { }
                },
                null,
                () =>
                {
                    Logger.LogDebug("scheduled task {0} started", scheduledTask.Name);
                    scheduledTask.StartedCount += 1;
                    try
                    {
                        TaskStarted?.Invoke(scheduledTask.Name);
                    }
                    catch { }
                },
                true,
                scheduledTask.IsForced);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        internal bool CanRun(DateTime now, ScheduledTask scheduledTask)
        {
            if (scheduledTask.IsQueued)
            {
                return false;
            }

            // Delayed start
            if (scheduledTask.DelayedStartInMillisecond > 0
                && now < scheduledTask.CreationDate.AddSeconds(scheduledTask.DelayedStartInMillisecond))
            {
                return false;
            }

            if (scheduledTask.Period == ScheduledTaskTimePeriod.WorkingDay
                    && (scheduledTask.NextRunningDate.DayOfWeek == DayOfWeek.Saturday
                    || scheduledTask.NextRunningDate.DayOfWeek == DayOfWeek.Sunday))
            {
                return false;
            }

            if (now >= scheduledTask.NextRunningDate
                || scheduledTask.StartedCount == 0)
            {
                return true;
            }

            return false;
        }

        internal void SetNextRuningDate(DateTime now, ScheduledTask scheduledTask)
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

                    while (true)
                    {
                        scheduledTask.NextRunningDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(scheduledTask.Interval);
                        if (scheduledTask.NextRunningDate.DayOfWeek != DayOfWeek.Saturday
                            && scheduledTask.NextRunningDate.DayOfWeek != DayOfWeek.Sunday)
                        {
                            break;
                        }
                        now = now.AddDays(1);
                    }
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
        }

    }
}
