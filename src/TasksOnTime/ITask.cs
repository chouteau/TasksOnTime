using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
    public interface ITask 
    {
        void Execute(ExecutionContext context);
    }
}
