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
using System.Xml;
using Microsoft.VisualBasic.FileIO;

namespace UIEditor.Project
{
	/// <summary>
	/// ProjFile.xaml 的交互逻辑
	/// </summary>
	
	public enum FileType
	{
		Normal,
		BoloUI_Ctrl,
		BoloUI_Skin,
		Image_Png,
		Image_Folder
	}
	public partial class IncludeFile : TreeViewItem
	{
		public string m_path;
		public FileType m_fileType;

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
			mx_radio.Content = "_" + System.IO.Path.GetFileName(m_path);
		}

		public void deleteSelf()
		{
			IncludeFile fileDef;

			if (System.IO.File.Exists(m_path))
			{
				try
				{
					if (m_fileType == FileType.BoloUI_Ctrl || m_fileType == FileType.BoloUI_Skin)
					{
						FileSystem.DeleteFile(m_path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
					}
					else
					{
						File.Delete(m_path);
					}
				}
				catch
				{
				}
			}
			if(MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(m_path, out fileDef))
			{
				MainWindow.s_pW.m_mapIncludeFiles.Remove(m_path);
			}

			OpenedFile openFileDef;

			if (MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(m_path, out openFileDef) && openFileDef.m_tabItem != null)
			{
				openFileDef.m_tabItem.closeFile();
			}
			if (this.Parent != null &&
				(this.Parent is TreeViewItem ||
				this.Parent.GetType().BaseType.ToString() == "System.Windows.Controls.TreeViewItem"))
			{
				TreeViewItem pItem = (TreeViewItem)this.Parent;

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
		private void moveFile(string oldPath, string newPath)
		{
			string newFolder = System.IO.Path.GetDirectoryName(newPath);

			if (System.IO.File.Exists(oldPath))
			{
				try
				{
					System.IO.File.Move(oldPath, newPath);
				}
				catch
				{
					Public.ResultLink.createResult("\r\n移动(从" + oldPath + "，到" + newPath + ")失败。", Public.ResultType.RT_ERROR);

					return;
				}
				if (m_fileType == FileType.Image_Png)
				{
					deleteSelf();
					IncludeFile newFolderDef;

					if (MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(System.IO.Path.GetDirectoryName(newPath), out newFolderDef))
					{
						newFolderDef.AddChild(new IncludeFile(newPath));
					}

					FileInfo fi = new FileInfo(oldPath);

					//重新打包
					ImageTools.ImageNesting.pngToTgaRectNesting(fi.DirectoryName);
					if (fi.DirectoryName != newFolder)
					{
						ImageTools.ImageNesting.pngToTgaRectNesting(newFolder);
					}
					//刷新皮肤的引用
					ImageTools.ImageNesting.moveImageLink(
						System.IO.Path.GetFileNameWithoutExtension(oldPath),
						System.IO.Path.GetFileNameWithoutExtension(newPath),
						System.IO.Path.GetFileName(fi.DirectoryName),
						System.IO.Path.GetFileName(newFolder));
				}
				else
				{
					if (this.Parent != null &&
						(this.Parent is TreeViewItem ||
						this.Parent.GetType().BaseType.ToString() == "System.Windows.Controls.TreeViewItem"))
					{
						TreeViewItem pItem = (TreeViewItem)this.Parent;

						pItem.Items.Add(new IncludeFile(newPath));
					}
					deleteSelf();
				}
			}
			else if (System.IO.Directory.Exists(oldPath))
			{
				try
				{
					System.IO.Directory.Move(oldPath, newPath);
				}
				catch
				{
					Public.ResultLink.createResult("\r\n移动(从" + oldPath + "，到" + newPath + ")失败。", Public.ResultType.RT_ERROR);

					return;
				}
				if (this.Parent != null &&
					(this.Parent is TreeViewItem ||
					this.Parent.GetType().BaseType.ToString() == "System.Windows.Controls.TreeViewItem"))
				{
					TreeViewItem pItem = (TreeViewItem)this.Parent;

					pItem.Items.Add(new IncludeFile(newPath));
				}
				deleteSelf();
			}
		}
		private void pngFileDeal(object pngItem, string type)
		{
			switch(type)
			{
				case "delete":
					{
						deleteSelf();
						if (m_fileType == FileType.Image_Png)
						{
							FileInfo fi = new FileInfo(m_path);
							string oldResPath = Project.Setting.s_projPath + "\\images\\" + System.IO.Path.GetFileName(fi.DirectoryName) + ".xml";

							//重新打包
							ImageTools.ImageNesting.pngToTgaRectNesting(fi.DirectoryName);
						}
					}
					break;
				case "rename":
					{
						string oldFileName = mx_radio.Content.ToString();

						mx_tbRename.Visibility = System.Windows.Visibility.Visible;
						mx_radio.Visibility = System.Windows.Visibility.Collapsed;
						if (oldFileName.IndexOf('_') == 0)
						{
							mx_tbRename.Text = oldFileName.Substring(1, oldFileName.Length - 1);
						}
						else
						{
							mx_tbRename.Text = oldFileName;
						}
						mx_tbRename.Focus();
					}
					break;
				default:
					{
						if (pngItem is MenuItem)
						{
							MenuItem clickItem = (MenuItem)pngItem;
							string newFolder = clickItem.ToolTip.ToString();

							if (System.IO.Directory.Exists(newFolder))
							{
								string newPath = newFolder + "\\" + System.IO.Path.GetFileName(m_path);

								if (!System.IO.File.Exists(newPath))
								{
									switch (type)
									{
										case "moveTo":
											{
												moveFile(m_path, newPath);
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
														Public.ResultLink.createResult("\r\n拷贝(从" + m_path + "，到" + newPath + ")失败。",
															Public.ResultType.RT_ERROR);

														return;
													}

													IncludeFile newFolderDef;

													if (MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(System.IO.Path.GetDirectoryName(newPath), out newFolderDef))
													{
														newFolderDef.AddChild(new IncludeFile(newPath));
													}
													string newResPath = Project.Setting.s_projPath + "\\images\\" + System.IO.Path.GetFileName(newFolder) + ".xml";

													//重新打包
													ImageTools.ImageNesting.pngToTgaRectNesting(newFolder);
												}
											}
											break;
										default:
											return;
									}
								}
								else
								{
									MessageBox.Show("新路径已有同名文件。", "文件名冲突", MessageBoxButton.OK, MessageBoxImage.Error);
									return;
								}
							}
						}
					}
					break;
			}
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
		public void refreshMenuItem(FileType fileType)
		{
			m_fileType = fileType;
			switch(fileType)
			{
				case FileType.Image_Png:
					{
						mx_moveTo.Visibility = System.Windows.Visibility.Visible;
						mx_copyTo.Visibility = System.Windows.Visibility.Visible;
						mx_delete.Visibility = System.Windows.Visibility.Visible;
						mx_rename.Visibility = System.Windows.Visibility.Visible;
						mx_spImg.Visibility = System.Windows.Visibility.Visible;
						mx_repackImage.Visibility = System.Windows.Visibility.Collapsed;
					}
					break;
				case FileType.BoloUI_Ctrl:
					{
						mx_moveTo.Visibility = System.Windows.Visibility.Collapsed;
						mx_copyTo.Visibility = System.Windows.Visibility.Collapsed;
						mx_delete.Visibility = System.Windows.Visibility.Visible;
						mx_rename.Visibility = System.Windows.Visibility.Visible;
						mx_spImg.Visibility = System.Windows.Visibility.Visible;
						mx_repackImage.Visibility = System.Windows.Visibility.Collapsed;
					}
					break;
				case FileType.Image_Folder:
					{
						mx_moveTo.Visibility = System.Windows.Visibility.Collapsed;
						mx_copyTo.Visibility = System.Windows.Visibility.Collapsed;
						mx_delete.Visibility = System.Windows.Visibility.Collapsed;
						mx_rename.Visibility = System.Windows.Visibility.Collapsed;
						mx_spImg.Visibility = System.Windows.Visibility.Collapsed;
						mx_repackImage.Visibility = System.Windows.Visibility.Visible;
					}
					break;
				default:
					{
						mx_moveTo.Visibility = System.Windows.Visibility.Collapsed;
						mx_copyTo.Visibility = System.Windows.Visibility.Collapsed;
						mx_delete.Visibility = System.Windows.Visibility.Collapsed;
						mx_rename.Visibility = System.Windows.Visibility.Collapsed;
						mx_spImg.Visibility = System.Windows.Visibility.Collapsed;
						mx_repackImage.Visibility = System.Windows.Visibility.Collapsed;
					}
					break;
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
						fi.Directory.Parent.Parent.FullName == Project.Setting.s_projPath)
					{
						refreshMenuItem(FileType.Image_Png);
						addResFolderToMenu(fi.Directory.Parent, mx_moveTo, mx_moveToChild_Click);
						addResFolderToMenu(fi.Directory.Parent, mx_copyTo, mx_copyToChild_Click);

						return;
					}
					else if (fi.Directory.FullName == Project.Setting.s_projPath)
					{
						if(fi.Extension == ".xml")
						{
							XmlDocument docXml = new XmlDocument();

							docXml.Load(m_path);
							if(docXml.DocumentElement.Name == "BoloUI")
							{
								refreshMenuItem(FileType.BoloUI_Ctrl);
								return;
							}
						}
					}
					else if (fi.Directory.Name == "skin" &&
						fi.Directory.Parent.FullName == Project.Setting.s_projPath)
					{
						if (fi.Extension == ".xml")
						{
							XmlDocument docXml = new XmlDocument();

							docXml.Load(m_path);
							if (docXml.DocumentElement.Name == "BoloUI")
							{
								refreshMenuItem(FileType.BoloUI_Skin);
								return;
							}
						}
					}
				}
				catch
				{

				}
			}
			else if (Directory.Exists(m_path))
			{
				DirectoryInfo dri = new DirectoryInfo(m_path);

				try
				{
					if(dri.Parent.Name == "images" &&
						dri.Parent.Parent.FullName == Project.Setting.s_projPath)
					{
						refreshMenuItem(FileType.Image_Folder);
						return;
					}
				}
				catch
				{

				}
			}
			refreshMenuItem(FileType.Normal);
		}
		private void mx_newFile_Click(object sender, RoutedEventArgs e)
		{
			NewFileWin winNewFile = new NewFileWin(".\\data\\Template\\");
			winNewFile.ShowDialog();
		}
		private void mx_moveToChild_Click(object sender, RoutedEventArgs e)
		{
			pngFileDeal(sender, "moveTo");
		}
		private void mx_copyToChild_Click(object sender, RoutedEventArgs e)
		{
			pngFileDeal(sender, "copyTo");
		}
		private void mx_delete_Click(object sender, RoutedEventArgs e)
		{
			pngFileDeal(sender, "delete");
		}
		private void mx_rename_Click(object sender, RoutedEventArgs e)
		{
			pngFileDeal(sender, "rename");
		}

