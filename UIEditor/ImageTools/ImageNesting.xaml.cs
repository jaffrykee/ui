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
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.Collections;
using System.Windows.Threading;
using System.Threading;

namespace UIEditor.ImageTools
{
	public class RectNode
	{
		public System.Drawing.Rectangle m_rect;
		public bool m_isAdd;
		public int m_packCount;

		public RectNode(System.Drawing.Rectangle rect, bool isAdd = false)
		{
			m_rect = rect;
			m_isAdd = false;
			m_packCount = 0;
		}
	}

	public partial class ImageNesting : Window
	{
		public static int s_fileCount;

		public string m_path;
		public string m_filter;
		public int m_deep;

		public static ImageNesting s_pW;

		public const int conf_maxSizeOfPreset = 2048;

		public ImageNesting(string path, string filter = "*.png", int deep = 0)
		{
			m_path = path;
			m_filter = filter;
			m_deep = deep;
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			s_pW = this;
		}
		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{
			s_pW = this;
		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			s_pW = null;
		}

		private static int getMaxHeight(List<RectNode> lstRectNode)
		{
			int maxHeight = 0;

			foreach (RectNode node in lstRectNode)
			{
				if (node.m_rect.Height > maxHeight)
				{
					maxHeight = node.m_rect.Height;
				}
			}

			return maxHeight;
		}
		private static int getMaxWidth(List<RectNode> lstRectNode)
		{
			int maxWidth = 0;

			foreach (RectNode node in lstRectNode)
			{
				if (node.m_rect.Width > maxWidth)
				{
					maxWidth = node.m_rect.Width;
				}
			}

			return maxWidth;
		}
		private static int getAreaSum(List<RectNode> lstRectNode)
		{
			int area = 0;

			foreach (RectNode node in lstRectNode)
			{
				area += node.m_rect.Width * node.m_rect.Height;
			}

			return area;
		}
		private static int getMaxPow(List<RectNode> lstRectNode)
		{
			double maxPow = 0;
			double hPow = Math.Log(getMaxHeight(lstRectNode), 2);
			double wPow = Math.Log(getMaxWidth(lstRectNode), 2);
			double areaPow = Math.Log(Math.Pow(getAreaSum(lstRectNode), 0.5), 2);

			if (hPow > wPow)
			{
				maxPow = hPow;
			}
			else
			{
				maxPow = wPow;
			}
			if (areaPow > maxPow)
			{
				maxPow = areaPow;
			}

			return (int)Math.Ceiling(maxPow);
		}

