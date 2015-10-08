using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Xml;

namespace UIEditor.Project
{
	public partial class NewFileWin : Window
	{
		public string m_tmplPath;
		public FileTypeRadio m_curFileType;
		public bool m_isProj;
		static public NewFileWin m_pW;

		public NewFileWin(string path, bool isProj = false)
		{
			m_pW = this;
			m_curFileType = null;
			m_isProj = isProj;
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			if (isProj)
			{
				mx_projFrame.Visibility = System.Windows.Visibility.Visible;
				mx_fileFrame.Visibility = System.Windows.Visibility.Collapsed;
			}
			else
			{
				mx_projFrame.Visibility = System.Windows.Visibility.Collapsed;
				mx_fileFrame.Visibility = System.Windows.Visibility.Visible;
			}
			if (Directory.Exists(path))
			{
				m_tmplPath = path;
				refreshFolder(m_tmplPath, mx_tree, m_isProj, true, true, addTmplToRadioGroup);
			}
			else
			{
				MessageBox.Show("模板目录（" + path + "）不存在，UI编辑器可能已经损坏。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			m_pW = null;
		}

		static public void refreshFolder(
			string path,
			object item,
			bool isProj = false,
			bool onlyFolder = false,
			bool isExpanded = true,
			MouseButtonEventHandler clickHandler = null)
		{
			TreeViewItem rootItem = null;
			TreeView root = null;

			if (item is TreeViewItem)
			{
				rootItem = (TreeViewItem)item;
			}
			else if(item is TreeView)
			{
				root = (TreeView)item;
			}
			else
			{
				return;
			}

			if (rootItem != null)
			{
				rootItem.Items.Clear();
				rootItem.IsExpanded = isExpanded;
			}

			int i = 0;
			int j = 0;

			DirectoryInfo di = new DirectoryInfo(path);
			foreach (var dri in di.GetDirectories())
			{
				TreeViewItem treeUIChild = new TreeViewItem();
				ToolTip treeTip = new ToolTip();

				i++;
				treeTip.Content = path + "\\" + dri.Name;
				treeUIChild.ToolTip = path + "\\" + dri.Name;
				treeUIChild.Header = dri.Name;
				if(rootItem != null)
				{
					rootItem.Items.Add(treeUIChild);
				}
				else if (root != null)
				{
					root.Items.Add(treeUIChild);
				}
				if (clickHandler != null)
				{
					treeUIChild.PreviewMouseLeftButtonDown += clickHandler;
				}

				if (!isProj)
				{
					refreshFolder(path + "\\" + dri.Name, treeUIChild, isProj, onlyFolder, isExpanded, clickHandler);
				}
			}
			if (!onlyFolder)
			{
				foreach (var dri in di.GetFiles("*"))
				{
					TreeViewItem treeUIChild = new TreeViewItem();
					ToolTip treeTip = new ToolTip();

					j++;
					treeTip.Content = path + "\\" + dri.Name;
					treeUIChild.ToolTip = path + "\\" + dri.Name;
					treeUIChild.Header = dri.Name;
					if (clickHandler != null)
					{
						treeUIChild.PreviewMouseLeftButtonDown += clickHandler;
					}
					if (rootItem != null)
					{
						rootItem.Items.Add(treeUIChild);
					}
					else if (root != null)
					{
						root.Items.Add(treeUIChild);
					}
				}
			}
		}

		private void addTmplToRadioGroup(object sender, RoutedEventArgs e)
		{
			mx_fileTypeFrame.Children.Clear();
			if(sender is TreeViewItem)
			{
				string path = ((TreeViewItem)sender).ToolTip.ToString();
				if(Directory.Exists(path))
				{
					DirectoryInfo di = new DirectoryInfo(path);
					if(!m_isProj)
					{
						foreach (var fileInfo in di.GetFiles("*"))
						{
							FileTypeRadio ftr = new FileTypeRadio(this, System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name), di.Name);
							ToolTip treeTip = new ToolTip();

							treeTip.Content = path + "\\" + fileInfo.Name;
							ftr.ToolTip = treeTip.Content;
							mx_fileTypeFrame.Children.Add(ftr);
						}
					}
					else
					{
						foreach (var dirInfo in di.GetDirectories("*"))
						{
							FileTypeRadio ftr = new FileTypeRadio(this, System.IO.Path.GetFileNameWithoutExtension(dirInfo.Name), di.Name);
							ToolTip treeTip = new ToolTip();

							treeTip.Content = path + "\\" + dirInfo.Name;
							ftr.ToolTip = treeTip.Content;
							mx_fileTypeFrame.Children.Add(ftr);
						}
					}
				}
			}
		}
		private void CopyDirectory(string srcdir, string desdir)
		{
			string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);
			string desfolderdir = desdir + "\\" + folderName;

			if (desdir.LastIndexOf("\\") == (desdir.Length - 1))
			{
				desfolderdir = desdir + folderName;
			}
			string[] filenames = Directory.GetFileSystemEntries(srcdir);

			foreach (string file in filenames)
			{
				if (Directory.Exists(file))
				{
					string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
					if (!Directory.Exists(currentdir))
					{
						Directory.CreateDirectory(currentdir);
					}

					CopyDirectory(file, desfolderdir);
				}
				else
				{
					string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);

					srcfileName = desfolderdir + "\\" + srcfileName;
					if (!Directory.Exists(desfolderdir))
					{
						Directory.CreateDirectory(desfolderdir);
					}
					File.Copy(file, srcfileName);
				}
			}
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			string tmplPath = m_curFileType.ToolTip.ToString();

