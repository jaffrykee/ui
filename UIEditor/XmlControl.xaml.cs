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
		public XmlDocument m_xmlDoc;
		public XmlElement m_xeRootCtrl;
		public XmlElement m_xeRoot;
		public bool m_isOnlySkin;
		public BoloUI.Basic m_skinViewCtrlUI;

		public BoloUI.Basic m_treeUI;
		public BoloUI.ResBasic m_treeSkin;

		public XmlControl(FileTabItem parent, OpenedFile fileDef)
		{
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
			refreshXmlText();
			refreshControl();
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

				pW.openFileByPath(path);
				if(pW.m_mapOpenedFiles.TryGetValue(path, out skinFile))
				{
					if (skinFile.m_frame != null)
					{
						if (skinFile.m_frame.GetType().ToString() == "UIEditor.XmlControl")
						{
							XmlControl xmlCtrl = (XmlControl)skinFile.m_frame;
							BoloUI.ResBasic skinBasic;

							if(xmlCtrl.m_mapSkin.TryGetValue(skinName, out skinBasic))
							{
								skinBasic.changeSelectItem(ctrlUI);
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
		public bool findSkinAndSelect(string skinName, BoloUI.Basic ctrlUI = null)
		{
			string groupName;
			BoloUI.ResBasic skinBasic;

			if (m_mapSkin.TryGetValue(skinName, out skinBasic))
			{
				skinBasic.changeSelectItem(ctrlUI);
				//changeSelectSkinAndFile(MainWindow.s_pW, m_openedFile.m_path, skinName, ctrlUI);
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
		public void refreshBoloUIView(bool changeItem = false)
		{
			if(m_showGL)
			{
				if (m_isOnlySkin)
				{
					MainWindow.s_pW.mx_leftToolFrame.SelectedItem = MainWindow.s_pW.mx_skinFrame;
					MainWindow.s_pW.mx_ctrlFrame.IsEnabled = false;
					MainWindow.s_pW.mx_skinFrame.IsEnabled = true;
				}
				else
				{
					if (changeItem)
					{
						MainWindow.s_pW.mx_leftToolFrame.SelectedItem = MainWindow.s_pW.mx_ctrlFrame;
					}
					MainWindow.s_pW.mx_ctrlFrame.IsEnabled = true;
					MainWindow.s_pW.mx_skinFrame.IsEnabled = true;
				}
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
				MainWindow.s_pW.mx_debug.Text += "<警告>皮肤组：\"" + skinGroupName + "\"不存在，请检查路径：\"" + path + "\"。\r\n";
			}
		}
		public void refreshSkinDicByGroupName(string skinGroupName)
		{
			string path = MainWindow.s_pW.m_skinPath + "\\" + skinGroupName + ".xml";

			refreshSkinDicByPath(path, skinGroupName);
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
		public void refreshXmlText()
		{
			MainWindow.s_pW.m_isCanEdit = false;
			TextRange rag = new TextRange(MainWindow.s_pW.mx_xmlText.Document.ContentStart, MainWindow.s_pW.mx_xmlText.Document.ContentEnd);

			rag.Text = getOutXml(m_xmlDoc);
			MainWindow.s_pW.refreshXmlTextTip();
			//rag.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
			MainWindow.s_pW.m_isCanEdit = true;
		}
		public void refreshControl()
		{
			m_mapCtrlUI = new Dictionary<string, BoloUI.Basic>();
			m_mapSkinLink = new Dictionary<string, string>();
			m_mapSkin = new Dictionary<string, BoloUI.ResBasic>();
			m_mapXeItem = new Dictionary<XmlElement, XmlItem>();
			m_isOnlySkin = true;
			m_skinViewCtrlUI = null;
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
							m_treeUI.IsExpanded = true;
							MainWindow.s_pW.mx_treeSkinFrame.Items.Add(m_treeSkin);
							m_treeSkin.mx_radio.Content = "_" + StringDic.getFileNameWithoutPath(m_openedFile.m_path);
							m_treeSkin.mx_radio.ToolTip = m_openedFile.m_path;
							m_treeSkin.IsExpanded = true;

							m_treeUI.Items.Clear();
							m_treeSkin.Items.Clear();

							XmlNodeList xnl = m_xeRoot.ChildNodes;

							//MainWindow.s_pW.mx_debug.Text += ("未被解析的项目：\r\n");
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
										ResBasic treeChild = new ResBasic(xe, this, skinPtr);
										m_treeSkin.Items.Add(treeChild);
										treeChild.IsExpanded = false;
										if (xe.Name == "skingroup")
										{
											refreshSkinDicByGroupName(xe.GetAttribute("Name"));
										}
									}
								}
							}
							refreshSkinDicByGroupName("publicskin");
							refreshBoloUIView(true);
							MainWindow.s_pW.updateXmlToGLAtOnce(this);
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
					MainWindow.s_pW.mx_drawFrame.Visibility = System.Windows.Visibility.Visible;
				}
				else
				{
					MainWindow.s_pW.hiddenGLAttr();
					MainWindow.s_pW.mx_drawFrame.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
			else
			{
				MainWindow.s_pW.mx_debug.Text += ("<错误>xml文件格式错误。" + "\r\n");
			}
		}
	}
}
