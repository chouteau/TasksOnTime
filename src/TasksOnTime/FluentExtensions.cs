using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public static class FluentExtensions
	{
		public static TaskEntry EveryMonth(this TaskEntry task, int months = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Month;
			task.Interval = months;
			return task;
		}

		public static TaskEntry EveryDay(this TaskEntry task, int days = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Day;
			task.Interval = days;
			return task;
		}

		public static TaskEntry EveryWorkingDay(this TaskEntry task)
		{
			task.Period = ScheduledTaskTimePeriod.WorkingDay;
			task.Interval = 1;
			return task;
		}

		public static TaskEntry EveryHour(this TaskEntry task, int hours = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Hour;
			task.Interval = hours;
			return task;
		}

		public static TaskEntry EveryMinute(this TaskEntry task, int minutes = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Minute;
			task.Interval = minutes;
			return task;
		}

		public static TaskEntry AddParameter(this TaskEntry task, string name, object value)
		{
			if (!task.WorkflowProperties.Keys.Contains(name))
			{
				task.WorkflowProperties.Add(name, value);
			}
			return task;
		}

		public static TaskEntry WithActivity(this TaskEntry task, Activity activity)
		{
			task.GetActivityInstance = () => activity;
			return task;
		}

		public static TaskEntry StartWithDelay(this TaskEntry task, int delayInSeconds = 1)
		{
			task.DelayedStart = delayInSeconds;
			return task;
		}


	}
}
