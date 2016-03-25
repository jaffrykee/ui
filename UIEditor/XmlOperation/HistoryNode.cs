using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;

namespace UIEditor.XmlOperation
{
	public class HistoryNode
	{
		public XmlOptType m_optType;
		public XmlElement m_dstXe;
		public XmlElement m_srcXe;
		public int m_oldIndex;
		public int m_newIndex;
		public XmlElement m_newSrcXe;

		public string m_attrName;
		public string m_oldValue;
		public string m_newValue;
		public XmlDocument m_oldDoc;
		public XmlDocument m_newDoc;

		public string m_path;

		public static int getXeIndex(XmlElement xe)
		{
			int iXe = 0;

			if (xe.ParentNode != null)
			{
				foreach (XmlNode xn in xe.ParentNode.ChildNodes)
				{
					if (xn.NodeType == XmlNodeType.Element)
					{
						XmlElement xec = (XmlElement)xn;

						if (xec == xe)
						{
							break;
						}
						iXe++;
					}
				}

				return iXe;
			}
			else
			{
				return -1;
			}
		}

		public HistoryNode(XmlOptType optType, XmlElement dstXe, XmlElement srcXe = null, int newIndex = 0)
		{
			if(SkinEditor.isCurItemSkinEditor())
			{
				if (MainWindow.s_pW.mx_skinEditor.m_xmlPath != null && MainWindow.s_pW.mx_skinEditor.m_xmlPath != "")
				{
					m_path = MainWindow.s_pW.mx_skinEditor.m_xmlPath;
				}
			}
			m_optType = optType;
			m_dstXe = dstXe;
			switch (optType)
			{
				case XmlOptType.NODE_DELETE:
					{
						m_srcXe = (XmlElement)m_dstXe.ParentNode;
						int tmpIndex = getXeIndex(m_dstXe);
						if (tmpIndex < 0)
						{
							return;
						}
						else
						{
							m_oldIndex = tmpIndex;
						}
					}
					break;
				case XmlOptType.NODE_INSERT:
					{
						m_srcXe = srcXe;
						m_newIndex = newIndex;
					}
					break;
				case XmlOptType.NODE_MOVE:
					{
						m_srcXe = (XmlElement)m_dstXe.ParentNode;
						m_newSrcXe = srcXe;
						m_newIndex = newIndex;
						m_oldIndex = getXeIndex(m_dstXe);
					}
					break;
				default:
					break;
			}
		}
		public HistoryNode(XmlElement dstXe, string attrName, string oldValue, string newValue)
		{
			if (SkinEditor.isCurItemSkinEditor())
			{
				if (MainWindow.s_pW.mx_skinEditor.m_xmlPath != null && MainWindow.s_pW.mx_skinEditor.m_xmlPath != "")
				{
					m_path = MainWindow.s_pW.mx_skinEditor.m_xmlPath;
				}
			}
			//NODE_UPDATE
			m_optType = XmlOptType.NODE_UPDATE;
			m_attrName = attrName;
			m_oldValue = oldValue;
			m_newValue = newValue;
			m_dstXe = dstXe;
		}
		public HistoryNode(XmlDocument oldDoc, XmlDocument newDoc)
		{
			m_optType = XmlOptType.TEXT;
			m_oldDoc = oldDoc;
			m_newDoc = newDoc;
		}

