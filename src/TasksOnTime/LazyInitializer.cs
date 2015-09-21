using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksOnTime
{
	public class Lazy<T> where T : class
	{
		private Func<T> m_Action;
		private T m_Instance;
		private static object m_Lock = new object();

		public Lazy(Func<T> action)
		{
			m_Action = action;
		}

		public T Value
		{
			get
			{
				if (m_Instance == null)
				{
					lock(m_Lock)
					{
						if (m_Instance == null)
						{
							m_Instance = m_Action.Invoke();
						}
					}
				}
				return m_Instance;
			}
		}
	}
}
