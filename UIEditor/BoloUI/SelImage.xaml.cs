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
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.BoloUI
{
	public partial class SelImage
	{
		public IAttrRow m_iRowImage;
		public PackImage m_curTgaCtrl;
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
		public Dictionary<string, TreeViewItem> m_mapPngItem;
		public XmlDocument m_doc;

		static public SelImage s_pW;

		public SelImage(IAttrRow iRowImage)
		{
			s_pW = this;
			m_iRowImage = iRowImage;
			m_mapLocalRes = new Dictionary<string, TreeViewItem>();
			m_mapOtherRes = new Dictionary<string, TreeViewItem>();
			m_mapPngItem = new Dictionary<string, TreeViewItem>();
			InitializeComponent();
			this.Owner = MainWindow.s_pW;

			m_curTgaCtrl = null;
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

		public void refreshImageItem(TreeViewItem tgaItem, string resName)
		{
			string resPath = Project.Setting.s_imagePath + "\\" + resName + ".xml";
			string tgaPath = Project.Setting.s_imagePath + "\\" + resName + ".tga";
			string pngFolderPath = Project.Setting.s_imagePath + "\\" + resName + "\\";

			tgaItem.ToolTip = resPath;
			tgaItem.Selected += tgaItem_Selected;
			if(System.IO.File.Exists(resPath) && System.IO.File.Exists(tgaPath))
			{
				ImageIndex imgIndex;

				if(MainWindow.s_pW.m_mapImageIndex.TryGetValue(resPath, out imgIndex))
				{
					foreach(KeyValuePair<string, XmlElement> pairImg in imgIndex.m_mapImageXe.ToList())
					{
						TreeViewItem pngItem = new TreeViewItem();
						string pngPath = pngFolderPath + pairImg.Key + ".png";

						pngItem.Header = pairImg.Key;
						pngItem.ToolTip = resName + "." + pairImg.Key;
						pngItem.Selected += pngItem_Selected;
						tgaItem.Items.Add(pngItem);
						m_mapPngItem.Add(pngPath, pngItem);
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
				if (m_iRowImage != null && m_iRowImage.m_parent != null && m_iRowImage.m_parent.m_xmlCtrl != null &&
					m_iRowImage.m_parent.m_xmlCtrl.m_xmlDoc != null)
				{
					m_doc = m_iRowImage.m_parent.m_xmlCtrl.m_xmlDoc;
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

		private void showPackImage(TreeViewItem tgaItem, string pngName = null)
		{
			string tgaPath = Project.Setting.s_imagePath + "\\" + tgaItem.Header.ToString() + ".xml";

			m_curTgaCtrl = new PackImage(tgaPath, false);

			mx_imgFrame.Children.Clear();
			mx_imgFrame.Width = m_curTgaCtrl.m_imageWidth;
			mx_imgFrame.Height = m_curTgaCtrl.m_imageHeight;
			m_curTgaCtrl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			m_curTgaCtrl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
			mx_imgFrame.Children.Add(m_curTgaCtrl);

			m_curTgaCtrl.m_curPngName = pngName;
		}
		private void tgaItem_Selected(object sender, RoutedEventArgs e)
		{
			if (sender is TreeViewItem && mx_skinTreeFrame.SelectedItem == sender)
			{
				TreeViewItem tgaItem = (TreeViewItem)sender;

				showPackImage(tgaItem);
			}
		}
		private void pngItem_Selected(object sender, RoutedEventArgs e)
		{
			if (sender is TreeViewItem && mx_skinTreeFrame.SelectedItem == sender)
			{
				TreeViewItem pngItem = (TreeViewItem)sender;

				if(pngItem.Parent is TreeViewItem)
				{
					TreeViewItem tgaItem = (TreeViewItem)pngItem.Parent;

					showPackImage(tgaItem, pngItem.Header.ToString());
					m_curImg = pngItem;
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
			if (m_curImg != null && m_curTgaCtrl != null && m_curTgaCtrl.m_curPngName != null && m_curTgaCtrl.m_curPngName != "")
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
							m_iRowImage.m_parent.m_xmlCtrl.m_treeSkin.addResItem(newXe);
						}
					}

					newImgValue = ((TreeViewItem)m_curImg.Parent).Header.ToString() + "." + m_curImg.Header.ToString();

					m_iRowImage.m_value = newImgValue;

					m_iRowImage.m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
						new XmlOperation.HistoryNode(m_iRowImage.m_parent.m_xe, "Width", m_iRowImage.m_parent.m_xe.GetAttribute("Width"),
							m_curTgaCtrl.getCurPngWidth().ToString())
						);
					m_iRowImage.m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(
						new XmlOperation.HistoryNode(m_iRowImage.m_parent.m_xe, "Height", m_iRowImage.m_parent.m_xe.GetAttribute("Height"),
							m_curTgaCtrl.getCurPngHeight().ToString())
						);
					m_iRowImage.m_parent.m_basic.changeSelectItem();
				}
			}
			this.Close();
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void mx_skinTreeFrame_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
		}
	}
}
