using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UIEditor.XmlOperation.XmlAttr;
using UIEditor.Project.PlugIn;

namespace UIEditor.BoloUI.DefConfig
{
	/// <summary>
	/// 用于表示BoloUI控件属性结构的类
	/// </summary>
	public class CtrlDef_T : Project.PlugIn.DataNode
	{
		public Dictionary<string, string> m_mapApprPrefix;
		public Dictionary<string, string> m_mapApprSuffix;
		public bool m_isFrame;
		public bool m_isBasic;
		public bool m_hasBasic;
		public bool m_hasPointerEvent;
		public bool m_enInsert;
		public bool m_enInsertAll;

		public CtrlDef_T(DataNodesDef parent, string name, Dictionary<string, DataAttr> mapDataAttr, AttrList attrListUI)
			: base(parent, name, mapDataAttr, attrListUI)
		{
			m_mapApprSuffix = new Dictionary<string, string>();
			m_mapApprPrefix = new Dictionary<string, string>();
		}
	}
}
