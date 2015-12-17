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
using System.Windows.Shapes;
using System.IO;

namespace UIEditor.Public
{
	public partial class ErrorInfo
	{
		public static string s_errorInfo = "";

		public ErrorInfo(string errorInfo)
		{
			InitializeComponent();
			mx_errorInfo.Text = errorInfo + "\r\n\r\n在工程目录： " + Project.Setting.s_projPath + " 已经生成已打开文件的当前状态备份：";
			foreach (KeyValuePair<string, OpenedFile> pairFileDef in MainWindow.s_pW.m_mapOpenedFiles.ToList())
			{
				if (File.Exists(pairFileDef.Key))
				{
					FileInfo fi = new FileInfo(pairFileDef.Key);

					if (pairFileDef.Value.m_frame is XmlControl)
					{
						XmlControl xmlCtrl = (XmlControl)pairFileDef.Value.m_frame;

						xmlCtrl.m_xmlDoc.Save(pairFileDef.Key + ".backup");
						mx_errorInfo.Text += "\r\n" + pairFileDef.Key + ".backup";
					}
				}
			}
		}

		public static void clearErrorInfo()
		{
			s_errorInfo = "";
		}
		public static void addToErrorInfo(string str)
		{
			if (s_errorInfo == null)
			{
				s_errorInfo = str;
			}
			else
			{
				s_errorInfo += str;
			}
		}
		public static void showErrorInfo()
		{
			if(s_errorInfo != null && s_errorInfo != "")
			{
				MessageBox.Show(s_errorInfo, "错误信息", MessageBoxButton.OK, MessageBoxImage.Error);
				clearErrorInfo();
			}
		}
	}
}
