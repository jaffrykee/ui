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
	/// ProjectProject.Setting.xaml 的交互逻辑
	/// </summary>
	public partial class ProjectSettingWin
	{
		static public ProjectSettingWin s_pW;

		public XmlDocument m_docXml;
		public XmlElement m_xeResolutionDef;
		public XmlElement m_xeThemeDef;
		public Dictionary<ListBoxItem, XmlElement> m_mapLbiResolutionRow;
		public Dictionary<ListBoxItem, XmlElement> m_mapLbiThemeRow;

		public ProjectSettingWin()
		{
			s_pW = this;
			m_docXml = new XmlDocument();
			m_docXml.LoadXml(Project.Setting.s_docProj.OuterXml);
			m_mapLbiResolutionRow = new Dictionary<ListBoxItem, XmlElement>();
			m_mapLbiThemeRow = new Dictionary<ListBoxItem, XmlElement>();

			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			refreshResolution();
			refreshSetting("ThemeSetting", mx_lbTheme, m_mapLbiThemeRow, new string[0], ref m_xeThemeDef);
			refreshPathTextBox();
		}

		void refreshPathTextBox()
		{
			mx_tbPackUI.Text = Setting.s_uiPackPath;
			mx_tbPackScript.Text = Setting.s_scriptPackPath;
			mx_tbGame.Text = Setting.s_gamePath;
			mx_tbParticle.Text = Setting.s_particlePath;
			mx_tbLang.Text = Setting.s_langPath;
			mx_tbBackground.Text = Setting.s_backgroundPath;
		}
		void refreshResolution()
		{
			if (m_docXml != null && m_docXml.DocumentElement != null)
			{
				XmlNode xnResolutionSetting = m_docXml.DocumentElement.SelectSingleNode("ResolutionSetting");

				if (xnResolutionSetting == null)
				{
					xnResolutionSetting = Project.Setting.initResolutionSetting(m_docXml.DocumentElement);
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
									m_xeResolutionDef = xeNode;
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
							m_mapLbiResolutionRow.Add(lbiRow, xeNode);
						}
					}
				}
			}
		}

		private void mx_lbResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null)
			{
				mx_delResolutionRow.IsEnabled = true;
			}
			else
			{
				mx_delResolutionRow.IsEnabled = false;
			}
		}
		private void mx_addResolutionRow_Click(object sender, RoutedEventArgs e)
		{
			XmlNode xnRst = m_docXml.DocumentElement.SelectSingleNode("ResolutionSetting");

			if(xnRst != null)
			{
				XmlElement xeRow = m_docXml.CreateElement("row");

				xnRst.AppendChild(xeRow);
				if(m_mapLbiResolutionRow != null)
				{
					ListBoxItem lbiRow = new ListBoxItem();
					SetResolutionWin winSetRlt;

					m_mapLbiResolutionRow.Add(lbiRow, xeRow);
					mx_lbResolution.Items.Add(lbiRow);
					winSetRlt = new SetResolutionWin(lbiRow);
					winSetRlt.ShowDialog();

					if(lbiRow != null && (lbiRow.Content == null || lbiRow.Content.ToString() == ""))
					{
						delResolutionRow(lbiRow);
					}
				}
			}
		}
		static public void delResolutionRow(ListBoxItem lbiRow)
		{
			XmlElement xe;

			if (lbiRow != null && ProjectSettingWin.s_pW.m_mapLbiResolutionRow.TryGetValue(lbiRow, out xe) && xe != null)
			{
				xe.ParentNode.RemoveChild(xe);
				if (lbiRow.Parent == ProjectSettingWin.s_pW.mx_lbResolution)
				{
					ProjectSettingWin.s_pW.mx_lbResolution.Items.Remove(lbiRow);
				}
				ProjectSettingWin.s_pW.m_mapLbiResolutionRow.Remove(lbiRow);
			}
		}
		private void mx_delResolutionRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				delResolutionRow((ListBoxItem)mx_lbResolution.SelectedItem);
			}
		}
		private void mx_updResolutionRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbResolution.SelectedItem;
				SetResolutionWin winSetRlt = new SetResolutionWin(lbi, false);

				winSetRlt.ShowDialog();
			}
		}
		private void mx_setResolutionDefault_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbResolution.SelectedItem != null && mx_lbResolution.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbResolution.SelectedItem;
				XmlElement xe;

				if (m_mapLbiResolutionRow.TryGetValue(lbi, out xe))
				{
					if (m_xeResolutionDef != null)
					{
						m_xeResolutionDef.RemoveAttribute("isDefault");
					}
					xe.SetAttribute("isDefault", "true");
					m_xeResolutionDef = xe;
					refreshResolution();
				}
			}
		}

		private void setPathSetting()
		{
			if(m_docXml != null)
			{
				XmlNode xnPathSetting = m_docXml.DocumentElement.SelectSingleNode("PathSetting");
				XmlElement xePathSetting;

				if(xnPathSetting != null && xnPathSetting is XmlElement)
				{
					xePathSetting = (XmlElement)xnPathSetting;
				}
				else
				{
					xePathSetting = m_docXml.CreateElement("PathSetting");
				}
				xePathSetting.SetAttribute("uiPackPath", mx_tbPackUI.Text);
				xePathSetting.SetAttribute("scriptPackPath", mx_tbPackScript.Text);
				xePathSetting.SetAttribute("gamePath", mx_tbGame.Text);
				xePathSetting.SetAttribute("particlePath", mx_tbParticle.Text);
				xePathSetting.SetAttribute("langPath", mx_tbLang.Text);
				xePathSetting.SetAttribute("backgroundPath", mx_tbBackground.Text);
			}
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			setPathSetting();
			Project.Setting.s_docProj = m_docXml;
			Setting.refreshAllProjectSetting();

			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void openFileBoxAndSetTextBox(TextBox tb, string filter = null, string defPath = null)
		{
			string path = Setting.openSelectFileBox(filter, defPath);

			if (path != null)
			{
				tb.Text = path;
			}
		}
		private void openFolderBoxAndSetTextBox(TextBox tb)
		{
			string path = Setting.openSelectFolderBox();

			if (path != null)
			{
				tb.Text = path;
			}
		}
		private void mx_btnPackUI_Click(object sender, RoutedEventArgs e)
		{
			openFileBoxAndSetTextBox(mx_tbPackUI);
		}
		private void mx_btnPackScript_Click(object sender, RoutedEventArgs e)
		{
			openFileBoxAndSetTextBox(mx_tbPackScript);
		}
		private void mx_btnGame_Click(object sender, RoutedEventArgs e)
		{
			openFileBoxAndSetTextBox(mx_tbGame);
		}
		private void mx_btnParticle_Click(object sender, RoutedEventArgs e)
		{
			openFolderBoxAndSetTextBox(mx_tbParticle);
		}
		private void mx_btnLang_Click(object sender, RoutedEventArgs e)
		{
			openFileBoxAndSetTextBox(mx_tbLang, "language文件|Language.xml");
		}
		private void mx_btnBackground_Click(object sender, RoutedEventArgs e)
		{
			openFileBoxAndSetTextBox(mx_tbBackground, "所有图片文件|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff;*.tga");
		}


		void refreshSetting(string settingName, ListBox lbSetting, Dictionary<ListBoxItem, XmlElement> mapLbiRow,
			string[] arrInitValue, ref XmlElement xeDef)
		{
			if (m_docXml != null && m_docXml.DocumentElement != null)
			{
				XmlNode xnSetting = m_docXml.DocumentElement.SelectSingleNode(settingName);

				if (xnSetting == null)
				{
					xnSetting = Project.Setting.initSetting(m_docXml.DocumentElement, settingName, arrInitValue);
				}

				if (xnSetting != null)
				{
					bool isDefaultFound = false;

					lbSetting.Items.Clear();
					foreach (XmlNode xnNode in xnSetting.ChildNodes)
					{
						if (xnNode is XmlElement && xnNode.Name == "row")
						{
							string strShow = xnNode.InnerXml;
							string strTip = xnNode.InnerXml;
							XmlElement xeNode = (XmlElement)xnNode;

							if (xeNode.GetAttribute("isDefault") == "true")
							{
								if (isDefaultFound == false)
								{
									xeDef = xeNode;
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
							lbSetting.Items.Add(lbiRow);
							mapLbiRow.Add(lbiRow, xeNode);
						}
					}
				}
			}
		}
		static public void delSettingRow(ListBoxItem lbiRow, Dictionary<ListBoxItem, XmlElement> mapLbiRow)
		{
			XmlElement xe;

			if (lbiRow != null && mapLbiRow.TryGetValue(lbiRow, out xe) && xe != null)
			{
				xe.ParentNode.RemoveChild(xe);
				if (lbiRow.Parent != null && lbiRow.Parent is ListBox)
				{
					ListBox lbFrame = (ListBox)lbiRow.Parent;

					lbFrame.Items.Remove(lbiRow);
				}
				mapLbiRow.Remove(lbiRow);
			}
		}
		public void setSettingRowDefault(ListBoxItem lbiRow, Dictionary<ListBoxItem, XmlElement> mapLbiRow, ref XmlElement xeDef)
		{
			XmlElement xe;

			if (mapLbiRow.TryGetValue(lbiRow, out xe) && xe != null)
			{
				if (xeDef != null)
				{
					xeDef.RemoveAttribute("isDefault");
				}
				xe.SetAttribute("isDefault", "true");
				xeDef = xe;
				refreshSetting("ThemeSetting", mx_lbTheme, m_mapLbiThemeRow, new string[0], ref m_xeThemeDef);
			}
		}

		private void mx_lbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(mx_lbTheme.SelectedItem != null)
			{
				mx_delThemeRow.IsEnabled = true;
			}
			else
			{
				mx_delThemeRow.IsEnabled = false;
			}
		}
		private void mx_addThemeRow_Click(object sender, RoutedEventArgs e)
		{
			XmlNode xnTheme = m_docXml.DocumentElement.SelectSingleNode("ThemeSetting");

			if (xnTheme != null)
			{
				XmlElement xeRow = m_docXml.CreateElement("row");

				xnTheme.AppendChild(xeRow);
				if (m_mapLbiThemeRow != null)
				{
					ListBoxItem lbiRow = new ListBoxItem();
					SetSettingRowWin winSetRow;

					m_mapLbiThemeRow.Add(lbiRow, xeRow);
					mx_lbTheme.Items.Add(lbiRow);
					winSetRow = new SetSettingRowWin(lbiRow, m_mapLbiThemeRow);
					winSetRow.ShowDialog();

					if (lbiRow != null && (lbiRow.Content == null || lbiRow.Content.ToString() == ""))
					{
						delSettingRow(lbiRow, m_mapLbiThemeRow);
					}
				}
			}
		}
		private void mx_delThemeRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbTheme.SelectedItem != null && mx_lbTheme.SelectedItem is ListBoxItem)
			{
				delSettingRow((ListBoxItem)mx_lbTheme.SelectedItem, m_mapLbiThemeRow);
			}
		}
		private void mx_updThemeRow_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbTheme.SelectedItem != null && mx_lbTheme.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbTheme.SelectedItem;
				SetSettingRowWin winSetRow = new SetSettingRowWin(lbi, m_mapLbiThemeRow, false);

				winSetRow.ShowDialog();
			}
		}
		private void mx_setThemeDefault_Click(object sender, RoutedEventArgs e)
		{
			if (mx_lbTheme.SelectedItem != null && mx_lbTheme.SelectedItem is ListBoxItem)
			{
				ListBoxItem lbi = (ListBoxItem)mx_lbTheme.SelectedItem;

				setSettingRowDefault(lbi, m_mapLbiThemeRow, ref m_xeThemeDef);
			}
		}

		private void mx_lbTemplateGlobal_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void mx_renameTemplateRow_Click(object sender, RoutedEventArgs e)
		{

		}

		private void mx_delTemplateRow_Click(object sender, RoutedEventArgs e)
		{

		}

		private void mx_lbTemplateProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
	}
}
