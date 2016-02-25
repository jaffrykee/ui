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
using System.Collections;

namespace UIEditor.XmlOperation.XmlAttr
{
	/// <summary>
	/// RowEnum.xaml 的交互逻辑
	/// </summary>
	public partial class RowWeight : Grid, IAttrRow
	{
		private string mt_name;
		private string mt_value;
		private string mt_type;
		private bool m_eventLock;
		private Dictionary<string, CheckBox> m_mapRow;
		private Dictionary<string, Dictionary<string, RadioButton>> m_mapRowGroup;
		private Dictionary<RadioButton, string> m_mapRbGroupName;
		private Dictionary<string, int> m_mapGroupTakeBack;

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

				int iValue;

				if (!int.TryParse(value, out iValue))
				{
					iValue = 0;
				}
				foreach (object row in m_lstWeight)
				{
					if (row is string)
					{
						int iRow;
						bool isChecked = false;
						CheckBox cb;

						if (int.TryParse((string)row, out iRow) && (iValue & iRow) > 0)
						{
							isChecked = true;
						}
						else
						{
							isChecked = false;
						}
						if (m_mapRow.TryGetValue((string)row, out cb) && cb != null)
						{
							cb.IsChecked = isChecked;
						}
					}
					else if (row is WeightRowGroup_T)
					{
						WeightRowGroup_T gRow = (WeightRowGroup_T)row;
						Dictionary<string, RadioButton> mapRow;

						if (m_mapRowGroup.TryGetValue(gRow.m_groupName, out mapRow))
						{
							foreach (string subRow in gRow.m_lstRow)
							{
								int iSubRow;
								bool isChecked = false;
								RadioButton rb;

								if (int.TryParse(subRow, out iSubRow))
								{
									if (iSubRow == 0 || (iValue & iSubRow) == iSubRow)
									{
										isChecked = true;
									}
									else
									{
										isChecked = false;
									}
								}
								else
								{
									isChecked = false;
								}

								if (mapRow.TryGetValue((string)subRow, out rb) && rb != null)
								{
									rb.IsChecked = isChecked;
									if (isChecked)
									{
										break;
									}
								}
							}
						}
					}
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
				if (outStr != "")
				{
					string tip = MainWindow.s_pW.m_strDic.getWordByKey(value, StringDic.conf_ctrlAttrTipDic);

					mx_name.Content = outStr;
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
					mx_name.Content = value;
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

		private string mt_defValue;
		public string m_defValue { get; set; }

		public ArrayList m_lstWeight;

		public RowWeight(AttrDef_T attrDef, string name = "", string value = "", AttrList parent = null)
		{
			m_mapRow = new Dictionary<string, CheckBox>();
			m_mapRowGroup = new Dictionary<string, Dictionary<string, RadioButton>>();
			m_mapRbGroupName = new Dictionary<RadioButton, string>();
			m_mapGroupTakeBack = new Dictionary<string, int>();
			InitializeComponent();
			m_parent = parent;
			m_lstWeight = attrDef.m_lstWeight;
			m_isCommon = attrDef.m_isCommon;
			m_subType = attrDef.m_subType;
			m_eventLock = false;

			if (m_lstWeight != null && m_lstWeight.Count > 0)
			{
				foreach (object rowWt in m_lstWeight)
				{
					if (rowWt != null)
					{
						if (rowWt is string)
						{
							string rowName = (string)rowWt;
							CheckBox cbRow = new CheckBox();
							string strRow = "";

							if (m_subType != null && m_subType != "")
							{
								strRow = MainWindow.s_pW.m_strDic.getWordByKey(rowName, StringDic.conf_ctrlAttrTipDic + "_" + m_subType);
							}

							if (strRow == "")
							{
								strRow = rowName;
							}
							cbRow.Content = strRow;
							cbRow.ToolTip = rowName;
							cbRow.Margin = new Thickness(5);
							cbRow.IsChecked = false;
							cbRow.Checked += mx_cbRow_Checked;
							cbRow.Unchecked += mx_cbRow_Unchecked;
							mx_valueFrame.Children.Add(cbRow);
							m_mapRow.Add(rowName, cbRow);
						}
						else if (rowWt is WeightRowGroup_T)
						{
							WeightRowGroup_T wrgDef = (WeightRowGroup_T)rowWt;
							WrapPanel wpGroup = new WrapPanel();
							Label lbTitle = new Label();
							string strGroup = "";
							Dictionary<string, RadioButton> mapRow = new Dictionary<string, RadioButton>();

							if (m_subType != null && m_subType != "")
							{
								strGroup = MainWindow.s_pW.m_strDic.getWordByKey(wrgDef.m_groupName, StringDic.conf_ctrlAttrTipDic + "_" + m_subType);
							}

							if (strGroup == "")
							{
								strGroup = wrgDef.m_groupName;
							}
							lbTitle.Content = strGroup;
							lbTitle.ToolTip = wrgDef.m_groupName;
							wpGroup.Children.Add(lbTitle);
							wpGroup.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0x00, 0x00, 0x00));
							mx_valueFrame.Children.Add(wpGroup);

							int numTakeBack = 0;

							foreach (string rowName in wrgDef.m_lstRow)
							{
								RadioButton rbRow = new RadioButton();
								string strRow = "";

								if (m_subType != null && m_subType != "")
								{
									strRow = MainWindow.s_pW.m_strDic.getWordByKey(rowName, StringDic.conf_ctrlAttrTipDic + "_" + m_subType);
								}

								if (strRow == "")
								{
									strRow = rowName;
								}
								rbRow.Content = strRow;
								rbRow.ToolTip = rowName;
								rbRow.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
								rbRow.Checked += mx_rbRow_Checked;
								rbRow.Margin = new Thickness(1);
								rbRow.Padding = new Thickness(5);
								wpGroup.Children.Add(rbRow);
								mapRow.Add(rowName, rbRow);
								m_mapRbGroupName.Add(rbRow, wrgDef.m_groupName);

								int subNum;

								if(int.TryParse(rowName, out subNum))
								{
									numTakeBack |= subNum;
								}
							}
							m_mapRowGroup.Add(wrgDef.m_groupName, mapRow);
							m_mapGroupTakeBack.Add(wrgDef.m_groupName, numTakeBack);
						}
					}
				}
			}

			m_name = name;
			m_preValue = value;
			m_type = attrDef.m_type;
		}

