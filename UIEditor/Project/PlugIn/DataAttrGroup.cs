using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Windows.Controls;
using UIEditor.XmlOperation.XmlAttr;

namespace UIEditor.Project.PlugIn
{
	public class DataAttrGroup
	{
		static public string getDataConfigFullPath(string modName, string subName)
		{
			return Setting.conf_plugInPath + modName + "\\" + subName + Setting.conf_attrDefSubFileExt + Setting.conf_plugInDefExt;
		}

		public string m_className;
		public AttrList m_uiAttrList;
		public Dictionary<string, DataAttr> m_mapDataAttr;

		public DataAttrGroup(XmlElement xeAttrClass)
		{
			m_mapDataAttr = new Dictionary<string, DataAttr>();

			initData(xeAttrClass);
		}
		public bool initData(XmlElement xeAttrClass)
		{
			if(parseClassData(xeAttrClass) == true)
			{
				m_uiAttrList = new AttrList(m_className, this);
				m_uiAttrList.Visibility = System.Windows.Visibility.Collapsed;
				MainWindow.s_pW.mx_toolArea.Items.Add(m_uiAttrList);
			}

			return true;
		}
		public bool parseClassData(XmlElement xeAttrClass)
		{
			if (xeAttrClass != null)
			{
				m_className = xeAttrClass.GetAttribute("key");

				if(m_className == "")
				{
					return false;
				}
				foreach (XmlNode xnAttrDef in xeAttrClass.SelectNodes("attrDef"))
				{
					if (xnAttrDef is XmlElement)
					{
						XmlElement xeAttr = (XmlElement)xnAttrDef;
						DataAttr attrDef = new DataAttr(this);

						if (xeAttr.GetAttribute("type") != "")
						{
							attrDef.m_type = xeAttr.GetAttribute("type");
							if (xeAttr.GetAttribute("type") == "weight")
							{
								attrDef.m_lstWeight = new ArrayList();

								foreach (XmlNode xnWt in xeAttr.ChildNodes)
								{
									if (xnWt is XmlElement)
									{
										switch (xnWt.Name)
										{
											case "rowGroup":
												{
													WeightRowGroup_T wtGroup = new WeightRowGroup_T(((XmlElement)xnWt).GetAttribute("key"));
													attrDef.m_lstWeight.Add(wtGroup);
													foreach (XmlNode xnRow in xnWt.ChildNodes)
													{
														if (xnRow is XmlElement && xnRow.Name == "row")
														{
															wtGroup.m_lstRow.Add(xnRow.InnerText);
														}
													}
												}
												break;
											case "row":
												{
													attrDef.m_lstWeight.Add(xnWt.InnerText);
												}
												break;
											default:
												break;
										}
									}
								}
							}
						}
						else
						{
							attrDef.m_type = "string";
						}
						if (xeAttr.GetAttribute("isEnum") == "true")
						{
							attrDef.m_isEnum = true;
							attrDef.m_mapEnum = new Dictionary<string, ComboBoxItem>();
							foreach (XmlNode xnEnum in xeAttr.ChildNodes)
							{
								if (xnEnum.NodeType == XmlNodeType.Element)
								{
									XmlElement xeEnum = (XmlElement)xnEnum;

									if (xeEnum.Name == "row")
									{
										attrDef.m_mapEnum.Add(xeEnum.InnerText, null);
									}
								}
							}
						}
						else
						{
							attrDef.m_isEnum = false;
						}
						if (xeAttr.GetAttribute("isCommon") == "true")
						{
							attrDef.m_isCommon = true;
						}
						else
						{
							attrDef.m_isCommon = false;
						}
						attrDef.m_subType = xeAttr.GetAttribute("subType");
						attrDef.m_defValue = xeAttr.GetAttribute("defValue");

						string keyName = xeAttr.GetAttribute("key");

						if(keyName != "")
						{
							m_mapDataAttr.Add(keyName, attrDef);
						}
					}
				}
			}

			return true;
		}
	}
}
