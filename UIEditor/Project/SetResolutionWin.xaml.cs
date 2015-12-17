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
using System.Text.RegularExpressions;
using System.Xml;

namespace UIEditor.Project
{
	/// <summary>
	/// SetResolutionWin.xaml 的交互逻辑
	/// </summary>
	public partial class SetResolutionWin
	{
		public ListBoxItem m_lbiRow;
		public bool m_isAdd;
		public SetResolutionWin(ListBoxItem lbiRow, bool isAdd = true)
		{
			InitializeComponent();
			this.Owner = ProjectSettingWin.s_pW;
			m_lbiRow = lbiRow;
			m_isAdd = isAdd;
		}

		private void mx_tbWidth_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(mx_tbWidth.Text.ToString() != "" && mx_tbHeight.Text.ToString() != "")
			{
				mx_ok.IsEnabled = true;
			}
			else
			{
				mx_ok.IsEnabled = false;
			}
		}
		private void mx_tbHeight_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (mx_tbWidth.Text.ToString() != "" && mx_tbHeight.Text.ToString() != "")
			{
				mx_ok.IsEnabled = true;
			}
			else
			{
				mx_ok.IsEnabled = false;
			}
		}
		private void mx_tbWidth_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex re = new Regex("[^0-9.-]+");
			e.Handled = re.IsMatch(e.Text);
		}
		private void mx_tbHeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex re = new Regex("[^0-9.-]+");
			e.Handled = re.IsMatch(e.Text);
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if(ProjectSettingWin.s_pW != null && ProjectSettingWin.s_pW.m_mapLbiResolutionRow != null)
			{
				XmlElement xeRow;

				if(ProjectSettingWin.s_pW.m_mapLbiResolutionRow.TryGetValue(m_lbiRow, out xeRow) && xeRow != null)
				{
					int w, h;

					if (int.TryParse(mx_tbWidth.Text.ToString(), out w) &&
						int.TryParse(mx_tbHeight.Text.ToString(), out h))
					{
						xeRow.InnerXml = w.ToString() + " x " + h.ToString();
						m_lbiRow.Content = xeRow.InnerXml;
						m_lbiRow.ToolTip = xeRow.InnerXml;
					}
				}
			}

			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			if (m_isAdd)
			{
				ProjectSettingWin.delResolutionRow(m_lbiRow);
			}

			this.Close();
		}
	}
}
