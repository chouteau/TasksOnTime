using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFluent;

namespace TasksOnTime.Tests
{
	public class ForceTask : ITask
	{
		public void Execute(ExecutionContext context)
		{
			Check.That(context.Force).IsTrue();
		}
	}
}
