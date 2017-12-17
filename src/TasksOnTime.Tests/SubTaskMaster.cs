using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime;

namespace TasksOnTime.Tests
{
	public class SubTaskMaster : ITask
	{
		public void Execute(ExecutionContext context)
		{
			for (int i = 0; i < 10; i++)
			{
				context.Parameters.AddOrUpdateParameter("subparameter", i);
				context.ExecuteSubTask<SubTask>();
			}
		}
	}
}
