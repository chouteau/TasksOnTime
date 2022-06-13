using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.Client
{
	internal class ForceTaskService : IForceTaskService
	{
		public ForceTaskService(Ariane.IServiceBus bus,
			DistributedTasksOnTimeSettings settings,
			ILogger<ForceTaskService> logger)
		{
			this.Bus = bus;
			this.Settings = settings;
			this.Logger = logger;
		}

		protected Ariane.IServiceBus Bus { get; }
		protected DistributedTasksOnTimeSettings Settings { get; }
		protected ILogger Logger { get; }

		public async Task Force(ForceTask task)
		{
			if (task == null)
			{
				Logger.LogWarning("force task is null");
				return;
			}
			if (string.IsNullOrWhiteSpace(task.TaskName))
			{
				Logger.LogWarning("force task with taskname is null or empty");
				return;
			}
			await Bus.SendAsync(Settings.ForceTaskQueueName, task);
		}
	}
}
