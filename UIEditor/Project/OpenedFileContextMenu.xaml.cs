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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace UIEditor.Project
{
	/// <summary>
	/// OpenedFileContextMenu.xaml 的交互逻辑
	/// </summary>
	public partial class OpenedFileContextMenu : UserControl
	{
		public OpenedFileContextMenu()
		{
			InitializeComponent();
		}

		private void mx_close_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile curFileDef = OpenedFile.getCurFileDef();

			if(curFileDef != null)
			{
				curFileDef.m_tabItem.closeFile();
			}
		}
		private void mx_closeAll_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile.closeAllFile();
		}
		private void mx_closeAllExSelf_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile.closeAllFile(OpenedFile.getCurFileDef());
		}
		private void mx_openFolder_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile curFileDef = OpenedFile.getCurFileDef();

			if(curFileDef != null)
			{
				OpenedFile.openLocalFolder(curFileDef.m_path);
			}
		}
	}
}
