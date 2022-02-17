using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public interface IProgressReporter
	{
		Task Notify(ProgressInfo info);
	}
}
