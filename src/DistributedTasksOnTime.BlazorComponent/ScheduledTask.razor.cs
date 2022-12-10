namespace DistributedTasksOnTime.BlazorComponent;

public partial class ScheduledTask
{
	[Parameter] public string TaskName { get; set; }
	[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
	[Inject] DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }

    Persistence.Models.ScheduledTask scheduledTask = new();
	List<Persistence.Models.RunningTask> runningTaskList = new();

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			TasksOrchestrator.OnRunningTaskChanged += async (s, r) =>
			{
				if (r.TaskName != TaskName)
				{
					return;
				}
				await InvokeAsync(() =>
				{
					runningTaskList = TasksOrchestrator.GetRunningTaskList(TaskName).ToList();
					StateHasChanged();
				});
			};
		}
	}

	protected override void OnInitialized()
	{
		var scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
		scheduledTask = scheduledTaskList.FirstOrDefault(i => i.Name.Equals(TaskName, StringComparison.InvariantCultureIgnoreCase));

		runningTaskList = TasksOrchestrator.GetRunningTaskList(TaskName).ToList();

		base.OnInitialized();
	}

}
