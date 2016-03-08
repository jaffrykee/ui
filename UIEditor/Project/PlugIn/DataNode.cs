using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.Project.PlugIn
{
	public class DataNode
	{
		public DataNodesDef m_parent;
		public string m_name;
		public HashSet<string> m_hlstClassName;
		public HashSet<string> m_hlstChildNode;
		public Dictionary<string, DataAttr> m_mapDataAttr;
		/// <summary>
		/// 对应的属性组UI。
		/// </summary>
		public AttrList m_uiAttrList;
		public Dictionary<string, AttrList> m_mapDataAttr;

		public DataNode(DataNodesDef parent, string name, Dictionary<string, DataAttr> mapDataAttr, AttrList attrListUI)
		{
			m_parent = parent;
			m_name = name;
			m_hlstClassName = new HashSet<string>();
			m_hlstChildNode = new HashSet<string>();
			m_uiAttrList = attrListUI;
		}
	}
}
