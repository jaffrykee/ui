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
		static public string s_skinPath;
		static public string s_imagePath;
		static public string s_projPath;
		static public string s_projName;

		static public string s_uiPackPath;
		static public string s_scriptPackPath;
		static public string s_gamePath;
		static private string st_particlePath;
		static public string s_particlePath
		{
			get { return st_particlePath; }
			set
			{
				st_particlePath = value;
				if(value != "" && value != null)
				{
					MainWindow.s_pW.updateGL(value, W2GTag.W2G_PATH_PARTICLE);
				}
			}
		}
		static public string s_langPath;
		static private string st_backgroundPath;
		static public string s_backgroundPath
		{
			get { return st_backgroundPath; }
			set
			{
				st_backgroundPath = value;
				if (value != "" && value != null && MainWindow.s_pW.mx_showBack.IsChecked == true)
				{
					MainWindow.s_pW.updateGL(value, W2GTag.W2G_PATH_BACKGROUND);
				}
			}
		}
		static public Dictionary<string, List<string>> s_mapSkinIndex;

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
			s_particlePath = xePathSetting.GetAttribute("particlePath");
			s_langPath = xePathSetting.GetAttribute("langPath");
			s_backgroundPath = xePathSetting.GetAttribute("backgroundPath");
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
				MainWindow.s_pW.mx_resolutionBasic.Items.Clear();
				foreach (XmlNode xnRow in xnResolutionSetting.ChildNodes)
				{
					if (xnRow.Name == "row" && xnRow is XmlElement)
					{
						System.Windows.Controls.ComboBoxItem cbiRow = new System.Windows.Controls.ComboBoxItem();
						System.Windows.Controls.ComboBoxItem cbiRowBasic = new System.Windows.Controls.ComboBoxItem();

						cbiRow.Content = xnRow.InnerXml;
						cbiRowBasic.Content = xnRow.InnerXml;
						MainWindow.s_pW.mx_resolution.Items.Add(cbiRow);
						MainWindow.s_pW.mx_resolutionBasic.Items.Add(cbiRowBasic);
						if (((XmlElement)xnRow).GetAttribute("isDefault") == "true")
						{
							if (isDefault)
							{
								//cbiRow.Content += " <默认>";
								MainWindow.s_pW.mx_resolution.SelectedItem = cbiRow;
								MainWindow.s_pW.mx_resolutionBasic.SelectedItem = cbiRowBasic;
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
			if (mapEnum != null && cbEnum != null)
			{
				cbEnum.Items.Clear();
				mapEnum.Clear();

				ComboBoxItem cbiDefault = new ComboBoxItem();

				cbiDefault.Content = "[默认值]";
				cbiDefault.ToolTip = "";
				cbEnum.Items.Add(cbiDefault);

				if (Directory.Exists(Project.Setting.getParticlePath()))
				{
					DirectoryInfo driPrefab = new DirectoryInfo(Project.Setting.getParticlePath());

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
					pathPack = StringDic.getRealPath(s_projPath + "\\..\\..\\..\\tools\\模型数据打包工具\\ui打包.bat");
				}
			}

			return pathPack;
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
					pathPack = StringDic.getRealPath(s_projPath + "\\..\\..\\..\\tools\\模型数据打包工具\\script打包.bat");
				}
			}

			return pathPack;
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
					pathPack = StringDic.getRealPath(s_projPath + "\\..\\..\\..\\tools\\客户端\\bin\\960x540.bat");
				}
			}

			return pathPack;
		}
		static public string getParticlePath()
		{
			string pathPack = "";

			if (s_projPath != null)
			{
				if (s_particlePath != null && s_particlePath != "")
				{
					pathPack = s_particlePath;
				}
				else
				{
					pathPack = StringDic.getRealPath(s_projPath + "\\..\\..\\texiao\\");
				}
			}

			return pathPack;
		}
		static public string getLangPath()
		{
			string pathPack = "";

			if (s_projPath != null)
			{
				if (s_langPath != null && s_langPath != "")
				{
					pathPack = s_langPath;
				}
				else
				{
					pathPack = StringDic.getRealPath(s_projPath + "\\..\\..\\words\\Language\\free\\Language.xml");
				}
			}

			return pathPack;
		}
		static public string getBackgroundPath()
		{
			string pathPack = "";

			if (s_projPath != null)
			{
				if (s_backgroundPath != null && s_backgroundPath != "")
				{
					pathPack = s_backgroundPath;
				}
				else
				{
					pathPack = "";
				}
			}

			return pathPack;
		}

		static private void openLinkFile(string pathPack, bool isBlock = false)
		{
			if (File.Exists(pathPack))
			{
				FileInfo fiPack = new FileInfo(pathPack);
				System.Diagnostics.Process prcPack = new System.Diagnostics.Process();

				prcPack.StartInfo.WorkingDirectory = fiPack.Directory.FullName;
				prcPack.StartInfo.FileName = fiPack.FullName;
				prcPack.StartInfo.CreateNoWindow = false;
				prcPack.Start();
				if (isBlock)
				{
					prcPack.WaitForExit();
				}
			}
		}
		static public void packUiMod()
		{
			if (s_projPath != null)
			{
				string pathPack = getUiPackPath();

				openLinkFile(pathPack);
			}
		}
		static public void packScriptMod()
		{
			if (s_projPath != null)
			{
				string pathPack = getScriptPackPath();

				openLinkFile(pathPack);
			}
		}
		static public void openGame()
		{
			if (s_projPath != null)
			{
				string pathPack = getGamePath();

				openLinkFile(pathPack);
			}
		}

		static public string openSelectFolderBox(string curPath = null)
		{
			if (curPath == null)
			{
				curPath = s_projPath;
			}

			System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
			folderDialog.Tag = "选择文件";
			folderDialog.SelectedPath = curPath;
			System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				return null;
			}
			return folderDialog.SelectedPath;
		}
		static public string openSelectFileBox(string filter = null, string curPath = null)
		{
			if (curPath == null)
			{
				curPath = s_projPath;
			}

			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Title = "选择文件";
			if (filter == null)
			{
				openFileDialog.Filter = "所有文件|*.*";
			}
			else
			{
				openFileDialog.Filter = filter;
			}
			openFileDialog.FileName = string.Empty;
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.DefaultExt = "*";
			openFileDialog.InitialDirectory = curPath;
			System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				return null;
			}
			return openFileDialog.FileName;
		}

		static public string getSkinGroupPathByName(string skinGroupName)
		{
			if(skinGroupName != null && skinGroupName != "")
			{
				string path = s_skinPath + "\\" + skinGroupName + ".xml";

				return StringDic.getRealPath(path);
			}
			else
			{
				return null;
			}
		}
		static public void refreshSkinIndex()
		{
			if (s_mapSkinIndex != null)
			{
				s_mapSkinIndex.Clear();
			}
			else
			{
				s_mapSkinIndex = new Dictionary<string, List<string>>();
			}

			if (Directory.Exists(Project.Setting.s_skinPath))
			{
				DirectoryInfo dri = new DirectoryInfo(Project.Setting.s_skinPath);

				foreach (FileInfo fi in dri.GetFiles("*.xml"))
				{
					XmlDocument docXml = new XmlDocument();

					docXml.Load(fi.FullName);
					if (docXml.DocumentElement.Name == "BoloUI")
					{
						foreach (XmlNode xnSkin in docXml.DocumentElement.SelectNodes("skin"))
						{
							if (xnSkin is XmlElement)
							{
								XmlElement xeSkin = (XmlElement)xnSkin;
								string skinName = xeSkin.GetAttribute("Name");

								if (skinName != "")
								{
									List<string> lstGroupName;

									if (s_mapSkinIndex.TryGetValue(skinName, out lstGroupName) && lstGroupName != null)
									{
										lstGroupName.Add(System.IO.Path.GetFileNameWithoutExtension(fi.Name));
									}
									else
									{
										s_mapSkinIndex[skinName] = new List<string>();
										s_mapSkinIndex[skinName].Add(System.IO.Path.GetFileNameWithoutExtension(fi.Name));
									}
								}
							}
						}
					}
				}
			}
		}
		//找到这个xml元素下的所有[attrName]属性
		static public void findAttrFromXeAndInsertToDic(XmlElement xe, Dictionary<string, int> mapAttrCount, string attrName = "text")
		{
			string attrValue = xe.GetAttribute(attrName);

			if(attrValue != "")
			{
				int count;

				if(mapAttrCount.TryGetValue(attrValue, out count))
				{
					mapAttrCount[attrValue] = mapAttrCount[attrValue] + 1;
				}
				else
				{
					mapAttrCount[attrValue] = 1;
				}
			}

			foreach(XmlNode xnChild in xe.ChildNodes)
			{
				if(xnChild is XmlElement)
				{
					findAttrFromXeAndInsertToDic((XmlElement)xnChild, mapAttrCount, attrName);
				}
			}
		}
		//找到所有这个目录下所有xml文件的所有xml元素的[attrName]属性
		static public void findAttrFromFolderAndInsertToDic(string path, Dictionary<string, int> mapAttrCount, string attrName = "text")
		{
			if(Directory.Exists(path))
			{
				DirectoryInfo dri = new DirectoryInfo(path);

				foreach(FileInfo fi in dri.GetFiles())
				{
					if(fi.Extension == ".xml")
					{
						XmlDocument docXml = new XmlDocument();

						try
						{
							docXml.Load(fi.FullName);
						}
						catch
						{
							continue;
						}
						if (docXml.DocumentElement != null && docXml.DocumentElement.Name == "BoloUI")
						{
							findAttrFromXeAndInsertToDic(docXml.DocumentElement, mapAttrCount, attrName);
							//Public.ResultLink.createResult("\r\n" + fi.FullName);
						}
					}
				}
			}
		}
		static public void exportLanguageSettingLog()
		{
			string langPath = Setting.getLangPath();

			//Public.ResultLink.createResult("\r\n开始导出Lang");
			if (File.Exists(langPath))
			{
				XmlDocument docLang = new XmlDocument();

				try
				{
					docLang.Load(langPath);
				}
				catch
				{
					return;
				}

				XmlNode xnIndex = docLang.DocumentElement.SelectSingleNode("index");
				Dictionary<string, int> mapLangIndex = new Dictionary<string, int>();
				int max = 1;
				string retString = "";

				if (xnIndex != null)
				{
					foreach (XmlNode xn in xnIndex.ChildNodes)
					{
						int indexLang = 0;

						if (int.TryParse(xn.InnerText, out indexLang))
						{
							mapLangIndex[xn.Name] = indexLang;
							max = max > indexLang ? max : indexLang;
						}
					}
				}
				mapLangIndex["id"] = 0;

				Dictionary<string, Dictionary<int, string>> mapIdLangRowMap = new Dictionary<string, Dictionary<int, string>>();

				foreach (XmlNode xnString in docLang.DocumentElement.SelectNodes("string"))
				{
					XmlNode xnId = xnString.SelectSingleNode("id");
					string strId = xnId.InnerText;

					mapIdLangRowMap[strId] = new Dictionary<int, string>();
					foreach (XmlNode xnRow in xnString.ChildNodes)
					{
						int index;

						if (mapLangIndex.TryGetValue(xnRow.Name, out index))
						{
							mapIdLangRowMap[strId][index] = xnRow.InnerText;
						}
					}
				}

				Dictionary<string, int> mapTextCount = new Dictionary<string, int>();

				findAttrFromFolderAndInsertToDic(Setting.s_projPath, mapTextCount, "text");

				foreach(KeyValuePair<string, int> pairTextCount in mapTextCount)
				{
					Dictionary<int, string> mapLangIndexRow;

					retString += pairTextCount.Key;

					if (mapIdLangRowMap.TryGetValue(pairTextCount.Key, out mapLangIndexRow))
					{
						for (int i = 1; i <= max; i++)
						{
							string rowValue;

							if (mapLangIndexRow.TryGetValue(i, out rowValue) && rowValue != null)
							{
								retString += "|" + rowValue;
							}
							else
							{
								retString += "|";
							}
						}
					}
					else
					{
						retString += "|";
					}

					retString += "\r\n";
				}

				StreamWriter sw;

				sw = File.CreateText(Setting.s_projPath + "\\outputText.txt");
				sw.WriteLine(retString);
				sw.Close();
			}
			Public.ResultLink.createResult("\r\n导出Lang结束");
		}
	}
}
