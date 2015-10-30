using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;


namespace UIEditor.XmlOperation
{
	public enum XmlOptType
	{
		NODE_INSERT,
		NODE_DELETE,
		NODE_MOVE,
		NODE_UPDATE,
		TEXT,
	}

	public class HistoryList
	{
		public MainWindow m_pW;
		public XmlControl m_xmlCtrl;
		public int m_maxSize;
		public LinkedList<HistoryNode> m_lstOpt;
		public LinkedListNode<HistoryNode> m_curNode;
		public LinkedListNode<HistoryNode> m_headNode;
		public LinkedListNode<HistoryNode> m_saveNode;

		public HistoryList(MainWindow pW, XmlControl xmlCtrl, int maxSize = 255)
		{
			m_pW = pW;
			m_xmlCtrl = xmlCtrl;
			m_maxSize = maxSize;
			m_lstOpt = new LinkedList<HistoryNode>();
			m_curNode = new LinkedListNode<HistoryNode>(null);
			m_saveNode = m_curNode;
			m_headNode = m_curNode;
			m_lstOpt.AddLast(m_curNode);
		}

		public void addOperation(HistoryNode optNode)
		{
			for (LinkedListNode<HistoryNode> iNode = m_curNode.Next; iNode != m_headNode && iNode != null; iNode = m_curNode.Next)
			{
				iNode.List.Remove(iNode);
			}
			if (m_lstOpt.Count() >= m_maxSize)
			{
				m_headNode = m_headNode.Next;
				m_headNode.List.Remove(m_headNode.Previous);
			}
			m_curNode = new LinkedListNode<HistoryNode>(optNode);
			m_lstOpt.AddLast(m_curNode);
			redoOperation(true);
			m_xmlCtrl.m_openedFile.updateSaveStatus();
		}
		static public void updateAttrToGL(XmlControl xmlCtrl, Basic uiCtrl, string attrName, string newValue)
		{
// 			if (attrName != "value" &&
// 				(attrName != "text" || MainWindow.s_pW.m_pathGlApp != MainWindow.conf_pathGlApp) &&
// 				MainWindow.s_pW.mx_isShowAll.IsChecked != true)
// 			{
// 				foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapBasicCtrlDef.ToList())
// 				{
// 					AttrDef_T attrDef;
// 
// 					if (pairCtrlDef.Value.m_mapAttrDef.TryGetValue(attrName, out attrDef))
// 					{
// 						switch (attrName)
// 						{
// 							case "dock":
// 								MainWindow.s_pW.updateGL(
// 									System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + attrName + ":" + newValue,
// 									W2GTag.W2G_NORMAL_UPDATE);
// 								MainWindow.s_pW.updateGL(
// 									System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + "w" + ":" + uiCtrl.m_xe.GetAttribute("w"),
// 									W2GTag.W2G_NORMAL_UPDATE);
// 								MainWindow.s_pW.updateGL(
// 									System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + "h" + ":" + uiCtrl.m_xe.GetAttribute("h"),
// 									W2GTag.W2G_NORMAL_UPDATE);
// 								MainWindow.s_pW.updateGL(
// 									System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + "x" + ":" + uiCtrl.m_xe.GetAttribute("x"),
// 									W2GTag.W2G_NORMAL_UPDATE);
// 								MainWindow.s_pW.updateGL(
// 									System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + "y" + ":" + uiCtrl.m_xe.GetAttribute("y"),
// 									W2GTag.W2G_NORMAL_UPDATE);
// 								break;
// 							default:
// 								MainWindow.s_pW.updateGL(
// 									System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + attrName + ":" + newValue,
// 									W2GTag.W2G_NORMAL_UPDATE);
// 								break;
// 						}
// 
// 						return;
// 					}
// 				}
// 			}
			MainWindow.s_pW.updateXmlToGL(xmlCtrl);
		}
		static public void updateAttrToGL(XmlControl xmlCtrl, string baseID, string attrName, string newValue)
		{
			if (attrName != "value" && MainWindow.s_pW.mx_isShowAll.IsChecked != true)
 			{
 				foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapBasicCtrlDef.ToList())
 				{
 					AttrDef_T attrDef;
 
 					if (pairCtrlDef.Value.m_mapAttrDef.TryGetValue(attrName, out attrDef))
					{
						MainWindow.s_pW.updateGL(
							System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + baseID + ":" + attrName + ":" + newValue,
							W2GTag.W2G_NORMAL_UPDATE);
 
 						return;
 					}
				}
 			}
			MainWindow.s_pW.updateXmlToGL(xmlCtrl);
		}
		public void redoOperation(bool isAddOpt = false)
		{
			switch (m_curNode.Value.m_optType)
			{
				case XmlOptType.NODE_INSERT:
					{
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_srcXe);
					}
					break;
				case XmlOptType.NODE_DELETE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe);
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe);
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_newSrcXe,
							m_curNode.Value.m_newIndex);
					}
					break;
				case XmlOptType.NODE_UPDATE:
					{
						HistoryNode.updateXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_attrName,
							m_curNode.Value.m_newValue);
					}
					break;
				case XmlOptType.TEXT:
					{
						HistoryNode.updateXmlText(m_xmlCtrl, m_curNode.Value.m_newDoc);
						if (!isAddOpt)
						{
							m_xmlCtrl.refreshXmlText();
						}
						return;
					}
					break;
				default:
					return;
			}
			if (m_xmlCtrl.m_isOnlySkin)
			{
				XmlElement xeView;

				if (m_xmlCtrl.m_skinViewCtrlUI != null && m_xmlCtrl.m_skinViewCtrlUI.m_xe != null)
				{
					BoloUI.ResBasic.resetXeView(m_xmlCtrl.m_skinViewCtrlUI, out xeView);
				}
				else
				{
					xeView = MainWindow.s_pW.m_xeTest;
				}
				m_pW.updateXmlToGL(m_xmlCtrl, xeView, false);
				XmlItem dstItem;

				if (m_xmlCtrl.m_mapXeItem.TryGetValue(m_curNode.Value.m_dstXe, out dstItem))
				{
					dstItem.initHeader();
				}
			}
			else
			{
				XmlItem dstItem;

				if (m_xmlCtrl.m_mapXeItem.TryGetValue(m_curNode.Value.m_dstXe, out dstItem))
				{
					dstItem.initHeader();
				}

				if (m_curNode.Value.m_optType == XmlOptType.NODE_UPDATE && dstItem != null && dstItem.m_type == "CtrlUI")
				{
					Basic ctrlItem = (Basic)dstItem;

					updateAttrToGL(m_xmlCtrl, ctrlItem, m_curNode.Value.m_attrName, m_curNode.Value.m_newValue);
				}
				else
				{
					m_pW.updateXmlToGL(m_xmlCtrl);
				}
			}

			if (!isAddOpt)
			{
				XmlItem dstItem;

				if (m_xmlCtrl.m_mapXeItem.TryGetValue(m_curNode.Value.m_dstXe, out dstItem))
				{
					if (dstItem != null)
					{
						switch (dstItem.m_type)
						{
							case "CtrlUI":
								((BoloUI.Basic)dstItem).changeSelectItem();
								m_pW.refreshAllCtrlUIHeader();
								break;
							case "Skin":
								((BoloUI.ResBasic)dstItem).changeSelectItem();
								m_pW.refreshAllSkinHeader();
								break;
							default:
								break;
						}
					}
				}
			}
			else
			{
				if (m_xmlCtrl != null && m_xmlCtrl.m_curItem != null && m_xmlCtrl.m_curItem is BoloUI.Basic)
				{
					BoloUI.Basic uiCtrl = (BoloUI.Basic)m_xmlCtrl.m_curItem;

					uiCtrl.showBlueRect();
				}
			}

			m_xmlCtrl.refreshXmlText();
		}
		static public void refreshItemHeader(XmlItem dstItem)
		{
			if (dstItem != null)
			{
				switch (dstItem.m_type)
				{
					case "CtrlUI":
						((BoloUI.Basic)dstItem).changeSelectItem();
						MainWindow.s_pW.refreshAllCtrlUIHeader();
						dstItem.initHeader();
						break;
					case "Skin":
						((BoloUI.ResBasic)dstItem).changeSelectItem();
						MainWindow.s_pW.refreshAllSkinHeader();
						dstItem.initHeader();
						break;
					default:
						break;
				}
			}
		}
		public void undoOperation()
		{
			switch (m_curNode.Value.m_optType)
			{
				case XmlOptType.NODE_INSERT:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe);
					}
					break;
				case XmlOptType.NODE_DELETE:
					{
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_srcXe,
							m_curNode.Value.m_oldIndex);
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe);
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_srcXe,
							m_curNode.Value.m_oldIndex);
					}
					break;
				case XmlOptType.NODE_UPDATE:
					{
						HistoryNode.updateXmlNode(
							m_pW,
							m_xmlCtrl.m_openedFile.m_path,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_attrName,
							m_curNode.Value.m_oldValue);
					}
					break;
				case XmlOptType.TEXT:
					{
						HistoryNode.updateXmlText(m_xmlCtrl, m_curNode.Value.m_oldDoc);
						m_xmlCtrl.refreshXmlText();
						return;
					}
					break;
				default:
					return;
			}
			XmlItem dstItem = null;

			m_xmlCtrl.m_mapXeItem.TryGetValue(m_curNode.Value.m_dstXe, out dstItem);
			if (m_curNode.Value.m_optType == XmlOptType.NODE_UPDATE && dstItem != null && dstItem.m_type == "CtrlUI")
			{
				Basic ctrlItem = (Basic)dstItem;

				updateAttrToGL(m_xmlCtrl, ctrlItem, m_curNode.Value.m_attrName, m_curNode.Value.m_oldValue);
			}
			else
			{
				m_pW.updateXmlToGL(m_xmlCtrl);
			}

			if (dstItem != null)
			{
				switch (dstItem.m_type)
				{
					case "CtrlUI":
						((BoloUI.Basic)dstItem).changeSelectItem();
						m_pW.refreshAllCtrlUIHeader();
						dstItem.initHeader();
						break;
					case "Skin":
						((BoloUI.ResBasic)dstItem).changeSelectItem();
						m_pW.refreshAllSkinHeader();
						dstItem.initHeader();
						break;
					default:
						break;
				}
			}

			m_xmlCtrl.refreshXmlText();
		}
		public void undo()
		{
			if (m_curNode != null && m_curNode.Previous != null && m_curNode != m_headNode)
			{
				undoOperation();
				m_curNode = m_curNode.Previous;
				m_xmlCtrl.m_openedFile.updateSaveStatus();
			}
		}
		public void redo()
		{
			if (m_curNode != null && m_curNode.Next != null && m_curNode.Next != m_headNode)
			{
				m_curNode = m_curNode.Next;
				redoOperation();
				m_xmlCtrl.m_openedFile.updateSaveStatus();
			}
		}
	}
}
