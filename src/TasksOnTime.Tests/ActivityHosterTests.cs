using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class ActivityHosterTests
	{
		[TestMethod]
		public void Run_Activity()
		{
			var message = new Variable<string>();

			var activity = new Sequence()
			{
				Variables = { message },
				Activities = 
				{
					new WriteLine() 
					{
						Text = message
					}
				}
			};

			var mre = new ManualResetEvent(false);
			string key = Guid.NewGuid().ToString();
			ActivityHoster.Current.Run(key, activity, parameters: null, completed: (dic) =>
				{
					mre.Set();
				});

			mre.WaitOne();

			bool exists = ActivityHoster.Current.Exists(key);

			Assert.AreEqual(exists, true);
		}


		[TestMethod]
		public void Cancel_Activity()
		{
			var mre = new ManualResetEvent(false);
			string key = Guid.NewGuid().ToString();
			bool isCanceled = false;
			ActivityHoster.Current.Run(key,
				new LongActivity(),
				parameters: null,
				completed: (dic) =>
				{
					mre.Set();
				},
				canceled: () =>
				{
					isCanceled = true;
					mre.Set();
				},
				aborted: (ex) =>
				{
					mre.Set();
				});

			System.Threading.Thread.Sleep(2 * 1000);
			ActivityHoster.Current.Cancel(key);

			mre.WaitOne();

			Assert.IsTrue(isCanceled);
		}

	}
}
