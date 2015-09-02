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

namespace UIEditor.BoloUI
{
	public partial class newSkin : Window
	{
		public AttrRow m_attrRow;
		public string m_skinGroup;
		public string m_skinName;

		static public newSkin s_pW;

		public newSkin(AttrRow attrRow)
		{
			s_pW = this;
			m_attrRow = attrRow;
			m_skinGroup = null;
			m_skinName = null;
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			if (MainWindow.s_pW.m_skinPath != null && MainWindow.s_pW.m_skinPath != "" &&
				Directory.Exists(MainWindow.s_pW.m_skinPath))
			{
				DirectoryInfo skinDi = new DirectoryInfo(MainWindow.s_pW.m_skinPath);

				foreach(FileInfo fi in skinDi.GetFiles())
				{
					ComboBoxItem cbiSkin = new ComboBoxItem();

					cbiSkin.Content = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
					cbiSkin.ToolTip = fi.FullName;
					cbiSkin.Selected += mx_groupCbi_Selected;
					mx_groupCbBox.Items.Add(cbiSkin);
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
			if(sender != null && sender.GetType().ToString() == "System.Windows.Controls.ComboBoxItem")
			{
				ComboBoxItem cbiGroup = (ComboBoxItem)sender;

				m_skinGroup = cbiGroup.ToolTip.ToString();
			}
		}
		private void mx_localCbi_Selected(object sender, RoutedEventArgs e)
		{
			m_skinGroup = m_attrRow.m_parent.m_xmlCtrl.m_openedFile.m_path;
		}
		private void mx_skinName_TextChanged(object sender, TextChangedEventArgs e)
		{
			m_skinName = mx_skinName.Text;
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if(m_skinGroup == null)
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
					OpenedFile fileDef;

					m_attrRow.m_value = m_skinName;
					if (System.IO.File.Exists(m_skinGroup))
					{
						if (!MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(m_skinGroup, out fileDef))
						{
							MainWindow.s_pW.openFileByPath(m_skinGroup);
						}
						if (MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(m_skinGroup, out fileDef))
						{
							if (fileDef.m_frame.GetType().ToString() == "UIEditor.XmlControl")
							{
								this.Close();
								XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;
								XmlElement newXe = xmlCtrl.m_xmlDoc.CreateElement("skin");

								newXe.SetAttribute("Name", m_skinName);
								xmlCtrl.m_treeSkin.addResItem(newXe);

								if (m_attrRow != null && m_attrRow.m_parent != null && m_attrRow.m_parent.m_basic != null &&
									m_attrRow.m_parent.m_basic.GetType().ToString() == "UIEditor.BoloUI.Basic")
								{
									xmlCtrl.findSkinAndSelect(m_skinName, (BoloUI.Basic)m_attrRow.m_parent.m_basic);
								}
								else
								{
									xmlCtrl.findSkinAndSelect(m_skinName);
								}
							}
						}
					}
				}
			}
			mx_errorInfo.Content = "皮肤组文件不存在或格式错误";
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
