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

using NFluent;

namespace TasksOnTime.Tests
{
	[TestClass]
	public class TasksHostTests
	{
		[TestMethod]
		public void Enqueue()
		{
			var message = new Variable<string>();

			var task = new MyTask();

			var mre = new ManualResetEvent(false);
			var key = Guid.NewGuid();
			TasksHost.Enqueue<MyTask>(key,
                null,
                completed: (dic) =>
				{
					mre.Set();
				});

			mre.WaitOne();

			var history = TasksHost.GetHistory(key);

            Check.That(history).IsNotNull();
            Check.That(history.TerminatedDate).IsNotNull();
            Check.That(history.Context).IsNull();
		}

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Enqueue_With_Task_Not_Implements_ITask()
        {
            TasksHost.Enqueue<BadTask>();
        }

        [TestMethod]
		public void Enqueue_And_Cancel()
		{
			var mre = new ManualResetEvent(false);
			var key = Guid.NewGuid();

            var parameter = new Dictionary<string, object>();
            parameter.Add("count", 0);

            TasksHost.Enqueue<LongTask>(key,
                parameter,
				completed: (dic) =>
				{
					mre.Set();
				});

			System.Threading.Thread.Sleep(2 * 1000);
			TasksHost.Cancel(key);

			mre.WaitOne();

            var history = TasksHost.GetHistory(key);

			Check.That(history.CanceledDate).IsNotNull();
            var count = history.Parameters["count"];
            Check.That((int)count).IsLessThan(10);
		}

        [TestMethod]
        public void Enqueue_With_Delay()
        {
            var message = new Variable<string>();

            var task = new MyTask();

            var mre = new ManualResetEvent(false);
            var chrono = new System.Diagnostics.Stopwatch();
            chrono.Start();
            TasksHost.Enqueue<MyTask>(
                completed: (dic) =>
                {
                    mre.Set();
                },
                delayInMillisecond: 5 * 1000);

            mre.WaitOne();
            chrono.Stop();

            Check.That(chrono.ElapsedMilliseconds).IsGreaterThan(4 * 1000);
        }

        [TestMethod]
        public void Enqueue_Multi_Task()
        {
            var chrono = new System.Diagnostics.Stopwatch();
            chrono.Start();

            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();
            TasksHost.Enqueue<LongTask>();

            while(true)
            {
                if (!TasksHost.IsRunning())
                {
                    break;
                }
                System.Threading.Thread.Sleep(1000);
            }

            chrono.Stop();

            Check.That(chrono.ElapsedMilliseconds).IsLessThan(10 * 10 * 1000);
        }

        [TestMethod]
        public void Enqueue_Fail_Task()
        {
            var id = Guid.NewGuid();
            var mre = new ManualResetEvent(false);
            TasksHost.Enqueue<FailedTask>(id
                , completed : (dic) =>
                {
                    mre.Set();
                });

            mre.WaitOne();

            var histo = TasksHost.GetHistory(id);

            Check.That(histo.Exception != null).IsTrue();
        }

        [TestMethod]
        public void Enqueue_With_Parameter()
        {
            var id = Guid.NewGuid();
            var mre = new ManualResetEvent(false);
            TasksHost.Enqueue<ParameterizedTask>(id,
                new Dictionary<string, object>()
                {
                    { "input", "test" }
                }, completed: (dic) =>
                {
                    var output = dic["output"];
                    Check.That(output).Equals("test");
                    mre.Set();
                });

            mre.WaitOne();
        }

    }
}
