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

using s32 = System.Int32;
using f32 = System.Single;
using b2 = System.Boolean;

namespace UIEditor.BoloUI
{
	public class ResBasic : UIEditor.BoloUI.XmlItem
	{
		public SkinDef_T m_curDeepDef;
		public bool m_isSkinEditor;

		public ResBasic(XmlElement xe, XmlControl rootControl, SkinDef_T deepDef, bool isSkinEditor = false)
			: base(xe, rootControl)
		{
			m_type = "Skin";
			m_curDeepDef = deepDef;
			m_apprPre = "";
			m_apprTagStr = "";
			m_apprSuf = "";
			m_isSkinEditor = isSkinEditor;
			InitializeComponent();
			m_isCtrl = false;

			IsExpanded = true;
			if (m_curDeepDef != null)
			{
				if ((m_xe.Name == "skin" || m_xe.Name == "publicskin") && m_xmlCtrl != null)
				{
					ResBasic resItem;

					if (m_xmlCtrl.m_mapSkin.TryGetValue(m_xe.GetAttribute("Name"), out resItem))
					{
						string errorInfo = "\r\n" + m_xmlCtrl.m_openedFile.m_path + " - 存在重名皮肤(" +
							m_xe.GetAttribute("Name") + ")，前一个同名的皮肤将不能被正确引用。";

						Public.ResultLink.createResult(errorInfo, Public.ResultType.RT_ERROR);
						Public.ResultLink.createResult("<重名皮肤1> ", Public.ResultType.RT_ERROR, resItem);
						Public.ResultLink.createResult("<重名皮肤2>", Public.ResultType.RT_ERROR, this);
						//Public.ErrorInfo.addToErrorInfo(errorInfo);
					}
					m_xmlCtrl.m_mapSkin[m_xe.GetAttribute("Name")] = this;
					IsExpanded = false;
				}
				else
				{
					//显示或隐藏皮肤节点
					addChild();
				}
			}

			initHeader();
		}

