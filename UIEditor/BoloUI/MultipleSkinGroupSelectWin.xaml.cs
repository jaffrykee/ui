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
using UIEditor.Project;

namespace UIEditor.BoloUI
{
	public partial class MultipleSkinGroupSelectWin
	{
		public XmlControl m_xmlCtrl;
		public string m_skinName;
		public List<SkinUsingCount_T> m_lstSkinCount;

		static public MultipleSkinGroupSelectWin s_pW;

		public MultipleSkinGroupSelectWin(XmlControl xmlCtrl, string skinName, List<SkinUsingCount_T> lstSkinCount)
		{
			s_pW = this;
			m_xmlCtrl = xmlCtrl;
			m_skinName = skinName;
			m_lstSkinCount = lstSkinCount;

			InitializeComponent();
			refreshSkinGroupComboBox();
			this.Owner = MainWindow.s_pW;
		}

		public void refreshSkinGroupComboBox()
		{
			mx_tbSkinName.Text = "发现了多个皮肤组定义了该名称(" + m_skinName + ")的皮肤，请选择想要引用的皮肤组。";
			mx_cbSkinGroup.Items.Clear();
			if(m_lstSkinCount != null && m_lstSkinCount.Count > 0)
			{
				foreach (SkinUsingCount_T skinCountDef in m_lstSkinCount)
				{
					ComboBoxItem cbiSkinGroup = new ComboBoxItem();

					cbiSkinGroup.Content = skinCountDef.m_groupName;
					mx_cbSkinGroup.Items.Add(cbiSkinGroup);
				}
				mx_cbSkinGroup.SelectedIndex = 0;
			}
		}

		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{

		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			s_pW = null;
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			object selObj = mx_cbSkinGroup.SelectedItem;
			
			if (selObj != null && selObj is ComboBoxItem)
			{
				ComboBoxItem selItem = (ComboBoxItem)selObj;
				string skinGroupName = selItem.Content.ToString();

				if (skinGroupName != null && skinGroupName !="")
				{
					Public.ResultLink.createResult("\r\n已自动在本UI添加皮肤[" + m_skinName + "]的所在皮肤组[", Public.ResultType.RT_INFO);
					Public.ResultLink.createResult(skinGroupName, Public.ResultType.RT_INFO, Project.Setting.getSkinGroupPathByName(skinGroupName));
					Public.ResultLink.createResult("]", Public.ResultType.RT_INFO);
					if (m_xmlCtrl != null)
					{
						m_xmlCtrl.addSkinGroup(skinGroupName);
					}
				}
			}

			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
