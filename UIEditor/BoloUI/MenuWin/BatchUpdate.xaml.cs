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
using System.Xml;

namespace UIEditor.BoloUI.MenuWin
{
	/// <summary>
	/// BatchUpdate.xaml 的交互逻辑
	/// </summary>
	public partial class BatchUpdate
	{
		public static BatchUpdate s_pW;

		public XmlItem m_ctrlDef;

		public BatchUpdate(XmlItem ctrlDef)
		{
			m_ctrlDef = ctrlDef;

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

		public void resetAllAttrByCtrlName(XmlElement xeParent, string ctrlName, string attrName, string attrValue, string type = "attr")
		{
			foreach(XmlNode xn in xeParent.ChildNodes)
			{
				if (xn is XmlElement)
				{
					XmlElement xe = (XmlElement)xn;

					if (xe.Name == ctrlName)
					{
						switch (type)
						{
							case "attr":
								{
									if (attrValue != null && attrValue != "")
									{
										xe.SetAttribute(attrName, attrValue);
									}
									else
									{
										xe.RemoveAttribute(attrName);
									}
								}
								break;
							case "event":
								{
									XmlControl.setEvent(xe, attrName, attrValue);
								}
								break;
							default:
								break;
						}
					}

					resetAllAttrByCtrlName(xe, ctrlName, attrName, attrValue, type);
				}
			}
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if(mx_cbCtrl.SelectedItem != null && mx_cbCtrl.SelectedItem is ComboBoxItem &&
				mx_cbAttr.SelectedItem != null && mx_cbAttr.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem cbiCtrl = (ComboBoxItem)mx_cbCtrl.SelectedItem;
				ComboBoxItem cbiAttr = (ComboBoxItem)mx_cbAttr.SelectedItem;
				XmlControl xmlCtrlDef = m_ctrlDef.m_xmlCtrl;
				XmlDocument docBatch = new XmlDocument();
				List<int> lstXeLc = new List<int>();
				XmlElement xeDst;
				string dealType = "event";

				docBatch.LoadXml(xmlCtrlDef.m_xmlDoc.OuterXml);
				XmlControl.getElementLocation(m_ctrlDef.m_xe, lstXeLc);
				xeDst = XmlControl.getXeByLocationList(docBatch, lstXeLc);
			
				if(mx_rbEvent.IsChecked == true)
				{
					dealType = "event";
				}
				else
				{
					dealType = "attr";
				}
				resetAllAttrByCtrlName(xeDst, cbiCtrl.ToolTip.ToString(), cbiAttr.ToolTip.ToString(), mx_tbValue.Text.ToString(), dealType);

				string oldStr = XmlControl.getOutXml(xmlCtrlDef.m_xmlDoc);
				string newStr = XmlControl.getOutXml(docBatch);

				if (string.Compare(oldStr, newStr) != 0)
				{
					xmlCtrlDef.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(xmlCtrlDef.m_xmlDoc, docBatch));
					xmlCtrlDef.refreshControl();
					xmlCtrlDef.refreshXmlText();
				}
			}

			this.Close();
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
		private void addAttrCbi(DefConfig.CtrlDef_T ctrlDef)
		{
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
							foreach(KeyValuePair<string, DefConfig.CtrlDef_T> pairCtrl in MainWindow.s_pW.m_mapCtrlDef.ToList())
							{
								if (pairCtrl.Value != null && pairCtrl.Value.m_isBasic)
								{
									addAttrCbi(pairCtrl.Value);
								}
							}
							addAttrCbi(ctrlDef);
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
