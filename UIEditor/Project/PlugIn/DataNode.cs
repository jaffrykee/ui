using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIEditor.Project.PlugIn
{
	public class DataNode
	{
		public DataNodesDef m_parent;
		public string m_name;
		public HashSet<string> m_hlstClassName;
		public HashSet<string> m_hlstChildNode;

		public DataNode(DataNodesDef parent, string name)
		{
			m_parent = parent;
			m_name = name;
			m_hlstClassName = new HashSet<string>();
			m_hlstChildNode = new HashSet<string>();
		}
	}
}
