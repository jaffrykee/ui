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
using System.Globalization;
using System.Net;

namespace UIEditor.Public
{
	public partial class ErrorInfo
	{
		public static string s_errorInfo = "";
		public const string conf_errorInfoPath = @".\errorInfo\";

		static public string GetAddressIP()
		{
			///获取本地的IP地址
			string strIp = string.Empty;

			foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
				{
					strIp = _IPAddress.ToString();
					if(strIp.IndexOf("10.0.") >= 0)
					{
						strIp = strIp.Replace("10.0.", "");
						strIp = strIp.Replace(".", "-");

						return strIp;
					}
				}
			}

			return "";
		}

		public ErrorInfo(string errorInfo)
		{
			DateTime date = DateTime.Now;
			string fileName = GetAddressIP() + date.ToString("-yyyyMMdd-HHmmss", DateTimeFormatInfo.InvariantInfo) + ".log";
			
			InitializeComponent();

			mx_errorInfo.Text = errorInfo + "\r\n\r\n错误报告已经存放于：\"" + conf_errorInfoPath + fileName +
				"\"文件中。\r\n\r\n在工程目录： " + Project.Setting.s_projPath + " 已经生成已打开文件的当前状态备份：";
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

			StreamWriter SW;

			SW = File.CreateText(conf_errorInfoPath + fileName);
			SW.WriteLine(mx_errorInfo.Text);
			SW.Close();
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
