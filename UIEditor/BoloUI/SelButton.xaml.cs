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

namespace UIEditor.BoloUI
{
	/// <summary>
	/// SelButton.xaml 的交互逻辑
	/// </summary>
	public partial class SelButton : Grid
	{
		public MainWindow m_pW;
		public Basic m_ctrlUI;

		public SelButton(MainWindow pW, Basic ctrl)
		{
			InitializeComponent();
			m_pW = pW;
			m_ctrlUI = ctrl;
		}

		private void mx_radio_Checked(object sender, RoutedEventArgs e)
		{
			m_ctrlUI.changeSelectItem();
		}
	}
}
