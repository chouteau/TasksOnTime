using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime
{
	public class TaskRegistrationInfo
	{
		public string TaskName { get; set; }
		public string AssemblyQualifiedName { get; set; }
		public bool AllowMultipleInstances { get; set; } = false;
		public bool Enabled { get; set; } = true;
	}
}
