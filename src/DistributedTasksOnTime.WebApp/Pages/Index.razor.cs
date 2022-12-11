using Microsoft.AspNetCore.Components;

namespace DistributedTasksOnTime.WebApp.Pages
{
	public partial class Index
	{
        [Inject]
        DistributedTasksOnTime.Orchestrator.ITasksOrchestrator TasksOrchestrator { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                TasksOrchestrator.OnRunningTaskChanged += (s, t) =>
                {
                    
                };
            }
        }
    }
}
