using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
	public class LongTask : ITask
	{
        public void Execute(ExecutionContext context)
		{
			for (int i = 0; i < 10; i++)
			{
                if (context.IsCancelRequested)
                {
                    break;
                }
                System.Diagnostics.Debug.Write(i);
				System.Threading.Thread.Sleep(1 * 1000);

                if (context.Parameters.ContainsKey("count"))
                {
                    context.Parameters["count"] = i;
                }
            }
		}
	}
}
