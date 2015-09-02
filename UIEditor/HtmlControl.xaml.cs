using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UIEditor
{
	/// <summary>
	/// HtmControl.xaml 的交互逻辑
	/// </summary>
	public partial class HtmlControl : UserControl
	{
		public FileTabItem m_tabItem;
		public OpenedFile m_openedFile;

		public HtmlControl(FileTabItem tabItem, OpenedFile fileDef)
		{
			m_tabItem = tabItem;
			m_openedFile = fileDef;
			InitializeComponent();
			//mx_browser.Source = new Uri(tabItem.m_filePath, UriKind.Relative);
			string path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);

			mx_browser.Source = new Uri("file:///" + path + "\\" + m_tabItem.m_filePath);
		}
	}
}
