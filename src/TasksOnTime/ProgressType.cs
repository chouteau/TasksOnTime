using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
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
