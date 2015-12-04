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

namespace UIEditor.BoloUI
{
	/// <summary>
	/// AnimationKeyFrame.xaml 的交互逻辑
	/// </summary>
	public partial class AnimationTimeScaleTips : UserControl
	{
		public static readonly DependencyProperty md_curTimeScale =
		   DependencyProperty.Register("ma_curTimeScale", typeof(double), typeof(AnimationKeyFrame), new PropertyMetadata(0.0d));
		public double ma_curTimeScale
		{
			get { return (double)GetValue(md_curTimeScale); }
			set
			{
				SetValue(md_curTimeScale, value);
				resetPosition();
				mx_blockTimeScaleText.Text = (ma_curTimeScale * ma_maxTimeScale / 100.0d).ToString();
			}
			
		}

		public static readonly DependencyProperty md_maxTimeScale =
		   DependencyProperty.Register("ma_maxTimeScale", typeof(double), typeof(AnimationKeyFrame), new PropertyMetadata(10000.0d));
		public double ma_maxTimeScale
		{
			get { return (double)GetValue(md_maxTimeScale); }
			set
			{
				SetValue(md_maxTimeScale, value);
				mx_blockTimeScaleText.Text = (ma_curTimeScale * ma_maxTimeScale / 100.0d).ToString();
			}
		}

		public AnimationTimeScaleTips()
		{
			InitializeComponent();
		}

		private void resetPosition()
		{
			this.Margin = new Thickness(ma_curTimeScale * 100.0d - mx_blockTimeScaleText.ActualWidth / 2 + 5, 0, 0, 0);
		}
		private void mx_blockTimeScaleText_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			resetPosition();
		}
	}
}
