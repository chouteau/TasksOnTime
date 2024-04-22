namespace DistributedTasksOnTime.BlazorComponent;

public partial class ScheduledTask
{
	[Parameter]
	public string TaskName { get; set; }
	[Inject]
	DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
	[Inject]
	DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }
	[Inject]
	IDbRepository DbRepository { get; set; }

	DistributedTasksOnTime.ScheduledTask scheduledTask = new();
	List<SuperRunningTask> runningTaskList = new();

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
				await InvokeAsync(async () =>
				{
					if (!runningTaskList.Exists(i => i.Id == r.Id))
					{
                        var list = (await TasksOrchestrator.GetRunningTaskList(TaskName, withHistory: true)).ToList();
						foreach (var item in list)
						{
							if (runningTaskList.Exists(i => i.Id == item.Id))
							{
								continue;
							}
							var super = new SuperRunningTask(item);
							runningTaskList.Add(super);
						}
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
						await Expand(currentTask, true);
					}

                    StateHasChanged();
				});
			};
		}
	}

	protected override async Task OnInitializedAsync()
	{
		var scheduledTaskList = await TasksOrchestrator.GetScheduledTaskList();
		scheduledTask = scheduledTaskList.FirstOrDefault(i => i.Name.Equals(TaskName, StringComparison.InvariantCultureIgnoreCase));

        var list = (await TasksOrchestrator.GetRunningTaskList(TaskName, withHistory: true)).ToList();
		foreach (var item in list)
		{
			var super = new SuperRunningTask(item);
			runningTaskList.Add(super);
		}

        base.OnInitialized();
	}

	async Task Expand(SuperRunningTask runningTask, bool expand)
	{
		if (expand)
		{
			runningTask.ProgressInfoList = await DbRepository.GetProgressInfoList(runningTask.Id);
		}
		else
		{
			runningTask.ProgressInfoList.Clear();
		}
		runningTask.IsExpanded = expand;
		StateHasChanged();
	}
}
