using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;

namespace UIEditor
{
	public partial class AttrRow : Grid
	{
		private string mt_name;
		private string mt_value;
		private string mt_type;
		public AttrList m_parent;
		public bool m_isCommon;
		public string m_subType;
		public Dictionary<string, ComboBoxItem> m_mapCbiApprPre;
		public Dictionary<string, ComboBoxItem> m_mapCbiApprSuf;

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
				if (m_subType != "" && m_subType != null)
				{
					switch(m_subType)
					{
						case "Apperance":
							{
								MainWindow pW = MainWindow.s_pW;
								Dictionary<string, Dictionary<string, string>> preDic, sufDic;

								if (pW.m_strDic.m_mapStrDic.TryGetValue("SkinApprPre", out preDic) &&
									pW.m_strDic.m_mapStrDic.TryGetValue("SkinApprSuf", out sufDic))
								{
									foreach (KeyValuePair<string, Dictionary<string, string>> pairPreRow in preDic)
									{
										ComboBoxItem cbiPre = new ComboBoxItem();

										cbiPre.Content = pW.m_strDic.getWordByKey(pairPreRow.Key, "SkinApprPre");
										cbiPre.ToolTip = pairPreRow.Key;
										mx_valueApprPre.Items.Add(cbiPre);
										m_mapCbiApprPre.Add(pairPreRow.Key, cbiPre);
									}
									foreach (KeyValuePair<string, Dictionary<string, string>> pairSufRow in sufDic)
									{
										ComboBoxItem cbiSuf = new ComboBoxItem();

										cbiSuf.Content = pW.m_strDic.getWordByKey(pairSufRow.Key, "SkinApprSuf");
										cbiSuf.ToolTip = pairSufRow.Key;
										mx_valueApprSuf.Items.Add(cbiSuf);
										m_mapCbiApprSuf.Add(pairSufRow.Key, cbiSuf);
									}
								}
								mx_apprFrame.Visibility = Visibility.Visible;
							}
							break;
						default:
							break;
					}
				}
				switch(m_name)
				{
					case "skin":
						mx_skinFrame.Visibility = System.Windows.Visibility.Visible;
						mx_link.Visibility = System.Windows.Visibility.Visible;
						break;
					case "ImageName":
						mx_skinFrame.Visibility = System.Windows.Visibility.Visible;
						mx_link.Visibility = System.Windows.Visibility.Collapsed;
						break;
					default:
						mx_skinFrame.Visibility = Visibility.Collapsed;
						break;
				}
			}
		}
		private void setValue(bool isPre, string value)
		{
			if (mt_value != value)
			{
				if (!isPre && m_parent != null && m_parent.m_xmlCtrl != null &&
					m_parent.m_xe != null && m_parent.m_xmlCtrl.m_openedFile != null &&
					m_parent.m_xmlCtrl.m_openedFile.m_lstOpt != null && m_parent.m_basic != null)
				{
					m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(m_parent.m_basic.m_xe, m_name, mt_value, value));
				}
				mt_value = value;
				mx_value.Text = value;
				if (!m_isEnum)
				{
					if (m_subType != null && m_subType != "")
					{
						switch(m_subType)
						{
							case "Apperance":
								{
									if (m_parent != null && m_parent.m_basic != null)
									{
										m_parent.m_basic.initHeader();
										if (m_parent.m_basic.m_apprPre != null && m_parent.m_basic.m_apprPre != "")
										{
											ComboBoxItem cbiPre;

											if (m_mapCbiApprPre.TryGetValue(m_parent.m_basic.m_apprPre, out cbiPre))
											{
												mx_valueApprPre.SelectedItem = cbiPre;
											}
										}
										if (m_parent.m_basic.m_apprTagStr != null && m_parent.m_basic.m_apprTagStr != "")
										{
											mx_valueApprTag.Text = m_parent.m_basic.m_apprTagStr;
										}
										else
										{
											mx_valueApprTag.Text = "";
										}
										if (m_parent.m_basic.m_apprSuf != null && m_parent.m_basic.m_apprSuf != "")
										{
											ComboBoxItem cbiSuf;

											if (m_mapCbiApprSuf.TryGetValue(m_parent.m_basic.m_apprSuf, out cbiSuf))
											{
												mx_valueApprSuf.SelectedItem = cbiSuf;
											}
										}
									}
								}
								break;
							default:
								break;
						}
					}
					else
					{
						switch (m_type)
						{
							case "bool":
								{
									if (value == "")
									{
										mx_defaultBool.IsChecked = true;
									}
									else
									{
										mx_defaultBool.IsChecked = false;
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
												mx_defaultBool.IsChecked = true;
												break;
										}
									}
								}
								break;
							default:
								break;
						}
					}
				}
				else
				{
					ComboBoxItem selCb;

					if (m_mapEnum != null && m_mapEnum.TryGetValue(value, out selCb) && selCb != null)
					{
						selCb.IsSelected = true;
					}
					else
					{
						mx_defaultEnum.IsSelected = true;
					}
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
		public string m_type
		{
			get{return mt_type;}
			set
			{
				mt_type = value;
				if(!m_isEnum)
				{
					if (m_subType != null && m_subType != "")
					{
						switch(m_subType)
						{
							case "Apperance":
								{
									//m_name.set
									//mx_apprFrame.Visibility = Visibility.Visible;
								}
								break;
							default:
								{
									mx_normalFrame.Visibility = Visibility.Visible;
								}
								break;
						}
					}
					else
					{
						switch (value)
						{
							case "bool":
								{
									mx_boolFrame.Visibility = Visibility.Visible;
								}
								break;
							default:
								{
									mx_normalFrame.Visibility = Visibility.Visible;
								}
								break;
						}
					}
				}
				else
				{
					mx_enumFrame.Visibility = Visibility.Visible;

					if(m_mapEnum != null && m_mapEnum.Count() > 0)
					{
						foreach (KeyValuePair<string, ComboBoxItem> pairEnum in m_mapEnum.ToList())
						{
							ComboBoxItem cbEnum = new ComboBoxItem();
							string strEnum = "";
							if(m_subType != null && m_subType != "")
							{
								strEnum = MainWindow.s_pW.m_strDic.getWordByKey(pairEnum.Key, StringDic.conf_ctrlAttrTipDic + "_" + m_subType);
							}

							if(strEnum == "")
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
			}
		}
		public bool m_isEnum;
		public Dictionary<string, ComboBoxItem> m_mapEnum;

		public AttrRow(string type = "string", string name = "", string value = "", AttrList parent = null)
		{
			InitializeComponent();
			mt_name = name;
			mt_value = value;
			mt_type = type;
			m_parent = parent;
			m_isEnum = false;
			m_mapEnum = null;
			m_isCommon = false;
			m_subType = "";

			m_name = mt_name;
			m_preValue = mt_value;
			m_type = mt_type;
		}
		public AttrRow(AttrDef_T attrDef, string name = "", string value = "", AttrList parent = null)
		{
			m_mapCbiApprPre = new Dictionary<string, ComboBoxItem>();
			m_mapCbiApprSuf = new Dictionary<string, ComboBoxItem>();
			InitializeComponent();
			m_parent = parent;
			m_isEnum = attrDef.m_isEnum;
			m_mapEnum = attrDef.m_mapEnum;
			m_isCommon = attrDef.m_isCommon;
			m_subType = attrDef.m_subType;

			m_name = name;
			m_preValue = value;
			m_type = attrDef.m_type;
		}

		private void mx_value_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				m_value = mx_value.Text;
			}
		}
		private void mx_value_LostFocus(object sender, RoutedEventArgs e)
		{
			m_value = mx_value.Text;
		}
		private void mx_value_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (mx_value.Text == "")
			{
				mx_valueDef.Visibility = Visibility.Visible;
				mx_link.Content = "新建";
			}
			else
			{
				mx_valueDef.Visibility = Visibility.Hidden;
				mx_link.Content = "跳转";
			}
		}
		private void mx_defaultBool_Unchecked(object sender, RoutedEventArgs e)
		{
			mx_valueBool.IsEnabled = true;

			if(mx_valueBool.IsChecked == true)
			{
				m_value = "true";
			}
			else
			{
				m_value = "false";
			}
		}
		private void mx_defaultBool_Checked(object sender, RoutedEventArgs e)
		{
			mx_valueBool.IsEnabled = false;
			m_value = "";
		}
		private void mx_valueBool_Checked(object sender, RoutedEventArgs e)
		{
			m_value = "true";
		}
		private void mx_valueBool_Unchecked(object sender, RoutedEventArgs e)
		{
			m_value = "false";
		}
		private void mx_valueBool_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}
		private void mx_valueEnum_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if((ComboBox)sender != null && ((ComboBox)sender).SelectedItem != null &&
				((ComboBox)sender).SelectedItem.GetType().ToString() == "System.Windows.Controls.ComboBoxItem")
			{
				ComboBoxItem selCb = (ComboBoxItem)(((ComboBox)sender).SelectedItem);

				m_value = selCb.ToolTip.ToString();
			}
		}
		private void mx_link_Click(object sender, RoutedEventArgs e)
		{
			if (m_parent.m_basic != null && m_parent.m_basic.GetType().ToString() == "UIEditor.BoloUI.Basic")
			{
				switch (m_name)
				{
					case "skin":
						{
							if(m_value != "")
							{
								m_parent.m_xmlCtrl.findSkinAndSelect(mx_value.Text, (BoloUI.Basic)m_parent.m_basic);
							}
							else
							{
								newSkin winNewSkin = new newSkin(this);

								winNewSkin.ShowDialog();
							}
						}
						break;
					case "ImageName":
						{
						}
						break;
					default:
						break;
				}
			}
		}
		private void mx_sel_Click(object sender, RoutedEventArgs e)
		{
			if (m_parent.m_basic != null && m_parent.m_basic.GetType().ToString() == "UIEditor.BoloUI.Basic")
			{
				switch(m_name)
				{
					case "skin":
						{
							BoloUI.SelSkin winSkin = new BoloUI.SelSkin(m_parent.m_xmlCtrl.m_openedFile.m_path, m_parent.m_basic.m_xe.OuterXml, this);

							winSkin.ShowDialog();

							if (!BoloUI.SelSkin.s_pW.m_msgMng.m_GLHost.m_process.HasExited)
							{
								BoloUI.SelSkin.s_pW.m_msgMng.m_GLHost.m_process.Kill();
								BoloUI.SelSkin.s_pW = null;
							}
						}
						break;
					default:
						break;
				}
			}
			if (m_parent.m_basic != null && m_parent.m_basic.GetType().ToString() == "UIEditor.BoloUI.ResBasic")
			{
				switch (m_name)
				{
					case "ImageName":
						{
							BoloUI.SelImage winImage = new BoloUI.SelImage(this);

							winImage.ShowDialog();
						}
						break;
					default:
						break;
				}
			}
		}

		private void mx_frame_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue.GetType().ToString() == "System.Windows.Visibility")
			{
				Visibility newValue = (Visibility)e.NewValue;

				if(newValue == System.Windows.Visibility.Visible)
				{
					if (sender.GetType().ToString() == "System.Windows.Controls.Grid")
					{
						Grid frame = (Grid)sender;

						if (frame != mx_normalFrame)
						{
							mx_normalFrame.Visibility = System.Windows.Visibility.Collapsed;
						}
						if (frame != mx_boolFrame)
						{
							mx_boolFrame.Visibility = System.Windows.Visibility.Collapsed;
						}
						if (frame != mx_enumFrame)
						{
							mx_enumFrame.Visibility = System.Windows.Visibility.Collapsed;
						}
						if (frame != mx_apprFrame)
						{
							mx_apprFrame.Visibility = System.Windows.Visibility.Collapsed;
						}
					}
				}
			}
		}
		private void getNewApprValue()
		{
			string newValue = "";

			if (mx_valueApprPre.SelectedItem != null && mx_valueApprPre.SelectedItem.GetType().ToString() == "System.Windows.Controls.ComboBoxItem")
			{
				ComboBoxItem cbiPre = (ComboBoxItem)mx_valueApprPre.SelectedItem;

				if (cbiPre.ToolTip.ToString() != "_def")
				{
					newValue += cbiPre.ToolTip.ToString();
				}
			}
			else
			{
				newValue += "";
			}
			newValue += mx_valueApprTag.Text;
			if (mx_valueApprSuf.SelectedItem != null && mx_valueApprSuf.SelectedItem.GetType().ToString() == "System.Windows.Controls.ComboBoxItem")
			{
				ComboBoxItem cbiSuf = (ComboBoxItem)mx_valueApprSuf.SelectedItem;

				newValue += cbiSuf.ToolTip.ToString();
			}
			else
			{
				newValue += "0";
			}
			m_value = newValue;
		}
		private void mx_valueAppr_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			getNewApprValue();
		}
		private void mx_valueApprTag_TextChanged(object sender, TextChangedEventArgs e)
		{
			getNewApprValue();
		}
	}
}
