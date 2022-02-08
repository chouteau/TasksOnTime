using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime
{
	public enum TaskState
	{
		Enqueued,
		Started,
		Terminated,
		Failed,
		Canceling,
		Canceled,
		Progress
	}
}
