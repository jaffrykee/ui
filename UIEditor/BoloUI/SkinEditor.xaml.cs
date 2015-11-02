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
using System.Windows.Shapes;
using System.Xml;

namespace UIEditor.BoloUI
{
	/// <summary>
	/// SkinEditor.xaml 的交互逻辑
	/// </summary>
	public partial class SkinEditor : TabItem
	{
		public object m_curSkin;
		private Basic mt_curCtrl;
		public Basic m_curCtrl
		{
			get { return mt_curCtrl; }
			set
			{
				mt_curCtrl = value;

				if(value.m_rootControl != null)
				{
					string skinName = value.m_xe.GetAttribute("skin");

					if (skinName != "")
					{
						object retSkin = value.m_rootControl.findSkin(skinName);

						if(retSkin != null)
						{
							mx_skinApprPre.Items.Clear();
							mx_skinApprSuf.Items.Clear();

							m_curSkin = retSkin;

							string ctrlName = mt_curCtrl.m_xe.Name;
							DefConfig.CtrlDef_T ctrlDef;

							if(MainWindow.s_pW.m_mapCtrlDef.TryGetValue(ctrlName, out ctrlDef))
							{
								foreach(KeyValuePair<string, string> pairPrefix in ctrlDef.m_mapApprPrefix.ToList())
								{
									ComboBoxItem cbItem = new ComboBoxItem();

									cbItem.Content = pairPrefix.Value;
									if (pairPrefix.Key == "_def")
									{
										cbItem.ToolTip = "";
									}
									else
									{
										cbItem.ToolTip = pairPrefix.Key;
									}
									cbItem.Selected += mx_cbItemPrefix_Selected;
									mx_skinApprPre.Items.Add(cbItem);
								}
								foreach (KeyValuePair<string, string> pairSuffix in ctrlDef.m_mapApprSuffix.ToList())
								{
									ComboBoxItem cbItem = new ComboBoxItem();

									cbItem.Content = pairSuffix.Value;
									cbItem.ToolTip = pairSuffix.Key;
									cbItem.Selected += mx_cbItemSuffix_Selected;
									mx_skinApprSuf.Items.Add(cbItem);
								}
							}

							this.Visibility = Visibility.Visible;
						}
						else
						{
							MainWindow.s_pW.mx_debug.Text += "无法找到皮肤：\"" + skinName + "\"\r\n";
						}
					}
				}
			}
		}
		public void refreshAppr(string apprName)
		{
			mx_treeAppr.Items.Clear();

			if (m_curSkin != null && apprName != null && apprName != "")
			{
				if(m_curSkin is XmlElement)
				{
					XmlElement xeSkin = (XmlElement)m_curSkin;

					foreach(XmlNode xnAppr in xeSkin.ChildNodes)
					{
						if(xnAppr is XmlElement)
						{
							XmlElement xeAppr = (XmlElement)xnAppr;

							if (xeAppr.Name == "apperance" && xeAppr.GetAttribute("id") == apprName)
							{
								foreach(XmlNode xnShape in xeAppr.ChildNodes)
								{
									if(xnShape is XmlElement)
									{
										XmlElement xeShape = (XmlElement)xnShape;

										ResBasic apprCtrl = new ResBasic(xeShape, XmlControl.getCurXmlControl(), MainWindow.s_pW.m_mapSkinAllDef[xeShape.Name]);
										mx_treeAppr.Items.Add(apprCtrl);
									}
								}
							}
						}
					}
				}
			}
		}
		private void mx_cbItemSuffix_Selected(object sender, RoutedEventArgs e)
		{
			if (sender != null && sender is ComboBoxItem &&
				mx_skinApprPre.SelectedItem != null && mx_skinApprPre.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem cbPreItem = (ComboBoxItem)mx_skinApprPre.SelectedItem;
				ComboBoxItem cbSufItem = (ComboBoxItem)sender;
				string apprName = cbPreItem.ToolTip.ToString() + cbSufItem.ToolTip.ToString();

				refreshAppr(apprName);
			}
		}
		private void mx_cbItemPrefix_Selected(object sender, RoutedEventArgs e)
		{
			if (sender != null && sender is ComboBoxItem &&
				mx_skinApprSuf.SelectedItem != null && mx_skinApprSuf.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem cbPreItem = (ComboBoxItem)sender;
				ComboBoxItem cbSufItem = (ComboBoxItem)mx_skinApprSuf.SelectedItem;
				string apprName = cbPreItem.ToolTip.ToString() + cbSufItem.ToolTip.ToString();

				refreshAppr(apprName);
			}
		}

		public SkinEditor()
		{
			m_curSkin = null;
			InitializeComponent();
		}

		public void refreshSkinEditor(Basic curCtrl = null)
		{
			if (curCtrl == null)
			{
				XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

				if (curXmlCtrl != null && curXmlCtrl.m_curItem is Basic)
				{
					m_curCtrl = (Basic)(curXmlCtrl.m_curItem);
				}
			}
			else
			{
				m_curCtrl = curCtrl;
			}
		}
	}
}
