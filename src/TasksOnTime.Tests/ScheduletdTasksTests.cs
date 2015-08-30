using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class ScheduletdTasksTests
	{
		[TestInitialize]
		public void Initialize()
		{
			GlobalConfiguration.Settings.DisabledByDefault = false;
		}

        [TestCleanup]
        public void Init()
        {
            TasksHost.ResetScheduledTaskList();
        }

        [TestMethod]
		public void Add_Sheduled_Task()
		{
            var task = TasksHost.CreateScheduledTask<MyTask>("TestAdd")
                            .EveryDay();

            TasksHost.ScheduleTask(task);
            var taskList = TasksHost.GetScheduledTaskList();
			Check.That(taskList.Any(i => i.Name == "TestAdd")).IsTrue();
		}

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Add_Sheduled_Task_With_Same_Name()
        {
            var t1 = TasksHost.CreateScheduledTask<MyTask>("TestAdd")
                        .EveryDay();

            TasksHost.ScheduleTask(t1);

            var t2 = TasksHost.CreateScheduledTask<MyTask>("TestAdd")
                        .EveryDay();

            TasksHost.ScheduleTask(t2);
        }

        [TestMethod]
		public void Schedule_Task()
		{
            var task = TasksHost.CreateScheduledTask<TextTask>("Test")
                                .EveryMinute();

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

			System.Threading.Thread.Sleep(4 * 1000);

            TasksHost.Stop();

            Check.That(task.StartedCount).IsGreaterThan(0);
		}

        [TestMethod]
        public void Restart_Long_Task_With_Only_Instance_Allowed()
        {
            var task = TasksHost.CreateScheduledTask<LongTask>("Test")
                            .AllowMultipleInstance(false)
                            .EverySecond(3);

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

            System.Threading.Thread.Sleep(9 * 1000);

            TasksHost.Stop();

            Check.That(task.StartedCount).Equals(1);
        }

        [TestMethod]
        public void Start_Long_Task_With_Delay()
        {
            var task = TasksHost.CreateScheduledTask<LongTask>("Test")
                            .AllowMultipleInstance(false)
                            .StartWithDelay(60)
                            .EveryMinute();

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

            System.Threading.Thread.Sleep(5 * 1000);

            TasksHost.Stop();

            Check.That(task.StartedCount).Equals(0);
        }

        [TestMethod]
        public void Force_Schedule_Task()
        {
            var task = TasksHost.CreateScheduledTask<TextTask>("Test")
                                .StartWithDelay(60)
                                .EverySecond(3);

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

            System.Threading.Thread.Sleep(4 * 1000);
            Check.That(task.StartedCount).Equals(0);

            TasksHost.ForceScheduledTask("Test");

            System.Threading.Thread.Sleep(4 * 1000);

            TasksHost.Stop();

            Check.That(task.StartedCount).IsGreaterThan(0);
        }

        [TestMethod]
        public void Remove_Task()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("Test")
                                .EverySecond(2);

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

            System.Threading.Thread.Sleep(1 * 1000);

            TasksHost.RemoveScheduledTask("Test");

            System.Threading.Thread.Sleep(4 * 1000);

            var count = TasksHost.GetScheduledTaskList().Count();
            Check.That(count).Equals(0);

            TasksHost.Stop();
        }

        [TestMethod]
        public void Add_Scheduled_Task_Disabled_By_Config()
        {
            TasksOnTime.GlobalConfiguration.Settings.DisabledByDefault = true;
            var task = TasksHost.CreateScheduledTask<MyTask>("configTask")
                                .EveryMinute();

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

            System.Threading.Thread.Sleep(1 * 1000);

            var t = TasksHost.GetScheduledTaskList().SingleOrDefault(i => i.Name == "configTask");
            Check.That(t).IsNull();

            TasksHost.Stop();
        }

        [TestMethod]
        public void Can_Run_By_Month()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("canRunMonth")
                            .EveryMonth();

            bool canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddHours(1);
            canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Day()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("canRunDay")
                            .EveryDay();

            var canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddHours(1);
            canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_WorkingDay()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("canRunWorkingDay")
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
            var canRun = TasksHost.Current.SchedulerService.CanRun(startDate, task);
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
            canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }


        [TestMethod]
        public void Can_Run_By_Hour()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("canRunHour")
                            .EveryHour();

            var canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddMinutes(1);
            canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Minute()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("canRunMinute")
                            .EveryMinute();

            var canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddSeconds(1);
            canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Can_Run_By_Second()
        {
            var task = TasksHost.CreateScheduledTask<MyTask>("canRunSecond")
                            .EverySecond(2);

            var canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsTrue();

            task.StartedCount = 1;
            task.NextRunningDate = DateTime.Now.AddSeconds(1);
            canRun = TasksHost.Current.SchedulerService.CanRun(DateTime.Now, task);
            Check.That(canRun).IsFalse();
        }

        [TestMethod]
        public void Schedule_Fail_Task()
        {
            var task = TasksHost.CreateScheduledTask<FailedTask>("failTask")
                            .EveryMinute();

            TasksHost.ScheduleTask(task);
            TasksHost.StartScheduling();

            System.Threading.Thread.Sleep(2 * 1000);

            var hList = TasksHost.GetHistory("failTask");

            Check.That(hList).IsNotNull();

            TasksHost.Stop();
        }
    }
}
