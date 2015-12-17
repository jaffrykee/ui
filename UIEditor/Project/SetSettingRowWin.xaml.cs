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
	/// SetSettingRowWin.xaml 的交互逻辑
	/// </summary>
	public partial class SetSettingRowWin
	{
		public ListBoxItem m_lbiRow;
		public bool m_isAdd;
		public Dictionary<ListBoxItem, XmlElement> m_mapLbiRow;
		public SetSettingRowWin(ListBoxItem lbiRow, Dictionary<ListBoxItem, XmlElement> mapLbiRow, bool isAdd = true)
		{
			InitializeComponent();
			this.Owner = ProjectSettingWin.s_pW;
			m_lbiRow = lbiRow;
			m_mapLbiRow = mapLbiRow;
			m_isAdd = isAdd;
		}

		private void mx_tbValue_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (mx_tbValue.Text.ToString() != "")
			{
				mx_ok.IsEnabled = true;
			}
			else
			{
				mx_ok.IsEnabled = false;
			}
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if (m_mapLbiRow != null)
			{
				XmlElement xeRow;

				if (m_mapLbiRow.TryGetValue(m_lbiRow, out xeRow) && xeRow != null)
				{
					xeRow.InnerXml = mx_tbValue.Text.ToString();
					m_lbiRow.Content = xeRow.InnerXml;
					m_lbiRow.ToolTip = xeRow.InnerXml;
				}
			}

			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			if (m_isAdd)
			{
				ProjectSettingWin.delSettingRow(m_lbiRow, m_mapLbiRow);
			}

			this.Close();
		}
	}
}
