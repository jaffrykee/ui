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
	/// PngControl.xaml 的交互逻辑
	/// </summary>
	public partial class PngControl : UserControl
	{
		public FileTabItem m_parent;
		public OpenedFile m_openedFile;
		public System.Drawing.Bitmap m_Bitmap;
		public BitmapSource m_imgSource;
		public int m_imgHeight;
		public int m_imgWidth;

		public PngControl(FileTabItem parent, OpenedFile fileDef)
		{
			InitializeComponent();
			m_parent = parent;
			m_openedFile = fileDef;
			m_openedFile.m_frame = this;

			string path = this.m_parent.m_filePath;

			if(System.IO.File.Exists(path))
			{
				m_Bitmap = DevIL.DevIL.LoadBitmap(path);
				IntPtr ip = m_Bitmap.GetHbitmap();
				m_imgSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					ip, IntPtr.Zero, Int32Rect.Empty,
					System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
				MainWindow.DeleteObject(ip);

				m_imgHeight = m_imgSource.PixelHeight;
				m_imgWidth = m_imgSource.PixelWidth;
				m_parent.itemFrame.Height = m_imgHeight;
				m_parent.itemFrame.Width = m_imgWidth;
				mx_image.Source = m_imgSource;
				mx_image.Stretch = Stretch.Uniform;
			}
		}
		public PngControl(string path)
		{
			InitializeComponent();
			m_parent = null;
			m_openedFile = null;

			if (System.IO.File.Exists(path))
			{
				m_Bitmap = DevIL.DevIL.LoadBitmap(path);
				IntPtr ip = m_Bitmap.GetHbitmap();
				m_imgSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					ip, IntPtr.Zero, Int32Rect.Empty,
					System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
				MainWindow.DeleteObject(ip);

				m_imgHeight = m_imgSource.PixelHeight;
				m_imgWidth = m_imgSource.PixelWidth;
				mx_image.Source = m_imgSource;
				mx_image.Stretch = Stretch.Uniform;
			}
		}

		private void mx_imageLoaded(object sender, RoutedEventArgs e)
		{
		}
		private void mx_image_Unloaded(object sender, RoutedEventArgs e)
		{
		}
	}
}
