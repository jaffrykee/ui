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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;
using UIEditor.Project;
using UIEditor.XmlOperation.XmlAttr;
using UIEditor.Public;
using UIEditor.Project.PlugIn;

namespace UIEditor
{
	#region 皮肤属性文本
	public enum ApprSuf
	{
		NORMAL = 0,					//正常状态
		PRESSED = 1,				//按下状态
		DISABLED = 2,				//禁用状态
		CHECKED_NORMAL = 3,			//选中 - 正常状态
		CHECKED_PRESSED = 4,		//选中 - 按下状态
		CHECKED_DISABLED = 5,		//选中 - 禁用状态
		NORMAL_BORDER = 6,			//正常边框
		NORMAL_FILLED = 7,			//正常填充
		NORMAL_BACKGROUND = 8,		//正常背景
		NORMAL_FILLEDHEAD = 9,		//正常填充头
		NORMAL_FILLEDTAIL = 10,		//正常填充尾
		SELECT = 11,				//选中
		NORMAL_FILLEDSLIDER = 12,	//正常填充滑块
		TIPS = 13,					//tips
		MESSAGE_TIPS_NORMAL = 14,	//消息提示正常
		MESSAGE_TIPS_LIGHT = 15,	//消息提示亮
		NORMAL_PRE_GROWTH = 16,		//预增长
		KEYSELECT_NOMAL = 17,		//按键选中
		KEYSELECT_CHECK = 18,		//按键选中 - 选中
		MAX
	};
	#endregion

	public partial class MainWindow
	{
		public static MainWindow s_pW;
		public Dictionary<string, OpenedFile> m_mapOpenedFiles;
		public Dictionary<string, IncludeFile> m_mapIncludeFiles;
		//public Dictionary<string, SkinIndex> m_mapSkinIndex;
		public Dictionary<string, ImageIndex> m_mapImageIndex;

		//已过时
// 		public Dictionary<string, CtrlDef_T> m_mapCtrlDef;
// 		public Dictionary<string, CtrlDef_T> m_mapPanelCtrlDef;
// 		public Dictionary<string, CtrlDef_T> m_mapBasicCtrlDef;
// 		public Dictionary<string, CtrlDef_T> m_mapHasBasicCtrlDef;
// 		public Dictionary<string, CtrlDef_T> m_mapEnInsertCtrlDef;
// 		public Dictionary<string, CtrlDef_T> m_mapEnInsertAllCtrlDef;

		//已过时
// 		public Dictionary<string, SkinDef_T> m_mapSkinTreeDef;
// 		public Dictionary<string, SkinDef_T> m_mapSkinAllDef;
// 		public Dictionary<string, Dictionary<string, AttrDef_T>> m_mapSkinAttrDef;
		public StringDic m_strDic;

		public Dictionary<XmlElement, BoloUI.SelButton> m_mapXeSel;
		public float m_dpiSysX;
		public float m_dpiSysY;
		//public AttrList m_otherAttrList;
		public SkinEditor mx_skinEditor;
		public bool m_vCtrlName;
		public bool m_vCtrlId;
		public bool m_isLockMsgPath;

		public MsgManager m_msgMng;

		private void refreshScreenStatus()
		{
			updateGL(m_screenWidth.ToString() + ":" + m_screenHeight.ToString() + ":" + m_isMoba.ToString() + ":" +
				m_screenWidthBasic.ToString() + ":" + m_screenHeightBasic.ToString(), W2GTag.W2G_VIEWSIZE);
		}
		private int mt_screenWidth;
		public int m_screenWidth
		{
			get { return mt_screenWidth; }
			set
			{
				mt_screenWidth = value;
				if (mx_scrollGrid != null)
				{
					mx_scrollGrid.Width = value;
					refreshScreenStatus();
				}
			}
		}
		private int mt_screenHeight;
		public int m_screenHeight
		{
			get { return mt_screenHeight; }
			set
			{
				mt_screenHeight = value;
				if (mx_scrollGrid != null)
				{
					mx_scrollGrid.Height = value;
					refreshScreenStatus();
				}
			}
		}
		public bool m_isMoba;
		public int m_screenWidthBasic;
		private int mt_screenHeightBasic;
		public int m_screenHeightBasic
		{
			get { return mt_screenHeightBasic; }
			set
			{
				mt_screenHeightBasic = value;
				refreshScreenStatus();
			}
		}

		public XmlDocument m_xdTest;
		public XmlElement m_xeTest;
		public string m_strTestXml;

		public string m_curFile;	//todo

		public bool m_isMouseDown;
		public bool m_isCtrlMoved;
		public int m_downX;
		public int m_downY;
		public string m_pasteFilePath;

		public string m_curLang;

		public const string conf_pathGlApp = @".\dsuieditor.exe";
		public const string conf_pathGlApp_New = @".\SSUIEditor.exe";
		public const string conf_pathConf = @".\conf.xml";
		public const string conf_pathConfDefault = @".\data\conf.xml";
		public const string conf_pathConfEvent = @".\data\conf_event.xml";
		public const string conf_pathPlugInBoloUI = @".\data\PlugIn\BoloUI\";
		public const string conf_pathStringDic = @".\data\Lang\";
		public XmlDocument m_docConf;
		public XmlDocument m_docEventConf;
		public bool m_isDebug;
		public string m_pathGlApp;

		private void refreshStatusBar()
		{
			mx_status.Text = mb_status0 + "\t\t" + mb_status1 + "\t\t" + mb_status2 + "\t\t" + mb_status3;
		}
		private string mt_status0;
		public string mb_status0
		{
			get
			{
				return mt_status0;
			}
			set
			{
				mt_status0 = value;
				refreshStatusBar();
			}
		}
		private string mt_status1;
		public string mb_status1
		{
			get
			{
				return mt_status1;
			}
			set
			{
				mt_status1 = value;
				refreshStatusBar();
			}
		}
		private string mt_status2;
		public string mb_status2
		{
			get
			{
				return mt_status2;
			}
			set
			{
				mt_status2 = value;
				refreshStatusBar();
			}
		}
		private string mt_status3;
		public string mb_status3
		{
			get
			{
				return mt_status3;
			}
			set
			{
				mt_status3 = value;
				refreshStatusBar();
			}
		}

		private int maxLink = 100000;
		private int currentLinked;
		private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

		public static int s_countWin = 1;

		static public void initData()
		{
			DataNodeGroup.s_mapDataNodesDef = new Dictionary<string, Dictionary<string, DataNodeGroup>>();
			new Project.PlugIn.DataNodeGroup("BoloUI", "Skin");
			new Project.PlugIn.DataNodeGroup("BoloUI", "Ctrl");

			DataNodeGroup nodeGroup;

			if (DataNodeGroup.tryGetDataNodeGroup("BoloUI", "Ctrl", out nodeGroup) && nodeGroup != null)
			{
				foreach (KeyValuePair<string, DataNode> pairNodeDef in nodeGroup.m_mapDataNode.ToList())
				{
					if (pairNodeDef.Value is CtrlDef_T && ((CtrlDef_T)pairNodeDef.Value).isNormal())
					{
						CtrlDef_T ctrlDef = (CtrlDef_T)pairNodeDef.Value;
						string ctrlKey = pairNodeDef.Key;
						string ctrlName = MainWindow.s_pW.m_strDic.getWordByKey(ctrlKey);

						if (ctrlName == null || ctrlName == "")
						{
							ctrlName = ctrlKey;
						}
						Button btnCtrl = new Button();

						btnCtrl.Content = ctrlName;
						btnCtrl.ToolTip = ctrlKey;
						btnCtrl.Width = 60;
						btnCtrl.Height = MainWindow.s_pW.mx_ctrlNormalTitle.Height;
						btnCtrl.Margin = MainWindow.s_pW.mx_ctrlNormalTitle.Margin;
						btnCtrl.Click += MainWindow.s_pW.mx_addCtrl_Click;
						if (ctrlDef.isFrame())
						{
							MainWindow.s_pW.mx_ctrlFrame.Children.Add(btnCtrl);
						}
						else
						{
							MainWindow.s_pW.mx_ctrlNormal.Children.Add(btnCtrl);
						}
					}
				}
			}
		}

		public MainWindow()
		{
			s_pW = this;
			Project.Setting.s_skinPath = "";
			Project.Setting.s_projPath = "";
			m_msgMng = new MsgManager(true);
			m_mapIncludeFiles = new Dictionary<string, IncludeFile>();
			m_mapOpenedFiles = new Dictionary<string, OpenedFile>();
			m_strDic = new StringDic("zh-CN", conf_pathStringDic);
			m_isCanEdit = true;
			m_tLast = 0;
			m_hitCount = 0;
			mx_skinEditor = new SkinEditor();
			mt_status0 = "";
			mt_status1 = "";
			mt_status2 = "";
			mt_status3 = "";
			//system("taskkill /im conhost.exe /f");

			InitializeComponent();

			initData();

			OpenedFile.s_paraResult = new Paragraph();
			ResultLink.s_curResultFrame = OpenedFile.s_paraResult;
			m_isLoadOver = true;
			this.DataContext = this;

			mb_status0 = "就绪";
			m_screenWidth = 960;
			m_screenHeight = 540;
			m_isMoba = false;
			m_screenWidthBasic = 960;
			m_screenHeightBasic = 540;
			m_dpiSysX = 96.0f;
			m_dpiSysY = 96.0f;
			m_curFile = "";
			m_vCtrlName = true;
			m_vCtrlId = true;
			m_mapXeSel = new Dictionary<XmlElement, BoloUI.SelButton>();
			mx_treeFrame.Items.Add(mx_skinEditor);

			m_xdTest = new XmlDocument();
			// w=\"400\" h=\"300\"
			m_strTestXml = "<label dock=\"4\" baseID=\"testCtrl\" text=\"测试Test\"/>";

			m_xdTest.LoadXml(m_strTestXml);
			m_xeTest = m_xdTest.DocumentElement;

			checkAndInitToolConfig();

			// hook keyboard 可能会报毒
			// 			IntPtr hModule = GetModuleHandle(IntPtr.Zero);
			// 			hookProc = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
			// 			hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hModule, 0);
			// 			if (hHook == IntPtr.Zero)
			// 			{
			// 				MessageBox.Show("Failed to set hook, error = " + Marshal.GetLastWin32Error());
			// 			}

			return;

			TcpListener server = new TcpListener(new System.Net.IPEndPoint(IPAddress.Parse("10.0.6.10"), 10088));
			server.Start(100);
			tcpClientConnected.Reset();
			IAsyncResult result = server.BeginAcceptTcpClient(new AsyncCallback(Acceptor), server);
			tcpClientConnected.WaitOne();

		}
		private void Acceptor(IAsyncResult o)
		{
			TcpListener server = o.AsyncState as TcpListener;
			System.Diagnostics.Debug.Assert(server != null);
			TcpClient client = null;
			try
			{
				client = server.EndAcceptTcpClient(o);
				System.Threading.Interlocked.Increment(ref currentLinked);

			}
			catch
			{

			}
			IAsyncResult result = server.BeginAcceptTcpClient(new AsyncCallback(Acceptor), server);
			if (client == null)
			{
				return;
			}
			else
			{
				Thread.CurrentThread.Join();
			}
			Close(client);
		}

		private void Close(TcpClient client)
		{
			if (client.Connected)
			{
				client.Client.Shutdown(SocketShutdown.Both);
			}
			client.Client.Close();
			client.Close();

			System.Threading.Interlocked.Decrement(ref currentLinked);
		}

