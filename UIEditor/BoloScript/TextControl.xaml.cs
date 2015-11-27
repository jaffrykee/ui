using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace UIEditor.BoloScript
{
	public partial class TextControl : UserControl
	{
		public FileTabItem m_parent;
		public OpenedFile m_openedFile;

		public TextControl(FileTabItem parent, OpenedFile fileDef)
		{
			InitializeComponent();
			m_parent = parent;
			m_openedFile = fileDef;
			m_openedFile.m_frame = this;

			if (System.IO.File.Exists(m_parent.m_filePath))
			{
				Run run = new Run();

				try
				{
					FileStream aFile = new FileStream(m_parent.m_filePath, FileMode.Open);
					StreamReader reader = new StreamReader(aFile, Encoding.Default);

					run.Text = reader.ReadToEnd();
					mx_textPara.Inlines.Add(run);
				}
				catch (IOException ex)
				{
					Public.ResultLink.createResult("\r\n打开文件失败。(" + ex + ")", Public.ResultType.RT_ERROR);
				}
			}
		}
	}
}
