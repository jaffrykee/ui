﻿using System;
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
using UIEditor.Project;

namespace UIEditor
{
	public partial class PackImage : Grid
	{
		public Dictionary<string, System.Drawing.Rectangle> m_mapImgRect;
		public string m_xmlPath;
		public int m_imageWidth;
		public int m_imageHeight;

		private int mt_poiX;
		public int m_poiX
		{
			get { return mt_poiX; }
			set
			{
				mt_poiX = value;
				refreshWinStatus();
			}
		}
		private int mt_poiY;
		public int m_poiY
		{
			get { return mt_poiY; }
			set
			{
				mt_poiY = value;
				refreshWinStatus();
			}
		}
		public string getPngFullPath(string pngName)
		{
			string pngPath = m_xmlPath.Remove(m_xmlPath.IndexOf(".")) + "\\" + pngName + ".png";

			return pngPath;
		}
		public int getCurPngWidth()
		{
			System.Drawing.Rectangle pngArea;

			if (m_curPngName != null && m_curPngName != "" && m_mapImgRect.TryGetValue(m_curPngName, out pngArea) && pngArea != null)
			{
				return pngArea.Width;
			}

			return 0;
		}
		public int getCurPngHeight()
		{
			System.Drawing.Rectangle pngArea;

			if (m_curPngName != null && m_curPngName != "" && m_mapImgRect.TryGetValue(m_curPngName, out pngArea) && pngArea != null)
			{
				return pngArea.Height;
			}

			return 0;
		}
		private string mt_curPngName;
		public string m_curPngName
		{
			get { return mt_curPngName; }
			set
			{
				mt_curPngName = value;

				System.Drawing.Rectangle pngArea;

				if (value != null && value != "" && m_mapImgRect.TryGetValue(value, out pngArea) && pngArea != null)
				{
					mx_selPath.Visibility = System.Windows.Visibility.Visible;
					mx_selPath.Data = new RectangleGeometry(new Rect(
						pngArea.X,
						pngArea.Y,
						pngArea.Width,
						pngArea.Height
					));

					MainWindow.s_pW.mb_status3 = ("png图片显示范围：( " + pngArea.X.ToString() + " , " + pngArea.Y.ToString() + " ) " +
						pngArea.Width.ToString() + " x " + pngArea.Height.ToString());

					if (BoloUI.SelImage.s_pW != null)
					{
						TreeViewItem curPngItem;

						if (BoloUI.SelImage.s_pW.m_mapPngItem.TryGetValue(getPngFullPath(value), out curPngItem) && curPngItem != null)
						{
							curPngItem.IsSelected = true;
							curPngItem.BringIntoView();
						}
					}
				}
				refreshWinStatus();
			}
		}

