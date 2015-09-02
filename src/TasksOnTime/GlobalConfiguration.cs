using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class GlobalConfiguration
	{
		private static Lazy<Settings> m_LazyConfig =
			new Lazy<Settings>(() =>
				{
					var settings = new Settings();
					return settings;
				}, true);

		static GlobalConfiguration()
		{
			Logger = new DebugLogger();
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
		public static IDependencyResolver DependencyResolver { get; set; }

	}
}
