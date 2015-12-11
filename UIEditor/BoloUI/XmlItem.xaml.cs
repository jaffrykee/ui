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

namespace UIEditor.BoloUI
{
	public partial class XmlItem : TreeViewItem
	{
		public XmlControl m_xmlCtrl;
		public XmlElement m_xe;
		public bool m_isCtrl;
		public string m_type;

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
		}

		public void cutItem()
		{
			MainWindow.s_pW.m_xePaste = (XmlElement)m_xe.CloneNode(true);
			m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
				new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_DELETE, m_xe)
				);
		}
		public void copyItem()
		{
			MainWindow.s_pW.m_xePaste = (XmlElement)m_xe.CloneNode(true);
		}
		public void pasteItem()
		{
			XmlElement xeCopy;

			if (MainWindow.s_pW.m_xePaste != null)
			{
				CtrlDef_T ctrlPtr;
				SkinDef_T skinPtr;
				XmlItem treeChild = null;

				xeCopy = m_xe.OwnerDocument.CreateElement("tmp");
				xeCopy.InnerXml = MainWindow.s_pW.m_xePaste.OuterXml;
				xeCopy = (XmlElement)xeCopy.FirstChild;

				if (MainWindow.s_pW.m_mapCtrlDef.TryGetValue(xeCopy.Name, out ctrlPtr))
				{
					treeChild = new BoloUI.Basic(xeCopy, m_xmlCtrl);
				}
				else if (MainWindow.s_pW.m_mapSkinAllDef.TryGetValue(xeCopy.Name, out skinPtr))
				{
					treeChild = new BoloUI.ResBasic(xeCopy, m_xmlCtrl, skinPtr);
				}
				if (treeChild != null)
				{
					if (xeCopy.Name == "event")
					{
						m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
							new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, m_xe)
							);
					}
					else
					{
						if (m_type == "CtrlUI")
						{
							CtrlDef_T panelCtrlDef;

							if (m_xe.Name == "BoloUI" && MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(treeChild.m_xe.Name, out panelCtrlDef))
							{
								m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
									new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
										treeChild.m_xe, m_xe, m_xe.ChildNodes.Count));
							}
							else if (MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(m_xe.Name, out panelCtrlDef))
							{
								m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
									new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
										treeChild.m_xe, m_xe, m_xe.ChildNodes.Count));
							}
							else if (m_xe.ParentNode != null && m_xe.ParentNode.ParentNode != null && m_xe.ParentNode.ParentNode.NodeType == XmlNodeType.Element)
							{
								m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
									new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
										treeChild.m_xe, (XmlElement)m_xe.ParentNode, XmlOperation.HistoryNode.getXeIndex(m_xe) + 1));
							}
						}
						else if (m_type == "Skin")
						{
							SkinDef_T skinDef;
							if (MainWindow.s_pW.m_mapSkinAllDef.TryGetValue(m_xe.Name, out skinDef))
							{
								SkinDef_T skinChildDef;
								if (skinDef.m_mapEnChild != null && skinDef.m_mapEnChild.Count > 0
									&& skinDef.m_mapEnChild.TryGetValue(treeChild.m_xe.Name, out skinChildDef))
								{
									m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
										new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
											treeChild.m_xe, m_xe, m_xe.ChildNodes.Count)
										);
								}
								else
								{
									if (m_xe.ParentNode.NodeType == XmlNodeType.Element)
									{
										if (((XmlElement)m_xe.ParentNode).Name == "BoloUI" &&
											MainWindow.s_pW.m_mapSkinTreeDef.TryGetValue(treeChild.m_xe.Name, out skinChildDef))
										{
											m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
												new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
													treeChild.m_xe, (XmlElement)m_xe.ParentNode, XmlOperation.HistoryNode.getXeIndex(m_xe) + 1)
												);
										}
										else if (MainWindow.s_pW.m_mapSkinAllDef.TryGetValue(((XmlElement)m_xe.ParentNode).Name, out skinDef))
										{
											if (skinDef.m_mapEnChild.TryGetValue(treeChild.m_xe.Name, out skinChildDef))
											{
												m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
													new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT,
														treeChild.m_xe, (XmlElement)m_xe.ParentNode, XmlOperation.HistoryNode.getXeIndex(m_xe) + 1)
													);
											}
										}
									}
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
			if (m_xe.ParentNode.LastChild != m_xe)
			{
				m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
					new XmlOperation.HistoryNode(
						XmlOperation.XmlOptType.NODE_MOVE,
						m_xe,
						(XmlElement)m_xe.ParentNode,
						XmlOperation.HistoryNode.getXeIndex(m_xe) + 1
					)
				);
			}
		}
		public void moveUpItem()
		{
			if (m_xe.ParentNode.FirstChild != m_xe)
			{
				m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
					new XmlOperation.HistoryNode(
						XmlOperation.XmlOptType.NODE_MOVE,
						m_xe,
						(XmlElement)m_xe.ParentNode,
						XmlOperation.HistoryNode.getXeIndex(m_xe) - 1
					)
				);
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
			CtrlDef_T ctrlDef;
			if (m_xe.NextSibling != null && MainWindow.s_pW.m_mapPanelCtrlDef.TryGetValue(m_xe.NextSibling.Name, out ctrlDef))
			{
				m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
					new XmlOperation.HistoryNode(
						XmlOperation.XmlOptType.NODE_MOVE,
						m_xe,
						(XmlElement)m_xe.NextSibling,
						m_xe.NextSibling.ChildNodes.Count
						)
					);
			}
		}
	}
}
