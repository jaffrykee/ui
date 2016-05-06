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
using System.IO;
using System.Xml;
using System.Windows.Threading;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;
using UIEditor.Project;

namespace UIEditor
{
	public partial class XmlControl : UserControl
	{
		public FileTabItem m_parent;
		public OpenedFile m_openedFile;
		public bool m_showGL;
		//以baseID为索引的UI们(假id)
		public Dictionary<string, BoloUI.Basic> m_mapCtrlUI;
		//真id
		public Dictionary<string, BoloUI.Basic> m_mapBaseIdCtrlUI;
		public Dictionary<string, long> m_mapImageSize;
		public Dictionary<string, BoloUI.ResBasic> m_mapSkin;
		public Dictionary<XmlElement, XmlItem> m_mapXeItem;
		public Dictionary<Run, XmlItem> m_mapRunItem;
		public XmlDocument m_xmlDoc;
		public XmlElement m_xeRootCtrl;
		public XmlElement m_xeRoot;
		public bool m_isOnlySkin;
		public BoloUI.Basic m_skinViewCtrlUI;
		public XmlItem m_curItem;

		public BoloUI.Basic m_treeUI;
		public BoloUI.ResBasic m_treeSkin;

		public Run m_curSearchRun;
		public int m_curSearchIndex;

		public XmlControl(FileTabItem parent, OpenedFile fileDef, string skinName = "")
		{
			m_curSearchIndex = 0;

			InitializeComponent();
			m_parent = parent;
			m_showGL = false;

			m_openedFile = fileDef;
			m_openedFile.m_frame = this;

			try
			{
				m_xmlDoc = new XmlDocument();
				m_xmlDoc.Load(m_openedFile.m_path);
			}
			catch
			{
				return;
			}

			m_openedFile.m_lstOpt = new XmlOperation.HistoryList(MainWindow.s_pW, this, 65535);
			refreshControl(skinName);
			refreshXmlText();
		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			MainWindow.s_pW.mx_selCtrlLstFrame.Children.Clear();
			//clearNodeTreeFrame();
		}
		
		static public void setAllChildExpand(TreeViewItem item)
		{
			item.IsExpanded = true;
			foreach(TreeViewItem childItem in item.Items)
			{
				setAllChildExpand(childItem);
			}
		}
		static public XmlControl getCurXmlControl()
		{
			OpenedFile fileDef;

			if (MainWindow.s_pW != null &&
				MainWindow.s_pW.m_curFile != null &&
				MainWindow.s_pW.m_curFile != "" &&
				MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(MainWindow.s_pW.m_curFile, out fileDef) &&
				fileDef != null &&
				fileDef.m_frame != null &&
				fileDef.m_frame is XmlControl)
			{
				XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;

				return xmlCtrl;
			}

			return null;
		}
		static public void changeSelectCtrlAndFile(MainWindow pW, string path, string baseId)
		{
			if (File.Exists(path))
			{
				//todo
			}
			else
			{
				Public.ResultLink.createResult("\r\n文件：\"" + path + "\"不存在，请检查路径。", Public.ResultType.RT_WARNING);
			}
		}
		static public void changeSelectSkinAndFile(string path, string skinName, BoloUI.Basic ctrlUI = null)
		{
			if (File.Exists(path))
			{
				OpenedFile skinFile;

				//pW.openFileByPath(path, skinName);
				MainWindow.s_pW.openFileByPath(path);
				if (MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(path, out skinFile))
				{
					if (skinFile.m_frame != null)
					{
						if (skinFile.m_frame is XmlControl)
						{
							XmlControl xmlCtrl = (XmlControl)skinFile.m_frame;
							BoloUI.ResBasic skinBasic;

							if(xmlCtrl.m_mapSkin.TryGetValue(skinName, out skinBasic))
							{
								skinBasic.changeSelectItem(ctrlUI);
								setAllChildExpand(skinBasic);
							}
							else
							{
								Public.ResultLink.createResult("\r\n没有找到该皮肤。(" + skinName + ")", Public.ResultType.RT_WARNING);
							}
						}
					}
					else
					{
						skinFile.m_preViewSkinName = skinName;
						skinFile.m_prePlusCtrlUI = ctrlUI;
					}
				}
			}
			else
			{
				Public.ResultLink.createResult("\r\n文件：\"" + path + "\"不存在，请检查路径。", Public.ResultType.RT_WARNING);
			}
		}
		public Dictionary<string, XmlElement> getSkinGroupMap()
		{
			Dictionary<string, XmlElement> mapXeGroup = new Dictionary<string, XmlElement>();

			if(m_xmlDoc.DocumentElement.Name == "BoloUI")
			{
				foreach(XmlNode xn in m_xmlDoc.DocumentElement.ChildNodes)
				{
					if(xn.NodeType == XmlNodeType.Element)
					{
						XmlElement xe = (XmlElement)xn;

						if(xe.Name == "skingroup")
						{
							string attrName = xe.GetAttribute("Name");

							if(attrName != "")
							{
								XmlElement xeOut;

								if(!mapXeGroup.TryGetValue(attrName, out xeOut))
								{
									mapXeGroup.Add(attrName, xe);
								}
							}
						}
					}
				}
			}

			return mapXeGroup;
		}
		public void checkSkinLinkAndAddSkinGroup(string skinName)
		{
			string xmlPath;
			object retSkin;

			//Project.Setting.refreshSkinIndex();
			retSkin = findSkin(skinName, out xmlPath);
			if (retSkin == null)
			{
				List<SkinUsingCount_T> lstSkinCount;

				//没在本文件的皮肤组中发现该皮肤。
				findSkinForAll(skinName, out lstSkinCount);
				if(lstSkinCount != null && lstSkinCount.Count > 0)
				{
					if(lstSkinCount.Count == 1)
					{
						Public.ResultLink.createResult("\r\n已自动在本UI添加皮肤[" + skinName + "]的所在皮肤组[", Public.ResultType.RT_INFO);
						Public.ResultLink.createResult(lstSkinCount[0].m_groupName, Public.ResultType.RT_INFO, Setting.getSkinGroupPathByName(lstSkinCount[0].m_groupName));
						Public.ResultLink.createResult("]", Public.ResultType.RT_INFO);
						addSkinGroup(lstSkinCount[0].m_groupName);
					}
					else
					{
						MultipleSkinGroupSelectWin winMulSkinGroup = new MultipleSkinGroupSelectWin(this, skinName, lstSkinCount);

						winMulSkinGroup.ShowDialog();
					}
				}
				else
				{
					Public.ResultLink.createResult("\r\n在全工程范围没有发现本皮肤(" + skinName + ")", Public.ResultType.RT_ERROR);
				}
			}
		}
		public void addSkinGroup(string groupName)
		{
			Dictionary<string, XmlElement> mapXeGroup = getSkinGroupMap();
			XmlItem lastItem = m_curItem;

			if (groupName != null && groupName != "")
			{
				XmlElement xeOut;

				if (!mapXeGroup.TryGetValue(groupName, out xeOut))
				{
					XmlElement newXe = m_xmlDoc.CreateElement("skingroup");

					newXe.SetAttribute("Name", groupName);

					SkinDef_T skinDef;

					if(SkinDef_T.tryGetSkinDef("skingroup", out skinDef) && skinDef != null)
					{
						BoloUI.ResBasic treeChild = new BoloUI.ResBasic(newXe, this, skinDef);

						m_openedFile.m_lstOpt.addOperation(
							new XmlOperation.HistoryNode(
								XmlOperation.XmlOptType.NODE_INSERT,
								treeChild.m_xe,
								m_xmlDoc.DocumentElement)
							);
						if(lastItem != null)
						{
							lastItem.changeSelectItem();
						}
					}
				}
			}
		}
		static public void createSkin(string pathSkinGroup, string skinName, string skinContent, XmlOperation.XmlAttr.IAttrRow iAttrRow)
		{
			OpenedFile fileDef;

			//MainWindow.s_pW.openFileByPath(m_pathSkinGroup);
			if (MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(pathSkinGroup, out fileDef))
			{
				if (fileDef.m_frame is XmlControl)
				{
					XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;
					XmlElement newXe = xmlCtrl.m_xmlDoc.CreateElement("skin");

					if (skinContent != null && skinContent != "")
					{
						XmlDocument docTmpl = new XmlDocument();

						try
						{
							docTmpl.LoadXml(skinContent);
							if (docTmpl.DocumentElement != null &&
								docTmpl.DocumentElement.InnerXml != null &&
								docTmpl.DocumentElement.InnerXml != "")
							{
								newXe.InnerXml = docTmpl.DocumentElement.InnerXml;
							}
						}
						catch
						{

						}
					}
					newXe.SetAttribute("Name", skinName);
					xmlCtrl.m_treeSkin.addResItem(newXe);

					xmlCtrl.saveCurStatus();
				}
			}
			else
			{
				if (File.Exists(pathSkinGroup))
				{
					XmlDocument docSkinGroup = new XmlDocument();

					try
					{
						docSkinGroup.Load(pathSkinGroup);
						if (docSkinGroup.DocumentElement.Name == "BoloUI")
						{
							XmlElement xeSkin = docSkinGroup.CreateElement("skin");

							if (skinContent != null && skinContent != "")
							{
								XmlDocument docTmpl = new XmlDocument();

								try
								{
									docTmpl.LoadXml(skinContent);
									if (docTmpl.DocumentElement != null &&
										docTmpl.DocumentElement.InnerXml != null &&
										docTmpl.DocumentElement.InnerXml != "")
									{
										xeSkin.InnerXml = docTmpl.DocumentElement.InnerXml;
									}
								}
								catch
								{

								}
							}
							xeSkin.SetAttribute("Name", skinName);
							docSkinGroup.DocumentElement.AppendChild(xeSkin);

							docSkinGroup.Save(pathSkinGroup);
							Project.Setting.refreshSkinIndex();

						}
					}
					catch
					{

					}
				}
			}

			if (newSkin.s_pW != null)
			{
				newSkin.s_pW.Close();
			}

			iAttrRow.m_parent.m_basic.changeSelectItem();
			MainWindow.s_pW.mx_treeFrame.SelectedItem = MainWindow.s_pW.mx_skinEditor;
		}

