using System;
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
		RT_ERROR = 0x0000,
		RT_WARNING = 0x0001,
		RT_INFO = 0x0002,
		RT_NONE = 0x0003,
		RT_MAX = 0x0004
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
	public class ResultLink
	{
		static private Paragraph st_curResultFrame;
		static public Paragraph s_curResultFrame
		{
			get { return st_curResultFrame; }
			set
			{
				if (value != null)
				{
					st_curResultFrame = value;
				}
				else
				{
					st_curResultFrame = OpenedFile.s_paraResult;
				}
				MainWindow.s_pW.mx_docResult.Blocks.Clear();
				MainWindow.s_pW.mx_docResult.Blocks.Add(value);
				refreshResultVisibility();
			}
		}
		static private Inline st_curRun;
		static public Inline s_curRun
		{
			get{ return st_curRun; }
			set
			{
				if(st_curRun != null)
				{
					st_curRun.Background = null;
				}
				value.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0x33, 0x99, 0xff));
				st_curRun = value;
			}
		}
		static public BitmapImage s_bmpError = new BitmapImage(new Uri(@".\data\image\error.png", UriKind.Relative));
		static public BitmapImage s_bmpWarning = new BitmapImage(new Uri(@".\data\image\warning.png", UriKind.Relative));
		static public BitmapImage s_bmpLinkInfo = new BitmapImage(new Uri(@".\data\image\linkInfo.png", UriKind.Relative));

		static private void showResultRow(ResultLink rLink)
		{
			switch (rLink.m_rt)
			{
				case ResultType.RT_ERROR:
					{
						if (MainWindow.s_pW.mx_showErrorResult.IsChecked == true && rLink.m_line != null)
						{
							s_curResultFrame.Inlines.Add(rLink.m_line);
						}
					}
					break;
				case ResultType.RT_WARNING:
					{
						if (MainWindow.s_pW.mx_showWarningResult.IsChecked == true && rLink.m_line != null)
						{
							s_curResultFrame.Inlines.Add(rLink.m_line);
						}
					}
					break;
				case ResultType.RT_INFO:
					{
						if (MainWindow.s_pW.mx_showInfoResult.IsChecked == true && rLink.m_line != null)
						{
							s_curResultFrame.Inlines.Add(rLink.m_line);
						}
					}
					break;
				default:
					{
						if (MainWindow.s_pW.mx_showOtherResult.IsChecked == true && rLink.m_line != null)
						{
							s_curResultFrame.Inlines.Add(rLink.m_line);
						}
					}
					break;
			}
		}
		static public void refreshResultVisibility()
		{
			OpenedFile curFileDef = OpenedFile.getCurFileDef();

			if (s_curResultFrame != null)
			{
				s_curResultFrame.Inlines.Clear();
				if (curFileDef != null)
				{
					foreach (ResultLink rLink in curFileDef.m_lstResult)
					{
						showResultRow(rLink);
					}
				}
				foreach (ResultLink rLink in OpenedFile.s_lstResult)
				{
					showResultRow(rLink);
				}
				if (s_curResultFrame.Inlines.Count() > 0 && s_curResultFrame.Inlines.First() != null &&
					s_curResultFrame.Inlines.First() is Run && ((Run)s_curResultFrame.Inlines.First()).Text == "\r\n")
				{
					s_curResultFrame.Inlines.Remove(s_curResultFrame.Inlines.First());
				}
			}
		}
		static public void addLineToCurResultFrame(ResultLink rLink, bool isAlwaysShow = false)
		{
			if(isAlwaysShow == true)
			{
				OpenedFile.s_lstResult.Add(rLink);
			}
			else
			{
				OpenedFile curFileDef = OpenedFile.getCurFileDef();

				if (curFileDef != null && curFileDef.m_lstResult != null)
				{
					curFileDef.m_lstResult.Add(rLink);
				}
				else
				{
					OpenedFile.s_lstResult.Add(rLink);
				}
			}
		}
		static public void createResult(string text, bool isAlwaysShow)
		{
			createResult(text, ResultType.RT_NONE, null, isAlwaysShow);
		}
		static public void createResult(string text, ResultType rt = ResultType.RT_NONE, object link = null, bool isAlwaysShow = false)
		{
			if (text[0] == '\r' && text[1] == '\n')
			{
				text = text.Substring(2);

				ResultLink rLink = new Public.ResultLink("\r\n", rt, link);
				ResultLink rImageLink = new Public.ResultLink(null, rt, link);

				addLineToCurResultFrame(rLink, isAlwaysShow);
				addLineToCurResultFrame(rImageLink, isAlwaysShow);
			}
			ResultLink rTextLink = new Public.ResultLink(text, rt, link);

			addLineToCurResultFrame(rTextLink, isAlwaysShow);
			MainWindow.s_pW.mx_bFrameError.IsSelected = true;
		}

		public ResultType m_rt;
		public Inline m_line;
		public object m_link;

		private ResultLink(string text, ResultType rt = ResultType.RT_NONE, object link = null)
		{
			m_rt = rt;
			if(text != null)
			{
				m_line = new Run();
				((Run)m_line).Text = text;
			}
			else
			{
				if (rt == ResultType.RT_WARNING || rt == ResultType.RT_ERROR || rt == ResultType.RT_INFO)
				{
					Image imgResult = new Image();
					m_line = new InlineUIContainer();

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
						case ResultType.RT_INFO:
							{
								imgResult.Source = s_bmpLinkInfo;
							}
							break;
						default:
							break;
					}
					imgResult.Height = 16;
					imgResult.Width = 16;
					((InlineUIContainer)m_line).Child = imgResult;
				}
			}
			m_link = link;

			if (m_line != null)
			{
				m_line.MouseDown += ResultLink_MouseDown;
				if (m_link != null && m_line is Run)
				{
					Run runLink = (Run)m_line;

					runLink.TextDecorations = TextDecorations.Underline;
					runLink.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x99, 0xcc, 0xff));
					runLink.Cursor = Cursors.Hand;
				}
			}

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
			if(m_line != null)
			{
				s_curRun = m_line;
				if (m_link != null)
				{
					if (m_link is XmlItem)
					{
						((XmlItem)m_link).changeSelectItem();
					}
					else if (m_link is SkinLinkDef_T)
					{
						SkinLinkDef_T linkDef = (SkinLinkDef_T)m_link;

						XmlControl.changeSelectSkinAndFile(linkDef.m_xmlPath, linkDef.m_skinName);
					}
					else if (m_link is XmlControl)
					{
						MainWindow.s_pW.openFileByPath(((XmlControl)m_link).m_openedFile.m_path);
					}
					else if (m_link is string)
					{
						MainWindow.s_pW.openFileByPath((string)m_link);
					}
				}
			}
		}
	}
}
