using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UIEditor.BoloUI.DefConfig
{
	public class SkinDef_T
	{
		public Dictionary<string, SkinDef_T> m_mapEnChild;
		public Dictionary<string, AttrDef_T> m_mapAttrDef;
		public AttrList m_skinAttrList;

		public SkinDef_T(Dictionary<string, SkinDef_T> mapChild, Dictionary<string, AttrDef_T> mapAttrDef, AttrList attrListUI)
		{
			m_mapEnChild = mapChild;
			m_mapAttrDef = mapAttrDef;
			m_skinAttrList = attrListUI;
		}
	}
}
