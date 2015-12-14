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
		private LinkedListNode<HistoryNode> mt_saveNode;
		public LinkedListNode<HistoryNode> m_saveNode
		{
			get { return mt_saveNode; }
			set
			{
				mt_saveNode = value;
				if (m_xmlCtrl.isSkinXmlControl())
				{
					Project.Setting.refreshSkinIndex();
				}
			}
		}

		public HistoryList(MainWindow pW, XmlControl xmlCtrl, int maxSize = 65535)
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
			if (uiCtrl.m_xe.Name == "progress" && attrName == "value")
			{
				MainWindow.s_pW.updateGL(System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" +
					uiCtrl.m_vId + ":" + attrName + ":" + newValue, W2GTag.W2G_NORMAL_UPDATE);
				return;
			}
			if (MainWindow.s_pW.mx_isShowAll.IsChecked != true)
			{
				foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapBasicCtrlDef.ToList())
				{
					AttrDef_T attrDef;

					if (pairCtrlDef.Value.m_mapAttrDef.TryGetValue(attrName, out attrDef))
					{
						switch (attrName)
						{
							case "name":
								break;
							case "baseID":
								break;
							case "skin":
								break;
							case "angle":
								break;
							case "rotateType":
								break;
							case "ownEvt":
								break;
							case "ownDragEvt":
								break;
							case "isEffectParentAutosize":
								break;
							case "canUsedEvent":
								break;
							case "canHandleEvent":
								break;
							case "canSelectByKey":
								break;
							case "onSelectByKey":
								break;
							case "isMaskAreaByKey":
								break;
							case "canAutoBuildTopKey":
								break;
							case "canAutoBuildBottomKey":
								break;
							case "canAutoBuildLeftKey":
								break;
							case "canAutoBuildRightKey":
								break;
							case "assignTopKeyBaseID":
								break;
							case "assignBottomKeyBaseID":
								break;
							case "assignLeftKeyBaseID":
								break;
							case "assignRightKeyBaseID":
								break;
							case "enCloseAni":
								break;
							case "showStyle":
								break;
							case "movieLayer":
								break;
							case "movieSpe":
								break;
							case "movieType":
								break;
							case "aimSpeed":
								break;
							case "text":
								if (uiCtrl.m_xe.Name == "richText")
								{
									MainWindow.s_pW.updateXmlToGL(xmlCtrl);
									return;
								}
								break;
							default:
								MainWindow.s_pW.updateXmlToGL(xmlCtrl);
								return;
						}
						MainWindow.s_pW.updateGL(
							System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" + uiCtrl.m_vId + ":" + attrName + ":" + newValue,
							W2GTag.W2G_NORMAL_UPDATE);

						return;
					}
				}
			}
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
			XmlDocument docXml = null;

			if (m_curNode.Value.m_dstXe != null)
			{
				docXml = m_curNode.Value.m_dstXe.OwnerDocument;
			}
			switch (m_curNode.Value.m_optType)
			{
				case XmlOptType.NODE_INSERT:
					{
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_srcXe,
							m_curNode.Value.m_newIndex);
					}
					break;
				case XmlOptType.NODE_DELETE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe);
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe);
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_newSrcXe,
							m_curNode.Value.m_newIndex);
					}
					break;
				case XmlOptType.NODE_UPDATE:
					{
						HistoryNode.updateXmlNode(
							m_pW,
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
			if (m_curNode.Value.m_path != null && m_curNode.Value.m_path != "")
			{
				if (docXml != null)
				{
					docXml.Save(m_curNode.Value.m_path);
				}
			}

			XmlItem dstItem;

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

				if (m_xmlCtrl.m_mapXeItem.TryGetValue(m_curNode.Value.m_dstXe, out dstItem))
				{
					dstItem.initHeader();
				}
			}
			else
			{
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
				if (dstItem != null)
				{
					dstItem.changeSelectItem();
					switch (dstItem.m_type)
					{
						case "CtrlUI":
							m_pW.refreshAllCtrlUIHeader();
							break;
						case "Skin":
							m_pW.refreshAllSkinHeader();
							break;
						default:
							break;
					}
				}
			}
			else
			{
				if (dstItem != null)
				{
					dstItem.changeSelectItem();
					switch(m_curNode.Value.m_optType)
					{
							//用于添加控件中带有皮肤名或修改控件的皮肤名后，自动添加皮肤组。
						case XmlOptType.NODE_UPDATE:
							{
								if(m_curNode.Value.m_attrName == "skin")
								{
									checkSkinLink(dstItem.m_xmlCtrl, dstItem.m_xe, false);
								}
							}
							break;
						case XmlOptType.NODE_INSERT:
							{
								checkSkinLink(dstItem.m_xmlCtrl, dstItem.m_xe);
							}
							break;
						default:
							break;
					}
				}
			}

			if (m_curNode.Value.m_path != null && m_curNode.Value.m_path != "")
			{

			}
			else
			{
				if (MainWindow.s_pW.mx_showUITab.Visibility == System.Windows.Visibility.Visible &&
					MainWindow.s_pW.mx_showUITab.IsChecked == true)
				{

				}
				else
				{
					m_xmlCtrl.refreshXmlText();
				}
			}
		}
		static private void checkSkinLink(XmlControl xmlCtrl, XmlElement xeUiCtrl, bool isAll = true)
		{
			string skinName = xeUiCtrl.GetAttribute("skin");

			if (skinName != "")
			{
				Project.Setting.refreshSkinIndex();
				xmlCtrl.checkSkinLinkAndAddSkinGroup(skinName);
			}
			if (isAll)
			{
				foreach (XmlNode xn in xeUiCtrl.ChildNodes)
				{
					if (xn is XmlElement)
					{
						checkSkinLink(xmlCtrl, (XmlElement)xn, isAll);
					}
				}
			}
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
			XmlDocument docXml = null;

			if (m_curNode.Value.m_dstXe != null)
			{
				docXml = m_curNode.Value.m_dstXe.OwnerDocument;
			}
			switch (m_curNode.Value.m_optType)
			{
				case XmlOptType.NODE_INSERT:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe);
					}
					break;
				case XmlOptType.NODE_DELETE:
					{
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_srcXe,
							m_curNode.Value.m_oldIndex);
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe);
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							m_curNode.Value.m_dstXe,
							m_curNode.Value.m_srcXe,
							m_curNode.Value.m_oldIndex);
					}
					break;
				case XmlOptType.NODE_UPDATE:
					{
						HistoryNode.updateXmlNode(
							m_pW,
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
			if (m_curNode.Value.m_path != null && m_curNode.Value.m_path != "")
			{
				if (docXml != null)
				{
					docXml.Save(m_curNode.Value.m_path);
				}
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

			if (m_curNode.Value.m_path != null && m_curNode.Value.m_path != "")
			{

			}
			else
			{
				m_xmlCtrl.refreshXmlText();
			}
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
