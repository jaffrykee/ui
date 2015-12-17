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
using System.Xml;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;

namespace UIEditor.XmlOperation.XmlAttr
{
	public partial class AttrList : TabItem
	{
		public string m_name;
		public XmlElement m_xe;
		public XmlControl m_xmlCtrl;
		public XmlItem m_basic;

		public AttrList(string name = "", Dictionary<string, AttrDef_T> mapAttrDef = null)
		{
			m_name = name;
			this.InitializeComponent();

			if(mapAttrDef != null)
			{
				foreach (KeyValuePair<string, AttrDef_T> pairAttrDef in mapAttrDef.ToList())
				{
					if (pairAttrDef.Value.m_isEnum == false)
					{
						switch (pairAttrDef.Value.m_type)
						{
							case "bool":
								{
									pairAttrDef.Value.m_iAttrRowUI = new RowBool(pairAttrDef.Value, pairAttrDef.Key, "", this);
								}
								break;
							case "weight":
								{
									pairAttrDef.Value.m_iAttrRowUI = new RowWeight(pairAttrDef.Value, pairAttrDef.Key, "", this);
								}
								break;
							default:
								{
									pairAttrDef.Value.m_iAttrRowUI = new RowNormal(pairAttrDef.Value, pairAttrDef.Key, "", this);
								}
								break;
						}
					}
					else
					{
						pairAttrDef.Value.m_iAttrRowUI = new RowEnum(pairAttrDef.Value, pairAttrDef.Key, "", this);
					}
					mx_frame.Children.Add((Grid)pairAttrDef.Value.m_iAttrRowUI);
				}
			}

			string ctrlWord = MainWindow.s_pW.m_strDic.getWordByKey(m_name);

			if (ctrlWord == "")
			{
				ctrlWord = m_name;
			}
			this.Header = ctrlWord;
			mx_title.Content = ctrlWord;
		}

		public void clearRowValue()
		{
			foreach (object row in mx_frame.Children)
			{
				if (row.GetType().ToString() != "System.Windows.Controls.Grid")
				{
					((IAttrRow)row).m_preValue = "";
				}
			}
		}
		public void refreshRowVisible()
		{
			bool onlySetted;
			bool onlyCommon;

			if (mx_onlySetted != null && mx_onlySetted.IsChecked == true)
			{
				onlySetted = true;
			}
			else
			{
				onlySetted = false;
			}
			if (mx_onlyCommon != null && mx_onlyCommon.IsChecked == true)
			{
				onlyCommon = true;
			}
			else
			{
				onlyCommon = false;
			}
			foreach (object row in mx_frame.Children)
			{
				if (row.GetType().ToString() != "System.Windows.Controls.Grid")
				{
					((Grid)row).Visibility = Visibility.Visible;
				}
			}
			if(onlySetted)
			{
				foreach (object row in mx_frame.Children)
				{
					if (row.GetType().ToString() != "System.Windows.Controls.Grid" && ((IAttrRow)row).m_preValue == "")
					{
						((Grid)row).Visibility = Visibility.Collapsed;
					}
				}
			}
			if (onlyCommon)
			{
				foreach (object row in mx_frame.Children)
				{
					if (row.GetType().ToString() != "System.Windows.Controls.Grid" && ((IAttrRow)row).m_isCommon == false)
					{
						((Grid)row).Visibility = Visibility.Collapsed;
					}
				}
			}
			mx_toolScroll.ScrollToRightEnd();
		}

