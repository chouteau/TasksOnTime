using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class ActivityHoster : IActivityHoster
	{
		protected SynchronizedCollection<ActivityInstance> m_WFInstances;

		static ActivityHoster()
		{
			Current = new ActivityHoster();
		}

		protected ActivityHoster()
		{
			m_WFInstances = new SynchronizedCollection<ActivityInstance>();
		}

		public static IActivityHoster Current { get; internal set; }

		public event EventHandler<EventArgs<string>> ActivityCompleted;

		public virtual void Run(string key, 
			System.Activities.Activity activity, 
			IDictionary<string, object> parameters = null, 
			Action<IDictionary<string, object>> completed = null, 
			Action<Exception> failed = null, 
			Action<Exception> aborted = null, 
			Action canceled = null)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key parameter required");
			}

			if (activity == null)
			{
				throw new ArgumentNullException("activity parameter required");
			}

			var activityInstance = new ActivityInstance();
			activityInstance.Id = key;
			activityInstance.Completed = completed ?? activityInstance.Completed;
			activityInstance.Failed = failed ?? activityInstance.Failed;
			activityInstance.Aborted = aborted ?? activityInstance.Aborted;
			activityInstance.Cancelled = canceled ?? activityInstance.Cancelled;
			activityInstance.ActivityTypeName = activity.GetType().FullName;

			parameters = parameters ?? new Dictionary<string, object>();

			lock (m_WFInstances.SyncRoot)
			{
				var referenced = m_WFInstances.SingleOrDefault(i => i.Id == key);
				if (referenced != null)
				{
					referenced.Id = Guid.NewGuid().ToString();
				}
				m_WFInstances.Add(activityInstance);
			}

			var wfa = new WorkflowApplication(activity, parameters);
			wfa.Completed = ActivityCompletedInternal;
			wfa.OnUnhandledException = ActivityOnUnhandledException;
			wfa.Aborted = ActivityAborted;

			activityInstance.WFApplication = wfa;

			try
			{
				wfa.Run();
			}
			catch(Exception ex)
			{
				activityInstance.Exception = ex;
				activityInstance.TerminatedDate = DateTime.Now;
				activityInstance.Failed(ex);
			}
		}

		public bool IsRunning(string key)
		{
			lock (m_WFInstances.SyncRoot)
			{
				return m_WFInstances.Any(i => i.Id == key);
			}
		}

		public virtual void Cancel(string key)
		{
			var existing = m_WFInstances.SingleOrDefault(i => i.Id == key);
			if (existing == null)
			{
				return;
			}
			if (existing.TerminatedDate.HasValue)
			{
				return;
			}
			GlobalConfiguration.Logger.Debug("Cancel activity {0} requested", key);
			existing.IsCancelRequested = true;
			existing.CancelledDate = DateTime.Now;
		}

		public virtual void Abort(string key)
		{
			var existing = m_WFInstances.SingleOrDefault(i => i.Id == key);
			if (existing == null)
			{
				return;
			}
			if (existing.TerminatedDate.HasValue)
			{
				return;
			}
			try
			{
				existing.WFApplication.Abort("aborted by external");
			}
			catch (Exception ex)
			{
				GlobalConfiguration.Logger.Error(ex);
			}
		}

		public virtual bool Exists(string key)
		{
			return m_WFInstances.Any(i => i.Id == key);
		}

		public virtual void Cleanup()
		{
			lock (m_WFInstances)
			{
				var cleanupList = (from instance in m_WFInstances
								  where instance.TerminatedDate.HasValue
										&& instance.TerminatedDate.Value.AddSeconds(GlobalConfiguration.Settings.CleanupTimeoutInSeconds) > DateTime.Now
								  select instance.Id).ToList();

				foreach (var item in cleanupList)
				{
					var first = m_WFInstances.FirstOrDefault(i => i.Id == item);
					if (first == null)
					{
						continue;
					}
					first.Dispose();
					m_WFInstances.Remove(first);
				}
			}
		}

		public virtual string GetKey(Guid wfId)
		{
			lock (m_WFInstances.SyncRoot)
			{
				var result = m_WFInstances.Where(i => i.WFApplication != null)
								.SingleOrDefault(i => i.WFApplication.Id == wfId);
				if (result != null)
				{
					return result.Id;
				}
			}
			return null;
		}

		public virtual bool IsCancelRequested(Guid wfId)
		{
			lock (m_WFInstances.SyncRoot)
			{
				var result = m_WFInstances.Where(i => i.WFApplication != null).SingleOrDefault(i => i.WFApplication.Id == wfId);
				if (result != null)
				{
					return result.IsCancelRequested;
				}
			}
			return false;
		}

		public virtual void Stop()
		{
			lock (m_WFInstances.SyncRoot)
			{
				foreach (var item in m_WFInstances)
				{
					item.IsCancelRequested = true;
				}
				try
				{
					System.Threading.Thread.Sleep(2 * 1000);
				}
				catch
				{
				}
				foreach (var item in m_WFInstances)
				{
					if (item.TerminatedDate.HasValue)
					{
						continue;
					}
					try
					{
						item.WFApplication.Terminate("host stopped", TimeSpan.FromSeconds(2));
					}
					catch (Exception ex)
					{
						ex.Data.Add("ActivityType", item.ActivityTypeName);
						GlobalConfiguration.Logger.Error(ex);
					}
					item.Dispose();
				}
			}
		}

		public virtual ActivityHistory GetHistory(string id)
		{
			ActivityInstance ai = null;
			lock (m_WFInstances.SyncRoot)
			{
				ai = m_WFInstances.SingleOrDefault(i => i.Id == id);
			}

			if (ai == null)
			{
				return null;
			}

			var result = new ActivityHistory();
			result.Id = id;
			result.CreationDate = ai.CreationDate;
			result.Exception = ai.Exception;
			result.CancelledDate = ai.CancelledDate;
			result.TerminatedDate = ai.TerminatedDate;


			return result;
		}

		protected void ActivityCompletedInternal(WorkflowApplicationCompletedEventArgs arg)
		{
			var activityInstance = m_WFInstances.Single(i => i.WFApplication.Id == arg.InstanceId);
			try
			{
				if (activityInstance.CancelledDate.HasValue)
				{
					activityInstance.Cancelled();
				}
				else if (arg.CompletionState == ActivityInstanceState.Faulted)
				{
					activityInstance.Failed(arg.TerminationException);
				}
				else
				{
					var dic = new Dictionary<string, object>();
					foreach (var outputParameter in arg.Outputs)
					{
						dic.Add(outputParameter.Key, outputParameter.Value);
					}

					activityInstance.Completed(dic);
				}
			}
			catch (Exception ex)
			{
				activityInstance.Exception = ex;
			}
			finally
			{
				activityInstance.TerminatedDate = DateTime.Now;
				if (ActivityCompleted != null)
				{
					try
					{
						ActivityCompleted(this, new EventArgs<string>(activityInstance.Id));
					}
					catch(Exception ex)
					{
						GlobalConfiguration.Logger.Error(ex);
					}
				}
			}
		}

		protected UnhandledExceptionAction ActivityOnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs arg)
		{
			var activityInstance = m_WFInstances.Single(i => i.WFApplication.Id == arg.InstanceId);
			activityInstance.Exception = arg.UnhandledException;
			activityInstance.TerminatedDate = DateTime.Now;
			try
			{
				activityInstance.Failed(arg.UnhandledException);
			}
			catch (Exception ex)
			{
				GlobalConfiguration.Logger.Error(ex);
			}
			return UnhandledExceptionAction.Terminate;
		}

		protected void ActivityAborted(WorkflowApplicationAbortedEventArgs arg)
		{
			var activityInstance = m_WFInstances.Single(i => i.WFApplication.Id == arg.InstanceId);
			activityInstance.TerminatedDate = DateTime.Now;
			activityInstance.Aborted(arg.Reason);
		}

	}
}