		public List<SkinUsingCount_T> getIncludeSkinGroupList(string skinName)
		{
			List<SkinUsingCount_T> lstSkinCount;

			if (Project.Setting.s_mapSkinUsingCount.TryGetValue(skinName, out lstSkinCount))
			{
				return getIncludeSkinGroupList(lstSkinCount);
			}
			else
			{
				return null;
			}
		}
		public List<SkinUsingCount_T> getIncludeSkinGroupList(List<SkinUsingCount_T> lstSkinCount)
		{
			List<SkinUsingCount_T> lstIncludeSkinCount = new List<SkinUsingCount_T>();

			if (lstSkinCount == null || lstSkinCount.Count == 0)
			{
				return lstIncludeSkinCount;
			}
			foreach (XmlNode xnGroup in m_xmlDoc.DocumentElement.SelectNodes("skingroup"))
			{
				if (xnGroup is XmlElement)
				{
					XmlElement xeGroup = (XmlElement)xnGroup;

					foreach (SkinUsingCount_T skinCountDef in lstSkinCount)
					{
						if (xeGroup.GetAttribute("Name") == skinCountDef.m_groupName)
						{
							lstIncludeSkinCount.Add(skinCountDef);
							break;
						}
					}
				}
			}

			return lstIncludeSkinCount;
		}

