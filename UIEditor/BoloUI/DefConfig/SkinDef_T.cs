using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UIEditor.XmlOperation.XmlAttr;
using UIEditor.Project.PlugIn;

namespace UIEditor.BoloUI.DefConfig
{
	public class SkinDef_T : Project.PlugIn.DataNode
	{
		static public bool tryGetSkinDef(string nodeName, out SkinDef_T skinDef)
		{
			DataNode nodeDef;

			skinDef = null;
			if (DataNodeGroup.tryGetDataNode("BoloUI", "Ctrl", nodeName, out nodeDef) && nodeDef != null && nodeDef is SkinDef_T)
			{
				skinDef = (SkinDef_T)nodeDef;

				return true;
			}

			return false;
		}

		public SkinDef_T(DataNodeGroup parent, string name)
			: base(parent, name)
		{
		}
	}
}
