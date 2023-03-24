using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.Orchestrator
{
	internal class EnqueueTaskItem
	{
        public ScheduledTask ScheduledTask { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public bool Force { get; set; }
    }
}
