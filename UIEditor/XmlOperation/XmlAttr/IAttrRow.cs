using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;

namespace UIEditor.XmlOperation.XmlAttr
{
	public interface IAttrRow
	{
		AttrList m_parent { get; set; }
		bool m_isCommon { get; set; }
		string m_subType { get; set; }

		string m_name { get; set; }
		string m_preValue { get; set; }
		string m_value { get; set; }
		string m_type { get; set; }
		string m_defValue { get; set; }
	}
}