		private static void printString(string str, bool delLast = false)
		{
			TextBox tbDebug;

			if (ImageNesting.s_pW != null)
			{
				tbDebug = ImageNesting.s_pW.mx_imgDebug;
			}
			else
			{
				tbDebug = MainWindow.s_pW.mx_debug;
			}
			if (delLast)
			{
				tbDebug.Text = tbDebug.Text.Remove(tbDebug.Text.LastIndexOf("\r\n"));
			}
			tbDebug.Text += str;
		}
		private static int addFileToArr(string basicPath, string filter, Dictionary<string, RectNode> mapRectNode)
		{
			DirectoryInfo di = new DirectoryInfo(basicPath);
			FileInfo[] arrFileInfo = di.GetFiles(filter);
			int retCount = arrFileInfo.Count();

			foreach (FileInfo fi in arrFileInfo)
			{
				System.Drawing.Image img = System.Drawing.Image.FromFile(fi.FullName);
				//因为差值什么的，所以要+2。
				RectNode rn = new RectNode(new System.Drawing.Rectangle(0, 0, img.Width + 2, img.Height + 2), false);

				mapRectNode.Add(fi.Name, rn);
			}

			return retCount;
		}
		private static void addFileToDirMap(string rootPath, Dictionary<string, string> mapNameDir)
		{
			DirectoryInfo di = new DirectoryInfo(rootPath);
			DirectoryInfo[] arrDirInfo = di.GetDirectories();

			foreach (DirectoryInfo dri in arrDirInfo)
			{
				FileInfo[] arrPngInfo = dri.GetFiles("*.png");

				foreach (FileInfo fi in arrPngInfo)
				{
					mapNameDir.Add(System.IO.Path.GetFileNameWithoutExtension(fi.Name), dri.Name);
				}
			}
		}
		private static void crossInsertToGrid(ArrayList mapGrid, int i0, int j0, int di, int dj, int dw, int dh)
		{
			//十字插入
			//左上描黑，di和dj最低可以为0，也就是只占了一个、一列或一行格子的情况，这种情况不需要描黑。
			if (dw == 0)
			{
				di++;
			}
			if (dh == 0)
			{
				dj++;
			}
			for (int i = 0; i < di; i++)
			{
				for (int j = 0; j < dj; j++)
				{
					RectNode node = ((RectNode)(((ArrayList)mapGrid[i0 + i])[j0 + j]));
					node.m_isAdd = true;
				}
			}

			if (dw > 0)
			{
				//纵向插入
				ArrayList newArr = new ArrayList();
				mapGrid.Insert(i0 + di + 1, newArr);
				for (int j = 0; j < ((ArrayList)mapGrid[i0 + di]).Count; j++)
				{
					RectNode node = ((RectNode)(((ArrayList)mapGrid[i0 + di])[j]));
					RectNode newNode = new RectNode(
						new System.Drawing.Rectangle(
							node.m_rect.X + dw,
							node.m_rect.Y,
							node.m_rect.Width - dw,
							node.m_rect.Height),
						false);

					node.m_rect.Width = dw;
					if (j >= j0 && j <= j0 + dj)
					{
						node.m_isAdd = true;
						newNode.m_isAdd = false;
					}
					else
					{
						newNode.m_isAdd = node.m_isAdd;
					}
					newArr.Add(newNode);
				}
			}

			if (dh > 0)
			{
				//横向插入
				for (int i = 0; i < mapGrid.Count; i++)
				{
					RectNode node = ((RectNode)(((ArrayList)mapGrid[i])[j0 + dj]));
					RectNode newNode = new RectNode(
						new System.Drawing.Rectangle(
							node.m_rect.X,
							node.m_rect.Y + dh,
							node.m_rect.Width,
							node.m_rect.Height - dh),
						false);

					node.m_rect.Height = dh;
					if (i >= i0 && i <= i0 + di)
					{
						node.m_isAdd = true;
						newNode.m_isAdd = false;
					}
					else
					{
						newNode.m_isAdd = node.m_isAdd;
					}
					((ArrayList)mapGrid[i]).Insert(j0 + dj + 1, newNode);
				}
			}
		}
		private static int getXFromGrid(ArrayList mapGrid, int di)
		{
			return getWidthFromGrid(mapGrid, 0, di);
		}
		private static int getYFromGrid(ArrayList mapGrid, int dj)
		{
			return getHeightFromGrid(mapGrid, 0, dj);
		}
		private static int getWidthFromGrid(ArrayList mapGrid, int i0, int di)
		{
			int sw = 0;

			for (int i = 0; i < di; i++)
			{
				sw += ((RectNode)((ArrayList)mapGrid[i0 + i])[0]).m_rect.Width;
			}

			return sw;
		}
		private static int getHeightFromGrid(ArrayList mapGrid, int j0, int dj)
		{
			int sh = 0;

			for (int j = 0; j < dj; j++)
			{
				sh += ((RectNode)((ArrayList)mapGrid[0])[j0 + j]).m_rect.Height;
			}

			return sh;
		}

