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
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;
using System.Windows.Threading;

namespace UIEditor
{
	public partial class XmlControl : UserControl
	{
		public FileTabItem m_parent;
		public OpenedFile m_openedFile;
		public bool m_showGL;
		//以baseID为索引的UI们
		public Dictionary<string, BoloUI.Basic> m_mapCtrlUI;
		public Dictionary<string, string> m_mapSkinLink;
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

			MainWindow.s_pW.mx_debug.Text += "=====" + m_openedFile.m_path + "=====\r\n";
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
			if (MainWindow.s_pW.mx_treeCtrlFrame.Items.Count > 0 && MainWindow.s_pW.mx_treeCtrlFrame.Items[0] != null)
			{
				TreeViewItem firstItem = (TreeViewItem)MainWindow.s_pW.mx_treeCtrlFrame.Items[0];

				if (firstItem.Header.ToString() == StringDic.getFileNameWithoutPath(m_openedFile.m_path))
				{
					MainWindow.s_pW.mx_treeCtrlFrame.Items.Clear();
					MainWindow.s_pW.mx_treeSkinFrame.Items.Clear();
				}
			}
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
				pW.mx_debug.Text += "<警告>文件：\"" + path + "\"不存在，请检查路径。\r\n";
			}
		}
		static public void changeSelectSkinAndFile(MainWindow pW, string path, string skinName, BoloUI.Basic ctrlUI = null)
		{
			if (File.Exists(path))
			{
				OpenedFile skinFile;

				//pW.openFileByPath(path, skinName);
				pW.openFileByPath(path);
				if(pW.m_mapOpenedFiles.TryGetValue(path, out skinFile))
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
								pW.mx_debug.Text += "<警告>然而，并没有这个皮肤。(" + skinName + ")\r\n";
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
				pW.mx_debug.Text += "<警告>文件：\"" + path + "\"不存在，请检查路径。\r\n";
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
		public bool findSkinAndSelect(string skinName, BoloUI.Basic ctrlUI = null)
		{
			string groupName;
			BoloUI.ResBasic skinBasic;

			if (m_mapSkin.TryGetValue(skinName, out skinBasic))
			{
				skinBasic.changeSelectItem(ctrlUI);
				setAllChildExpand(skinBasic);
			}
			else if (m_mapSkinLink.TryGetValue(skinName, out groupName))
			{
				string path = MainWindow.s_pW.m_skinPath + "\\" + groupName + ".xml";

				changeSelectSkinAndFile(MainWindow.s_pW, path, skinName, ctrlUI);
			}
			else
			{
				MainWindow.s_pW.mx_debug.Text += "<警告>然而，并没有这个皮肤。(" + skinName + ")\r\n";
			}

			return false;
		}
		public string tryFindSkin(string skinName)
		{
			string groupName;
			BoloUI.ResBasic skinBasic;

			if (m_mapSkin.TryGetValue(skinName, out skinBasic))
			{
				return m_openedFile.m_path;
			}
			else if (m_mapSkinLink.TryGetValue(skinName, out groupName))
			{
				string path = MainWindow.s_pW.m_skinPath + "\\" + groupName + ".xml";

				return path;
			}
			else
			{
				return "";
			}
		}
		public void refreshVRect()
		{
			string msgData = "";

			foreach(KeyValuePair<string, BoloUI.Basic> pairCtrlUI in m_mapCtrlUI.ToList())
			{
				XmlItem item;

				if (m_mapXeItem.TryGetValue(pairCtrlUI.Value.m_xe, out item))
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
				skinDoc.Load(path);
				XmlNode xn = skinDoc.SelectSingleNode("BoloUI");

				if (xn != null)
				{
					XmlNodeList xnlSkin = xn.ChildNodes;

					foreach (XmlNode xnfSkin in xnlSkin)
					{
						if (xnfSkin.NodeType == XmlNodeType.Element)
						{
							XmlElement xeSkin = (XmlElement)xnfSkin;

							if (xeSkin.Name == "skin" || xeSkin.Name == "publicskin")
							{
								if (xeSkin.GetAttribute("Name") != "")
								{
									string nullStr;
									if (!m_mapSkinLink.TryGetValue(xeSkin.GetAttribute("Name"), out nullStr))
									{
										m_mapSkinLink.Add(xeSkin.GetAttribute("Name"), skinGroupName);
									}
									else
									{
										m_mapSkinLink[xeSkin.GetAttribute("Name")] = skinGroupName;
										string errorInfo = "<错误>文件:\"" + path + "\"中，存在重复Name的皮肤(" +
											xeSkin.GetAttribute("Name") + ")，前一个同名的皮肤将不能正确显示。\r\n";

										MainWindow.s_pW.mx_debug.Text += errorInfo;
										//Public.ErrorInfo.addToErrorInfo(errorInfo);
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
					MainWindow.s_pW.mx_debug.Text += "<警告>皮肤组：\"" + skinGroupName + "\"不存在，请检查路径：\"" + path + "\"。\r\n";
				}
			}
		}
		public void refreshSkinDicByGroupName(string skinGroupName)
		{
			string path = MainWindow.s_pW.m_skinPath + "\\" + skinGroupName + ".xml";

			refreshSkinDicByPath(path, skinGroupName);
		}
		public void refreshSkinDicForAll()
		{
			m_mapSkinLink.Clear();
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
					addRunAndLinkItem(para, "\n", xeItem, Colors.Blue, ref oldRun, ref diffRun);
				}
				switch(xn.NodeType)
				{
					case XmlNodeType.Element:
						{
							XmlElement xe = (XmlElement)xn;

							addRunAndLinkItem(para, strTabs + "<", xeItem, Colors.Blue, ref oldRun, ref diffRun);
							if (xeItem != null)
							{
								xeItem.m_runXeName = addRunAndLinkItem(para, xe.Name, xeItem, Colors.DarkMagenta, ref oldRun, ref diffRun);
							}
							else
							{
								addRunAndLinkItem(para, xe.Name, xeItem, Colors.DarkMagenta, ref oldRun, ref diffRun);
							}
							foreach(XmlAttribute attr in xe.Attributes)
							{
								//addRunAndLinkItem(para, "\n" + strTabs + "    ", xeItem, Colors.Blue, ref oldRun, ref diffRun);
								addRunAndLinkItem(para, " " + attr.Name, xeItem, Colors.Red, ref oldRun, ref diffRun);
								addRunAndLinkItem(para, "=\"" + attr.InnerXml + "\"", xeItem, Colors.Blue, ref oldRun, ref diffRun);
							}
							if(xe.OuterXml.IndexOf("/>") == xe.OuterXml.Length - "/>".Length)
							{
								endStr = "/>";
							}
							else
							{
								endStr = ">";
							}
							addRunAndLinkItem(para, endStr, xeItem, Colors.Blue, ref oldRun, ref diffRun);
						}
						break;
					case XmlNodeType.EndElement:
						{
						}
						break;
					case XmlNodeType.XmlDeclaration:
						{
							addRunAndLinkItem(para, xn.OuterXml, xeItem, Colors.Red, ref oldRun, ref diffRun);
						}
						break;
					case XmlNodeType.Comment:
						{
							addRunAndLinkItem(para, xn.OuterXml, xeItem, Colors.DarkSeaGreen, ref oldRun, ref diffRun);
						}
						break;
					default:
						{
							addRunAndLinkItem(para, xn.OuterXml, xeItem, Colors.Black, ref oldRun, ref diffRun);
						}
						break;
				}
				refreshXmlSign(xn, para, ref oldRun, ref diffRun, deep + 1);
				if (xn.NodeType == XmlNodeType.Element && endStr == ">")
				{
					addRunAndLinkItem(para, "\n" + strTabs, xeItem, Colors.Blue, ref oldRun, ref diffRun);
					addRunAndLinkItem(para, "</", xeItem, Colors.Blue, ref oldRun, ref diffRun);
					addRunAndLinkItem(para, xn.Name, xeItem, Colors.DarkMagenta, ref oldRun, ref diffRun);
					addRunAndLinkItem(para, ">", xeItem, Colors.Blue, ref oldRun, ref diffRun);
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

								if(run.Text.Last() == '<')
								{
									if (run.NextInline != null && run.NextInline is Run)
									{
										Run runXe = (Run)run.NextInline;
										string strName = runXe.Text;

										if (xeCount.Name == strName && m_mapXeItem.TryGetValue(xeCount, out item) && item != null)
										{
											item.m_runXeName = runXe;
											XmlElement xeNext = getNextXmlElement(xeCount);

											if(xeNext != null)
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
		public void refreshControl(string skinName = "")
		{
			m_mapCtrlUI = new Dictionary<string, BoloUI.Basic>();
			m_mapSkinLink = new Dictionary<string, string>();
			m_mapSkin = new Dictionary<string, BoloUI.ResBasic>();
			m_mapXeItem = new Dictionary<XmlElement, XmlItem>();
			m_isOnlySkin = true;
			MainWindow.s_pW.mx_treeCtrlFrame.Items.Clear();
			MainWindow.s_pW.mx_treeSkinFrame.Items.Clear();
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
							m_treeUI.mx_radio.Content = "_" + StringDic.getFileNameWithoutPath(m_openedFile.m_path);
							m_treeUI.mx_radio.ToolTip = m_openedFile.m_path;
							MainWindow.s_pW.mx_treeSkinFrame.Items.Add(m_treeSkin);
							m_treeSkin.mx_radio.Content = "_" + StringDic.getFileNameWithoutPath(m_openedFile.m_path);
							m_treeSkin.mx_radio.ToolTip = m_openedFile.m_path;

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

									if (MainWindow.s_pW.m_mapCtrlDef.TryGetValue(xe.Name, out ctrlPtr))
									{
										m_treeUI.Items.Add(new Basic(xe, this, false));
										m_isOnlySkin = false;
										m_xeRootCtrl = xe;
									}
									else if (MainWindow.s_pW.m_mapSkinTreeDef.TryGetValue(xe.Name, out skinPtr))
									{
										if (skinName == "" || 
											((xe.Name == "skin" || xe.Name == "publicskin") && xe.GetAttribute("Name") == skinName))
										{
											ResBasic treeChild = new ResBasic(xe, this, skinPtr);

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
									MainWindow.s_pW.mx_debug.Text += "<警告>然而，并没有这个皮肤。(" + m_openedFile.m_preViewSkinName + ")\r\n";
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
							MainWindow.s_pW.mx_debug.Text += ("<提示>todo。" + "\r\n");
						}
						break;
					default:
						MainWindow.s_pW.mx_debug.Text += ("<错误>这不是一个有效的BoloUI或UIImageResource文件。" + "\r\n");
						break;
				}
				if(m_showGL)
				{
					MainWindow.s_pW.showGLCtrl(true);
				}
				else
				{
					MainWindow.s_pW.showGLCtrl(false);
				}
			}
			else
			{
				MainWindow.s_pW.mx_debug.Text += ("<错误>xml文件格式错误。" + "\r\n");
			}
		}
	}
}
