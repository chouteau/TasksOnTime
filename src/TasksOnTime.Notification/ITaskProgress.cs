﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Notification
{
    public interface ITaskProgress
    {
        void Add(NotificationItem notificaiton);
    }
}
