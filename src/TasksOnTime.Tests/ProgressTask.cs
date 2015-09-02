using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime.Notification;

namespace TasksOnTime.Tests
{
    public class ProgressTask : ITask
    {
        public void Execute(ExecutionContext context)
        {
            context.StartNotification("test", "task started");

            context.StartProgressNotification("test", "progress started", 5);
            for (int i = 0; i < 5; i++)
            {
                context.ProgressNotification("test", string.Format("progress : {0}", i), i);
            }
            context.EndProgressNotification("test");

            context.CompletedNotification("test");
        }
    }
}
