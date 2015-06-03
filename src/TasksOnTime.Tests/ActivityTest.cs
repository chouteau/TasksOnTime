using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace TasksOnTime.Tests
{

	public sealed class ActivityTest : CodeActivity
	{
		// Define an activity input argument of type string
		public InOutArgument<string> Text { get; set; }

		// If your activity returns a value, derive from CodeActivity<TResult>
		// and return the value from the Execute method.
		protected override void Execute(CodeActivityContext context)
		{
			// Obtain the runtime value of the Text input argument
			string text = context.GetValue(this.Text);

			text = text + " tested !";

			context.SetValue(Text, text);
		}
	}
}
