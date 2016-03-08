using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace UIEditor.Project.PlugIn
{
	/// <summary>
	/// 所有可扩展xml数据文件的数据结构基类。
	/// </summary>
	public class DataNodesDef
	{
		/// <summary>
		/// 当前能够获取到的所有可扩展数据结构。
		/// para
		/// index0	rootName，代表xml文档根节点的Name。
		/// index1	partName，代表文档解析组的名字。
		/// node1	配置节点，表示文档解析组的配置实例。
		/// </summary>
		static public Dictionary<string, Dictionary<string, DataNodesDef>> s_mapDataNodesDef;
		static public string getDataConfigFullPath(string mod)
		{

		}

		public string m_path;
		public XmlDocument m_docXml;
		/// <summary>
		/// 本文档解析组所包含的xml节点解析实例的索引。
		/// </summary>
		public Dictionary<string, DataNode> m_mapDataNode;
		/// <summary>
		/// 本文档解析组所包含的自定义类别词典的索引。
		/// </summary>
		public Dictionary<string, HashSet<string>> m_mapNodeClass;
		/// <summary>
		/// xml文档根节点的Name。
		/// </summary>
		public string m_rootName;
		/// <summary>
		/// 文档解析组的Name。
		/// </summary>
		public string m_partName;

		/// <summary>
		/// 通过xml文件路径构造。
		/// </summary>
		/// <param name="path">xml文件的路径</param>
		public DataNodesDef(string modeName, string subDefName)
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

		/// <summary>
		/// 通过xml文档，解析出自定义类别词典。
		/// </summary>
		/// <param name="xeNodeClass">包含有一个自定义类别词典的XmlElement。</param>
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
		/// <summary>
		/// 通过xml文档，解析出xml节点解析实例。
		/// </summary>
		/// <param name="xeNode">包含有一个xml节点解析实例的XmlElement。</param>
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
		/// <summary>
		/// 读取xml文档，以解析数据。
		/// </summary>
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
		/// <summary>
		/// 初始化数据。
		/// </summary>
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
