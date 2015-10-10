using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace UIEditor
{
	public enum W2GTag
	{
		W2G_PATH = 0x0000,
		W2G_NORMAL_NAME = 0x0001,
		W2G_NORMAL_DATA = 0x0011,
		W2G_NORMAL_UPDATE = 0x0021,
		W2G_NORMAL_TURN = 0x0101,
		W2G_SKIN_NAME = 0x0002,
		W2G_SKIN_DATA = 0x0012,
		W2G_SELECT_UI = 0x0003,
		W2G_UI_VRECT = 0x0004,
		W2G_DRAWRECT = 0x0014,
		W2G_VIEWMODE = 0x0005,
		W2G_VIEWSIZE = 0x0015,
		W2G_IMAGE_RELOAD = 0x0006
	};
	public enum G2WTag
	{
		G2W_HWND = 0x0000,
		G2W_EVENT = 0x0003,
		G2W_UI_VRECT = 0x0004,
	};
	public struct COPYDATASTRUCT_SEND
	{
		public IntPtr dwData;
		public int cbData;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpData;
	}
	public struct COPYDATASTRUCT_SENDEX
	{
		public IntPtr dwData;
		public int cbData;
		public IntPtr lpData;
	}
	public struct COPYDATASTRUCT
	{
		public IntPtr dwData;
		public int cbData;
		[MarshalAs(UnmanagedType.LPStr)]
		public IntPtr lpData;
	}

	public class MsgManager
	{
		public ControlHost m_GLHost;
		public IntPtr m_hwndGL;
		public IntPtr m_hwndGLParent;
		public IntPtr m_hwndGLFrame;

		public MsgManager(bool isInit = false, double width = 960, double height = 540)
		{
			m_hwndGL = IntPtr.Zero;
			m_hwndGLParent = IntPtr.Zero;
			m_hwndGLFrame = IntPtr.Zero;
			if (!isInit)
			{
				m_GLHost = new ControlHost(this, width, height);
			}
		}
	}
}
