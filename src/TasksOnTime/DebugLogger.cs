using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class DebugLogger : ILogger
	{
		private static DateTime m_StartLogging;
		private static DateTime m_LastLogging;

		public DebugLogger()
		{
			System.Diagnostics.Debug.AutoFlush = true;
			m_StartLogging = DateTime.Now;
        }

		private System.Diagnostics.TextWriterTraceListener m_Out;

		public TextWriter Out
		{
			get
			{
				if (m_Out == null)
				{
					m_Out = new System.Diagnostics.TextWriterTraceListener();
				}
				return m_Out.Writer;
			}
		}

		public void Debug(string message)
		{
			System.Diagnostics.Debug.WriteLine(Format(message), "Debug");
		}

		public void Debug(string message, params object[] prms)
		{
			System.Diagnostics.Debug.WriteLine(Format(message, prms), "Debug");
		}

		public void Dispose()
		{
		}

		public void Error(Exception x)
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
			System.Diagnostics.Debug.WriteLine(Format(x.ToString()), "Error");
			System.Diagnostics.Debug.WriteLine(string.Empty);
        }

		public void Error(string message)
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
			System.Diagnostics.Debug.WriteLine(Format(message), "Error");
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public void Error(string message, params object[] prms)
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
			System.Diagnostics.Debug.WriteLine(Format(message, prms), "Error");
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public void Fatal(Exception x)
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
			System.Diagnostics.Debug.WriteLine(Format(x.ToString()), "Fatal");
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public void Fatal(string message)
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
			System.Diagnostics.Debug.WriteLine(Format(message), "Fatal");
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public void Fatal(string message, params object[] prms)
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
			System.Diagnostics.Debug.WriteLine(Format(message, prms), "Fatal");
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public void Info(string message)
		{
			System.Diagnostics.Debug.WriteLine(Format(message), "Info");
		}

		public void Info(string message, params object[] prms)
		{
			System.Diagnostics.Debug.WriteLine(Format(message, prms), "Info");
		}

		public void Notification(string message)
		{
			System.Diagnostics.Debug.WriteLine(Format(message), "Notification");
		}

		public void Notification(string message, params object[] prms)
		{
			System.Diagnostics.Debug.WriteLine(Format(message, prms), "Notification");
		}

		public void Sql(string message)
		{
			System.Diagnostics.Debug.WriteLine(Format(message), "Sql");
		}

		public void Warn(string message)
		{
			System.Diagnostics.Debug.WriteLine(Format(message), "Warn");
		}

		public void Warn(string message, params object[] prms)
		{
			System.Diagnostics.Debug.WriteLine(Format(message, prms),"Warn");
		}

		public void Watch(string title, Action method)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			watch.Start();
			method.Invoke();
			watch.Stop();
			Debug("{0} = {1}ms", title, watch.ElapsedMilliseconds);
		}

		string Format(string message, params object[] prms)
		{
			var logDate = DateTime.Now;
			var fromStartDiff = (logDate - m_StartLogging);
			var diffFromStart = string.Format("{0:00}:{1:00}:{2:00}:{3:0000}", fromStartDiff.Hours, fromStartDiff.Minutes, fromStartDiff.Seconds, fromStartDiff.Milliseconds);
			var fromLastDiff = (logDate - m_LastLogging);
			var diffFromLast = string.Format("{0:00}:{1:00}:{2:00}:{3:0000}", fromLastDiff.Hours, fromLastDiff.Minutes, fromLastDiff.Seconds, fromLastDiff.Milliseconds);
			var row = string.Format("\tS{0}|L{1}|{2}({3}){4}",
				diffFromStart,
				diffFromLast,
                System.Threading.Thread.CurrentThread.Name,
				System.Threading.Thread.CurrentThread.ManagedThreadId,
				prms != null && prms.Count() > 0 ? string.Format(message, prms) : message
				);

			m_LastLogging = DateTime.Now;
			return row;
		}

	}
}
