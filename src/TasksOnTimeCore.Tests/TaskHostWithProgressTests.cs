using System;
using System.Collections.Generic;
using System.Text;

using NFluent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;

using TasksOnTime;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class TaskHostWithProgressTests
	{
		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			var serviceCollection = new ServiceCollection()
										.AddLogging(builder =>
										{
											builder.SetMinimumLevel(LogLevel.Debug);
											builder.AddConsole();
											builder.AddDebug();
										});

			var configuration = new ConfigurationBuilder()
										.Build();

			serviceCollection.AddTasksOnTimeServices(configuration);
			serviceCollection.AddSingleton<TasksOnTime.IProgressReporter, ConsoleProgressReporter>();

			var serviceProvider = serviceCollection.BuildServiceProvider();

			TasksHost = serviceProvider.GetRequiredService<TasksHost>();

			TasksHost.TaskStarted += (s, arg) =>
			{
				Console.WriteLine(arg);
			};
			TasksHost.TaskFailed += (s, arg) =>
			{
				Console.WriteLine(arg);
			};
			TasksHost.TaskTerminated += (s, arg) =>
			{
				Console.WriteLine(arg);
			};
		}

		private static TasksHost TasksHost { get; set; }

		[ClassCleanup()]
		public static void ClassCleanup()
		{
			TasksHost.Stop();
		}

		[TestMethod]
		public void Progress()
		{
			var mre = new ManualResetEvent(false);
			var key = Guid.NewGuid();
			var completed = false;
			TasksHost.Enqueue<LongProgressTask>(key,
				null,
				completed: (dic) =>
				{
					completed = (bool)dic.GetParameter("completed");
					mre.Set();
				});

			mre.WaitOne();

			Check.That(completed).IsTrue();
		}
	}
}
