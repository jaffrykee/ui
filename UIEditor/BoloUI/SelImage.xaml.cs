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
using System.Xml;
using System.IO;

namespace UIEditor.BoloUI
{
	public partial class SelImage
	{
		public AttrRow m_rowImage;
		public PngControl m_curPngCtrl;
		private TreeViewItem mt_curImg;
		public TreeViewItem m_curImg
		{
			get { return mt_curImg; }
			set
			{
				mt_curImg = value;
				if(value == null)
				{
					mx_ok.IsEnabled = false;
				}
				else
				{
					mx_ok.IsEnabled = true;
				}
			}
		}
		public Dictionary<string, TreeViewItem> m_mapLocalRes;
		public Dictionary<string, TreeViewItem> m_mapOtherRes;
		public XmlDocument m_doc;

		static public SelImage s_pW;

		public SelImage(AttrRow rowImage)
		{
			s_pW = this;
			m_rowImage = rowImage;
			m_mapLocalRes = new Dictionary<string, TreeViewItem>();
			m_mapOtherRes = new Dictionary<string, TreeViewItem>();
			InitializeComponent();
			this.Owner = MainWindow.s_pW;

			m_curPngCtrl = null;
			m_curImg = null;
			ImageIndex.refreshImageIndex();
			refreshResMap();
			refreshResTree();
		}
		private void mx_root_Loaded(object sender, RoutedEventArgs e)
		{

		}
		private void mx_root_Unloaded(object sender, RoutedEventArgs e)
		{
			s_pW = null;
		}

		public void refreshImageItem(TreeViewItem viewItem, string resName)
		{
			string resPath = MainWindow.s_pW.m_imagePath + "\\" + resName + ".xml";
			string tgaPath = MainWindow.s_pW.m_imagePath + "\\" + resName + ".tga";

			viewItem.ToolTip = resPath;
			if(System.IO.File.Exists(resPath) && System.IO.File.Exists(tgaPath))
			{
				ImageIndex imgIndex;

				if(MainWindow.s_pW.m_mapImageIndex.TryGetValue(resPath, out imgIndex))
				{
					foreach(KeyValuePair<string, XmlElement> pairImg in imgIndex.m_mapImageXe.ToList())
					{
						TreeViewItem imgItem = new TreeViewItem();

						imgItem.Header = pairImg.Key;
						imgItem.ToolTip =  resName + "." + pairImg.Key;
						imgItem.Selected += imageItem_Selected;
						viewItem.Items.Add(imgItem);
					}
				}
			}
		}
		public void refreshResMap()
		{
			XmlElement xeRoot = null;

			if (SkinEditor.isCurItemSkinEditor())
			{
				m_doc = XmlControl.getCurXmlControl().m_curItem.m_xe.OwnerDocument;
			}
			else
			{
				if (m_rowImage != null && m_rowImage.m_parent != null && m_rowImage.m_parent.m_xmlCtrl != null &&
					m_rowImage.m_parent.m_xmlCtrl.m_xmlDoc != null)
				{
					m_doc = m_rowImage.m_parent.m_xmlCtrl.m_xmlDoc;
				}
			}
			if (m_doc != null)
			{
				xeRoot = m_doc.DocumentElement;
				if (xeRoot.Name == "BoloUI")
				{
					foreach (XmlNode xn in xeRoot.ChildNodes)
					{
						if (xn.NodeType == XmlNodeType.Element)
						{
							XmlElement xe = (XmlElement)xn;

							if (xe.Name == "resource" || xe.Name == "publicresource")
							{
								string resName = xe.GetAttribute("name");

								if (resName != "")
								{
									TreeViewItem viewItem;

									if (!m_mapLocalRes.TryGetValue(resName, out viewItem))
									{
										m_mapLocalRes.Add(resName, new TreeViewItem());
									}
								}
							}
						}
					}
				}
			}
			foreach (KeyValuePair<string, ImageIndex> pairImageIndex in MainWindow.s_pW.m_mapImageIndex.ToList())
			{
				TreeViewItem tmpItem;
				string groupName = System.IO.Path.GetFileNameWithoutExtension(pairImageIndex.Key);

				if (!m_mapLocalRes.TryGetValue(groupName, out tmpItem) &&
					!m_mapOtherRes.TryGetValue(groupName, out tmpItem))
				{
					m_mapOtherRes.Add(groupName, new TreeViewItem());
				}
			}
		}
		public void refreshResTree()
		{
			foreach (KeyValuePair<string, TreeViewItem> pairItem in m_mapLocalRes.ToList())
			{
				pairItem.Value.Header = pairItem.Key;
				mx_localRes.Items.Add(pairItem.Value);
				refreshImageItem(pairItem.Value, pairItem.Key);
			}
			foreach (KeyValuePair<string, TreeViewItem> pairItem in m_mapOtherRes.ToList())
			{
				pairItem.Value.Header = pairItem.Key;
				mx_otherRes.Items.Add(pairItem.Value);
				refreshImageItem(pairItem.Value, pairItem.Key);
			}
		}

