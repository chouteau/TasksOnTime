using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    public class ProgressTaskTests
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void Enqueue_Task_With_Progress()
        {
            var tp = new MockTaskProgress();
            TasksOnTime.Notification.NotificationService.SetTaskProgress(tp);

            var mre = new ManualResetEvent(false);
            var key = Guid.NewGuid();   
            TasksHost.Enqueue<ProgressTask>(key,
                completed: (dic) =>
                {
                    mre.Set();
                });

            mre.WaitOne();

			Check.That(tp.NotificationList).IsNotNull();
			Check.That(tp.NotificationList.Count).IsGreaterThan(0);
        }
    }
}
