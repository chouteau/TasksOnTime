using DistributedTasksOnTime;
using DistributedTasksOnTime.Client;
using DistributedTasksOnTime.Orchestrator;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTaskOnTime.Tests
{
	[TestClass]
	public class FailTaskTest
	{
		[TestMethod]
		public async Task Start()
		{
			var host = TestsHelper.CreateTestHostWith1Client(config =>
			{
				config.RegisterScheduledTask<MyFailedTask>(new DistributedTasksOnTime.TaskRegistrationInfo
				{
					TaskName = "MyFailedTask",
					AllowMultipleInstances = false,
					Enabled = true,
					DefaultPeriod = ScheduledTaskTimePeriod.Second,
					DefaultInterval = 2
				});
			});

			var configDbRepository = host.Services.GetRequiredService<IDbRepository>();

			await host.Services.UseDistributedTasksOnTimeClient();

			StaticCounter.Reset();

			await host.StartAsync();

			var mre = new System.Threading.ManualResetEvent(false);
			var orchestrator = host.Services.GetRequiredService<ITasksOrchestrator>();
			orchestrator.OnHostRegistered += hostName =>
			{
				mre.Set();
			};
			mre.WaitOne();

			await Task.Delay(10 * 1000);

			StaticCounter.CounterValue.Should().BeGreaterThan(1);
		}
	}
}