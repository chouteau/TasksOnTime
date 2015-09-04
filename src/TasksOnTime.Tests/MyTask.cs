using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
    public class MyTask : ITask
    {
        public void Execute(ExecutionContext context)
        {
            System.Diagnostics.Debug.WriteLine("Task executed");
        }
    }
}
