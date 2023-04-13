using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTasksOnTime
{
	public enum ProgressType
	{
		Start = 0,
		Write = 1,
		StartProgress = 2,
		StartContinuousProgress = 3,
		Progress = 4,
		EndProgress = 5,
		EndContinuousProgress = 6,
		EntityChanged = 7,
		Failed = 8,
		Cancel = 9,
		Completed = 10
	}
}
