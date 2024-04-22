using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent;
internal class SuperRunningTask : RunningTask
{
    public SuperRunningTask(RunningTask task)
    {
        Id = task.Id;
		TaskName = task.TaskName;
		HostKey = task.HostKey;
		CreationDate = task.CreationDate;
		EnqueuedDate = task.EnqueuedDate;
		RunningDate = task.RunningDate;
		CancelingDate = task.CancelingDate;
		CanceledDate = task.CanceledDate;
		TerminatedDate = task.TerminatedDate;
		FailedDate = task.FailedDate;
		LastUpdate = task.LastUpdate;
		ErrorStack = task.ErrorStack;
		IsForced = task.IsForced;
    }

    public bool IsExpanded { get; set; } = false;
    public List<DistributedTasksOnTime.ProgressInfo> ProgressInfoList { get; set; } = new();
}
