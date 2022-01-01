using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFluent;

namespace TasksOnTime.Tests
{
    public class ParameterizedOutputTask : ITask
    {
        public Task ExecuteAsync(ExecutionContext context)
        {
            context.Parameters.Add("output", "test");
            return Task.CompletedTask;
        }
    }
}
