using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
	public interface IProgressReporter
	{
		void Notify(ProgressInfo info);
	}
}
