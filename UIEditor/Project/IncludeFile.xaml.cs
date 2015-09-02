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
	/// ProjFile.xaml 的交互逻辑
	/// </summary>
	public partial class IncludeFile : TreeViewItem
	{
		public Project m_parent;
		public string m_path;

		public IncludeFile(string path)
		{
			m_path = path;
			InitializeComponent();
			mx_root.ToolTip = m_path;
			mx_radio.Content = System.IO.Path.GetFileName(m_path);
		}

		private void mx_radio_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			mx_root.IsExpanded = !(mx_root.IsExpanded);
			if (e.ChangedButton == MouseButton.Left && File.Exists(m_path))
			{
				MainWindow.s_pW.openFileByPath(m_path);
			}
		}
		private void mx_moveToChild_Click(object sender, RoutedEventArgs e)
		{
			//todo
		}
		private void mx_copyToChild_Click(object sender, RoutedEventArgs e)
		{
			//todo
		}
		private static void addResFolderToMenu(DirectoryInfo imgDri, MenuItem menuItem, RoutedEventHandler clickEvent)
		{
			foreach(DirectoryInfo dri in imgDri.GetDirectories())
			{
				MenuItem childItem = new MenuItem();

				childItem.Header = dri.Name;
				childItem.ToolTip = dri.FullName;
				childItem.Click += clickEvent;
				menuItem.Items.Add(childItem);
			}
		}
		private void mx_menu_Loaded(object sender, RoutedEventArgs e)
		{
			if(File.Exists(m_path))
			{
				FileInfo fi = new FileInfo(m_path);

				try
				{
					if (fi.Directory.Parent.Name == "images" &&
						fi.Directory.Parent.Parent.FullName == MainWindow.s_pW.m_projPath)
					{
						mx_moveTo.Visibility = System.Windows.Visibility.Visible;
						mx_copyTo.Visibility = System.Windows.Visibility.Visible;

						addResFolderToMenu(fi.Directory.Parent, mx_moveTo, mx_moveToChild_Click);
						addResFolderToMenu(fi.Directory.Parent, mx_copyTo, mx_copyToChild_Click);

						return;
					}
				}
				catch
				{

				}
			}
			mx_moveTo.Visibility = System.Windows.Visibility.Collapsed;
			mx_copyTo.Visibility = System.Windows.Visibility.Collapsed;
		}
		private void mx_newFile_Click(object sender, RoutedEventArgs e)
		{
			NewFileWin winNewFile = new NewFileWin(".\\data\\Template\\");
			winNewFile.ShowDialog();
		}
	}
}
