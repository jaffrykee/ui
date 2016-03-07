using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using UIEditor.XmlOperation.XmlAttr;
using System.Collections;

namespace UIEditor.BoloUI.DefConfig
{
	public class DefConf
	{
		static private void addAttrConf(XmlElement xe, Dictionary<string, AttrDef_T> mapAttrDef)
		{
			foreach (XmlNode xnAttr in xe.ChildNodes)
			{
				if (xnAttr.NodeType == XmlNodeType.Element)
				{
					XmlElement xeAttr = (XmlElement)xnAttr;

					if (xeAttr.Name == "AttrDef")
					{
						string keyAttr = xeAttr.GetAttribute("key");

						if (keyAttr != "")
						{
							AttrDef_T attrDef;

							if (!mapAttrDef.TryGetValue(keyAttr, out attrDef))
							{
								attrDef = new AttrDef_T();
								if (xeAttr.GetAttribute("type") != "")
								{
									attrDef.m_type = xeAttr.GetAttribute("type");
									if(xeAttr.GetAttribute("type") == "weight")
									{
										attrDef.m_lstWeight = new ArrayList();

										foreach(XmlNode xnWt in xeAttr.ChildNodes)
										{
											if (xnWt is XmlElement)
											{
												switch(xnWt.Name)
												{
													case "rowGroup":
														{
															WeightRowGroup_T wtGroup = new WeightRowGroup_T(((XmlElement)xnWt).GetAttribute("key"));
															attrDef.m_lstWeight.Add(wtGroup);
															foreach(XmlNode xnRow in xnWt.ChildNodes)
															{
																if(xnRow is XmlElement && xnRow.Name == "row")
																{
																	wtGroup.m_lstRow.Add(xnRow.InnerText);
																}
															}
														}
														break;
													case "row":
														{
															attrDef.m_lstWeight.Add(xnWt.InnerText);
														}
														break;
													default:
														break;
												}
											}
										}
									}
								}
								else
								{
									attrDef.m_type = "string";
								}
								if (xeAttr.GetAttribute("isEnum") == "true")
								{
									attrDef.m_isEnum = true;
									attrDef.m_mapEnum = new Dictionary<string, ComboBoxItem>();
									foreach (XmlNode xnEnum in xeAttr.ChildNodes)
									{
										if (xnEnum.NodeType == XmlNodeType.Element)
										{
											XmlElement xeEnum = (XmlElement)xnEnum;

											if (xeEnum.Name == "row")
											{
												attrDef.m_mapEnum.Add(xeEnum.InnerText, null);
											}
										}
									}
								}
								else
								{
									attrDef.m_isEnum = false;
								}
								if (xeAttr.GetAttribute("isCommon") == "true")
								{
									attrDef.m_isCommon = true;
								}
								else
								{
									attrDef.m_isCommon = false;
								}
								attrDef.m_subType = xeAttr.GetAttribute("subType");
								attrDef.m_defValue = xeAttr.GetAttribute("defValue");
								mapAttrDef.Add(keyAttr, attrDef);
							}
						}
					}
				}
			}
		}
		static private bool initCtrlDef()
		{
			MainWindow pW = MainWindow.s_pW;
			string confPath = MainWindow.conf_pathPlugInBoloUI + @"CtrlAttrDef.xml";
			XmlDocument docDef = new XmlDocument();

			docDef.Load(confPath);
			pW.m_mapCtrlDef = new Dictionary<string, CtrlDef_T>();
			pW.m_mapPanelCtrlDef = new Dictionary<string, CtrlDef_T>();
			pW.m_mapBasicCtrlDef = new Dictionary<string, CtrlDef_T>();
			pW.m_mapHasBasicCtrlDef = new Dictionary<string, CtrlDef_T>();
			pW.m_mapEnInsertCtrlDef = new Dictionary<string, CtrlDef_T>();
			pW.m_mapEnInsertAllCtrlDef = new Dictionary<string, CtrlDef_T>();

			if (docDef.DocumentElement.Name == "CtrlAttrDef")
			{
				XmlElement xeRoot = docDef.DocumentElement;
				string nameDic;
				string tipDic;
				Dictionary<string, Dictionary<string, string>> mapNameDic;
				Dictionary<string, Dictionary<string, string>> mapTipDic;

				#region 词典
				if (xeRoot.GetAttribute("nameDic") != "")
				{
					nameDic = xeRoot.GetAttribute("nameDic");
				}
				else
				{
					nameDic = "DefDic";
				}
				if (pW.m_strDic.m_mapStrDic.TryGetValue(nameDic, out mapNameDic))
				{

				}
				else
				{
					if (pW.m_strDic.m_mapStrDic.TryGetValue("DefDic", out mapNameDic))
					{

					}
					else
					{
						return false;
					}
				}
				if (xeRoot.GetAttribute("tipDic") != "")
				{
					tipDic = xeRoot.GetAttribute("tipDic");
				}
				else
				{
					tipDic = "DefDic";
				}
				if (pW.m_strDic.m_mapStrDic.TryGetValue(tipDic, out mapTipDic))
				{

				}
				else
				{
					if (pW.m_strDic.m_mapStrDic.TryGetValue("DefDic", out mapTipDic))
					{

					}
					else
					{
						return false;
					}
				}
				#endregion

				#region conf
				foreach (XmlNode xnCtrl in xeRoot.ChildNodes)
				{
					if (xnCtrl.NodeType == XmlNodeType.Element)
					{
						XmlElement xeCtrl = (XmlElement)xnCtrl;

						if (xeCtrl.Name == "CtrlDef" && xeCtrl.GetAttribute("key") != "")
						{
							string keyCtrl = xeCtrl.GetAttribute("key");
							CtrlDef_T ctrlDef;

							if (!pW.m_mapCtrlDef.TryGetValue(keyCtrl, out ctrlDef))
							{
								Dictionary<string, AttrDef_T> mapAttrDef = new Dictionary<string, AttrDef_T>();
								string isFrame = xeCtrl.GetAttribute("isFrame");
								string isBasic = xeCtrl.GetAttribute("isBasic");
								string hasBasic = xeCtrl.GetAttribute("hasBasic");
								string hasPointerEvent = xeCtrl.GetAttribute("hasPointerEvent");
								string enInsert = xeCtrl.GetAttribute("enInsert");

								ctrlDef = new CtrlDef_T(mapAttrDef, null);
								if (isFrame == "true")
								{
									ctrlDef.m_isFrame = true;
									pW.m_mapPanelCtrlDef.Add(keyCtrl, ctrlDef);
								}
								else
								{
									ctrlDef.m_isFrame = false;
								}
								if (isBasic == "true")
								{
									ctrlDef.m_isBasic = true;
									pW.m_mapBasicCtrlDef.Add(keyCtrl, ctrlDef);
								}
								else
								{
									ctrlDef.m_isBasic = false;
								}
								if (hasBasic == "false")
								{
									ctrlDef.m_hasBasic = false;
								}
								else
								{
									ctrlDef.m_hasBasic = true;
									pW.m_mapHasBasicCtrlDef.Add(keyCtrl, ctrlDef);
								}
								if (hasPointerEvent == "false")
								{
									ctrlDef.m_hasPointerEvent = false;
								}
								else
								{
									ctrlDef.m_hasPointerEvent = true;
								}
								if (enInsert == "false")
								{
									ctrlDef.m_enInsert = false;
									ctrlDef.m_enInsertAll = false;
								}
								else if (enInsert == "all")
								{
									ctrlDef.m_enInsert = false;
									ctrlDef.m_enInsertAll = true;
									pW.m_mapEnInsertAllCtrlDef.Add(keyCtrl, ctrlDef);
								}
								else
								{
									ctrlDef.m_enInsert = true;
									ctrlDef.m_enInsertAll = false;
									pW.m_mapEnInsertCtrlDef.Add(keyCtrl, ctrlDef);
								}
								pW.m_mapCtrlDef.Add(keyCtrl, ctrlDef);
								addAttrConf(xeCtrl, mapAttrDef);
							}
						}
					}
				}
				#endregion
				refreshSkinApprDef();

				return true;
			}

			return false;
		}
		static private void addSkinNodeConf(XmlElement xe, Dictionary<string, SkinDef_T> mapSkinTreeDef)
		{
			MainWindow pW = MainWindow.s_pW;

			foreach (XmlNode xnNode in xe.ChildNodes)
			{
				if (xnNode.NodeType == XmlNodeType.Element)
				{
					XmlElement xeNode = (XmlElement)xnNode;

					if (xeNode.Name == "node")
					{
						string keySkin = xeNode.GetAttribute("key");
						SkinDef_T skinDef;

						if (keySkin != "" && !mapSkinTreeDef.TryGetValue(keySkin, out skinDef))
						{
							Dictionary<string, AttrDef_T> mapAttrDef;
							if (pW.m_mapSkinAttrDef.TryGetValue(keySkin, out mapAttrDef))
							{
								Dictionary<string, SkinDef_T> mapTree = new Dictionary<string, SkinDef_T>();

								addSkinNodeConf(xeNode, mapTree);
								skinDef = new SkinDef_T(mapTree, mapAttrDef, null);
								mapSkinTreeDef.Add(keySkin, skinDef);
							}
						}
					}
				}
			}
		}
		static private bool initSkinDef()
		{
			MainWindow pW = MainWindow.s_pW;

			#region 属性设置
			string attrPath = MainWindow.conf_pathPlugInBoloUI + @"SkinAttrDef.xml";
			XmlDocument docAttr = new XmlDocument();
			docAttr.Load(attrPath);

			if (docAttr.DocumentElement.Name != "SkinAttrDef")
			{
				return false;
			}

			pW.m_mapSkinAttrDef = new Dictionary<string, Dictionary<string, AttrDef_T>>();
			XmlElement xe = docAttr.DocumentElement;

			foreach (XmlNode xnSkinDef in xe.ChildNodes)
			{
				if (xnSkinDef.NodeType == XmlNodeType.Element)
				{
					XmlElement xeSkinDef = (XmlElement)xnSkinDef;
					string keySkin = xeSkinDef.GetAttribute("key");

					if (xeSkinDef.Name == "SkinDef" && keySkin != "")
					{
						Dictionary<string, AttrDef_T> mapAttrDef = new Dictionary<string, AttrDef_T>();

						addAttrConf(xeSkinDef, mapAttrDef);
						pW.m_mapSkinAttrDef.Add(keySkin, mapAttrDef);
					}
				}
			}
			#endregion

			#region 节点设置
			string nodePath = MainWindow.conf_pathPlugInBoloUI + @"SkinNodes.xml";
			XmlDocument docNode = new XmlDocument();

			docNode.Load(nodePath);

			if (docNode.DocumentElement.Name != "DataNodes")
			{
				return false;
			}

			pW.m_mapSkinTreeDef = new Dictionary<string, SkinDef_T>();
			XmlElement xeRoot = docNode.DocumentElement;

			addSkinNodeConf(xeRoot, pW.m_mapSkinTreeDef);
			#endregion

			return true;
		}
		static private void initResMap(Dictionary<string, SkinDef_T> mapResDef)
		{
			MainWindow pW = MainWindow.s_pW;

			foreach (KeyValuePair<string, SkinDef_T> pairSkinDef in mapResDef.ToList())
			{
				SkinDef_T skinDef;
				if (!pW.m_mapSkinAllDef.TryGetValue(pairSkinDef.Key, out skinDef))
				{
					pW.m_mapSkinAllDef.Add(pairSkinDef.Key, pairSkinDef.Value);
					if (pairSkinDef.Value.m_mapEnChild != null && pairSkinDef.Value.m_mapEnChild.Count > 0)
					{
						initResMap(pairSkinDef.Value.m_mapEnChild);
					}
				}
			}
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

								if (pW.m_mapCtrlDef.TryGetValue(ctrlKey, out ctrlDef))
								{
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
		static private void loadSkinApprSuf(XmlDocument docConf, Dictionary<string, string> mapStrDic, string ctrlName, Dictionary<string, string> mapSufStr)
		{

		}

		static public void initXmlValueDef()
		{
			MainWindow pW = MainWindow.s_pW;

			initCtrlDef();
			initSkinDef();
			initResMap(pW.m_mapSkinTreeDef);

			foreach (KeyValuePair<string, CtrlDef_T> pairCtrlDef in pW.m_mapCtrlDef.ToList())
			{
				pairCtrlDef.Value.m_ctrlAttrList = new AttrList(pairCtrlDef.Key, pairCtrlDef.Value.m_mapAttrDef);
				pW.mx_toolArea.Items.Add(pairCtrlDef.Value.m_ctrlAttrList);
				pairCtrlDef.Value.m_ctrlAttrList.Visibility = Visibility.Collapsed;
			}
			foreach (KeyValuePair<string, SkinDef_T> pairSkinDef in pW.m_mapSkinAllDef.ToList())
			{
				pairSkinDef.Value.m_skinAttrList = new AttrList(pairSkinDef.Key, pairSkinDef.Value.m_mapAttrDef);
				pW.mx_toolArea.Items.Add(pairSkinDef.Value.m_skinAttrList);
				pairSkinDef.Value.m_skinAttrList.Visibility = Visibility.Collapsed;
			}
		}
	}
}
