using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTasksOnTime
{
	public enum ProgressType
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
