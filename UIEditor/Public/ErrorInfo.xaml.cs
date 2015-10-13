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

namespace UIEditor.Public
{
	public partial class ErrorInfo
	{
		public static string s_errorInfo = "";

		public ErrorInfo(string errorInfo)
		{
			InitializeComponent();
			mx_errorInfo.Text = errorInfo;
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
