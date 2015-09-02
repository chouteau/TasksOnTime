using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Notification
{
	public enum NotificationType
	{
		Start,
		Write,
		StartProgress,
		StartContinuousProgress,
		Progress,
		EndProgress,
		EndContinuousProgress,
		EntityChanged,
		Failed,
		Cancel,
		Completed
	}
}
