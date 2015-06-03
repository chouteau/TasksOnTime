using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace TasksOnTime
{
	internal class DefaultDependencyResolver : IDependencyResolver
	{
		public object GetService(Type serviceType)
		{
			try
			{
				return Activator.CreateInstance(serviceType);
			}
			catch
			{
				return null;
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return Enumerable.Empty<object>();
		}

		public IEnumerable<object> GetAllServices()
		{
			return null;
		}
	}
}
