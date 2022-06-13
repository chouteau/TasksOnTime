using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.Client
{
	public interface IForceTaskService
	{
		Task Force(ForceTask task);
	}
}