		static public void findSkinForAll(string skinName, out List<SkinUsingCount_T> lstSkinCount)
		{
			Project.Setting.s_mapSkinUsingCount.TryGetValue(skinName, out lstSkinCount);
		}
		private XmlElement findSkinWithThemeAndLanguage(string skinName, out string xmlPath)
		{
			ResBasic retSkinCtrl = null;
			xmlPath = null;

			if (m_mapSkin.TryGetValue(skinName, out retSkinCtrl) && retSkinCtrl != null && retSkinCtrl.m_xe != null)
			{
				return retSkinCtrl.m_xe;
				//return retSkinCtrl;
			}
			else
			{
				List<SkinUsingCount_T> lstSkinCount = getIncludeSkinGroupList(skinName);

				if (lstSkinCount != null && lstSkinCount.Count > 0)
				{
					if (lstSkinCount.Count > 1)
					{
						Public.ResultLink.createResult("\r\n皮肤 " + skinName + " 存在多个皮肤组归属：", Public.ResultType.RT_WARNING);
						foreach (SkinUsingCount_T skinCountDef in lstSkinCount)
						{
							Public.ResultLink.createResult(" [" + skinCountDef + "] ", Public.ResultType.RT_WARNING,
								Setting.getSkinGroupPathByName(skinCountDef.m_groupName));
						}
					}
					string path = Project.Setting.s_skinPath + "\\" + lstSkinCount[0].m_groupName + ".xml";

					if (File.Exists(path))
					{
						XmlDocument docSkin = new XmlDocument();

						try
						{
							docSkin.Load(path);
						}
						catch
						{
							return null;
						}
						if (docSkin.DocumentElement != null && docSkin.DocumentElement.Name == "BoloUI")
						{
							foreach (XmlNode xnSkin in docSkin.DocumentElement.ChildNodes)
							{
								if (xnSkin.NodeType == XmlNodeType.Element)
								{
									XmlElement xeSkin = (XmlElement)xnSkin;

									if ((xeSkin.Name == "skin" || xeSkin.Name == "publicskin") && xeSkin.GetAttribute("Name") == skinName)
									{
										xmlPath = path;

										return xeSkin;
									}
								}
							}
						}
					}
				}
			}

			return null;
		}
		public XmlElement findSkin(string skinName, out string xmlPath)
		{
			XmlElement xeRet = null;

			if(MainWindow.s_pW.mx_isEnableTheme.IsChecked != true)
			{
				xeRet = findSkinWithThemeAndLanguage(skinName, out xmlPath);
			}
			else
			{
				ComboBoxItem cbiCurTheme = null;
				ComboBoxItem cbiCurLanguage = null;
				string curThemeName = "";
				string curLangName = "";

				if(MainWindow.s_pW.mx_cbThemeName.SelectedItem != null && MainWindow.s_pW.mx_cbThemeName.SelectedItem is ComboBoxItem)
				{
					cbiCurTheme = (ComboBoxItem)MainWindow.s_pW.mx_cbThemeName.SelectedItem;
				}
				if(MainWindow.s_pW.mx_cbLangName.SelectedItem != null && MainWindow.s_pW.mx_cbLangName.SelectedItem is ComboBoxItem)
				{
					cbiCurLanguage = (ComboBoxItem)MainWindow.s_pW.mx_cbLangName.SelectedItem;
				}
				if(cbiCurTheme != null)
				{
					curThemeName = cbiCurTheme.Content.ToString();
				}
				if(cbiCurLanguage != null)
				{
					curLangName = cbiCurLanguage.Content.ToString();
				}
				
				string skinName_full = skinName + "{" + curThemeName + "}" + "{" + curLangName + "}";
				string skinName_onlyTheme = skinName + "{" + curThemeName + "}" + "{}";
				string skinName_onlyLang = skinName + "{}" + "{" + curLangName + "}";
				string skinName_none = skinName + "{}{}";

				xeRet = findSkinWithThemeAndLanguage(skinName_full, out xmlPath);
				if (xeRet == null)
				{
					xeRet = findSkinWithThemeAndLanguage(skinName_onlyTheme, out xmlPath);
					if (xeRet == null)
					{
						xeRet = findSkinWithThemeAndLanguage(skinName_onlyLang, out xmlPath);
						if (xeRet == null)
						{
							xeRet = findSkinWithThemeAndLanguage(skinName_none, out xmlPath);
							if (xeRet == null)
							{
								xeRet = findSkinWithThemeAndLanguage(skinName, out xmlPath);
							}
						}
					}
				}
			}

			return xeRet;
		}
		private bool findSkinAndSelectWithThemeAndLanguage(string skinName, BoloUI.Basic ctrlUI = null)
		{
			BoloUI.ResBasic skinBasic;

			if (m_mapSkin.TryGetValue(skinName, out skinBasic))
			{
				skinBasic.changeSelectItem(ctrlUI);
				setAllChildExpand(skinBasic);

				return true;
			}
			else
			{
				List<SkinUsingCount_T> lstSkinCount = getIncludeSkinGroupList(skinName);
				if (lstSkinCount != null && lstSkinCount.Count > 0)
				{
					string path = Project.Setting.s_skinPath + "\\" + lstSkinCount[0].m_groupName + ".xml";

					changeSelectSkinAndFile(path, skinName, ctrlUI);

					return true;
				}
			}

			return false;
		}
		public bool findSkinAndSelect(string skinName, BoloUI.Basic ctrlUI = null)
		{
			bool ret = false;

			if (MainWindow.s_pW.mx_isEnableTheme.IsChecked != true)
			{
				ret = findSkinAndSelectWithThemeAndLanguage(skinName, ctrlUI);
			}
			else
			{
				ComboBoxItem cbiCurTheme = null;
				ComboBoxItem cbiCurLanguage = null;
				string curThemeName = "";
				string curLangName = "";

				if (MainWindow.s_pW.mx_cbThemeName.SelectedItem != null && MainWindow.s_pW.mx_cbThemeName.SelectedItem is ComboBoxItem)
				{
					cbiCurTheme = (ComboBoxItem)MainWindow.s_pW.mx_cbThemeName.SelectedItem;
				}
				if (MainWindow.s_pW.mx_cbLangName.SelectedItem != null && MainWindow.s_pW.mx_cbLangName.SelectedItem is ComboBoxItem)
				{
					cbiCurLanguage = (ComboBoxItem)MainWindow.s_pW.mx_cbLangName.SelectedItem;
				}
				if (cbiCurTheme != null)
				{
					curThemeName = cbiCurTheme.Content.ToString();
				}
				if (cbiCurLanguage != null)
				{
					curLangName = cbiCurLanguage.Content.ToString();
				}

				string skinName_full = skinName + "{" + curThemeName + "}" + "{" + curLangName + "}";
				string skinName_onlyTheme = skinName + "{" + curThemeName + "}" + "{}";
				string skinName_onlyLang = skinName + "{}" + "{" + curLangName + "}";
				string skinName_none = skinName + "{}{}";

				ret = findSkinAndSelectWithThemeAndLanguage(skinName_full, ctrlUI);
				if (ret == false)
				{
					ret = findSkinAndSelectWithThemeAndLanguage(skinName_onlyTheme, ctrlUI);
					if (ret == false)
					{
						ret = findSkinAndSelectWithThemeAndLanguage(skinName_onlyLang, ctrlUI);
						if (ret == false)
						{
							ret = findSkinAndSelectWithThemeAndLanguage(skinName_none, ctrlUI);
							if (ret == false)
							{
								ret = findSkinAndSelectWithThemeAndLanguage(skinName, ctrlUI);
							}
						}
					}
				}
			}
			if(ret == false)
			{
				Public.ResultLink.createResult("\r\n没有找到该皮肤。(" + skinName + ")", Public.ResultType.RT_ERROR);
			}

			return ret;
		}

