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
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;
using UIEditor.XmlOperation.XmlAttr;
using UIEditor.Project;
using UIEditor.Project.PlugIn;

namespace UIEditor.BoloUI
{
	public class Basic : UIEditor.BoloUI.XmlItem
	{
		public const string conf_modName = "BoloUI";
		public const string conf_partName = "Ctrl";

		public int m_selScreenX;
		public int m_selScreenY;
		public int m_selRelativeX;
		public int m_selRelativeY;
		public int m_selW;
		public int m_selH;
		public string m_vId;

		public Basic(XmlElement xe, XmlControl rootControl, bool isRoot = false) : base(xe, rootControl)
		{
			m_type = "CtrlUI";
			m_configModName = conf_modName;
			m_configPartName = conf_partName;
			InitializeComponent();
			CtrlDef_T ctrlDef;
			m_isCtrl = true;

			if (isRoot == false)
			{
				if (CtrlDef_T.tryGetCtrlDef(m_xe.Name, out ctrlDef) && ctrlDef != null)
				{
					m_isCtrl = true;
				}
				else
				{
					m_isCtrl = false;
				}
				if (m_xe.Name != "event")
				{
					m_vId = System.Guid.NewGuid().ToString().Substring(10);
					m_xmlCtrl.m_mapCtrlUI[m_vId] = this;
					IsExpanded = true;
				}
				else
				{
					m_vId = "";
				}
				addChild();
			}
			else
			{
				IsExpanded = true;
			}
			initHeader();
			if (m_isCtrl && m_xe.Name != "scriptPanel")
			{
				string skinName = m_xe.GetAttribute("skin");

				if (skinName != "")
				{
					string xmlPath = null;
					object retSkin = m_xmlCtrl.findSkin(skinName, out xmlPath);

					if (retSkin == null)
					{
						Public.ResultLink.createResult(
							"\r\n" + m_xmlCtrl.m_openedFile.m_path + " - [" + this.mx_text.Text + "] 无法找到皮肤：\"" + skinName + "\"",
							Public.ResultType.RT_ERROR, this);
					}
				}
			}
		}

