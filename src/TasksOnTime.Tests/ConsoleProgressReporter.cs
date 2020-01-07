using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime.Tests
{
	public class ConsoleProgressReporter : IProgressReporter
	{
		public void Notify(ProgressInfo info)
		{
			GlobalConfiguration.Logger.Info($"Type : {info.Type}");
			GlobalConfiguration.Logger.Info($"Subject : {info.Subject}");
			GlobalConfiguration.Logger.Info($"TaskId : {info.TaskId}");
			GlobalConfiguration.Logger.Info($"Body : {info.Body}");
			GlobalConfiguration.Logger.Info($"Entity : {info.Entity}");
			GlobalConfiguration.Logger.Info($"EntityId : {info.EntityId}");
			GlobalConfiguration.Logger.Info($"EntityName : {info.EntityName}");
			GlobalConfiguration.Logger.Info($"GroupName : {info.GroupName}");
			GlobalConfiguration.Logger.Info($"Index : {info.Index}");
			GlobalConfiguration.Logger.Info($"TotalCount : {info.TotalCount}");
			GlobalConfiguration.Logger.Info("--");
		}

	}
}
