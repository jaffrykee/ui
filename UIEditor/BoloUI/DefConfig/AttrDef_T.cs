using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace UIEditor.BoloUI.DefConfig
{
	public class AttrDef_T
	{
		//右侧属性工具栏的UI
		public AttrRow m_attrRowUI;
		//数据类型
		public string m_type;
		//子类型
		public string m_subType;
		//默认值(废弃)
		public string m_defValue;
		//枚举
		public bool m_isEnum;
		//常用项
		public bool m_isCommon;
		//枚举列表
		public Dictionary<string, ComboBoxItem> m_mapEnum;

		public AttrDef_T(string type = "int", string defValue = null, AttrRow rowUI = null, bool isEnum = false, Dictionary<string, ComboBoxItem> mapEnum = null)
		{
			m_attrRowUI = rowUI;
			m_type = type;
			m_defValue = defValue;
			m_isEnum = isEnum;
			m_mapEnum = mapEnum;
		}
	}
}
