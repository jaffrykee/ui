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
using System.Xml;

namespace UIEditor.BoloUI
{
	/// <summary>
	/// AnimationKeyFrame.xaml 的交互逻辑
	/// </summary>
	public partial class AnimationKeyFrame : UserControl
	{
		#region Control Property
		public static readonly DependencyProperty md_curValue =
			DependencyProperty.Register("ma_curValue", typeof(double), typeof(AnimationKeyFrame), new PropertyMetadata(0.0d));
		public double ma_curValue
		{
			get { return (double)GetValue(md_curValue); }
			set
			{
				SetValue(md_curValue, value);
				this.Margin = new Thickness(ma_curValue / ma_maxValue * 10000.0d, 0, 0, 0);
			}
			
		}

		public static readonly DependencyProperty md_maxValue =
			DependencyProperty.Register("ma_maxValue", typeof(double), typeof(AnimationKeyFrame), new PropertyMetadata(10000.0d));
		public double ma_maxValue
		{
			get { return (double)GetValue(md_maxValue); }
			set
			{
				SetValue(md_maxValue, value);
				this.Margin = new Thickness(ma_curValue / ma_maxValue * 10000.0d, 0, 0, 0);
			}
		}
		#endregion

		public XmlElement m_xe;

		public AnimationKeyFrame(XmlElement xe = null)
		{
			m_xe = xe;
			InitializeComponent();
		}

		private void mx_rbKeyFrame_Checked(object sender, RoutedEventArgs e)
		{
			mx_rbKeyFrame.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0xff, 0x00, 0x00));
		}
		private void mx_rbKeyFrame_Unchecked(object sender, RoutedEventArgs e)
		{
			mx_rbKeyFrame.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x00, 0x00, 0xff));
		}
	}
}
