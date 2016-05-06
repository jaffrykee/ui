using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using UIEditor.Project;
using UIEditor.Project.PlugIn;
using UIEditor.XmlOperation;

namespace UIEditor.BoloUI
{
	public partial class XmlItem : TreeViewItem
	{
		public XmlControl m_xmlCtrl;
		public XmlElement m_xe;
		public bool m_isCtrl;
		public string m_type;
		public string m_configModName;
		public string m_configPartName;

		public string m_apprPre;
		public string m_apprTagStr;
		public string m_apprSuf;

		public Run m_runXeName;
		public EventLock m_selLock;

		static private XmlItemContextMenu st_menu;
		static public XmlItemContextMenu s_menu
		{
			get
			{
				if(st_menu == null)
				{
					st_menu = new XmlItemContextMenu();
				}

				if (XmlItem.getCurItem() == null)
				{
					return null;
				}
				else
				{
					return st_menu;
				}
			}
		}

		public XmlItem()
		{
		}
		public XmlItem(XmlElement xe, XmlControl rootControl)
		{
			m_runXeName = null;
			m_selLock = new EventLock();
			InitializeComponent();
			m_xmlCtrl = rootControl;
			m_xe = xe;

			if (m_xmlCtrl != null && m_xmlCtrl.m_mapXeItem != null)
			{
				m_xmlCtrl.m_mapXeItem[xe] = this;
			}
		}

