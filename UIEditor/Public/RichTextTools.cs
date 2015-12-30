using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace UIEditor.Public
{
	public class RichTextTools
	{
		private enum TextChangedType_E
		{
			OLDLARGER_TRUE,
			OLDLARGER_FLASE,
			NEWLARGER_TRUE,
			NEWLARGER_FALSE,
		}
		static private TextChangedType_E checkDiff(TextPointer poiStart, TextPointer poiEnd, string lastText)
		{
			TextRange trCheck = new TextRange(poiStart, poiEnd);
			string strCheck = trCheck.Text;

			if (lastText.Length > strCheck.Length)
			{
				string curSubStr = lastText.Substring(0, strCheck.Length);

				if (curSubStr == strCheck)
				{
					return TextChangedType_E.OLDLARGER_TRUE;
				}
				else
				{
					return TextChangedType_E.OLDLARGER_FLASE;
				}
			}
			else
			{
				string curSubStr = strCheck.Substring(0, lastText.Length);

				if (curSubStr == lastText)
				{
					return TextChangedType_E.NEWLARGER_TRUE;
				}
				else
				{
					return TextChangedType_E.NEWLARGER_FALSE;
				}
			}
		}
		private const int sc_maxTextChangeCheckCount = 4;
		static public void getDiffTextArea(ref TextPointer poiStart, ref TextPointer poiEnd, string lastText)
		{
			int tpCount = poiStart.GetOffsetToPosition(poiEnd);
			int tpCountLast = poiEnd.GetOffsetToPosition(poiStart.DocumentEnd);

			if (lastText == null || lastText == "" || (new TextRange(poiStart, poiEnd)).Text == "" ||
				tpCount <= sc_maxTextChangeCheckCount)
			{
				poiEnd = poiStart;

				return;
			}

			TextPointer tpHalf = poiStart.GetPositionAtOffset(tpCount / 2);
			TextPointer tpLastHalf = poiEnd.GetPositionAtOffset(tpCountLast / 2);

			switch (checkDiff(poiStart.DocumentStart, poiEnd, lastText))
			{
				case TextChangedType_E.OLDLARGER_TRUE:
					{
						poiStart = poiEnd;
						poiEnd = tpLastHalf;
					}
					break;
				case TextChangedType_E.OLDLARGER_FLASE:
					{
						poiEnd = tpHalf;
					}
					break;
				case TextChangedType_E.NEWLARGER_TRUE:
					{
						poiStart = tpHalf;
						poiEnd = tpHalf;
					}
					break;
				case TextChangedType_E.NEWLARGER_FALSE:
					{
						poiEnd = tpHalf;
					}
					break;
				default:
					{

					}
					break;
			}
			getDiffTextArea(ref poiStart, ref poiEnd, lastText);
		}


		static public List<TextRange> FindAllMatchedTextRanges(RichTextBox rtb, string keyWord)
		{
			if (keyWord == "" || keyWord == null)
			{
				return null;
			}
			List<TextRange> trList = new List<TextRange>();
			//设置文字指针为Document初始位置
			TextPointer position = rtb.Document.ContentStart;
			while (position != null)
			{
				//向前搜索,需要内容为Text
				if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
				{
					//拿出Run的Text
					string text = position.GetTextInRun(LogicalDirection.Forward);
					//可能包含多个keyword,做遍历查找
					int index = 0;
					while (index < text.Length)
					{
						index = text.IndexOf(keyWord, index);
						if (index == -1)
						{
							break;
						}
						else
						{
							//添加为新的Range
							TextPointer start = position.GetPositionAtOffset(index);
							TextPointer end = start.GetPositionAtOffset(keyWord.Length);

							trList.Add(new TextRange(start, end));
							index += keyWord.Length;
						}
					}
				}
				//文字指针向前偏移
				position = position.GetNextContextPosition(LogicalDirection.Forward);
			}
			return trList;
		}
		static public void findKeyWord(RichTextBox rtb, string keyWord, SolidColorBrush brush)  //给关键字上色
		{
			List<TextRange> lstRag = FindAllMatchedTextRanges(rtb, keyWord);

			foreach (TextRange rag in lstRag)
			{
				rag.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
			}
		}
		static public void refreshXmlTextTip(RichTextBox rtb, Dictionary<SolidColorBrush, List<string>> mapKeyWordsGroup)
		{
			//太浪费时间
			TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
			textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xBD, 0xB7, 0x6B)));

			foreach (KeyValuePair<SolidColorBrush, List<string>> pairKeyWordsGroup in mapKeyWordsGroup)
			{
				foreach(string keyWord in pairKeyWordsGroup.Value)
				{
					findKeyWord(rtb, keyWord, pairKeyWordsGroup.Key);
				}
			}
		}
	}
}
