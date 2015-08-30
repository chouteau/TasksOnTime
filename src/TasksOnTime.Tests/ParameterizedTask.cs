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
        public void Execute(ExecutionContext context)
        {
            var inputParameter = context.Parameters["input"];
            Check.That(inputParameter).Equals("test");

            context.Parameters.Add("output", "test");
        }
    }
}
