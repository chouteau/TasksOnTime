using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

using TasksOnTime;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class SubTasksTests
	{
		[TestMethod]
		public void Compute_Sub_Tasks()
		{
			int value = 10;
			var mre = new System.Threading.ManualResetEvent(false);
			TasksHost.Enqueue<SubTaskMaster>(new Dictionary<string, object>()
			{
				{ "value", value }
			}, completed : (dic) =>
			{
				value = (int)dic.GetParameter("value");
				mre.Set();
			});

			mre.WaitOne();

			Check.That(value).IsEqualTo(100);
		}

		[TestMethod]
		public void Compute_Sub_Tasks_With_Fail()
		{
			int value = 10;
			var mre = new System.Threading.ManualResetEvent(false);
			TasksHost.Enqueue<SubTaskMaster>(new Dictionary<string, object>()
			{
				{ "value", value },
				{ "fail", true }
			}, completed: (dic) =>
			{
				value = (int)dic.GetParameter("value");
				mre.Set();
			});

			mre.WaitOne();

			Check.That(value).IsEqualTo(100);
		}

	}
}
