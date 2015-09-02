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
using System.Xml;

namespace UIEditor
{
	/// <summary>
	/// PackImage.xaml 的交互逻辑
	/// </summary>
	public partial class PackImage : Grid
	{
		public XmlControl m_parent;
		public System.Drawing.Bitmap m_Bitmap;
		public System.Drawing.Bitmap m_tgaImg;
		public BitmapSource m_imgSource;
		public int m_imgHeight;
		public int m_imgWidth;
		public bool m_loaded;
		public Dictionary<string, System.Drawing.Rectangle> m_mapImgRect;
		public int m_maxX;
		public int m_maxY;

		public PackImage(XmlControl parent, bool isRePack = true)
		{
			InitializeComponent();
			m_parent = parent;
			m_loaded = false;
			m_mapImgRect = new Dictionary<string, System.Drawing.Rectangle>();
			m_maxX = 0;
			m_maxY = 0;

			foreach (XmlNode xn in m_parent.m_xeRoot.SelectNodes("Image"))
			{
				if (xn.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xn;
					System.Drawing.Rectangle rt;

					if (xe.GetAttribute("Name") != "" && !m_mapImgRect.TryGetValue(xe.GetAttribute("Name"), out rt))
					{
						int x = int.Parse(xe.GetAttribute("X"));
						int y = int.Parse(xe.GetAttribute("Y"));
						int w = int.Parse(xe.GetAttribute("Width"));
						int h = int.Parse(xe.GetAttribute("Height"));

						//因为差分，所以这么算
						int mx = x + w + 1;
						int my = y + h + 1;

						m_maxX = m_maxX > mx ? m_maxX : mx;
						m_maxY = m_maxY > my ? m_maxY : my;

						m_mapImgRect.Add(xe.GetAttribute("Name"), new System.Drawing.Rectangle(x, y, w, h));
					}
				}
			}

			int wPow = (int)Math.Ceiling(Math.Log(m_maxX, 2));
			int hPow = (int)Math.Ceiling(Math.Log(m_maxY, 2));

			m_imgWidth = (int)Math.Pow(2, wPow > hPow ? wPow : hPow);
			m_imgHeight = (int)Math.Pow(2, wPow > hPow ? wPow : hPow);

			IntPtr ip;

			m_parent.m_parent.itemFrame.Width = m_imgWidth;
			m_parent.m_parent.itemFrame.Height = m_imgHeight;
			mx_canvas.Width = m_imgWidth;
			mx_canvas.Height = m_imgHeight;

			string pngPath = m_parent.m_openedFile.m_path.Remove(m_parent.m_openedFile.m_path.LastIndexOf("."));
			string tgaPath = pngPath + ".tga";

			m_tgaImg = new System.Drawing.Bitmap(m_imgWidth, m_imgHeight);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(m_tgaImg);
			g.Clear(System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00));

			if(isRePack)
			{
				foreach (KeyValuePair<string, System.Drawing.Rectangle> pairImgRect in m_mapImgRect)
				{
					addPicToGraphics(
						pngPath + "\\" + pairImgRect.Key + ".png",
						pairImgRect.Value,
						g);
				}
				g.Dispose();
				if (System.IO.File.Exists(tgaPath))
				{
					System.IO.File.Delete(tgaPath);
				}
				DevIL.DevIL.SaveBitmap(tgaPath, m_tgaImg);
				if (m_imgHeight > 4096)
				{
					MainWindow.s_pW.mx_debug.Text += "<警告>图片尺寸过大，不提供预览功能\r\n";
					return;
				}
			}
			else
			{
				System.Drawing.Bitmap bmp = DevIL.DevIL.LoadBitmap(tgaPath);
				g.DrawImage(bmp,
					0,
					0,
					bmp.Width,
					bmp.Height);
				g.Dispose();
			}

