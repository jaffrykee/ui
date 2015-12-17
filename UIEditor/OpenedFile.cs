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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text.RegularExpressions;
using System.Collections;
using UIEditor.XmlOperation;
using UIEditor.Public;
using UIEditor.Project;

namespace UIEditor
{
	public class OpenedFile
	{
		public string m_path;
		public TabItem m_tab;
		public UserControl m_frame;
		public FileTabItem m_tabItem;
		public string m_fileType;
		public HistoryList m_lstOpt;
		public string m_curViewSkin;

		public string m_preViewBaseId;
		public string m_preViewSkinName;
		public BoloUI.Basic m_prePlusCtrlUI;

		public DateTime m_lastWriteTime;
		public Paragraph m_paraResult;
		static public Paragraph s_paraResult;
		public List<ResultLink> m_lstResult;
		static public List<ResultLink> s_lstResult = new List<ResultLink>();

		static private OpenedFileContextMenu st_menu = null;
		static public OpenedFileContextMenu s_menu
		{
			get
			{
				if(OpenedFile.getCurFileDef() != null)
				{
					if(st_menu == null)
					{
						st_menu = new OpenedFileContextMenu();
					}

					return st_menu;
				}
				else
				{
					return null;
				}
			}
			set
			{
				st_menu = value;
			}
		}

		public OpenedFile(string path, string skinName = "", bool isShowTabItem = true)
		{
			Public.ErrorInfo.clearErrorInfo();
			MainWindow pW = MainWindow.s_pW;
			MainWindow.s_pW.m_mapOpenedFiles[path] = this;

			m_path = path;
			m_paraResult = new Paragraph();
			m_lstResult = new List<ResultLink>();
			Public.ResultLink.s_curResultFrame = m_paraResult;
			if(File.Exists(m_path))
			{
				FileInfo fi = new FileInfo(m_path);

				m_lastWriteTime = fi.LastWriteTime;
			}
			m_fileType = StringDic.getFileType(m_path);
			m_tab = new TabItem();
			m_curViewSkin = skinName;

			pW.mx_workTabs.Items.Add(m_tab);
			pW.mx_workTabs.SelectedItem = m_tab;

			m_tab.Unloaded += new RoutedEventHandler(pW.eventCloseFile);
			ToolTip tabTip = new ToolTip();
			tabTip.Content = m_path;
			m_tab.Header = "_" + StringDic.getFileNameWithoutPath(path);
			m_tab.ToolTip = tabTip;
			m_tab.MouseRightButtonDown += fileTabItem_MouseRightButtonDown;

			if(isShowTabItem)
			{
				UserControl tabContent = new UIEditor.FileTabItem(this, skinName);

				m_tab.Content = tabContent;
				m_tabItem = (FileTabItem)tabContent;
			}
			else
			{
				m_tabItem = null;
			}
			Public.ErrorInfo.showErrorInfo();
		}

		private void fileTabItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			MainWindow.s_pW.openFileByDef(this);

			if (s_menu != null)
			{
				s_menu.mx_menu.PlacementTarget = m_tab;
				s_menu.mx_menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
				s_menu.mx_menu.IsOpen = true;
			}
		}

		public bool frameIsXmlCtrl()
		{
			if (m_frame != null)
			{
				//if (m_frame is XmlControl) && ((XmlControl)m_frame).m_showGL == true)
				if (m_frame is XmlControl)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		public bool haveDiffToFile()
		{
			if (frameIsXmlCtrl() && m_lstOpt != null)
			{
				if (m_lstOpt.m_saveNode == m_lstOpt.m_curNode)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}
		public void updateSaveStatus()
		{
			if (File.Exists(m_path))
			{
				FileInfo fi = new FileInfo(m_path);

				m_lastWriteTime = fi.LastWriteTime;
			}
			string ec = m_tab.Header.ToString().Substring(m_tab.Header.ToString().Length - 1, 1);
			if (haveDiffToFile())
			{
				//加上*
				if (ec != "*")
				{
					m_tab.Header = m_tab.Header.ToString() + "*";
				}
			}
			else
			{
				//去掉*
				if (ec == "*")
				{
					m_tab.Header = m_tab.Header.ToString().Substring(0, m_tab.Header.ToString().Length - 1);
				}
			}
		}

		static public ArrayList checkFileChangedAtOutside()
		{
			ArrayList arrFileChanged = new ArrayList();

			foreach(KeyValuePair<string, OpenedFile> pairFiledef in MainWindow.s_pW.m_mapOpenedFiles.ToList())
			{
				if(File.Exists(pairFiledef.Value.m_path))
				{
					
					FileInfo fi = new FileInfo(pairFiledef.Value.m_path);

					if(fi.LastWriteTime != pairFiledef.Value.m_lastWriteTime)
					{
						arrFileChanged.Add(pairFiledef.Value.m_path);
						pairFiledef.Value.m_lastWriteTime = fi.LastWriteTime;
					}
				}
			}

			return arrFileChanged;
		}
		static public OpenedFile getCurFileDef()
		{
			OpenedFile fileDef;

			if (MainWindow.s_pW != null &&
				MainWindow.s_pW.m_curFile != null &&
				MainWindow.s_pW.m_curFile != "" &&
				MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(MainWindow.s_pW.m_curFile, out fileDef) &&
				fileDef != null)
			{
				return fileDef;
			}

			return null;
		}
		static public void closeAllFile(OpenedFile exFileDef = null)
		{
			List<OpenedFile> lstFileDef = new List<OpenedFile>();

			foreach(KeyValuePair<string, OpenedFile> pairFileDef in MainWindow.s_pW.m_mapOpenedFiles.ToList())
			{
				if (pairFileDef.Value != null && pairFileDef.Value != exFileDef)
				{
					lstFileDef.Add(pairFileDef.Value);
				}
			}

			foreach(OpenedFile fileDef in lstFileDef)
			{
				fileDef.m_tabItem.closeFile();
			}
		}
		static public void openLocalFolder(string path)
		{
			if (File.Exists(path))
			{
				FileInfo fi = new FileInfo(path);

				System.Diagnostics.Process.Start("explorer.exe", fi.DirectoryName);
			}
			else if (Directory.Exists(path))
			{
				System.Diagnostics.Process.Start("explorer.exe", path);
			}
		}
	}
}
