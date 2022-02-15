using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime
{
	public class ProcessTask
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public DateTime CreationDate { get; set; } = DateTime.Now;
		public string FullTypeName { get; set; }
		public string TaskName { get; set; }
		public bool AllowMultipleInstances { get; set; }
		public bool IsForced { get; set; } = false;
	}
}
