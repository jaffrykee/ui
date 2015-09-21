using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIEditor.Public
{
	public class EventLock
	{
		private bool m_isLock;

		public EventLock()
		{
			m_isLock = false;
		}

		public void addLock(out bool stackLock)
		{
			if(m_isLock)
			{
				stackLock = true;
			}
			else
			{
				m_isLock = true;
				stackLock = false;
			}
		}
		public void delLock(ref bool stackLock)
		{
			if(stackLock)
			{
				stackLock = false;
			}
			else
			{
				m_isLock = false;
			}
		}
		public bool isLock()
		{
			return m_isLock;
		}
	}
}
