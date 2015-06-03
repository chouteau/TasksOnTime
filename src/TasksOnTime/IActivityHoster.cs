using System;
namespace TasksOnTime
{
	public interface IActivityHoster
	{
		void Abort(string key);
		event EventHandler<EventArgs<string>> ActivityCompleted;
		void Cancel(string key);
		void Cleanup();
		bool Exists(string key);
		ActivityHistory GetHistory(string id);
		string GetKey(Guid wfId);
		bool IsCancelRequested(Guid wfId);
		bool IsRunning(string key);
		void Run(string key, System.Activities.Activity activity, System.Collections.Generic.IDictionary<string, object> parameters = null, Action<System.Collections.Generic.IDictionary<string, object>> completed = null, Action<Exception> failed = null, Action<Exception> aborted = null, Action canceled = null);
		void Stop();
	}
}
