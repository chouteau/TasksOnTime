using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.Orchestrator.Readers
{
	public class ForceTaskReader : Ariane.MessageReaderBase<DistributedTasksOnTime.ForceTask>
	{
		public ForceTaskReader(ITasksOrchestrator tasksOrchestrator)
		{
			TasksOrchestrator = tasksOrchestrator;
		}

		protected ITasksOrchestrator TasksOrchestrator { get; }

		public override async Task ProcessMessageAsync(ForceTask message)
		{
			await TasksOrchestrator.ForceTask(message.TaskName);
		}
	}
}
