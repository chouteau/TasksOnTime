using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
    public interface ITask 
    {
        void Execute(ExecutionContext context);
    }
}
