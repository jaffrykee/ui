using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace UIEditor
{
	public class StringDic
	{
		public string m_curLang;
		public string m_pathDic;
		public Dictionary<string, Dictionary<string, Dictionary<string, string>>> m_mapStrDic;

		public const string conf_ctrlTipDic = "CtrlTip";
		public const string conf_ctrlAttrTipDic = "CtrlAttrTip";
	

		public StringDic(string lang, string pathDic)
		{
			m_curLang = lang;
			m_pathDic = pathDic;
			refreshStrDic();
		}

		public void loadStrDicByPath(string path)
		{

			XmlDocument docDic = new XmlDocument();

			docDic.Load(path);
			if (docDic.DocumentElement.Name == "DicRoot")
			{
				foreach (XmlNode xn in docDic.DocumentElement.ChildNodes)
				{
					if (xn.NodeType == XmlNodeType.Element)
					{
						XmlElement xe = (XmlElement)xn;

						switch (xe.Name)
						{
							case "DefDic":
								{
									foreach (XmlNode xnRow in xe.ChildNodes)
									{
										if (xnRow.NodeType == XmlNodeType.Element)
										{
											XmlElement xeRow = (XmlElement)xnRow;

											addWordRowToDic(xeRow, m_mapStrDic["DefDic"]);
										}
									}
								}
								break;
							case "SubDic":
								{
									string keyDic = xe.GetAttribute("key");
									Dictionary<string, Dictionary<string, string>> subDic;

									if (keyDic != "")
									{
										if (!m_mapStrDic.TryGetValue(keyDic, out subDic))
										{
											subDic = new Dictionary<string, Dictionary<string, string>>();

											m_mapStrDic.Add(keyDic, subDic);
										}
										foreach (XmlNode xnRow in xe.ChildNodes)
										{
											if (xnRow.NodeType == XmlNodeType.Element)
											{
												XmlElement xeRow = (XmlElement)xnRow;

												addWordRowToDic(xeRow, subDic);
											}
										}
									}
								}
								break;
							default:
								break;
						}
					}
				}
			}
		}
		public static void addWordRowToDic(XmlElement xeRow, Dictionary<string, Dictionary<string, string>> mapDic)
		{
			if (xeRow.Name == "row" && xeRow.GetAttribute("key") != "")
			{
				string rowKey = xeRow.GetAttribute("key");
				Dictionary<string, string> mapTmp;

				if (!mapDic.TryGetValue(rowKey, out mapTmp))
				{
					mapDic.Add(rowKey, new Dictionary<string, string>());
					foreach (XmlNode xnLang in xeRow.ChildNodes)
					{
						if (xnLang.NodeType == XmlNodeType.Element)
						{
							XmlElement xeLang = (XmlElement)xnLang;
							string strLang;

							if (!mapDic[rowKey].TryGetValue(xeLang.Name, out strLang))
							{
								mapDic[rowKey].Add(xeLang.Name, xeLang.InnerText);
							}
						}
					}
				}
			}
		}
		public void refreshStrDic()
		{
			string dicPath = m_pathDic + m_curLang;
			DirectoryInfo di = new DirectoryInfo(dicPath);
			FileInfo[] arrFi = di.GetFiles("*.xml");

			m_mapStrDic = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

			m_mapStrDic.Add("DefDic", new Dictionary<string, Dictionary<string, string>>());
			foreach (FileInfo fi in arrFi)
			{
				#region 遍历每一个子词典
				loadStrDicByPath(fi.FullName);
				#endregion
			}
		}
		public string getWordByKey(string key, string dicName = "DefDic")
		{
			Dictionary<string, Dictionary<string, string>> dicData;
			Dictionary<string, string> mapLang;
			string retStr;

			if (dicName != null && dicName != "" &&
				m_mapStrDic.TryGetValue(dicName, out dicData) &&
				dicData.TryGetValue(key, out mapLang) &&
				mapLang.TryGetValue(m_curLang, out retStr))
			{
				return retStr;
			}

			return "";
		}
		public bool setWordByKey(string key, string newValue, string dicName = "DefDic")
		{
			Dictionary<string, Dictionary<string, string>> dicData;
			Dictionary<string, string> mapLang;

			if (dicName != null && dicName != "" && m_mapStrDic.TryGetValue(dicName, out dicData))
			{
				if (dicData.TryGetValue(key, out mapLang))
				{
					mapLang[m_curLang] = newValue;
				}
				else
				{
					dicData[key] = new Dictionary<string, string>();
					dicData[key][m_curLang] = newValue;
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public void getNameAndTip(MenuItem menuItem, string tipDic, string key, string nameDic = "DefDic")
		{
			string strName;

			strName = getWordByKey(key, nameDic);
			if (strName != "")
			{
				menuItem.Header = strName;
			}
			else
			{
				menuItem.Header = key;
			}

			strName = getWordByKey(key, tipDic);
			if (strName != "")
			{
				menuItem.ToolTip = strName;
			}
			else
			{
				menuItem.ToolTip = key;
			}
		}
		public static string getRandString(int pwdlen = 16, string pwdchars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
		{
			string tmpstr = "";
			int iRandNum;
			Random rnd = new Random();

			for (int i = 0; i < pwdlen; i++)
			{
				iRandNum = rnd.Next(pwdchars.Length);
				tmpstr += pwdchars[iRandNum];
			}
			return tmpstr;
		}

		public static string getFileType(string filePath)
		{
			return filePath.Substring(filePath.LastIndexOf(".") + 1, (filePath.Length - filePath.LastIndexOf(".") - 1));
		}
		public static string getFileNameWithoutPath(string filePath)
		{
			return filePath.Substring(filePath.LastIndexOf("\\") + 1, (filePath.Length - filePath.LastIndexOf("\\") - 1));
		}
		public static string getFileNameWithoutType(string filePath)
		{
			string fileName = getFileNameWithoutPath(filePath);

			return fileName.Substring(0, fileName.LastIndexOf("."));
		}

		public static int getFirstDiffOffset(string str1, string str2)
		{
			int minLen = (str1.Length < str2.Length) ? str1.Length : str2.Length;

			for (int i = 0; i < minLen; i++)
			{
				if(str1[i] != str2[i])
				{
					return i;
				}
			}

			return minLen;
		}

		static public string getRealPath(string path)
		{
			if(File.Exists(path))
			{
				FileInfo fi = new FileInfo(path);

				return fi.FullName;
			}
			else if(Directory.Exists(path))
			{
				DirectoryInfo dri = new DirectoryInfo(path);

				return dri.FullName;
			}

			return path;
		}
	}
}
