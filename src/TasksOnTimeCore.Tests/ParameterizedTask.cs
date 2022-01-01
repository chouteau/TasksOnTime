using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFluent;

namespace TasksOnTime.Tests
{
    public class ParameterizedTask : ITask
    {
        public Task ExecuteAsync(ExecutionContext context)
        {
            var inputParameter = context.Parameters["input"];
            context.Parameters.AddOrUpdateParameter("output", inputParameter);
            return Task.CompletedTask;
        }
    }
}