		private void mx_valueEnum_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((ComboBox)sender != null && ((ComboBox)sender).SelectedItem != null &&
				((ComboBox)sender).SelectedItem is ComboBoxItem)
			{
				ComboBoxItem selCb = (ComboBoxItem)(((ComboBox)sender).SelectedItem);

				m_value = selCb.ToolTip.ToString();
			}
		}
		void mx_cbRow_Checked(object sender, RoutedEventArgs e)
		{
			int iWtValue;

			if (sender is CheckBox && int.TryParse(((CheckBox)sender).ToolTip.ToString(), out iWtValue))
			{
				int iValue;

				if (m_value != "" && int.TryParse(m_value, out iValue))
				{
					iValue |= iWtValue;
				}
				else
				{
					iValue = iWtValue;
				}

				m_value = iValue.ToString();
			}
		}
		void mx_cbRow_Unchecked(object sender, RoutedEventArgs e)
		{
			int iWtValue;

			if (sender is CheckBox && int.TryParse(((CheckBox)sender).ToolTip.ToString(), out iWtValue))
			{
				int iValue;

				if (m_value != "" && int.TryParse(m_value, out iValue))
				{
					if ((iValue & iWtValue) != 0)
					{
						iValue -= iWtValue;
					}
				}
				else
				{
					iValue = 0;
				}

				m_value = iValue.ToString();
			}
		}
		void mx_rbRow_Checked(object sender, RoutedEventArgs e)
		{
			int iWtValue;

			if (sender is RadioButton && int.TryParse(((RadioButton)sender).ToolTip.ToString(), out iWtValue))
			{
				RadioButton rbSender = (RadioButton)sender;
				string groupName;

				if(m_mapRbGroupName.TryGetValue(rbSender, out groupName) && groupName != null)
				{
					int iTakeBack;

					if(m_mapGroupTakeBack.TryGetValue(groupName, out iTakeBack))
					{
						int iValue;

						if (m_value != "" && int.TryParse(m_value, out iValue))
						{
							iValue &= (~iTakeBack);
							iValue |= iWtValue;
						}
						else
						{
							iValue = iWtValue;
						}

						m_value = iValue.ToString();
					}
				}
			}
		}
	}
}
