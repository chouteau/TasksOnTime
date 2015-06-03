using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TasksOnTime;

namespace TasksOnTime.Tests
{
	public class HostedActivityTest : CodeActivity
	{
		public OutArgument<string> Key { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			var key = context.GetActivityInstanceKey();
			context.SetValue(Key, key);
		}
	}
}
