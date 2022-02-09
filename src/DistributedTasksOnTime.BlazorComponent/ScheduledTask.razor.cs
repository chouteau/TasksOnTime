using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent
{
	public partial class ScheduledTask
	{
		[Parameter] public string TaskName { get; set; }
		[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }

		Orchestrator.Models.ScheduledTask scheduledTask = new();
		List<Orchestrator.Models.RunningTask> runningTaskList = new();

		protected override void OnInitialized()
		{
			var scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
			scheduledTask = scheduledTaskList.FirstOrDefault(i => i.Name.Equals(TaskName, StringComparison.InvariantCultureIgnoreCase));

			runningTaskList = TasksOrchestrator.GetRunningTaskList(TaskName).ToList();

			TasksOrchestrator.OnRunningTaskChanged += async (s, r) =>
			{
				await InvokeAsync(() =>
				{
					runningTaskList = TasksOrchestrator.GetRunningTaskList(TaskName).ToList();
					StateHasChanged();
				});
			};

			base.OnInitialized();
		}

	}
}