		static public void hiddenOtherAttrList()
		{
			if (MainWindow.s_pW.m_otherAttrList != null)
			{
				MainWindow.s_pW.mx_toolArea.Items.Remove(MainWindow.s_pW.m_otherAttrList);
				MainWindow.s_pW.m_otherAttrList = null;
			}
		}
		static private int s_lastRightToolsIndex = 0;
		static public int getVisibleSelectItemIndex(TabControl tab)
		{
			int index = 0;
			var temp = tab.Items;

			foreach (TabItem tabItem in temp)
			{
				if (tabItem.Visibility == System.Windows.Visibility.Visible)
				{
					if (tabItem.IsSelected == true)
					{
						return index;
					}
					else
					{
						index++;
					}
				}
			}

			return -1;
		}
		static public void setVisibleSelectItemIndex(TabControl tab, int selIndex)
		{
			int index = 0;
			var temp = tab.Items;

			foreach (TabItem tabItem in temp)
			{
				if (tabItem.Visibility == System.Windows.Visibility.Visible)
				{
					if (index == selIndex)
					{
						tabItem.IsSelected = true;

						return;
					}
					else
					{
						index++;
					}
				}
			}
		}
		static public void hiddenAllAttr()
		{
			s_lastRightToolsIndex = getVisibleSelectItemIndex(MainWindow.s_pW.mx_toolArea);
			hiddenOtherAttrList();
			//AttrList
			foreach (object attrList in MainWindow.s_pW.mx_toolArea.Items)
			{
				if (attrList is TabItem)
				{
					TabItem tab = (TabItem)attrList;

					tab.Visibility = Visibility.Collapsed;
				}
			}
			if (MainWindow.s_pW.mx_skinEditor != null)
			{
				XmlControl curXml = XmlControl.getCurXmlControl();

				if (curXml != null && curXml.m_curItem != null && curXml.m_curItem is ResBasic)
				{
					ResBasic curSkin = (ResBasic)curXml.m_curItem;

					if (curSkin.m_isSkinEditor == true)
					{
						MainWindow.s_pW.mx_skinEditor.Visibility = System.Windows.Visibility.Visible;
					}
					else
					{
						MainWindow.s_pW.mx_skinEditor.Visibility = System.Windows.Visibility.Collapsed;
						if (MainWindow.s_pW.mx_treeFrame.SelectedItem == MainWindow.s_pW.mx_skinEditor)
						{
							MainWindow.s_pW.mx_skinEditor.mx_treeAppr.Items.Clear();
							MainWindow.s_pW.mx_treeFrame.SelectedIndex = 0;
						}
					}
				}
				else
				{
					MainWindow.s_pW.mx_skinEditor.Visibility = System.Windows.Visibility.Collapsed;
					if (MainWindow.s_pW.mx_treeFrame.SelectedItem == MainWindow.s_pW.mx_skinEditor)
					{
						MainWindow.s_pW.mx_skinEditor.mx_treeAppr.Items.Clear();
						MainWindow.s_pW.mx_treeFrame.SelectedIndex = 0;
					}
				}
			}
		}
		static public void selectFirstVisibleAttrList()
		{
			foreach (TabItem tabItem in MainWindow.s_pW.mx_toolArea.Items)
			{
				if (tabItem.Visibility == Visibility.Visible)
				{
					MainWindow.s_pW.mx_toolArea.SelectedItem = tabItem;

					return;
				}
			}
		}
		static public void selectLastAttrList()
		{
			if (s_lastRightToolsIndex >= 0)
			{
				setVisibleSelectItemIndex(MainWindow.s_pW.mx_toolArea, s_lastRightToolsIndex);
			}
			else
			{
				selectFirstVisibleAttrList();
			}
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			mx_toolScroll.ScrollToRightEnd();
		}
		private void mx_onlySetted_Checked(object sender, RoutedEventArgs e)
		{
			refreshRowVisible();
		}
		private void mx_onlySetted_Unchecked(object sender, RoutedEventArgs e)
		{
			refreshRowVisible();
		}
		private void mx_onlyCommon_Checked(object sender, RoutedEventArgs e)
		{
			refreshRowVisible();
		}
		private void mx_onlyCommon_Unchecked(object sender, RoutedEventArgs e)
		{
			refreshRowVisible();
		}
	}
}