		private void mx_tbRename_LostFocus(object sender, RoutedEventArgs e)
		{
			if(mx_tbRename.Text != mx_radio.Content.ToString())
			{
				string newPath = System.IO.Path.GetDirectoryName(m_path) + "\\" + mx_tbRename.Text;

				moveFile(m_path, newPath);
			}
			mx_radio.Visibility = System.Windows.Visibility.Visible;
			mx_tbRename.Visibility = System.Windows.Visibility.Collapsed;
		}
		private void mx_tbRename_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return || e.Key == Key.Enter)
			{
				mx_radio.Visibility = System.Windows.Visibility.Visible;
				mx_tbRename.Visibility = System.Windows.Visibility.Collapsed;
				mx_radio.Focus();
			}
		}

		private void mx_repackImage_Click(object sender, RoutedEventArgs e)
		{
			ImageTools.ImageNesting.pngToTgaRectNesting(m_path);
		}
		private void mx_newImageFolder_Click(object sender, RoutedEventArgs e)
		{
			IncludeFile imageRootFolder = getImageRootFolder();

			if (imageRootFolder != null)
			{
				string newPath = Project.Setting.s_projPath + "\\images\\newFolder";
				try
				{
					Directory.CreateDirectory(newPath);
				}
				catch
				{
					Public.ResultLink.createResult("\r\n图片包(" + newPath + ")创建失败，可能是由于文件名重复或没有写权限。",
						Public.ResultType.RT_ERROR);

					return;
				}
				IncludeFile newImageFolder = new IncludeFile(newPath);

				imageRootFolder.Items.Add(newImageFolder);
				imageRootFolder.IsExpanded = true;
				newImageFolder.BringIntoView();
				newImageFolder.mx_radio.IsChecked = true;
				newImageFolder.pngFileDeal(null, "rename");
			}
		}
		static public IncludeFile getImageRootFolder()
		{
			IncludeFile imageRootFolder;

			if (MainWindow.s_pW.m_mapIncludeFiles != null && Project.Setting.s_projPath != null && Project.Setting.s_projPath != "" &&
				MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(Project.Setting.s_projPath + "\\images", out imageRootFolder))
			{
				return imageRootFolder;
			}

			return null;
		}
		private void mx_root_GotFocus(object sender, RoutedEventArgs e)
		{
			mx_radio.IsChecked = true;
		}
		private void mx_root_LostFocus(object sender, RoutedEventArgs e)
		{
			mx_radio.IsChecked = false;
		}
	}
}
