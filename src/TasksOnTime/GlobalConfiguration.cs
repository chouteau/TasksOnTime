using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime
{
	public class GlobalConfiguration
	{
		private static Settings m_Settings;
		private static Lazy<Settings> m_LazyConfig =
			new Lazy<Settings>(() =>
				{
					m_Settings = new Settings();
					return m_Settings;
				}, true);

		static GlobalConfiguration()
		{
			Logger = new DiagnosticsLogger();
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
