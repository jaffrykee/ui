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

namespace UIEditor.BoloUI
{
	public class ResBasic : UIEditor.BoloUI.XmlItem
	{
		public SkinDef_T m_curDeepDef;

		public ResBasic(XmlElement xe, XmlControl rootControl, SkinDef_T deepDef) : base(xe, rootControl)
		{
			m_type = "Skin";
			m_curDeepDef = deepDef;
			m_apprPre = "";
			m_apprTagStr = "";
			m_apprSuf = "";
			InitializeComponent();
			m_isCtrl = false;

			if (m_curDeepDef != null)
			{
				if (m_xe.Name == "skin" || m_xe.Name == "publicskin")
				{
					ResBasic nullPtr;

					if (m_rootControl.m_mapSkin.TryGetValue(m_xe.GetAttribute("Name"), out nullPtr))
					{
						string errorInfo = "<错误>文件:\"" + rootControl.m_openedFile.m_path + "\"中，存在重复Name的皮肤(" +
							m_xe.GetAttribute("Name") + ")，前一个同名的皮肤将不能正确显示。\r\n";

						MainWindow.s_pW.mx_debug.Text += errorInfo;
						Public.ErrorInfo.addToErrorInfo(errorInfo);
					}
					m_rootControl.m_mapSkin[m_xe.GetAttribute("Name")] = this;
				}
				addChild();
			}

			initHeader();
		}

		public void addResItem(XmlElement newXe)
		{
			ResBasic treeChild;
			if (m_xe.Name == "BoloUI")
			{
				treeChild = new ResBasic(newXe, m_rootControl, MainWindow.s_pW.m_mapSkinTreeDef[newXe.Name]);
			}
			else
			{
				treeChild = new ResBasic(newXe, m_rootControl, m_curDeepDef.m_mapEnChild[newXe.Name]);
			}

			m_rootControl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(XmlOperation.XmlOptType.NODE_INSERT, treeChild.m_xe, m_xe));
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
								this.Items.Add(new ResBasic(xe, m_rootControl, skinPtr));
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
					tmpCon = "<" + ctrlName + ">";
				}
				else
				{
					tmpCon = "<" + m_xe.Name + ">";
				}

				if (ctrlTip != "")
				{
					mx_radio.ToolTip = m_xe.Name + "\r\n" + ctrlTip;
				}
				else
				{
					mx_radio.ToolTip = m_xe.Name;
				}

				if (m_curDeepDef.m_mapAttrDef != null && m_curDeepDef.m_mapAttrDef.ToList().Count > 0)
				{
					name = m_xe.GetAttribute(m_curDeepDef.m_mapAttrDef.ToList().First().Key);
				}

				if (m_xe.Name == "apperance")
				{
					name = parseApprIdFromDic(name);
				}
				tmpCon += name;
				mx_radio.Content = "_" + tmpCon;
			}
		}
		public static void resetXeView(XmlElement srcXe, out XmlElement xeView)
		{
			xeView = srcXe.OwnerDocument.CreateElement(srcXe.Name);

			foreach (XmlAttribute attr in srcXe.Attributes)
			{
				xeView.SetAttribute(attr.Name, attr.Value);
			}
			xeView.SetAttribute("baseID", "selSkinTestCtrl");
			xeView.RemoveAttribute("x");
			xeView.RemoveAttribute("y");
			xeView.RemoveAttribute("visible");
			xeView.RemoveAttribute("dock");
			xeView.RemoveAttribute("anchor");
			xeView.RemoveAttribute("anchorSelf");
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
			m_rootControl.m_curItem = this;
			if(m_xe.Name != "BoloUI")
			{
				BoloUI.Basic ctrlUI;
				MainWindow.s_pW.hiddenAllAttr();

				if (obj != null && obj is Basic)
				{
					ctrlUI = (BoloUI.Basic)obj;
				}
				else
				{
					if (m_rootControl.m_skinViewCtrlUI != null)
					{
						ctrlUI = m_rootControl.m_skinViewCtrlUI;
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
					skinDef.m_skinAttrList.m_xmlCtrl = m_rootControl;
					skinDef.m_skinAttrList.m_basic = this;
					skinDef.m_skinAttrList.m_xe = m_xe;

					foreach (XmlAttribute attr in m_xe.Attributes)
					{
						AttrDef_T attrDef;

						if(skinDef.m_mapAttrDef.TryGetValue(attr.Name, out attrDef))
						{
							attrDef.m_attrRowUI.m_preValue = attr.Value;
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
					if (m_rootControl.m_isOnlySkin)
					{
						XmlElement xeView;

						if (ctrlUI == null && m_rootControl.m_skinViewCtrlUI == null)
						{
							xeView = MainWindow.s_pW.m_xeTest;
						}
						else
						{
							if (ctrlUI != null)
							{
								m_rootControl.m_skinViewCtrlUI = ctrlUI;
							}
							resetXeView(m_rootControl.m_skinViewCtrlUI.m_xe, out xeView);
						}
						((XmlElement)xeView).SetAttribute("skin", xeSkin.GetAttribute("Name"));
						MainWindow.s_pW.updateXmlToGL(m_rootControl, xeView, false);
					}
					//todo 更改皮肤预览
				}
			}
			mx_radio.IsChecked = true;
			BringIntoView();
			gotoSelectXe();
			m_selLock.delLock(ref stackLock);
		}
	}
}