		public void addResItem(XmlElement newXe)
		{
			m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT, newXe, m_xe));
		}
		void insertSkinItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem)
			{
				MenuItem ctrlItem = (MenuItem)sender;
				XmlElement newXe = m_xe.OwnerDocument.CreateElement(ctrlItem.ToolTip.ToString());

				addResItem(newXe);
			}
			//throw new NotImplementedException();
		}

		override protected void addChild()
		{
			XmlNodeList xnl = m_xe.ChildNodes;
			foreach (XmlNode xnf in xnl)
			{
				if (xnf.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xnf;
					SkinDef_T skinPtr;

					if (m_curDeepDef.m_mapEnChild != null)
					{
						if (m_curDeepDef.m_mapEnChild.TryGetValue(xe.Name, out skinPtr))
						{
							if (skinPtr != null)
							{
								this.Items.Add(new ResBasic(xe, m_xmlCtrl, skinPtr, m_isSkinEditor));
								mx_imgFolder.Visibility = System.Windows.Visibility.Visible;
							}
						}
					}
				}
			}
		}
		public string parseApprIdFromDic(string apprId)
		{
			string retId = "";
			bool isHave = false;
			MainWindow pW = MainWindow.s_pW;
			Dictionary<string, Dictionary<string, string>> preDic, sufDic;

			if (pW.m_strDic.m_mapStrDic.TryGetValue("SkinApprPre", out preDic) &&
				pW.m_strDic.m_mapStrDic.TryGetValue("SkinApprSuf", out sufDic))
			{
				foreach(KeyValuePair<string, Dictionary<string, string>> pairPreRow in preDic)
				{
					string preKey = "";
					if (pairPreRow.Key == "_def")
					{
						preKey = "";
					}
					else
					{
						preKey = pairPreRow.Key;
					}
					if (preKey == "" || apprId.IndexOf(preKey) == 0)
					{
						string otherStr = apprId.Substring(preKey.Length);
						foreach (KeyValuePair<string, Dictionary<string, string>> pairSufRow in sufDic)
						{
							if (otherStr.LastIndexOf(pairSufRow.Key) >= 0 &&
								otherStr.LastIndexOf(pairSufRow.Key) == otherStr.Length - pairSufRow.Key.Length)
							{
								string tagStr = otherStr.Substring(0, otherStr.Length - pairSufRow.Key.Length);
								if(preKey != "" || tagStr == "")
								{
									string newRet = pW.m_strDic.getWordByKey(pairPreRow.Key, "SkinApprPre") + tagStr +
										pW.m_strDic.getWordByKey("的") + pW.m_strDic.getWordByKey(pairSufRow.Key, "SkinApprSuf");

									if (isHave == false)
									{
										m_apprPre = pairPreRow.Key;
										m_apprTagStr = tagStr;
										m_apprSuf = pairSufRow.Key;
										isHave = true;
										retId = newRet;
									}
									else
									{
										retId += " " + pW.m_strDic.getWordByKey("或者") + " " + newRet;
									}
								}
							}
						}
					}
				}
			}

			if (isHave)
			{
				return retId;
			}
			else
			{
				return apprId;
			}
		}
		public override void initHeader()
		{
			if(m_curDeepDef != null)
			{
				string ctrlName = MainWindow.s_pW.m_strDic.getWordByKey(m_xe.Name);
				string ctrlTip = MainWindow.s_pW.m_strDic.getWordByKey(m_xe.Name, StringDic.conf_ctrlTipDic);
				string name = "";
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

				if (m_curDeepDef.m_mapAttrDef != null && m_curDeepDef.m_mapAttrDef.ToList().Count > 0)
				{
					name = m_xe.GetAttribute(m_curDeepDef.m_mapAttrDef.ToList().First().Key);
				}

				if (m_xe.Name == "apperance")
				{
					name = parseApprIdFromDic(name);
				}
				tmpCon += "<" + name + ">";
				mx_text.Text = tmpCon;
			}
		}
		public static void resetXeView(Basic uiView, out XmlElement xeView)
		{
			xeView = uiView.m_xe.OwnerDocument.CreateElement(uiView.m_xe.Name);

			foreach (XmlAttribute attr in uiView.m_xe.Attributes)
			{
				xeView.SetAttribute(attr.Name, attr.Value);
			}
			xeView.RemoveAttribute("x");
			xeView.RemoveAttribute("y");
			xeView.RemoveAttribute("visible");
			xeView.RemoveAttribute("dock");
			xeView.RemoveAttribute("anchor");
			xeView.RemoveAttribute("anchorSelf");
			xeView.SetAttribute("x", uiView.m_selScreenX.ToString());
			xeView.SetAttribute("y", uiView.m_selScreenY.ToString());
			xeView.SetAttribute("w", uiView.m_selW.ToString());
			xeView.SetAttribute("h", uiView.m_selH.ToString());
			xeView.SetAttribute("baseID", "selSkinTestCtrl");
			xeView.SetAttribute("skin", uiView.m_xe.GetAttribute("skin"));
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
			if (m_isSkinEditor == false)
			{
				MainWindow.s_pW.mx_treeFrame.SelectedItem = MainWindow.s_pW.mx_treeFrameSkin;
			}
			if (m_xmlCtrl != null)
			{
				m_xmlCtrl.m_curItem = this;
				if (m_isSkinEditor == true)
				{
					if(MainWindow.s_pW.mx_skinEditor != null)
					{
						MainWindow.s_pW.mx_skinEditor.m_selResItem = this;
					}
				}
			}
			if(m_xe.Name != "BoloUI")
			{
				BoloUI.Basic ctrlUI;
				AttrList.hiddenAllAttr();

				if (obj != null && obj is Basic)
				{
					ctrlUI = (BoloUI.Basic)obj;
				}
				else
				{
					if (m_xmlCtrl != null && m_xmlCtrl.m_skinViewCtrlUI != null)
					{
						ctrlUI = m_xmlCtrl.m_skinViewCtrlUI;
					}
					else
					{
						ctrlUI = null;
					}
				}
				SkinDef_T skinDef;
				if(MainWindow.s_pW.m_mapSkinAllDef.TryGetValue(m_xe.Name, out skinDef))
				{
					skinDef.m_skinAttrList.Visibility = Visibility.Visible;
					skinDef.m_skinAttrList.clearRowValue();

					skinDef.m_skinAttrList.refreshRowVisible();
					skinDef.m_skinAttrList.m_xmlCtrl = m_xmlCtrl;
					skinDef.m_skinAttrList.m_basic = this;
					skinDef.m_skinAttrList.m_xe = m_xe;

					foreach (XmlAttribute attr in m_xe.Attributes)
					{
						AttrDef_T attrDef;

						if(skinDef.m_mapAttrDef.TryGetValue(attr.Name, out attrDef))
						{
							attrDef.m_iAttrRowUI.m_preValue = attr.Value;
						}
					}
				}

				//预览皮肤
				XmlElement xeSkin = null;
				XmlElement xeTmp = m_xe;

				for (int i = 0; i < 7 && xeSkin == null && xeTmp != null; i++)
				{
					if (xeTmp.Name == "skin" || xeTmp.Name == "publicskin")
					{
						xeSkin = xeTmp;
					}
					else
					{
						if (xeTmp.ParentNode is XmlElement)
						{
							xeTmp = (XmlElement)xeTmp.ParentNode;
						}
						else
						{
							xeTmp = null;
						}
					}
				}
				if (xeSkin != null)
				{
					if (m_xmlCtrl.m_isOnlySkin)
					{
						XmlElement xeView;

						if (ctrlUI == null && m_xmlCtrl.m_skinViewCtrlUI == null)
						{
							xeView = MainWindow.s_pW.m_xeTest;
						}
						else
						{
							if (ctrlUI != null)
							{
								m_xmlCtrl.m_skinViewCtrlUI = ctrlUI;
							}
							resetXeView(m_xmlCtrl.m_skinViewCtrlUI, out xeView);
						}
						((XmlElement)xeView).SetAttribute("skin", xeSkin.GetAttribute("Name"));
						MainWindow.s_pW.updateXmlToGL(m_xmlCtrl, xeView, false);
					}
					//todo 更改皮肤预览
				}
				if (m_xe.Name == "particleKeyFrameGroup")
				{
					sendKeyFrameDrawData();
				}
			}
			this.IsSelected = true;
			BringIntoView();
			gotoSelectXe();
			AttrList.selectLastAttrList();
			m_selLock.delLock(ref stackLock);
		}
		public void sendKeyFrameDrawData()
		{
			string msgData;

			if (MainWindow.s_pW.mx_skinEditor != null && MainWindow.s_pW.mx_skinEditor.Visibility == System.Windows.Visibility.Visible)
			{
				Basic uiCtrl = MainWindow.s_pW.mx_skinEditor.m_curCtrl;

				if (uiCtrl != null)
				{
					msgData = "true:" + uiCtrl.m_selScreenX + ":" + uiCtrl.m_selScreenY + ":" + uiCtrl.m_selW + ":" + uiCtrl.m_selH + ":" +
						((XmlElement)m_xe.ParentNode).GetAttribute("Anchor") + ":";
					foreach(XmlNode xnFrame in m_xe.SelectNodes("particleKeyFrame"))
					{
						if(xnFrame is XmlElement)
						{
							XmlElement xeFrame = (XmlElement)xnFrame;

							addRowToDrawParticleLineMsgData(xeFrame, ref msgData);
						}
					}
					MainWindow.s_pW.updateGL(msgData, W2GTag.W2G_DRAW_PARTICLE_LINE);
				}
			}
		}
		public void addRowToDrawParticleLineMsgData(XmlElement xeKeyFrame, ref string msgData)
		{
			if(msgData != null && xeKeyFrame != null)
			{
				msgData += xeKeyFrame.GetAttribute("X") + ":";
				msgData += xeKeyFrame.GetAttribute("Y") + ":";
				msgData += xeKeyFrame.GetAttribute("time") + ":";
				msgData += xeKeyFrame.GetAttribute("perX") + ":";
				msgData += xeKeyFrame.GetAttribute("perY") + ":";

				msgData += xeKeyFrame.GetAttribute("moveType") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx1") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx2") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx3") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx4") + ":";

				msgData += xeKeyFrame.GetAttribute("moveTypeEx5") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx6") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx7") + ":";
				msgData += xeKeyFrame.GetAttribute("moveTypeEx8") + ":";
				msgData += xeKeyFrame.GetAttribute("addPerX") + ":";

				msgData += xeKeyFrame.GetAttribute("addPerY") + ":";

				msgData += "true" + ":";//isKeyFrame
				msgData += xeKeyFrame.GetAttribute("isKeyHide") + ":";
			}
		}

		enum BoloUIKeyFrameMoveType
		{
			KEYFRAME_MOVETYPE_ULM = 0x0000, //直线运动
			KEYFRAME_MOVETYPE_UCM = 0x0001, //圆弧运动
			KEYFRAME_MOVETYPE_FIBON = 0x0002, //斐波那契螺旋线
			KEYFRAME_MOVETYPE_SIN = 0x0003, //正弦线
			KEYFRAME_MOVETYPE_LEAF = 0x0004, //叶形线
			KEYFRAME_MOVETYPE_SCREW = 0x0005, //平面螺旋线
			KEYFRAME_MOVETYPE_COLUMNSCREW = 0x0006, //螺旋线
			KEYFRAME_MOVETYPE_MAX
		};
		//BoloUISkin.h BoloUISkin::Frame::KeyFrame_T
        public struct KeyFrame_S
		{
			//0
			s32 x;
			s32 y;
			s32 beginTime;
			f32 m_perX;
			f32 m_perY;

			//5
			BoloUIKeyFrameMoveType m_moveType;
			s32 m_moveTypeEx1;
			s32 m_moveTypeEx2;
			s32 m_moveTypeEx3;
			s32 m_moveTypeEx4;

			//10
			s32 m_moveTypeEx5;
			s32 m_moveTypeEx6;
			s32 m_moveTypeEx7;
			s32 m_moveTypeEx8;
			f32 m_addPerX;

			//15
			f32 m_addPerY;
			b2 m_isKeyFrame;
			b2 m_isKeyHide;
        };
	}
}