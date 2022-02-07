using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime
{
	public class DistributedTaskInfo
	{
		public Guid Id { get; set; }
		public string HostKey { get; set; }
		public TaskState State { get; set; }
		public string ErrorStack { get; set; }
		public DateTime EventDate { get; set; } = DateTime.Now;
		public ProgressInfo ProgressInfo { get; set; }
	}
}
