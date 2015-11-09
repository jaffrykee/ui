using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIEditor;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;

namespace UIEditor.XmlOperation.XmlAttr
{
	/// <summary>
	/// RowEnum.xaml 的交互逻辑
	/// </summary>
	public partial class RowEnum : Grid, IAttrRow
	{
		private string mt_name;
		private string mt_value;
		private string mt_type;
		private bool m_eventLock;
		private void setValue(bool isPre, string value)
		{
			if (mt_value != value && m_eventLock == false)
			{
				if (!isPre && m_parent != null && m_parent.m_xmlCtrl != null &&
					m_parent.m_xe != null && m_parent.m_xmlCtrl.m_openedFile != null &&
					m_parent.m_xmlCtrl.m_openedFile.m_lstOpt != null && m_parent.m_basic != null)
				{
					m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(m_parent.m_basic.m_xe, m_name, mt_value, value));
				}
				mt_value = value;
				m_eventLock = true;

				ComboBoxItem selCb;

				if (m_mapEnum != null && m_mapEnum.TryGetValue(value, out selCb) && selCb != null)
				{
					selCb.IsSelected = true;
				}
				else
				{
					mx_defaultEnum.IsSelected = true;
				}
				m_eventLock = false;
			}
		}

		public AttrList m_parent { get; set; }
		public bool m_isCommon { get; set; }
		public string m_subType { get; set; }
		public string m_name
		{
			get { return mt_name; }
			set
			{
				if (m_subType == "Apperance")
				{

				}
				mt_name = value;

				string outStr = MainWindow.s_pW.m_strDic.getWordByKey(value);
				if(outStr != "")
				{
					string tip = MainWindow.s_pW.m_strDic.getWordByKey(value, StringDic.conf_ctrlAttrTipDic);

					mx_nameEnum.Content = outStr;
					if (tip != "")
					{
						mx_root.ToolTip = tip;
					}
					else
					{
						mx_root.ToolTip = value;
					}
				}
				else
				{
					mx_nameEnum.Content = value;
					mx_root.ToolTip = value;
				}
			}
		}
		public string m_preValue
		{
			get { return mt_value; }
			set
			{
				setValue(true, value);
			}
		}
		public string m_value
		{
			get { return mt_value; }
			set
			{
				setValue(false, value);
			}
		}
		public string m_type { get; set; }
		public Dictionary<string, ComboBoxItem> m_mapEnum;

		public RowEnum(AttrDef_T attrDef, string name = "", string value = "", AttrList parent = null)
		{
			InitializeComponent();
			m_parent = parent;
			m_mapEnum = attrDef.m_mapEnum;
			m_isCommon = attrDef.m_isCommon;
			m_subType = attrDef.m_subType;
			m_eventLock = false;

			m_name = name;
			m_preValue = value;
			m_type = attrDef.m_type;

			if (m_mapEnum != null && m_mapEnum.Count() > 0)
			{
				foreach (KeyValuePair<string, ComboBoxItem> pairEnum in m_mapEnum.ToList())
				{
					ComboBoxItem cbEnum = new ComboBoxItem();
					string strEnum = "";
					if (m_subType != null && m_subType != "")
					{
						strEnum = MainWindow.s_pW.m_strDic.getWordByKey(pairEnum.Key, StringDic.conf_ctrlAttrTipDic + "_" + m_subType);
					}

					if (strEnum == "")
					{
						strEnum = pairEnum.Key;
					}
					cbEnum.Content = strEnum;
					cbEnum.ToolTip = pairEnum.Key;
					m_mapEnum[pairEnum.Key] = cbEnum;
					mx_valueEnum.Items.Add(cbEnum);
				}
			}
		}

		private void mx_valueEnum_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if((ComboBox)sender != null && ((ComboBox)sender).SelectedItem != null &&
				((ComboBox)sender).SelectedItem is ComboBoxItem)
			{
				ComboBoxItem selCb = (ComboBoxItem)(((ComboBox)sender).SelectedItem);

				m_value = selCb.ToolTip.ToString();
			}
		}
	}
}
