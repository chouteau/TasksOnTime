using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class ActivityInstance : IDisposable
	{
		internal ActivityInstance()
		{
			CreationDate = DateTime.Now;
			Completed = (dic) => { };
			Failed = (ex) => { GlobalConfiguration.Logger.Error(ex); };
			Aborted = (ex) => { };
			Cancelled = () => { };
		}

		public static ActivityInstance Create()
		{
			return new ActivityInstance();
		}

		public string Id { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime? TerminatedDate { get; set; }
		public Exception Exception { get; set; }
		public WorkflowApplication WFApplication { get; set; }
		public Action<IDictionary<string, object>> Completed { get; set; }
		public Action<Exception> Failed { get; set; }
		public Action<Exception> Aborted { get; set; }
		public Action Cancelled { get; set; }
		public DateTime? CancelledDate { get; set; }
		public bool IsCancelRequested { get; set; }
		public string ActivityTypeName { get; set; }

		public void Dispose()
		{
			Completed = null;
			Failed = null;
			Aborted = null;
			Cancelled = null;
			if (WFApplication != null)
			{
				WFApplication = null;
			}
		}
	}
}
