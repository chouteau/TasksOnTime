using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime.Tests
{

	public sealed class TextTask : ITask
	{
		// Define an activity input argument of type string
		public string Text { get; set; }

		// If your activity returns a value, derive from CodeActivity<TResult>
		// and return the value from the Execute method.
		public void Execute(ExecutionContext context)
		{
            // Obtain the runtime value of the Text input argument
            Text += " tested !";
		}
	}
}
