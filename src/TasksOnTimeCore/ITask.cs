using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
    public interface ITask 
    {
        Task ExecuteAsync(ExecutionContext context);
    }
}
