using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent
{
	public class TaskInfo
	{
		public Orchestrator.Models.ScheduledTask ScheduledTask { get; set; }
		public Orchestrator.Models.RunningTask LastRunningTask { get; set; }
	}

	public partial class ScheduledTaskList
	{
		[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
		[Inject] NavigationManager NavigationManager { get; set; }
		[Inject] DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }

		List<TaskInfo> taskInfoList = new();

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
					LoadTaskInfoList();
					await InvokeAsync(() =>
					{
						StateHasChanged();
					});
				};
				TasksOrchestrator.OnRunningTaskChanged += async (s, r) =>
				{
					await InvokeAsync(() =>
					{
						LoadTaskInfoList();
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
