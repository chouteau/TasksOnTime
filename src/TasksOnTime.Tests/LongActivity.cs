using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Tests
{
	public class LongActivity : CodeActivity
	{
		protected override void Execute(CodeActivityContext context)
		{
			for (int i = 0; i < 10; i++)
			{
				if (context.IsCancelRequested())
				{
					break;
				}
				Console.WriteLine(i);
				System.Threading.Thread.Sleep(1 * 1000);
			}
		}


	}
}
