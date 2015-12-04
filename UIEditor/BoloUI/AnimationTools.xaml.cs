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
using UIEditor.Public;

namespace UIEditor.BoloUI
{
	/// <summary>
	/// AnimationTools.xaml 的交互逻辑
	/// </summary>
	public partial class AnimationTools : UserControl
	{
		Dictionary<AnimationKeyFrame, XmlElement> m_mapFrameXe;
		List<AnimationTimeScaleTips> m_lstTimeScaleTips;
		public AnimationTools()
		{
			m_maxValueLock = new EventLock();
			m_mapFrameXe = new Dictionary<AnimationKeyFrame, XmlElement>();
			m_lstTimeScaleTips = new List<AnimationTimeScaleTips>();

			InitializeComponent();
			for (int i = 0; i < 100; i++)
			{
				AnimationTimeScaleTips aniTips = new AnimationTimeScaleTips();

				aniTips.ma_maxTimeScale = m_maxValue;
				aniTips.ma_curTimeScale = (double)i;
				m_lstTimeScaleTips.Add(aniTips);
				mx_timeScaleTipsFrame.Children.Add(aniTips);
			}
		}
		private EventLock m_maxValueLock;
		private double mt_maxValue;
		public double m_maxValue
		{
			get { return mt_maxValue; }
			set
			{
				bool stackLock;
				if (m_maxValueLock.isLock())
				{
					return;
				}
				else
				{
					m_maxValueLock.addLock(out stackLock);
				}

				if (value > 102400)
				{
					value = 102400;
				}
				if (value < 1)
				{
					value = 1;
				}
				if (mt_maxValue != value)
				{
					foreach (KeyValuePair<AnimationKeyFrame, XmlElement> pairFrameXe in m_mapFrameXe.ToList())
					{
						pairFrameXe.Key.ma_maxValue = value;
					}
					foreach (AnimationTimeScaleTips aniTips in m_lstTimeScaleTips)
					{
						aniTips.ma_maxTimeScale = value;
					}
					mx_slider.Maximum = value;
					mx_tbMaxValue.Text = value.ToString();
					mt_maxValue = value;
				}

				m_maxValueLock.delLock(ref stackLock);
			}
		}
		private double mt_curValue;
		public double m_curValue
		{
			get
			{
				double curValue;

				if (double.TryParse(mx_tbValue.Text, out curValue))
				{
					mt_curValue = curValue;
				}

				return mt_curValue;
			}
			set
			{
				if (mx_tbValue.Text != value.ToString("F2"))
				{
					mx_tbValue.Text = value.ToString("F2");
				}
				mt_curValue = value;
			}
		}

		private void mx_slider_Loaded(object sender, RoutedEventArgs e)
		{
			mx_slider.Maximum = 100;
		}
		private void mx_tbMaxValue_TextChanged(object sender, TextChangedEventArgs e)
		{
			double maxValue;

			if (double.TryParse(mx_tbMaxValue.Text, out maxValue))
			{
				m_maxValue = maxValue;
			}
		}
		private void mx_tbValue_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Keyboard.FocusedElement != sender)
			{
				double curValue;

				if (double.TryParse(mx_tbValue.Text, out curValue))
				{
					m_curValue = curValue;
				}
			}
		}

		public void createKeyFrame(XmlElement xeFrame)
		{
			AnimationKeyFrame aniFrame = new AnimationKeyFrame();

			aniFrame.ma_maxValue = m_maxValue;
			aniFrame.ma_curValue = m_curValue;
			m_mapFrameXe.Add(aniFrame, null);
			mx_timeButtonFrame.Children.Add(aniFrame);
		}
		private void mx_createKeyFrame_Click(object sender, RoutedEventArgs e)
		{
			createKeyFrame(null);
		}
		private void mx_plusMax_Click(object sender, RoutedEventArgs e)
		{
			m_maxValue *= 2;
		}
		private void mx_minusMax_Click(object sender, RoutedEventArgs e)
		{
			m_maxValue /= 2;
		}
	}
}