		static public bool deleteItemByXe(MainWindow pW, XmlControl xmlCtrl, XmlElement dstXe)
		{
			bool ret = false;
			Project.PlugIn.DataNode dataNode;
			XmlItem dstItem;

			if (xmlCtrl != null && xmlCtrl.m_mapXeItem != null && xmlCtrl.m_mapXeItem.TryGetValue(dstXe, out dstItem))
			{
				if (dstItem != null)
				{
					XmlItem parentNode;

					if (dstItem.Parent != null && dstItem.Parent is XmlItem || dstItem.Parent.GetType().BaseType.ToString() == "UIEditor.BoloUI.XmlItem")
					{
						parentNode = (XmlItem)(dstItem.Parent);
					}
					else
					{
						parentNode = null;
					}

					xmlCtrl.m_mapXeItem.Remove(dstXe);
					//if (dstItem.Parent != null && dstItem.Parent is TreeViewItem)
					if (dstItem.Parent != null)
					{
						((TreeViewItem)dstItem.Parent).Items.Remove(dstItem);
						if (Project.PlugIn.DataNodeGroup.tryGetDataNode("BoloUI", "Ctrl", dstXe.Name, out dataNode) &&
							dstXe.Name != "event")
						{
							//仅Ctrl
							BoloUI.Basic uiCtrl;

							if (dstItem.m_type == "CtrlUI")
							{
								uiCtrl = (BoloUI.Basic)dstItem;
								if (xmlCtrl.m_mapCtrlUI.TryGetValue(uiCtrl.m_vId, out uiCtrl))
								{
									xmlCtrl.m_mapCtrlUI.Remove(uiCtrl.m_vId);
									ret = true;
								}
							}
						}
					}

					if (parentNode != null && parentNode.Items.Count <= 0)
					{
						parentNode.mx_imgFolder.Visibility = System.Windows.Visibility.Collapsed;
					}
				}
			}

			return ret;
		}
		static public bool insertItemByXe(MainWindow pW, XmlControl xmlCtrl, XmlElement dstXe, XmlElement srcXe,ref int index)
		{
			CtrlDef_T nullCtrlDef;

			if (xmlCtrl != null)
			{
				XmlItem dstItem;
				XmlItem srcItem = null;
				Project.PlugIn.DataNode skinPtr;

				if (srcXe.Name != "BoloUI")
				{
					if (!(xmlCtrl.m_mapXeItem.TryGetValue(srcXe, out srcItem) && srcItem != null))
					{
						return false;
					}
				}

				if (CtrlDef_T.tryGetCtrlDef(dstXe.Name, out nullCtrlDef))
				{
					dstItem = new Basic(dstXe, xmlCtrl, false);
				}
				else if (Project.PlugIn.DataNodeGroup.s_mapDataNodesDef["BoloUI"]["Skin"].m_mapDataNode.TryGetValue(dstXe.Name, out skinPtr) && skinPtr is SkinDef_T)
				{
					if (srcItem != null && srcItem is ResBasic && ((ResBasic)srcItem).m_isSkinEditor)
					{
						dstItem = new ResBasic(dstXe, xmlCtrl, (SkinDef_T)skinPtr, true);
					}
					else
					{
						dstItem = new ResBasic(dstXe, xmlCtrl, (SkinDef_T)skinPtr, false);
					}
				}
				else
				{
					return false;
				}

				if (srcItem != null && index > srcItem.Items.Count)
				{
					index = srcItem.Items.Count;
				}

				if (srcXe.Name != "BoloUI")
				{
					if (srcItem != null)
					{
						srcItem.Items.Insert(index, dstItem);
						srcItem.mx_imgFolder.Visibility = System.Windows.Visibility.Visible;
					}
					else
					{
						return false;
					}
				}
				else
				{
					if (dstItem.m_type == "CtrlUI")
					{
						if (index > xmlCtrl.m_treeUI.Items.Count)
						{
							index = xmlCtrl.m_treeUI.Items.Count;
						}
						xmlCtrl.m_treeUI.Items.Insert(index, dstItem);
					}
					else if (dstItem.m_type == "Skin")
					{
						if (index > xmlCtrl.m_treeSkin.Items.Count)
						{
							index = xmlCtrl.m_treeSkin.Items.Count;
						}
						xmlCtrl.m_treeSkin.Items.Insert(index, dstItem);
					}
				}

				return true;
			}

			return false;
		}
		//直接和xml打交道的处理和部分对于显示的刷新。
		//把dstXe加到srcXe里
		static public bool insertXmlNode(MainWindow pW, XmlControl xmlCtrl, XmlElement dstXe, XmlElement srcXe, int index = 0)
		{
			XmlElement tmpXe1 = dstXe.OwnerDocument.CreateElement("tmp1");
			XmlElement tmpXe2 = dstXe.OwnerDocument.CreateElement("tmp2");

			if(index < (srcXe.ChildNodes.Count + 1) / 2)
			{
				srcXe.PrependChild(tmpXe1);
				XmlNode iXe = tmpXe1;
				for (int i = 0; i < index; i++)
				{
					XmlNode nextXe = iXe.NextSibling;

					srcXe.ReplaceChild(tmpXe2, nextXe);
					srcXe.ReplaceChild(nextXe, tmpXe1);
					srcXe.ReplaceChild(tmpXe1, tmpXe2);
					iXe = tmpXe1;
				}
				srcXe.ReplaceChild(dstXe, tmpXe1);
			}
			else
			{
				srcXe.AppendChild(tmpXe1);
				XmlNode iXe = tmpXe1;
				for (int i = srcXe.ChildNodes.Count - 1; i > index; i--)
				{
					XmlNode prevXe = iXe.PreviousSibling;

					srcXe.ReplaceChild(tmpXe2, prevXe);
					srcXe.ReplaceChild(prevXe, tmpXe1);
					srcXe.ReplaceChild(tmpXe1, tmpXe2);
					iXe = tmpXe1;
				}
				srcXe.ReplaceChild(dstXe, tmpXe1);
			}
			if (xmlCtrl != null)
			{
				insertItemByXe(pW, xmlCtrl, dstXe, srcXe, ref index);
			}

			return false;
		}
		static public bool deleteXmlNode(MainWindow pW, XmlControl xmlCtrl, XmlElement dstXe)
		{
			if (dstXe.ParentNode != null)
			{
				dstXe.ParentNode.RemoveChild(dstXe);
			}
			if (xmlCtrl != null)
			{
				deleteItemByXe(pW, xmlCtrl, dstXe);
			}

			return false;
		}
		static public bool updateXmlNode(MainWindow pW, XmlElement dstXe, string attrName, string newValue)
		{
			string oldValue = dstXe.GetAttribute(attrName);

			if (newValue != "")
			{
				dstXe.SetAttribute(attrName, newValue);
			}
			else
			{
				dstXe.RemoveAttribute(attrName);
			}

			return false;
		}
		static public bool updateXmlText(XmlControl xmlCtrl, XmlDocument newDoc)
		{
			xmlCtrl.m_xmlDoc = newDoc;
			xmlCtrl.refreshControl();

			return false;
		}
	}
}