		private void imageItem_Selected(object sender, RoutedEventArgs e)
		{
			if(sender is TreeViewItem)
			{
				TreeViewItem imgItem = (TreeViewItem)sender;

				if(imgItem.Parent is TreeViewItem)
				{
					TreeViewItem resItem = (TreeViewItem)imgItem.Parent;
					string pngPath = MainWindow.s_pW.m_imagePath + "\\" + resItem.Header.ToString() +
						"\\" + imgItem.Header.ToString() + ".png";
					m_curPngCtrl = new PngControl(pngPath);

					mx_imgFrame.Children.Clear();
					mx_imgFrame.Width = m_curPngCtrl.m_imgWidth;
					mx_imgFrame.Height = m_curPngCtrl.m_imgHeight;
					m_curPngCtrl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					m_curPngCtrl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					mx_imgFrame.Children.Add(m_curPngCtrl);
					m_curImg = imgItem;
				}
				else
				{
					m_curImg = null;
				}
			}
			else
			{
				m_curImg = null;
			}
		}
		private void mx_search_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (mx_search.Text != "")
			{
				MainWindow.refreshSearch(mx_rootItem, null);
				MainWindow.refreshSearch(mx_rootItem, mx_search.Text.ToString());
			}
			else
			{
				MainWindow.refreshSearch(mx_rootItem, null);
			}
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if (m_curImg != null && m_curPngCtrl != null)
			{
				string newImgValue;

				if(m_curImg.Parent != null && m_curImg.Parent is TreeViewItem)
				{
					if (((TreeViewItem)m_curImg.Parent).Parent == mx_otherRes)
					{
						XmlElement newXe = m_doc.CreateElement("resource");

						newXe.SetAttribute("name", ((TreeViewItem)m_curImg.Parent).Header.ToString());

						if (SkinEditor.isCurItemSkinEditor())
						{
							m_doc.DocumentElement.PrependChild(newXe);
						}
						else
						{
							m_rowImage.m_parent.m_xmlCtrl.m_treeSkin.addResItem(newXe);
						}
					}

					newImgValue = ((TreeViewItem)m_curImg.Parent).Header.ToString() + "." + m_curImg.Header.ToString();

					m_rowImage.m_value = newImgValue;

					m_rowImage.m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
						new XmlOperation.HistoryNode(m_rowImage.m_parent.m_xe, "Width", m_rowImage.m_parent.m_xe.GetAttribute("Width"),
							m_curPngCtrl.m_imgWidth.ToString())
						);
					m_rowImage.m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
						new XmlOperation.HistoryNode(m_rowImage.m_parent.m_xe, "Height", m_rowImage.m_parent.m_xe.GetAttribute("Height"),
							m_curPngCtrl.m_imgHeight.ToString())
						);
					m_rowImage.m_parent.m_basic.changeSelectItem();
				}
			}
			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
