using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class TasksServiceTests
	{
		[TestInitialize]
		public void Initialize()
		{
			TaskService = new TasksOnTime.TasksService();
			GlobalConfiguration.Settings.DisabledByDefault = false;
		}

		protected TasksOnTime.TasksService TaskService { get; private set; }

		[TestMethod]
		public void Add_Task()
		{
			var taskEntry = TaskService.CreateTask("TestAdd");
			TaskService.Add(taskEntry);

			Check.That(TaskService.Contains("TestAdd")).IsTrue();
		}

		[TestMethod]
		public void Force_Task()
		{
			var taskEntry = TaskService.CreateTask("Test");
			taskEntry.GetActivityInstance = () => new ActivityTest();
			taskEntry.WorkflowProperties.Add("Text", "my text");
			taskEntry.Period = ScheduledTaskTimePeriod.Minute;
			taskEntry.Name = "Test";

			TaskService.Add(taskEntry);
			TaskService.Start();
			TaskService.ForceTask("Test");

			System.Threading.Thread.Sleep(4 * 1000);

			Check.That(((string)taskEntry.WorkflowProperties["Text"]).EndsWith("tested !")).IsTrue();

			TaskService.Stop();
		}

	}
}
