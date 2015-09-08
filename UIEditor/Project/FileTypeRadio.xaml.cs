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

namespace UIEditor.Project
{
	/// <summary>
	/// FileTypeRadio.xaml 的交互逻辑
	/// </summary>
	public partial class FileTypeRadio : Grid
	{
		NewFileWin m_frame;

		public FileTypeRadio(NewFileWin frame, string text, string groupName)
		{
			m_frame = frame;
			InitializeComponent();
			mx_radio.Content = text;
			mx_radio.GroupName = groupName;
		}
		private void mx_radio_Checked(object sender, RoutedEventArgs e)
		{
			m_frame.m_curFileType = this;
			if(m_frame.mx_projPath.Text != "" || m_frame.mx_fileName.Text != "")
			{
				m_frame.mx_ok.IsEnabled = true;
			}
		}
	}
}
