using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UIEditor.BoloUI.DefConfig
{
	public class CtrlDef_T
	{
		public Dictionary<string, AttrDef_T> m_mapAttrDef;
		public AttrList m_ctrlAttrList;
		public Dictionary<string, string> m_mapApprPrefix;
		public Dictionary<string, string> m_mapApprSuffix;
		public bool m_isFrame;
		public bool m_isBasic;
		public bool m_hasBasic;
		public bool m_hasPointerEvent;
		public bool m_enInsert;
		public bool m_enInsertAll;

		public CtrlDef_T(Dictionary<string, AttrDef_T> mapAttrDef, AttrList attrListUI)
		{
			m_mapAttrDef = mapAttrDef;
			m_ctrlAttrList = attrListUI;
			m_mapApprSuffix = new Dictionary<string, string>();
			m_mapApprPrefix = new Dictionary<string, string>();
		}
	}
}
