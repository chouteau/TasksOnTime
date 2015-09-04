using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TasksOnTime
{
	internal class VoidLogger : ILogger
	{
		public void Info(string message)
		{
           
		}

		public void Info(string message, params object[] prms)
		{
           
		}

		public void Notification(string message)
		{
			
		}

		public void Notification(string message, params object[] prms)
		{
			
		}

		public void Warn(string message)
		{
		}

		public void Warn(string message, params object[] prms)
		{
		}

		public void Debug(string message)
		{
         
		}

		public void Debug(string message, params object[] prms)
		{
         
		}

		public void Error(string message)
		{
         
		}

		public void Error(string message, params object[] prms)
		{
         
		}

		public void Error(Exception x)
		{
         
		}

		public void Fatal(string message)
		{
         
		}

		public void Fatal(string message, params object[] prms)
		{
         
		}

		public void Fatal(Exception x)
		{
         
		}
	}
}
