using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace UIEditor
{
	/// <summary>
	/// TemplateCreate.xaml 的交互逻辑
	/// </summary>
	public partial class TemplateCreate
	{
		public static TemplateCreate s_pW;

		XmlElement m_xe;
		string m_strTmpl;

		public TemplateCreate(XmlElement xe)
		{
			m_xe = xe;
			InitializeComponent();
			this.Owner = MainWindow.s_pW;
			s_pW = this;
		}

		private void createTmpl(XmlElement xeConfig)
		{
			if (xeConfig.SelectSingleNode("template") == null)
			{
				xeConfig.AppendChild(xeConfig.OwnerDocument.CreateElement("template"));
			}
			XmlElement xeTmpl = (XmlElement)xeConfig.SelectSingleNode("template");
			if (xeTmpl.SelectSingleNode(m_xe.Name + "Tmpls") == null)
			{
				xeTmpl.AppendChild(xeTmpl.OwnerDocument.CreateElement(m_xe.Name + "Tmpls"));
			}
			XmlElement xeCtrlTmpl = (XmlElement)xeTmpl.SelectSingleNode(m_xe.Name + "Tmpls");
			XmlElement xeRow = xeCtrlTmpl.OwnerDocument.CreateElement("row");
			xeCtrlTmpl.AppendChild(xeRow);
			xeRow.SetAttribute("name", mx_tmplName.Text.ToString());
			xeRow.InnerXml = m_strTmpl;
		}
		private void mx_ok_Click(object sender, RoutedEventArgs e)
		{
			if (mx_tmplName.Text.ToString() != "")
			{
				if (mx_rootChild.IsChecked == true)
				{
					m_strTmpl = m_xe.OuterXml;
				}
				else
				{
					if (m_xe.NodeType == XmlNodeType.Element)
					{
						XmlElement xetmp = (XmlElement)m_xe.CloneNode(true);

						xetmp.InnerXml = "";
						m_strTmpl = xetmp.OuterXml;
					}
				}
				if (mx_pathAll.IsChecked == true)
				{
					if (MainWindow.s_pW.m_docConf.SelectSingleNode("Config").NodeType == XmlNodeType.Element)
					{
						XmlElement xeConfig = (XmlElement)MainWindow.s_pW.m_docConf.SelectSingleNode("Config");

						createTmpl(xeConfig);
						xeConfig.OwnerDocument.Save(MainWindow.conf_pathConf);
					}
				}
				else if (mx_pathProj.IsChecked == true)
				{
					if (MainWindow.s_pW.m_docProj.SelectSingleNode("BoloUIProj").NodeType == XmlNodeType.Element)
					{
						XmlElement xeConfig = (XmlElement)MainWindow.s_pW.m_docProj.SelectSingleNode("BoloUIProj");

						createTmpl(xeConfig);
						xeConfig.OwnerDocument.Save(MainWindow.s_pW.m_projPath + "\\" + MainWindow.s_pW.m_projName);
					}
				}
				this.Close();
			}
		}
		private void mx_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
