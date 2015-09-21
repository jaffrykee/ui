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
	public class Basic : UIEditor.BoloUI.XmlItem
	{
		public int m_selX;
		public int m_selY;
		public int m_selW;
		public int m_selH;
		public string m_vId;

		public Basic(XmlElement xe, XmlControl rootControl, bool isRoot = false) : base(xe, rootControl)
		{
			m_type = "CtrlUI";
			InitializeComponent();
			CtrlDef_T ctrlDef;
			m_isCtrl = true;

			if (isRoot == false)
			{
				if (MainWindow.s_pW.m_mapCtrlDef.TryGetValue(m_xe.Name, out ctrlDef) && ctrlDef != null)
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
					m_rootControl.m_mapCtrlUI[m_vId] = this;
				}
				else
				{
					m_vId = "";
				}
				addChild();
			}
			initHeader();
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

					if (MainWindow.s_pW.m_mapCtrlDef.TryGetValue(xe.Name, out ctrlPtr))
					{
						this.Items.Add(new Basic(xe, m_rootControl, false));
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

				if (m_isCtrl && m_xe.Name != "event")
				{
					name = m_xe.GetAttribute("name");
					id = m_xe.GetAttribute("baseID");
				}
				else
				{
					tmpCon = "<" + m_xe.Name + ">";
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
					tmpCon += name;
				}
				if (MainWindow.s_pW.m_vCtrlId)
				{
					tmpCon += "(" + id + ")";
				}

				mx_radio.Content = "_" + tmpCon;
			}
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
			MainWindow.s_pW.m_curFile = m_rootControl.m_openedFile.m_path;
			MainWindow.s_pW.mx_workTabs.SelectedItem = m_rootControl.m_openedFile.m_tab;
			if (m_vId != "")
			{
				MainWindow.s_pW.updateGL(
					StringDic.getFileNameWithoutPath(m_rootControl.m_openedFile.m_path) + ":" + m_vId,
					W2GTag.W2G_SELECT_UI
				);
			}

			MainWindow.s_pW.m_curItem = this;
			MainWindow.s_pW.hiddenAllAttr();
			CtrlDef_T ctrlDef;

			if (MainWindow.s_pW.m_mapCtrlDef.TryGetValue(m_xe.Name, out ctrlDef))
			{
				if (ctrlDef.m_hasBasic == true)
				{
					foreach(KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapBasicCtrlDef.ToList())
					{
						pairCtrlDef.Value.m_ctrlAttrList.Visibility = Visibility.Visible;
						pairCtrlDef.Value.m_ctrlAttrList.clearRowValue();
					}
				}
				ctrlDef.m_ctrlAttrList.Visibility = Visibility.Visible;
				ctrlDef.m_ctrlAttrList.clearRowValue();
			}

			foreach (XmlAttribute attr in m_xe.Attributes)
			{
				bool isOther = false;

				if (m_isCtrl)
				{
					AttrDef_T attrDef;
					bool tmpFound = false;

					foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapBasicCtrlDef.ToList())
					{
						if (!tmpFound && pairCtrlDef.Value.m_mapAttrDef.TryGetValue(attr.Name, out attrDef))
						{
							attrDef.m_attrRowUI.m_preValue = attr.Value;
							tmpFound = true;
						}
					}
					if (!tmpFound && ctrlDef.m_mapAttrDef.TryGetValue(attr.Name, out attrDef))
					{
						attrDef.m_attrRowUI.m_preValue = attr.Value;
					}
					else
					{
						isOther = true;
					}
				}
				else
				{
					isOther = true;
				}

				if (isOther)
				{
					if (MainWindow.s_pW.m_otherAttrList == null)
					{
						MainWindow.s_pW.m_otherAttrList = new AttrList("other");
						MainWindow.s_pW.mx_toolArea.Children.Add(MainWindow.s_pW.m_otherAttrList);
					}
					MainWindow.s_pW.m_otherAttrList.mx_frame.Children.Add(new AttrRow("string", attr.Name, attr.Value, MainWindow.s_pW.m_otherAttrList));
				}
			}
			if (m_isCtrl)
			{
				foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in MainWindow.s_pW.m_mapBasicCtrlDef.ToList())
				{
					pairCtrlDef.Value.m_ctrlAttrList.refreshRowVisible();
					pairCtrlDef.Value.m_ctrlAttrList.m_xmlCtrl = m_rootControl;
					pairCtrlDef.Value.m_ctrlAttrList.m_basic = this;
					pairCtrlDef.Value.m_ctrlAttrList.m_xe = m_xe;
				}
				ctrlDef.m_ctrlAttrList.refreshRowVisible();
				ctrlDef.m_ctrlAttrList.m_xmlCtrl = m_rootControl;
				ctrlDef.m_ctrlAttrList.m_basic = this;
				ctrlDef.m_ctrlAttrList.m_xe = m_xe;
			}
			if (MainWindow.s_pW.m_otherAttrList != null)
			{
				MainWindow.s_pW.m_otherAttrList.refreshRowVisible();
				MainWindow.s_pW.m_otherAttrList.Visibility = Visibility.Visible;
				MainWindow.s_pW.m_otherAttrList.m_xmlCtrl = m_rootControl;
				MainWindow.s_pW.m_otherAttrList.m_basic = this;
				MainWindow.s_pW.m_otherAttrList.m_xe = m_xe;
			}

			mx_radio.IsChecked = true;
			BringIntoView();

			SelButton selBn;

			if (MainWindow.s_pW.m_mapXeSel != null && MainWindow.s_pW.m_mapXeSel.TryGetValue(m_xe, out selBn) && selBn != null)
			{
				selBn.mx_radio.IsChecked = true;
			}
			gotoSelectXe();
			//showSkinFrame();
			m_selLock.delLock(ref stackLock);
		}
		public XmlElement getLinkSkinXe()
		{
			XmlElement xeRet = null;

			m_rootControl.refreshSkinDicForAll();
			if (m_xe != null && m_xe.GetAttribute("skin") != "")
			{
				string skinName = m_xe.GetAttribute("skin");
				string groupName;

				if (m_rootControl.m_mapSkinLink.TryGetValue(skinName, out groupName))
				{
					string path = MainWindow.s_pW.m_skinPath + "\\" + groupName + ".xml";

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
		private bool checkXeIsVisible(XmlElement xe)
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
			if (x >= m_selX && y >= m_selY)
			{
				if (x <= m_selX + m_selW && y <= m_selY + m_selH)
				{
					return true;
				}
			}

			return false;
		}
	}
}