		private void loadToolConfigByDefault()
		{
			if (!File.Exists(conf_pathConfDefault))
			{
				string initConfXml = "<Config><runMode>release</runMode><ProjHistory>E:\\mmo2015001\\artist\\ui\\free</ProjHistory></Config>";

				m_docConf.LoadXml(initConfXml);
			}
			else
			{
				m_docConf.Load(conf_pathConfDefault);
			}
		}
		public void checkAndInitToolConfig()
		{
			m_docConf = new XmlDocument();
			m_docEventConf = new XmlDocument();

			if (File.Exists(conf_pathConfEvent))
			{
				m_docEventConf.Load(conf_pathConfEvent);
			}
			if (!File.Exists(conf_pathConf))
			{
				loadToolConfigByDefault();
			}
			else
			{
				try
				{
					m_docConf.Load(conf_pathConf);
				}
				catch
				{
					MessageBoxResult ret = MessageBox.Show("编辑器配置文件已经损坏(" + conf_pathConf + ")。是否使用默认配置文件？（可能是由于SVN更新后冲突导致，选“是”则会失去原有的编辑器配置）", "配置文件损坏", MessageBoxButton.YesNo, MessageBoxImage.Error);

					switch (ret)
					{
						case MessageBoxResult.Yes:
							{
								loadToolConfigByDefault();
							}
							break;
						case MessageBoxResult.No:
							{
								return;
							}
							break;
						default:
							{
								return;
							}
							break;
					}
				}
			}

			XmlNode xnConfig = m_docConf.SelectSingleNode("Config");

			if (xnConfig != null)
			{
				XmlNode xnRunMode = xnConfig.SelectSingleNode("runMode");

				if (xnRunMode != null)
				{
					switch (xnRunMode.InnerXml)
					{
						case "debug":
							{
								m_isDebug = true;
								m_pathGlApp = conf_pathGlApp_New;
							}
							break;
						case "release":
							{
								m_isDebug = false;
								m_pathGlApp = conf_pathGlApp_New;
							}
							break;
						case "debug_old":
							{
								m_isDebug = true;
								m_pathGlApp = conf_pathGlApp;
							}
							break;
						case "release_old":
							{
								m_isDebug = false;
								m_pathGlApp = conf_pathGlApp;
							}
							break;
						default:
							m_isDebug = false;
							m_pathGlApp = conf_pathGlApp_New;
							break;
					}
				}
				else
				{
					XmlElement xe = m_docConf.CreateElement("runMode");

					xe.InnerXml = "release";
					xnConfig.AppendChild(xe);

					m_isDebug = false;
					m_pathGlApp = conf_pathGlApp_New;
				}

				XmlNode xnResolutionSetting = xnConfig.SelectSingleNode("ResolutionSetting");

				if (xnResolutionSetting == null)
				{
					xnResolutionSetting = Project.Setting.initResolutionSetting(xnConfig);
				}
				Project.Setting.refreshResolutionBoxByConfigNode(xnResolutionSetting);

				XmlNode xnThemeSetting = xnConfig.SelectSingleNode("ThemeSetting");

				if (xnThemeSetting == null)
				{
					xnThemeSetting = Project.Setting.initSetting(xnConfig, "ThemeSetting", new string[0]);
				}
				Project.Setting.refreshSettingUIByConfigNode(xnThemeSetting, mx_cbThemeName);
			}

			try
			{
				m_docConf.Save(conf_pathConf);
			}
			catch
			{
				Public.ResultLink.createResult("\r\nconf.xml被占用，保存失败。", ResultType.RT_ERROR, null, true);
			}
		}
		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{
			return;
			HwndSource source = PresentationSource.FromVisual(this) as HwndSource;

			if (source != null)
			{
				source.AddHook(WndProc);
			}
			m_msgMng = new MsgManager(false, m_screenWidth, m_screenHeight);
			mx_GLCtrl.Child = m_msgMng.m_GLHost;
			m_msgMng.m_GLHost.MessageHook += new HwndSourceHook(ControlMsgFilter);
			if (m_isDebug)
			{
				mx_debugTools.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				mx_debugTools.Visibility = System.Windows.Visibility.Collapsed;
			}
			MainWindow.s_pW.showGLCtrl(false);

			m_textTimer = new DispatcherTimer();
			m_textTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
			m_textTimer.Tick += new EventHandler(m_textTimer_Tick);
			m_textTimer.Start();

			m_fileChangeTimer = new DispatcherTimer();
			m_fileChangeTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
			m_fileChangeTimer.Tick += new EventHandler(m_fileChangeTimer_Tick);
			m_fileChangeTimer.Start();
		}
		void mx_addCtrl_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button)
			{
				Button btnCtrl = (Button)sender;
				string ctrlName = btnCtrl.ToolTip.ToString();
				CtrlDef_T ctrlDef;

				if (ctrlName != null && ctrlName != "" && CtrlDef_T.tryGetCtrlDef(ctrlName, out ctrlDef))
				{
					XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

					if (curXmlCtrl != null && curXmlCtrl.m_curItem != null && curXmlCtrl.m_curItem is Basic)
					{
						string dstCtrlName = curXmlCtrl.m_curItem.m_xe.Name;

						if (dstCtrlName != "event")
						{
							CtrlDef_T dstCtrlDef;

							if (CtrlDef_T.tryGetCtrlDef(dstCtrlName, out dstCtrlDef) && dstCtrlDef.isNormal())
							{
								if (dstCtrlDef.isFrame())
								{
									XmlItem.addItemToCurItemByString(XmlItem.getDefaultNewXmlItem(btnCtrl.ToolTip.ToString()), curXmlCtrl.m_curItem);
								}
								else
								{
									if (curXmlCtrl.m_curItem.Parent is Basic)
									{
										XmlItem.addItemToCurItemByString(XmlItem.getDefaultNewXmlItem(btnCtrl.ToolTip.ToString()), (Basic)curXmlCtrl.m_curItem.Parent);
									}
								}
							}
						}
					}
				}
			}
		}
		private void mx_closeWin_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		public void openProjByPath(string projPath, string projName)
		{
			Project.Setting.s_projPath = projPath;
			Project.Setting.s_projName = projName;

			if (System.IO.Path.GetExtension(Project.Setting.s_projName) == ".ryrp")
			{
				Project.Setting.s_projName = System.IO.Path.ChangeExtension(Project.Setting.s_projName, ".bup");
				if (!File.Exists(Project.Setting.s_projPath + "\\" + Project.Setting.s_projName))
				{
					string initProjXml = "<BoloUIProj><template></template></BoloUIProj>";

					Project.Setting.s_docProj = new XmlDocument();
					Project.Setting.s_docProj.LoadXml(initProjXml);
				}
				else
				{
					Project.Setting.s_docProj = new XmlDocument();
					Project.Setting.s_docProj.Load(Project.Setting.s_projPath + "\\" + Project.Setting.s_projName);
				}
			}
			else if (System.IO.Path.GetExtension(Project.Setting.s_projName) == ".bup")
			{
				Project.Setting.s_docProj = new XmlDocument();
				Project.Setting.s_docProj.Load(Project.Setting.s_projPath + "\\" + Project.Setting.s_projName);
			}
			else
			{
				return;
			}

			if (Project.Setting.s_docProj.DocumentElement == null || Project.Setting.s_docProj.DocumentElement.Name != "BoloUIProj" ||
				m_docConf.DocumentElement == null || m_docConf.DocumentElement.Name != "Config")
			{
				return;
			}
			OpenedFile.closeAllFile();
			m_docConf.DocumentElement.SelectSingleNode("ProjHistory").InnerXml = Project.Setting.s_projPath;
			XmlElement xeBup = null;
			if (m_docConf.DocumentElement.SelectSingleNode("bupHistory") == null)
			{
				xeBup = m_docConf.CreateElement("bupHistory");
				m_docConf.DocumentElement.InsertAfter(
					xeBup,
					m_docConf.DocumentElement.SelectSingleNode("ProjHistory"));
			}
			else
			{
				if (m_docConf.DocumentElement.SelectSingleNode("bupHistory").NodeType == XmlNodeType.Element)
				{
					xeBup = (XmlElement)m_docConf.DocumentElement.SelectSingleNode("bupHistory");
				}
			}

			if (xeBup != null)
			{
				string newPath = Project.Setting.s_projPath + "\\" + Project.Setting.s_projName;
				int delCount = xeBup.SelectNodes("row").Count - 9;
				XmlElement xeTop = null;

				foreach (XmlNode xnRow in xeBup.SelectNodes("row"))
				{
					if (xnRow.NodeType == XmlNodeType.Element)
					{
						XmlElement xeRow = (XmlElement)xnRow;
						string rowPath = xeRow.GetAttribute("key");

						if (rowPath != "" && rowPath == newPath)
						{
							xeTop = xeRow;
						}
					}
				}
				if (xeTop == null)
				{
					XmlElement xeNewRow = m_docConf.CreateElement("row");

					for (int i = 0; i < delCount; i++)
					{
						xeBup.RemoveChild(xeBup.SelectSingleNode("row"));
					}
					xeNewRow.SetAttribute("key", newPath);
					xeBup.InsertBefore(xeNewRow, xeBup.SelectSingleNode("row"));
				}
				else
				{
					xeBup.InsertBefore(xeTop, xeBup.SelectSingleNode("row"));
				}
			}
			m_isLockMsgPath = true;
			MainWindow.s_pW.m_docConf.Save(MainWindow.conf_pathConf);
			Project.Setting.refreshAllProjectSetting();
			if (Directory.Exists(Project.Setting.s_skinPath))
			{
				if (File.Exists(Project.Setting.s_skinPath + "\\publicskin.xml"))
				{
					refreshSkin(Project.Setting.s_skinPath);
				}
			}
			refreshProjTree(Project.Setting.s_projPath, this.mx_treePro, true);
			mx_root.Title = Project.Setting.s_projPath + "\\" + Project.Setting.s_projName + " - UI编辑器";
			mx_toolNew.IsEnabled = true;

			m_isLockMsgPath = false;
			updateGL(Project.Setting.s_projPath, W2GTag.W2G_PATH);
			//<inc>暂时去掉
			//ScriptManager.copySSUEScriptToProject();
			Setting.s_mapScriptClass = ScriptManager.getSSUEScriptClassMap();
		}
		public void refreshImageTree()
		{

		}
		public void refreshSkinTree()
		{

		}
		public void openProjSelectBox(string projPath = null)
		{
			if (projPath == null)
			{
				projPath = m_docConf.SelectSingleNode("Config").SelectSingleNode("ProjHistory").InnerXml;
			}

			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Title = "选择BoloUI工程文件";
			openFileDialog.Filter = "BoloUI工程文件|*.bup|BoloUI工程文件(旧)|*.ryrp";
			openFileDialog.FileName = string.Empty;
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.DefaultExt = "bup";
			openFileDialog.InitialDirectory = projPath;
			System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				return;
			}
			openProjByPath(System.IO.Path.GetDirectoryName(openFileDialog.FileName), System.IO.Path.GetFileName(openFileDialog.FileName));
		}
		private void openProj(object sender, RoutedEventArgs e)//打开工程
		{
			openProjSelectBox();
		}
		private void refreshSkin(string path)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(path + "\\publicskin.xml");
			XmlNode xn = xmlDoc.SelectSingleNode("BoloUI");

			if (xn != null)
			{
				string buffer = xmlDoc.InnerXml;
				updateGL("publicskin.xml", W2GTag.W2G_NORMAL_NAME);
				updateGL(buffer, W2GTag.W2G_NORMAL_DATA);
			}
		}
		public void refreshProjTree(string path, TreeViewItem rootItem, bool rootNode)
		{
			rootItem.Items.Clear();

			int i = 0;
			int j = 0;

			DirectoryInfo di = new DirectoryInfo(path);
			foreach (var dri in di.GetDirectories())
			{
				TreeViewItem treeUIChild = new IncludeFile(path + "\\" + dri.Name);

				i++;
				rootItem.Items.Add(treeUIChild);

				refreshProjTree(path + "\\" + dri.Name, treeUIChild, false);
			}
			foreach (var dri in di.GetFiles("*"))
			{
				TreeViewItem treeUIChild = new IncludeFile(path + "\\" + dri.Name);

				j++;
				rootItem.Items.Add(treeUIChild);
			}
			if (rootNode == true)
			{
				ToolTip rootTip = new ToolTip();
				rootTip.Content = path;
				rootItem.ToolTip = rootTip;
				rootItem.IsExpanded = true;
				rootItem.Header = "UI工程目录(" + i + "个目录和" + j + "个项目)";
				Project.Setting.refreshSkinIndex();
			}
		}

		public void showGLCtrl(bool isShow = true, bool isShowText = false)
		{
			if (isShow)
			{
				mx_selModeFrame.Visibility = System.Windows.Visibility.Visible;
				mx_scrollFrame.Visibility = System.Windows.Visibility.Visible;
				mx_GLCtrl.Visibility = System.Windows.Visibility.Visible;
				if (mx_showTextTab.IsChecked == true)
				{
					mx_textFrame.Visibility = System.Windows.Visibility.Visible;
					mx_drawFrame.Visibility = System.Windows.Visibility.Collapsed;
				}
				else
				{
					mx_drawFrame.Visibility = System.Windows.Visibility.Visible;
					mx_textFrame.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
			else
			{
				mx_drawFrame.Visibility = System.Windows.Visibility.Collapsed;
				mx_scrollFrame.Visibility = System.Windows.Visibility.Collapsed;
				mx_GLCtrl.Visibility = System.Windows.Visibility.Collapsed;

				if (OpenedFile.getCurFileDef() != null && OpenedFile.getCurFileDef().m_fileType == "xml")
				{
					if (mx_showTextTab.IsChecked == true && (m_mapOpenedFiles.Count > 0 || isShowText == true))
					{
						mx_textFrame.Visibility = System.Windows.Visibility.Visible;
					}
					else
					{
						mx_textFrame.Visibility = System.Windows.Visibility.Collapsed;
					}
					mx_selModeFrame.Visibility = System.Windows.Visibility.Visible;
				}
				else
				{
					mx_selModeFrame.Visibility = System.Windows.Visibility.Collapsed;
					mx_textFrame.Visibility = System.Windows.Visibility.Collapsed;
				}
				foreach (object attrList in mx_toolArea.Items)
				{
					if (attrList is AttrList)
					{
						((AttrList)attrList).Visibility = System.Windows.Visibility.Collapsed;
					}
				}
			}
		}
		public void openFileByDef(OpenedFile fileDef)
		{
			OpenedFile openedFile;

			if (fileDef != null)
			{
				if (m_mapOpenedFiles.TryGetValue(fileDef.m_path, out openedFile))
				{
					mx_workTabs.SelectedItem = fileDef.m_tab;
				}
				else
				{
					m_mapOpenedFiles.Add(fileDef.m_path, fileDef);
					mx_workTabs.Items.Add(fileDef.m_tab);
					mx_workTabs.SelectedItem = fileDef.m_tab;
					m_curFile = fileDef.m_path;
					if (fileDef.m_frame != null && fileDef.m_frame is XmlControl)
					{
						XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;

						if (xmlCtrl.m_treeUI != null)
						{
							if (xmlCtrl.m_treeUI.Parent != null && xmlCtrl.m_treeUI.Parent is ItemsControl)
							{
								((ItemsControl)xmlCtrl.m_treeUI.Parent).Items.Remove(xmlCtrl.m_treeUI);
							}
							MainWindow.s_pW.mx_treeCtrlFrame.Items.Add(xmlCtrl.m_treeUI);
						}
						if (xmlCtrl.m_treeSkin != null)
						{
							if (xmlCtrl.m_treeSkin.Parent != null && xmlCtrl.m_treeSkin.Parent is ItemsControl)
							{
								((ItemsControl)xmlCtrl.m_treeSkin.Parent).Items.Remove(xmlCtrl.m_treeSkin);
							}
							MainWindow.s_pW.mx_treeSkinFrame.Items.Add(xmlCtrl.m_treeSkin);
						}
					}
				}
			}
		}
		public void openFileByPath(string path, string skinName = "")
		{
			OpenedFile openedFile;
			string fileType = StringDic.getFileType(path);

			if (m_mapOpenedFiles.TryGetValue(path, out openedFile))
			{
				mx_workTabs.SelectedItem = openedFile.m_tab;
				Public.ResultLink.s_curResultFrame = openedFile.m_paraResult;
			}
			else
			{
				if (File.Exists(path))
				{
					m_curFile = path;
					new OpenedFile(path, skinName);
				}
				else
				{
					Public.ResultLink.createResult("\r\n文件：\"" + path + "\"不存在，请检查路径。", Public.ResultType.RT_ERROR);
				}
			}
		}
		public void eventCloseFile(object sender, RoutedEventArgs e)
		{
			if (m_mapOpenedFiles.Count == 0)
			{
				showGLCtrl(false);
			}
		}
		private void mx_workTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			XmlControl.clearNodeTreeFrame();

			AttrList.hiddenAllAttr();
			if (((TabItem)mx_workTabs.SelectedItem) != null)
			{
				if (((ToolTip)((TabItem)mx_workTabs.SelectedItem).ToolTip) != null)
				{
					string tabPath = ((ToolTip)((TabItem)mx_workTabs.SelectedItem).ToolTip).Content.ToString();
					string fileType = StringDic.getFileType(tabPath);

					m_curFile = tabPath;
					if (fileType == "xml")
					{
						string fileName = StringDic.getFileNameWithoutPath(tabPath);

						OpenedFile openFile;
						if (m_mapOpenedFiles.TryGetValue(tabPath, out openFile))
						{
							Public.ResultLink.s_curResultFrame = openFile.m_paraResult;
							if (openFile.m_frame is XmlControl)
							{
								//updateGL(fileName, W2GTag.W2G_NORMAL_TURN);
								updateGL("<null>:-1", W2GTag.W2G_CURSTATE);
								refreshCurFile();
								XmlControl xmlCtrl = (XmlControl)openFile.m_frame;

								if (xmlCtrl.m_showGL)
								{
									xmlCtrl.m_treeUI.Visibility = System.Windows.Visibility.Visible;
									xmlCtrl.m_treeSkin.Visibility = System.Windows.Visibility.Visible;
									MainWindow.s_pW.showGLCtrl(true);
								}
								else
								{
									MainWindow.s_pW.showGLCtrl(false, true);
								}
								xmlCtrl.refreshXmlText();
								xmlCtrl.refreshSkinDicForAll();
								if (xmlCtrl.m_curItem != null)
								{
									xmlCtrl.m_curItem.changeSelectItem();
								}
								return;
							}
						}
					}
				}
			}
			MainWindow.s_pW.showGLCtrl(false);
		}
		private void mx_root_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			List<OpenedFile> lstChangedFiles = new List<OpenedFile>();
			string strMsg = "是否将更改保存到：\r\n";

			foreach (KeyValuePair<string, OpenedFile> pairFile in m_mapOpenedFiles.ToList())
			{
				if (pairFile.Value != null)
				{
					if (pairFile.Value.haveDiffToFile())
					{
						lstChangedFiles.Add(pairFile.Value);
						strMsg += pairFile.Value.m_path + "\r\n";
					}
				}
			}
			if (lstChangedFiles != null && lstChangedFiles.Count > 0)
			{

				MessageBoxResult ret = MessageBox.Show(strMsg, "保存确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
				switch (ret)
				{
					case MessageBoxResult.Yes:
						{
							foreach (OpenedFile fileDef in lstChangedFiles)
							{
								((XmlControl)fileDef.m_frame).saveCurStatus();
							}
						}
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
					default:
						e.Cancel = true;
						return;
				}
			}

			UnhookWindowsHookEx(hHook); // release keyboard hook
		}
		private void mx_root_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch(e.KeyboardDevice.Modifiers)
			{
				case ModifierKeys.None:
					{
						#region 无控制键
						switch (e.Key)
						{
							case Key.F5:
								{
									Setting.refreshSkinIndex();
									refreshCurFile();
									e.Handled = true;
								}
								break;
							case Key.Delete:
								{
									if ((!(Keyboard.FocusedElement is TextBox && ((TextBox)Keyboard.FocusedElement).IsReadOnly == false) &&
										!(Keyboard.FocusedElement is RichTextBox && ((RichTextBox)Keyboard.FocusedElement).IsReadOnly == false)) &&
										XmlItem.s_menu != null && XmlItem.s_menu.canDelete())
									{
										XmlItem.getCurItem().deleteItem();
									}
								}
								break;
							default:
								return;
						}
						#endregion
					}
					break;
				case ModifierKeys.Control:
					{
						#region Ctrl
						switch (e.Key)
						{
							case Key.W:
								{
									OpenedFile curFileDef = OpenedFile.getCurFileDef();

									if (curFileDef != null)
									{
										curFileDef.m_tabItem.closeFile();
									}
								}
								break;
							case Key.O:
								{
									openProjSelectBox();
								}
								break;
							case Key.D1:
								{
									if (mx_treeFrameProj != null && mx_treeFrameProj.Visibility == System.Windows.Visibility.Visible)
									{
										mx_treeFrameProj.IsSelected = true;
									}
								}
								break;
							case Key.D2:
								{
									if (mx_treeFrameUI != null && mx_treeFrameUI.Visibility == System.Windows.Visibility.Visible)
									{
										mx_treeFrameUI.IsSelected = true;
									}
								}
								break;
							case Key.D3:
								{
									if (mx_treeFrameSkin != null && mx_treeFrameSkin.Visibility == System.Windows.Visibility.Visible)
									{
										mx_treeFrameSkin.IsSelected = true;
									}
								}
								break;
							case Key.D4:
								{
									if (mx_skinEditor != null && mx_skinEditor.Visibility == System.Windows.Visibility.Visible)
									{
										mx_skinEditor.IsSelected = true;
									}
								}
								break;
							case Key.PageUp:
								{
									viewPrevFile(s_pW);
								}
								break;
							case Key.PageDown:
								{
									viewNextFile(s_pW);
								}
								break;
							case Key.Up:
								{
									if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveUp())
									{
										XmlItem.getCurItem().moveUpItem();
									}
								}
								break;
							case Key.Down:
								{
									if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveDown())
									{
										XmlItem.getCurItem().moveDownItem();
									}
								}
								break;
							case Key.Left:
								{
									if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveToParent())
									{
										XmlItem.getCurItem().moveToParent();
									}
								}
								break;
							case Key.Right:
								{
									if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveToChild())
									{
										XmlItem.getCurItem().moveToChild();
									}
								}
								break;
							case Key.Y:
								{
									s_pW.curFileRedo();
									if (Keyboard.FocusedElement is TextBox)
									{
										TextBox tb = (TextBox)Keyboard.FocusedElement;

										tb.SelectionStart = tb.Text.Length;
									}
								}
								break;
							case Key.Z:
								{
									s_pW.curFileUndo();
									if (Keyboard.FocusedElement is TextBox)
									{
										TextBox tb = (TextBox)Keyboard.FocusedElement;

										tb.SelectionStart = tb.Text.Length;
									}
								}
								break;
							case Key.X:
								{
									if ((!(Keyboard.FocusedElement is TextBox && ((TextBox)Keyboard.FocusedElement).IsReadOnly == false) &&
										!(Keyboard.FocusedElement is RichTextBox && ((RichTextBox)Keyboard.FocusedElement).IsReadOnly == false)) &&
										XmlItem.s_menu != null && XmlItem.s_menu.canCut())
									{
										XmlItem.getCurItem().cutItem();
									}
									else
									{
										return;
									}
								}
								break;
							case Key.C:
								{
									if ((!(Keyboard.FocusedElement is TextBox && ((TextBox)Keyboard.FocusedElement).IsReadOnly == false) &&
										!(Keyboard.FocusedElement is RichTextBox && ((RichTextBox)Keyboard.FocusedElement).IsReadOnly == false)) &&
										XmlItem.s_menu != null && XmlItem.s_menu.canCopy())
									{
										XmlItem.getCurItem().copyItem();
									}
									else
									{
										return;
									}
								}
								break;
							case Key.V:
								{
									if ((!(Keyboard.FocusedElement is TextBox && ((TextBox)Keyboard.FocusedElement).IsReadOnly == false) &&
										!(Keyboard.FocusedElement is RichTextBox && ((RichTextBox)Keyboard.FocusedElement).IsReadOnly == false)) &&
										XmlItem.s_menu != null && XmlItem.s_menu.canPaste())
									{
										XmlItem.getCurItem().pasteItem();
									}
									else
									{
										return;
									}
								}
								break;
							default:
								return;
						}
						#endregion
					}
					break;
				case ModifierKeys.Control | ModifierKeys.Shift:
					{
						#region Ctrl+Shift
						switch (e.Key)
						{
							case Key.W:
								{
									OpenedFile.closeAllFile();
								}
								break;
							default:
								return;
						}
						#endregion
					}
					break;
				case ModifierKeys.Shift:
					{
						#region Alt
						switch (e.Key)
						{
							case Key.W:
								{
									OpenedFile.closeAllFile(OpenedFile.getCurFileDef());
								}
								break;
							default:
								return;
						}
						#endregion
					}
					break;
				default:
					return;
			}
			e.Handled = true;
		}

		//============================================================

		#region WIN32消息相关

		#region WIN32预定义
		internal const int
			WM_DESTROY = 0x0002,
			WM_CLOSE = 0x0010,
			WM_QUIT = 0x0012,
			WM_COPYDATA = 0x004A,
			WM_COMMAND = 0x0111,

			WM_KEYDOWN = 0x0100,

			WM_MOUSEFIRST = 0x0200,
			WM_MOUSEMOVE = 0x0200,
			WM_LBUTTONDOWN = 0x0201,
			WM_LBUTTONUP = 0x0202,
			WM_LBUTTONDBLCLK = 0x0203,
			WM_RBUTTONDOWN = 0x0204,
			WM_RBUTTONUP = 0x0205,
			WM_RBUTTONDBLCLK = 0x0206,
			WM_MBUTTONDOWN = 0x0207,
			WM_MBUTTONUP = 0x0208,
			WM_MBUTTONDBLCLK = 0x0209,

			LBN_SELCHANGE = 0x0001,
			LB_GETCURSEL = 0x0188,
			LB_GETTEXTLEN = 0x018A,
			LB_ADDSTRING = 0x0180,
			LB_GETTEXT = 0x0189,
			LB_DELETESTRING = 0x0182,
			LB_GETCOUNT = 0x018B,

			VK_PRIOR = 0x21,
			VK_NEXT = 0x22,
			VK_LEFT = 0x25,
			VK_UP = 0x26,
			VK_RIGHT = 0x27,
			VK_DOWN = 0x28,
			VK_DELETE = 0x2E,

			VK_OEM_PLUS = 0xBB,
			VK_OEM_MINUS = 0xBD,

			VK_0 = 0x30,
			VK_1 = 0x31,
			VK_2 = 0x32,
			VK_3 = 0x33,
			VK_4 = 0x34,
			VK_5 = 0x35,
			VK_6 = 0x36,
			VK_7 = 0x37,
			VK_8 = 0x38,
			VK_9 = 0x39,

			VK_A = 0x41,
			VK_B = 0x42,
			VK_C = 0x43,
			VK_D = 0x44,
			VK_E = 0x45,
			VK_F = 0x46,
			VK_G = 0x47,
			VK_H = 0x48,
			VK_I = 0x49,
			VK_J = 0x4A,
			VK_K = 0x4B,
			VK_L = 0x4C,
			VK_M = 0x4D,
			VK_N = 0x4E,
			VK_O = 0x4F,
			VK_P = 0x50,
			VK_Q = 0x51,
			VK_R = 0x52,
			VK_S = 0x53,
			VK_T = 0x54,
			VK_U = 0x55,
			VK_V = 0x56,
			VK_W = 0x57,
			VK_X = 0x58,
			VK_Y = 0x59,
			VK_Z = 0x5A,

			//WM_NCHITTEST消息处理返回值
			HTERROR = -2,
			HTTRANSPARENT = -1,
			HTNOWHERE = 0,
			HTCLIENT = 1,
			HTCAPTION = 2,
			HTSYSMENU = 3,
			HTGROWBOX = 4,
			HTSIZE = HTGROWBOX,
			HTMENU = 5,
			HTHSCROLL = 6,
			HTVSCROLL = 7,
			HTMINBUTTON = 8,
			HTMAXBUTTON = 9,
			HTLEFT = 10,
			HTRIGHT = 11,
			HTTOP = 12,
			HTTOPLEFT = 13,
			HTTOPRIGHT = 14,
			HTBOTTOM = 15,
			HTBOTTOMLEFT = 16,
			HTBOTTOMRIGHT = 17,
			HTBORDER = 18,
			HTREDUCE = HTMINBUTTON,
			HTZOOM = HTMAXBUTTON,
			HTSIZEFIRST = HTLEFT,
			HTSIZELAST = HTBOTTOMRIGHT,
			HTOBJECT = 19,
			HTCLOSE = 20,
			HTHELP = 21;
		#endregion

		#region SendMessage函数接口
		[DllImport("gdi32")]
		public static extern int DeleteObject(IntPtr o);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
		internal static extern int SendMessage(
			IntPtr hwnd,
			int msg,
			IntPtr wParam,
			IntPtr lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
		internal static extern int SendMessage(
			IntPtr hwnd,
			int msg,
			int wParam,
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
		internal static extern IntPtr SendMessage(
			IntPtr hwnd,
			int msg,
			IntPtr wParam,
			String lParam);

		[DllImport("User32.dll")]
		public static extern int SendMessage(
			IntPtr hwnd,
			int msg,
			int wParam,
			ref COPYDATASTRUCT IParam);

		[DllImport("User32.dll")]
		public static extern int SendMessage(
			IntPtr hwnd,
			int msg,
			int wParam,
			ref COPYDATASTRUCT_SEND IParam);

		[DllImport("User32.dll")]
		public static extern int SendMessage(
			IntPtr hwnd,
			int msg,
			int wParam,
			ref COPYDATASTRUCT_SENDEX IParam);
		#endregion

		#endregion

		private struct KBDLLHOOKSTRUCT
		{
			public int vkCode;
			int scanCode;
			public int flags;
			int time;
			int dwExtraInfo;
		}

		private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);
		[DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(IntPtr hHook);
		[DllImport("user32.dll")]
		private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetModuleHandle(IntPtr path);
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();

		private IntPtr hHook;
		LowLevelKeyboardProcDelegate hookProc; // prevent gc
		const int WH_KEYBOARD_LL = 13;

		static public string getArtistPath(string freePath)
		{
			string resPath = "";

			if (freePath != null && freePath != "" && Directory.Exists(freePath))
			{
				DirectoryInfo dri = new DirectoryInfo(freePath);

				if (dri != null && dri.Parent != null && dri.Parent.Parent != null)
				{
					DirectoryInfo driRes = dri.Parent.Parent;

					resPath = driRes.FullName;
				}
			}

			return resPath;
		}
		public void updateGL(string buffer, W2GTag msgTag = W2GTag.W2G_NORMAL_DATA)
		{
			if (m_isLockMsgPath == true && msgTag == W2GTag.W2G_PATH)
			{
				return;
			}
			if (mx_hwndDebug.Text != "")
			{
				m_msgMng.m_hwndGL = (IntPtr)long.Parse(mx_hwndDebug.Text);
			}
			
			m_msgMng.updateGL(buffer, msgTag);
			if (msgTag != W2GTag.W2G_SELECT_UI && msgTag != W2GTag.W2G_VIEWMODE && msgTag != W2GTag.W2G_DRAWRECT
				&& msgTag != W2GTag.W2G_NORMAL_UPDATE && msgTag != W2GTag.W2G_RENDERCACHE_IMAGECOLOR)
			{
				XmlControl curXmlCtrl = XmlControl.getCurXmlControl();
				if (curXmlCtrl != null && curXmlCtrl.m_curItem != null && curXmlCtrl.m_curItem is BoloUI.Basic)
				{
					BoloUI.Basic curUICtrl = (BoloUI.Basic)curXmlCtrl.m_curItem;

					curUICtrl.showBlueRect();
				}
			}
			if (msgTag == W2GTag.W2G_PATH)
			{
				MoveWindow(m_msgMng.m_hwndGL, 0, 0, m_screenWidth, m_screenHeight, true);
			}
		}
		//响应主逻辑
		private IntPtr ControlMsgFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			handled = false;
			switch (msg)
			{
				case WM_MOUSEMOVE:
					#region WM_MOUSEMOVE
					{
						int pX = (int)lParam & 0xFFFF;
						int pY = ((int)lParam >> 16) & 0xFFFF;

						mb_status0 = "( " + pX + " , " + pY + " )";
						if (mx_isViewMode.IsChecked == true || ResBasic.isEnableSkinEditor())
						{
							break;
						}
						if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control)
						{
							if (m_isMouseDown)
							{
								if (m_isCtrlMoved == false)
								{
									if (System.Math.Abs(pX - m_downX) > 10 || System.Math.Abs(pY - m_downY) > 10)
									{
										m_isCtrlMoved = true;
									}
								}
								else
								{
									XmlItem curItem = XmlItem.getCurItem();

									if (curItem != null && curItem is BoloUI.Basic)
									{
										BoloUI.Basic selItem = (BoloUI.Basic)curItem;
										string msgData;

										msgData = (selItem.m_selScreenX + (pX - m_downX)).ToString() + ":" + (selItem.m_selScreenY + (pY - m_downY)).ToString() + ":" +
											selItem.m_selW.ToString() + ":" + selItem.m_selH.ToString() + ":";
										updateGL(msgData, W2GTag.W2G_DRAWRECT);
									}
								}
							}
						}
					}
					#endregion
					break;
				case WM_LBUTTONDOWN:
					#region WM_LBUTTONDOWN
					{
						if (mx_isViewMode.IsChecked == true || ResBasic.isEnableSkinEditor())
						{
							break;
						}
						XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

						if (curXmlCtrl != null)
						{
							curXmlCtrl.refreshVRect();
						}
						int pX = (int)lParam & 0xFFFF;
						int pY = ((int)lParam >> 16) & 0xFFFF;

						m_isMouseDown = true;
						m_isCtrlMoved = false;
						m_downX = pX;
						m_downY = pY;
					}
					#endregion
					break;
				case WM_LBUTTONUP:
					#region WM_LBUTTONUP
					{
						if (mx_isViewMode.IsChecked == true || ResBasic.isEnableSkinEditor())
						{
							break;
						}
						if (m_isMouseDown == true)
						{
							int pX = (int)lParam & 0xFFFF;
							int pY = ((int)lParam >> 16) & 0xFFFF;
							if (m_isCtrlMoved == false)
							{
								List<BoloUI.Basic> lstSelCtrl = new List<BoloUI.Basic>();
								BoloUI.Basic selCtrl = null;
								BoloUI.Basic lastCtrl = null;

								mx_selCtrlLstFrame.Children.Clear();
								m_mapXeSel.Clear();

								XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

								if (curXmlCtrl != null)
								{
									foreach (KeyValuePair<string, BoloUI.Basic> pairCtrlDef in
										curXmlCtrl.m_mapCtrlUI.ToList())
									{
										if (pairCtrlDef.Value.checkPointInFence(pX, pY))
										{
											BoloUI.SelButton selCtrlButton;
											if (!m_mapXeSel.TryGetValue(pairCtrlDef.Value.m_xe, out selCtrlButton))
											{
												lstSelCtrl.Add(pairCtrlDef.Value);
												if (XmlItem.getCurItem() == pairCtrlDef.Value)
												{
													selCtrl = lastCtrl;
												}
												lastCtrl = pairCtrlDef.Value;

												selCtrlButton = new BoloUI.SelButton(this, pairCtrlDef.Value);
												selCtrlButton.mx_radio.Content = pairCtrlDef.Value.mx_text.Text;
												mx_selCtrlLstFrame.Children.Add(selCtrlButton);
												m_mapXeSel.Add(pairCtrlDef.Value.m_xe, selCtrlButton);
											}
										}
									}
									if (lstSelCtrl.Count > 0)
									{
										if (selCtrl == null)
										{
											selCtrl = lstSelCtrl.Last();
										}
										selCtrl.changeSelectItem();
									}
								}
								mx_bFrameCtrl.IsSelected = true;
							}
							else
							{
								if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) ==
									System.Windows.Forms.Keys.Control)
								{
									if (XmlItem.getCurItem() != null && XmlItem.getCurItem().m_type == "CtrlUI")
									{
										BoloUI.Basic selItem = (BoloUI.Basic)XmlItem.getCurItem();
										int x, y;

										if (selItem.m_xe.GetAttribute("x") == "")
										{
											x = 0;
										}
										else
										{
											x = int.Parse(selItem.m_xe.GetAttribute("x"));
										}
										x += pX - m_downX;
										if (selItem.m_xe.GetAttribute("y") == "")
										{
											y = 0;
										}
										else
										{
											y = int.Parse(selItem.m_xe.GetAttribute("y"));
										}
										y += pY - m_downY;

										List<XmlOperation.HistoryNode> lstOptNode = new List<XmlOperation.HistoryNode>();

										lstOptNode.Add(new XmlOperation.HistoryNode(selItem.m_xe, "x", selItem.m_xe.GetAttribute("x"), x.ToString()));
										lstOptNode.Add(new XmlOperation.HistoryNode(selItem.m_xe, "y", selItem.m_xe.GetAttribute("y"), y.ToString()));
										selItem.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(lstOptNode);
										selItem.changeSelectItem();
									}
								}
							}
						}
						m_isMouseDown = false;
						m_isCtrlMoved = false;
					}
					#endregion
					break;
				case WM_LBUTTONDBLCLK:
					break;
				case WM_RBUTTONDOWN:
					break;
				case WM_RBUTTONUP:
					{
						if (XmlItem.s_menu != null)
						{
							XmlItem.s_menu.mx_menu.PlacementTarget = mx_root;
							XmlItem.s_menu.mx_menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
							XmlItem.s_menu.mx_menu.IsOpen = true;
						}
					}
					break;
				case WM_RBUTTONDBLCLK:
					break;
				case WM_MBUTTONDOWN:
					break;
				case WM_MBUTTONUP:
					break;
				case WM_MBUTTONDBLCLK:
					break;
				case WM_COPYDATA:
					#region WM_COPYDATA
					G2WTag tag;
					string strData;
					unsafe
					{
						COPYDATASTRUCT msgData = *(COPYDATASTRUCT*)lParam;
						strData = Marshal.PtrToStringAnsi(msgData.lpData, msgData.cbData);
						tag = (G2WTag)((COPYDATASTRUCT*)lParam)->dwData;
					}
					switch (tag)
					{
						case G2WTag.G2W_HWND:
							{
								m_msgMng.m_hwndGL = wParam;
							}
							break;
						case G2WTag.G2W_DRAW_COUNT:
							{
								#region 绘制计数
								string[] sArray = Regex.Split(strData, ":", RegexOptions.IgnoreCase);

								if (sArray.Length >= 4)
								{
									int imageCount = 0;
									int textCount = 0;
									int particleCount = 0;
									int particleSize = 0;
									int sumCount = 0;


									if (int.TryParse(sArray[0], out imageCount))
									{
										sumCount += imageCount;
									}
									if (int.TryParse(sArray[1], out textCount))
									{
										sumCount += textCount;
									}
									if (int.TryParse(sArray[2], out particleCount))
									{
										sumCount += particleCount;
									}
									if (int.TryParse(sArray[3], out particleSize))
									{
									}

									double szMb = (double)particleSize / 1024 / 1024;
									mb_status3 = "特效：" + Math.Round(szMb, 2).ToString() + " MB" + "\t\t绘制次数：  图片：" + imageCount.ToString() +
										"  文字：" + textCount.ToString() + "  特效：" + particleCount.ToString() +
										"  总计：" + sumCount.ToString();
								}
								#endregion
							}
							break;
						case G2WTag.G2W_EVENT:
							{
								#region 事件回传
								string[] sArray = Regex.Split(strData, ":", RegexOptions.IgnoreCase);

								if (sArray.Length >= 2)
								{
									string id = sArray[0];
									string ent = sArray[1];

									switch (ent)
									{
										case "click":
											{
												BoloUI.Basic tmpCtrl;
												XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

												if (curXmlCtrl != null && curXmlCtrl.m_mapCtrlUI != null)
												{
													if (curXmlCtrl.m_mapCtrlUI.TryGetValue(id, out tmpCtrl))
													{
														tmpCtrl.changeSelectItem();
														tmpCtrl.IsSelected = true;
													}
												}
											}
											break;
										default:
											break;
									}
								}
								#endregion
							}
							break;
						case G2WTag.G2W_UI_VRECT:
							{
								#region UI显示范围
								string[] sArray = Regex.Split(strData, ":", RegexOptions.IgnoreCase);
								const int iVRectMaxNum = 7;

								for (int i = iVRectMaxNum; i < sArray.Length; i += iVRectMaxNum)
								{
									string baseId = sArray[i - iVRectMaxNum];
									XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

									if (curXmlCtrl != null)
									{
										BoloUI.Basic curCtrl;

										if (curXmlCtrl.m_mapCtrlUI.TryGetValue(baseId, out curCtrl))
										{
											curCtrl.m_selRelativeX = int.Parse(sArray[i - iVRectMaxNum + 1]);
											curCtrl.m_selRelativeY = int.Parse(sArray[i - iVRectMaxNum + 2]);
											curCtrl.m_selScreenX = int.Parse(sArray[i - iVRectMaxNum + 3]);
											curCtrl.m_selScreenY = int.Parse(sArray[i - iVRectMaxNum + 4]);
											curCtrl.m_selW = int.Parse(sArray[i - iVRectMaxNum + 5]);
											curCtrl.m_selH = int.Parse(sArray[i - iVRectMaxNum + 6]);
										}
										else
										{
											Public.ResultLink.createResult("\r\n<G2W_UI_VRECT>没有找到控件，vId:" + baseId, Public.ResultType.RT_ERROR);
										}
									}
									else
									{

									}
								}
								#endregion
							}
							break;
						case G2WTag.G2W_PARTICLE_SELECT_KEYFRAME:
							{
								#region 选中某一特效关键帧
								int index = 0;
								XmlItem curAniFrame = null;
								string[] sArray = Regex.Split(strData, ":", RegexOptions.IgnoreCase);

								if (sArray.Count() > 0 && int.TryParse(sArray[0], out index))
								{
									curAniFrame = XmlItem.getAniFrameItem(index);
									if (curAniFrame != null)
									{
										curAniFrame.changeSelectItem();
									}
								}
								#endregion
							}
							break;
						case G2WTag.G2W_PARTICLE_CHANGE_KEYFRAME:
							{
								#region 修改某一特效关键帧
								XmlItem.updateParticleKeyFrameFromG2WData(strData);
								#endregion
							}
							break;
						case G2WTag.G2W_RENDERCACHE_DATA:
							{
								#region RenderCache中的渲染批次结构数据
								const int cSendDataSize = 3;
								string[] sArray = Regex.Split(strData, ":", RegexOptions.IgnoreCase);
								string[] strCache = {"", "", ""};
								int index = 0;
								List<RenderCacheViewData> lstBinding = new List<RenderCacheViewData>();

								foreach(string curStr in sArray)
								{
									if (index % cSendDataSize == 0 && index != 0)
									{
										string strType;

										switch (strCache[0])
										{
											case "0":
												{
													strType = "图片";
												}
												break;
											case "1":
												{
													strType = "文字";
												}
												break;
											case "2":
												{
													strType = "扇图";
												}
												break;
											case "3":
												{
													strType = "特效或3DModel";
												}
												break;
											case "4":
												{
													strType = "矩形";
												}
												break;
											case "5":
												{
													strType = "缩放";
												}
												break;
											case "6":
												{
													strType = "旋转";
												}
												break;
											case "7":
												{
													strType = "灰度";
												}
												break;
											case "8":
												{
													strType = "战魂特效";
												}
												break;
											default:
												{
													strType = "未知";
												}
												break;
										}

										lstBinding.Add(new RenderCacheViewData(index / cSendDataSize, strType, strCache[1], strCache[2], true));
									}
									strCache[index % cSendDataSize] = curStr;
									index++;
								}
								mx_lvRenderCache.ItemsSource = lstBinding;
								#endregion
							}
							break;
						default:
							break;
					}
					#endregion
					break;
				default:
					break;
			}

			return IntPtr.Zero;
		}
		private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)//底层相应，用于屏蔽系统和WPF控件默认事件
		{
			if (nCode >= 0 && MainWindow.s_pW != null)
			{
				IntPtr fdHwnd = GetForegroundWindow();
				IntPtr curHwnd = new WindowInteropHelper(MainWindow.s_pW).Handle;

				if (curHwnd == fdHwnd)
				{
					switch (wParam)
					{
						default:
							break;
					}
				}
			}

			return CallNextHookEx(0, nCode, wParam, ref lParam);
		}
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)//根窗体消息响应
		{
			switch (msg)
			{
				case WM_COPYDATA:
					break;
				case WM_CLOSE:
					//因为有保存确认，这里不向GL端发送关闭消息。
					//SendMessage(m_hwndGL, WM_QUIT, m_hwndGLParent, IntPtr.Zero);
					break;
				case WM_QUIT:
					//规避GL端报错窗口，因为也没有什么要保存的。
					if (!m_msgMng.m_GLHost.m_process.HasExited)
					{
						m_msgMng.m_GLHost.m_process.Kill();
					}
					break;
				case WM_DESTROY:
					if (!m_msgMng.m_GLHost.m_process.HasExited)
					{
						m_msgMng.m_GLHost.m_process.Kill();
					}
					break;
				case WM_KEYDOWN:
					{
						//移动到MainWindow的根节点处理
					}
					break;
				default:
					break;
			}

			return hwnd;
		}
		public void addVidToMsgXml(XmlElement srcXe, XmlElement dstXe, XmlDocument dstDoc, XmlControl xmlCtrl)
		{
			foreach (XmlAttribute attr in srcXe.Attributes)
			{
				dstXe.SetAttribute(attr.Name, attr.Value);
			}
			foreach (XmlNode xn in srcXe.ChildNodes)
			{
				if (xn.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xn;
					XmlItem item;
					XmlElement newXe = dstDoc.CreateElement(xe.Name);

					addVidToMsgXml(xe, newXe, dstDoc, xmlCtrl);

					if (xmlCtrl.m_mapXeItem.TryGetValue(xe, out item))
					{
						if (item is Basic)
						{
							BoloUI.Basic uiCtrl = (BoloUI.Basic)item;

							if (uiCtrl.m_vId != null && uiCtrl.m_vId != "")
							{
								newXe.SetAttribute("baseID", uiCtrl.m_vId);
							}
							if (mx_isShowAll.IsChecked == true)
							{
								newXe.SetAttribute("visible", "true");
							}
						}
					}
					dstXe.AppendChild(newXe);
				}
			}
		}
		public static void DoEvents()//WPF强制渲染
		{
			DispatcherFrame frame = new DispatcherFrame();

			Dispatcher.CurrentDispatcher.BeginInvoke(
				DispatcherPriority.Background,
				new DispatcherOperationCallback(
					delegate(object f)
					{
						((DispatcherFrame)f).Continue = false;

						return null;
					}
				),
				frame);
			Dispatcher.PushFrame(frame);
		}

		//立即更新GL端
		public void updateXmlToGL(XmlControl xmlCtrl, XmlElement xePlus = null, bool isCtrlUI = false)
		{
			string path = xmlCtrl.m_openedFile.m_path;
			XmlDocument doc = xmlCtrl.m_xmlDoc;
			XmlDocument newDoc = new XmlDocument();
			string fileName = StringDic.getFileNameWithoutPath(path);

			XmlElement newRootXe = newDoc.CreateElement(doc.DocumentElement.Name);
			addVidToMsgXml(doc.DocumentElement, newRootXe, newDoc, xmlCtrl);
			newDoc.AppendChild(newRootXe);

			XmlNodeList nodeList;

			if (xePlus != null)
			{
				if (isCtrlUI == false)
				{
					string strTmp = "<panel dock=\"4\" name=\"background\" skin=\"BackPure\"></panel>";

					XmlElement xeTmp = newDoc.CreateElement("tmp");
					xeTmp.InnerXml = strTmp;
					//xePlus.OuterXml
					xeTmp.FirstChild.InnerXml = xePlus.OuterXml;
					newRootXe.AppendChild(xeTmp.FirstChild);
				}
				else
				{
					XmlNode xn;
					XmlElement xe;

					for (xn = xePlus; xn.ParentNode != null && xn.ParentNode.NodeType == XmlNodeType.Element && xn.ParentNode.Name != "BoloUI"; xn = xn.ParentNode)
					{

					}
					if (xn.ParentNode != null && xn.ParentNode.NodeType == XmlNodeType.Element && xn.ParentNode.Name == "BoloUI")
					{
						xe = (XmlElement)xn;

						string strTmp = xe.OuterXml;

						XmlElement xeTmp = newDoc.CreateElement("tmp");
						xeTmp.InnerXml = strTmp;
						newRootXe.AppendChild(xeTmp.FirstChild);
					}
				}
			}
			//去掉所有事件(<event>)
			//<inc>因为要测试新需求，所以这里不屏蔽。
// 			nodeList = newRootXe.SelectNodes("descendant::event");
// 
// 			foreach (XmlNode xnEvent in nodeList)
// 			{
// 				xnEvent.ParentNode.RemoveChild(xnEvent);
// 			}

			string buffer = newDoc.InnerXml;
			updateGL(fileName, W2GTag.W2G_NORMAL_NAME);
			updateGL(buffer, W2GTag.W2G_NORMAL_DATA);
			xmlCtrl.refreshVRect();
		}

		public void refreshAllCtrlUIHeader()
		{
			foreach (KeyValuePair<string, OpenedFile> pairOpenedFile in m_mapOpenedFiles.ToList())
			{
				if (pairOpenedFile.Value != null && pairOpenedFile.Value.m_frame != null && pairOpenedFile.Value.m_frame is XmlControl)
				{
					foreach (KeyValuePair<string, BoloUI.Basic> pairCtrlUI in ((XmlControl)pairOpenedFile.Value.m_frame).m_mapCtrlUI.ToList())
					{
						if (pairCtrlUI.Value != null)
						{
							pairCtrlUI.Value.initHeader();
						}
					}
				}
			}
		}
		public void refreshAllSkinHeader()
		{
			foreach (KeyValuePair<string, OpenedFile> pairOpenedFile in m_mapOpenedFiles.ToList())
			{
				if (pairOpenedFile.Value.m_frame is XmlControl)
				{
					foreach (KeyValuePair<string, BoloUI.ResBasic> pairSkin in ((XmlControl)pairOpenedFile.Value.m_frame).m_mapSkin.ToList())
					{
						if (pairSkin.Value != null)
						{
							pairSkin.Value.initHeader();
						}
					}
				}
			}
		}
		private void selCtrlBybutton(object sender, RoutedEventArgs e)
		{
		}

		private void mx_checkVName_Checked(object sender, RoutedEventArgs e)
		{
			m_vCtrlName = true;
			refreshAllCtrlUIHeader();
		}
		private void mx_checkVName_Unchecked(object sender, RoutedEventArgs e)
		{
			m_vCtrlName = false;
			refreshAllCtrlUIHeader();
		}
		private void mx_checkVId_Checked(object sender, RoutedEventArgs e)
		{
			m_vCtrlId = true;
			refreshAllCtrlUIHeader();
		}
		private void mx_checkVId_Unchecked(object sender, RoutedEventArgs e)
		{
			m_vCtrlId = false;
			refreshAllCtrlUIHeader();
		}
		private void mx_toolSave_Click(object sender, RoutedEventArgs e)
		{
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (curXmlCtrl != null)
			{
				curXmlCtrl.saveCurStatus();
			}
		}
		private void mx_toolSaveAll_Click(object sender, RoutedEventArgs e)
		{
			foreach (KeyValuePair<string, OpenedFile> pairFile in m_mapOpenedFiles.ToList())
			{
				if (pairFile.Value.frameIsXmlCtrl())
				{
					((XmlControl)pairFile.Value.m_frame).saveCurStatus();
				}
			}
		}
		public bool curFileUndo()
		{
			OpenedFile openFileDef;

			if (m_curFile != "" &&
				m_mapOpenedFiles.TryGetValue(m_curFile, out openFileDef) &&
				openFileDef != null &&
				openFileDef.m_frame != null &&
				openFileDef.m_frame is XmlControl &&
				openFileDef.m_lstOpt != null)
			{
				openFileDef.m_lstOpt.undo();

				return true;
			}
			else
			{
				return false;
			}
		}
		private void mx_toolUndo_Click(object sender, RoutedEventArgs e)
		{
			curFileUndo();
		}
		public bool curFileRedo()
		{
			OpenedFile openFileDef;

			if (m_curFile != "" &&
				m_mapOpenedFiles.TryGetValue(m_curFile, out openFileDef) &&
				openFileDef != null &&
				openFileDef.m_frame != null &&
				openFileDef.m_frame is XmlControl &&
				openFileDef.m_lstOpt != null)
			{
				openFileDef.m_lstOpt.redo();

				return true;
			}
			else
			{
				return false;
			}
		}
		private void mx_toolRedo_Click(object sender, RoutedEventArgs e)
		{
			curFileRedo();
		}

		private void mx_toolCut_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canCut())
			{
				XmlItem.getCurItem().cutItem();
			}
		}
		private void mx_toolCopy_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canCopy())
			{
				XmlItem.getCurItem().copyItem();
			}
		}
		private void mx_toolPaste_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.getCurItem() != null && Keyboard.FocusedElement != null &&
				Keyboard.FocusedElement is RadioButton &&
				(((RadioButton)Keyboard.FocusedElement).Parent is XmlItem ||
				((RadioButton)Keyboard.FocusedElement).Parent.GetType().BaseType.ToString() == "UIEditor.BoloUI.XmlItem") &&
				XmlItem.getCurItem() == (UIEditor.BoloUI.XmlItem)(((RadioButton)Keyboard.FocusedElement).Parent))
			{
				XmlItem.getCurItem().pasteItem();
			}
		}
		private void mx_toolDelete_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canDelete())
			{
				XmlItem.getCurItem().deleteItem();
			}
		}
		private void mx_toolMoveUp_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveUp())
			{
				XmlItem.getCurItem().moveUpItem();
			}
		}
		private void mx_toolMoveDown_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveDown())
			{
				XmlItem.getCurItem().moveDownItem();
			}
		}
		private void mx_toolMoveToParent_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveToParent())
			{
				XmlItem.getCurItem().moveToParent();
			}
		}
		private void mx_toolMoveToChild_Click(object sender, RoutedEventArgs e)
		{
			if (XmlItem.s_menu != null && XmlItem.s_menu.canMoveToChild())
			{
				XmlItem.getCurItem().moveToChild();
			}
		}

		static void viewPrevFile(MainWindow pW)
		{
			if (pW.mx_workTabs.Items.Count > 1)
			{
				if (pW.mx_workTabs.SelectedIndex > 0)
				{
					pW.mx_workTabs.SelectedItem = pW.mx_workTabs.Items.GetItemAt(pW.mx_workTabs.SelectedIndex - 1);
				}
				else
				{
					pW.mx_workTabs.SelectedItem = pW.mx_workTabs.Items.GetItemAt(pW.mx_workTabs.Items.Count - 1);
				}
			}
		}
		static void viewNextFile(MainWindow pW)
		{
			if (pW.mx_workTabs.Items.Count > 1)
			{
				if (pW.mx_workTabs.SelectedIndex < pW.mx_workTabs.Items.Count - 1)
				{
					pW.mx_workTabs.SelectedItem = pW.mx_workTabs.Items.GetItemAt(pW.mx_workTabs.SelectedIndex + 1);
				}
				else
				{
					pW.mx_workTabs.SelectedItem = pW.mx_workTabs.Items.GetItemAt(0);
				}
			}
		}
		private void mx_newFile_Click(object sender, RoutedEventArgs e)
		{
			NewFileWin winNewFile = new NewFileWin(".\\data\\Template\\");
			winNewFile.ShowDialog();
		}
		private void mx_newProj_Click(object sender, RoutedEventArgs e)
		{
			NewFileWin winNewFile = new NewFileWin(".\\data\\ProjTemplate\\", true);
			winNewFile.ShowDialog();
		}
		private void mx_windowPrevFile_Click(object sender, RoutedEventArgs e)
		{
			viewPrevFile(this);
		}
		private void mx_windowNextFile_Click(object sender, RoutedEventArgs e)
		{
			viewNextFile(this);
		}
		private void mx_windowCloseFile_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile curFileDef = OpenedFile.getCurFileDef();

			if (curFileDef != null)
			{
				curFileDef.m_tabItem.closeFile();
			}
		}
		private void mx_windowCloseAll_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile.closeAllFile();
		}
		private void mx_windowCloseAllExSelf_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile.closeAllFile(OpenedFile.getCurFileDef());
		}
		private void mx_windowOpenFolder_Click(object sender, RoutedEventArgs e)
		{
			OpenedFile curFileDef = OpenedFile.getCurFileDef();

			if (curFileDef != null)
			{
				OpenedFile.openLocalFolder(curFileDef.m_path);
			}
		}

		static private string getStrFromItemHeader(object header, string type = "")
		{
			switch (type)
			{
				case "XmlItem":
					{
						XmlItem itemCtrl = (XmlItem)header;

						return itemCtrl.mx_text.Text;
					}
					break;
				default:
					{
						if (header is RadioButton || header.GetType().BaseType.ToString() == "System.Windows.Controls.RadioButton")
						{
							RadioButton rb = (RadioButton)header;

							return rb.Content.ToString();
						}
						else if (header is Grid)
						{
							Grid frame = (Grid)header;

							foreach (UIElement ue in frame.Children)
							{
								if (ue is RadioButton)
								{
									RadioButton rb = (RadioButton)ue;

									return rb.Content.ToString();
								}
							}
						}

						return header.ToString();
					}
					break;
			}
		}
		static public void refreshSearch(TreeViewItem viewItem, string key, bool isExpanded = false)
		{
			if (viewItem.Items.Count > 0)
			{
				foreach (TreeViewItem item in viewItem.Items)
				{
					if (key != "" && key != null)
					{
						object header = null;
						string type = "";

						if (item is XmlItem || item.GetType().BaseType.ToString() == "UIEditor.BoloUI.XmlItem")
						{
							XmlItem itemCtrl = (XmlItem)item;

							header = itemCtrl;
							type = "XmlItem";
						}
						else
						{
							header = item.Header;
						}

						if (getStrFromItemHeader(header, type).IndexOf(key, StringComparison.OrdinalIgnoreCase) < 0)
						{
							item.Visibility = System.Windows.Visibility.Collapsed;
						}
						else
						{
							for (object pItem = item.Parent;
								pItem is TreeViewItem ||
									pItem.GetType().BaseType.ToString() == "System.Windows.Controls.TreeViewItem" ||
									pItem.GetType().BaseType.BaseType.ToString() == "System.Windows.Controls.TreeViewItem" ||
									pItem.GetType().BaseType.BaseType.BaseType.ToString() == "System.Windows.Controls.TreeViewItem";
								pItem = ((TreeViewItem)pItem).Parent)
							{
								((TreeViewItem)pItem).Visibility = System.Windows.Visibility.Visible;
								((TreeViewItem)pItem).IsExpanded = true;
							}
							refreshSearch(item, null, true);
							continue;
						}
					}
					else
					{
						item.Visibility = System.Windows.Visibility.Visible;
						if (!isExpanded)
						{
							item.IsExpanded = false;
						}
					}
					refreshSearch(item, key, isExpanded);
				}
			}
		}
		private void mx_search_TextChanged(object sender, TextChangedEventArgs e)
		{
			bool isDefEx = false;

			if (mx_search.Text != "")
			{
				refreshSearch(mx_treePro, null, isDefEx);
				refreshSearch(mx_treePro, mx_search.Text.ToString());
			}
			else
			{
				refreshSearch(mx_treePro, null, isDefEx);
			}
		}
		private void mx_uiSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			bool isDefEx = true;
			XmlControl curCtrl = XmlControl.getCurXmlControl();

			if (curCtrl != null)
			{
				if (mx_uiSearch.Text != "")
				{
					refreshSearch(curCtrl.m_treeUI, null, isDefEx);
					refreshSearch(curCtrl.m_treeUI, mx_uiSearch.Text.ToString(), isDefEx);
				}
				else
				{
					refreshSearch(curCtrl.m_treeUI, null, isDefEx);
				}
			}
		}
		private void mx_skinSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			bool isDefEx = true;
			XmlControl curCtrl = XmlControl.getCurXmlControl();

			if (curCtrl != null)
			{
				if (mx_skinSearch.Text != "")
				{
					refreshSearch(curCtrl.m_treeSkin, null, isDefEx);
					refreshSearch(curCtrl.m_treeSkin, mx_skinSearch.Text.ToString(), isDefEx);
				}
				else
				{
					refreshSearch(curCtrl.m_treeSkin, null, isDefEx);
				}
			}
		}

		private void mx_help_Click(object sender, RoutedEventArgs e)
		{
			string path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location) + "/doc/help.html";

			if (System.IO.File.Exists(path))
			{
				System.Diagnostics.Process.Start("file:///" + path);
			}
			else
			{
				Public.ResultLink.createResult("\r\n没有找到文件：" + path + "。", Public.ResultType.RT_ERROR);
			}
		}
		private void mx_version_Click(object sender, RoutedEventArgs e)
		{
			string path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location) + "/doc/version.html";

			if (System.IO.File.Exists(path))
			{
				System.Diagnostics.Process.Start("file:///" + path);
			}
			else
			{
				Public.ResultLink.createResult("\r\n没有找到文件：" + path + "。", Public.ResultType.RT_ERROR);
			}
		}

		private void mx_isViewMode_Checked(object sender, RoutedEventArgs e)
		{
			updateGL("true", W2GTag.W2G_VIEWMODE);
		}
		private void mx_isViewMode_Unchecked(object sender, RoutedEventArgs e)
		{
			updateGL("false", W2GTag.W2G_VIEWMODE);
		}
		private void mx_isScriptMode_Checked(object sender, RoutedEventArgs e)
		{
			updateGL("true", W2GTag.W2G_VIEWMODE_SCRIPT);
		}
		private void mx_isScriptMode_Unchecked(object sender, RoutedEventArgs e)
		{
			updateGL("false", W2GTag.W2G_VIEWMODE_SCRIPT);
		}
		private void mx_isShowCtrlRect_Checked(object sender, RoutedEventArgs e)
		{

		}
		private void mx_isShowCtrlRect_Unchecked(object sender, RoutedEventArgs e)
		{
			MainWindow.s_pW.updateGL("", W2GTag.W2G_SELECT_UI);
		}
		private void mx_isUpdateMode_Checked(object sender, RoutedEventArgs e)
		{

		}
		private void mx_isUpdateMode_Unchecked(object sender, RoutedEventArgs e)
		{

		}
		private void mx_showBorder_Checked(object sender, RoutedEventArgs e)
		{
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (curXmlCtrl != null)
			{
				curXmlCtrl.refreshVRect();
			}
			updateGL("true", W2GTag.W2G_SHOWBORDER);
		}
		private void mx_showBorder_Unchecked(object sender, RoutedEventArgs e)
		{
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (curXmlCtrl != null)
			{
				curXmlCtrl.refreshVRect();
			}
			updateGL("false", W2GTag.W2G_SHOWBORDER);
		}
		private void mx_showBack_Checked(object sender, RoutedEventArgs e)
		{
			string pathBack = Setting.getBackgroundPath();
			if(System.IO.File.Exists(pathBack))
			{
				FileInfo fi = new FileInfo(pathBack);
				string dstFolder = Setting.s_projPath;

				if(PackImage.createSSImage(fi.FullName, dstFolder) == true)
				{
					MainWindow.s_pW.updateGL(dstFolder + "\\bg_ss.tga", W2GTag.W2G_PATH_BACKGROUND);
				}
			}
		}
		private void mx_showBack_Unchecked(object sender, RoutedEventArgs e)
		{
			MainWindow.s_pW.updateGL("", W2GTag.W2G_PATH_BACKGROUND);
		}
		private void mx_viewShowRenderCacheFrame_Click(object sender, RoutedEventArgs e)
		{
			mx_viewShowRenderCache.IsChecked = mx_viewShowRenderCache.IsChecked == true ? false : true;
		}
		private void mx_viewShowRenderCache_Checked(object sender, RoutedEventArgs e)
		{
			MainWindow.s_pW.updateGL("true", W2GTag.W2G_RENDERCACHE_SWITCH);
			if (mx_lvRenderCache != null)
			{
				mx_lvRenderCache.Visibility = System.Windows.Visibility.Visible;
			}
		}
		private void mx_viewShowRenderCache_Unchecked(object sender, RoutedEventArgs e)
		{
			MainWindow.s_pW.updateGL("false", W2GTag.W2G_RENDERCACHE_SWITCH);
			if (mx_lvRenderCache != null)
			{
				mx_lvRenderCache.Visibility = System.Windows.Visibility.Collapsed;
			}
		}
		private void mx_btnNesting_Click(object sender, RoutedEventArgs e)
		{
			if (Project.Setting.s_projPath != null && Project.Setting.s_projPath != "")
			{
				if (!System.IO.Directory.Exists(Project.Setting.s_projPath + "\\images\\"))
				{
					Directory.CreateDirectory(Project.Setting.s_projPath + "\\images\\");
				}
				ImageTools.ImageNesting winNesting = new ImageTools.ImageNesting(Project.Setting.s_projPath + "\\images\\", "*.png", 1);
				winNesting.ShowDialog();
			}
		}
		private void mx_showNotUsingSkin_Click(object sender, RoutedEventArgs e)
		{
			ResultLink.createResult("\r\n=======================开始统计未被引用的皮肤=========================", true);
			XmlControl.showNotUsingSkin();
			Public.ResultLink.createResult("\r\n显示结果存放于[" + Setting.s_projPath + "\\skinCount.txt" + "]文件中。", Public.ResultType.RT_INFO, null, true);
			ResultLink.createResult("\r\n=======================统计未被引用的皮肤结束=========================", true);
		}
		private void mx_checkBaseId_Click(object sender, RoutedEventArgs e)
		{
			ResultLink.createResult("\r\n开始检测重复的baseID", true);
			XmlControl.checkAllBoloUIXmlControlBaseId();
			ResultLink.createResult("\r\n检测结束", true);
		}
		private void mx_exportLang_Click(object sender, RoutedEventArgs e)
		{
			Setting.exportLanguageSettingLog();
		}
		private void mx_refreshShape_Click(object sender, RoutedEventArgs e)
		{
			Public.ResultLink.createResult("\r\n开始shape的重排", Public.ResultType.RT_INFO);
			if (Project.Setting.s_skinPath != null && Project.Setting.s_skinPath != "")
			{
				if (Directory.Exists(Project.Setting.s_skinPath))
				{
					DirectoryInfo di = new DirectoryInfo(Project.Setting.s_skinPath);

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
															this,
															null,
															(XmlElement)xnShape);
														XmlOperation.HistoryNode.insertXmlNode(
															this,
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
			Public.ResultLink.createResult("\r\n重排完成", Public.ResultType.RT_INFO);
		}
		private void mx_templateSetting_Click(object sender, RoutedEventArgs e)
		{

		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

		static double s_x = 0;
		static double s_y = 0;
		private void mx_scrollFrame_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			/*
			mx_debug.Text += "sw:" + mx_scrollFrame.ActualWidth + "\t" + "sh:" + mx_scrollFrame.ActualHeight + "\t" +
				"sx:" + mx_scrollFrame.HorizontalOffset + "\t" + "sy:" + mx_scrollFrame.VerticalOffset + "\r\n";
			*/
			double dx = s_x - mx_scrollFrame.HorizontalOffset;
			double dy = s_y - mx_scrollFrame.VerticalOffset;

			s_x = mx_scrollFrame.HorizontalOffset;
			s_y = mx_scrollFrame.VerticalOffset;
			MoveWindow(m_msgMng.m_hwndGL, (int)-s_x, (int)-s_y, m_screenWidth, m_screenHeight, true);
		}
		private void getSizeByResolutionString(string strScreen, out int w, out int h)
		{
			string[] sArray = Regex.Split(strScreen, " x ", RegexOptions.IgnoreCase);

			if (sArray.Count() >= 2)
			{
				int.TryParse(sArray[0], out w);
				int.TryParse(sArray[1], out h);
			}
			else
			{
				w = 960;
				h = 540;
			}
		}
		public bool m_isLoadOver;
		public void refreshResolution()
		{
			if (m_isLoadOver != true)
			{
				return;
			}
			if (mx_resolution.SelectedItem != null && mx_resolution.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem cbiSel = (ComboBoxItem)mx_resolution.SelectedItem;
				string strScreen = cbiSel.Content.ToString();
				int w, h;

				getSizeByResolutionString(strScreen, out w, out h);
				m_screenWidth = w;
				m_screenHeight = h;
			}
			if (mx_resolutionBasic.SelectedItem != null && mx_resolutionBasic.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem cbiSel = (ComboBoxItem)mx_resolutionBasic.SelectedItem;
				string strScreen = cbiSel.Content.ToString();
				int w, h;

				getSizeByResolutionString(strScreen, out w, out h);
				m_screenWidthBasic = w;
				m_screenHeightBasic = h;
			}
			if (mx_isMoba.IsChecked == true)
			{
				m_isMoba = true;
				mx_resolutionBasic.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				m_isMoba = false;
				mx_resolutionBasic.Visibility = System.Windows.Visibility.Collapsed;
			}

			refreshScreenStatus();
		}
		private void mx_resolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			refreshResolution();
		}
		private void mx_resolutionBasic_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			refreshResolution();
		}
		private void mx_isMoba_Checked(object sender, RoutedEventArgs e)
		{
			refreshResolution();
		}
		private void mx_isMoba_Unchecked(object sender, RoutedEventArgs e)
		{
			refreshResolution();
		}

		private void refreshSkinEditorAtMainWindow()
		{
			if (mx_skinEditor != null && mx_treeFrame != null && mx_treeFrame.SelectedItem == mx_skinEditor)
			{
				mx_skinEditor.refreshSkinEditor();
			}
		}
		private void mx_isEnableTheme_Checked(object sender, RoutedEventArgs e)
		{
			Setting.setEnableTheme(true);
			refreshSkinEditorAtMainWindow();
		}
		private void mx_isEnableTheme_Unchecked(object sender, RoutedEventArgs e)
		{
			Setting.setEnableTheme(false);
			refreshSkinEditorAtMainWindow();
		}
		private void mx_cbThemeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string msgData = "";

			if (mx_cbThemeName.SelectedItem != null && mx_cbThemeName.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem selCbi = (ComboBoxItem)mx_cbThemeName.SelectedItem;

				msgData = selCbi.Content.ToString();
			}
			refreshSkinEditorAtMainWindow();

			updateGL(msgData, W2GTag.W2G_THEME_THEMENAME);
		}
		private void mx_cbLangName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string msgData = "";

			if (mx_cbLangName.SelectedItem != null && mx_cbLangName.SelectedItem is ComboBoxItem)
			{
				ComboBoxItem selCbi = (ComboBoxItem)mx_cbLangName.SelectedItem;
				msgData += selCbi.ToolTip.ToString();
			}
			refreshSkinEditorAtMainWindow();

			updateGL(msgData, W2GTag.W2G_THEME_LANGUAGE);
		}

		public void refreshCurFile()
		{
			OpenedFile fileDef;

			if (m_curFile != null && m_curFile != "" && m_mapOpenedFiles.TryGetValue(m_curFile, out fileDef))
			{
				if (fileDef != null && fileDef.m_frame != null && fileDef.m_frame is XmlControl)
				{
					XmlControl xmlDef = (XmlControl)fileDef.m_frame;

					if (xmlDef.m_xmlDoc != null && xmlDef.m_xmlDoc.DocumentElement.Name == "BoloUI" && xmlDef.m_isOnlySkin == false)
					{
						updateXmlToGL(xmlDef);
					}
				}
			}
		}
		private void mx_isShowAll_CheckChanged(object sender, RoutedEventArgs e)
		{
			Setting.refreshSkinIndex();
			MainWindow.s_pW.updateGL("", W2GTag.W2G_IMAGE_RELOAD);
			MainWindow.s_pW.updateGL("", W2GTag.W2G_PARTICLE_RELOAD);
			refreshCurFile();
		}

		public bool m_isCanEdit;
		public bool m_isTextChanged;
		public long m_tLast;
		public int m_hitCount;
		public DispatcherTimer m_textTimer;
		public DispatcherTimer m_fileChangeTimer;
		public Run m_lastSelRun;
		public Run m_lastUpdateRun;
		private void mx_xmlText_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (m_isCanEdit)
			{
				m_isCanEdit = false;

				m_hitCount = 0;
				m_isTextChanged = true;
				AttrList.hiddenAllAttr();

				m_isCanEdit = true;
			}
		}
		//文件修改触发器
		private void m_fileChangeTimer_Tick(object send, EventArgs e)
		{
			ArrayList arrFileChanged = OpenedFile.checkFileChangedAtOutside();

			foreach (object objFile in arrFileChanged)
			{
				if (objFile is string)
				{
					string path = (string)objFile;

					if (File.Exists(path))
					{
						ReloadConfirm winReload = new ReloadConfirm(path);
						winReload.ShowDialog();
					}
				}
			}
			if (Setting.checkUiConfigReload())
			{
				updateGL("", W2GTag.W2G_SETTING_RELOAD_UICONFIG);
				refreshCurFile();
				ResultLink.createResult("\r\n检测到\"uiconfig.xml\"发生改变，已经自动重新加载", ResultType.RT_INFO, null, true);
			}

			//<inc>太耗费资源
			//ResultLink.refreshResultVisibility();
		}
		private void m_textTimer_Tick(object send, EventArgs e)
		{
			if (MainWindow.s_pW.m_lastUpdateRun != null)
			{
				XmlItem.changeLightRun(MainWindow.s_pW.m_lastUpdateRun);
				MainWindow.s_pW.m_lastUpdateRun = null;
			}
			if (m_isTextChanged && m_hitCount < 5)
			{
				m_hitCount++;
			}
			else
			{
				m_hitCount = 0;
				if (m_isTextChanged)
				{
					m_isCanEdit = false;
					m_isTextChanged = false;
					TextRange textRange = new TextRange(mx_xmlText.Document.ContentStart, mx_xmlText.Document.ContentEnd);

					OpenedFile fileDef;
					if (m_mapOpenedFiles.TryGetValue(m_curFile, out fileDef))
					{
						if (fileDef.m_frame != null && fileDef.m_frame is XmlControl)
						{
							XmlControl xmlCtrl = (XmlControl)fileDef.m_frame;
							XmlDocument newDoc = new XmlDocument();

							try
							{
								newDoc.LoadXml(textRange.Text);
							}
							catch
							{
								m_isCanEdit = true;
								return;
							}
							string oldStr = XmlControl.getOutXml(xmlCtrl.m_xmlDoc);
							string newStr = XmlControl.getOutXml(newDoc);

							if (string.Compare(oldStr, newStr) != 0)
							{
								fileDef.m_lstOpt.addOperation(new XmlOperation.HistoryNode(xmlCtrl.m_xmlDoc, newDoc));
								xmlCtrl.resetXmlItemLink();
							}
						}
					}
					m_isCanEdit = true;
				}
			}
		}
		private void mx_bupHistory_Loaded(object sender, RoutedEventArgs e)
		{
			XmlNode xnConf = m_docConf.SelectSingleNode("Config");

			if (xnConf != null)
			{
				XmlNode xnBup = xnConf.SelectSingleNode("bupHistory");

				if (xnBup != null)
				{
					int countItem = 0;
					mx_bupHistory.Items.Clear();
					for (XmlNode xnRow = xnBup.FirstChild; xnRow != null; countItem++)
					{
						if (xnRow.NodeType == XmlNodeType.Element)
						{
							XmlElement xeRow = (XmlElement)xnRow;

							if (xeRow.Name == "row")
							{
								string bupPath = xeRow.GetAttribute("key");

								if (bupPath != "")
								{
									if (System.IO.File.Exists(bupPath))
									{
										MenuItem bupItem = new MenuItem();

										bupItem.Header = "_" + countItem.ToString() + " " + bupPath;
										bupItem.ToolTip = bupPath;
										bupItem.Click += mx_bupItem_Click;
										mx_bupHistory.Items.Add(bupItem);
									}
									else
									{
										XmlNode xnDel = xnRow;

										xnBup.RemoveChild(xnDel);
										xnRow = xnRow.NextSibling;
										continue;
									}
								}
							}
						}
						xnRow = xnRow.NextSibling;
					}
				}
			}
			m_docConf.Save(conf_pathConf);
		}
		void mx_bupItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.MenuItem)
			{
				openProjByPath(
					System.IO.Path.GetDirectoryName((sender as MenuItem).ToolTip.ToString()),
					System.IO.Path.GetFileName((sender as MenuItem).ToolTip.ToString()));
			}
		}
		public void changeCurSearchSelection(TextRange trSel)
		{
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (trSel != null && curXmlCtrl != null)
			{
				if (trSel.Start.Parent is Run)
				{
					Run runSel = (Run)trSel.Start.Parent;

					if (curXmlCtrl.m_curSearchRun != null)
					{
						curXmlCtrl.m_curSearchRun.Background = null;
					}
					curXmlCtrl.m_curSearchRun = runSel;
					runSel.Background = new SolidColorBrush(XmlControl.s_arrTextColor[(int)XmlControl.XmlTextColorType.BCK_SEARCH]);
				}
			}
		}
		private void setSearchHighLighted()
		{
			List<TextRange> lstRag = RichTextTools.FindAllMatchedTextRanges(mx_xmlText, mx_xmlTextSearch.Text);
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (curXmlCtrl != null && lstRag != null && lstRag.Count != 0)
			{

				if (lstRag.Count <= curXmlCtrl.m_curSearchIndex)
				{
					curXmlCtrl.m_curSearchIndex = 0;
				}

				TextRange trSel = lstRag[curXmlCtrl.m_curSearchIndex];

				if (trSel != null)
				{
					mx_xmlText.Focus();
					mx_xmlText.Selection.Select(trSel.Start, trSel.End);
					mx_xmlTextSearch.Focus();
					changeCurSearchSelection(trSel);
				}
			}
		}
		private void mx_xmlTextSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (curXmlCtrl != null)
			{
				curXmlCtrl.m_curSearchIndex = 0;
				setSearchHighLighted();
			}
		}
		private void mx_xmlTextSearch_KeyDown(object sender, KeyEventArgs e)
		{
			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if ((e.Key == Key.Return || e.Key == Key.Enter) && curXmlCtrl != null)
			{
				curXmlCtrl.m_curSearchIndex++;
				setSearchHighLighted();
			}
		}

		private void mx_showUITab_Checked(object sender, RoutedEventArgs e)
		{
			if (mx_drawFrame != null && mx_textFrame != null)
			{
				mx_drawFrame.Visibility = System.Windows.Visibility.Visible;
				mx_textFrame.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void mx_showTextTab_Checked(object sender, RoutedEventArgs e)
		{
			if (mx_drawFrame != null && mx_textFrame != null)
			{
				if (m_mapOpenedFiles.Count > 0)
				{
					mx_textFrame.Visibility = System.Windows.Visibility.Visible;
				}
				else
				{
					mx_textFrame.Visibility = System.Windows.Visibility.Collapsed;
				}
				mx_drawFrame.Visibility = System.Windows.Visibility.Collapsed;
			}

			XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

			if (curXmlCtrl != null)
			{
				curXmlCtrl.refreshXmlText();
				if (curXmlCtrl.m_curItem != null)
				{
					curXmlCtrl.m_curItem.changeSelectItem();
				}
			}
		}
		private void mx_treeFrame_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mx_skinEditor != null)
			{
				if (mx_treeFrame.SelectedItem == mx_treeFrameUI)
				{
					XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

					if (curXmlCtrl != null && curXmlCtrl.m_curItem != null && curXmlCtrl.m_curItem is ResBasic)
					{
						ResBasic curResItem = (ResBasic)curXmlCtrl.m_curItem;

						if (curResItem.m_isSkinEditor == true)
						{
							if (mx_skinEditor.m_curCtrl != null)
							{
								mx_skinEditor.m_curCtrl.changeSelectItem();
							}
						}
					}
				}
				else if (mx_treeFrame.SelectedItem == mx_skinEditor)
				{
					if (e.AddedItems.Count > 0 && e.AddedItems[0] == mx_skinEditor)
					{
						mx_skinEditor.refreshSkinEditor();
					}
					if (mx_skinEditor.mx_skinApprPre.SelectedItem == null || mx_skinEditor.mx_skinApprSuf.SelectedItem == null)
					{
						mx_skinEditor.mx_skinApprPre.SelectedIndex = 0;
						mx_skinEditor.mx_skinApprSuf.SelectedIndex = 0;
						//mx_skinEditor.mx_treeAppr.Items.Clear();
					}

					return;
				}
				updateGL("<null>:-1", W2GTag.W2G_CURSTATE);
			}
		}
		private void mx_textFrame_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				XmlControl curXmlCtrl = XmlControl.getCurXmlControl();

				if (curXmlCtrl != null && curXmlCtrl.m_curItem != null)
				{
					curXmlCtrl.m_curItem.gotoSelectXe();
				}
			}
		}
		private void mx_clearResult_Click(object sender, RoutedEventArgs e)
		{
			Public.ResultLink.s_curResultFrame.Inlines.Clear();
		}

		private void mx_resolutionBasic_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

		}

		private void mx_statistics_Click(object sender, RoutedEventArgs e)
		{

		}
		private void mx_projSetting_Click(object sender, RoutedEventArgs e)
		{
			if (Project.Setting.s_docProj != null)
			{
				ProjectSettingWin winPs = new ProjectSettingWin();

				winPs.ShowDialog();
			}
		}
		private void mx_toolPackUi_Click(object sender, RoutedEventArgs e)
		{
			Setting.packUiMod();
		}
		private void mx_toolPackScript_Click(object sender, RoutedEventArgs e)
		{
			Setting.packScriptMod();
		}

		private void mx_toolRunGame_Click(object sender, RoutedEventArgs e)
		{
			Setting.openGame();
		}
		private void mx_getHwnd_Click(object sender, RoutedEventArgs e)
		{
			if (System.Diagnostics.Process.GetProcessesByName("SSUIEditor").Count() > 0)
			{
				mx_hwndDebug.Text = System.Diagnostics.Process.GetProcessesByName("SSUIEditor")[0].MainWindowHandle.ToString();
			}
		}

		private void mx_showErrorResult_Checked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showErrorResult_Unchecked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showWarningResult_Checked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showWarningResult_Unchecked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showInfoResult_Checked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showInfoResult_Unchecked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showOtherResult_Checked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}
		private void mx_showOtherResult_Unchecked(object sender, RoutedEventArgs e)
		{
			ResultLink.refreshResultVisibility();
		}

		const string conf_errorString = null;
		private void mx_error_Click(object sender, RoutedEventArgs e)
		{
			string iToldYouThat = conf_errorString.Substring(0, 1);
		}

		private void mx_tbTimeSleep_TextChanged(object sender, TextChangedEventArgs e)
		{
			int iSleepTime;

			if (int.TryParse(mx_tbTimeSleep.Text, out iSleepTime))
			{
				if (iSleepTime <= 0)
				{
					iSleepTime = 33;
				}
				updateGL(iSleepTime.ToString(), W2GTag.W2G_SETTING_SLEEPTIME);
			}
		}
		private void mx_btnSendScript_Click(object sender, RoutedEventArgs e)
		{
			string scriptName = mx_tbScriptName.Text;

			if(scriptName != "")
			{
				updateGL(scriptName, W2GTag.W2G_SCRIPT_RUN);
			}
		}
		static string CapText(Match m)
		{
			string x = m.ToString();
			int maxCount = 4;

			if (char.IsLower(x[maxCount]))
			{
				return x.Substring(0, maxCount) + char.ToUpper(x[maxCount]) + x.Substring(maxCount + 1, x.Length - maxCount - 1);
			}

			return x;
		}

		private void mx_replaceTools_Click(object sender, RoutedEventArgs e)
		{
			string path = mx_tbReplacePath.Text;
			if(File.Exists(path))
			{
				StreamReader sr = new StreamReader(path, Encoding.Default);
				string strFileData;
				Regex regex = new Regex(@"([ :][gs]et)([a-z])");

				strFileData = sr.ReadToEnd();

				string toLower = regex.Replace(strFileData, new MatchEvaluator(CapText));

				sr.Close();

				StreamWriter sw = new StreamWriter(path, false, Encoding.Default);

				sw.Write(toLower);
				sw.Close();
			}
		}

		private void updateCamera(object sender, W2GTag tag)
		{
			if (sender is TextBox)
			{
				TextBox tb = (TextBox)sender;
				string data = "";

				if (tb.Text != "")
				{
					data = tb.Text;
				}
				else
				{
					data = "0";
				}
				updateGL(data, tag);
			}
		}
		private void mx_cbCameraIs3D_Checked(object sender, RoutedEventArgs e)
		{
			updateGL("true", W2GTag.W2G_CAMERA_IS3D);
		}
		private void mx_cbCameraIs3D_Unchecked(object sender, RoutedEventArgs e)
		{
			updateGL("false", W2GTag.W2G_CAMERA_IS3D);
		}
		private void mx_tbCameraFov_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_FOV);
		}
		private void mx_tbCameraAspect_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_ASPECT);
		}
		private void mx_tbCameraNear_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_NEAR);
		}
		private void mx_tbCameraFar_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_FAR);
		}
		private void mx_tbCameraX_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_X);
		}
		private void mx_tbCameraY_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_Y);
		}
		private void mx_tbCameraZ_TextChanged(object sender, TextChangedEventArgs e)
		{
			updateCamera(sender, W2GTag.W2G_CAMERA_Z);
		}

		protected class RenderCacheViewData
		{
			public int m_id { get; set; }
			public string m_type { get; set; }
			public string m_image { get; set; }
			public int m_bcCount { get; set; }
			public string m_color { get; set; }
			public bool m_isVisible { get; set; }
			public RenderCacheViewData(int id, string type, string image, string color, bool isVisible)
			{
				m_id = id;
				m_type = type;
				m_image = image;
				m_color = color;
				m_isVisible = isVisible;
			}
//			public bool IsActived { get; set; }
// 			public string BackGround
// 			{
// 				get
// 				{
// 					return IsActived
// 					  ? "/test;component/Assets/Images/UserItemNull.png"
// 					  : "/test;component/Assets/Images/UserItemNullg.png";
// 				}
// 			}
		}
		private void mx_lvRenderCache_Loaded(object sender, RoutedEventArgs e)
		{
		}
		private void mx_lviRenderCache_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

		}
		private void mx_lvRenderCache_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(mx_lvRenderCache.SelectedItem != null && mx_lvRenderCache.SelectedItem is RenderCacheViewData)
			{
				RenderCacheViewData selData = (RenderCacheViewData)mx_lvRenderCache.SelectedItem;

				updateGL(selData.m_id.ToString(), W2GTag.W2G_RENDERCACHE_IMAGECOLOR);
			}
		}

		private void mx_viewLanguegeFrame_Click(object sender, RoutedEventArgs e)
		{
			mx_viewLanguege.IsChecked = mx_viewLanguege.IsChecked == true ? false : true;
		}
		private void mx_viewLanguege_Checked(object sender, RoutedEventArgs e)
		{
			if (mx_languageToolsFrame != null)
			{
				mx_languageToolsFrame.Visibility = System.Windows.Visibility.Visible;
			}
		}
		private void mx_viewLanguege_Unchecked(object sender, RoutedEventArgs e)
		{
			if (mx_languageToolsFrame != null)
			{
				mx_languageToolsFrame.Visibility = System.Windows.Visibility.Collapsed;
			}
		}
	}

	class TreeViewLineConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
		object parameter, System.Globalization.CultureInfo culture)
		{
			TreeViewItem item = (TreeViewItem)value;
			ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
			return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
		}

		public object ConvertBack(object value, Type targetType,
		object parameter, System.Globalization.CultureInfo culture)
		{
			return false;
		}
	}
}