			ip = m_tgaImg.GetHbitmap();
			m_imgSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				ip, IntPtr.Zero, Int32Rect.Empty,
				System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
			MainWindow.DeleteObject(ip);

			System.Windows.Controls.Image cImg = new System.Windows.Controls.Image();
			cImg.Source = m_imgSource;
			cImg.Stretch = Stretch.Uniform;
			mx_canvas.Children.Insert(0, cImg);
		}
		public static bool addPicToGraphics(string path, System.Drawing.Rectangle rect, System.Drawing.Graphics g)
		{
			if (System.IO.File.Exists(path))
			{
				//拼图和延展1像素
				System.Drawing.Bitmap bmp = DevIL.DevIL.LoadBitmap(path);
				g.DrawImage(bmp,
					rect.X,
					rect.Y,
					rect.Width,
					rect.Height);
				g.DrawImage(bmp,
					new System.Drawing.Rectangle(rect.X - 1, rect.Y, 1, rect.Height),
					new System.Drawing.Rectangle(0, 0, 1, rect.Height),
					System.Drawing.GraphicsUnit.Pixel);
				g.DrawImage(bmp,
					new System.Drawing.Rectangle(rect.X, rect.Y - 1, rect.Width, 1),
					new System.Drawing.Rectangle(0, 0, rect.Width, 1),
					System.Drawing.GraphicsUnit.Pixel);
				g.DrawImage(bmp,
					new System.Drawing.Rectangle(rect.X + rect.Width, rect.Y, 1, rect.Height),
					new System.Drawing.Rectangle(rect.Width - 1, 0, 1, rect.Height),
					System.Drawing.GraphicsUnit.Pixel);
				g.DrawImage(bmp,
					new System.Drawing.Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 1),
					new System.Drawing.Rectangle(0, rect.Height - 1, rect.Width, 1),
					System.Drawing.GraphicsUnit.Pixel);

				return true;
			}
			else
			{
				return false;
			}
		}
		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private void mx_canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			int x = (int)Math.Round(e.GetPosition(mx_canvas).X);
			int y = (int)Math.Round(e.GetPosition(mx_canvas).Y);

			MainWindow.s_pW.mx_debug.Text += "<坐标>(" + e.GetPosition(mx_canvas).X.ToString() + "," + e.GetPosition(mx_canvas).Y.ToString() + ")\r\n";
			foreach (KeyValuePair<string, System.Drawing.Rectangle> pairImgRect in m_mapImgRect.ToList())
			{
				if(pairImgRect.Value.Contains(x, y))
				{
					mx_selPath.Visibility = System.Windows.Visibility.Visible;
					mx_selPath.Data = new RectangleGeometry(new Rect(
						pairImgRect.Value.X,
						pairImgRect.Value.Y,
						pairImgRect.Value.Width,
						pairImgRect.Value.Height
					));
					MainWindow.s_pW.mx_debug.Text += "<图片>Name:" + pairImgRect.Key + "\r\n";

					return;
				}
			}
			mx_selPath.Visibility = System.Windows.Visibility.Collapsed;
		}
		private void mx_canvas_MouseMove(object sender, MouseEventArgs e)
		{
			int x = (int)Math.Round(e.GetPosition(mx_canvas).X);
			int y = (int)Math.Round(e.GetPosition(mx_canvas).Y);

			foreach (KeyValuePair<string, System.Drawing.Rectangle> pairImgRect in m_mapImgRect.ToList())
			{
				if (pairImgRect.Value.Contains(x, y))
				{
					mx_overPath.Visibility = System.Windows.Visibility.Visible;
					mx_overPath.Data = new RectangleGeometry(
						new Rect(
							pairImgRect.Value.X,
							pairImgRect.Value.Y,
							pairImgRect.Value.Width,
							pairImgRect.Value.Height
						)
					);

					return;
				}
			}
			mx_overPath.Visibility = System.Windows.Visibility.Collapsed;
		}
	}
}
