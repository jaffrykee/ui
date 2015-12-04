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
		public string m_lastText;

		public TextControl(FileTabItem parent, OpenedFile fileDef)
		{
			InitializeComponent();
			m_parent = parent;
			m_openedFile = fileDef;
			m_openedFile.m_frame = this;
			TextBox tb = new TextBox();
			RichTextBox rtb = new RichTextBox();

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

		private void mx_rtbText_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Tab && sender is RichTextBox)
			{
				RichTextBox rtb = (RichTextBox)sender;
				String tab = new String(' ', 4);
				TextPointer caretPosition = rtb.CaretPosition;
				TextRange a = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd); 
				
				caretPosition.InsertTextInRun(tab);
				//base.CaretIndex = caretPosition + TabSize + 1;
				e.Handled = true;
			}
		}
		static public void findDiffArea(TextPointer poiStart, TextPointer poiEnd, string text, int areaLen = 1)
		{
			int i = 0;
			int tpCount = poiStart.GetOffsetToPosition(poiEnd);
			TextPointer tStart = poiStart;
			TextPointer tEnd = tStart.GetPositionAtOffset(areaLen);

			for (; tEnd != poiEnd && i < text.Length;)
			{
				TextRange trCheck = new TextRange(tStart, tEnd);
				string strCheck = trCheck.Text;
				string curSubStr = text.Substring(i, strCheck.Length);

				if(curSubStr != strCheck)
				{
					break;
				}
				else
				{
					i += strCheck.Length;
					tStart = tEnd;
					tEnd = tStart.GetPositionAtOffset(areaLen);
				}
			}
		}
		private void mx_rtbText_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(sender is RichTextBox)
			{
				RichTextBox rtb = (RichTextBox)sender;

				if(m_lastText != null)
				{
					TextPointer poiStart = rtb.Document.ContentStart;
					TextPointer poiEnd = rtb.Document.ContentEnd;
					findDiffArea(poiStart, poiEnd, m_lastText);
				}
			}
		}
	}
}
