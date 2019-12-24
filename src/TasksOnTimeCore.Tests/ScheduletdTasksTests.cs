using System;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Debug;

using NFluent;

using TasksOnTime.Scheduling;
using System.Threading;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class ScheduletdTasksTests
	{
        [ClassInitialize()]
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
            serviceCollection.AddTasksOnTimeScheduledServices(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Scheduler = serviceProvider.GetRequiredService<TaskScheduler>();
            Scheduler.Start();

            Settings = serviceProvider.GetRequiredService<ScheduleSettings>();
            Settings.ScheduledTaskDisabledByDefault = false;
            TasksHost = serviceProvider.GetRequiredService<TasksHost>();
        }

        protected static TaskScheduler Scheduler { get; set;  }
        protected static ScheduleSettings Settings { get; set; }
        protected static TasksHost TasksHost { get; set; }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            Scheduler.Stop();
        }

        [TestInitialize]
		public void Initialize()
		{
        }

        [TestCleanup]
        public void Cleanup()
        {
            Scheduler.ResetScheduledTaskList();
        }

        [TestMethod]
		public void Add_Sheduled_Task()
		{
            var task = Scheduler.CreateScheduledTask<MyTask>("TestAdd")
                            .EveryDay();

            Scheduler.Add(task);
            var taskList = Scheduler.GetList();
			Check.That(taskList.Any(i => i.Name == "TestAdd")).IsTrue();
		}

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Add_Sheduled_Task_With_Same_Name()
        {
            var t1 = Scheduler.CreateScheduledTask<MyTask>("TestAdd")
                        .EveryDay();

            Scheduler.Add(t1);

            var t2 = Scheduler.CreateScheduledTask<MyTask>("TestAdd")
                        .EveryDay();

            Scheduler.Add(t2);
        }

        [TestMethod]
		public void Schedule_Task_And_Start()
		{
            var task = Scheduler.CreateScheduledTask<TextTask>("SimpleTest")
                                .EveryMinute();

            Scheduler.Add(task);
            var loop = 0;
            while(true)
            {
                if (task.StartedCount > 0)
                {
                    break;
                }
                loop++;
                if (loop > 20)
                {
                    break;
                }
                System.Threading.Thread.Sleep(1 * 1000);
            }

            Check.That(task.StartedCount).IsStrictlyGreaterThan(0);
		}

        [TestMethod]
        public void Restart_Long_Task_With_Only_Instance_Allowed()
        {
            var task = Scheduler.CreateScheduledTask<LongTask>("OneInstanceTest")
                            .AllowMultipleInstance(false)
                            .EverySecond(3);

            Scheduler.Add(task);

            System.Threading.Thread.Sleep(9 * 1000);

            Check.That(task.StartedCount).Equals(1);
        }

        [TestMethod]
        public void Try_Start_Long_Task_With_Delay()
        {
            var task = Scheduler.CreateScheduledTask<LongTask>("LongTestWithDelay")
                            .AllowMultipleInstance(false)
                            .StartWithDelay(60)
                            .EveryMinute();

            Scheduler.Add(task);

            System.Threading.Thread.Sleep(5 * 1000);

            Check.That(task.StartedCount).Equals(0);
        }

        [TestMethod]
        public void Remove_Task()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("RemoveTest")
                                .EverySecond(2);

            Scheduler.Add(task);

            System.Threading.Thread.Sleep(1 * 1000);

            Scheduler.Remove("RemoveTest");

            System.Threading.Thread.Sleep(4 * 1000);

            var count = Scheduler.GetList().Count(i => i.Name == "RemoveTest");
            Check.That(count).Equals(0);
        }

		[TestMethod]
		public void Remove_Running_Task()
		{
			var id = Guid.NewGuid();
			var task = Scheduler.CreateScheduledTask<LongTask>("runningTask")
							.EverySecond(10);

			Scheduler.Add(task);
			System.Threading.Thread.Sleep(1 * 1000);
			Scheduler.Remove("runningTask");
			System.Threading.Thread.Sleep(5 * 1000);
			var result = Scheduler.GetList().Any(t => t.Name == "runningTask");
			Check.That(result).IsFalse();
		}


		[TestMethod]
        public void Add_Scheduled_Task_Disabled_By_Config()
        {
            Settings.ScheduledTaskDisabledByDefault = true;
            var task = Scheduler.CreateScheduledTask<MyTask>("configTask")
                                .EveryMinute();

            Scheduler.Add(task);

            System.Threading.Thread.Sleep(1 * 1000);

            var t = Scheduler.GetList().SingleOrDefault(i => i.Name == "configTask");
            Check.That(t).IsNull();
        }

        [TestMethod]
        public void Can_Run_By_Month()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunMonth")
                            .EveryMonth();

            bool canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

			Scheduler.SetNextRuningDate(DateTime.Now, task);

			task.StartedCount = 1;
			canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Day()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunDay")
                            .EveryDay();

            var canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

			Scheduler.SetNextRuningDate(DateTime.Now, task);

            task.StartedCount = 1;
            canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_WorkingDay()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunWorkingDay")
                            .EveryWorkingDay();

            var startDate = DateTime.Now;
            while(true)
            {
                if (startDate.DayOfWeek == DayOfWeek.Saturday
                    || startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDate = startDate.AddDays(1);
                }
                else
                {
                    break;
                }
            }
            var canRun = Scheduler.CanRun(startDate, task);
            Check.That(canRun).IsTrue();

			Scheduler.SetNextRuningDate(startDate, task);

			task.StartedCount = 1;
            canRun = Scheduler.CanRun(startDate, task);
            Check.That(canRun).IsFalse();
        }


        [TestMethod]
        public void Can_Run_By_Hour()
        {
			Scheduler.ResetScheduledTaskList();
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunHour")
                            .EveryHour();

			Scheduler.Add(task);

            var canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

			Scheduler.SetNextRuningDate(DateTime.Now, task);
			task.StartedCount = 1;

			canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();

			task.StartedCount = 0;
			task.NextRunningDate = DateTime.Now.AddMinutes(-1);
			Scheduler.ProcessNextTasks(DateTime.Now);

			System.Threading.Thread.Sleep(1 * 1000);

			Check.That(task.StartedCount).Equals(1);
		}

		[TestMethod]
        public void Can_Run_By_Minute()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunMinute")
                            .EveryMinute();

            var canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

			Scheduler.SetNextRuningDate(DateTime.Now, task);

            task.StartedCount = 1;
            canRun = Scheduler.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Second()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunSecond")
                            .EverySecond(2);

			var now = DateTime.Now;
            var canRun = Scheduler.CanRun(now, task);
            Check.That(canRun).IsTrue();

			Scheduler.SetNextRuningDate(now, task);

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddSeconds(1);
            canRun = Scheduler.CanRun(now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Schedule_Fail_Task()
        {
            var task = Scheduler.CreateScheduledTask<FailedTask>("failTask")
                            .EveryMinute();

            Scheduler.Add(task);

            System.Threading.Thread.Sleep(2 * 1000);

            var hList = TasksHost.GetHistory("failTask");

            Check.That(hList).IsNotNull();
        }

		[TestMethod]
		public void Scheduled_Task_Is_Running()
		{
			var id = Guid.NewGuid();
			var task = Scheduler.CreateScheduledTask<LongTask>("runningTask")
							.EverySecond(10);

			Scheduler.Add(task);
			System.Threading.Thread.Sleep(1 * 1000);
			var result = TasksHost.IsRunning("runningTask");

			System.Threading.Thread.Sleep(10 * 1000);
			Check.That(result).IsTrue();
		}

		[TestMethod]
		public void Scheduled_Task_With_Completed()
		{
			var id = Guid.NewGuid();
			var task = Scheduler.CreateScheduledTask<ParameterizedOutputTask>("scheduledparameterizedtask")
							.EverySecond(10);

			var mre = new System.Threading.ManualResetEvent(false);

			Scheduler.Add(task);

			task.Completed += (dic) =>
			{
				var parameter = (string) dic["output"];
				Check.That(parameter).Equals("test");
				mre.Set();
			};

			mre.WaitOne();
		}

		[TestMethod]
		public void Scheduled_Task_With_Parameters()
		{
			var id = Guid.NewGuid();
			var task = Scheduler.CreateScheduledTask<ParameterizedTask>("scheduledparameterizedtask", new System.Collections.Generic.Dictionary<string, object>() { { "input", "test" } })
							.EverySecond(10);

			var mre = new System.Threading.ManualResetEvent(false);

			Scheduler.Add(task);

			task.Completed += (dic) =>
			{
				var parameter = (string)dic["output"];
				Check.That(parameter).Equals("test");
				mre.Set();
			};

			mre.WaitOne();
		}

		[TestMethod]
		public void Scheduled_Same_Task_With_Parameters_And_Different_Name()
		{
			var id = Guid.NewGuid();
			var task1 = Scheduler.CreateScheduledTask<ParameterizedTask>("scheduledparameterizedtask1", new System.Collections.Generic.Dictionary<string, object>() { { "input", "test" } })
							.EverySecond(10);

			var task2 = Scheduler.CreateScheduledTask<ParameterizedTask>("scheduledparameterizedtask2", new System.Collections.Generic.Dictionary<string, object>() { { "input", "test1" } })
				            .EverySecond(10);

            var mre1 = new System.Threading.ManualResetEvent(false);
            var mre2 = new System.Threading.ManualResetEvent(false);
            var waitHandles = new WaitHandle[] { mre1, mre2 };

			Scheduler.Add(task1);
			Scheduler.Add(task2);

			task1.Completed += (dic) =>
			{
				var parameter = (string)dic["output"];
				Check.That(parameter).Equals("test");
				mre1.Set();
			};

			task2.Completed += (dic) =>
			{
				var parameter = (string)dic["output"];
				Check.That(parameter).Equals("test1");
				mre2.Set();
			};

            ManualResetEvent.WaitAll(waitHandles);
		}

		[TestMethod]
		public void Scheduled_Task_With_NextRunningDate()
		{
			var id = Guid.NewGuid();
			var nextDate = DateTime.Now.AddHours(1);
			var task = Scheduler.CreateScheduledTask<ParameterizedTask>("schedulednextrunningdate", new System.Collections.Generic.Dictionary<string, object>() { { "input", "test" } })
							.NextRunningDate(() => nextDate);

			var mre = new System.Threading.ManualResetEvent(false);

			Scheduler.Add(task);

			task.Completed += (dic) =>
			{
				mre.Set();
			};

			mre.WaitOne();

			Check.That(task.NextRunningDate.Ticks).IsEqualTo(nextDate.Ticks);
		}

		[TestMethod]
		public void Force_Scheduled_Task()
		{
			var id = Guid.NewGuid();
			var nextDate = DateTime.Now.AddHours(1);
			var task = Scheduler.CreateScheduledTask<ForceTask>("forcescheduled")
						.StartWithDelay(60 * 1000)
						.EveryMinute(1);

			var mre = new System.Threading.ManualResetEvent(false);

			Scheduler.Add(task);

			task.Completed += (dic) =>
			{
				mre.Set();
			};

			Scheduler.ForceTask("forcescheduled");

			mre.WaitOne();

		}



	}
}
