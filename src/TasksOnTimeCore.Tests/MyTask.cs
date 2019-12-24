using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime;

namespace TasksOnTime.Tests
{
    public class MyTask : ITask
    {
        public MyTask(ILogger<MyTask> logger)
        {
            this.Logger = logger;
        }

        protected ILogger<MyTask> Logger { get;  }

        public void Execute(ExecutionContext context)
        {
            Logger.LogInformation("Task executed");
        }
    }
}
