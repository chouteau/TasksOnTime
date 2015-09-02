using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
    public class MockTaskProgress : TasksOnTime.Notification.ITaskProgress
    {
		public MockTaskProgress()
		{
			NotificationList = new List<Notification.NotificationItem>();
		}

		public List<Notification.NotificationItem> NotificationList { get; set; }

		public void Add(Notification.NotificationItem item)
        {
			NotificationList.Add(item);
        }
    }
}
