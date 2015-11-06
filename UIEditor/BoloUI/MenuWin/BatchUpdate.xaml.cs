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
using System.Windows.Shapes;

namespace UIEditor.BoloUI.MenuWin
{
	/// <summary>
	/// BatchUpdate.xaml 的交互逻辑
	/// </summary>
	public partial class BatchUpdate
	{
		public static BatchUpdate s_pW;

		public BatchUpdate()
		{
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			s_pW = this;
			refreshCtrlComboBox();
		}
		public void refreshCtrlComboBox()
		{
			bool isEvent;

			if(mx_rbEvent.IsChecked == true)
			{
				isEvent = true;
			}
			else
			{
				isEvent = false;
			}

			mx_cbCtrl.Items.Clear();
			mx_cbAttr.Items.Clear();
			foreach(KeyValuePair<string, DefConfig.CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapCtrlDef.ToList())
			{
				if(pairCtrlDef.Value.m_enInsert == true || pairCtrlDef.Value.m_enInsertAll == true)
				{
					ComboBoxItem cbItem = new ComboBoxItem();
					string ctrlName = MainWindow.s_pW.m_strDic.getWordByKey(pairCtrlDef.Key);

					if (ctrlName == "")
					{
						ctrlName = pairCtrlDef.Key;
					}
					else
					{
						ctrlName = pairCtrlDef.Key + " | " + ctrlName;
					}
					cbItem.Content = ctrlName;
					cbItem.ToolTip = pairCtrlDef.Key;
					mx_cbCtrl.Items.Add(cbItem);
				}
			}
		}

		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{

		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void mx_rbEvent_Checked(object sender, RoutedEventArgs e)
		{
			if (mx_lbAttr != null && mx_lbValue != null)
			{
				mx_lbAttr.Content = "事件：";
				mx_lbValue.Content = "脚本：";
				refreshCtrlComboBox();
			}
		}
		private void mx_rbAttr_Checked(object sender, RoutedEventArgs e)
		{
			if (mx_lbAttr != null && mx_lbValue != null)
			{
				mx_lbAttr.Content = "属性：";
				mx_lbValue.Content = "值：";
				refreshCtrlComboBox();
			}
		}
		private void mx_cbCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(sender is ComboBox)
			{
				object selObj = ((ComboBox)sender).SelectedItem;

				if(selObj != null && selObj is ComboBoxItem)
				{
					ComboBoxItem cbItem = (ComboBoxItem)selObj;
					string ctrlName = cbItem.ToolTip.ToString();
					DefConfig.CtrlDef_T ctrlDef;

					mx_cbAttr.Items.Clear();
					if (ctrlName != "" && MainWindow.s_pW.m_mapCtrlDef.TryGetValue(ctrlName, out ctrlDef) && ctrlDef != null && ctrlDef.m_mapAttrDef != null)
					{
						if (mx_rbAttr.IsChecked == true)
						{
							//改属性
							foreach (KeyValuePair<string, DefConfig.AttrDef_T> pairAttr in ctrlDef.m_mapAttrDef.ToList())
							{
								string attrName = MainWindow.s_pW.m_strDic.getWordByKey(pairAttr.Key);
								ComboBoxItem cbiAttr = new ComboBoxItem();

								if (attrName == "")
								{
									attrName = pairAttr.Key;
								}
								else
								{
									attrName = pairAttr.Key + " | " + attrName;
								}
								cbiAttr.Content = attrName;
								cbiAttr.ToolTip = pairAttr.Key;
								mx_cbAttr.Items.Add(cbiAttr);
							}
						}
						else
						{
							//改事件
							Dictionary<string, string> mapEvent = XmlControl.getCtrlEventMap(ctrlName);

							foreach(KeyValuePair<string, string> pairEvent in mapEvent)
							{
								string strShow = pairEvent.Key;
								ComboBoxItem cbiEvent = new ComboBoxItem();

								if(pairEvent.Value != null && pairEvent.Value != "")
								{
									strShow += " | " + pairEvent.Value;
								}
								cbiEvent.Content = strShow;
								cbiEvent.ToolTip = pairEvent.Key;
								mx_cbAttr.Items.Add(cbiEvent);
							}
						}
					}
				}
			}
		}
		private void mx_cbAttr_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is ComboBox)
			{
				object selObj = ((ComboBox)sender).SelectedItem;

				if (selObj != null && selObj is ComboBoxItem)
				{
					mx_ok.IsEnabled = true;
				}
				else
				{
					mx_ok.IsEnabled = false;
				}
			}
		}
	}
}
