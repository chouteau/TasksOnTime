using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent
{
	public partial class ScheduledTaskList
	{
		[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
		[Inject] NavigationManager NavigationManager { get; set; }

		IEnumerable<DistributedTasksOnTime.Orchestrator.Models.ScheduledTask> scheduledTaskList;

		protected override Task OnInitializedAsync()
		{
			scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
			return base.OnInitializedAsync();
		}

		void EditTask(Orchestrator.Models.ScheduledTask task)
		{
			NavigationManager.NavigateTo($"/editscheduledtask/{task.Name}", true);
		}

		void ForceTask(Orchestrator.Models.ScheduledTask task)
		{
			TasksOrchestrator.ForceTask(task.Name);
		}

		void CancelTask(Orchestrator.Models.ScheduledTask task)
		{
			TasksOrchestrator.CancelTask(task.Name);
		}
	}
}
