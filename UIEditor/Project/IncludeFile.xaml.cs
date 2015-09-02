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

			IncludeFile fileDef;
			if (!MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(m_path, out fileDef))
			{
				MainWindow.s_pW.m_mapIncludeFiles.Add(m_path, this);
			}
			else
			{
				MainWindow.s_pW.m_mapIncludeFiles[m_path] = this;
			}
			InitializeComponent();
			mx_root.ToolTip = m_path;
			mx_radio.Content = System.IO.Path.GetFileName(m_path);
		}

		public void deleteSelf()
		{
			IncludeFile fileDef;

			if (System.IO.File.Exists(m_path))
			{
				try
				{
					System.IO.File.Delete(m_path);
				}
				catch
				{
				}
			}
			if(MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(m_path, out fileDef))
			{
				MainWindow.s_pW.m_mapIncludeFiles.Remove(m_path);
			}
			if (this.Parent != null &&
				(this.Parent.GetType().ToString() == "System.Windows.Controls.TreeViewItem" ||
				this.Parent.GetType().BaseType.ToString() == "System.Windows.Controls.TreeViewItem"))
			{
				IncludeFile pItem = (IncludeFile)this.Parent;

				pItem.Items.Remove(this);
			}
		}
		private void mx_radio_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			mx_root.IsExpanded = !(mx_root.IsExpanded);
			if (e.ChangedButton == MouseButton.Left && File.Exists(m_path))
			{
				MainWindow.s_pW.openFileByPath(m_path);
			}
		}
		private void pngFileDeal(object pngItem, string type)
		{
			if(type != "delete")
			{
				if (pngItem.GetType().ToString() == "System.Windows.Controls.MenuItem")
				{
					MenuItem clickItem = (MenuItem)pngItem;
					string newFolder = clickItem.ToolTip.ToString();

					if (System.IO.Directory.Exists(newFolder))
					{
						string newPath = newFolder + "\\" + System.IO.Path.GetFileName(m_path);

						if (!System.IO.File.Exists(newPath))
						{
							switch(type)
							{
								case "moveTo":
									{
										if (System.IO.File.Exists(m_path))
										{
											try
											{
												System.IO.File.Move(m_path, newPath);
											}
											catch
											{
												break;
											}
											deleteSelf();

											IncludeFile newFolderDef;

											if (MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(System.IO.Path.GetDirectoryName(newPath), out newFolderDef))
											{
												newFolderDef.AddChild(new IncludeFile(newPath));
											}
										}
									}
									break;
								case "copyTo":
									{
										if (System.IO.File.Exists(m_path))
										{
											try
											{
												System.IO.File.Copy(m_path, newPath);
											}
											catch
											{
												break;
											}

											IncludeFile newFolderDef;

											if (MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(System.IO.Path.GetDirectoryName(newPath), out newFolderDef))
											{
												newFolderDef.AddChild(new IncludeFile(newPath));
											}
										}
									}
									break;
								default:
									break;
							}
						}
						else
						{
							MessageBox.Show("新路径已有同名文件。", "文件名冲突", MessageBoxButton.OK, MessageBoxImage.Error);
						}
					}
				}
			}
			else
			{
				deleteSelf();
			}
		}
		private void mx_moveToChild_Click(object sender, RoutedEventArgs e)
		{
			pngFileDeal(sender, "moveTo");
		}
		private void mx_copyToChild_Click(object sender, RoutedEventArgs e)
		{
			pngFileDeal(sender, "copyTo");
		}
		private static void addResFolderToMenu(DirectoryInfo imgDri, MenuItem menuItem, RoutedEventHandler clickEvent)
		{
			menuItem.Items.Clear();
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
