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

namespace UIEditor.BoloUI
{
	/// <summary>
	/// SkinEditor.xaml 的交互逻辑
	/// </summary>
	public partial class SkinEditor : Window
	{
		public SkinEditor()
		{
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
		}

		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{

		}
	}
}
