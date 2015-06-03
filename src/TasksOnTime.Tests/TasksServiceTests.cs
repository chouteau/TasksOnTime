using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class TasksServiceTests
	{
		[TestMethod]
		public void Add_Task()
		{
			var taskService = new TasksOnTime.TasksService();

			var taskEntry = taskService.CreateTask("Test");
			taskService.Add(taskEntry);
		}

		[TestMethod]
		public void Force_Task()
		{
			var taskService = new TasksOnTime.TasksService();
			var taskEntry = taskService.CreateTask("Test");
			taskEntry.GetActivityInstance = () => new ActivityTest();
			taskEntry.WorkflowProperties.Add("Text", "my text");
			taskEntry.Period = ScheduledTaskTimePeriod.Minute;
			taskEntry.Name = "Test";

			taskService.Add(taskEntry);
			taskService.Start();
			taskService.ForceTask("Test");

			System.Threading.Thread.Sleep(4 * 1000);

			Assert.AreEqual(true, ((string)taskEntry.WorkflowProperties["Text"]).EndsWith("tested !"));

			taskService.Stop();
		}

	}
}
