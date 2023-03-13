using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime
{
	public class ForceTask
	{
		public string TaskName { get; set; }
		public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
