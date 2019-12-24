using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime.Scheduling
{
	public enum ScheduledTaskTimePeriod
	{
		Month,
		Day,
		WorkingDay,
		Hour,
		Minute,
        Second,
		Custom
	}
}