		private static bool enableToPutIn(ArrayList mapGrid, RectNode rNode)
		{
			//按照高度，从高到低寻找空格子
			for (int i = 0; i < mapGrid.Count; i++)
			{
				for (int j = 0; j < ((ArrayList)mapGrid[i]).Count; j++)
				{
					RectNode nodeFirst = (RectNode)((ArrayList)mapGrid[i])[j];
					int sW = 0;
					int sH = 0;
					int di = 0;
					int dj = 0;

					if (nodeFirst.m_rect.Width == 0 || nodeFirst.m_rect.Height == 0)
					{
						continue;
					}
					for (di = 0; (i + di) < mapGrid.Count; di++)
					{
						RectNode wRn = (RectNode)((ArrayList)mapGrid[i + di])[j];

						if (wRn.m_isAdd == true)
						{
							goto ctn;
						}
						sW += wRn.m_rect.Width;
						if (sW >= rNode.m_rect.Width)
						{
							goto hLoop;
						}
					}
					goto ctn;
				hLoop:
					for (dj = 0; (j + dj) < ((ArrayList)mapGrid[i]).Count; dj++)
					{
						RectNode hRn = (RectNode)((ArrayList)mapGrid[i])[j + dj];

						if (hRn.m_isAdd == true)
						{
							goto ctn;
						}
						sH += hRn.m_rect.Height;
						if (sH >= rNode.m_rect.Height)
						{
							goto lastNode;
						}
					}
					goto ctn;
				lastNode:
					RectNode rn = (RectNode)((ArrayList)mapGrid[i + di])[j + dj];
					if (rn.m_isAdd == true)
					{
						goto ctn;
					}
					else
					{
						rNode.m_rect.X = getXFromGrid(mapGrid, i);
						rNode.m_rect.Y = getYFromGrid(mapGrid, j);
						rNode.m_isAdd = true;
						crossInsertToGrid(
							mapGrid,
							i, j, di, dj,
							rn.m_rect.Width - sW + rNode.m_rect.Width,
							rn.m_rect.Height - sH + rNode.m_rect.Height);
						return true;
					}
				ctn:
					continue;
				}
			}
			return false;
		}
		private static int getRectNestingByPreset(Dictionary<string, RectNode> mapRectNode, int width = conf_maxSizeOfPreset, int height = conf_maxSizeOfPreset)
		{
			var resultByWidth = from pair in mapRectNode orderby pair.Value.m_rect.Width descending select pair;

			int count = 0;
			ArrayList arrMapGrid = new ArrayList();
			ArrayList tmpMapGrid = new ArrayList();
			ArrayList tmpArr = new ArrayList();
			RectNode tmpNode = new RectNode(new System.Drawing.Rectangle(0, 0, width, height), false);
			int refreshCount = 0;

			tmpArr.Add(tmpNode);
			tmpMapGrid.Add(tmpArr);
			arrMapGrid.Add(tmpMapGrid);
			if (ImageNesting.s_pW != null && ImageNesting.s_pW.mx_canvas != null)
			{
				ImageNesting.s_pW.mx_canvas.Children.Clear();
			}

			foreach (KeyValuePair<string, RectNode> pair in resultByWidth)
			{
				count++;
				for (int i = 0; true; i++)
				{
					if (!enableToPutIn((ArrayList)arrMapGrid[i], pair.Value))
					{
						if (i == arrMapGrid.Count - 1)
						{
							tmpMapGrid = new ArrayList();
							tmpArr = new ArrayList();
							tmpNode = new RectNode(new System.Drawing.Rectangle(0, 0, width, height), false);
							tmpArr.Add(tmpNode);
							tmpMapGrid.Add(tmpArr);
							arrMapGrid.Add(tmpMapGrid);
						}
					}
					else
					{
						pair.Value.m_packCount = i;
						break;
					}
				}
				printString("\r\n进度：" + count.ToString() + "/" + mapRectNode.Count.ToString() + "\n" + "包数：" + arrMapGrid.Count, true);
				if (ImageNesting.s_pW != null && ImageNesting.s_pW.mx_canvas != null)
				{
					ImageNesting.s_pW.drawRectNode(pair.Value, ImageNesting.s_pW.mx_canvas.ActualWidth / width / 4);
				}
				refreshCount++;
				if (refreshCount % 17 == 0)
				{
					DoEvents();
				}
			}

			Dictionary<string, RectNode> lstMap = new Dictionary<string, RectNode>();

			foreach (KeyValuePair<string, RectNode> pair in mapRectNode)
			{

				if (pair.Value.m_packCount == arrMapGrid.Count - 1)
				{
					lstMap.Add(pair.Key, pair.Value);
				}
			}
			int maxPow = 0;

			//得到预期的2的整数次幂
			maxPow = getMaxPow(lstMap.Values.ToList());

			if (maxPow != 11)
			{
				maxPow--;
				do
				{
					maxPow++;
					if (ImageNesting.s_pW != null)
					{
						ImageNesting.s_pW.clearChildGrid(arrMapGrid.Count - 1);
					}
					printString("\r\n即将完成，开始尾包尺寸重定" + Math.Pow(2, maxPow), true);
				} while (!getRectNesting(lstMap, (int)Math.Pow(2, maxPow), (int)Math.Pow(2, maxPow), true));
			}
			printString("\r\n[完成]\r\n");

			return arrMapGrid.Count;
		}
		private static bool getRectNesting(Dictionary<string, RectNode> mapRectNode, int width, int height, bool isPreset = false)
		{
			var resultByWidth = from pair in mapRectNode orderby pair.Value.m_rect.Width descending select pair;

			ArrayList mapGrid = new ArrayList();
			ArrayList firstArr = new ArrayList();
			RectNode firstNode = new RectNode(new System.Drawing.Rectangle(0, 0, width, height), false);
			int refreshCount = 0;

			firstArr.Add(firstNode);
			mapGrid.Add(firstArr);
			if (ImageNesting.s_pW != null)
			{
				if (!isPreset)
				{
					ImageNesting.s_pW.mx_canvas.Children.Clear();
				}
			}

			foreach (KeyValuePair<string, RectNode> pair in resultByWidth)
			{
				if (!enableToPutIn(mapGrid, pair.Value))
				{
					return false;
				}
				if (!isPreset)
				{
					printString("\r\n进度：" + mapGrid.Count.ToString() + "/" + (mapRectNode.Count + 1).ToString(), true);
					if (ImageNesting.s_pW != null)
					{
						ImageNesting.s_pW.drawRectNode(pair.Value, ImageNesting.s_pW.mx_canvas.ActualWidth / width);
					}
				}
				else
				{
					if (ImageNesting.s_pW != null)
					{
						ImageNesting.s_pW.drawRectNode(pair.Value, ImageNesting.s_pW.mx_canvas.ActualWidth / conf_maxSizeOfPreset / 4);
					}
				}
				refreshCount++;
				if (refreshCount % 17 == 0)
				{
					DoEvents();
				}
			}
			if (!isPreset)
			{
				printString("[完成]\r\n");
			}

			return true;
		}
		public static void pngToTgaRectNesting(string path, string filter = "*.png", int deep = 0, bool isPreset = false)
		{
			Dictionary<string, RectNode> mapRectNode = new Dictionary<string, RectNode>();
			string projPath = MainWindow.s_pW.m_projPath;
			int presetCount = 0;
			int fileCount = 0;

			printString("========开始打包========\r\n" + "路径：" + path + "\r\n");
			fileCount = addFileToArr(path, filter, mapRectNode);

			if (fileCount == 0)
			{
				return;
			}
			if (isPreset)
			{
				DirectoryInfo di = new DirectoryInfo(projPath + "\\images\\");
				DirectoryInfo[] arrPreDri = di.GetDirectories("*_preset*");
				DirectoryInfo[] arrAllDri = di.GetDirectories("*");

				if (arrPreDri.Count() != arrAllDri.Count())
				{
					MessageBox.Show("存在自定义图片分包，无法进行预设打包。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
				printString("模式：预设打包\r\n");
				presetCount = getRectNestingByPreset(mapRectNode, conf_maxSizeOfPreset, conf_maxSizeOfPreset);
			}
			else
			{
				bool isFirst = true;
				int maxPow = 0;

				//得到预期的2的整数次幂
				maxPow = getMaxPow(mapRectNode.Values.ToList());
				s_fileCount = mapRectNode.Count;

				printString("模式：开发打包\r\n");
				do
				{
					if (isFirst)
					{
						isFirst = false;
						printString("文件数量:" + mapRectNode.Count + "\r\ntga图片预计尺寸:" + Math.Pow(2, maxPow) + "\r\n");
					}
					else
					{
						maxPow++;
						printString("本次尝试失败，尝试下一尺寸...\r\ntga图片预计尺寸:" + Math.Pow(2, maxPow) + "\r\n");
					}
				} while (!getRectNesting(mapRectNode, (int)Math.Pow(2, maxPow), (int)Math.Pow(2, maxPow)));
			}

			if (path.Last() == '\\')
			{
				path = path.Remove(path.Length - 1);
				if (path.Last() == '\\')
				{
					path = path.Remove(path.Length - 1);
				}
			}

			if (isPreset && presetCount != 0)
			{
				DirectoryInfo di = new DirectoryInfo(projPath + "\\images\\");
				DirectoryInfo[] arrDirInfo = di.GetDirectories("*_preset*");
				XmlDocument docRes = new XmlDocument();
				XmlElement xeResRoot = docRes.CreateElement("BoloUI");

				foreach(DirectoryInfo dri in arrDirInfo)
				{
					Directory.Delete(dri.FullName, true);
				}

				XmlDocument[] arrDocGrid = new XmlDocument[presetCount];

				for (int i = 0; i < presetCount; i++)
				{
					arrDocGrid[i] = new XmlDocument();
					XmlElement xeRoot = arrDocGrid[i].CreateElement("UIImageResource");
					arrDocGrid[i].AppendChild(xeRoot);
					Directory.CreateDirectory(projPath + "\\images\\_preset" + (i + 1).ToString("00"));

					XmlElement xeRes = docRes.CreateElement("resource");

					xeRes.SetAttribute("name", "_preset" + (i + 1).ToString("00"));
					xeResRoot.AppendChild(xeRes);
				}

				foreach (KeyValuePair<string, RectNode> pairRn in mapRectNode.ToList())
				{
					XmlDocument docGrid = arrDocGrid[pairRn.Value.m_packCount];
					XmlElement xe = docGrid.CreateElement("Image");
					string name = pairRn.Key.Remove(pairRn.Key.LastIndexOf("."));

					xe.SetAttribute("Name", name);
					//因为差值什么的显示问题，所以要-2和+1。
					xe.SetAttribute("Width", (pairRn.Value.m_rect.Width - 2).ToString());
					xe.SetAttribute("Height", (pairRn.Value.m_rect.Height - 2).ToString());
					xe.SetAttribute("X", (pairRn.Value.m_rect.X + 1).ToString());
					xe.SetAttribute("Y", (pairRn.Value.m_rect.Y + 1).ToString());
					docGrid.DocumentElement.AppendChild(xe);
					File.Copy(path + "\\" + pairRn.Key, projPath + "\\images\\_preset" + (pairRn.Value.m_packCount + 1).ToString("00") + "\\" + pairRn.Key, true);
				}

				for(int i = 0; i < presetCount; i++)
				{
					string xmlPath = projPath + "\\images\\_preset" + (i + 1).ToString("00") + ".xml";

					arrDocGrid[i].Save(xmlPath);
					//MainWindow.s_pW.openFileByPath(xmlPath);
					PackImage.refreshImagePack(xmlPath);
				}

				docRes.AppendChild(xeResRoot);
				docRes.Save(projPath + "\\images\\resource.xml");
			}
			else
			{
				string fileName = System.IO.Path.GetFileName(path);
				XmlDocument docGrid = new XmlDocument();
				XmlElement xeRoot = docGrid.CreateElement("UIImageResource");

				docGrid.AppendChild(xeRoot);
				foreach (KeyValuePair<string, RectNode> pairRn in mapRectNode.ToList())
				{
					XmlElement xe = docGrid.CreateElement("Image");
					string name = pairRn.Key.Remove(pairRn.Key.LastIndexOf("."));

					xe.SetAttribute("Name", name);
					//因为差值什么的显示问题，所以要-2和+1。
					xe.SetAttribute("Width", (pairRn.Value.m_rect.Width - 2).ToString());
					xe.SetAttribute("Height", (pairRn.Value.m_rect.Height - 2).ToString());
					xe.SetAttribute("X", (pairRn.Value.m_rect.X + 1).ToString());
					xe.SetAttribute("Y", (pairRn.Value.m_rect.Y + 1).ToString());
					xeRoot.AppendChild(xe);
				}
				string xmlPath = projPath + "\\images\\" + fileName + ".xml";

				docGrid.Save(xmlPath);
				//MainWindow.s_pW.openFileByPath(xmlPath);
				PackImage.refreshImagePack(xmlPath);
			}
			MainWindow.s_pW.updateGL("", W2GTag.W2G_IMAGE_RELOAD);
		}
		private void clearChildGrid(int num)
		{
			System.Windows.Shapes.Rectangle dRect = new Rectangle()
			{
				Width = mx_canvas.ActualWidth / 4,
				Height = mx_canvas.ActualHeight / 4,
				Margin = new Thickness(
					(mx_canvas.ActualWidth / 4) * (num % 4),
					(mx_canvas.ActualWidth / 4) * (num / 4),
					0, 0),
				Stroke = new SolidColorBrush(System.Windows.Media.Colors.White),
				StrokeThickness = 0,
			};
			dRect.Fill = new SolidColorBrush(System.Windows.Media.Colors.White);
			mx_canvas.Children.Add(dRect);
		}
		private void drawRectNode(RectNode node, double per = 1)
		{
			System.Windows.Shapes.Rectangle dRect = new Rectangle()
			{
				Width = node.m_rect.Width * per,
				Height = node.m_rect.Height * per,
				Margin = new Thickness(
					node.m_rect.X * per + (mx_canvas.ActualWidth / 4) * (node.m_packCount % 4),
					node.m_rect.Y * per + (mx_canvas.ActualHeight / 4) * (node.m_packCount / 4),
					0, 0),
				Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x00, 0x00, 0x00)),
				StrokeThickness = 0,
			};
			if (node.m_isAdd)
			{
				dRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0x33, 0x99, 0xff));
			}
			else
			{
				dRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x00, 0x33, 0x99, 0xff));
			}
			mx_canvas.Children.Add(dRect);
		}
		private void refreshDrawGrid(ArrayList mapGrid, double per = 1)
		{
			mx_canvas.Children.Clear();
			for(int i = 0; i < mapGrid.Count; i++)
			{
				for(int j = 0; j < ((ArrayList)mapGrid[i]).Count; j++)
				{
					RectNode node = (RectNode)((ArrayList)mapGrid[i])[j];
					System.Windows.Shapes.Rectangle dRect = new Rectangle()
					{
						Width = node.m_rect.Width * per,
						Height = node.m_rect.Height * per,
						Margin = new Thickness(node.m_rect.X * per, node.m_rect.Y * per, 0, 0),
						Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x00, 0x00, 0x00)),
						StrokeThickness = 0,
					};
					if(node.m_isAdd)
					{
						dRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0x33, 0x99, 0xff));
					}
					else
					{
						dRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x00, 0x33, 0x99, 0xff));
					}
					mx_canvas.Children.Add(dRect);
				}
			}
		}
		private static void DoEvents()
		{
			if (ImageNesting.s_pW != null)
			{
				DispatcherFrame frame = new DispatcherFrame();

				Dispatcher.CurrentDispatcher.BeginInvoke(
					DispatcherPriority.Background,
					new DispatcherOperationCallback(
						delegate(object f)
						{
							((DispatcherFrame)f).Continue = false;

							return null;
						}
					),
					frame);
				Dispatcher.PushFrame(frame);
			}
		}

		private static bool refreshRes(XmlElement xeRoot, string oldName, string newName)
		{
			string imgName = "";
			string attrName = "";
			bool isChanged = false;

			if (oldName != "" && oldName != null && newName != null && (xeRoot.Name == "imageShape" || xeRoot.Name == "frame"))
			{
				if (xeRoot.GetAttribute("image") != "")
				{
					imgName = xeRoot.GetAttribute("image");
					attrName = "image";
				}
				if (xeRoot.GetAttribute("ImageName") != "")
				{
					imgName = xeRoot.GetAttribute("ImageName");
					attrName = "ImageName";
				}

				if (imgName == oldName)
				{
					xeRoot.SetAttribute(attrName, newName);
					isChanged = true;
				}
			}

			foreach (XmlNode xn in xeRoot.ChildNodes)
			{
				if (xn.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xn;
					bool retChild = false;

					retChild = refreshRes(xe, oldName, newName);
					if (retChild == true)
					{
						isChanged = true;
					}
				}
			}

			return isChanged;
		}
		private static void updateResLink(string path, string oldName, string newName, string oldFolder, string newFolder)
		{
			if(!Directory.Exists(path))
			{
				return;
			}
			DirectoryInfo di = new DirectoryInfo(path);
			FileInfo[] arrBoloUI = di.GetFiles("*.xml");

			foreach (FileInfo fi in arrBoloUI)
			{
				XmlDocument docXml = new XmlDocument();

				docXml.Load(fi.FullName);
				if(docXml.DocumentElement.Name == "BoloUI")
				{
					string newFullName;
					if (newFolder == "" || newName == "")
					{
						newFullName = "";
					}
					else
					{
						newFullName = newFolder + "." + newName;
					}
					if(refreshRes(docXml.DocumentElement, oldFolder + "." + oldName, newFullName))
					{
						bool isHaveRes = false;

						foreach (XmlNode xn in docXml.DocumentElement.SelectNodes("resource"))
						{
							if(xn.NodeType == XmlNodeType.Element)
							{
								XmlElement xeRes = (XmlElement)xn;

								if(xeRes.Name == newFolder)
								{
									isHaveRes = true;
								}
							}
						}
						if(!isHaveRes)
						{
							XmlElement xeRes = docXml.CreateElement("resource");

							xeRes.SetAttribute("name", newFolder);
							docXml.DocumentElement.AppendChild(xeRes);
						}
						docXml.Save(fi.FullName);
					}
				}
			}
		}
		public static void moveImageLink(string oldName, string newName, string oldFolder, string newFolder)
		{
			updateResLink(MainWindow.s_pW.m_projPath + "\\skin", oldName, newName, oldFolder, newFolder);
			updateResLink(MainWindow.s_pW.m_projPath, oldName, newName, oldFolder, newFolder);
		}
		private static bool refreshRes(XmlElement xeRoot, Dictionary<string, string> mapNameDir, Dictionary<string, int> mapResDir, string dirHead = "")
		{
			string imgName = "";
			bool isChanged = false;

			if (xeRoot.Name == "imageShape" || xeRoot.Name == "frame")
			{
				if (xeRoot.GetAttribute("image") != "")
				{
					imgName = xeRoot.GetAttribute("image");
				}
				if (xeRoot.GetAttribute("ImageName") != "")
				{
					imgName = xeRoot.GetAttribute("ImageName");
				}

				if (imgName != "")
				{
					string dirName = System.IO.Path.GetFileNameWithoutExtension(imgName);
					string fileName = System.IO.Path.GetExtension(imgName).Remove(0, 1);

					if (dirName != "" && fileName != "")
					{
						if (dirName == null || dirHead == "" || (dirHead != "" && dirName.IndexOf(dirHead) == 0))
						{
							string newDirName = "";

							if (mapNameDir.TryGetValue(fileName, out newDirName))
							{
								string newName = newDirName + "." + fileName;
								int dirCount = 0;

								if (xeRoot.GetAttribute("image") != "" && xeRoot.GetAttribute("image") != newName)
								{
									xeRoot.SetAttribute("image", newName);
									isChanged = true;
								}
								if (xeRoot.GetAttribute("ImageName") != "" && xeRoot.GetAttribute("ImageName") != newName)
								{
									xeRoot.SetAttribute("ImageName", newName);
									isChanged = true;
								}
								if(mapResDir.TryGetValue(newDirName, out dirCount))
								{
									mapResDir[newDirName] = dirCount + 1;
								}
								else
								{
									mapResDir.Add(newDirName, 1);
								}
							}
						}
					}
				}
			}

			foreach(XmlNode xn in xeRoot.ChildNodes)
			{
				if(xn.NodeType == XmlNodeType.Element)
				{
					XmlElement xe = (XmlElement)xn;
					bool retChild = false;

					retChild = refreshRes(xe, mapNameDir, mapResDir, dirHead);
					if(retChild == true)
					{
						isChanged = true;
					}
				}
			}

			return isChanged;
		}
		private static void refreshAllSkinRes(string path, Dictionary<string, string> mapNameDir)
		{
			if(!Directory.Exists(path))
			{
				return;
			}
			DirectoryInfo di = new DirectoryInfo(path);
			FileInfo[] arrBoloUI = di.GetFiles("*.xml");
			Dictionary<string, int> mapResDir = new Dictionary<string, int>();

			foreach (FileInfo fi in arrBoloUI)
			{
				XmlDocument docSkin = new XmlDocument();

				printString("\r\n" + fi.FullName, true);
				DoEvents();
				docSkin.Load(fi.FullName);
				//refreshRes(docSkin.DocumentElement, mapNameDir, "_preset");

				if (docSkin.DocumentElement.Name == "BoloUI")
				{
					if(refreshRes(docSkin.DocumentElement, mapNameDir, mapResDir, ""))
					{
						foreach (XmlNode xn in docSkin.DocumentElement.SelectNodes("resource"))
						{
							docSkin.DocumentElement.RemoveChild(xn);
						}
						foreach (KeyValuePair<string, int> pairRes in mapResDir)
						{
							XmlElement xeRes = docSkin.CreateElement("resource");

							xeRes.SetAttribute("name", pairRes.Key);
							docSkin.DocumentElement.AppendChild(xeRes);
						}
						docSkin.Save(fi.FullName);
					}
				}
			}
			printString("\r\n[完成]\r\n", false);
		}
		private static void reLinkSkin()
		{
			Dictionary<string, string> mapNameDir = new Dictionary<string, string>();

			printString("========开始重定向皮肤资源========\r\n");
			addFileToDirMap(MainWindow.s_pW.m_projPath + "\\images", mapNameDir);
			refreshAllSkinRes(MainWindow.s_pW.m_projPath + "\\skin", mapNameDir);
			refreshAllSkinRes(MainWindow.s_pW.m_projPath, mapNameDir);
		}

		private void mx_start_Click(object sender, RoutedEventArgs e)
		{
			mx_start.IsEnabled = false;
			DoEvents();

			string path = MainWindow.s_pW.m_projPath;
			if(mx_rPreset.IsChecked == true)
			{
				mx_imgDebug.Text = "";
				if (Directory.Exists(path + "\\preset"))
				{
					pngToTgaRectNesting(path + "\\preset", m_filter, m_deep, true);
					reLinkSkin();
				}
				else
				{
					MessageBox.Show("没有找到预设图片目录，如想要进行预设图片打包，请新建preset目录。（" + path + "\\preset" + "）\r\n",
						"错误", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			else if(mx_rDev.IsChecked == true)
			{
				DirectoryInfo di = new DirectoryInfo(path + "\\images");
				DirectoryInfo[] arrDirInfo = di.GetDirectories();
				XmlDocument docRes = new XmlDocument();
				XmlElement xeResRoot = docRes.CreateElement("BoloUI");

				foreach (DirectoryInfo dri in arrDirInfo)
				{
					mx_imgDebug.Text = "";
					pngToTgaRectNesting(dri.FullName, m_filter, m_deep, false);
					XmlElement xeRes = docRes.CreateElement("resource");

					xeRes.SetAttribute("name", dri.Name);
					xeResRoot.AppendChild(xeRes);
				}
				docRes.AppendChild(xeResRoot);
				docRes.Save(MainWindow.s_pW.m_projPath + "\\images\\resource.xml");
			}
			else if(mx_rRefreshRes.IsChecked == true)
			{
				mx_imgDebug.Text = "";

				MessageBoxResult retInit = MessageBox.Show(
					"此操作会删除工程目录下的preset文件夹（预设打包初始目录），且操作不可逆，是否继续？",
					"确认", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);

				if (retInit == MessageBoxResult.OK)
				{
					if(Directory.Exists(MainWindow.s_pW.m_projPath + "\\preset\\"))
					{
						reLinkSkin();
						Directory.Delete(MainWindow.s_pW.m_projPath + "\\preset\\", true);
					}
					else
					{
						MessageBoxResult ret = MessageBox.Show(
							"这不是一个使用预设打包的BoloUI工程（没有找到preset文件夹）。如存在不同开发打包下的同名文件，可能会引发不可预知的问题，即便如此仍然要继续吗？",
							"警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

						if(ret == MessageBoxResult.OK)
						{
							reLinkSkin();
						}
					}
				}
			}

			mx_start.IsEnabled = true;
		}
	}
}
