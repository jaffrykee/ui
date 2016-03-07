using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace UIEditor.Project.PlugIn
{
	public class DataNodesDef
	{
		//para
		//index0	rootName
		//index1	partName
		//node1		配置文件
		static public Dictionary<string, Dictionary<string, DataNodesDef>> s_mapDataNodesDef;

		public string m_path;
		public XmlDocument m_docXml;
		public Dictionary<string, DataNode> m_mapDataNode;
		public Dictionary<string, HashSet<string>> m_mapNodeClass;
		public string m_rootName;
		public string m_partName;

		public DataNodesDef(string path)
		{
			m_docXml = new XmlDocument();

			if(path != "" && path != null && File.Exists(path))
			{
				m_path = path;
				try
				{
					m_docXml.Load(path);
				}
				catch
				{
					return;
				}
				initData();
			}
		}

		private void parseNodeClass(XmlElement xeNodeClass)
		{
			if(m_mapNodeClass != null)
			{
				string keyName = xeNodeClass.GetAttribute("key");

				if(keyName != "")
				{
					HashSet<string> hlstChildNode = new HashSet<string>();

					foreach(XmlNode xn in xeNodeClass)
					{
						if(xn.Name == "childNode" && xn is XmlElement)
						{
							string childKey = ((XmlElement)xn).GetAttribute("key");

							if (childKey != "" && !hlstChildNode.Contains(childKey))
							{
								hlstChildNode.Add(childKey);
							}
						}
					}
					m_mapNodeClass[keyName] = hlstChildNode;
				}
			}
		}
		private void parseNode(XmlElement xeNode)
		{
			if(m_mapDataNode != null)
			{
				string keyName = xeNode.GetAttribute("key");

				if (keyName != "")
				{
					DataNode dataNode = new DataNode(this, keyName);
					HashSet<string> hlstChildNode = new HashSet<string>();

					foreach (XmlNode xn in xeNode.SelectNodes("childNode"))
					{
						if (xn is XmlElement)
						{
							string childKey = ((XmlElement)xn).GetAttribute("key");

							if (childKey != "" && !hlstChildNode.Contains(childKey))
							{
								hlstChildNode.Add(childKey);
							}
						}
					}

					HashSet<string> hlstClassName = new HashSet<string>();

					foreach (XmlNode xnClass in xeNode.SelectSingleNode("classGroup").SelectNodes("inludeClass"))
					{
						if (xnClass is XmlElement)
						{
							string childKey = ((XmlElement)xnClass).GetAttribute("key");

							if (childKey != "" && !hlstClassName.Contains(childKey))
							{
								hlstClassName.Add(childKey);

								HashSet<string> hlstClass;

								if(m_mapNodeClass.TryGetValue(childKey, out hlstClass))
								{
									hlstChildNode.Union(hlstClass);
								}
							}
						}
					}
					dataNode.m_hlstChildNode = hlstChildNode;
					m_mapDataNode[keyName] = dataNode;
				}
			}
		}
		private void loadData()
		{
			if(m_docXml != null)
			{
				XmlElement roootNode = m_docXml.DocumentElement;


				foreach (XmlNode xn in roootNode.SelectNodes("nodeClass"))
				{
					if(xn is XmlElement)
					{
						parseNodeClass((XmlElement)xn);
					}
				}
				foreach (XmlNode xn in roootNode.SelectNodes("node"))
				{
					if (xn is XmlElement)
					{
						parseNode((XmlElement)xn);
					}
				}
			}
		}
		public void initData()
		{
			if (m_docXml != null)
			{
				XmlElement rootNode = m_docXml.DocumentElement;

				if (rootNode != null && rootNode.Name == "DataNodes")
				{
					m_rootName = rootNode.GetAttribute("rootName");
					m_partName = rootNode.GetAttribute("partName");
					if (m_rootName != "" && m_partName != "")
					{
						if(s_mapDataNodesDef == null)
						{
							s_mapDataNodesDef = new Dictionary<string, Dictionary<string, DataNodesDef>>();
						}

						Dictionary<string, DataNodesDef> mapRootNode;

						if (!s_mapDataNodesDef.TryGetValue(m_rootName, out mapRootNode) || mapRootNode == null)
						{
							mapRootNode = new Dictionary<string, DataNodesDef>();
							s_mapDataNodesDef[m_rootName] = mapRootNode;
						}
						mapRootNode[m_partName] = this;
						m_mapDataNode = new Dictionary<string, DataNode>();
						m_mapNodeClass = new Dictionary<string, HashSet<string>>();

						loadData();
					}
				}
			}
		}
	}
}
