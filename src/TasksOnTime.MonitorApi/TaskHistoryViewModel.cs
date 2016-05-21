using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.MonitorApi
{
	public class TaskHistoryViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime? StartedDate { get; set; }
		public DateTime? TerminatedDate { get; set; }
		public DateTime? CanceledDate { get; set; }
		public Exception Exception { get; set; }
		public Dictionary<string, object> Parameters { get; set; }
	}
}
