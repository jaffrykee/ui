using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.IO;

namespace UIEditor.Project
{
	public class Setting
	{
		static public XmlDocument s_docProj;
		static public string s_uiPackPath;
		static public string s_scriptPackPath;
		static public string s_gamePath;
		static public string s_skinPath;
		static public string s_imagePath;
		static public string s_projPath;
		static public string s_projName;

		static public void refreshAllProjectSetting()
		{
			XmlNode xnResolutionSetting = s_docProj.DocumentElement.SelectSingleNode("ResolutionSetting");

			refreshResolutionBoxByConfigNode(xnResolutionSetting);
			refreshPathSetting(initPathSetting(s_docProj.DocumentElement));
			s_docProj.Save(s_projPath + "\\" + s_projName);
			s_skinPath = s_projPath + "\\skin";
			s_imagePath = s_projPath + "\\images";
		}
		static public void refreshPathSetting(XmlElement xePathSetting)
		{
			s_uiPackPath = xePathSetting.GetAttribute("uiPackPath");
			s_scriptPackPath = xePathSetting.GetAttribute("scriptPackPath");
			s_gamePath = xePathSetting.GetAttribute("gamePath");
		}
		static public XmlElement initPathSetting(XmlNode xnConfig)
		{
			XmlNode xnPathSetting = xnConfig.SelectSingleNode("PathSetting");
			XmlElement xePathSetting = null;

			if(xnPathSetting != null && xnPathSetting is XmlElement)
			{
				xePathSetting = (XmlElement)xnPathSetting;
			}
			else
			{
				xePathSetting = xnConfig.OwnerDocument.CreateElement("PathSetting");
				xnConfig.AppendChild(xePathSetting);
				xnConfig.OwnerDocument.Save(s_projPath + "\\" + s_projName);
			}

			return xePathSetting;
		}
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

				MainWindow.s_pW.mx_resolution.Items.Clear();
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
		static public void refreshPrefabCombox(Dictionary<string, ComboBoxItem> mapEnum, ComboBox cbEnum)
		{
			string prefabPath = Project.Setting.s_projPath;

			if (prefabPath != null && prefabPath != "" && mapEnum != null && cbEnum != null)
			{
				cbEnum.Items.Clear();
				mapEnum.Clear();

				ComboBoxItem cbiDefault = new ComboBoxItem();

				cbiDefault.Content = "[默认值]";
				cbiDefault.ToolTip = "";
				cbEnum.Items.Add(cbiDefault);

				prefabPath += "\\..\\..\\texiao\\";

				if(Directory.Exists(prefabPath))
				{
					DirectoryInfo driPrefab = new DirectoryInfo(prefabPath);

					foreach(FileInfo fi in driPrefab.GetFiles())
					{
						if(fi.Extension == ".prefab")
						{
							ComboBoxItem cbiPrefab = new ComboBoxItem();

							cbiPrefab.Content = Path.GetFileNameWithoutExtension(fi.FullName);
							cbiPrefab.ToolTip = Path.GetFileNameWithoutExtension(fi.FullName);
							mapEnum.Add(Path.GetFileNameWithoutExtension(fi.FullName), cbiPrefab);
							cbEnum.Items.Add(cbiPrefab);
						}
					}
				}
				cbEnum.IsEditable = true;
			}
		}

		static private void openLinkFile(string pathPack, bool isBlock = true)
		{
			if (File.Exists(pathPack))
			{
				FileInfo fiPack = new FileInfo(pathPack);
				System.Diagnostics.Process prcPack = new System.Diagnostics.Process();

				prcPack.StartInfo.WorkingDirectory = fiPack.Directory.FullName;
				prcPack.StartInfo.FileName = fiPack.FullName;
				prcPack.StartInfo.CreateNoWindow = false;
				prcPack.Start();
				prcPack.WaitForExit();
			}
		}
		static public string getUiPackPath()
		{
			string pathPack = "";

			if (s_projPath != null)
			{
				if (s_uiPackPath != null && s_uiPackPath != "")
				{
					pathPack = s_uiPackPath;
				}
				else
				{
					pathPack = "\\..\\..\\..\\tools\\模型数据打包工具\\ui打包.bat";
					pathPack = s_projPath + pathPack;
				}
			}

			return pathPack;
		}
		static public void packUiMod()
		{
			if (s_projPath != null)
			{
				string pathPack = getUiPackPath();

				openLinkFile(pathPack);
			}
		}
		static public string getScriptPackPath()
		{
			string pathPack = "";

			if (s_projPath != null)
			{
				if (s_scriptPackPath != null && s_scriptPackPath != "")
				{
					pathPack = s_scriptPackPath;
				}
				else
				{
					pathPack = "\\..\\..\\..\\tools\\模型数据打包工具\\script打包.bat";
					pathPack = s_projPath + pathPack;
				}
			}

			return pathPack;
		}
		static public void packScriptMod()
		{
			if (s_projPath != null)
			{
				string pathPack = getScriptPackPath();

				openLinkFile(pathPack);
			}
		}
		static public string getGamePath()
		{
			string pathPack = "";

			if (s_projPath != null)
			{
				if (s_gamePath != null && s_gamePath != "")
				{
					pathPack = s_gamePath;
				}
				else
				{
					pathPack = "\\..\\..\\..\\tools\\客户端\\bin\\960x540.bat";
					pathPack = s_projPath + pathPack;
				}
			}

			return pathPack;
		}
		static public void openGame()
		{
			if (s_projPath != null)
			{
				string pathPack = getGamePath();

				openLinkFile(pathPack);
			}
		}

		static public string openSelectFileBox(string projPath = null)
		{
			if (projPath == null)
			{
				projPath = s_projPath;
			}

			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Title = "选择文件";
			openFileDialog.Filter = "所有文件|*.*";
			openFileDialog.FileName = string.Empty;
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.DefaultExt = "*";
			openFileDialog.InitialDirectory = projPath;
			System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				return null;
			}
			return openFileDialog.FileName;
		}
	}
}
