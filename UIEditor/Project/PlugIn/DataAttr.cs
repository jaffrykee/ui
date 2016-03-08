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
	/// <summary>
	/// 一切插件xml数据配置文档属性设置的基类。
	/// </summary>
	public class DataAttr
	{
		/// <summary>
		/// 右侧属性工具栏的UI
		/// </summary>
		public DataAttrGroup m_parent;
		public IAttrRow m_iAttrRowUI;
		/// <summary>
		/// 数据类型
		/// </summary>
		public string m_type;
		/// <summary>
		/// 子类型
		/// </summary>
		public string m_subType;
		/// <summary>
		/// 默认值
		/// </summary>
		public string m_defValue;
		/// <summary>
		/// 枚举
		/// </summary>
		public bool m_isEnum;
		/// <summary>
		/// 常用项
		/// </summary>
		public bool m_isCommon;
		/// <summary>
		/// 枚举列表
		/// </summary>
		public Dictionary<string, ComboBoxItem> m_mapEnum;
		public ArrayList m_lstWeight;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">数据类型</param>
		/// <param name="defValue">默认值</param>
		/// <param name="iRowUI">属性条目UI接口</param>
		/// <param name="isEnum">是否是枚举类型</param>
		/// <param name="mapEnum">枚举单选UI字典</param>
		public DataAttr(DataAttrGroup parent, string type = "int", string defValue = "", IAttrRow iRowUI = null,
			bool isEnum = false, Dictionary<string, ComboBoxItem> mapEnum = null)
		{
			m_parent = parent;
			m_iAttrRowUI = iRowUI;
			m_type = type;
			m_defValue = defValue;
			m_isEnum = isEnum;
			m_mapEnum = mapEnum;
			m_lstWeight = null;
		}
	}
}
