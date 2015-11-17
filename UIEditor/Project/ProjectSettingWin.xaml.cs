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

namespace UIEditor.Project
{
	/// <summary>
	/// ProjectSetting.xaml 的交互逻辑
	/// </summary>
	public partial class ProjectSettingWin
	{
		static public ProjectSettingWin s_pW;

		public XmlDocument m_docXml;
		public XmlElement m_xeDef;
		public Dictionary<ListBoxItem, XmlElement> m_mapLbiRow;

		public ProjectSettingWin()
		{
			s_pW = this;
			m_docXml = new XmlDocument();
			m_docXml.LoadXml(MainWindow.s_pW.m_docProj.OuterXml);
			m_mapLbiRow = new Dictionary<ListBoxItem, XmlElement>();

			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			refreshResolution();
		}

		void refreshResolution()
		{
			if (m_docXml != null && m_docXml.DocumentElement != null)
			{
				XmlNode xnResolutionSetting = m_docXml.DocumentElement.SelectSingleNode("ResolutionSetting");

				if (xnResolutionSetting == null)
				{
					xnResolutionSetting = Setting.initResolutionSetting(m_docXml.DocumentElement);
				}

				if(xnResolutionSetting != null)
				{
					bool isDefaultFound = false;

					mx_lbResolution.Items.Clear();
					foreach(XmlNode xnNode in xnResolutionSetting.ChildNodes)
					{
						if (xnNode is XmlElement && xnNode.Name == "row")
						{
							string strShow = xnNode.InnerXml;
							string strTip = xnNode.InnerXml;
							XmlElement xeNode = (XmlElement)xnNode;

							if(xeNode.GetAttribute("isDefault") == "true")
							{
								if (isDefaultFound == false)
								{
									m_xeDef = xeNode;
									isDefaultFound = true;
									strShow += " (默认)";
								}
								else
								{
									xeNode.RemoveAttribute("isDefault");
								}
							}

							ListBoxItem lbiRow = new ListBoxItem();

							lbiRow.Content = strShow;
							lbiRow.ToolTip = strTip;
							lbiRow.Margin = new Thickness(0);
							lbiRow.Padding = new Thickness(0);
							mx_lbResolution.Items.Add(lbiRow);
							m_mapLbiRow.Add(lbiRow, xeNode);
						}
					}
				}
			}
		}

		private void mx_lbResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null)
			{
				mx_delRow.IsEnabled = true;
			}
			else
			{
				mx_delRow.IsEnabled = false;
			}
		}
		private void mx_addRow_Click(object sender, RoutedEventArgs e)
		{
			XmlNode xnRst = m_docXml.DocumentElement.SelectSingleNode("ResolutionSetting");

			if(xnRst != null)
			{
				XmlElement xeRow = m_docXml.CreateElement("row");

				xnRst.AppendChild(xeRow);
				if(m_mapLbiRow != null)
				{
					ListBoxItem lbiRow = new ListBoxItem();
					SetResolutionWin winSetRlt;

					m_mapLbiRow.Add(lbiRow, xeRow);
					mx_lbResolution.Items.Add(lbiRow);
					winSetRlt = new SetResolutionWin(lbiRow);
					winSetRlt.ShowDialog();

					if(lbiRow != null && (lbiRow.Content == null || lbiRow.Content.ToString() == ""))
					{
						delRow(lbiRow);
					}
				}
			}
		}
		static public void delRow(ListBoxItem lbiRow)
		{
			XmlElement xe;

			if (lbiRow != null && ProjectSettingWin.s_pW.m_mapLbiRow.TryGetValue(lbiRow, out xe) && xe != null)
			{
				xe.ParentNode.RemoveChild(xe);
				if (lbiRow.Parent == ProjectSettingWin.s_pW.mx_lbResolution)
				{
					ProjectSettingWin.s_pW.mx_lbResolution.Items.Remove(lbiRow);
				}
				ProjectSettingWin.s_pW.m_mapLbiRow.Remove(lbiRow);
			}
		}
		private void mx_delRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				delRow((ListBoxItem)mx_lbResolution.SelectedItem);
			}
		}
		private void mx_updRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbResolution.SelectedItem;
				SetResolutionWin winSetRlt = new SetResolutionWin(lbi, false);

				winSetRlt.ShowDialog();
			}
		}
		private void mx_setDefault_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbResolution.SelectedItem;
				XmlElement xe;

				if (m_mapLbiRow.TryGetValue(lbi, out xe))
				{
					m_xeDef.RemoveAttribute("isDefault");
					xe.SetAttribute("isDefault", "true");
					m_xeDef = xe;
					refreshResolution();
				}
			}
		}

		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			m_docXml.Save(MainWindow.s_pW.m_projPath + "\\" + MainWindow.s_pW.m_projName);
			MainWindow.s_pW.m_docProj = m_docXml;
			Setting.refreshResolutionBoxByConfigNode(MainWindow.s_pW.m_docProj.DocumentElement.SelectSingleNode("ResolutionSetting"));

			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
