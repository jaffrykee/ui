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
	public class DataNodeGroup
	{
		/// <summary>
		/// 当前能够获取到的所有可扩展数据结构。
		/// para
		/// index0	rootName，代表xml文档根节点的Name。
		/// index1	partName，代表文档解析组的名字。
		/// node1	配置节点，表示文档解析组的配置实例。
		/// </summary>
		static public Dictionary<string, Dictionary<string, DataNodeGroup>> s_mapDataNodesDef;
		static public string getDataConfigFullPath(string modName, string subName)
		{
			return Setting.conf_plugInPath + modName + "\\" + subName + Setting.conf_nodeDefSubFileExt + Setting.conf_plugInDefExt;
		}
		/// <summary>
		/// 尝试读取配置文件。
		/// </summary>
		/// <param name="path">配置文件路径。</param>
		/// <param name="xmlDoc">读取后的XmlDocument实例。</param>
		/// <returns>
		/// 返回：成功-true；失败-false。
		/// </returns>
		static public bool tryLoadConfigDataFile(string path, out XmlDocument xmlDoc)
		{
			xmlDoc = new XmlDocument();

			if (path != "" && path != null && File.Exists(path))
			{
				try
				{
					xmlDoc.Load(path);
				}
				catch
				{
					//<inc>这里的连接要显示为跳转到所在目录的形式。
					Public.ResultLink.createResult("\r\n配置文件格式错误或无法读取，请检查UI编辑器目录下相对路径：\"" + path + "\"的配置文件。",
						Public.ResultType.RT_ERROR, null, true);

					return false;
				}
			}
			else
			{
				Public.ResultLink.createResult("\r\n配置文件不存在，请检查UI编辑器目录下相对路径：\"" + path + "\"的配置文件。",
					Public.ResultType.RT_ERROR, null, true);

				return false;
			}

			return true;
		}
		static public bool tryGetDataNodeGroup(string modName, string partName, out DataNodeGroup nodeGroup)
		{
			Dictionary<string , DataNodeGroup> mapNodeGroup;

			nodeGroup = null;
			if (s_mapDataNodesDef.TryGetValue(modName, out mapNodeGroup) && mapNodeGroup != null)
			{
				if (mapNodeGroup.TryGetValue(partName, out nodeGroup))
				{
					return true;
				}
			}

			return false;
		}
		static public bool tryGetDataNode(string modName, string partName, string nodeName, out DataNode dataNode)
		{
			DataNodeGroup nodeGroup;

			dataNode = null;
			if (tryGetDataNodeGroup(modName, partName, out nodeGroup) && nodeGroup != null &&
				nodeGroup.m_mapDataNode.TryGetValue(nodeName, out dataNode))
			{
				return true;
			}

			return false;
		}
		static public bool tryGetDataNode(string modName, string nodeName, out DataNode dataNode)
		{
			Dictionary<string, DataNodeGroup> mapNodeGroup;

			dataNode = null;
			if(s_mapDataNodesDef.TryGetValue(modName, out mapNodeGroup) && mapNodeGroup != null)
			{
				foreach(KeyValuePair<string, DataNodeGroup> pairNodeGroup in mapNodeGroup.ToList())
				{
					if(pairNodeGroup.Value != null && pairNodeGroup.Value.m_mapDataNode.TryGetValue(nodeName, out dataNode))
					{
						return true;
					}
				}
			}

			return false;
		}

		#region nodeDef属性
		public string m_pathNodeDef;
		public XmlDocument m_nodeDefDocXml;
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
		#endregion
		#region attrDef属性
		public string m_pathAttrDef;
		public XmlDocument m_attrDefDocXml;
		public Dictionary<string, DataAttrGroup> m_mapAttrGroup;
		#endregion

		/// <summary>
		/// 通过xml文件路径构造。
		/// </summary>
		/// <param name="path">xml文件的路径</param>
		public DataNodeGroup(string modName, string subName)
		{
			m_mapDataNode = new Dictionary<string, DataNode>();
			m_mapNodeClass = new Dictionary<string, HashSet<string>>();
			m_mapAttrGroup = new Dictionary<string, DataAttrGroup>();

			m_pathNodeDef = getDataConfigFullPath(modName, subName);
			m_pathAttrDef = DataAttrGroup.getDataConfigFullPath(modName, subName);
			if(tryLoadConfigDataFile(m_pathNodeDef, out m_nodeDefDocXml) && m_nodeDefDocXml != null &&
				tryLoadConfigDataFile(m_pathAttrDef, out m_attrDefDocXml) && m_attrDefDocXml != null)
			{
				if(!initData())
				{
					Public.ResultLink.createResult("\r\n插件配置文件格式错误，请检查路径：\"" + m_pathNodeDef + "\"和\"" + m_pathAttrDef + "\"。");
				}
			}
		}

		public System.Type getDataNodeType()
		{
			System.Type retType = System.Type.GetType("UIEditor." + m_rootName + ".DefConfig." + m_partName + "Def_T");

			if(retType == null)
			{
				retType = System.Type.GetType("UIEditor.Project.PlugIn.DataNode");
			}

			return retType;
		}
		/// <summary>
		/// 通过xml文档，解析出自定义类别词典。
		/// </summary>
		/// <param name="xeNodeClass">包含有一个自定义类别词典的XmlElement。</param>
		private bool parseNodeClass(XmlElement rootNode)
		{
			if (rootNode == null)
			{
				return false;
			}
			foreach (XmlNode xnNodeClass in rootNode.SelectNodes("nodeClass"))
			{
				if (xnNodeClass is XmlElement)
				{
					XmlElement xeNodeClass = (XmlElement)xnNodeClass;

					if (m_mapNodeClass != null)
					{
						string keyName = xeNodeClass.GetAttribute("key");

						if (keyName != "")
						{
							HashSet<string> hlstChildNode = new HashSet<string>();

							foreach (XmlNode xn in xeNodeClass)
							{
								if (xn.Name == "childNode" && xn is XmlElement)
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
			}

			return true;
		}
		/// <summary>
		/// 通过xml文档，解析出xml节点解析实例。
		/// </summary>
		/// <param name="xeNode">包含有一个xml节点解析实例的XmlElement。</param>
		private bool parseNode(XmlElement rootNode)
		{
			if (rootNode == null)
			{
				return false;
			}
			foreach (XmlNode xnNode in rootNode.SelectNodes("node"))
			{
				if (xnNode is XmlElement)
				{
					XmlElement xeNode = (XmlElement)xnNode;

					if (m_mapDataNode != null)
					{
						string keyName = xeNode.GetAttribute("key");

						if (keyName != "")
						{
							DataNode dataNode = (DataNode)System.Activator.CreateInstance(getDataNodeType(), new object[] { this, keyName });
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
							XmlNode xnClassGroup = xeNode.SelectSingleNode("classGroup");

							if (xnClassGroup != null)
							{
								foreach (XmlNode xnClass in xnClassGroup.SelectNodes("inludeClass"))
								{
									if (xnClass is XmlElement)
									{
										string childKey = ((XmlElement)xnClass).GetAttribute("key");

										if (childKey != "" && !hlstClassName.Contains(childKey))
										{
											hlstClassName.Add(childKey);

											HashSet<string> hlstClass;

											if (m_mapNodeClass.TryGetValue(childKey, out hlstClass))
											{
												hlstChildNode.UnionWith(hlstClass);
											}
										}
									}
								}
							}
							dataNode.m_hlstChildNode = hlstChildNode;
							dataNode.m_hlstClassName = hlstClassName;
							m_mapDataNode[keyName] = dataNode;
						}
					}
				}
			}

			return true;
		}
		/// <summary>
		/// 读取xml文档，以解析数据。
		/// </summary>
		private bool loadNodeDefData()
		{
			if (m_nodeDefDocXml != null)
			{
				XmlElement rootNode = m_nodeDefDocXml.DocumentElement;

				if (rootNode != null && rootNode.Name == "DataNodeGroup")
				{
					m_rootName = rootNode.GetAttribute("rootName");
					m_partName = rootNode.GetAttribute("partName");
					if (m_rootName != "" && m_partName != "")
					{
						if (s_mapDataNodesDef == null)
						{
							s_mapDataNodesDef = new Dictionary<string, Dictionary<string, DataNodeGroup>>();
						}

						Dictionary<string, DataNodeGroup> mapRootNode;

						if (!s_mapDataNodesDef.TryGetValue(m_rootName, out mapRootNode) || mapRootNode == null)
						{
							mapRootNode = new Dictionary<string, DataNodeGroup>();
							s_mapDataNodesDef[m_rootName] = mapRootNode;
						}
						mapRootNode[m_partName] = this;
						m_mapDataNode = new Dictionary<string, DataNode>();
						m_mapNodeClass = new Dictionary<string, HashSet<string>>();

						if (parseNodeClass(rootNode) == true && parseNode(rootNode) == true)
						{
							return true;
						}
					}
				}
			}

			return false;
		}
		private bool parseAttrClass(XmlElement rootNode)
		{
			if (rootNode != null && rootNode.Name == "DataAttrGroup")
			{
				foreach (XmlNode xnAttrClass in rootNode.SelectNodes("attrClass"))
				{
					if (xnAttrClass is XmlElement)
					{
						XmlElement xeAttrClass = (XmlElement)xnAttrClass;
						string keyName = xeAttrClass.GetAttribute("key");

						if (keyName != "")
						{
							DataAttrGroup attrGroup = new DataAttrGroup(xeAttrClass);

							m_mapAttrGroup.Add(keyName, attrGroup);
						}
					}
				}

				return true;
			}

			return false;
		}
		private bool parseNodeAttrInclude(XmlElement rootNode)
		{
			if(rootNode == null)
			{
				return false;
			}
			foreach (XmlNode xnAttrInclude in rootNode.SelectNodes("nodeAttrInclude"))
			{
				if(xnAttrInclude is XmlElement)
				{
					XmlElement xeAttrInclude = (XmlElement)xnAttrInclude;
					string keyName = xeAttrInclude.GetAttribute("key");

					if(keyName != "")
					{
						DataNode dataNode;

						if (m_mapDataNode.TryGetValue(keyName, out dataNode) && dataNode != null)
						{
							XmlNode xnClassGroup = xeAttrInclude.SelectSingleNode("classGroup");

							foreach (XmlNode xnClass in xnClassGroup.SelectNodes("inludeClass"))
							{
								if(xnClass is XmlElement)
								{
									XmlElement xeClass = (XmlElement)xnClass;
									string className = xeClass.GetAttribute("key");

									if(className != "")
									{
										DataAttrGroup attrGroup;

										if (m_mapAttrGroup.TryGetValue(className, out attrGroup) && attrGroup != null)
										{
											dataNode.m_mapDataAttrGroup[className] = attrGroup;
										}
									}
								}
							}
						}
					}
				}
			}

			return true;
		}
		private bool loadAttrDefData()
		{
			if(m_attrDefDocXml != null)
			{
				XmlElement rootNode = m_attrDefDocXml.DocumentElement;

				if(parseAttrClass(rootNode) && parseNodeAttrInclude(rootNode))
				{
					return true;
				}
			}

			return false;
		}
		/// <summary>
		/// 初始化数据。
		/// </summary>
		public bool initData()
		{
			if(loadNodeDefData() && loadAttrDefData())
			{
				return true;
			}

			return false;
		}
	}
}
