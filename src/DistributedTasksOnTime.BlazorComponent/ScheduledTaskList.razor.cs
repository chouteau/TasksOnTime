namespace DistributedTasksOnTime.BlazorComponent;

public partial class ScheduledTaskList
{
	[Inject] 
	DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
	[Inject] 
	NavigationManager NavigationManager { get; set; }
	[Inject] 
	DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }


	List<TaskInfo> taskInfoList = new();
	ConfirmDialog confirmDeleteTask;
	Toast toast;

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			TasksOrchestrator.OnHostRegistered += async s =>
			{
				await LoadTaskInfoList();
				await InvokeAsync(() =>
				{
					StateHasChanged();
				});
			};
			TasksOrchestrator.OnScheduledTaskStarted += async s =>
			{
				await InvokeAsync(() =>
				{
					StateHasChanged();
				});
			};
			TasksOrchestrator.OnRunningTaskChanged += async (s, r) =>
			{
				await InvokeAsync(() =>
				{
					var current = taskInfoList.Find(i => i.ScheduledTask.Name == r.TaskName);
					if (current != null)
					{
						current.LastRunningTask = r;
					}
					StateHasChanged();
				});
			};
		}
	}

	protected override async Task OnInitializedAsync()
	{
		await LoadTaskInfoList();
	}

	async Task LoadTaskInfoList()
	{
		var scheduledTaskList = await TasksOrchestrator.GetScheduledTaskList();
		foreach (var scheduledTask in scheduledTaskList)
		{
			var taskInfo = taskInfoList.Find(i => i.ScheduledTask.Name == scheduledTask.Name);
			if (taskInfo != null)
			{
				taskInfo.ScheduledTask = scheduledTask;
			}
			else
			{
				taskInfo = new TaskInfo
				{
					ScheduledTask = scheduledTask
				};
				taskInfoList.Add(taskInfo);
			}

			var history = await TasksOrchestrator.GetRunningTaskList(taskInfo.ScheduledTask.Name);
			if (history.Any())
			{
				taskInfo.LastRunningTask = history.Last();
			}
		}
	}

	void EditTask(DistributedTasksOnTime.ScheduledTask task)
	{
		NavigationManager.NavigateTo($"/editscheduledtask/{task.Name}", true);
	}

	void ForceTask(DistributedTasksOnTime.ScheduledTask task)
	{
		TasksOrchestrator.ForceTask(task.Name, task.Parameters);
	}

	void CancelTask(DistributedTasksOnTime.ScheduledTask task)
	{
		TasksOrchestrator.CancelTask(task.Name);
	}

	async Task TerminateTask(DistributedTasksOnTime.ScheduledTask task)
	{
		await TasksOrchestrator.TerminateTask(task.Name);
	}

	void ConfirmDeleteTask(DistributedTasksOnTime.ScheduledTask task)
	{
		confirmDeleteTask.Tag = task;
		confirmDeleteTask.ShowDialog($"Are you sure you want to delete task '{task.Name}'?");
	}

	async Task DeleteTask(object tag)
	{
		var task = (DistributedTasksOnTime.ScheduledTask)tag;
		await TasksOrchestrator.DeleteTask(task.Name);
		toast.Show("Task deleted", ToastLevel.Info);
		await LoadTaskInfoList();
		StateHasChanged();
	}

	void ResetRunningTasks()
	{
		TasksOrchestrator.ResetRunningTasks();
	}
}
