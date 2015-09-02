using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

using TasksOnTime.Scheduling;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class ScheduletdTasksTests
	{
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            GlobalConfiguration.Settings.DisabledByDefault = false;
            Scheduler.Start();
        }

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

			System.Threading.Thread.Sleep(4 * 1000);

            Check.That(task.StartedCount).IsGreaterThan(0);
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
        public void Force_Schedule_Task()
        {
            var task = Scheduler.CreateScheduledTask<TextTask>("ForceTaskTest")
                                .StartWithDelay(60)
                                .EverySecond(3);

            Scheduler.Add(task);

            System.Threading.Thread.Sleep(4 * 1000);
            Check.That(task.StartedCount).Equals(0);

            Scheduler.ForceTask("ForceTaskTest");

            System.Threading.Thread.Sleep(4 * 1000);

            Check.That(task.StartedCount).IsGreaterThan(0);
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
        public void Add_Scheduled_Task_Disabled_By_Config()
        {
            TasksOnTime.GlobalConfiguration.Settings.DisabledByDefault = true;
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

            bool canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddHours(1);
            canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Day()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunDay")
                            .EveryDay();

            var canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddHours(1);
            canRun = Scheduler.Current.CanRun(DateTime.Now, task);
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
            var canRun = Scheduler.Current.CanRun(startDate, task);
            Check.That(canRun).IsTrue();

            startDate = DateTime.Now;
            while (true)
            {
                if (startDate.DayOfWeek == DayOfWeek.Saturday
                    || startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    break;
                }
                else
                {
                    startDate = startDate.AddDays(1);
                }
            }
            task.NextRunningDate = startDate;
            canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }


        [TestMethod]
        public void Can_Run_By_Hour()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunHour")
                            .EveryHour();

            var canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddMinutes(1);
            canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Minute()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunMinute")
                            .EveryMinute();

            var canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddSeconds(1);
            canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Second()
        {
            var task = Scheduler.CreateScheduledTask<MyTask>("canRunSecond")
                            .EverySecond(2);

            var canRun = Scheduler.Current.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddSeconds(1);
            canRun = Scheduler.Current.CanRun(DateTime.Now, task);
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
    }
}
