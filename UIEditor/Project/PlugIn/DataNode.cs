using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.Project.PlugIn
{
	public class DataNode
	{
		public DataNodeGroup m_parent;
		public string m_name;
		public HashSet<string> m_hlstClassName;
		public HashSet<string> m_hlstChildNode;
		public Dictionary<string, DataAttrGroup> m_mapDataAttrGroup;

		public DataNode(DataNodeGroup parent, string name)
		{
			m_parent = parent;
			m_name = name;
			m_hlstClassName = new HashSet<string>();
			m_hlstChildNode = new HashSet<string>();
			m_mapDataAttrGroup = new Dictionary<string, DataAttrGroup>();
		}

		public bool tryGetAttrDef(string attrName, out DataAttr attrDef)
		{
			attrDef = null;
			if(attrName != null && attrName != "")
			{
				foreach(KeyValuePair<string, DataAttrGroup> pairGroup in m_mapDataAttrGroup.ToList())
				{
					if(pairGroup.Value.m_mapDataAttr.TryGetValue(attrName, out attrDef))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
