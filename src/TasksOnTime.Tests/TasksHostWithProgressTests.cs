using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;


namespace TasksOnTime.Tests
{
	[TestClass]
	public class TasksHostWithProgressTests
	{
		[ClassInitialize()]
		public static void ClassInit(TestContext context)
		{
			GlobalConfiguration.Logger = new DebugLogger();
			GlobalConfiguration.ProgressReporter = new ConsoleProgressReporter();
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

		[ClassCleanup()]
		public static void ClassCleanup()
		{
			TasksHost.Stop();
		}

		[TestMethod]
		public void Enqueue_With_Progress()
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
