﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent
{
	public partial class EditScheduledTask
	{
		[Parameter] public string TaskName { get; set; }
		[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
		[Inject] NavigationManager NavigationManager { get; set; }
		[Inject] DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }
		CustomValidator CustomValidator { get; set; } = new();
		
		DistributedTasksOnTime.Orchestrator.Models.ScheduledTask scheduledTask = new();

		protected override void OnInitialized()
		{
			var scheduledTaskList = TasksOrchestrator.GetScheduledTaskList();
			scheduledTask = scheduledTaskList.FirstOrDefault(i => i.Name.Equals(TaskName, StringComparison.InvariantCultureIgnoreCase));
			base.OnInitialized();
		}

		void ValidateAndSave()
		{
			TasksOrchestrator.SaveScheduledTaskList(scheduledTask);
			NavigationManager.NavigateTo(Settings.ScheduledTaskListBlazorPage);
		}

		void ChangeProcessMode(ChangeEventArgs e)
		{
			var selectedModeString = e.Value.ToString();
			var selectedMode = (ProcessMode)Enum.Parse(typeof(ProcessMode), selectedModeString);
			scheduledTask.ProcessMode = selectedMode;
		}
	}
}
