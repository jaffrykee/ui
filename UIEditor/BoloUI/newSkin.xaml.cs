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
using System.IO;
using System.Xml;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.BoloUI
{
	public partial class newSkin
	{
		public IAttrRow m_iAttrRow;
		public string m_pathSkinGroup;
		public string m_skinGroupShortName;
		public string m_skinName;
		public string m_skinContent;

		static public newSkin s_pW;

		public newSkin(IAttrRow iAttrRow)
		{
			s_pW = this;
			m_iAttrRow = iAttrRow;
			m_pathSkinGroup = m_iAttrRow.m_parent.m_xmlCtrl.m_openedFile.m_path;
			m_skinGroupShortName = "";
			m_skinName = null;
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			if (Project.Setting.s_skinPath != null && Project.Setting.s_skinPath != "" &&
				Directory.Exists(Project.Setting.s_skinPath))
			{
				DirectoryInfo skinDi = new DirectoryInfo(Project.Setting.s_skinPath);

				foreach(FileInfo fi in skinDi.GetFiles())
				{
					ComboBoxItem cbiSkin = new ComboBoxItem();

					cbiSkin.Content = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
					cbiSkin.ToolTip = fi.FullName;
					cbiSkin.Selected += mx_groupCbi_Selected;
					mx_groupCbBox.Items.Add(cbiSkin);
				}
			}

			object ret = XmlItemContextMenu.showTmplGroup("skin", mx_tmplCbBox, mx_tmplCbi_Selected, m_iAttrRow.m_parent.m_xe.Name + "_skinTmpl");

			if (mx_tmplCbBox.Items.Count > 0)
			{
				if (ret != null && ret is ComboBoxItem)
				{
					ComboBoxItem cbiTmpl = (ComboBoxItem)ret;

					cbiTmpl.IsSelected = true;
				}
				else
				{
					if (mx_tmplCbBox.Items.GetItemAt(0) is ComboBoxItem)
					{
						ComboBoxItem cbiFirst = (ComboBoxItem)mx_tmplCbBox.Items.GetItemAt(0);

						cbiFirst.IsSelected = true;
					}
				}
			}
		}

		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{

		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			s_pW = null;
		}
		private void mx_groupCbi_Selected(object sender, RoutedEventArgs e)
		{
			if(sender != null && sender is ComboBoxItem)
			{
				ComboBoxItem cbiGroup = (ComboBoxItem)sender;

				m_pathSkinGroup = cbiGroup.ToolTip.ToString();
				m_skinGroupShortName = cbiGroup.Content.ToString();
			}
		}
		private void mx_localCbi_Selected(object sender, RoutedEventArgs e)
		{
			m_pathSkinGroup = m_iAttrRow.m_parent.m_xmlCtrl.m_openedFile.m_path;
			m_skinGroupShortName = "";
		}
		private void mx_skinName_TextChanged(object sender, TextChangedEventArgs e)
		{
			m_skinName = mx_skinName.Text;
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if(m_pathSkinGroup == null)
			{
				mx_errorInfo.Content = "请选择一个皮肤组";
			}
			else
			{
				if(m_skinName == null || m_skinName == "")
				{
					mx_errorInfo.Content = "请填写皮肤名";
				}
				else
				{
					m_iAttrRow.m_value = m_skinName;
					if (System.IO.File.Exists(m_pathSkinGroup))
					{
						XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

						if (curXmlCtrl != null)
						{
							curXmlCtrl.addSkinGroup(m_skinGroupShortName);
						}

						XmlControl.createSkin(m_pathSkinGroup, m_skinName, m_skinContent, m_iAttrRow);
					}
				}
			}
			mx_errorInfo.Content = "皮肤组文件不存在或格式错误";
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void mx_tmplCbi_Selected(object sender, RoutedEventArgs e)
		{
			if(sender is ComboBoxItem)
			{
				ComboBoxItem cbiSel = (ComboBoxItem)sender;

				if(cbiSel.ToolTip.ToString() != "")
				{
					m_skinContent = cbiSel.ToolTip.ToString();
				}
			}
		}
	}
}
