namespace DistributedTasksOnTime.BlazorComponent;

public partial class ScheduledTask
{
	[Parameter] 
	public string TaskName { get; set; }
	[Inject] 
	DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
	[Inject] 
	DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }

	DistributedTasksOnTime.ScheduledTask scheduledTask = new();
	List<DistributedTasksOnTime.RunningTask> runningTaskList = new();

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
					if (!runningTaskList.Any(i => i.Id == r.Id))
					{
                        runningTaskList = TasksOrchestrator.GetRunningTaskList(TaskName, true).ToList();
                    }

					var currentTask = runningTaskList.SingleOrDefault(i => i.Id == r.Id);
					if (currentTask != null)
					{
						currentTask.TerminatedDate = r.TerminatedDate;
						currentTask.CanceledDate = r.CanceledDate;
						currentTask.CancelingDate = r.CancelingDate;
						currentTask.EnqueuedDate = r.EnqueuedDate;
						currentTask.RunningDate = r.RunningDate;
						currentTask.EnqueuedDate = r.EnqueuedDate;
						currentTask.FailedDate = r.FailedDate;
						currentTask.ProgressLogs = r.ProgressLogs;
					}

                    StateHasChanged();
				});
			};
		}
	}

	protected override void OnInitialized()
	{
		var scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
		scheduledTask = scheduledTaskList.FirstOrDefault(i => i.Name.Equals(TaskName, StringComparison.InvariantCultureIgnoreCase));

        runningTaskList = TasksOrchestrator.GetRunningTaskList(TaskName, true).ToList();

        base.OnInitialized();
	}
}
