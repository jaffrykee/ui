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

namespace UIEditor.Project
{
	/// <summary>
	/// ReloadConfirm.xaml 的交互逻辑
	/// </summary>
	public partial class ReloadConfirm
	{
		public string m_path;

		public ReloadConfirm(string path)
		{
			m_path = path;

			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			mx_fileName.Text = path;
		}

		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile fileDef;

			if (MainWindow.s_pW.m_mapOpenedFiles != null &&
				MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(m_path, out fileDef) &&
				fileDef != null &&
				fileDef.m_tabItem != null)
			{
				fileDef.m_tabItem.closeFile(true);
				if (System.IO.File.Exists(m_path))
				{
					MainWindow.s_pW.openFileByPath(m_path);
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
