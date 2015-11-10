using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UIEditor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
		private void mx_app_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			string strInfo = e.Exception.ToString();

			if(UIEditor.MainWindow.s_pW != null)
			{
				//strInfo += "\r\n调试记录:\r\n" + UIEditor.MainWindow.s_pW.mx_debug.Text + "\r\n";
			}
			Public.ErrorInfo winError = new Public.ErrorInfo(strInfo);
			winError.ShowDialog();
		}
	}
}
