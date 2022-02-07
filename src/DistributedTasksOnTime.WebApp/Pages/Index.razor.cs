using Microsoft.AspNetCore.Components;

namespace DistributedTasksOnTime.WebApp.Pages
{
	public partial class Index
	{
		[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }

		IEnumerable<DistributedTasksOnTime.Orchestrator.Models.ScheduledTask> scheduledTaskList;

		protected override Task OnInitializedAsync()
		{
			scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
			return base.OnInitializedAsync();
		}
	}
}
