using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
	public class DefaultProgressReporter : IProgressReporter
	{
		public void Notify(ProgressInfo info)
		{
			GlobalConfiguration.Logger.Debug(info.Subject);
		}
	}
}
