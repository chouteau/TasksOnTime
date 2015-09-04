using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime.Notification
{
    public interface ITaskProgress
    {
        void Add(NotificationItem notificaiton);
    }
}
