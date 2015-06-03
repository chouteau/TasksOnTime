using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TasksOnTime
{
	internal class DiagnosticsLogger : ILogger
	{
		public void Info(string message)
		{
            System.Diagnostics.Trace.TraceInformation(message);
		}

		public void Info(string message, params object[] prms)
		{
            System.Diagnostics.Trace.TraceInformation(message, prms);
		}

		public void Notification(string message)
		{
			System.Diagnostics.Trace.TraceWarning(message);
		}

		public void Notification(string message, params object[] prms)
		{
			System.Diagnostics.Trace.TraceWarning(message, prms);
		}

		public void Warn(string message)
		{
            System.Diagnostics.Trace.TraceWarning(message);
		}

		public void Warn(string message, params object[] prms)
		{
            System.Diagnostics.Trace.TraceWarning(message,prms);
		}

		public void Debug(string message)
		{
            System.Diagnostics.Debug.WriteLine(message, "Debug");
		}

		public void Debug(string message, params object[] prms)
		{
            System.Diagnostics.Debug.WriteLine(string.Format(message, prms));
		}

		public void Error(string message)
		{
            System.Diagnostics.Trace.TraceError(message, "Error");
		}

		public void Error(string message, params object[] prms)
		{
            System.Diagnostics.Trace.TraceError(message, prms);
		}

		public void Error(Exception x)
		{
            System.Diagnostics.Trace.TraceError(x.ToString());
		}

		public void Fatal(string message)
		{
            System.Diagnostics.Trace.TraceError(message);
		}

		public void Fatal(string message, params object[] prms)
		{
            System.Diagnostics.Trace.TraceError(message, prms);
		}

		public void Fatal(Exception x)
		{
            System.Diagnostics.Trace.TraceError(x.ToString());
		}

        string GetPrefix(string prf)
        {
            var thread = System.Threading.Thread.CurrentThread;
            return string.Format("{0:yyyyMMdd}|{0:HH}H{0:mm}:{0:ss}.{0:ffff}\t|{1}\t|\t{2} :", DateTime.Now, prf, thread.Name);
        }
	}
}
