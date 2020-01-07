using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public class GlobalConfiguration
	{
		private static Lazy<Settings> m_LazyConfig =
			new Lazy<Settings>(() =>
				{
					var settings = new Settings();
					return settings;
				});

		static GlobalConfiguration()
		{
			Logger = new VoidLogger();
			ProgressReporter = new DefaultProgressReporter();
			DependencyResolver = new DefaultDependencyResolver();
		}

		public static Settings Settings 
		{ 
			get
			{
				return m_LazyConfig.Value;
			} 
		}

		public static ILogger Logger { get; set; }
		public static IProgressReporter ProgressReporter { get; set; }
		public static IDependencyResolver DependencyResolver { get; set; }

	}
}
