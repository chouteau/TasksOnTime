namespace DistributedTasksOnTime.BlazorComponent;

public partial class EditScheduledTask
{
	[Parameter] public string TaskName { get; set; }
	[Inject] DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }
	[Inject] NavigationManager NavigationManager { get; set; }
	[Inject] DistributedTasksOnTime.Orchestrator.DistributedTasksOnTimeServerSettings Settings { get; set; }
	CustomValidator CustomValidator { get; set; } = new();

	DistributedTasksOnTime.ScheduledTask scheduledTask = new();

	protected override async Task OnInitializedAsync()
	{
		var scheduledTaskList = await TasksOrchestrator.GetScheduledTaskList();
		scheduledTask = scheduledTaskList.FirstOrDefault(i => i.Name.Equals(TaskName, StringComparison.InvariantCultureIgnoreCase));
		base.OnInitialized();
	}

	async Task ValidateAndSave()
	{
		scheduledTask.FromEditor = true;
		await TasksOrchestrator.SaveScheduledTask(scheduledTask);
		NavigationManager.NavigateTo(Settings.ScheduledTaskListBlazorPage);
	}

	void ChangeProcessMode(ChangeEventArgs e)
	{
		var selectedModeString = e.Value.ToString();
		var selectedMode = (ProcessMode)Enum.Parse(typeof(ProcessMode), selectedModeString);
		scheduledTask.ProcessMode = selectedMode;
	}
}
