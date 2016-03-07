using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.Project.PlugIn
{
	public class WeightRowGroup_T
	{
		public string m_groupName;
		public List<string> m_lstRow;

		public WeightRowGroup_T(string groupName)
		{
			m_groupName = groupName;
			m_lstRow = new List<string>();
		}
	}
	public class DataAttr
	{
		//右侧属性工具栏的UI
		public IAttrRow m_iAttrRowUI;
		//数据类型
		public string m_type;
		//子类型
		public string m_subType;
		//默认值
		public string m_defValue;
		//枚举
		public bool m_isEnum;
		//常用项
		public bool m_isCommon;
		//枚举列表
		public Dictionary<string, ComboBoxItem> m_mapEnum;
		public ArrayList m_lstWeight;

		public DataAttr(string type = "int", string defValue = "", IAttrRow iRowUI = null, bool isEnum = false, Dictionary<string, ComboBoxItem> mapEnum = null)
		{
			m_iAttrRowUI = iRowUI;
			m_type = type;
			m_defValue = defValue;
			m_isEnum = isEnum;
			m_mapEnum = mapEnum;
			m_lstWeight = null;
		}
	}
}
