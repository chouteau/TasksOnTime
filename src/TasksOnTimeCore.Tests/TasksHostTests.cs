using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NFluent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;

using TasksOnTime;
using System.Security.Cryptography;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class TasksHostTests
	{
		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			var serviceCollection = new ServiceCollection()
										.AddLogging();

			var configuration = new ConfigurationBuilder()
										.Build();

			serviceCollection.AddTasksOnTimeServices(config =>
			{
				var settings = new TasksOnTime.TasksOnTimeSettings();
				var section = configuration.GetSection("TasksOnTime");
				section.Bind(settings);
			});

			var serviceProvider = serviceCollection.BuildServiceProvider();

			TasksHost = (TasksHost) serviceProvider.GetRequiredService<ITasksHost>();

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
		public void Enqueue()
		{
			var mre = new ManualResetEvent(false);
			var key = Guid.NewGuid();
			TasksHost.Enqueue<MyTask>(key,
				null,
				completed: (dic) =>
				{
					mre.Set();
				});

			mre.WaitOne();

			var history = TasksHost.GetHistory(key);

			Check.That(history).IsNotNull();
			Check.That(history.TerminatedDate).IsNotNull();
			Check.That(history.Context).IsNull();
		}

		[TestMethod]
		public void Non_Generic_Enqueue()
		{
			var mre = new ManualResetEvent(false);
			var key = Guid.NewGuid();
			TasksHost.Enqueue(key,
				typeof(MyTask),
				completed: (dic) =>
				{
					mre.Set();
				});

			mre.WaitOne();

			var history = TasksHost.GetHistory(key);

			Check.That(history).IsNotNull();
			Check.That(history.TerminatedDate).IsNotNull();
			Check.That(history.Context).IsNull();
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void Enqueue_With_Task_Not_Implements_ITask()
		{
			TasksHost.Enqueue(typeof(BadTask));
		}

		[TestMethod]
		public void Enqueue_And_Cancel()
		{
			var mre = new ManualResetEvent(false);
			var key = Guid.NewGuid();

			var parameter = new Dictionary<string, object>();
			parameter.Add("count", 0);

			TasksHost.Enqueue<LongTask>(key,
				parameter,
				completed: (dic) =>
				{
					mre.Set();
				});

			System.Threading.Thread.Sleep(2 * 1000);
			TasksHost.Cancel(key);

			mre.WaitOne();

			var history = TasksHost.GetHistory(key);

			Check.That(history.CanceledDate).IsNotNull();
			var count = history.Parameters["count"];
			Check.That((int)count).IsStrictlyLessThan(10);
		}

		[TestMethod]
		public void Enqueue_With_Delay()
		{
			var mre = new ManualResetEvent(false);
			var chrono = new System.Diagnostics.Stopwatch();
			chrono.Start();
			TasksHost.Enqueue<MyTask>(
				completed: (dic) =>
				{
					mre.Set();
				},
				delayInMillisecond: 5 * 1000);

			mre.WaitOne();
			chrono.Stop();

			Check.That(chrono.ElapsedMilliseconds).IsStrictlyGreaterThan(4 * 1000);
		}

		[TestMethod]
		public void Enqueue_Multi_Task()
		{
			var chrono = new System.Diagnostics.Stopwatch();
			chrono.Start();

			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();
			TasksHost.Enqueue<LongTask>();

			while (true)
			{
				if (!TasksHost.IsRunning())
				{
					break;
				}
				System.Threading.Thread.Sleep(1000);
			}

			chrono.Stop();

			Check.That(chrono.ElapsedMilliseconds).IsStrictlyLessThan(10 * 10 * 1000);
		}

		[TestMethod]
		public void Enqueue_Fail_Task()
		{
			var id = Guid.NewGuid();
			var mre = new ManualResetEvent(false);
			TasksHost.Enqueue<FailedTask>(id
				, completed: (dic) =>
			   {
				   mre.Set();
			   });

			mre.WaitOne();

			var histo = TasksHost.GetHistory(id);

			Check.That(histo.Exception != null).IsTrue();
		}

		[TestMethod]
		public void Enqueue_With_Parameter()
		{
			var id = Guid.NewGuid();
			var mre = new ManualResetEvent(false);
			TasksHost.Enqueue<ParameterizedTask>(id,
				new Dictionary<string, object>()
				{
					{ "input", "test" }
				}, completed: (dic) =>
				{
					var output = dic["output"];
					Check.That(output).Equals("test");
					mre.Set();
				});

			mre.WaitOne();
		}

		[TestMethod]
		public void Cancel_Not_Existing_Task()
		{
			TasksHost.Cancel(Guid.NewGuid());
		}

		[TestMethod]
		public void Cancel_Terminated_Task()
		{
			var id = Guid.NewGuid();
			var mre = new ManualResetEvent(false);
			TasksHost.Enqueue<MyTask>(id,
				completed: (dic) =>
				{
					mre.Set();
				});
			mre.WaitOne();
			TasksHost.Cancel(id);
		}

		[TestMethod]
		public void Task_Exists()
		{
			var id = Guid.NewGuid();
			var mre = new ManualResetEvent(false);
			TasksHost.Enqueue<MyTask>(id,
				completed: (dic) =>
				{
					mre.Set();
				});
			mre.WaitOne();
			var result = TasksHost.Exists(id);
			Check.That(result).IsTrue();
		}

		[TestMethod]
		public void Task_Is_Running()
		{
			var id = Guid.NewGuid();
			var mre = new ManualResetEvent(false);
			TasksHost.Enqueue<LongTask>(id, completed: (dic) => mre.Set());
			System.Threading.Thread.Sleep(1 * 1000);
			var result = TasksHost.IsRunning(id);
			mre.WaitOne();
			Check.That(result).IsTrue();
		}

		[TestMethod]
		public void Tasks_Cleanup()
		{
			var id = Guid.NewGuid();
			var mre = new ManualResetEvent(false);
			TasksHost.Enqueue<MyTask>(id,
				completed: (dic) =>
				{
					mre.Set();
				});
			mre.WaitOne();
			TasksHost.Cleanup();
			var result = TasksHost.GetHistory(id);
			Check.That(result).IsNull();
		}

		[TestMethod]
		public void Get_Not_Exists_History()
		{
			var id = Guid.NewGuid();
			var h = TasksHost.GetHistory(id);
			Check.That(h).IsNull();
		}

		[TestMethod]
		public void Stop_TasksHost()
		{
			TasksHost.Enqueue<MyTask>();
			TasksHost.Enqueue<MyTask>();
			TasksHost.Enqueue<MyTask>();
			TasksHost.Enqueue<MyTask>();
			TasksHost.Enqueue<StressTask>();

			TasksHost.Stop();
		}

		[TestMethod]
		public void Stress_Tasks()
		{
			int maxThreadPool = 0;
			int completionPortThreads = 0;
			System.Threading.ThreadPool.GetMaxThreads(out maxThreadPool, out completionPortThreads);
			System.Threading.ThreadPool.SetMaxThreads(10, 10);
			var mre = new ManualResetEvent(false);
			int taskCountCompleted = 0;
			List<int> threadIdList = new List<int>();
			for (int i = 0; i < 50; i++)
			{
				TasksHost.Enqueue<StressTask>(completed: (dic) =>
				{
					var threadId = (int)dic["ThreadId"];
					if (!threadIdList.Any(t => t == threadId))
					{
						threadIdList.Add(threadId);
					}
					taskCountCompleted++;
					if (taskCountCompleted == 50)
					{
						mre.Set();
					}
				});
			}
			mre.WaitOne();
			System.Threading.ThreadPool.SetMaxThreads(maxThreadPool, completionPortThreads);

			Check.That(threadIdList.Count).IsStrictlyGreaterThan(0);
		}

	}
}