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
		W2G_PATH_PARTICLE = 0x0001,
		W2G_PATH_BACKGROUND = 0x0002,
		W2G_PATH_LANGUAGE = 0x0003,
		W2G_SETTING_SLEEPTIME = 0x0010,
		W2G_NORMAL_NAME = 0x0100,
		W2G_NORMAL_DATA = 0x0101,
		W2G_NORMAL_UPDATE = 0x0102,
		W2G_NORMAL_TURN = 0x0103,
		W2G_SKIN_NAME = 0x0110,
		W2G_SKIN_DATA = 0x0111,
		W2G_SELECT_UI = 0x0200,
		W2G_UI_VRECT = 0x0201,
		W2G_SHOWBORDER = 0x0202,
		W2G_DRAWRECT = 0x0203,
		W2G_DRAW_PARTICLE_LINE = 0x0204,
		W2G_VIEWMODE = 0x0300,
		W2G_VIEWSIZE = 0x0301,
		W2G_IMAGE_RELOAD = 0x0400,
		W2G_PARTICLE_RELOAD = 0x0401,
		W2G_THEME_ISENABLE = 0x0500,
		W2G_THEME_THEMENAME = 0x0501,
		W2G_THEME_LANGUAGE = 0x0502,
		W2G_SCRIPT_RUN = 0x0600,
	};
	public enum G2WTag
	{
		G2W_HWND = 0x0000,
		G2W_DRAW_COUNT = 0x0011,
		G2W_EVENT = 0x0003,
		G2W_UI_VRECT = 0x0004,
		G2W_PARTICLE_SELECT_KEYFRAME = 0x0005,
		G2W_PARTICLE_CHANGE_KEYFRAME = 0x0006
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