		private void refreshWinStatus()
		{
			MainWindow.s_pW.mb_status0 = "( " + m_poiX + " , " + m_poiY + " )\t图片总尺寸： " + m_imageWidth + " x " + m_imageHeight;
			MainWindow.s_pW.mb_status1 = "";
			string pngPath = Project.Setting.s_imagePath + "\\" + System.IO.Path.GetFileNameWithoutExtension(m_xmlPath)
				+ "\\" + m_curPngName + ".png";

			if(System.IO.File.Exists(pngPath))
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(pngPath);
			}
			MainWindow.s_pW.mb_status2 = pngPath;
		}
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
		static public bool createSSImage(string srcImagePath, string dstFolderPath)
		{
			if (System.IO.File.Exists(srcImagePath) && System.IO.Directory.Exists(dstFolderPath))
			{
				try
				{
					System.Drawing.Rectangle dstSize = new System.Drawing.Rectangle(0, 0, 2048, 2048);
					System.Drawing.Bitmap tgaImg = new System.Drawing.Bitmap(dstSize.Width, dstSize.Width);
					System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tgaImg);
					System.Drawing.Bitmap bmp = DevIL.DevIL.LoadBitmap(srcImagePath);

					g.Clear(System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00));
					g.DrawImage(bmp, dstSize.X, dstSize.Y, dstSize.Width, dstSize.Height);
					g.Dispose();
					DevIL.DevIL.SaveBitmap(dstFolderPath + "\\bg_ss.tga", tgaImg);

					return true;
				}
				catch
				{
					Public.ResultLink.createResult("\r\n背景图片转换错误。", Public.ResultType.RT_ERROR, null, true);

					return false;
				}
			}

			return false;
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

				IncludeFile imageRootFolder = IncludeFile.getImageRootFolder();

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

					IncludeFile tgaFileDef;

					if (!MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(tgaPath, out tgaFileDef))
					{
						imageRootFolder.Items.Add(new IncludeFile(tgaPath));
					}

					IncludeFile xmlFileDef;

					if (!MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(xmlPath, out xmlFileDef))
					{
						imageRootFolder.Items.Add(new IncludeFile(xmlPath));
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

			OpenedFile fileDef;

			if(MainWindow.s_pW.m_mapOpenedFiles.TryGetValue(xmlPath, out fileDef))
			{
				fileDef.m_tabItem.closeFile();
				MainWindow.s_pW.openFileByPath(xmlPath);
			}
		}

		public PackImage(string xmlPath, bool isRePack = true)
		{
			m_xmlPath = xmlPath;

			InitializeComponent();

			System.Drawing.Bitmap tgaImg;
			BitmapSource imgSource;

			if (!System.IO.File.Exists(xmlPath))
			{
				Public.ResultLink.createResult("\r\n" + xmlPath + "图包配置文件没有找到（资源缺失）。", Public.ResultType.RT_ERROR);

				return;
			}

			refreshImagePack(xmlPath, isRePack, out m_mapImgRect, out tgaImg, out m_imageWidth, out m_imageHeight);

			mx_canvas.Width = m_imageWidth;
			mx_canvas.Height = m_imageHeight;

			if (m_imageHeight > 4096)
			{
				Public.ResultLink.createResult("\r\n图片尺寸过大，不提供预览功能", Public.ResultType.RT_WARNING);

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
		public PackImage(XmlControl parent, bool isRePack = true)
		{
			if (parent == null || parent.m_openedFile == null || !System.IO.File.Exists(parent.m_openedFile.m_path))
			{
				return;
			}
			m_xmlPath = parent.m_openedFile.m_path;

			InitializeComponent();

			System.Drawing.Bitmap tgaImg;
			BitmapSource imgSource;

			refreshImagePack(m_xmlPath, isRePack, out m_mapImgRect, out tgaImg, out m_imageWidth, out m_imageHeight);

			if (parent.m_parent != null)
			{
				parent.m_parent.itemFrame.Width = m_imageWidth;
				parent.m_parent.itemFrame.Height = m_imageHeight;
			}
			mx_canvas.Width = m_imageWidth;
			mx_canvas.Height = m_imageHeight;

			if (m_imageHeight > 4096)
			{
				Public.ResultLink.createResult("\r\n图片尺寸过大，不提供预览功能", Public.ResultType.RT_WARNING);

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

		private void mouseDownEvent(MouseButtonEventArgs e)
		{
			m_poiX = (int)Math.Round(e.GetPosition(mx_canvas).X);
			m_poiY = (int)Math.Round(e.GetPosition(mx_canvas).Y);

			foreach (KeyValuePair<string, System.Drawing.Rectangle> pairImgRect in m_mapImgRect.ToList())
			{
				if (pairImgRect.Value.Contains(m_poiX, m_poiY))
				{
					if (e.ChangedButton == MouseButton.Right)
					{
						string pngPath = getPngFullPath(m_curPngName);
						IncludeFile pngFileDef;

						if (MainWindow.s_pW.m_mapIncludeFiles != null &&
							MainWindow.s_pW.m_mapIncludeFiles.TryGetValue(pngPath, out pngFileDef))
						{
							pngFileDef.mx_menu.PlacementTarget = mx_canvas;
							pngFileDef.mx_menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
							pngFileDef.mx_menu.IsOpen = true;
						}
					}
					m_curPngName = pairImgRect.Key;

					return;
				}
			}
			m_curPngName = "";
			mx_selPath.Visibility = System.Windows.Visibility.Collapsed;
		}
		private void mx_canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mouseDownEvent(e);
		}
		private void mx_canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			mouseDownEvent(e);
			e.Handled = true;
		}
		private void mx_canvas_MouseMove(object sender, MouseEventArgs e)
		{
			m_poiX = (int)Math.Round(e.GetPosition(mx_canvas).X);
			m_poiY = (int)Math.Round(e.GetPosition(mx_canvas).Y);

			foreach (KeyValuePair<string, System.Drawing.Rectangle> pairImgRect in m_mapImgRect.ToList())
			{
				if (pairImgRect.Value.Contains(m_poiX, m_poiY))
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
