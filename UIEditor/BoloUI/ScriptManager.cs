using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIEditor.Project;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace UIEditor.BoloUI
{
	public class ScriptManager
	{
		const string sc_ssueScriptSrcPath = @".\data\PlugIn\BoloUI\SSUEScript\";

		public static bool copySSUEScriptToProject()
		{
			string dstPath = Setting.s_projPath + "\\..\\..\\";

			if(Directory.Exists(sc_ssueScriptSrcPath) && Directory.Exists(dstPath))
			{
				DirectoryInfo srcInfo = new DirectoryInfo(sc_ssueScriptSrcPath);

				Setting.copyFolderToFolder(sc_ssueScriptSrcPath, dstPath);

				return true;
			}

			return false;
		}
		//	/\*([ \f\n\r\t\va-zA-Z0-9<>/\u4E00-\u9FA5="]*)\*/
		//  to
		//  /\\*([ \\f\\n\\r\\t\\va-zA-Z0-9<>/\\u4E00-\\u9FA5=\"]*)\\*/
		public static XmlDocument getXmlDocFromScriptComments(string scriptPath)
		{
			StreamReader sr = new StreamReader(scriptPath, Encoding.Default);
			string strFileData;

			strFileData = sr.ReadToEnd();
			sr.Close();

			Match matchComment = Regex.Match(strFileData, "/\\*([ \\f\\n\\r\\t\\va-zA-Z0-9<>/\\u4E00-\\u9FA5=\"]*)\\*/");

			if(matchComment.Success)
			{
				string strXmlComments = matchComment.Groups[1].Value;
				XmlDocument docComments = new XmlDocument();

				try
				{
					docComments.LoadXml(strXmlComments);
				}
				catch
				{
					return null;
				}

				return docComments;
			}

			return null;
		}
		public static Dictionary<string, Dictionary<string, XmlDocument>> getSSUEScriptClassMap()
		{
			string dstPath = Setting.getScriptPath() + @"source\";
			Dictionary<string, Dictionary<string, XmlDocument>> mapScriptClass = new Dictionary<string, Dictionary<string, XmlDocument>>();

			if(Directory.Exists(dstPath))
			{
				DirectoryInfo di = new DirectoryInfo(dstPath);

				foreach(FileInfo fi in di.GetFiles("SSUE*.bolos"))
				{
					XmlDocument docScript = getXmlDocFromScriptComments(fi.FullName);

					if(docScript != null)
					{
						string className = docScript.DocumentElement.GetAttribute("class");
						string scriptName = Path.GetFileNameWithoutExtension(fi.Name);
						string scriptTip = docScript.DocumentElement.GetAttribute("name");

						if(className != "" && scriptName != "" && scriptName != null && scriptTip != "")
						{
							Dictionary<string, XmlDocument> mapScript;

							if(mapScriptClass.TryGetValue(className, out mapScript) && mapScript != null)
							{
								mapScript[scriptName] = docScript;
							}
							else
							{
								mapScriptClass[className] = new Dictionary<string, XmlDocument>();
								mapScriptClass[className][scriptName] = docScript;
							}
						}
					}
				}
			}

			return mapScriptClass;
		}
	}
}
