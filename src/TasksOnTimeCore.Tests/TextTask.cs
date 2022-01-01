using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{

	public sealed class TextTask : ITask
	{
		// Define an activity input argument of type string
		public string Text { get; set; }

		// If your activity returns a value, derive from CodeActivity<TResult>
		// and return the value from the Execute method.
		public Task ExecuteAsync(ExecutionContext context)
		{
            // Obtain the runtime value of the Text input argument
            Text += " tested !";
			return Task.CompletedTask;
		}
	}
}
