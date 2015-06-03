using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class ActivityHistory
	{
		public string Id { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime? TerminatedDate { get; set; }
		public DateTime? CancelledDate { get; set; }
		public Exception Exception { get; set; }
		public IDictionary<string, object> OutputParameters { get; set; }
	}
}
