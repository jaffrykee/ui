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
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text.RegularExpressions;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.BoloUI
{
	public partial class SelSkin
	{
		public string m_path;
		public XmlDocument m_docView;
		public XmlElement m_testCtrl;
		public bool m_isInitPath;
		public TreeViewItem m_curSkin;
		public IAttrRow m_iRowSkin;
		public MsgManager m_msgMng;

		public static SelSkin s_pW;

		public SelSkin(string path, string xmlData, IAttrRow iRowSkin)
		{
			s_pW = this;
			m_isInitPath = false;
			m_path = path;
			m_iRowSkin = iRowSkin;
			m_curSkin = null;
			InitializeComponent();

			m_docView = new XmlDocument();
			XmlElement xeRoot = m_docView.CreateElement("BoloUI");
			XmlElement xePanel = m_docView.CreateElement("panel");

			xePanel.SetAttribute("w", "960");
			xePanel.SetAttribute("h", "540");
			xePanel.SetAttribute("dock", "4");
			xePanel.InnerXml = xmlData;
			m_testCtrl = (XmlElement)xePanel.FirstChild;
			string attrW = m_testCtrl.GetAttribute("w");
			string attrH = m_testCtrl.GetAttribute("h");
			string attrText = m_testCtrl.GetAttribute("text");
			m_testCtrl.RemoveAllAttributes();
			m_testCtrl.SetAttribute("baseID", "selSkinTestCtrl");
			if (attrW == "")
			{
				attrW = "300";
			}
			if (attrH == "")
			{
				attrH = "200";
			}
			if (attrText == "")
			{
				attrText = "测试Test123";
			}
			m_testCtrl.SetAttribute("w", attrW);
			m_testCtrl.SetAttribute("h", attrH);
			m_testCtrl.SetAttribute("text", attrText);
			m_docView.AppendChild(xeRoot);
			#region 显示皮肤树
			OpenedFile fileDef;
			if(MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(m_path, out fileDef))
			{
				if(fileDef != null && fileDef.m_frame != null && fileDef.m_frame is XmlControl)
				{
					XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;
					Dictionary<string, string> mapLocalGroup = new Dictionary<string,string>();

					if(xmlCtrl.m_xmlDoc != null && xmlCtrl.m_xmlDoc.DocumentElement.Name == "BoloUI")
					{
						XmlDocument xmlDoc = xmlCtrl.m_xmlDoc;
						foreach(XmlNode xn in xmlDoc.DocumentElement.ChildNodes)
						{
							if(xn.NodeType == XmlNodeType.Element)
							{
								XmlElement xe = (XmlElement)xn;

								switch(xe.Name)
								{
									case "skingroup":
										{
											#region 本地皮肤组
											string groupName = xe.GetAttribute("Name");
											string tmp;

											if (!mapLocalGroup.TryGetValue(groupName, out tmp))
											{
												mapLocalGroup.Add(groupName, "");
											}
											addItemByGroupName(groupName, mx_localGroup);
											#endregion
										}
										break;
									case "publicskin":
									case "skin":
										{
											#region 本地皮肤
											string skinName = xe.GetAttribute("Name");

											if(skinName != "")
											{
												TreeViewItem skinItem = new TreeViewItem();

												skinItem.Header = skinName;
												skinItem.ToolTip = m_path;
												skinItem.Selected += skinItem_Selected;

												mx_localSkin.Items.Add(skinItem);

												XmlElement xeSkin = m_docView.CreateElement(xe.Name);
												xeSkin.SetAttribute("Name", skinName);
												xeSkin.InnerXml = xe.InnerXml;
												xeRoot.AppendChild(xeSkin);
											}
											#endregion
										}
										break;
									default:
										break;
								}
							}
						}

						#region 未包含的皮肤组
						DirectoryInfo di = new DirectoryInfo(Project.Setting.s_projPath + "\\skin\\");
						FileInfo[] arrFi = di.GetFiles("*.xml");

						foreach(FileInfo fi in arrFi)
						{
							string groupName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
							string tmp;

							if (!mapLocalGroup.TryGetValue(groupName, out tmp))
							{
								addItemByGroupName(groupName, mx_otherGroup);
							}

							XmlElement xeGroup = m_docView.CreateElement("skingroup");
							xeGroup.SetAttribute("Name", groupName);
							xeRoot.AppendChild(xeGroup);
						}
						#endregion
					}
				}
			}
			#endregion

			xeRoot.AppendChild(xePanel);

			this.Owner = MainWindow.s_pW;
		}
		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{
			HwndSource source = PresentationSource.FromVisual(this) as HwndSource;

			if (source != null)
			{
				source.AddHook(WndProc);
			}

			m_msgMng = new MsgManager();
			mx_viewFrame.Child = m_msgMng.m_GLHost;
			m_msgMng.m_GLHost.MessageHook += new HwndSourceHook(ControlMsgFilter);
		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			s_pW = null;
		}

		private void skinItem_Selected(object sender, RoutedEventArgs e)
		{
			if(m_isInitPath == false)
			{
				string buffer = m_docView.InnerXml;

				updateGL(Project.Setting.s_projPath, W2GTag.W2G_PATH);
				updateGL(Project.Setting.getParticlePath(), W2GTag.W2G_PATH_PARTICLE);
				updateGL("960:540:False:960:540", W2GTag.W2G_VIEWSIZE);
				m_isInitPath = true;
				updateGL("selSkinTest.xml", W2GTag.W2G_NORMAL_NAME);
				updateGL(buffer, W2GTag.W2G_NORMAL_DATA);
			}
			if(sender is TreeViewItem)
			{
				m_curSkin = (TreeViewItem)sender;

				m_testCtrl.SetAttribute("skin", m_curSkin.Header.ToString());
				updateGL("selSkinTest.xml:selSkinTestCtrl:skin:" + m_curSkin.Header.ToString(), W2GTag.W2G_NORMAL_UPDATE);
			}
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)//根窗体消息响应
		{
			switch (msg)
			{
				case MainWindow.WM_COPYDATA:
					break;
				case MainWindow.WM_QUIT:
					if (!m_msgMng.m_GLHost.m_process.HasExited)
					{
						m_msgMng.m_GLHost.m_process.Kill();
					}
					break;
				case MainWindow.WM_DESTROY:
					if (!m_msgMng.m_GLHost.m_process.HasExited)
					{
						m_msgMng.m_GLHost.m_process.Kill();
					}
					break;
				default:
					break;
			}

			return hwnd;
		}
		private IntPtr ControlMsgFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)//响应主逻辑
		{
			handled = false;
			switch (msg)
			{
				case MainWindow.WM_COPYDATA:
					{
						G2WTag tag;
						string strData = "";

						unsafe
						{
							COPYDATASTRUCT msgData = *(COPYDATASTRUCT*)lParam;
							strData = Marshal.PtrToStringAnsi(msgData.lpData, msgData.cbData);
							tag = (G2WTag)((COPYDATASTRUCT*)lParam)->dwData;
						}
						switch (tag)
						{
							case G2WTag.G2W_HWND:
								m_msgMng.m_hwndGL = wParam;
								break;
							default:
								break;
						}
					}
					break;
				default:
					break;
			}

			return IntPtr.Zero;
		}
		public void updateGL(string buffer, W2GTag msgTag = W2GTag.W2G_NORMAL_DATA)
		{
			int len;
			byte[] charArr;
			COPYDATASTRUCT_SENDEX msgData;

			if (msgTag == W2GTag.W2G_PATH)
			{
				string resPath = MainWindow.getResPath(buffer);

				if (resPath != "")
				{
					charArr = Encoding.Default.GetBytes(resPath + "|" + buffer);
				}
				else
				{
					charArr = Encoding.Default.GetBytes(buffer);
				}
			}
			else
			{
				charArr = Encoding.UTF8.GetBytes(buffer);
			}
			len = charArr.Length;
			unsafe
			{
				fixed (byte* tmpBuff = charArr)
				{
					msgData.dwData = (IntPtr)msgTag;
					msgData.lpData = (IntPtr)tmpBuff;
					msgData.cbData = len + 1;
					MainWindow.SendMessage(m_msgMng.m_hwndGL, MainWindow.WM_COPYDATA, (int)m_msgMng.m_hwndGLParent, ref msgData);
				}
			}
		}

		void addItemByGroupName(string groupName, TreeViewItem dstItem)
		{
			if (groupName != "")
			{
				string skinGroupPath = Project.Setting.s_projPath + "\\skin\\" + groupName + ".xml";

				if (File.Exists(skinGroupPath))
				{
					TreeViewItem lgItem = new TreeViewItem();

					lgItem.Header = groupName;
					lgItem.ToolTip = skinGroupPath;
					lgItem.IsExpanded = false;
					dstItem.Items.Add(lgItem);

					XmlDocument docGroup = new XmlDocument();

					docGroup.Load(skinGroupPath);
					foreach (XmlNode xng in docGroup.DocumentElement.ChildNodes)
					{
						if (xng.NodeType == XmlNodeType.Element && (xng.Name == "skin" || xng.Name == "publicskin"))
						{
							XmlElement xeg = (XmlElement)xng;
							string xegName = xeg.GetAttribute("Name");

							if (xegName != "")
							{
								TreeViewItem skinItem = new TreeViewItem();

								skinItem.Header = xegName;
								skinItem.ToolTip = lgItem.ToolTip.ToString();
								skinItem.Selected += skinItem_Selected;
								lgItem.Items.Add(skinItem);
							}
						}
					}
				}
			}
		}
		private void mx_search_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (mx_search.Text != "")
			{
				MainWindow.refreshSearch(mx_rootItem, null);
				MainWindow.refreshSearch(mx_rootItem, mx_search.Text.ToString());
			}
			else
			{
				MainWindow.refreshSearch(mx_rootItem, null);
			}
		}

		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if (m_curSkin != null)
			{
				if (m_curSkin.Parent != null && m_curSkin.Parent is TreeViewItem &&
					((TreeViewItem)m_curSkin.Parent).Parent == mx_otherGroup &&
					((TreeViewItem)m_curSkin.Parent).Header.ToString() != "publicskin")
				{
					XmlElement newXe = m_iRowSkin.m_parent.m_xe.OwnerDocument.CreateElement("skingroup");

					newXe.SetAttribute("Name", ((TreeViewItem)m_curSkin.Parent).Header.ToString());
					m_iRowSkin.m_parent.m_xmlCtrl.m_treeSkin.addResItem(newXe);
				}
				m_iRowSkin.m_value = m_curSkin.Header.ToString();
			}
			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
