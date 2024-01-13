using TasksOnTime;

namespace DistributedTasksOnTime.DemoClient
{
    internal class FailTask : TasksOnTime.ITask
    {
        public async Task ExecuteAsync(TasksOnTime.ExecutionContext context)
        {
            context.StartNotification("", "FailTask Started");
            await Task.Delay(10 * 1000);
            throw new ApplicationException("failed");
        }
    }
}
