using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UIEditor.XmlOperation.XmlAttr;
using UIEditor.Project.PlugIn;

namespace UIEditor.BoloUI.DefConfig
{
	/// <summary>
	/// 用于表示BoloUI控件属性结构的类
	/// </summary>
	public class CtrlDef_T : Project.PlugIn.DataNode
	{
		static public bool tryGetCtrlDef(string nodeName, out CtrlDef_T ctrlDef)
		{
			DataNode nodeDef;

			ctrlDef = null;
			if(DataNodeGroup.tryGetDataNode("BoloUI", "Ctrl", nodeName, out nodeDef) && nodeDef != null && nodeDef is CtrlDef_T)
			{
				ctrlDef = (CtrlDef_T)nodeDef;

				return true;
			}

			return false;
		}
		static public bool? isFrame(string nodeName)
		{
			CtrlDef_T ctrlDef;

			if(tryGetCtrlDef(nodeName, out ctrlDef))
			{
				return ctrlDef.isFrame();
			}

			return null;
		}
		static private void refreshSkinApprFix(bool isPreConf)
		{
			MainWindow pW = MainWindow.s_pW;
			string confPath;

			if (isPreConf)
			{
				confPath = MainWindow.conf_pathPlugInBoloUI + @"SkinApprPre.xml";
			}
			else
			{
				confPath = MainWindow.conf_pathPlugInBoloUI + @"SkinApprSuf.xml";
			}
			XmlDocument docConf = new XmlDocument();

			docConf.Load(confPath);

			XmlElement xeRoot = docConf.DocumentElement;
			string nameDic = xeRoot.GetAttribute("nameDic");

			//if (nameDic != "")
			{
				foreach (XmlNode xnCtrl in xeRoot.ChildNodes)
				{
					if (xnCtrl.NodeType == XmlNodeType.Element)
					{
						XmlElement xeCtrl = (XmlElement)xnCtrl;

						if (xeCtrl.Name == "CtrlConf")
						{
							if (xeCtrl.GetAttribute("key") != "")
							{
								string ctrlKey = xeCtrl.GetAttribute("key");
								CtrlDef_T ctrlDef;
								DataNode dataNode;

								if (DataNodeGroup.tryGetDataNode("BoloUI", "Ctrl", ctrlKey, out dataNode) && dataNode != null && dataNode is CtrlDef_T)
								{
									ctrlDef = (CtrlDef_T)dataNode;
									if (isPreConf)
									{
										ctrlDef.m_mapApprPrefix = new Dictionary<string, string>();
									}
									else
									{
										ctrlDef.m_mapApprSuffix = new Dictionary<string, string>();
									}
									foreach (XmlNode xnRow in xeCtrl.ChildNodes)
									{
										if (xnRow.NodeType == XmlNodeType.Element)
										{
											XmlElement xeRow = (XmlElement)xnRow;

											if (xeRow.Name == "row" && xeRow.InnerText != "")
											{
												if (isPreConf)
												{
													ctrlDef.m_mapApprPrefix.Add(xeRow.InnerText, pW.m_strDic.getWordByKey(xeRow.InnerText, "SkinApprPre"));
												}
												else
												{
													ctrlDef.m_mapApprSuffix.Add(xeRow.InnerText, pW.m_strDic.getWordByKey(xeRow.InnerText, "SkinApprSuf"));
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
		}
		static private void refreshSkinApprDef()
		{
			refreshSkinApprFix(true);
			refreshSkinApprFix(false);
		}

		public Dictionary<string, string> m_mapApprPrefix;
		public Dictionary<string, string> m_mapApprSuffix;
		public bool m_isFrame;
		public bool m_hasPointerEvent;

		public CtrlDef_T(DataNodeGroup parent, string name)
			: base(parent, name)
		{
			m_mapApprSuffix = new Dictionary<string, string>();
			m_mapApprPrefix = new Dictionary<string, string>();

			initData();
		}

		public void initData()
		{
			m_isFrame = m_hlstClassName.Contains("frameClass");
			if(m_name == "label")
			{
				m_hasPointerEvent = false;
			}
			else
			{
				m_hasPointerEvent = true;
			}
		}
		public bool isFrame()
		{
			return m_hlstClassName.Contains("frameClass");
		}
		public bool isNormal()
		{
			return m_hlstClassName.Contains("normalClass");
		}
	}
}
