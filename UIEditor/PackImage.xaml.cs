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
	public partial class PackImage : Grid
	{
		public Dictionary<string, System.Drawing.Rectangle> m_mapImgRect;

		static private void getMapImgRect(
			XmlDocument docXml,
			out Dictionary<string, System.Drawing.Rectangle> mapImgRect,
			out int imgWidth,
			out int imgHeight)
		{
			mapImgRect = new Dictionary<string, System.Drawing.Rectangle>();
			int maxX = 0;
			int maxY = 0;

			foreach (XmlNode xn in docXml.DocumentElement.SelectNodes("Image"))
			{
				if (xn.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xn;
					System.Drawing.Rectangle rt;

					if (xe.GetAttribute("Name") != "" && !mapImgRect.TryGetValue(xe.GetAttribute("Name"), out rt))
					{
						int x = int.Parse(xe.GetAttribute("X"));
						int y = int.Parse(xe.GetAttribute("Y"));
						int w = int.Parse(xe.GetAttribute("Width"));
						int h = int.Parse(xe.GetAttribute("Height"));

						//因为差分，所以这么算
						int mx = x + w + 1;
						int my = y + h + 1;

						maxX = maxX > mx ? maxX : mx;
						maxY = maxY > my ? maxY : my;

						mapImgRect.Add(xe.GetAttribute("Name"), new System.Drawing.Rectangle(x, y, w, h));
					}
				}
			}
			int wPow = (int)Math.Ceiling(Math.Log(maxX, 2));
			int hPow = (int)Math.Ceiling(Math.Log(maxY, 2));

			imgWidth = (int)Math.Pow(2, wPow > hPow ? wPow : hPow);
			imgHeight = (int)Math.Pow(2, wPow > hPow ? wPow : hPow);
		}
		static private void refreshImagePack(
			string xmlPath,
			bool isRePack,
			out Dictionary<string, System.Drawing.Rectangle> mapImgRect,
			out System.Drawing.Bitmap tgaImg,
			out int imgWidth,
			out int imgHeight)
		{
			string pngPath = xmlPath.Remove(xmlPath.LastIndexOf("."));
			string tgaPath = pngPath + ".tga";

			if (System.IO.File.Exists(xmlPath))
			{
				XmlDocument docXml = new XmlDocument();

				try
				{
					docXml.Load(xmlPath);
				}
				catch
				{
					mapImgRect = null;
					tgaImg = null;
					imgWidth = 0;
					imgHeight = 0;

					return;
				}
				getMapImgRect(docXml, out mapImgRect, out imgWidth, out imgHeight);

				tgaImg = new System.Drawing.Bitmap(imgWidth, imgHeight);
				System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tgaImg);
				g.Clear(System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00));

				if (isRePack == true)
				{
					foreach (KeyValuePair<string, System.Drawing.Rectangle> pairImgRect in mapImgRect)
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
					DevIL.DevIL.SaveBitmap(tgaPath, tgaImg);
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
			}
			else
			{
				mapImgRect = null;
				tgaImg = null;
				imgWidth = 0;
				imgHeight = 0;
			}
		}
		static public void refreshImagePack(string xmlPath)
		{
			Dictionary<string, System.Drawing.Rectangle> mapImgRect;
			System.Drawing.Bitmap tgaImg;
			int imgWidth;
			int imgHeight;

			refreshImagePack(xmlPath, true, out mapImgRect, out tgaImg, out imgWidth, out imgHeight);
		}

		public PackImage(XmlControl parent, bool isRePack = true)
		{
			InitializeComponent();

			System.Drawing.Bitmap tgaImg;
			BitmapSource imgSource;
			int imgWidth;
			int imgHeight;

			refreshImagePack(parent.m_openedFile.m_path, isRePack, out m_mapImgRect, out tgaImg, out imgWidth, out imgHeight);

			parent.m_parent.itemFrame.Width = imgWidth;
			parent.m_parent.itemFrame.Height = imgHeight;
			mx_canvas.Width = imgWidth;
			mx_canvas.Height = imgHeight;

			if (imgHeight > 4096)
			{
				MainWindow.s_pW.mx_debug.Text += "<警告>图片尺寸过大，不提供预览功能\r\n";
				return;
			}

			IntPtr ip = tgaImg.GetHbitmap();
			imgSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				ip, IntPtr.Zero, Int32Rect.Empty,
				System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
			MainWindow.DeleteObject(ip);

			System.Windows.Controls.Image cImg = new System.Windows.Controls.Image();
			cImg.Source = imgSource;
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
