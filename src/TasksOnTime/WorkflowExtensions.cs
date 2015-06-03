using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace TasksOnTime
{
	public static class WorkflowExtensions
	{
		public static TService GetService<TService>(this IDependencyResolver resolver)
		{
			return (TService)resolver.GetService(typeof(TService));
		}

		public static IEnumerable<TService> GetServices<TService>(this IDependencyResolver resolver)
		{
			return resolver.GetServices(typeof(TService)).Cast<TService>();
		}

		public static T GetService<T>(this ActivityContext context)
			where T : class 
		{
			return GlobalConfiguration.DependencyResolver.GetService<T>();
		}

		public static IEnumerable<T> GetServices<T>(this ActivityContext context)
			where T : class
		{
			return GlobalConfiguration.DependencyResolver.GetServices<T>();
		}

		public static string GetActivityInstanceKey(this ActivityContext context)
		{
			return ActivityHoster.Current.GetKey(context.WorkflowInstanceId);
		}

		public static bool IsCancelRequested(this ActivityContext context)
		{
			return ActivityHoster.Current.IsCancelRequested(context.WorkflowInstanceId);
		}
	}
}