		public void refreshVRect()
		{
			string msgData = "";

			foreach(KeyValuePair<string, BoloUI.Basic> pairCtrlUI in m_mapCtrlUI.ToList())
			{
				XmlItem item;

				if (m_mapXeItem.TryGetValue(pairCtrlUI.Value.m_xe, out item) &&
					(MainWindow.s_pW.mx_isShowAll.IsChecked == true || Basic.checkXeIsVisible(pairCtrlUI.Value.m_xe)))
				{
					msgData += pairCtrlUI.Key + ":";
				}
			}
			MainWindow.s_pW.updateGL(msgData, W2GTag.W2G_UI_VRECT);
		}
		public void refreshSkinDicByPath(string path, string skinGroupName)
		{
			if (File.Exists(path))
			{
				XmlDocument skinDoc = new XmlDocument();

				try
				{
					skinDoc.Load(path);
				}
				catch
				{
					Public.ResultLink.createResult("\r\n皮肤文件：\"" + path + "\"格式错误，不是一个合法的xml文件。",
						Public.ResultType.RT_WARNING);

					return;
				}

				XmlNode xn = skinDoc.SelectSingleNode("BoloUI");

				if (xn != null)
				{
					XmlNodeList xnlSkin = xn.ChildNodes;

					foreach (XmlNode xnfSkin in xnlSkin)
					{
						if (xnfSkin.NodeType == XmlNodeType.Element)
						{
							XmlElement xeSkin = (XmlElement)xnfSkin;

// 							if (xeSkin.Name == "skin" || xeSkin.Name == "publicskin")
// 							{
// 								if (xeSkin.GetAttribute("Name") != "")
// 								{
// 									string oldSkinGroupName;
// 									if (!m_mapSkinLink.TryGetValue(xeSkin.GetAttribute("Name"), out oldSkinGroupName))
// 									{
// 										m_mapSkinLink.Add(xeSkin.GetAttribute("Name"), skinGroupName);
// 									}
// 									else
// 									{
// 										string oldSkinPath = Project.Setting.s_skinPath + "\\" + oldSkinGroupName + ".xml";
// 										string newSkinPath = Project.Setting.s_skinPath + "\\" + skinGroupName + ".xml";
// 
// 										m_mapSkinLink[xeSkin.GetAttribute("Name")] = skinGroupName;
// 
// 										Public.ResultLink.createResult("\r\n<" + oldSkinPath + ">和", Public.ResultType.RT_WARNING,
// 											new Public.SkinLinkDef_T(oldSkinPath, xeSkin.GetAttribute("Name")));
// 										Public.ResultLink.createResult("<" + path + "> - ", Public.ResultType.RT_WARNING,
// 											new Public.SkinLinkDef_T(newSkinPath, xeSkin.GetAttribute("Name")));
// 										Public.ResultLink.createResult("存在同名皮肤引用：" + xeSkin.GetAttribute("Name") + "。", Public.ResultType.RT_WARNING);
// 									}
// 								}
// 							}
							if (xeSkin.Name == "resource" || xeSkin.Name == "publicresource")
							{
								if (xeSkin.GetAttribute("name") != "")
								{
									string imgName = xeSkin.GetAttribute("name");
									string tgaPath = Project.Setting.s_imagePath + "\\" + imgName + ".tga";
									string bmpPath = Project.Setting.s_imagePath + "\\" + imgName + ".bmp";
									string imgPath = "";
									long imgSize;

									if (File.Exists(tgaPath))
									{
										imgPath = tgaPath;
									}
									else if(File.Exists(bmpPath))
									{
										imgPath = bmpPath;
									}
									else
									{
										continue;
									}
									if (!m_mapImageSize.TryGetValue(imgPath, out imgSize))
									{
										FileInfo fiImage = new FileInfo(imgPath);

										m_mapImageSize.Add(imgPath, fiImage.Length);
									}
								}
							}
						}
					}
				}
			}
			else
			{
				//不存在
				if (skinGroupName != "publicskin")
				{
					Public.ResultLink.createResult("\r\n皮肤组：\"" + skinGroupName + "\"不存在，请检查路径：\"" + path + "\"。",
						Public.ResultType.RT_WARNING);
				}
			}
		}
		public void refreshSkinDicByGroupName(string skinGroupName)
		{
			string path = Project.Setting.s_skinPath + "\\" + skinGroupName + ".xml";

			refreshSkinDicByPath(path, skinGroupName);
		}
		public void refreshSkinDicForAll()
		{
			m_mapImageSize.Clear();
			if (m_xeRoot != null && m_xeRoot.Name == "BoloUI")
			{
				foreach (XmlNode xnf in m_xeRoot.ChildNodes)
				{
					if (xnf.NodeType == XmlNodeType.Element)
					{
						XmlElement xe = (XmlElement)xnf;

						if (xe.Name == "skingroup" && xe.GetAttribute("Name") != "")
						{
							refreshSkinDicByGroupName(xe.GetAttribute("Name"));
						}
					}
				}
				refreshSkinDicByGroupName("publicskin");
			}
			long szSum = 0;
			foreach(KeyValuePair<string, long> pairImageSize in m_mapImageSize.ToList())
			{
				szSum += pairImageSize.Value;
			}
			double szMb = (double)szSum / 1024 / 1024;
			MainWindow.s_pW.mb_status2 = "占用内存:   图片：" + Math.Round(szMb, 2).ToString() + " MB";
		}
		void mx_newRun_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender != null && sender is Run)
			{
				Run newRun = (Run)sender;
				XmlItem item;

				if (m_mapRunItem.TryGetValue(newRun, out item) && item != null && item != XmlItem.getCurItem())
				{
					item.changeSelectItem();
				}
			}
		}
		private Run addRunAndLinkItem(Paragraph para, string text, XmlItem xeItem, Color color, ref Run oldRun, ref Run diffRun)
		{
			Run newRun = new Run(text);
			XmlItem tmpItem;

			newRun.Foreground = new SolidColorBrush(color);
			para.Inlines.Add(newRun);

			if (xeItem != null && !m_mapRunItem.TryGetValue(newRun, out tmpItem))
			{
				m_mapRunItem.Add(newRun, xeItem);
				newRun.MouseDown += mx_newRun_MouseDown;
			}

			if (oldRun != null)
			{
				if (oldRun.Text == text)
				{
					if (oldRun.NextInline != null && oldRun.NextInline is Run)
					{
						oldRun = (Run)oldRun.NextInline;
					}
					else
					{
						oldRun = null;
						diffRun = newRun;
					}
				}
				else
				{
					oldRun = null;
					diffRun = newRun;
				}
			}

			return newRun;
		}
		public enum XmlTextColorType
		{
			NORMAL = 0x0000,
			NAME = 0x0001,
			ATTRNAME = 0x0002,
			ATTRVALUE = 0x0003,
			COMMENT = 0x0004,
			XMLDECLARATION = 0x0005,
			OTHER = 0x0006,
			BCK_CURSEL = 0x0007,
			BCK_SEARCH = 0x0008,
			MAX
		}
		public static Color[] s_arrTextColor = {
			Colors.LightBlue, Colors.LightPink, Colors.LightGreen, Colors.White,
			Colors.LightGray, Colors.LightGray, Colors.LightBlue,
			System.Windows.Media.Color.FromArgb(0x77, 0x33, 0x99, 0xff),
			System.Windows.Media.Color.FromArgb(0x88, 0xaa, 0xaa, 0x33)};
		public void refreshXmlSign(XmlNode xnRoot, Paragraph para, ref Run oldRun, ref Run diffRun, int deep = 0)
		{
			string strTabs = "";
			bool isFirst = true;
			string endStr = ">";

			if (deep == 0)
			{
				m_mapRunItem = new Dictionary<Run, XmlItem>();
			}
			for(int i = 0; i < deep; i++)
			{
				strTabs += "    ";
			}
			foreach(XmlNode xn in xnRoot.ChildNodes)
			{
				XmlItem xeItem = null;

				if (xn.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xn;

					if (m_mapXeItem != null &&
						m_mapXeItem.TryGetValue(xe, out xeItem) &&
						xeItem != null)
					{

					}
				}

				if(deep == 0 && isFirst == true)
				{
					isFirst = false;
				}
				else
				{
					addRunAndLinkItem(para, "\n", xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
				}
				switch(xn.NodeType)
				{
					case XmlNodeType.Element:
						{
							XmlElement xe = (XmlElement)xn;

							addRunAndLinkItem(para, strTabs + "<", xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
							if (xeItem != null)
							{
								xeItem.m_runXeName = addRunAndLinkItem(para, xe.Name, xeItem, s_arrTextColor[(int)XmlTextColorType.NAME], ref oldRun, ref diffRun);
							}
							else
							{
								addRunAndLinkItem(para, xe.Name, xeItem, s_arrTextColor[(int)XmlTextColorType.NAME], ref oldRun, ref diffRun);
							}
							foreach(XmlAttribute attr in xe.Attributes)
							{
								//addRunAndLinkItem(para, "\n" + strTabs + "    ", xeItem, Colors.Blue, ref oldRun, ref diffRun);
								addRunAndLinkItem(para, " " + attr.Name, xeItem, s_arrTextColor[(int)XmlTextColorType.ATTRNAME], ref oldRun, ref diffRun);
								addRunAndLinkItem(para, "=\"", xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
								addRunAndLinkItem(para, attr.InnerXml, xeItem, s_arrTextColor[(int)XmlTextColorType.ATTRVALUE], ref oldRun, ref diffRun);
								addRunAndLinkItem(para, "\"", xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
							}
							if(xe.OuterXml.IndexOf("/>") == xe.OuterXml.Length - "/>".Length)
							{
								endStr = "/>";
							}
							else
							{
								endStr = ">";
							}
							addRunAndLinkItem(para, endStr, xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
						}
						break;
					case XmlNodeType.EndElement:
						{
						}
						break;
					case XmlNodeType.XmlDeclaration:
						{
							addRunAndLinkItem(para, xn.OuterXml, xeItem, s_arrTextColor[(int)XmlTextColorType.XMLDECLARATION], ref oldRun, ref diffRun);
						}
						break;
					case XmlNodeType.Comment:
						{
							addRunAndLinkItem(para, xn.OuterXml, xeItem, s_arrTextColor[(int)XmlTextColorType.COMMENT], ref oldRun, ref diffRun);
						}
						break;
					default:
						{
							addRunAndLinkItem(para, xn.OuterXml, xeItem, s_arrTextColor[(int)XmlTextColorType.OTHER], ref oldRun, ref diffRun);
						}
						break;
				}
				refreshXmlSign(xn, para, ref oldRun, ref diffRun, deep + 1);
				if (xn.NodeType == XmlNodeType.Element && endStr == ">")
				{
					addRunAndLinkItem(para, "\n" + strTabs, xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
					addRunAndLinkItem(para, "</", xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
					addRunAndLinkItem(para, xn.Name, xeItem, s_arrTextColor[(int)XmlTextColorType.NAME], ref oldRun, ref diffRun);
					addRunAndLinkItem(para, ">", xeItem, s_arrTextColor[(int)XmlTextColorType.NORMAL], ref oldRun, ref diffRun);
				}
			}
		}
		static public string getOutXml(XmlDocument docXml)
		{
			string retStr;
			StringBuilder strb = new StringBuilder();

			using (StringWriter sw = new StringWriter(strb))
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.NewLineOnAttributes = false;
				XmlWriter xmlWriter = XmlWriter.Create(sw, settings);
				docXml.Save(xmlWriter);
				xmlWriter.Close();
			}
			retStr = strb.ToString();

			return retStr;
		}
		public void refreshXmlText(int offset = 0)
		{
			if(m_openedFile.m_curViewSkin != "")
			{
				MainWindow.s_pW.mx_xmlText.Visibility = System.Windows.Visibility.Collapsed;
				return;
			}
			else
			{
				MainWindow.s_pW.mx_xmlText.Visibility = System.Windows.Visibility.Visible;
			}
			MainWindow.s_pW.m_isCanEdit = false;
			if(MainWindow.s_pW.mx_xmlText.Document == null)
			{
				MainWindow.s_pW.mx_xmlText.Document = new FlowDocument();
			}
			string outXml = getOutXml(m_xmlDoc);

			if (outXml.Length > 3000000)
			{
				TextRange rag = new TextRange(MainWindow.s_pW.mx_xmlText.Document.ContentStart, MainWindow.s_pW.mx_xmlText.Document.ContentEnd);

				MainWindow.s_pW.mx_xmlText.Document.LineHeight = 1;
				rag.Text = "文件过大（格式化xml字符超过300万），不提供基于文本的修改";
				MainWindow.s_pW.mx_xmlText.IsEnabled = false;
			}
			else
			{
				FlowDocument oldFlowDoc = MainWindow.s_pW.mx_xmlText.Document;
				Run oldRun = null;
				MainWindow.s_pW.mx_xmlText.Document = new FlowDocument();
				MainWindow.s_pW.mx_xmlText.Document.LineHeight = 1;
				MainWindow.s_pW.mx_xmlText.Document.PageWidth = 2000;
				Paragraph para = new Paragraph();

				if (oldFlowDoc.Blocks.Count > 0 && oldFlowDoc.Blocks.First() is Paragraph)
				{
					Paragraph oldPara = (Paragraph)oldFlowDoc.Blocks.First();

					if (oldPara.Inlines.Count > 0 && oldPara.Inlines.First() is Run)
					{
						oldRun = (Run)oldPara.Inlines.First();
					}
				}

				Run runDiff = null;

				refreshXmlSign(m_xmlDoc, para, ref oldRun, ref runDiff);
				MainWindow.s_pW.mx_xmlText.Document.Blocks.Add(para);
				MainWindow.s_pW.mx_xmlText.IsEnabled = true;

				if(runDiff != null)
				{
					MainWindow.s_pW.m_lastUpdateRun = runDiff;
				}
			}
			if (offset != 0 && MainWindow.s_pW.mx_xmlText.Document.Blocks.Count > 0)
			{
				TextPointer curPoi = MainWindow.s_pW.mx_xmlText.Document.Blocks.First().ElementStart.GetPositionAtOffset(-offset);

				if (curPoi != null)
				{
					MainWindow.s_pW.mx_xmlText.CaretPosition = curPoi;
				}
			}

			MainWindow.s_pW.m_isCanEdit = true;
		}
		static public XmlElement getNextXmlElement(XmlElement xe)
		{
			if(xe.FirstChild != null)
			{
				if(xe.FirstChild.NodeType == XmlNodeType.Element)
				{
					return (XmlElement)xe.FirstChild;
				}
				else
				{
					for (XmlNode xn = xe.FirstChild.NextSibling; xn != null; xn = xn.NextSibling)
					{
						if(xn.NodeType == XmlNodeType.Element)
						{
							return (XmlElement)xn;
						}
					}
				}
			}
			for (XmlNode xnParent = xe; xnParent != null; xnParent = xnParent.ParentNode)
			{
				for (XmlNode xn = xnParent.NextSibling; xn != null; xn = xn.NextSibling)
				{
					if (xn.NodeType == XmlNodeType.Element)
					{
						return (XmlElement)xn;
					}
				}
			}

			return null;
		}
		public void resetXmlItemLink()
		{
			XmlElement xeCount = m_xmlDoc.DocumentElement;

			m_mapRunItem = new Dictionary<Run, XmlItem>();
			if(MainWindow.s_pW.mx_xmlText.Document != null)
			{
				foreach(Block block in MainWindow.s_pW.mx_xmlText.Document.Blocks)
				{
					if(block is Paragraph)
					{
						Paragraph para = (Paragraph)block;
						XmlItem item = null;

						foreach(Inline line in para.Inlines)
						{
							if (line is Run)
							{
								Run run = (Run)line;

								if (run.Text != null && run.Text != "" && run.Text.Last() == '<')
								{
									if (run.NextInline != null && run.NextInline is Run)
									{
										Run runXe = (Run)run.NextInline;
										string strName = runXe.Text;

										if (xeCount.Name == strName && m_mapXeItem.TryGetValue(xeCount, out item) && item != null)
										{
											item.m_runXeName = runXe;
											XmlElement xeNext = getNextXmlElement(xeCount);

											if (xeNext != null)
											{
												xeCount = xeNext;
											}
											else
											{
												return;
											}
										}
										else
										{

										}
									}
								}
								if (run != null && item != null)
								{
									m_mapRunItem.Add(run, item);
								}
							}
						}
					}
				}
			}
		}
		static public void clearNodeTreeFrame()
		{
			foreach (object item in MainWindow.s_pW.mx_treeCtrlFrame.Items)
			{
				if (item != null && (item is XmlItem || item.GetType().BaseType.ToString() == "UIEditor.BoloUI.XmlItem"))
				{
					XmlItem xmlItem = (XmlItem)item;

					xmlItem.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
			foreach (object item in MainWindow.s_pW.mx_treeSkinFrame.Items)
			{
				if (item != null && (item is XmlItem || item.GetType().BaseType.ToString() == "UIEditor.BoloUI.XmlItem"))
				{
					XmlItem xmlItem = (XmlItem)item;

					xmlItem.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
		}
		private void checkUICtrlBaseIdByXe(XmlElement xe)
		{
			foreach(XmlNode xnChild in xe.ChildNodes)
			{
				CtrlDef_T ctrlDef;

				if(xnChild is XmlElement)
				{
					if (xnChild.Name == "BoloUI")
					{
						checkUICtrlBaseIdByXe((XmlElement)xnChild);
					}
					else if (CtrlDef_T.tryGetCtrlDef(xnChild.Name, out ctrlDef))
					{
						XmlElement xeChild = (XmlElement)xnChild;
						string baseId = xeChild.GetAttribute("baseID");

						if (baseId != "")
						{
							Basic ctrlItem;
							
							if(m_mapBaseIdCtrlUI.TryGetValue(baseId, out ctrlItem))
							{
								XmlItem xmlItem;

								if (m_mapXeItem.TryGetValue(xeChild, out xmlItem) && xmlItem != null && xmlItem is Basic)
								{
									Basic newCtrlItem = (Basic)xmlItem;

									Public.ResultLink.createResult("\r\n" + m_openedFile.m_path + " - 在控件 ", Public.ResultType.RT_ERROR);
									Public.ResultLink.createResult("[" + newCtrlItem.mx_text.Text + "]", Public.ResultType.RT_ERROR, newCtrlItem);
									Public.ResultLink.createResult(" 和 ", Public.ResultType.RT_ERROR);
									Public.ResultLink.createResult("[" + ctrlItem.mx_text.Text + "]", Public.ResultType.RT_ERROR, ctrlItem);
									Public.ResultLink.createResult(" 存在相同的\"baseID\"属性", Public.ResultType.RT_ERROR);
								}
							}
							else
							{
								XmlItem xmlItem;

								if(m_mapXeItem.TryGetValue(xeChild, out xmlItem) && xmlItem != null && xmlItem is Basic)
								{
									m_mapBaseIdCtrlUI.Add(baseId, (Basic)xmlItem);
								}
							}
						}
						checkUICtrlBaseIdByXe((XmlElement)xnChild);
					}
				}
			}
		}
		public void checkAllUICtrlBaseId()
		{
			m_mapBaseIdCtrlUI = new Dictionary<string, Basic>();
			checkUICtrlBaseIdByXe(m_xmlDoc.DocumentElement);
		}

		static private void checkUICtrlBaseIdByXe(string xmlPath, XmlElement xe, Dictionary<string, XmlElement> mapBaseIdXe)
		{
			foreach (XmlNode xnChild in xe.ChildNodes)
			{
				CtrlDef_T ctrlDef;

				if (xnChild is XmlElement)
				{
					if (xnChild.Name == "BoloUI")
					{
						checkUICtrlBaseIdByXe(xmlPath, (XmlElement)xnChild, mapBaseIdXe);
					}
					else if (CtrlDef_T.tryGetCtrlDef(xnChild.Name, out ctrlDef))
					{
						XmlElement xeChild = (XmlElement)xnChild;
						string baseId = xeChild.GetAttribute("baseID");

						if (baseId != "")
						{
							XmlElement xeCtrl;

							if (mapBaseIdXe.TryGetValue(baseId, out xeCtrl))
							{
								Public.ResultLink.createResult("\r\n" + xmlPath + " - 存在相同的\"baseID\"属性。("
									+ baseId + ")", Public.ResultType.RT_ERROR, xmlPath, true);
							}
							else
							{
								mapBaseIdXe.Add(baseId, xeCtrl);
							}
						}
						checkUICtrlBaseIdByXe(xmlPath, (XmlElement)xnChild, mapBaseIdXe);
					}
				}
			}
		}
		static public void checkAllBoloUIXmlControlBaseId()
		{
			if(Directory.Exists(Setting.s_projPath))
			{
				DirectoryInfo dri = new DirectoryInfo(Setting.s_projPath);

				foreach(FileInfo fi in dri.GetFiles("*.xml"))
				{
					XmlDocument xmlDoc = new XmlDocument();

					try
					{
						xmlDoc.Load(fi.FullName);
					}
					catch
					{
						continue;
					}

					if(xmlDoc.DocumentElement.Name == "BoloUI")
					{
						checkUICtrlBaseIdByXe(fi.FullName, xmlDoc.DocumentElement, new Dictionary<string, XmlElement>());
					}
				}
			}
		}
		static private void dealSingleXeSkinUsingCount(XmlElement xeRoot)
		{
			foreach (XmlNode xn in xeRoot.ChildNodes)
			{
				if (xn is XmlElement)
				{
					XmlElement xe = (XmlElement)xn;
					string skinName = xe.GetAttribute("skin");
					List<SkinUsingCount_T> lstSkinCount;

					if (skinName != "" && Setting.s_mapSkinUsingCount.TryGetValue(skinName, out lstSkinCount) &&
						lstSkinCount != null && lstSkinCount.Count > 0)
					{
						foreach(SkinUsingCount_T skinCountDef in lstSkinCount)
						{
							skinCountDef.m_count++;
						}
					}
					dealSingleXeSkinUsingCount(xe);
				}
			}
		}
		static public void refreshSkinUsingCount()
		{
			if (Directory.Exists(Project.Setting.s_projPath))
			{
				DirectoryInfo dri = new DirectoryInfo(Project.Setting.s_projPath);

				foreach (FileInfo fi in dri.GetFiles("*.xml"))
				{
					XmlDocument docXml = new XmlDocument();

					try
					{
						docXml.Load(fi.FullName);
					}
					catch
					{
						continue;
					}
					dealSingleXeSkinUsingCount(docXml.DocumentElement);
				}
			}
		}
		static public void showNotUsingSkin()
		{
			string countReport = "";
			int notUsingCount = 0;

			Project.Setting.refreshSkinIndex();
			refreshSkinUsingCount();
			Public.ResultLink.createResult("\r\n" + "共" + Setting.s_mapSkinUsingCount.Count + "个皮肤名，其中", Public.ResultType.RT_INFO, null, true);
			foreach(KeyValuePair<string, List<SkinUsingCount_T>> pairSkinCount in Setting.s_mapSkinUsingCount)
			{
				if (pairSkinCount.Value[0].m_count == 0 && pairSkinCount.Key.IndexOf("{") == -1)
				{
					notUsingCount++;
					foreach (SkinUsingCount_T skinCountDef in pairSkinCount.Value)
					{
						countReport += "\r\n皮肤文件[" + skinCountDef.m_groupName + ".xml" + "]中的皮肤[" + pairSkinCount.Key + "]没有被引用到。";
// 						Public.ResultLink.createResult("\r\n皮肤文件[", Public.ResultType.RT_INFO, null, true);
// 						Public.ResultLink.createResult(skinCountDef.m_groupName + ".xml", Public.ResultType.RT_INFO,
// 							Setting.s_skinPath + "\\" + skinCountDef.m_groupName + ".xml", true);
// 						Public.ResultLink.createResult("]中的皮肤[" + pairSkinCount.Key + "]没有被引用到。", Public.ResultType.RT_INFO, null, true);
					}
				}
			}
			Public.ResultLink.createResult(notUsingCount + "个皮肤没有被引用。", Public.ResultType.RT_INFO, null, true);
			StreamWriter sw;

			sw = File.CreateText(Setting.s_projPath + "\\skinCount.txt");
			sw.WriteLine(countReport);
			sw.Close();
		}

		public void refreshControl(string skinName = "")
		{
			m_mapCtrlUI = new Dictionary<string, BoloUI.Basic>();
			m_mapImageSize = new Dictionary<string, long>();
			m_mapSkin = new Dictionary<string, BoloUI.ResBasic>();
			m_mapXeItem = new Dictionary<XmlElement, XmlItem>();
			m_isOnlySkin = true;
			clearNodeTreeFrame();
			m_xeRoot = m_xmlDoc.DocumentElement;

			if (m_xeRoot != null)
			{
				switch (m_xeRoot.Name)
				{
					case "BoloUI":
						{
							m_showGL = true;
							m_treeUI = new BoloUI.Basic(m_xeRoot, this, true);
							m_treeSkin = new BoloUI.ResBasic(m_xeRoot, this, null);

							MainWindow.s_pW.mx_treeCtrlFrame.Items.Add(m_treeUI);
							m_treeUI.mx_text.Text = StringDic.getFileNameWithoutPath(m_openedFile.m_path);
							m_treeUI.ToolTip = m_openedFile.m_path;
							MainWindow.s_pW.mx_treeSkinFrame.Items.Add(m_treeSkin);
							m_treeSkin.mx_text.Text = StringDic.getFileNameWithoutPath(m_openedFile.m_path);
							m_treeSkin.ToolTip = m_openedFile.m_path;

							m_treeUI.Items.Clear();
							m_treeSkin.Items.Clear();

							XmlNodeList xnl = m_xeRoot.ChildNodes;

							foreach (XmlNode xnf in xnl)
							{
								if (xnf.NodeType == XmlNodeType.Element)
								{
									XmlElement xe = (XmlElement)xnf;
									CtrlDef_T ctrlPtr;
									SkinDef_T skinPtr;

									if (CtrlDef_T.tryGetCtrlDef(xe.Name, out ctrlPtr))
									{
										m_treeUI.Items.Add(new Basic(xe, this, false));
										m_isOnlySkin = false;
										m_xeRootCtrl = xe;
									}
									else if (SkinDef_T.tryGetSkinDef(xe.Name, out skinPtr))
									{
										if (skinName == "" || 
											((xe.Name == "skin" || xe.Name == "publicskin") && xe.GetAttribute("Name") == skinName))
										{
											ResBasic treeChild = new ResBasic(xe, this, skinPtr);

											//显示或隐藏皮肤节点
// 											if (xe.Name != "skin" && xe.Name != "publicskin")
// 											{
// 												m_treeSkin.Items.Add(treeChild);
// 											}
											m_treeSkin.Items.Add(treeChild);
											if (xe.Name == "skingroup")
											{
												refreshSkinDicByGroupName(xe.GetAttribute("Name"));
											}
										}
									}
								}
							}
							refreshSkinDicByGroupName("publicskin");
							MainWindow.s_pW.updateXmlToGL(this);
							if (m_openedFile.m_preViewSkinName != null && m_openedFile.m_preViewSkinName != "")
							{
								BoloUI.ResBasic skinBasic;

								if (m_mapSkin.TryGetValue(m_openedFile.m_preViewSkinName, out skinBasic))
								{
									skinBasic.changeSelectItem(m_openedFile.m_prePlusCtrlUI);
								}
								else
								{
									Public.ResultLink.createResult("\r\n没有找到该皮肤。(" + m_openedFile.m_preViewSkinName + ")",
										Public.ResultType.RT_WARNING);
								}
							}
						}
						break;
					case "UIImageResource":
						{
							m_showGL = false;
							if (ImageTools.ImageNesting.s_pW != null)
							{
								mx_root.AddChild(new PackImage(this, true));
							}
							else
							{
								mx_root.AddChild(new PackImage(this, false));
							}
						}
						break;
					default:
						Public.ResultLink.createResult("\r\n这不是一个有效的BoloUI或UIImageResource文件。", Public.ResultType.RT_ERROR);
						break;
				}
				if(m_showGL)
				{
					MainWindow.s_pW.showGLCtrl(true, true);
				}
				else
				{
					MainWindow.s_pW.showGLCtrl(false, true);
				}
			}
			else
			{
				Public.ResultLink.createResult("\r\nxml文件格式错误。", Public.ResultType.RT_ERROR);
			}
			checkAllUICtrlBaseId();
			refreshSkinDicForAll();
		}

		static public void addEventNodeByTemplateXmlElement(Dictionary<string, string> mapEvent, XmlElement xeTmpl)
		{
			if (mapEvent == null)
			{
				return;
			}
			foreach (XmlNode xnRow in xeTmpl.ChildNodes)
			{
				if (xnRow is XmlElement && xnRow.Name == "row")
				{
					string rowName = ((XmlElement)xnRow).GetAttribute("name");
					string rowTip = ((XmlElement)xnRow).GetAttribute("tip");

					mapEvent.Add(rowName, rowTip);
				}
			}
		}
		static public Dictionary<string, string> getCtrlEventMap(string ctrlName)
		{
			Dictionary<string, string> mapEvent = new Dictionary<string, string>();
			CtrlDef_T ctrlDef;
			XmlNode xnTmpls = MainWindow.s_pW.m_docEventConf.SelectSingleNode("Config").SelectSingleNode("template");

			if (xnTmpls != null)
			{
				if (CtrlDef_T.tryGetCtrlDef(ctrlName, out ctrlDef) && ctrlDef != null)
				{
					//控件节点的事件模板
					if (ctrlDef.m_hasPointerEvent)
					{
						XmlNode xnPoi = xnTmpls.SelectSingleNode("eventTmpls_pointer");

						if (xnPoi != null && xnPoi.NodeType == XmlNodeType.Element)
						{
							addEventNodeByTemplateXmlElement(mapEvent, (XmlElement)xnPoi);
						}
					}
					XmlNode xnBasic = xnTmpls.SelectSingleNode("eventTmpls_basic");

					if (xnBasic != null && xnBasic.NodeType == XmlNodeType.Element)
					{
						addEventNodeByTemplateXmlElement(mapEvent, (XmlElement)xnBasic);
					}
				}
				//所有节点的事件模板
				XmlNode xnCtrl = xnTmpls.SelectSingleNode("eventTmpls_" + ctrlName);

				if (xnCtrl != null && xnCtrl.NodeType == XmlNodeType.Element)
				{
					addEventNodeByTemplateXmlElement(mapEvent, (XmlElement)xnCtrl);
				}
			}

			return mapEvent;
		}
		static public void getElementLocation(XmlElement xe, List<int> lstLc)
		{
			if(lstLc == null)
			{
				return;
			}

			if (xe.ParentNode != null && xe.ParentNode is XmlElement)
			{
				int count = 0;
				foreach(XmlNode xn in xe.ParentNode.ChildNodes)
				{
					if(xn is XmlElement && xe == (XmlElement)xn)
					{
						lstLc.Insert(0, count);
						break;
					}
					count++;
				}
				getElementLocation((XmlElement)xe.ParentNode, lstLc);
			}

			return;
		}
		static public XmlElement getXeByOffset(XmlElement xeParent, int offset)
		{
			int count = 0;

			foreach(XmlNode xn in xeParent.ChildNodes)
			{
				if(count == offset)
				{
					if(xn is XmlElement)
					{
						return (XmlElement)xn;
					}
					else
					{
						return null;
					}
				}
				count++;
			}

			return null;
		}
		static public XmlElement getXeByLocationList(XmlDocument docXml, List<int> lstLc)
		{
			XmlElement xe = docXml.DocumentElement;

			foreach(int offset in lstLc)
			{
				if (xe != null)
				{
					xe = getXeByOffset(xe, offset);
				}
			}

			return xe;
		}
		static public int setEvent(XmlElement dstXe, string eName, string func)
		{
			XmlElement xeDel = null;

			foreach(XmlNode xn in dstXe.ChildNodes)
			{
				if(xn is XmlElement && xn.Name == "event")
				{
					XmlElement xe = (XmlElement)xn;

					if(xe.GetAttribute("type") == eName)
					{
						if (func == null || func == "")
						{
							xeDel = xe;

							break;
						}
						else
						{
							xe.SetAttribute("function", func);

							return 0;
						}
					}
				}
			}

			if(xeDel != null)
			{
				dstXe.RemoveChild(xeDel);

				return -1;
			}
			else
			{
				if (func == null || func == "")
				{
					return 0;
				}
				else
				{
					XmlElement xeEvent = dstXe.OwnerDocument.CreateElement("event");

					xeEvent.SetAttribute("type", eName);
					xeEvent.SetAttribute("function", func);
					dstXe.AppendChild(xeEvent);

					return 1;
				}
			}
		}
		static public void refreshShape(string path)
		{
			if (path != null && path != "")
			{
				if (Directory.Exists(path))
				{
					DirectoryInfo di = new DirectoryInfo(path);

					foreach (FileInfo fi in di.GetFiles())
					{
						if (fi.Extension == ".xml")
						{
							XmlDocument docSkin = new XmlDocument();
							bool isChange = false;

							try
							{
								docSkin.Load(fi.FullName);
							}
							catch
							{
								continue;
							}

							if (docSkin.DocumentElement.Name != "BoloUI")
							{
								continue;
							}
							foreach (XmlNode xnSkin in docSkin.DocumentElement.ChildNodes)
							{
								if (xnSkin is XmlElement && (xnSkin.Name == "skin" || xnSkin.Name == "publicskin"))
								{
									foreach (XmlNode xnAppr in xnSkin.ChildNodes)
									{
										if (xnAppr is XmlElement && (xnAppr.Name == "apperance"))
										{
											//用于多个textShape的情况
											//List<XmlElement> lstShape = new List<XmlElement>();
											for (int i = 0; i < xnAppr.ChildNodes.Count; i++)
											{
												XmlNode xnShape = xnAppr.ChildNodes[i];

												if (xnShape is XmlElement && xnShape.Name == "textShape")
												{
													if (i == xnAppr.ChildNodes.Count - 1)
													{
														break;
													}
													else
													{
														XmlOperation.HistoryNode.deleteXmlNode(
															MainWindow.s_pW,
															null,
															(XmlElement)xnShape);
														XmlOperation.HistoryNode.insertXmlNode(
															MainWindow.s_pW,
															null,
															(XmlElement)xnShape,
															(XmlElement)xnAppr,
															xnAppr.ChildNodes.Count);
														isChange = true;
													}
												}
											}
										}
									}
								}
							}

							if (isChange)
							{
								docSkin.Save(fi.FullName);
								Public.ResultLink.createResult("\r\n" + fi.Name, Public.ResultType.RT_INFO);
							}
						}
					}
				}
			}
		}
		static public void refreshAllShape()
		{
			refreshShape(Project.Setting.s_projPath);
			refreshShape(Project.Setting.s_skinPath);
		}

		public bool isSkinXmlControl()
		{
			if(File.Exists(m_parent.m_fileDef.m_path))
			{
				FileInfo fi = new FileInfo(m_parent.m_fileDef.m_path);

				if(fi.DirectoryName == Project.Setting.s_skinPath)
				{
					return true;
				}
			}

			return false;
		}
		public void saveCurStatus()
		{
			m_xmlDoc.Save(m_openedFile.m_path);
			m_openedFile.m_lstOpt.m_saveNode = MainWindow.s_pW.m_mapOpenedFiles[m_openedFile.m_path].m_lstOpt.m_curNode;
			m_openedFile.updateSaveStatus();
		}
	}
}
