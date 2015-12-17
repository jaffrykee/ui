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
using System.IO;
using System.Text.RegularExpressions;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;
using UIEditor.Public;

namespace UIEditor.BoloUI
{
	/// <summary>
	/// XmlItemContextMenu.xaml 的交互逻辑
	/// </summary>
	public partial class XmlItemContextMenu : UserControl
	{
		public XmlItemContextMenu()
		{
			InitializeComponent();
		}
		private void mx_menu_Loaded(object sender, RoutedEventArgs e)
		{
			refreshItemMenu();
		}
		private void mx_menu_Unloaded(object sender, RoutedEventArgs e)
		{
		}

		private void mx_addNode_Loaded(object sender, RoutedEventArgs e)
		{
			if (XmlItem.getCurItem().m_type == "CtrlUI")
			{
				CtrlDef_T panelCtrlDef;

				mx_addNode.Items.Clear();
				if (MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(XmlItem.getCurItem().m_xe.Name, out panelCtrlDef))
				{
					foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapEnInsertCtrlDef.ToList())
					{
						if (!pairCtrlDef.Value.m_isFrame)
						{
							showTmplGroup(pairCtrlDef.Key);
						}
					}
					mx_addNode.Items.Add(new Separator());
					foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapEnInsertCtrlDef.ToList())
					{
						if (pairCtrlDef.Value.m_isFrame)
						{
							showTmplGroup(pairCtrlDef.Key);
						}
					}
					mx_addNode.Items.Add(new Separator());
					foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapEnInsertAllCtrlDef.ToList())
					{
						showTmplGroup(pairCtrlDef.Key);
					}
				}
				else
				{
					if (XmlItem.getCurItem().m_xe.Name == "BoloUI")
					{
						//<inc>showTmplGroup(pairCtrlDef.Key);
						foreach (KeyValuePair<string, CtrlDef_T> pairCtrl in MainWindow.s_pW.m_mapPanelCtrlDef.ToList())
						{
							MenuItem ctrlItem = new MenuItem();
							string name = MainWindow.s_pW.m_strDic.getWordByKey(pairCtrl.Key);
							string tip = MainWindow.s_pW.m_strDic.getWordByKey(pairCtrl.Key, StringDic.conf_ctrlTipDic);

							if (tip != "")
							{
								ctrlItem.ToolTip = tip;
							}
							else
							{
								ctrlItem.ToolTip = pairCtrl.Key;
							}
							if (name != "")
							{
								ctrlItem.Header = name;
							}
							else
							{
								ctrlItem.Header = pairCtrl.Key;
							}
							ctrlItem.Click += insertCtrlItem_Click;
							mx_addNode.Items.Add(ctrlItem);
						}
					}
					else if (XmlItem.getCurItem().m_xe.Name != "event")
					{
						showTmplGroup("event");
						/*
							正则替换："E:\clientlib\DsBoloUIEditor\src\boloUI\BoloEvent.java" 到 "E:\clienttools\UIEditor2\conf.xml" -->
							[\t a-z_=]*("[a-zA-Z]*")[; \t\/\/]*([\/\u4e00-\u9fa5 a-zA-Z（）]*)
							\t<row name=$1 tip="$2">\r\n\t\t<event type=$1/>\r\n\t</row>
						*/
					}
					else
					{
						mx_addNode.IsEnabled = false;
					}
				}
			}
			else
			{
				Dictionary<string, SkinDef_T> mapSkinDef;
				mx_addNode.Items.Clear();
				if (XmlItem.getCurItem().m_xe.Name == "BoloUI")
				{
					mapSkinDef = MainWindow.s_pW.m_mapSkinTreeDef;
				}
				else
				{
					mapSkinDef = ((ResBasic)XmlItem.getCurItem()).m_curDeepDef.m_mapEnChild;
				}
				if (mapSkinDef != null)
				{
					foreach (KeyValuePair<string, SkinDef_T> pairSkinDef in mapSkinDef.ToList())
					{
						showTmplGroup(pairSkinDef.Key);
					}
					mx_addNode.IsEnabled = true;
				}
				else
				{
					mx_addNode.IsEnabled = false;
				}
			}
		}
		private void insertCtrlItem_Click(object sender, RoutedEventArgs e)
		{
			switch (sender.GetType().ToString())
			{
				case "System.Windows.Controls.MenuItem":
					{
						MenuItem ctrlItem = (MenuItem)sender;
						XmlElement newXe = XmlItem.getCurItem().m_xe.OwnerDocument.CreateElement(ctrlItem.ToolTip.ToString());
						BoloUI.Basic treeChild = new BoloUI.Basic(newXe, XmlItem.getCurItem().m_xmlCtrl);

						if (treeChild.m_xe.Name == "event")
						{
							XmlItem.getCurItem().m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, XmlItem.getCurItem().m_xe, 0));
						}
						else
						{
							XmlItem.getCurItem().m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, XmlItem.getCurItem().m_xe, XmlItem.getCurItem().m_xe.ChildNodes.Count));
						}
					}
					break;
				case "UIEditor.BoloUI.CtrlTemplate":
					{
						BoloUI.CtrlTemplate ctrlItem = (BoloUI.CtrlTemplate)sender;
						XmlDocument newDoc = new XmlDocument();
						XmlElement newXe = XmlItem.getCurItem().m_xe.OwnerDocument.CreateElement("tmp");

						if (ctrlItem.ToolTip.ToString() != "")
						{
							newXe.InnerXml = ctrlItem.ToolTip.ToString();
							if (newXe.FirstChild.NodeType == XmlNodeType.Element)
							{
								BoloUI.Basic treeChild = new BoloUI.Basic((XmlElement)newXe.FirstChild, XmlItem.getCurItem().m_xmlCtrl);

								if (treeChild.m_xe.Name == "event")
								{
									XmlItem.getCurItem().m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
										XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, XmlItem.getCurItem().m_xe, 0));
								}
								else
								{
									XmlItem.getCurItem().m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
										XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, XmlItem.getCurItem().m_xe, XmlItem.getCurItem().m_xe.ChildNodes.Count));
								}
							}
						}
					}
					break;
			}
		}
		private void mx_addTmpl_Click(object sender, RoutedEventArgs e)
		{
			TemplateCreate winAddtmpl = new TemplateCreate(XmlItem.getCurItem().m_xe);
			winAddtmpl.ShowDialog();
		}
		private void mx_batchUpdate_Click(object sender, RoutedEventArgs e)
		{
			MenuWin.BatchUpdate winBatchUpdate = new MenuWin.BatchUpdate(XmlItem.getCurItem());
			winBatchUpdate.ShowDialog();
		}
		private void checkOverflow(Basic ctrlFrame)
		{
			CtrlDef_T ctrlDef;
			System.Drawing.Rectangle rectFrame = new System.Drawing.Rectangle(
				ctrlFrame.m_selScreenX, ctrlFrame.m_selScreenY, ctrlFrame.m_selW, ctrlFrame.m_selH);

			if (ctrlFrame != null && ctrlFrame.m_xe != null && ctrlFrame.m_xe.Name != "" &&
				MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(ctrlFrame.m_xe.Name, out ctrlDef))
			{
				foreach (object item in ctrlFrame.Items)
				{
					if (item is Basic)
					{
						Basic ctrlItem = (Basic)item;

						if (ctrlItem.m_xe.GetAttribute("visible") != "false")
						{
							System.Drawing.Rectangle rectItem = new System.Drawing.Rectangle(
								ctrlItem.m_selScreenX, ctrlItem.m_selScreenY, ctrlItem.m_selW, ctrlItem.m_selH);

							if (ctrlItem.m_xe.Name != "event" && !rectFrame.Contains(rectItem))
							{
								Public.ResultLink.createResult("\r\n[" + ctrlItem.mx_text.Text + "]",
									Public.ResultType.RT_INFO, ctrlItem);
								Public.ResultLink.createResult(" 超出了 ", Public.ResultType.RT_INFO);
								Public.ResultLink.createResult("[" + ctrlFrame.mx_text.Text + "]",
									Public.ResultType.RT_INFO, ctrlFrame);
								Public.ResultLink.createResult(" 的范围。", Public.ResultType.RT_INFO, ctrlItem);
							}
							checkOverflow(ctrlItem);
						}
					}
				}
			}
		}
		private void mx_checkOverflow_Click(object sender, RoutedEventArgs e)
		{
			ResultLink.createResult("\r\n开始检测溢出的控件");
			if (XmlItem.getCurItem() != null && XmlItem.getCurItem() is Basic)
			{
				XmlControl.getCurXmlControl().refreshVRect();
				checkOverflow((Basic)XmlItem.getCurItem());
			}
			ResultLink.createResult("\r\n检测结束");
		}
		private void mx_checkBaseId_Click(object sender, RoutedEventArgs e)
		{
			ResultLink.createResult("\r\n开始检测重复的baseID");
			if (XmlControl.getCurXmlControl() != null)
			{
				XmlControl.getCurXmlControl().checkAllUICtrlBaseId();
			}
			ResultLink.createResult("\r\n检测结束");
		}
		private void mx_shrinkChildren_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().IsExpanded = true;

			foreach (object obj in XmlItem.getCurItem().Items)
			{
				if (obj is TreeViewItem)
				{
					TreeViewItem item = (TreeViewItem)obj;

					item.IsExpanded = false;
				}
			}
		}

		private void mx_cut_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().cutItem();
		}
		private void mx_copy_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().copyItem();
		}
		private void mx_paste_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().pasteItem();
		}
		private void mx_delete_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().deleteItem();
		}
		private void mx_moveUp_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().moveUpItem();
		}
		private void mx_moveDown_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().moveDownItem();
		}
		private void mx_moveToParent_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().moveToParent();
		}
		private void mx_moveToChild_Click(object sender, RoutedEventArgs e)
		{
			XmlItem.getCurItem().moveToChild();
		}

		public void refreshItemMenu()
		{
			if (XmlItem.getCurItem() == null)
			{
				return;
			}
			if (XmlItem.getCurItem().m_xe == XmlItem.getCurItem().m_xe.OwnerDocument.DocumentElement)
			{
				mx_cut.IsEnabled = false;
				mx_copy.IsEnabled = false;
				mx_delete.IsEnabled = false;
				mx_moveUp.IsEnabled = false;
				mx_moveDown.IsEnabled = false;
				mx_moveToParent.IsEnabled = false;
				mx_moveToChild.IsEnabled = false;
			}
			else
			{
				mx_cut.IsEnabled = true;
				mx_copy.IsEnabled = true;
				mx_delete.IsEnabled = true;
				mx_moveUp.IsEnabled = true;
				mx_moveDown.IsEnabled = true;
				if (XmlItem.getCurItem().m_xe.ParentNode == XmlItem.getCurItem().m_xe.OwnerDocument.DocumentElement)
				{
					mx_moveToParent.IsEnabled = false;
				}
				else
				{
					mx_moveToParent.IsEnabled = true;
				}
				CtrlDef_T ctrlDef;

				if (XmlItem.getCurItem().m_xe.NextSibling != null && MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(XmlItem.getCurItem().m_xe.NextSibling.Name, out ctrlDef))
				{
					mx_moveToChild.IsEnabled = true;
				}
				else
				{
					mx_moveToChild.IsEnabled = false;
				}
			}
			switch (XmlItem.getCurItem().m_type)
			{
				case "CtrlUI":
					{
						#region
						if (XmlItem.getCurItem().m_xe == XmlItem.getCurItem().m_xe.OwnerDocument.DocumentElement)
						{
							if (MainWindow.s_pW.m_xePaste != null)
							{
								CtrlDef_T panelCtrlDef;

								if (MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(MainWindow.s_pW.m_xePaste.Name, out panelCtrlDef))
								{
									mx_paste.IsEnabled = true;
								}
								else
								{
									mx_paste.IsEnabled = false;
								}
							}
							else
							{
								mx_paste.IsEnabled = false;
							}
						}
						else
						{
							if (MainWindow.s_pW.m_xePaste != null)
							{
								mx_paste.IsEnabled = true;
							}
							else
							{
								mx_paste.IsEnabled = false;
							}

							CtrlDef_T panelCtrlDef;

							if (MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(XmlItem.getCurItem().m_xe.Name, out panelCtrlDef))
							{
								mx_checkOverflow.IsEnabled = true;
								mx_batchUpdate.IsEnabled = true;
							}
						}
						mx_checkBaseId.IsEnabled = true;
						#endregion
					}
					break;
				case "Skin":
					{
						#region
						mx_moveToParent.IsEnabled = false;
						mx_moveToChild.IsEnabled = false;
						if (MainWindow.s_pW.m_xePaste != null)
						{
							mx_paste.IsEnabled = true;
						}
						else
						{
							mx_paste.IsEnabled = false;
						}
						#endregion
					}
					break;
				default:
					break;
			}
		}
		private bool checkMenuItemActive(MenuItem item)
		{
			if (item != null && item.IsEnabled == true && item.Visibility == System.Windows.Visibility.Visible)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public bool canCut()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_cut);
		}
		public bool canCopy()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_copy);
		}
		public bool canPaste()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_paste);
		}
		public bool canDelete()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_delete);
		}
		public bool canMoveUp()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_moveUp);
		}
		public bool canMoveDown()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_moveDown);
		}
		public bool canMoveToParent()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_moveToParent);
		}
		public bool canMoveToChild()
		{
			refreshItemMenu();

			return checkMenuItemActive(mx_moveToChild);
		}

		public void showTmplGroup(string addStr)
		{
			MenuItem ctrlMenuItem = new MenuItem();
			bool isNull = true;

			MainWindow.s_pW.m_strDic.getNameAndTip(ctrlMenuItem, StringDic.conf_ctrlTipDic, addStr);
			if (MainWindow.s_pW.m_docConf.SelectSingleNode("Config").SelectSingleNode("template") != null &&
				MainWindow.s_pW.m_docConf.SelectSingleNode("Config").SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls") != null)
			{
				isNull = false;
				XmlElement xeTmpls = (XmlElement)MainWindow.s_pW.m_docConf.SelectSingleNode("Config").SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls");

				showTmpl(ctrlMenuItem, xeTmpls, addStr, insertCtrlItem_Click);
			}

			if (addStr == "event")
			{
				CtrlDef_T ctrlDef;
				XmlNode xnTmpls = MainWindow.s_pW.m_docConf.SelectSingleNode("Config").SelectSingleNode("template");

				if (xnTmpls != null)
				{
					if (MainWindow.s_pW.m_mapCtrlDef.TryGetValue(XmlItem.getCurItem().m_xe.Name, out ctrlDef) && ctrlDef != null)
					{
						//控件节点的事件模板
						if (ctrlDef.m_hasPointerEvent)
						{
							XmlNode xnPoi = xnTmpls.SelectSingleNode("eventTmpls_pointer");

							if (xnPoi != null && xnPoi.NodeType == XmlNodeType.Element)
							{
								isNull = false;
								XmlElement xePoi = (XmlElement)xnPoi;

								showTmpl(ctrlMenuItem, xePoi, addStr, insertCtrlItem_Click);
							}
						}
						XmlNode xnBasic = xnTmpls.SelectSingleNode("eventTmpls_basic");

						if (xnBasic != null && xnBasic.NodeType == XmlNodeType.Element)
						{
							isNull = false;
							XmlElement xeBasic = (XmlElement)xnBasic;

							showTmpl(ctrlMenuItem, xeBasic, addStr, insertCtrlItem_Click);
						}
					}
					//所有节点的事件模板
					XmlNode xnCtrl = xnTmpls.SelectSingleNode("eventTmpls_" + XmlItem.getCurItem().m_xe.Name);

					if (xnCtrl != null && xnCtrl.NodeType == XmlNodeType.Element)
					{
						isNull = false;
						XmlElement xeCtrl = (XmlElement)xnCtrl;

						showTmpl(ctrlMenuItem, xeCtrl, addStr, insertCtrlItem_Click);
					}
				}
			}

			if (Project.Setting.s_docProj.SelectSingleNode("BoloUIProj").SelectSingleNode("template") != null &&
				Project.Setting.s_docProj.SelectSingleNode("BoloUIProj").SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls") != null)
			{
				if (!isNull)
				{
					ctrlMenuItem.Items.Add(new Separator());
				}
				isNull = false;

				XmlElement xeTmpls = (XmlElement)Project.Setting.s_docProj.SelectSingleNode("BoloUIProj").SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls");

				showTmpl(ctrlMenuItem, xeTmpls, addStr, insertCtrlItem_Click);
			}

			if (isNull)
			{
				ctrlMenuItem.Click += insertCtrlItem_Click;
			}
			mx_addNode.Items.Add(ctrlMenuItem);
		}


		static private void showTmpl(MenuItem ctrlMenuItem, XmlElement xeTmpls, string addStr, RoutedEventHandler rehClick)
		{
			if (ctrlMenuItem.Items.Count == 0)
			{
				MenuItem emptyCtrl = new MenuItem();

				emptyCtrl.Header = "空节点";
				emptyCtrl.ToolTip = addStr;
				emptyCtrl.Click += rehClick;
				ctrlMenuItem.Items.Add(emptyCtrl);
				ctrlMenuItem.Items.Add(new Separator());
			}

			XmlNodeList xlstTmpl = xeTmpls.SelectNodes("row");
			if (xlstTmpl.Count == 0)
			{
				MenuItem disTmpl = new MenuItem();

				disTmpl.Header = "<没有模板>";
				disTmpl.IsEnabled = false;
				ctrlMenuItem.Items.Add(disTmpl);
			}
			else
			{
				foreach (XmlNode xn in xlstTmpl)
				{
					if (xn.NodeType == XmlNodeType.Element)
					{
						XmlElement xeRow = (XmlElement)xn;
						BoloUI.CtrlTemplate rowTmpl = new BoloUI.CtrlTemplate();
						XmlDocument docXml = new XmlDocument();

						try
						{
							docXml.LoadXml(xeRow.InnerXml);
						}
						catch
						{
							rowTmpl.ToolTip = xeRow.InnerXml;
							rowTmpl.Header = xeRow.GetAttribute("name");
							ctrlMenuItem.Items.Add(rowTmpl);
							rowTmpl.Click += rehClick;

							continue;
						}

						StringBuilder strb = new StringBuilder();
						using (StringWriter sw = new StringWriter(strb))
						{
							XmlWriterSettings settings = new XmlWriterSettings();

							settings.Indent = true;
							settings.IndentChars = "    ";
							settings.NewLineOnAttributes = false;
							XmlWriter xmlWriter = XmlWriter.Create(sw, settings);
							docXml.Save(xmlWriter);
							xmlWriter.Close();
						}
						string outStr = strb.ToString();

						outStr = outStr.Substring(outStr.IndexOf("\n") + 1, outStr.Length - (outStr.IndexOf("\n") + 1));
						rowTmpl.ToolTip = outStr;
						rowTmpl.Header = xeRow.GetAttribute("name");
						ctrlMenuItem.Items.Add(rowTmpl);
						rowTmpl.Click += rehClick;
					}
				}
			}
		}
		static private ComboBoxItem showTmpl(ComboBox cbItem, XmlElement xeTmpls, string addStr, RoutedEventHandler rehClick, string rowId = "")
		{
			ComboBoxItem retItemFrame = null;

			if (cbItem.Items.Count == 0)
			{
				ComboBoxItem emptyCtrl = new ComboBoxItem();

				emptyCtrl.Content = "空节点";
				emptyCtrl.ToolTip = addStr;
				emptyCtrl.Selected += rehClick;
				cbItem.Items.Add(emptyCtrl);
				cbItem.Items.Add(new Separator());
			}

			XmlNodeList xlstTmpl = xeTmpls.SelectNodes("row");
			if (xlstTmpl.Count != 0)
			{
				foreach (XmlNode xn in xlstTmpl)
				{
					if (xn.NodeType == XmlNodeType.Element)
					{
						XmlElement xeRow = (XmlElement)xn;
						ComboBoxItem rowTmpl = new ComboBoxItem();
						XmlDocument docXml = new XmlDocument();

						try
						{
							docXml.LoadXml(xeRow.InnerXml);
						}
						catch
						{
							rowTmpl.ToolTip = xeRow.InnerXml;
							rowTmpl.Content = xeRow.GetAttribute("name");
							cbItem.Items.Add(rowTmpl);
							rowTmpl.Selected += rehClick;
							if (rowTmpl.Content.ToString() == rowId)
							{
								retItemFrame = rowTmpl;
							}

							continue;
						}

						StringBuilder strb = new StringBuilder();
						using (StringWriter sw = new StringWriter(strb))
						{
							XmlWriterSettings settings = new XmlWriterSettings();

							settings.Indent = true;
							settings.IndentChars = "    ";
							settings.NewLineOnAttributes = false;
							XmlWriter xmlWriter = XmlWriter.Create(sw, settings);
							docXml.Save(xmlWriter);
							xmlWriter.Close();
						}
						string outStr = strb.ToString();

						outStr = outStr.Substring(outStr.IndexOf("\n") + 1, outStr.Length - (outStr.IndexOf("\n") + 1));
						rowTmpl.ToolTip = outStr;
						rowTmpl.Content = xeRow.GetAttribute("name");
						cbItem.Items.Add(rowTmpl);
						rowTmpl.Selected += rehClick;
						if (rowTmpl.Content.ToString() == rowId)
						{
							retItemFrame = rowTmpl;
						}
					}
				}
			}

			return retItemFrame;
		}
		static private object showTmpl(ItemsControl itemFrame, XmlElement xeTmpls, string addStr, RoutedEventHandler rehClick, string rowId = "")
		{
			if (itemFrame is MenuItem)
			{
				showTmpl((MenuItem)itemFrame, xeTmpls, addStr, rehClick);

				return null;
			}
			else if (itemFrame is ComboBox)
			{
				return showTmpl((ComboBox)itemFrame, xeTmpls, addStr, rehClick, rowId);
			}

			return null;
		}
		static public object showTmplGroup(string addStr, ItemsControl itemFrame, RoutedEventHandler rehClick, string rowId = "")
		{
			object retItemFrame = null;

			if (MainWindow.s_pW.m_docConf.SelectSingleNode("Config").SelectSingleNode("template") != null &&
				MainWindow.s_pW.m_docConf.SelectSingleNode("Config").SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls") != null)
			{
				XmlElement xeTmpls = (XmlElement)MainWindow.s_pW.m_docConf.SelectSingleNode("Config").
					SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls");
				retItemFrame = showTmpl(itemFrame, xeTmpls, addStr, rehClick, rowId);
			}

			if (Project.Setting.s_docProj.SelectSingleNode("BoloUIProj").SelectSingleNode("template") != null &&
				Project.Setting.s_docProj.SelectSingleNode("BoloUIProj").SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls") != null)
			{
				XmlElement xeTmpls = (XmlElement)Project.Setting.s_docProj.SelectSingleNode("BoloUIProj").
					SelectSingleNode("template").SelectSingleNode(addStr + "Tmpls");
				object ret = showTmpl(itemFrame, xeTmpls, addStr, rehClick, rowId);

				if (ret != null)
				{
					retItemFrame = ret;
				}
			}

			return retItemFrame;
		}
	}
}
