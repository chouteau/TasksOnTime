using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
	public interface ILogger
	{
		void Info(string message);
		void Info(string message, params object[] prms);
		void Warn(string message);
		void Warn(string message, params object[] prms);
		void Debug(string message);
		void Debug(string message, params object[] prms);
		void Error(string message);
		void Error(string message, params object[] prms);
		void Error(Exception x);
		void Fatal(string message);
		void Fatal(string message, params object[] prms);
		void Fatal(Exception x);
		void Notification(string message);
		void Notification(string message, params object[] prms);
	}
}
