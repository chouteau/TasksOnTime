namespace DistributedTasksOnTime;

public enum TaskState
{
    Enqueued = 0,
    Started = 1,
    Terminated = 2,
    Failed = 3,
    Canceling = 4,
    Canceled = 5,
    Progress = 6,
    RunningConfirmed = 7
}