		override protected void addChild()
		{
			XmlNodeList xnl = m_xe.ChildNodes;
			foreach (XmlNode xnf in xnl)
			{
				if (xnf.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xnf;
					CtrlDef_T ctrlPtr;

					if (CtrlDef_T.tryGetCtrlDef(xe.Name, out ctrlPtr))
					{
						this.Items.Add(new Basic(xe, m_xmlCtrl, false));
						mx_imgFolder.Visibility = System.Windows.Visibility.Visible;
					}
					else
					{
						switch (xe.Name)
						{
							default:
								break;
						}
					}
				}
			}
		}
		public override void initHeader()
		{
			if(m_xe.Name != "BoloUI")
			{
				string ctrlName = MainWindow.s_pW.m_strDic.getWordByKey(m_xe.Name);
				string ctrlTip = MainWindow.s_pW.m_strDic.getWordByKey(m_xe.Name, StringDic.conf_ctrlTipDic);
				string name = "", id = "";
				string tmpCon = "";

				if (ctrlName != "")
				{
					tmpCon = ctrlName;
				}
				else
				{
					tmpCon = m_xe.Name;
				}

				if (ctrlTip != "")
				{
					this.ToolTip = m_xe.Name + "\r\n" + ctrlTip;
				}
				else
				{
					this.ToolTip = m_xe.Name;
				}

				if (m_isCtrl && m_xe.Name != "event")
				{
					name = m_xe.GetAttribute("name");
					id = m_xe.GetAttribute("baseID");
				}
				else
				{
					tmpCon = m_xe.Name;
					if (m_xe.GetAttribute("Name") != "")
					{
						name = m_xe.GetAttribute("Name");
					}
					else if (m_xe.GetAttribute("type") != "")
					{
						name = m_xe.GetAttribute("type");
					}

					if (m_xe.GetAttribute("id") != "")
					{
						id = m_xe.GetAttribute("id");
					}
					else if (m_xe.GetAttribute("function") != "")
					{
						id = m_xe.GetAttribute("function");
					}
				}

				if (MainWindow.s_pW.m_vCtrlName)
				{
					tmpCon += "<" + name + ">";
				}
				if (MainWindow.s_pW.m_vCtrlId)
				{
					tmpCon += id;
				}

				mx_text.Text = tmpCon;
			}
		}
		public bool showBlueRect()
		{
			if (MainWindow.s_pW.mx_isShowCtrlRect.IsChecked == true &&
				m_vId != null &&
				m_vId != "" &&
				m_xmlCtrl != null &&
				m_xmlCtrl.m_openedFile != null &&
				m_xmlCtrl.m_openedFile.m_path != null)
			{
				MainWindow.s_pW.updateGL(
					StringDic.getFileNameWithoutPath(m_xmlCtrl.m_openedFile.m_path) + ":" + m_vId,
					W2GTag.W2G_SELECT_UI
				);
				MainWindow.s_pW.mb_status1 = "当前控件屏幕坐标： (" + m_selScreenX + ", " + m_selScreenY +
					")    相对坐标： (" + m_selRelativeX + ", " + m_selRelativeY + ")    大小： " + m_selW + "x" + m_selH;

				return true;
			}

			return false;
		}
		public override void changeSelectItem(object obj = null)
		{
			bool stackLock;
			if (m_selLock.isLock())
			{
				return;
			}
			else
			{
				m_selLock.addLock(out stackLock);
			}
			MainWindow.s_pW.openFileByDef(m_xmlCtrl.m_openedFile);
			if(m_xe.Name != "event")
			{
				MainWindow.s_pW.mx_treeFrame.SelectedItem = MainWindow.s_pW.mx_treeFrameUI;
			}

			showBlueRect();

			m_xmlCtrl.m_curItem = this;
			AttrList.hiddenAllAttr();
			//CtrlDef_T ctrlDef;
			DataNode dataNode;

			if (DataNodeGroup.tryGetDataNode(m_xe.OwnerDocument.DocumentElement.Name, "Ctrl", m_xe.Name, out dataNode) && dataNode != null)
			{
				foreach(KeyValuePair<string, DataAttrGroup> pairGroup in dataNode.m_mapDataAttrGroup.ToList())
				{
					pairGroup.Value.m_uiAttrList.Visibility = Visibility.Visible;
					pairGroup.Value.m_uiAttrList.clearRowValue();
				}

				foreach (KeyValuePair<string, DataAttrGroup> pairGroup in dataNode.m_mapDataAttrGroup.ToList())
				{
					if (pairGroup.Value.m_uiAttrList != null)
					{
						pairGroup.Value.m_uiAttrList.refreshRowVisible();
						pairGroup.Value.m_uiAttrList.m_xmlCtrl = m_xmlCtrl;
						pairGroup.Value.m_uiAttrList.m_basic = this;
						pairGroup.Value.m_uiAttrList.m_xe = m_xe;
					}
				}

				foreach (XmlAttribute attr in m_xe.Attributes)
				{
					DataAttr attrDef;
					bool tmpFound = false;

					if (dataNode.tryGetAttrDef(attr.Name, out attrDef))
					{
						attrDef.m_iAttrRowUI.m_preValue = attr.Value;
					}
					else
					{
						//<inc>抛异常
					}
				}
			}

			XmlElement xeCurCheck = null;
			XmlElement xeCtrl = null;
			XmlItem ctrlItem = null;
			bool isFound = true;

			switch (m_xe.Name)
			{
				//特效关键帧的运动轨迹绘制
				case "event":
					{
						if (m_xe.ParentNode != null && m_xe.ParentNode is XmlElement)
						{
							xeCurCheck = m_xe;
							xeCtrl = (XmlElement)m_xe.ParentNode;
							ctrlItem = m_xmlCtrl.m_mapXeItem[xeCtrl];
						}
					}
					break;
				case "controlAnimation":
					{
						if (m_xe.ParentNode != null && m_xe.ParentNode is XmlElement &&
							m_xe.ParentNode.ParentNode != null && m_xe.ParentNode.ParentNode is XmlElement)
						{
							xeCurCheck = (XmlElement)m_xe.ParentNode;
							xeCtrl = (XmlElement)m_xe.ParentNode.ParentNode;
							ctrlItem = m_xmlCtrl.m_mapXeItem[xeCtrl];
						}
					}
					break;
				case "controlFrame":
					{
						if (m_xe.ParentNode != null && m_xe.ParentNode.ParentNode != null && m_xe.ParentNode.ParentNode is XmlElement &&
							m_xe.ParentNode.ParentNode.ParentNode != null && m_xe.ParentNode.ParentNode.ParentNode is XmlElement)
						{
							xeCurCheck = (XmlElement)m_xe.ParentNode.ParentNode;
							xeCtrl = (XmlElement)m_xe.ParentNode.ParentNode.ParentNode;
							ctrlItem = m_xmlCtrl.m_mapXeItem[xeCtrl];
						}
					}
					break;
				default:
					{
						isFound = false;
						clearControlKeyFrameDrawData();
					}
					break;
			}
			if (isFound == true && ctrlItem != null && ctrlItem is Basic && xeCurCheck != null)
			{
				sendControlKeyFrameDrawData(xeCurCheck, ((Basic)ctrlItem).m_vId);
			}

			SelButton selBn;

			if (MainWindow.s_pW.m_mapXeSel != null && MainWindow.s_pW.m_mapXeSel.TryGetValue(m_xe, out selBn) && selBn != null)
			{
				if (selBn.mx_radio.IsChecked != true)
				{
					selBn.mx_radio.IsChecked = true;
				}
			}
			BringIntoView();
			gotoSelectXe();
			ResBasic.clearKeyFrameDrawData();
			AttrList.selectLastAttrList();
			if(MainWindow.s_pW.mx_skinEditor != null)
			{
				MainWindow.s_pW.mx_skinEditor.refreshSkinEditor(this);
			}
			this.IsSelected = true;

			m_selLock.delLock(ref stackLock);
		}
		public void sendControlKeyFrameDrawData(XmlElement xeParticleShape, string baseId)
		{
			string msgData;
			XmlNode xnGroup = xeParticleShape.SelectSingleNode("controlAnimation");

			msgData = "true:" + baseId + ":";
			if (xnGroup != null && xnGroup is XmlElement)
			{
				XmlElement xeGroup = (XmlElement)xnGroup;

				foreach (XmlNode xnFrame in xeGroup.SelectNodes("controlFrame"))
				{
					if (xnFrame is XmlElement)
					{
						XmlElement xeFrame = (XmlElement)xnFrame;

						addRowToDrawLineMsgData(xeFrame, ref msgData);
					}
				}
				MainWindow.s_pW.updateGL(msgData, W2GTag.W2G_DRAW_CONTROL_LINE);
			}
		}
		static public void clearControlKeyFrameDrawData()
		{
			string msgData = "false";

			MainWindow.s_pW.updateGL(msgData, W2GTag.W2G_DRAW_CONTROL_LINE);
		}
		static public void expandAllTreeItemParent(TreeViewItem childItem)
		{
			childItem.IsExpanded = true;
			if (childItem.Parent is TreeViewItem ||
				childItem.Parent.GetType().BaseType.ToString() == "System.Windows.Controls.TreeViewItem" ||
				childItem.Parent.GetType().BaseType.BaseType.ToString() == "System.Windows.Controls.TreeViewItem")
			{
				TreeViewItem newChild = (TreeViewItem)childItem.Parent;

				expandAllTreeItemParent(newChild);
			}
		}
		public bool isIncludeSkinGroup(string groupName)
		{
			if (groupName == null || groupName == "")
			{
				return false;
			}
			foreach(XmlNode xnGroup in m_xe.OwnerDocument.SelectNodes("skingroup"))
			{
				if(xnGroup is XmlElement && ((XmlElement)xnGroup).GetAttribute("Name") == groupName)
				{
					return true;
				}
			}

			return false;
		}
		public XmlElement getLinkSkinXe()
		{
			XmlElement xeRet = null;

			//m_xmlCtrl.refreshSkinDicForAll();
			if (m_xe != null && m_xe.GetAttribute("skin") != "")
			{
				string skinName = m_xe.GetAttribute("skin");
				List<SkinUsingCount_T> lstIncludeSkinCount = m_xmlCtrl.getIncludeSkinGroupList(skinName);

				if (lstIncludeSkinCount != null && lstIncludeSkinCount.Count >= 0)
				{
					string groupName = lstIncludeSkinCount[0].m_groupName;
					string path = Project.Setting.s_skinPath + "\\" + groupName + ".xml";

					if (System.IO.File.Exists(path))
					{
						XmlDocument docSkin = new XmlDocument();

						docSkin.Load(path);
						foreach (XmlNode xn in docSkin.DocumentElement.ChildNodes)
						{
							if (xn.NodeType == XmlNodeType.Element)
							{
								XmlElement xeSkin = (XmlElement)xn;

								if (xeSkin.Name == "skin" || xeSkin.Name == "publicskin")
								{
									if (xeSkin.GetAttribute("Name") == skinName)
									{
										xeRet = xeSkin;
									}
								}
							}
						}

						return xeRet;
					}
				}
			}

			return null;
		}
/*
		public void showSkinFrame()
		{
			XmlElement xeSkin = getLinkSkinXe();

			if(xeSkin != null)
			{
				MainWindow.s_pW.mx_skinFrame.Visibility = System.Windows.Visibility.Visible;
				MainWindow.s_pW.mx_skinApprPre.Items.Clear();
				MainWindow.s_pW.mx_skinApprSuf.Items.Clear();
				CtrlDef_T ctrlDef;

				if(MainWindow.s_pW.m_mapCtrlDef.TryGetValue(m_xe.Name, out ctrlDef))
				{
					foreach (KeyValuePair<string, string> pairSuf in ctrlDef.m_mapApprSuffix.ToList())
					{
						ComboBoxItem cbSuf = new ComboBoxItem();

						cbSuf.Content = pairSuf.Value;
						MainWindow.s_pW.mx_skinApprSuf.Items.Add(cbSuf);
					}
					foreach (KeyValuePair<string, string> pairPre in ctrlDef.m_mapApprPrefix.ToList())
					{
						ComboBoxItem cbPre = new ComboBoxItem();

						cbPre.Content = pairPre.Value;
						MainWindow.s_pW.mx_skinApprPre.Items.Add(cbPre);
					}
				}
				MainWindow.s_pW.mx_skinApprPre.SelectedIndex = 0;
				MainWindow.s_pW.mx_skinApprSuf.SelectedIndex = 0;
			}
		}
*/
		static public bool checkXeIsVisible(XmlElement xe)
		{
			for(XmlNode xn = xe; xn != null && xn.NodeType == XmlNodeType.Element; xn = xn.ParentNode)
			{
				XmlElement xeRet = (XmlElement)xn;

				if(xeRet.GetAttribute("visible") == "false")
				{
					return false;
				}
			}

			return true;
		}
		public bool checkPointInFence(int x, int y)
		{
			if (MainWindow.s_pW.mx_isShowAll.IsChecked == false && m_xe.GetAttribute("visible") == "false")
			{
				return false;
			}
			if (MainWindow.s_pW.mx_isShowAll.IsChecked == false)
			{
				if (!checkXeIsVisible(m_xe))
				{
					return false;
				}
			}
			double perScale = 1.0f;

			if(MainWindow.s_pW.m_isMoba)
			{
				double perScaleX = (double)MainWindow.s_pW.m_screenWidthBasic / (double)MainWindow.s_pW.m_screenWidth;
				double perScaleY = (double)MainWindow.s_pW.m_screenHeightBasic / (double)MainWindow.s_pW.m_screenHeight;

				perScale = perScaleX > perScaleY ? perScaleX : perScaleY;
			}

			if (x * perScale >= m_selScreenX && y * perScale >= m_selScreenY &&
				x * perScale <= m_selScreenX + m_selW && y * perScale <= m_selScreenY + m_selH)
			{
				return true;
			}

			return false;
		}
	}
}
