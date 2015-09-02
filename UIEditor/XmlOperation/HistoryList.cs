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
						//todo
					}
					break;
				default:
					return;
			}
			if (m_xmlCtrl.m_isOnlySkin)
			{
				XmlElement xeView;

				if(m_xmlCtrl.m_skinViewCtrlUI != null && m_xmlCtrl.m_skinViewCtrlUI.m_xe != null)
				{
					BoloUI.ResBasic.resetXeView(m_xmlCtrl.m_skinViewCtrlUI.m_xe, out xeView);
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
				m_pW.updateXmlToGL(m_xmlCtrl);
				XmlItem dstItem;

				if (m_xmlCtrl.m_mapXeItem.TryGetValue(m_curNode.Value.m_dstXe, out dstItem))
				{
					dstItem.initHeader();
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
			m_xmlCtrl.refreshXmlText();
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
						//todo
					}
					break;
				default:
					return;
			}
			m_pW.updateXmlToGL(m_xmlCtrl);

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
