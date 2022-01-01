using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
    public class FailedTask : ITask
    {
        public Task ExecuteAsync(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
