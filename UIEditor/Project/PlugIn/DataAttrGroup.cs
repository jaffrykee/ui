using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.Project.PlugIn
{
	public class DataAttrGroup
	{
		public AttrList m_uiAttrList;
		public Dictionary<string, DataAttr> m_mapDataAttr;

		public DataAttrGroup(AttrList attrList = null)
		{
			m_uiAttrList = attrList;
		}
	}
}
