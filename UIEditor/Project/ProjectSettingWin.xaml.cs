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

		public ProjectSettingWin(XmlDocument docXml)
		{
			s_pW = this;
			m_docXml = new XmlDocument();
			m_docXml.LoadXml(docXml.OuterXml);
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

		}
		private void mx_delRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbResolution.SelectedItem;
				XmlElement xe;

				if(m_mapLbiRow.TryGetValue(lbi, out xe))
				{
					xe.ParentNode.RemoveChild(xe);
					mx_lbResolution.Items.Remove(lbi);
					m_mapLbiRow.Remove(lbi);
				}
			}
		}
		private void mx_updRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbResolution.SelectedItem;
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
	}
}
