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
using UIEditor.XmlOperation;
using System.Collections;

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

		public OpenedFile(string path, string skinName = "", bool isShowTabItem = true)
		{
			Public.ErrorInfo.clearErrorInfo();
			MainWindow pW = MainWindow.s_pW;

			m_path = path;
			m_paraResult = new Paragraph();
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
	}
}
