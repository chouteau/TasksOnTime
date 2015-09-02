using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime.Scheduling
{
	public static class FluentExtensions
	{
		public static ScheduledTask EveryMonth(this ScheduledTask task, int months = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Month;
            task.StartDay = 1;
			task.Interval = months;
			return task;
		}

		public static ScheduledTask EveryDay(this ScheduledTask task, int days = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Day;
			task.Interval = days;
			return task;
		}

		public static ScheduledTask EveryWorkingDay(this ScheduledTask task)
		{
			task.Period = ScheduledTaskTimePeriod.WorkingDay;
			task.Interval = 1;
			return task;
		}

		public static ScheduledTask EveryHour(this ScheduledTask task, int hours = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Hour;
			task.Interval = hours;
			return task;
		}

		public static ScheduledTask EveryMinute(this ScheduledTask task, int minutes = 1)
		{
			task.Period = ScheduledTaskTimePeriod.Minute;
			task.Interval = minutes;
			return task;
		}

        public static ScheduledTask EverySecond(this ScheduledTask task, int second = 1)
        {
            task.Period = ScheduledTaskTimePeriod.Second;
            task.Interval = second;
            return task;
        }

        public static ScheduledTask AllowMultipleInstance(this ScheduledTask task, bool allow)
        {
            task.AllowMultipleInstance = allow;
            return task;
        }

		public static ScheduledTask StartWithDelay(this ScheduledTask task, int delayInSeconds = 1)
		{
			task.DelayedStartInMillisecond = delayInSeconds;
			return task;
		}


	}
}
