﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using UIEditor.Project;

namespace UIEditor
{
	public enum W2GTag
	{
		W2G_PATH = 0x0000,
		W2G_PATH_BACKGROUND = 0x0002,
		W2G_PATH_LANGUAGE = 0x0003,
		W2G_SETTING_SLEEPTIME = 0x0010,
		W2G_SETTING_RELOAD_UICONFIG = 0x0011,
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
		W2G_DRAW_CONTROL_LINE = 0x0205,   //
		W2G_CURSTATE = 0x0206,
		W2G_VIEWMODE = 0x0300,
		W2G_VIEWSIZE = 0x0301,
		W2G_VIEWMODE_SCRIPT = 0x0302,
		W2G_IMAGE_RELOAD = 0x0400,
		W2G_PARTICLE_RELOAD = 0x0401,
		W2G_THEME_ISENABLE = 0x0500,
		W2G_THEME_THEMENAME = 0x0501,
		W2G_THEME_LANGUAGE = 0x0502,
		W2G_SCRIPT_RUN = 0x0600,
		W2G_RENDERCACHE_IMAGECOLOR = 0x0700,
		W2G_RENDERCACHE_SWITCH = 0x0701,
		W2G_CAMERA_IS3D = 0x0800,
		W2G_CAMERA_FOV = 0x0801,
		W2G_CAMERA_ASPECT = 0x0802,
		W2G_CAMERA_NEAR = 0x0803,
		W2G_CAMERA_FAR = 0x0804,
		W2G_CAMERA_X = 0x0805,
		W2G_CAMERA_Y = 0x0806,
		W2G_CAMERA_Z = 0x0807,
	};
	public enum G2WTag
	{
		G2W_HWND = 0x0000,
		G2W_DRAW_COUNT = 0x0011,
		G2W_EVENT = 0x0003,
		G2W_UI_VRECT = 0x0004,
		G2W_PARTICLE_SELECT_KEYFRAME = 0x0005,
		G2W_PARTICLE_CHANGE_KEYFRAME = 0x0006,
		G2W_RENDERCACHE_DATA = 0x0700,
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

		public void updateGL(string buffer, W2GTag msgTag = W2GTag.W2G_NORMAL_DATA)
		{
			int len;
			byte[] charArr;
			COPYDATASTRUCT_SENDEX msgData;

			if (msgTag == W2GTag.W2G_PATH)
			{
				string artistPath = "";
				string freePath = "";
				string fontPath = "";
				string langPath = Path.GetDirectoryName(Setting.getLangPath());
				DirectoryInfo di = new DirectoryInfo(Project.Setting.getParticlePath());

				if (di.Parent != null)
				{
					artistPath = di.Parent.FullName;
				}
				else
				{
					artistPath = MainWindow.getArtistPath(buffer);
				}
				freePath = buffer;
				if (Setting.s_fontPath != "" && Setting.s_fontPath != null && File.Exists(Setting.s_fontPath))
				{
					FileInfo fi = new FileInfo(Setting.s_fontPath);

					fontPath = fi.Directory.Parent.FullName;
				}
				charArr = Encoding.Default.GetBytes(artistPath + "|" + freePath + "|" + fontPath + "|" + langPath);
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
					if (len != 0)
					{
						msgData.lpData = (IntPtr)tmpBuff;
					}
					else
					{
						msgData.lpData = (IntPtr)0;
					}
					msgData.cbData = len + 1;
					MainWindow.SendMessage(m_hwndGL, MainWindow.WM_COPYDATA, (int)m_hwndGLParent, ref msgData);
				}
			}
		}
	}
}