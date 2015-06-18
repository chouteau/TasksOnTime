using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class SyncActivityHoster : TasksOnTime.ActivityHoster
	{
		private TasksOnTime.ActivityInstance m_ActivityInstance;

		public override void Run(string key, System.Activities.Activity activity, IDictionary<string, object> parameters = null, Action<IDictionary<string, object>> completed = null, Action<Exception> failed = null, Action<Exception> aborted = null, Action canceled = null)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key parameter required");
			}

			if (activity == null)
			{
				throw new ArgumentNullException("activity parameter required");
			}

			m_ActivityInstance = TasksOnTime.ActivityInstance.Create();
			m_ActivityInstance.Id = key;
			m_ActivityInstance.Completed = completed ?? m_ActivityInstance.Completed;
			m_ActivityInstance.Failed = failed ?? m_ActivityInstance.Failed;
			m_ActivityInstance.Aborted = aborted ?? m_ActivityInstance.Aborted;
			m_ActivityInstance.Cancelled = canceled ?? m_ActivityInstance.Cancelled;
			m_ActivityInstance.ActivityTypeName = activity.GetType().FullName;

			parameters = parameters ?? new Dictionary<string, object>();

			var wfi = new WorkflowInvoker(activity);

			try
			{
				var outputs = wfi.Invoke(parameters);
				foreach (var outputParameter in outputs)
				{
					parameters[outputParameter.Key] = outputParameter.Value;
				}
				m_ActivityInstance.TerminatedDate = DateTime.Now;
				m_ActivityInstance.Completed(parameters);
			}
			catch (Exception ex)
			{
				m_ActivityInstance.Exception = ex;
				m_ActivityInstance.TerminatedDate = DateTime.Now;
				m_ActivityInstance.Failed(ex);
			}
		}

		public override string GetKey(Guid wfId)
		{
			return m_ActivityInstance.Id;
		}
	}
}
