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

		public string m_preViewBaseId;
		public string m_preViewSkinName;
		public BoloUI.Basic m_prePlusCtrlUI;

		public OpenedFile(string path)
		{
			MainWindow pW = MainWindow.s_pW;

			m_path = path;
			m_fileType = StringDic.getFileType(m_path);
			m_tab = new TabItem();

			pW.mx_workTabs.Items.Add(m_tab);
			pW.mx_workTabs.SelectedItem = m_tab;

			m_tab.Template = (ControlTemplate)(App.Current.Resources["TmplFileItem"]);
			m_tab.Unloaded += new RoutedEventHandler(pW.eventCloseFile);
			ToolTip tabTip = new ToolTip();
			tabTip.Content = m_path;
			m_tab.Header = StringDic.getFileNameWithoutPath(path);
			m_tab.Header = m_tab.Header.ToString().Replace("_", "__");
			m_tab.ToolTip = tabTip;

			UserControl tabContent = new UIEditor.FileTabItem(this);

			m_tab.Content = tabContent;
			m_tabItem = (FileTabItem)tabContent;
		}

		public bool frameIsXmlCtrl()
		{
			if (m_frame != null)
			{
				if (m_frame.GetType() == Type.GetType("UIEditor.XmlControl") && ((XmlControl)m_frame).m_showGL == true)
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
	}
}
