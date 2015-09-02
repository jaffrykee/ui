using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;

namespace UIEditor
{
	public class ControlHost : HwndHost
	{
		public Process m_process;

		MsgManager m_msgMng;
		int m_hostHeight;
		int m_hostWidth;

		public ControlHost(MsgManager msgMng,double width = 960, double height = 640)
		{
			m_msgMng = msgMng;
			m_hostWidth = (int)width;
			m_hostHeight = (int)height;
			//(PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource).AddHook(new System.Windows.Interop.HwndSourceHook(WndProc));
		}

		internal const int
			WS_CHILD = 0x40000000,
			WS_VISIBLE = 0x10000000,
			LBS_NOTIFY = 0x00000001,
			HOST_ID = 0x00000002,
			LISTBOX_ID = 0x00000001,
			WS_HSCROLL = 0x00100000,
			WS_VSCROLL = 0x00200000,
			WS_CLIPSIBLINGS = 0x4000000,
			WS_CLIPCHILDREN = 0x02000000,
			WS_BORDER = 0x00800000;

		//PInvoke declarations
		[DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
		internal static extern IntPtr CreateWindowEx(
			int dwExStyle,
			string lpszClassName,
			string lpszWindowName,
			int style,
			int x, int y,
			int width, int height,
			IntPtr hwndParent,
			IntPtr hMenu,
			IntPtr hInst,
			[MarshalAs(UnmanagedType.AsAny)] object pvParam);

		protected override HandleRef BuildWindowCore(HandleRef hwndParent)
		{
			m_msgMng.m_hwndGLParent = IntPtr.Zero;

			m_msgMng.m_hwndGLParent = CreateWindowEx(
				0, "static", "",
				WS_CHILD | WS_VISIBLE,
				0, 0,
				m_hostWidth, m_hostHeight,
				hwndParent.Handle,
				(IntPtr)HOST_ID,
				IntPtr.Zero,
				0);

			string strRunMode;

			if (MainWindow.s_pW.m_isDebug)
			{
				strRunMode = "true";
			}
			else
			{
				strRunMode = "false";
			}

			m_process = System.Diagnostics.Process.Start(
				MainWindow.conf_pathGlApp,
				m_msgMng.m_hwndGLParent.ToString() + " " +
					m_hostWidth.ToString() + " " +
					m_hostHeight.ToString() + " " +
					strRunMode);

			return new HandleRef(this, m_msgMng.m_hwndGLParent);
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			handled = false;

			return IntPtr.Zero;
		}

		protected override void DestroyWindowCore(HandleRef hwnd)
		{
			DestroyWindow(hwnd.Handle);
		}

		[DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
		internal static extern bool DestroyWindow(IntPtr hwnd);
	}
}
