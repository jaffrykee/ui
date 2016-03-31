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
using UIEditor.Project.PlugIn;

namespace UIEditor.XmlOperation.XmlAttr
{
	/// <summary>
	/// RowBool.xaml 的交互逻辑
	/// </summary>
	public partial class RowBool : Grid, IAttrRow
	{
		private bool m_eventLock;
		private void setValue(bool isPre, string value)
		{
			if ((value == null || value == "") && m_defValue != null && m_defValue != "")
			{
				value = m_defValue;
			}
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

				switch (m_type)
				{
					case "bool":
						{
							if (value == "")
							{
								mx_valueBool.IsChecked = null;
							}
							else
							{
								switch (value)
								{
									case "true":
										mx_valueBool.IsChecked = true;
										break;
									case "false":
										mx_valueBool.IsChecked = false;
										break;
									default:
										//todo 这里是非法值的处理，还没有想好机制
										mx_valueBool.IsChecked = null;
										break;
								}
							}
						}
						break;
					default:
						break;
				}
				m_eventLock = false;
			}
		}

		public AttrList m_parent { get; set; }
		public bool m_isCommon { get; set; }
		public string m_subType { get; set; }

		private string mt_name;
		public string m_name
		{
			get { return mt_name; }
			set
			{
				mt_name = value;

				string outStr = MainWindow.s_pW.m_strDic.getWordByKey(value);
				if(outStr != "")
				{
					string tip = MainWindow.s_pW.m_strDic.getWordByKey(value, StringDic.conf_ctrlAttrTipDic);

					mx_valueBool.Content = outStr;
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
					mx_valueBool.Content = value;
					mx_root.ToolTip = value;
				}
			}
		}

		private string mt_value;
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

		private string mt_type;
		public string m_type { get; set; }

		private string mt_defValue;
		public string m_defValue { get; set; }

		public RowBool(DataAttr attrDef, string name = "", string value = "", AttrList parent = null)
		{
			InitializeComponent();
			m_parent = parent;
			m_isCommon = attrDef.m_isCommon;
			m_subType = attrDef.m_subType;
			m_eventLock = false;

			m_name = name;
			m_preValue = value;
			m_type = attrDef.m_type;
			m_defValue = attrDef.m_defValue;

            if (m_subType != null && m_subType != "")
            {
                switch (m_subType)
                {
                    case "allBool":
                        {
                            mx_root.MinWidth = 300;
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
		}

		private void mx_valueBool_Checked(object sender, RoutedEventArgs e)
		{
			m_value = "true";
		}
		private void mx_valueBool_Unchecked(object sender, RoutedEventArgs e)
		{
			m_value = "false";
		}
		private void mx_valueBool_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			m_value = "";
		}
	}
}
