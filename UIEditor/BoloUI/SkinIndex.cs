using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace UIEditor.BoloUI
{
	public class SkinIndex
	{
		public XmlDocument m_doc;
		public Dictionary<string, XmlElement> m_mapSkinXe;

		public SkinIndex(XmlDocument doc)
		{
			m_doc = doc;
			m_mapSkinXe = new Dictionary<string, XmlElement>();

			if(m_doc.DocumentElement.Name == "BoloUI")
			{
				foreach (XmlNode xn in m_doc.DocumentElement.ChildNodes)
				{
					if (xn.NodeType == XmlNodeType.Element)
					{
						XmlElement xe = (XmlElement)xn;

						if (xe.Name == "skin" || xe.Name == "publicskin")
						{
							string skinName = xe.GetAttribute("Name");
							XmlElement xeTmp;

							if(skinName != "" && !m_mapSkinXe.TryGetValue(skinName, out xeTmp))
							{
								m_mapSkinXe.Add(skinName, xe);
							}
						}
					}
				}
			}
		}

		//<SkinIndex>太影响加载速度，放弃。
// 		private static void addFileToSkinIndex(FileInfo fi)
// 		{
// 			if (fi.Extension == ".xml")
// 			{
// 				XmlDocument skinDoc = new XmlDocument();
// 
// 				try
// 				{
// 					skinDoc.Load(fi.FullName);
// 					if (skinDoc.DocumentElement.Name == "BoloUI")
// 					{
// 						MainWindow.s_pW.m_mapSkinIndex.Add(fi.FullName, new SkinIndex(skinDoc));
// 					}
// 				}
// 				catch
// 				{
// 
// 				}
// 			}
// 		}
// 		public static void refreshSkinIndex()
// 		{
// 			string proPath = MainWindow.s_pW.m_projPath;
// 			string skinPath = MainWindow.s_pW.m_skinPath;
// 
// 			if(proPath != null && skinPath != null && proPath != "" && skinPath != "" &&
// 				Directory.Exists(proPath) && Directory.Exists(skinPath))
// 			{
// 				DirectoryInfo diMain = new DirectoryInfo(proPath);
// 				DirectoryInfo diSkin = new DirectoryInfo(skinPath);
// 
// 				MainWindow.s_pW.m_mapSkinIndex = new Dictionary<string, SkinIndex>();
// 				foreach (FileInfo fi in diMain.GetFiles())
// 				{
// 					addFileToSkinIndex(fi);
// 				}
// 				foreach (FileInfo fi in diSkin.GetFiles())
// 				{
// 					addFileToSkinIndex(fi);
// 				}
// 			}
// 		}
	}
}
