﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIEditor.BoloUI;

namespace UIEditor.Public
{
	public enum ResultType
	{
		RT_ERROR,
		RT_WARNING,
		RT_INFO,
		RT_NONE
	}
	public class SkinLinkDef_T
	{
		public string m_xmlPath;
		public string m_skinName;

		public SkinLinkDef_T(string xmlPath, string skinName)
		{
			m_xmlPath = xmlPath;
			m_skinName = skinName;
		}
	}
	public partial class ResultLink : Run
	{
		static private Paragraph st_curResultFrame;
		static public Paragraph s_curResultFrame
		{
			get { return st_curResultFrame; }
			set
			{
				st_curResultFrame = value;
				MainWindow.s_pW.mx_docResult.Blocks.Clear();
				MainWindow.s_pW.mx_docResult.Blocks.Add(value);
			}
		}
		static private Brush s_lastBrush;
		static private Run st_curRun;
		static public Run s_curRun
		{
			get{ return st_curRun; }
			set
			{
				if(st_curRun != null)
				{
					st_curRun.Background = s_lastBrush;
				}
				s_lastBrush = value.Background;
				value.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0x33, 0x99, 0xff));
				st_curRun = value;
			}
		}
		public static BitmapImage s_bmpError = new BitmapImage(new Uri(@".\data\image\error.png", UriKind.Relative));
		public static BitmapImage s_bmpWarning = new BitmapImage(new Uri(@".\data\image\warning.png", UriKind.Relative));

		public ResultType m_rt;
		public string m_text;
		public object m_link;

		static public void showResult(string text, ResultType rt = ResultType.RT_NONE, object link = null)
		{
			if (text[0] == '\r' && text[1] == '\n')
			{
				text = text.Substring(2);
				if (s_curResultFrame.Inlines.Count > 0)
				{
					s_curResultFrame.Inlines.Add(new Public.ResultLink("\r\n", rt, link));
				}
				if (rt == ResultType.RT_WARNING || rt == ResultType.RT_ERROR)
				{
					Image imgResult = new Image();
					InlineUIContainer uiResult = new InlineUIContainer();

					switch (rt)
					{
						case ResultType.RT_WARNING:
							{
								imgResult.Source = s_bmpWarning;
							}
							break;
						case ResultType.RT_ERROR:
							{
								imgResult.Source = s_bmpError;
							}
							break;
						default:
							break;
					}
					imgResult.Height = 16;
					imgResult.Width = 16;
					uiResult.Child = imgResult;
					s_curResultFrame.Inlines.Add(uiResult);
				}
			}
			s_curResultFrame.Inlines.Add(new Public.ResultLink(text, rt, link));
		}
		public ResultLink(string text, ResultType rt = ResultType.RT_NONE, object link = null)
		{
			m_rt = rt;
			m_text = text;
			m_link = link;

			InitializeComponent();
			this.Text = m_text;
			this.MouseDown += ResultLink_MouseDown;

			switch(rt)
			{
				case ResultType.RT_ERROR:
					{
						//this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xff, 0x00, 0x00));
					}
					break;
				case ResultType.RT_WARNING:
					{
						//this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0xff, 0xff, 0x00));
					}
					break;
				case ResultType.RT_INFO:
					{
						//this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0x00, 0x00, 0xff));
					}
					break;
				default:
					break;
			}
		}

		void ResultLink_MouseDown(object sender, MouseButtonEventArgs e)
		{
			s_curRun = this;
			if(m_link != null)
			{
				if(m_link is XmlItem)
				{
					((XmlItem)m_link).changeSelectItem();
				}
				else if (m_link is SkinLinkDef_T)
				{
					SkinLinkDef_T linkDef = (SkinLinkDef_T)m_link;

					XmlControl.changeSelectSkinAndFile(linkDef.m_xmlPath, linkDef.m_skinName);
				}
				else if(m_link is XmlControl)
				{
					MainWindow.s_pW.openFileByPath(((XmlControl)m_link).m_openedFile.m_path);
				}
				else if(m_link is string)
				{
					MainWindow.s_pW.openFileByPath((string)m_link);
				}
			}
		}
	}
}
