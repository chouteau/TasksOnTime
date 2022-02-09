using DistributedTasksOnTime;
using DistributedTasksOnTime.Client;
using DistributedTasksOnTime.Orchestrator.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTaskOnTime.Tests
{
	[TestClass]
	public class UseCase1
	{
		// [TestMethod]
		public async Task Register_MyTask()
		{
			var host = TestsHelper.CreateTestHostWith1Client(config =>
			{
				config.RegisterScheduledTask<MyTask>(new DistributedTasksOnTime.TaskRegistrationInfo
				{
					TaskName = "MyTask",
					AllowMultipleInstances = false,
					Enabled = true,
				});
			});

			await host.Services.UseDistributedTasksOnTimeClient();

			await host.StartAsync();

			var orchestrator = host.Services.GetRequiredService<DistributedTasksOnTime.Orchestrator.ITasksOrchestrator>();
			orchestrator.OnHostRegistered += hostKey =>
			{
				var scheduledTask = orchestrator.GetScheduledTaskList().First();

				scheduledTask.Period = ScheduledTaskTimePeriod.Second;
				scheduledTask.Interval = 5;
			};
		}
	}
}