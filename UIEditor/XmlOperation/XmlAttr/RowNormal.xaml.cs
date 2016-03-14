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
	public partial class RowNormal : Grid, IAttrRow
	{
		private string mt_name;
		private string mt_value;
		private string mt_type;
		private bool m_eventLock;
		private Button mx_link;
		private Button mx_new;
		private Button mx_sel;
		private CustomWPFColorPicker.ColorPickerControlView mx_viewColor;
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
						{
							createSelButton();
							createLinkButton();
							mx_c2.Width = new GridLength(200);
						}
						break;
					case "image":
						{
							createSelButton();
						}
						break;
					case "ImageName":
						{
							createSelButton();
						}
						break;
					case "particleName":
						{
							createSelButton();
						}
						break;
					default:
						{

						}
						break;
				}
			}
		}
		private void createSelButton()
		{
			mx_sel = new Button();

			mx_sel.Content = "更改";
			mx_sel.Click += mx_sel_Click;
			mx_exFrame.Children.Add(mx_sel);
		}
		private void createLinkButton()
		{
			mx_link = new Button();
			mx_new = new Button();

			mx_new.Content = "新建";
			mx_new.Click += mx_new_Click;
			mx_exFrame.Children.Add(mx_new);
			mx_link.Content = "跳转";
			mx_link.Click += mx_link_Click;
			mx_exFrame.Children.Add(mx_link);
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

		public RowNormal(DataAttr attrDef = null, string name = "", string value = "", AttrList parent = null)
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
			if(m_type == "Color")
			{
				mx_viewColor = new CustomWPFColorPicker.ColorPickerControlView();

				mx_exFrame.Children.Add(mx_viewColor);
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
			if (mx_link != null)
			{
				if (mx_value.Text == "")
				{
					mx_valueDef.Visibility = Visibility.Visible;
				}
				else
				{
					mx_valueDef.Visibility = Visibility.Hidden;
				}
			}
		}
		private void mx_new_Click(object sender, RoutedEventArgs e)
		{
			if (m_parent.m_basic != null && m_parent.m_basic is Basic)
			{
				switch (m_name)
				{
					case "skin":
						{
							newSkin winNewSkin = new newSkin(this);

							winNewSkin.ShowDialog();
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
					case "particleName":
						{
						}
						break;
					default:
						break;
				}
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
							if(m_parent.m_xmlCtrl.findSkinAndSelect(mx_value.Text, (BoloUI.Basic)m_parent.m_basic))
							{
								MainWindow.s_pW.mx_showTextTab.IsChecked = true;
							}
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
					case "particleName":
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
					case "particleName":
						{
							BoloUI.SelParticle winParticle = new BoloUI.SelParticle(this);

							winParticle.ShowDialog();
						}
						break;
					default:
						break;
				}
			}
		}
	}
}
