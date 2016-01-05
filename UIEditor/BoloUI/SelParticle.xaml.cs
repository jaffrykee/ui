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
	public partial class SelParticle
	{
		static public SelParticle s_pW;

		public IAttrRow m_iRowParticle;
		public string m_pathTexiao;
		public TreeViewItem m_curParticle;
		public XmlDocument m_docView;
		public XmlElement m_xeParticle;
		public MsgManager m_msgMng;

		public SelParticle(IAttrRow iRowParticle)
		{
			s_pW = this;
			m_iRowParticle = iRowParticle;
			m_curParticle = null;

			InitializeComponent();

			this.Owner = MainWindow.s_pW;
			createViewDoc();
			refreshParticleTree();
		}

		public void createViewDoc()
		{
			m_docView = new XmlDocument();
			XmlElement xeRoot = m_docView.CreateElement("BoloUI");
			XmlElement xePanel = m_docView.CreateElement("panel");
			XmlElement xeSkin = m_docView.CreateElement("skin");
			XmlElement xeAppr = m_docView.CreateElement("apperance");
			XmlElement xeParticleShape = m_docView.CreateElement("particleShape");

			m_docView.AppendChild(xeRoot);
			xeRoot.AppendChild(xePanel);
			xeRoot.AppendChild(xeSkin);
			xeSkin.AppendChild(xeAppr);
			xeAppr.AppendChild(xeParticleShape);

			xePanel.SetAttribute("w", "960");
			xePanel.SetAttribute("h", "540");
			xePanel.SetAttribute("dock", "4");
			xePanel.SetAttribute("skin", "testParticle");
			xeSkin.SetAttribute("Name", "testParticle");
			xeAppr.SetAttribute("id", "0");
			xeParticleShape.SetAttribute("particleName", "");
			xeParticleShape.SetAttribute("Anchor", "3");

			m_xeParticle = xeParticleShape;
		}

		public void refreshParticleTree()
		{
			mx_rootItem.Items.Clear();

			if (MainWindow.s_pW != null && Directory.Exists(Project.Setting.getParticlePath()))
			{
				DirectoryInfo driParticle = new DirectoryInfo(Project.Setting.getParticlePath());

				m_pathTexiao = driParticle.FullName;
				foreach(FileInfo fiParticle in driParticle.GetFiles())
				{
					if (fiParticle.Extension == ".prefab")
					{
						TreeViewItem itemParticle = new TreeViewItem();

						itemParticle.Header = System.IO.Path.GetFileNameWithoutExtension(fiParticle.Name);
						itemParticle.ToolTip = fiParticle.FullName;
						itemParticle.Selected += mx_itemParticle_Selected;
						mx_rootItem.Items.Add(itemParticle);
					}
				}
			}
		}

		void mx_itemParticle_Selected(object sender, RoutedEventArgs e)
		{
			updateGL(Project.Setting.s_projPath, W2GTag.W2G_PATH);
			updateGL(Project.Setting.getParticlePath(), W2GTag.W2G_PATH_PARTICLE);
			updateGL("960:540:False:960:540", W2GTag.W2G_VIEWSIZE);
			if(sender is TreeViewItem)
			{
				m_curParticle = (TreeViewItem)sender;

				TreeViewItem itemParticle = (TreeViewItem)sender;

				m_xeParticle.SetAttribute("particleName", System.IO.Path.GetFileNameWithoutExtension(itemParticle.ToolTip.ToString()));

				string buffer = m_docView.InnerXml;

				updateGL("ParticleTest.xml", W2GTag.W2G_NORMAL_NAME);
				updateGL(buffer, W2GTag.W2G_NORMAL_DATA);
			}
		}

		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{
			s_pW = this;
			HwndSource source = PresentationSource.FromVisual(this) as HwndSource;

			if (source != null)
			{
				source.AddHook(WndProc);
			}

			m_msgMng = new MsgManager();
			mx_viewFrame.Child = m_msgMng.m_GLHost;
			m_msgMng.m_GLHost.MessageHook += new HwndSourceHook(ControlMsgFilter);
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
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			s_pW = null;
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
			if (m_curParticle != null)
			{
				m_iRowParticle.m_value = System.IO.Path.GetFileNameWithoutExtension(m_curParticle.ToolTip.ToString());
			}
			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
