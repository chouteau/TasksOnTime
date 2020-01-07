using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
	public class ProgressInfo
	{
		public Guid TaskId { get; set; }
		public ProgressType Type { get; set; }
		public string GroupName { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string EntityName { get; set; }
		public string EntityId { get; set; }
		public object Entity { get; set; }
		public int? TotalCount { get; set; }
		public int? Index { get; set; }

	}
}
