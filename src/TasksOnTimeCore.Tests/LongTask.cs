using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using TasksOnTime;

namespace TasksOnTime.Tests
{
	public class LongTask : ITask
	{
        public LongTask(ILogger<LongTask> logger)
        {
            this.Logger = logger;
        }

        protected ILogger<LongTask> Logger { get; }

        public async Task ExecuteAsync(ExecutionContext context)
		{
			for (int i = 0; i < 10; i++)
			{
                if (context.IsCancelRequested)
                {
                    break;
                }
                Logger.LogInformation($"LongTask {i}");
				await Task.Delay(1 * 1000);

                if (context.Parameters.ContainsKey("count"))
                {
                    context.Parameters["count"] = i;
                }
            }
		}
	}
}
