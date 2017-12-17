using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime;

namespace TasksOnTime.Tests
{
	public class SubTask : ITask
	{
		public void Execute(ExecutionContext context)
		{
			var subparameter = (int)context.Parameters.GetParameter("subparameter");
			var fail = context.Parameters.GetParameter("fail");
			var value = (int)context.Parameters.GetParameter("value");
			value += subparameter * 2;
			context.Parameters.AddOrUpdateParameter("value", value);
			if (fail != null
				&& subparameter == 5)
			{
				throw new Exception("sub task failed");
			}
		}
	}
}
