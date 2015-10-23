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

namespace UIEditor
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
					pairAttrDef.Value.m_attrRowUI = new AttrRow(pairAttrDef.Value, pairAttrDef.Key, "", this);
					mx_frame.Children.Add(pairAttrDef.Value.m_attrRowUI);
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
				if (row is AttrRow)
				{
					((AttrRow)row).m_preValue = "";
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
				if (row is AttrRow)
				{
					((AttrRow)row).mx_root.Visibility = Visibility.Visible;
				}
			}
			if(onlySetted)
			{
				foreach (object row in mx_frame.Children)
				{
					if (row is AttrRow)
					{
						if (((AttrRow)row).m_preValue == "")
						{
							((AttrRow)row).mx_root.Visibility = Visibility.Collapsed;
						}
					}
				}
			}
			if (onlyCommon)
			{
				foreach (object row in mx_frame.Children)
				{
					if (row is AttrRow)
					{
						if (((AttrRow)row).m_isCommon == false)
						{
							((AttrRow)row).mx_root.Visibility = Visibility.Collapsed;
						}
					}
				}
			}
			mx_toolScroll.ScrollToRightEnd();
		}
		static public void selectFirstVisibleAttrList()
		{
			foreach(TabItem tabItem in MainWindow.s_pW.mx_toolArea.Items)
			{
				if (tabItem.Visibility == Visibility.Visible)
				{
					MainWindow.s_pW.mx_toolArea.SelectedItem = tabItem;

					return;
				}
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