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
		public LinkedList<List<HistoryNode>> m_lstOpt;
		public LinkedListNode<List<HistoryNode>> m_curNode;
		public LinkedListNode<List<HistoryNode>> m_headNode;
		private LinkedListNode<List<HistoryNode>> mt_saveNode;
		public LinkedListNode<List<HistoryNode>> m_saveNode
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
			m_lstOpt = new LinkedList<List<HistoryNode>>();
			m_curNode = new LinkedListNode<List<HistoryNode>>(null);
			m_saveNode = m_curNode;
			m_headNode = m_curNode;
			m_lstOpt.AddLast(m_curNode);
		}

		public void addOperation(HistoryNode optNode)
		{
			List<HistoryNode> lstNode = new List<HistoryNode>();

			lstNode.Add(optNode);
			addOperation(lstNode);
		}
		public void addOperation(List<HistoryNode> lstNode)
		{
			if (lstNode.Count > 0)
			{
				for (LinkedListNode<List<HistoryNode>> iNode = m_curNode.Next; iNode != m_headNode && iNode != null; iNode = m_curNode.Next)
				{
					iNode.List.Remove(iNode);
				}
				if (m_lstOpt.Count() >= m_maxSize)
				{
					m_headNode = m_headNode.Next;
					m_headNode.List.Remove(m_headNode.Previous);
				}
				m_curNode = new LinkedListNode<List<HistoryNode>>(lstNode);
				m_lstOpt.AddLast(m_curNode);
				redoOperation(true);
				m_xmlCtrl.m_openedFile.updateSaveStatus();
			}
		}
		static public void updateAttrToGL(XmlControl xmlCtrl, Basic uiCtrl, string attrName, string newValue)
		{
			if (uiCtrl.m_xe.Name == "progress" && attrName == "value")
			{
				MainWindow.s_pW.updateGL(System.IO.Path.GetFileName(xmlCtrl.m_openedFile.m_path) + ":" +
					uiCtrl.m_vId + ":" + attrName + ":" + newValue, W2GTag.W2G_NORMAL_UPDATE);
				return;
			}
			MainWindow.s_pW.updateXmlToGL(xmlCtrl);
		}
		static public void updateAttrToOptList(ref List<HistoryNode> lstOptNode, XmlElement xe, string attrName, string newValue)
		{
			if (xe.GetAttribute(attrName) != newValue)
			{
				lstOptNode.Add(new XmlOperation.HistoryNode(xe, attrName, xe.GetAttribute(attrName), newValue));
			}
		}
		//处理操作节点组中单个节点的回退。
		//isGroupEnd	null:代表通常模式
		//				false:代表操作组模式未到达最后一步
		//				true:代表操作组模式到达最后一步
		private void redoStepNode(HistoryNode curStepNode, bool isAddOpt, bool? isGroupEnd)
		{
			XmlDocument docXml = null;

			if (curStepNode.m_dstXe != null)
			{
				docXml = curStepNode.m_dstXe.OwnerDocument;
			}
			switch (curStepNode.m_optType)
			{
				case XmlOptType.NODE_INSERT:
					{
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe,
							curStepNode.m_srcXe,
							curStepNode.m_newIndex);
					}
					break;
				case XmlOptType.NODE_DELETE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe);
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe);
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe,
							curStepNode.m_newSrcXe,
							curStepNode.m_newIndex);
					}
					break;
				case XmlOptType.NODE_UPDATE:
					{
						HistoryNode.updateXmlNode(
							m_pW,
							curStepNode.m_dstXe,
							curStepNode.m_attrName,
							curStepNode.m_newValue);
					}
					break;
				case XmlOptType.TEXT:
					{
						HistoryNode.updateXmlText(m_xmlCtrl, curStepNode.m_newDoc);
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
			if (curStepNode.m_path != null && curStepNode.m_path != "")
			{
				if (docXml != null)
				{
					docXml.Save(curStepNode.m_path);
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
				if (isGroupEnd != false)
				{
					m_pW.updateXmlToGL(m_xmlCtrl, xeView, false);
				}

				if (m_xmlCtrl.m_mapXeItem.TryGetValue(curStepNode.m_dstXe, out dstItem))
				{
					dstItem.initHeader();
				}
			}
			else
			{
				if (m_xmlCtrl.m_mapXeItem.TryGetValue(curStepNode.m_dstXe, out dstItem))
				{
					dstItem.initHeader();
				}

				if (isGroupEnd == null)
				{
					if (curStepNode.m_optType == XmlOptType.NODE_UPDATE && dstItem != null && dstItem.m_type == "CtrlUI")
					{
						Basic ctrlItem = (Basic)dstItem;

						updateAttrToGL(m_xmlCtrl, ctrlItem, curStepNode.m_attrName, curStepNode.m_newValue);
					}
					else
					{
						m_pW.updateXmlToGL(m_xmlCtrl);
					}
				}
				else if (isGroupEnd == true)
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
					switch (curStepNode.m_optType)
					{
						//用于添加控件中带有皮肤名或修改控件的皮肤名后，自动添加皮肤组。
						case XmlOptType.NODE_UPDATE:
							{
								if (curStepNode.m_attrName == "skin")
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

			if (curStepNode.m_path != null && curStepNode.m_path != "")
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
					if (isGroupEnd != false)
					{
						m_xmlCtrl.refreshXmlText();
					}
				}
			}
		}
		//处理整个操作节点组的撤销
		//isGroupEnd	null:代表通常模式
		//				false:代表操作组模式未到达最后一步
		//				true:代表操作组模式到达最后一步
		public void redoOperation(bool isAddOpt)
		{
			if(m_curNode.Value.Count <= 0)
			{
				return;
			}
			if (m_curNode.Value.Count == 1)
			{
				redoStepNode(m_curNode.Value[0], isAddOpt, null);
			}
			else
			{
				for (int i = 0; i < m_curNode.Value.Count; i++)
				{
					if (i < m_curNode.Value.Count - 1)
					{
						redoStepNode(m_curNode.Value[i], isAddOpt, false);
					}
					else
					{
						redoStepNode(m_curNode.Value[i], isAddOpt, true);
					}
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
		private void undoStepNode(HistoryNode curStepNode, bool? isGroupEnd)
		{
			XmlDocument docXml = null;

			if (curStepNode.m_dstXe != null)
			{
				docXml = curStepNode.m_dstXe.OwnerDocument;
			}
			switch (curStepNode.m_optType)
			{
				case XmlOptType.NODE_INSERT:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe);
					}
					break;
				case XmlOptType.NODE_DELETE:
					{
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe,
							curStepNode.m_srcXe,
							curStepNode.m_oldIndex);
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						HistoryNode.deleteXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe);
						HistoryNode.insertXmlNode(
							m_pW,
							m_xmlCtrl,
							curStepNode.m_dstXe,
							curStepNode.m_srcXe,
							curStepNode.m_oldIndex);
					}
					break;
				case XmlOptType.NODE_UPDATE:
					{
						HistoryNode.updateXmlNode(
							m_pW,
							curStepNode.m_dstXe,
							curStepNode.m_attrName,
							curStepNode.m_oldValue);
					}
					break;
				case XmlOptType.TEXT:
					{
						HistoryNode.updateXmlText(m_xmlCtrl, curStepNode.m_oldDoc);
						m_xmlCtrl.refreshXmlText();
						return;
					}
					break;
				default:
					return;
			}
			if (curStepNode.m_path != null && curStepNode.m_path != "")
			{
				if (docXml != null)
				{
					docXml.Save(curStepNode.m_path);
				}
			}
			XmlItem dstItem = null;

			m_xmlCtrl.m_mapXeItem.TryGetValue(curStepNode.m_dstXe, out dstItem);

			if (isGroupEnd == null)
			{
				if (curStepNode.m_optType == XmlOptType.NODE_UPDATE && dstItem != null && dstItem.m_type == "CtrlUI")
				{
					Basic ctrlItem = (Basic)dstItem;

					updateAttrToGL(m_xmlCtrl, ctrlItem, curStepNode.m_attrName, curStepNode.m_oldValue);
				}
				else
				{
					m_pW.updateXmlToGL(m_xmlCtrl);
				}
			}
			else if (isGroupEnd == true)
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

			if (curStepNode.m_path != null && curStepNode.m_path != "")
			{

			}
			else
			{
				if (isGroupEnd != false)
				{
					m_xmlCtrl.refreshXmlText();
				}
			}
		}
		public void undoOperation()
		{
			if (m_curNode.Value.Count <= 0)
			{
				return;
			}
			if (m_curNode.Value.Count == 1)
			{
				undoStepNode(m_curNode.Value[0], null);
			}
			else
			{
				for (int i = 0; i < m_curNode.Value.Count; i++)
				{
					if (i < m_curNode.Value.Count - 1)
					{
						undoStepNode(m_curNode.Value[i], false);
					}
					else
					{
						undoStepNode(m_curNode.Value[i], true);
					}
				}
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
				redoOperation(false);
				m_xmlCtrl.m_openedFile.updateSaveStatus();
			}
		}
	}
}
