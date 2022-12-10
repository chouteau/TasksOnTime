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
				LoadTaskInfoList();
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
					var current = taskInfoList.FirstOrDefault(i => i.ScheduledTask.Name == r.TaskName);
					if (current != null)
					{
						current.LastRunningTask = r;
					}
					StateHasChanged();
				});
			};
		}
	}

	protected override void OnInitialized()
	{
		LoadTaskInfoList();
	}

	void LoadTaskInfoList()
	{
		var scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
		foreach (var scheduledTask in scheduledTaskList)
		{
			var taskInfo = taskInfoList.FirstOrDefault(i => i.ScheduledTask.Name == scheduledTask.Name);
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

			var history = TasksOrchestrator.GetRunningTaskList(taskInfo.ScheduledTask.Name);
			if (history.Any())
			{
				taskInfo.LastRunningTask = history.Last();
			}
		}
	}

	void EditTask(Persistence.Models.ScheduledTask task)
	{
		NavigationManager.NavigateTo($"/editscheduledtask/{task.Name}", true);
	}

	void ForceTask(Persistence.Models.ScheduledTask task)
	{
		TasksOrchestrator.ForceTask(task.Name);
	}

	void CancelTask(Persistence.Models.ScheduledTask task)
	{
		TasksOrchestrator.CancelTask(task.Name);
	}

	void ConfirmDeleteTask(Persistence.Models.ScheduledTask task)
	{
		confirmDeleteTask.Tag = task;
		confirmDeleteTask.ShowDialog($"Are you sure you want to delete task '{task.Name}'?");
	}

	async Task DeleteTask(object tag)
	{
		var task = (Persistence.Models.ScheduledTask)tag;
		await TasksOrchestrator.DeleteTask(task.Name);
		toast.Show("Task deleted", ToastLevel.Info);
		LoadTaskInfoList();
		StateHasChanged();
	}
}
