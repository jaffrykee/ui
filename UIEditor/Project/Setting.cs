using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UIEditor.Project
{
	public class Setting
	{
		static public XmlElement initResolutionSetting(XmlNode xnConfig)
		{
			if(MainWindow.s_pW.m_docConf != null)
			{
				XmlElement xeToolConfig = MainWindow.s_pW.m_docConf.DocumentElement;

				if(xeToolConfig.Name == "Config")
				{
					XmlNodeList xnListRsl = xnConfig.SelectNodes("ResolutionSetting");

					if(xnListRsl.Count > 0)
					{
						foreach(XmlNode xnRsl in xnListRsl)
						{
							xnConfig.RemoveChild(xnRsl);
						}
					}

					XmlNode xnToolRsl = xeToolConfig.SelectSingleNode("ResolutionSetting");
					XmlElement xeRslSetting = xnConfig.OwnerDocument.CreateElement("ResolutionSetting");

					if(xnToolRsl != null && xnToolRsl is XmlElement)
					{
						XmlElement xeToolRsl = (XmlElement)xnToolRsl;
						xeRslSetting.InnerXml = xeToolRsl.InnerXml;
					}
					else
					{
						string[] arrStrRsl = {"960 x 540", "1024 x 768", "1134 x 640", "1280 x 720",
											 "1334 x 750", "1920 x 1080", "2560 x 1440"};
						bool isDef = true;

						foreach (string strRow in arrStrRsl)
						{
							XmlElement xeRow = xnConfig.OwnerDocument.CreateElement("row");

							xeRow.InnerXml = strRow;
							if (isDef)
							{
								isDef = false;
								xeRow.SetAttribute("isDefault", "true");
							}
							xeRslSetting.AppendChild(xeRow);
						}
					}
					xnConfig.AppendChild(xeRslSetting);

					return xeRslSetting;
				}
			}

			return null;
		}
		static public void refreshResolutionBoxByConfigNode(XmlNode xnResolutionSetting)
		{
			if (xnResolutionSetting != null)
			{
				bool isDefault = true;

				foreach (XmlNode xnRow in xnResolutionSetting.ChildNodes)
				{
					if (xnRow.Name == "row" && xnRow is XmlElement)
					{
						System.Windows.Controls.ComboBoxItem cbiRow = new System.Windows.Controls.ComboBoxItem();

						cbiRow.Content = xnRow.InnerXml;
						MainWindow.s_pW.mx_resolution.Items.Add(cbiRow);
						if (((XmlElement)xnRow).GetAttribute("isDefault") == "true")
						{
							if (isDefault)
							{
								//cbiRow.Content += " <默认>";
								MainWindow.s_pW.mx_resolution.SelectedItem = cbiRow;
								isDefault = false;
							}
							else
							{
								((XmlElement)xnRow).RemoveAttribute("isDefault");
							}
						}
					}
				}
			}
		}
	}
}