		static public XmlItem getCurItem()
		{
			OpenedFile fileDef;

			if(MainWindow.s_pW != null && MainWindow.s_pW.m_curFile != null && MainWindow.s_pW.m_curFile != "" &&
				MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(MainWindow.s_pW.m_curFile, out fileDef) &&
				fileDef != null && fileDef.m_frame != null && fileDef.m_frame is XmlControl)
			{
				XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;

				if(xmlCtrl.m_curItem != null)
				{
					return xmlCtrl.m_curItem;
				}
			}

			return null;
		}
		static public XmlItem getAniFrameItem(int index)
		{
			if (index < 0)
			{
				return null;
			}

			XmlItem curItem = XmlItem.getCurItem();
			XmlItem retItem = null;

			if (curItem != null)
			{
				if (curItem is ResBasic)
				{
					ResBasic curResItem = (ResBasic)curItem;
					ResBasic retGroup = null;

					switch (curItem.m_xe.Name)
					{
						//特效关键帧的运动轨迹绘制
						case "particleShape":
							{
								if (curResItem.Items.Count > 0 && curResItem.Items[0] != null &&
									curResItem.Items[0] is ResBasic && ((ResBasic)curResItem.Items[0]).m_xe.Name == "particleKeyFrameGroup")
								{
									retGroup = (ResBasic)curResItem.Items[0];
								}
							}
							break;
						case "particleKeyFrameGroup":
							{
								retGroup = curResItem;
							}
							break;
						case "particleKeyFrame":
							{
								if (curResItem.Parent != null && curResItem.Parent is ResBasic &&
									((ResBasic)curResItem.Parent).m_xe.Name == "particleKeyFrameGroup")
								{
									retGroup = (ResBasic)curResItem.Parent;
								}
							}
							break;
						default:
							break;
					}
					if (retGroup != null)
					{
						if (retGroup.Items.Count > index && retGroup.Items[index] != null && retGroup.Items[index] is ResBasic)
						{
							retItem = (ResBasic)retGroup.Items[index];

							if (retItem.m_xe.Name == "particleKeyFrame")
							{
								return retItem;
							}
						}
					}
				}
				else if (curItem is Basic)
				{
					Basic curBasicItem = (Basic)curItem;
					Basic retGroup = null;

					switch (curItem.m_xe.Name)
					{
						//特效关键帧的运动轨迹绘制
						case "event":
							{
								if (curBasicItem.Items.Count > 0 && curBasicItem.Items[0] != null &&
									curBasicItem.Items[0] is Basic && ((Basic)curBasicItem.Items[0]).m_xe.Name == "controlAnimation")
								{
									retGroup = (Basic)curBasicItem.Items[0];
								}
							}
							break;
						case "controlAnimation":
							{
								retGroup = curBasicItem;
							}
							break;
						case "controlFrame":
							{
								if (curBasicItem.Parent != null && curBasicItem.Parent is Basic &&
									((Basic)curBasicItem.Parent).m_xe.Name == "controlAnimation")
								{
									retGroup = (Basic)curBasicItem.Parent;
								}
							}
							break;
						default:
							break;
					}
					if (retGroup != null)
					{
						if (retGroup.Items.Count > index && retGroup.Items[index] != null && retGroup.Items[index] is Basic)
						{
							retItem = (Basic)retGroup.Items[index];

							if (retItem.m_xe.Name == "controlFrame")
							{
								return retItem;
							}
						}
					}
				}
			}

			return null;
		}
		static public void updateParticleKeyFrameFromG2WData(string msgData)
		{
			string[] sArray = Regex.Split(msgData, ":", RegexOptions.IgnoreCase);
			XmlItem curAniFrame = null;
			int index;
			List<HistoryNode> lstOptNode = new List<HistoryNode>();

			if (int.TryParse(sArray[0], out index))
			{
				curAniFrame = getAniFrameItem(index);
			}
			if (curAniFrame == null)
			{
				return;
			}

			for (int i = 1; i < sArray.Length; i++)
			{
				if (sArray[i] == null || sArray[i] == "")
				{
					continue;
				}
				switch (i)
				{
					case 1:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "X", curAniFrame.m_xe.GetAttribute("X"), sArray[i]));
						}
						break;
					case 2:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "Y", curAniFrame.m_xe.GetAttribute("Y"), sArray[i]));
						}
						break;
					case 3:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "time", curAniFrame.m_xe.GetAttribute("time"), sArray[i]));
						}
						break;
					case 4:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "perX", curAniFrame.m_xe.GetAttribute("perX"), sArray[i]));
						}
						break;
					case 5:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "perY", curAniFrame.m_xe.GetAttribute("perY"), sArray[i]));
						}
						break;

					case 6:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveType", curAniFrame.m_xe.GetAttribute("moveType"), sArray[i]));
						}
						break;
					case 7:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx1", curAniFrame.m_xe.GetAttribute("moveTypeEx1"), sArray[i]));
						}
						break;
					case 8:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx2", curAniFrame.m_xe.GetAttribute("moveTypeEx2"), sArray[i]));
						}
						break;
					case 9:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx3", curAniFrame.m_xe.GetAttribute("moveTypeEx3"), sArray[i]));
						}
						break;
					case 10:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx4", curAniFrame.m_xe.GetAttribute("moveTypeEx4"), sArray[i]));
						}
						break;

					case 11:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx5", curAniFrame.m_xe.GetAttribute("moveTypeEx5"), sArray[i]));
						}
						break;
					case 12:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx6", curAniFrame.m_xe.GetAttribute("moveTypeEx6"), sArray[i]));
						}
						break;
					case 13:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx7", curAniFrame.m_xe.GetAttribute("moveTypeEx7"), sArray[i]));
						}
						break;
					case 14:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "moveTypeEx8", curAniFrame.m_xe.GetAttribute("moveTypeEx8"), sArray[i]));
						}
						break;
					case 15:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "addPerX", curAniFrame.m_xe.GetAttribute("addPerX"), sArray[i]));
						}
						break;

					case 16:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "addPerY", curAniFrame.m_xe.GetAttribute("addPerY"), sArray[i]));
						}
						break;
					case 17:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "isKeyFrame", curAniFrame.m_xe.GetAttribute("isKeyFrame"), sArray[i]));
						}
						break;
					case 18:
						{
							lstOptNode.Add(new XmlOperation.HistoryNode(
								curAniFrame.m_xe, "isKeyHide", curAniFrame.m_xe.GetAttribute("isKeyHide"), sArray[i]));
						}
						break;
					default:
						{

						}
						break;
				}
			}
			if (lstOptNode != null && lstOptNode.Count > 0)
			{
				curAniFrame.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(lstOptNode);
			}
		}
		static public void addRowToDrawLineMsgData(XmlElement xeKeyFrame, ref string msgData)
		{
			if (msgData != null && xeKeyFrame != null)
			{
				//0
				msgData += xeKeyFrame.GetAttribute("X") + ":";
				msgData += xeKeyFrame.GetAttribute("Y") + ":";
				msgData += xeKeyFrame.GetAttribute("time") + ":";
				msgData += xeKeyFrame.GetAttribute("perX") + ":";
				msgData += xeKeyFrame.GetAttribute("perY") + ":";

				//5
				msgData += xeKeyFrame.GetAttribute("moveType") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx1") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx2") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx3") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx4") + ":";

				//10
				msgData += xeKeyFrame.GetAttribute("moveTypeEx5") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx6") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx7") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx8") + ":";
				msgData += xeKeyFrame.GetAttribute("addPerX") + ":";

				//15
				msgData += xeKeyFrame.GetAttribute("addPerY") + ":";
				msgData += "true" + ":";//isKeyFrame
				msgData += xeKeyFrame.GetAttribute("isKeyHide") + ":";
			}
		}
		static public void changeLightRun(Run runLight)
		{
			bool isLocked = false;

			if (MainWindow.s_pW.m_isCanEdit == false)
			{
				isLocked = true;
			}
			else
			{
				MainWindow.s_pW.m_isCanEdit = false;
			}
			if (MainWindow.s_pW.m_lastSelRun != null)
			{
				//<text>
				MainWindow.s_pW.m_lastSelRun.Background = null;
			}
			MainWindow.s_pW.m_lastSelRun = runLight;
			//<text>
			runLight.Background = new SolidColorBrush(XmlControl.s_arrTextColor[(int)XmlControl.XmlTextColorType.BCK_CURSEL]);
			if (isLocked == false)
			{
				MainWindow.s_pW.m_isCanEdit = true;
			}
			runLight.BringIntoView();
		}
		static public string getDefaultNewXmlItem(string xeName)
		{
			DataNode dataNode;
			DataAttrGroup dataAttrGroup;
			XmlDocument docTmp = new XmlDocument();
			XmlElement xeTmp = docTmp.CreateElement(xeName);

			if (DataNodeGroup.tryGetDataNode("BoloUI", "Ctrl", xeName, out dataNode) && dataNode != null &&
				dataNode.m_mapDataAttrGroup.TryGetValue("basic", out dataAttrGroup) && dataAttrGroup != null)
			{
				xeTmp.SetAttribute("w", "50");
				xeTmp.SetAttribute("h", "50");
				switch(xeName)
				{
					case "countDown":
						{
							xeTmp.SetAttribute("text", "%a:%b:%c:%d");
						}
						break;
					default:
						break;
				}
			}

			return xeTmp.OuterXml;
		}
		static public void addItemToCurItemByString(string xmlStr, XmlItem curItem = null)
		{
			if (curItem == null)
			{
				curItem = XmlItem.getCurItem();
				if (curItem == null)
				{
					return;
				}
			}

			XmlDocument newDoc = new XmlDocument();
			XmlElement newXe = curItem.m_xe.OwnerDocument.CreateElement("tmp");

			if (xmlStr != "")
			{
				newXe.InnerXml = xmlStr;
				if (newXe.FirstChild.NodeType == XmlNodeType.Element)
				{
					XmlItem treeChild;

					if(curItem is Basic)
					{
						treeChild = new BoloUI.Basic((XmlElement)newXe.FirstChild, curItem.m_xmlCtrl);
					}
					else if(curItem is ResBasic)
					{
						SkinDef_T skinDef;

						if(SkinDef_T.tryGetSkinDef(newXe.FirstChild.Name, out skinDef))
						{
							treeChild = new BoloUI.ResBasic((XmlElement)newXe.FirstChild, curItem.m_xmlCtrl, skinDef);
						}
						else
						{
							return;
						}
					}
					else
					{
						return;
					}

					if (treeChild != null)
					{
						if (treeChild.m_xe.Name == "event")
						{
							curItem.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, curItem.m_xe, 0));
						}
						else
						{
							curItem.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, curItem.m_xe, curItem.m_xe.ChildNodes.Count));
						}
					}
				}
			}
		}
		static public void shrinkChildren()
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

		public void gotoSelectXe()
		{
			if (m_runXeName != null &&
				m_runXeName.Parent != null &&
				m_runXeName.Parent is Paragraph)
			{
				Paragraph para = (Paragraph)m_runXeName.Parent;

				if (para.Parent != null &&
					para.Parent is FlowDocument)
				{
					FlowDocument fDoc = (FlowDocument)para.Parent;

					if (fDoc.Parent != null &&
						fDoc.Parent == MainWindow.s_pW.mx_xmlText)
					{
						MainWindow.s_pW.m_lastUpdateRun = m_runXeName;
					}
				}
			}
		}
		public virtual void changeSelectItem(object obj = null)
		{

		}
		public virtual void initHeader()
		{

		}
		protected virtual void addChild()
		{
		}

		private void mx_root_MouseUp(object sender, MouseButtonEventArgs e)
		{
			changeSelectItem();
			e.Handled = true;
		}
		private void mx_root_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			changeSelectItem();
			if (XmlItem.s_menu != null)
			{
				XmlItem.s_menu.mx_menu.PlacementTarget = this;
				XmlItem.s_menu.mx_menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
				XmlItem.s_menu.mx_menu.IsOpen = true;
			}
			e.Handled = true;
		}
		private void mx_root_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			switch (m_xe.Name)
			{
				case "skingroup":
					{
						string path = Project.Setting.s_skinPath + "\\" + m_xe.GetAttribute("Name") + ".xml";
						MainWindow.s_pW.openFileByPath(path);
					}
					break;
				case "resource":
					{
						string path = Project.Setting.s_imagePath + "\\" + m_xe.GetAttribute("name") + ".xml";
						MainWindow.s_pW.openFileByPath(path);
					}
					break;
				case "publicresource":
					{
						string path = Project.Setting.s_imagePath + "\\" + m_xe.GetAttribute("name") + ".xml";
						MainWindow.s_pW.openFileByPath(path);
					}
					break;
				case "event":
					{
						string funcValue = m_xe.GetAttribute("function");
						string[] scriptName = Regex.Split(funcValue, " ", RegexOptions.IgnoreCase);

						if (scriptName != null && scriptName[0] != "")
						{
							string scriptPath = Project.Setting.s_projPath + "\\..\\..\\scripts\\dev\\source\\";

							scriptPath = StringDic.getRealPath(scriptPath) + scriptName[0] + ".bolos";
							MainWindow.s_pW.openFileByPath(scriptPath);
						}
					}
					break;
				default:
					break;
			}
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				shrinkChildren();
			}
		}

		public void cutItem()
		{
			Clipboard.SetDataObject(m_xe.OuterXml, true);
			m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
				new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_DELETE, m_xe)
				);
		}
		public void copyItem()
		{
			Clipboard.SetDataObject(m_xe.OuterXml, true);
		}
		public void pasteItem()
		{
			XmlItem treeChild = null;
			XmlElement xeCopy;
			DataNode dataNodeCopy;
			XmlElement xeClip = null;
			string clipOutXml = Clipboard.GetText();
			XmlDocument tmpDoc = new XmlDocument();

			try
			{
				tmpDoc.LoadXml(clipOutXml);
				xeClip = tmpDoc.DocumentElement;
			}
			catch
			{
				xeClip = null;
			}

			if (xeClip != null)
			{
				xeCopy = m_xe.OwnerDocument.CreateElement("tmp");
				xeCopy.InnerXml = xeClip.OuterXml;
				xeCopy = (XmlElement)xeCopy.FirstChild;

				if (DataNodeGroup.tryGetDataNode(m_xe.OwnerDocument.DocumentElement.Name, xeClip.Name, out dataNodeCopy))
				{
					switch (dataNodeCopy.GetType().ToString())
					{
						case "UIEditor.BoloUI.DefConfig.CtrlDef_T":
							{
								treeChild = new BoloUI.Basic(xeCopy, m_xmlCtrl);
							}
							break;
						case "UIEditor.BoloUI.DefConfig.SkinDef_T":
							{
								treeChild = new BoloUI.ResBasic(xeCopy, m_xmlCtrl, (SkinDef_T)dataNodeCopy);
							}
							break;
						default:
							{

							}
							break;
					}
				}
				if (treeChild != null)
				{
					DataNode dataNode;
					DataNode dataNodeParent;

					if (DataNodeGroup.tryGetDataNode(m_xe.OwnerDocument.DocumentElement.Name, m_xe.Name, out dataNode) &&
						dataNode != null)
					{
						if(dataNode.m_hlstChildNode.Contains(xeCopy.Name))
						{
							m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, m_xe, m_xe.ChildNodes.Count));
						}
						else
						{
							if (m_xe.ParentNode != null && DataNodeGroup.tryGetDataNode(m_xe.OwnerDocument.DocumentElement.Name,
								m_xe.ParentNode.Name, out dataNodeParent))
							{
								if (dataNodeParent.m_hlstChildNode.Contains(xeCopy.Name))
								{
									m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
										treeChild.m_xe, (XmlElement)m_xe.ParentNode, XmlOperation.HistoryNode.getXeIndex(m_xe) + 1));
								}
							}
						}
					}
				}
			}
		}
		public void deleteItem()
		{
			m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
				new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_DELETE, m_xe)
				);
		}
		public void moveDownItem()
		{
			if(this.Parent is XmlItem)
			{
				XmlItem parentItem = (XmlItem)this.Parent;

				if(parentItem.Items.Count > 0 && parentItem.Items[parentItem.Items.Count - 1] != this)
				{
					object nextObject = parentItem.Items[parentItem.Items.IndexOf(this) + 1];

					if (nextObject != null && nextObject is XmlItem)
					{
						XmlItem nextChild = (XmlItem)nextObject;

						m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
							new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_MOVE,
								m_xe,
								(XmlElement)m_xe.ParentNode,
								XmlOperation.HistoryNode.getXeIndex(nextChild.m_xe)
							)
						);
					}
				}
			}
		}
		public void moveUpItem()
		{
			if (this.Parent is XmlItem)
			{
				XmlItem parentItem = (XmlItem)this.Parent;

				if (parentItem.Items.Count > 0 && parentItem.Items[0] != this)
				{
					object preObject = parentItem.Items[parentItem.Items.IndexOf(this) - 1];

					if (preObject != null && preObject is XmlItem)
					{
						XmlItem preChild = (XmlItem)preObject;

						m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
							new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_MOVE,
								m_xe,
								(XmlElement)m_xe.ParentNode,
								XmlOperation.HistoryNode.getXeIndex(preChild.m_xe)
							)
						);
					}
				}
			}
		}
		public void moveToParent()
		{
			if (m_xe.ParentNode != null && m_xe.ParentNode.ParentNode != null && m_xe.ParentNode is XmlElement &&
				m_xe.ParentNode.ParentNode is XmlElement && m_xe.ParentNode != m_xe.OwnerDocument.DocumentElement)
			{
				m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
					new XmlOperation.HistoryNode(
						XmlOperation.XmlOptType.NODE_MOVE,
						m_xe,
						(XmlElement)m_xe.ParentNode.ParentNode,
						XmlOperation.HistoryNode.getXeIndex((XmlElement)m_xe.ParentNode) + 1
					)
				);
			}
		}
		public void moveToChild()
		{
			if (this.Parent is XmlItem)
			{
				XmlItem parentItem = (XmlItem)this.Parent;

				if (parentItem.Items.Count > 0 && parentItem.Items[parentItem.Items.Count - 1] != this)
				{
					object nextObject = parentItem.Items[parentItem.Items.IndexOf(this) + 1];

					if (nextObject != null && nextObject is XmlItem)
					{
						XmlItem nextChild = (XmlItem)nextObject;

						if (CtrlDef_T.isFrame(nextChild.m_xe.Name) == true)
						{
							m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
								new XmlOperation.HistoryNode(
									XmlOperation.XmlOptType.NODE_MOVE,
									m_xe,
									nextChild.m_xe,
									0
								)
							);
						}
					}
				}
			}
		}
	}
}
