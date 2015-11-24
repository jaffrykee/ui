using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace UIEditor.BoloUI
{

	public class ImageIndex
	{
		public XmlDocument m_doc;
		public XmlControl m_xmlCtrl;
		public Dictionary<string, XmlElement> m_mapImageXe;

		public ImageIndex(XmlDocument doc, XmlControl xmlCtrl = null)
		{
			m_doc = doc;
			m_xmlCtrl = xmlCtrl;
			m_mapImageXe = new Dictionary<string, XmlElement>();

			if(m_doc.DocumentElement.Name == "UIImageResource")
			{
				foreach(XmlNode xnImg in m_doc.DocumentElement.ChildNodes)
				{
					if(xnImg.NodeType == XmlNodeType.Element)
					{
						XmlElement xeImg = (XmlElement)xnImg;

						if(xeImg.Name == "Image")
						{
							string imgName = xeImg.GetAttribute("Name");
							XmlElement xeTmp;

							if(imgName != "" && !m_mapImageXe.TryGetValue(imgName, out xeTmp))
							{
								m_mapImageXe.Add(imgName, xeImg);
							}
						}
					}
				}
			}
		}

		private static void addFileToImageIndex(FileInfo fi)
		{
			if(fi.Extension == ".xml")
			{
				XmlDocument imgDoc = new XmlDocument();

				try
				{
					imgDoc.Load(fi.FullName);
					if (imgDoc.DocumentElement.Name == "UIImageResource")
					{
						MainWindow.s_pW.m_mapImageIndex.Add(fi.FullName, new ImageIndex(imgDoc));
					}
				}
				catch
				{
					Public.ResultLink.showResult(
						"\r\n图片资源文件：\"" + fi.FullName + "\"文件的Xml格式错误，有可能是由于svn冲突造成的。",
						Public.ResultType.RT_ERROR);
				}
			}
		}
		public static void refreshImageIndex()
		{
			string imgPath = Project.Setting.s_imagePath;

			if(imgPath != null && imgPath != "" && Directory.Exists(imgPath))
			{
				DirectoryInfo diImage = new DirectoryInfo(imgPath);

				MainWindow.s_pW.m_mapImageIndex = new Dictionary<string, ImageIndex>();
				foreach(FileInfo fi in diImage.GetFiles())
				{
					addFileToImageIndex(fi);
				}
			}
		}
	}
}
