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
	public partial class RowNormal : Grid, IAttrRow
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
				mx_value.Text = value;
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
				switch(m_name)
				{
					case "skin":
						mx_skinFrame.Visibility = Visibility.Visible;
						mx_link.Visibility = Visibility.Visible;
						break;
					case "image":
						mx_skinFrame.Visibility = Visibility.Visible;
						mx_link.Visibility = Visibility.Collapsed;
						break;
					case "ImageName":
						mx_skinFrame.Visibility = Visibility.Visible;
						mx_link.Visibility = Visibility.Collapsed;
						break;
					default:
						mx_skinFrame.Visibility = Visibility.Collapsed;
						mx_link.Visibility = Visibility.Collapsed;
						break;
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

		public RowNormal(AttrDef_T attrDef = null, string name = "", string value = "", AttrList parent = null)
		{
			InitializeComponent();
			m_parent = parent;
			if (attrDef != null)
			{
				m_isCommon = attrDef.m_isCommon;
				m_subType = attrDef.m_subType;
				mt_type = attrDef.m_type;
			}
			else
			{
				m_isCommon = false;
				m_subType = "";
				mt_type = "string";
			}
			m_eventLock = false;

			m_name = name;
			m_preValue = value;
			m_type = mt_type;
			if (m_subType != null && m_subType != "")
			{
				switch (m_subType)
				{
					case "halfNormal":
						{
							mx_c2.Width = new GridLength(75);
							this.MinWidth = 150;
							mx_root.MinWidth = 150;
						}
						break;
					default:
						{

						}
						break;
				}
			}
		}

		private void mx_value_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return || e.Key == Key.Enter)
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
		private void mx_link_Click(object sender, RoutedEventArgs e)
		{
			if (m_parent.m_basic != null && m_parent.m_basic is Basic)
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
							MainWindow.s_pW.mx_showTextTab.IsChecked = true;
						}
						break;
					case "image":
						{
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
			if (m_parent.m_basic != null && m_parent.m_basic is Basic)
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
			if (m_parent.m_basic != null && m_parent.m_basic is ResBasic)
			{
				switch (m_name)
				{
					case "image":
						{
							BoloUI.SelImage winImage = new BoloUI.SelImage(this);

							winImage.ShowDialog();
						}
						break;
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
	}
}
