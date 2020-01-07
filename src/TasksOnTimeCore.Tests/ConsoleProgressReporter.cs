using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime.Tests
{
	public class ConsoleProgressReporter : IProgressReporter
	{
		public ConsoleProgressReporter(ILogger<ConsoleProgressReporter> logger)
		{
			this.Logger = logger;
		}

		protected ILogger<ConsoleProgressReporter> Logger { get; }

		public void Notify(ProgressInfo info)
		{
			Logger.LogInformation($"Type : {info.Type}");
			Logger.LogInformation($"Subject : {info.Subject}");
			Logger.LogInformation($"TaskId : {info.TaskId}");
			Logger.LogInformation($"Body : {info.Body}");
			Logger.LogInformation($"Entity : {info.Entity}");
			Logger.LogInformation($"EntityId : {info.EntityId}");
			Logger.LogInformation($"EntityName : {info.EntityName}");
			Logger.LogInformation($"GroupName : {info.GroupName}");
			Logger.LogInformation($"Index : {info.Index}");
			Logger.LogInformation($"TotalCount : {info.TotalCount}");
			Logger.LogInformation("--");
		}

	}
}
