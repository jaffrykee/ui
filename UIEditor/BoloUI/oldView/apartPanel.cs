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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace UIEditor.BoloUI
{
	public class apartPanel : Basic
	{
		public apartPanel(XmlElement xe, XmlControl rootControl, Canvas parentCanvas)
			: base(xe, rootControl, parentCanvas)
		{
		}

		override protected void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			initHeader();
			addChild();
			
			int i = 0;
			double widthCount = 0;
			Basic lastChild = new Basic();
			foreach (Basic treeChild in this.Items)
			{
				if (treeChild.m_curCanvas != null)
				{
					widthCount += treeChild.m_curCanvas.Width;
					lastChild = treeChild;
				}
				i++;
			}
			if (lastChild.m_curCanvas != null)
			{
				widthCount -= lastChild.m_curCanvas.Width;
				lastChild.m_curCanvas.Width = m_curCanvas.Width - widthCount;
			}
		}
	}
}