			if(!m_isProj)
			{
				string type = System.IO.Path.GetExtension(m_curFileType.ToolTip.ToString());
				string path = MainWindow.s_pW.m_projPath;

				if (type == ".bur")
				{
					path = path + "\\skin";
				}
				path = path + "\\" + mx_fileName.Text.ToString() + ".xml";

				if (File.Exists(path))
				{
					MessageBox.Show("该文件名已经存在(" + path + ")，请重新输入文件名。", "文件名重复", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
				else
				{
					File.Copy(tmplPath, path, false);
					MainWindow.s_pW.refreshProjTree(MainWindow.s_pW.m_projPath, MainWindow.s_pW.mx_treePro, true);
					this.Close();
				}
			}
			else
			{
				string path = mx_projPath.Text;

				if(!Directory.Exists(path))
				{
					MessageBox.Show("该目录不存在，请重新选择。（" + path + "）", "目录错误", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				DirectoryInfo di = new DirectoryInfo(tmplPath);
				DirectoryInfo[] arrDi = di.GetDirectories();

				if(arrDi.Count() == 1)
				{
					string dirName = arrDi[0].Name;
					if (Directory.Exists(path + "\\" + dirName))
					{
						MessageBox.Show("该目录已经存在“" + dirName + "”文件夹，请重新选择目录，或者删除该目录的此文件夹。（旧项目请通过打开项目来进行打开）",
							"项目已存在", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
					CopyDirectory(tmplPath + "\\" + dirName, path);
					MainWindow.s_pW.openProjByPath(path + "\\" + dirName , "pro.bup");
					this.Close();
				}
				else
				{
					MessageBox.Show("模板格式不合法，请检查目录：" + tmplPath, "模板损坏", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void mx_fileName_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(mx_fileName.Text != "")
			{
				if (!mx_ok.IsEnabled && m_curFileType != null)
				{
					mx_ok.IsEnabled = true;
				}
			}
			else
			{
				mx_ok.IsEnabled = false;
			}
		}
		private void mx_fileName_GotFocus(object sender, RoutedEventArgs e)
		{
		}

		private void mx_projPath_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (mx_projPath.Text != "")
			{
				if (!mx_ok.IsEnabled && m_curFileType != null)
				{
					mx_ok.IsEnabled = true;
				}
			}
			else
			{
				mx_ok.IsEnabled = false;
			}
		}
		private void mx_selPath_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog winDir = new System.Windows.Forms.FolderBrowserDialog();

			winDir.Description = "请选择工程目录保存的路径：";
			winDir.ShowNewFolderButton = true;

			System.Windows.Forms.DialogResult result = winDir.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				return;
			}
			mx_projPath.Text = winDir.SelectedPath;
		}
	}
}
