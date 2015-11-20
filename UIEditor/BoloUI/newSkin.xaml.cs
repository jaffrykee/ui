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
		public string m_skinGroup;
		public string m_skinGroupShortName;
		public string m_skinName;
		public string m_skinContent;

		static public newSkin s_pW;

		public newSkin(IAttrRow iAttrRow)
		{
			s_pW = this;
			m_iAttrRow = iAttrRow;
			m_skinGroup = m_iAttrRow.m_parent.m_xmlCtrl.m_openedFile.m_path;
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

			object ret = XmlItem.showTmplGroup("skin", mx_tmplCbBox, mx_tmplCbi_Selected, m_iAttrRow.m_parent.m_xe.Name + "_skinTmpl");

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

				m_skinGroup = cbiGroup.ToolTip.ToString();
				m_skinGroupShortName = cbiGroup.Content.ToString();
			}
		}
		private void mx_localCbi_Selected(object sender, RoutedEventArgs e)
		{
			m_skinGroup = m_iAttrRow.m_parent.m_xmlCtrl.m_openedFile.m_path;
			m_skinGroupShortName = "";
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

					m_iAttrRow.m_value = m_skinName;
					if (System.IO.File.Exists(m_skinGroup))
					{
						XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

						if (curXmlCtrl != null)
						{
							Dictionary<string, XmlElement> mapXeGroup = curXmlCtrl.getSkinGroupMap();

							if (m_skinGroupShortName != null && m_skinGroupShortName != "")
							{
								XmlElement xeOut;

								if (!mapXeGroup.TryGetValue(m_skinGroupShortName, out xeOut))
								{
									XmlElement newXe = curXmlCtrl.m_xmlDoc.CreateElement("skingroup");
									newXe.SetAttribute("Name", m_skinGroupShortName);
									BoloUI.Basic treeChild = new BoloUI.Basic(newXe, curXmlCtrl);

									curXmlCtrl.m_openedFile.m_lstOpt.addOperation(
										new XmlOperation.HistoryNode(
											XmlOperation.XmlOptType.NODE_INSERT,
											treeChild.m_xe,
											curXmlCtrl.m_xmlDoc.DocumentElement)
										);
								}
							}
						}

						MainWindow.s_pW.openFileByPath(m_skinGroup);
						if (MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(m_skinGroup, out fileDef))
						{
							if (fileDef.m_frame is XmlControl)
							{
								this.Close();
								XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;
								XmlElement newXe = xmlCtrl.m_xmlDoc.CreateElement("skin");

								if (m_skinContent != null && m_skinContent != "")
								{
									XmlDocument docTmpl = new XmlDocument();

									try
									{
										docTmpl.LoadXml(m_skinContent);
										if (docTmpl.DocumentElement != null &&
											docTmpl.DocumentElement.InnerXml != null &&
											docTmpl.DocumentElement.InnerXml != "")
										{
											newXe.InnerXml = docTmpl.DocumentElement.InnerXml;
										}
									}
									catch
									{

									}
								}
								newXe.SetAttribute("Name", m_skinName);
								xmlCtrl.m_treeSkin.addResItem(newXe);

								m_iAttrRow.m_parent.m_basic.changeSelectItem();
								MainWindow.s_pW.mx_treeFrame.SelectedItem = MainWindow.s_pW.mx_skinEditor;
// 								if (m_iAttrRow != null && m_iAttrRow.m_parent != null && m_iAttrRow.m_parent.m_basic != null &&
// 									m_iAttrRow.m_parent.m_basic is Basic)
// 								{
// 									xmlCtrl.findSkinAndSelect(m_skinName, (BoloUI.Basic)m_iAttrRow.m_parent.m_basic);
// 								}
// 								else
// 								{
// 									xmlCtrl.findSkinAndSelect(m_skinName);
// 								}
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
