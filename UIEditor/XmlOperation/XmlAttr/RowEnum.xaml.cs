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
using UIEditor;
using UIEditor.BoloUI;
using UIEditor.BoloUI.DefConfig;
using UIEditor.Project.PlugIn;

namespace UIEditor.XmlOperation.XmlAttr
{
	/// <summary>
	/// RowEnum.xaml 的交互逻辑
	/// </summary>
	public partial class RowEnum : Grid, IAttrRow
	{
		private string mt_name;
		private string mt_value;
		private string mt_type;
		private bool m_eventLock;
		private bool m_isUseEvent;
		private void setValue(bool isPre, string value)
		{
			if (mt_value != value && m_eventLock == false)
			{
				if (!isPre && m_parent != null && m_parent.m_xmlCtrl != null &&
					m_parent.m_xe != null && m_parent.m_xmlCtrl.m_openedFile != null &&
					m_parent.m_xmlCtrl.m_openedFile.m_lstOpt != null && m_parent.m_basic != null)
				{
					if (m_name == "moveType" && (m_parent.m_basic.m_xe.Name == "particleKeyFrame" ||
						m_parent.m_basic.m_xe.Name == "controlFrame") && m_isUseEvent)
					{
						#region 特效关键帧轨迹特殊处理，附加参数属性值修改部分。
						List<XmlOperation.HistoryNode> lstOptNode = new List<HistoryNode>();

						switch(value)
						{
							case "0":
								{
									//直线运动
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							case "1":
								{
									//圆弧运动
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							case "2":
								{
									//斐波那契螺旋线
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "8");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "10");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							case "3":
								{
									//正弦线
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "100");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							case "4":
								{
									//叶形线
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "100");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "-10");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "60");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							case "5":
								{
									//螺旋线
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "3");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "100");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							case "6":
								{
									//圆柱螺旋线
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx1", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx2", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx3", "3");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx4", "50");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx5", "100");

									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx6", "45");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx7", "");
									XmlOperation.HistoryList.updateAttrToOptList(ref lstOptNode, m_parent.m_basic.m_xe, "moveTypeEx8", "");
								}
								break;
							default:
								break;
						}

						lstOptNode.Add(new XmlOperation.HistoryNode(m_parent.m_basic.m_xe, m_name, mt_value, value));
						m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(lstOptNode);
						#endregion
					}
					else
					{
						m_parent.m_xmlCtrl.m_openedFile.m_lstOpt.addOperation(new XmlOperation.HistoryNode(m_parent.m_basic.m_xe, m_name, mt_value, value));
					}
				}

				if (m_name == "moveType" && (m_parent.m_basic.m_xe.Name == "particleKeyFrame" || m_parent.m_basic.m_xe.Name == "controlFrame"))
				{
					#region 特效关键帧轨迹特殊处理，附加参数属性名称修改部分
					List<XmlOperation.HistoryNode> lstOptNode = new List<HistoryNode>();
					bool isChanged = true;

					switch (value)
					{
						case "0":
							{
								//直线运动
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "加速度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "旋转角度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "无效的参数");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "无效的参数");
							}
							break;
						case "1":
							{
								//圆弧运动
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "附加横坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "附加纵坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "旋转方向");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "无效的参数");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "加速度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "旋转角度");
							}
							break;
						case "2":
							{
								//斐波那契螺旋线
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "始点圆心横坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "始点圆心纵坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "方向");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "圆弧数量");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "格子距离");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "运动方式");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "曲线旋转角度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "无效的参数");
							}
							break;
						case "3":
							{
								//正弦线
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "峰值");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "偏差值");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "周期数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "加速度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "旋转角度");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "无效的参数");
							}
							break;
						case "4":
							{
								//叶形线
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "中心横坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "中心纵坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "叶形长度相关");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "曲线起始画点");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "曲线线束画点");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "表示曲线旋转多少角度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "方向");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "无效的参数");
							}
							break;
						case "5":
							{
								//螺旋线
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "中心横坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "中心纵坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "圈数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "最大圈半径");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "旋转角度");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "方向");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "无效的参数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "无效的参数");
							}
							break;
						case "6":
							{
								//圆柱螺旋线
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx1", "中心横坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx2", "中心纵坐标");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx3", "圈数");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx4", "圆半径");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx5", "圆柱高度");

								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx6", "倾斜角度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx7", "旋转角度");
								MainWindow.s_pW.m_strDic.setWordByKey("moveTypeEx8", "方向");
							}
							break;
						default:
							{
								isChanged = false;
							}
							break;
					}
					if(isChanged == true)
					{
						m_parent.refreshRowHeader();
					}
					#endregion
				}

				mt_value = value;
				m_eventLock = true;

				ComboBoxItem selCb;

				if (m_mapEnum != null && m_mapEnum.TryGetValue(value, out selCb) && selCb != null)
				{
					selCb.IsSelected = true;
				}
				else
				{
					mx_defaultEnum.IsSelected = true;
				}
				m_eventLock = false;
			}
		}

		public AttrList m_parent { get; set; }
		public bool m_isCommon { get; set; }
		public string m_subType { get; set; }
		public string m_name
		{
			get { return mt_name; }
			set
			{
				if (m_subType == "Apperance")
				{

				}
				mt_name = value;

				string outStr = MainWindow.s_pW.m_strDic.getWordByKey(value);
				if(outStr != "")
				{
					string tip = MainWindow.s_pW.m_strDic.getWordByKey(value, StringDic.conf_ctrlAttrTipDic);

					mx_nameEnum.Content = outStr;
					if (tip != "")
					{
						mx_root.ToolTip = tip;
					}
					else
					{
						mx_root.ToolTip = value;
					}
				}
				else
				{
					mx_nameEnum.Content = value;
					mx_root.ToolTip = value;
				}
			}
		}
		public string m_preValue
		{
			get { return mt_value; }
			set
			{
				setValue(true, value);
			}
		}
		public string m_value
		{
			get { return mt_value; }
			set
			{
				setValue(false, value);
			}
		}
		public string m_type { get; set; }
		public Dictionary<string, ComboBoxItem> m_mapEnum;

		private string mt_defValue;
		public string m_defValue { get; set; }

		public RowEnum(DataAttr attrDef, string name = "", string value = "", AttrList parent = null)
		{
			m_isUseEvent = false;
			InitializeComponent();
			m_parent = parent;
			m_mapEnum = attrDef.m_mapEnum;
			m_isCommon = attrDef.m_isCommon;
			m_subType = attrDef.m_subType;
			m_eventLock = false;

			m_name = name;
			m_preValue = value;
			m_type = attrDef.m_type;

			if (m_mapEnum != null && m_mapEnum.Count() > 0)
			{
				foreach (KeyValuePair<string, ComboBoxItem> pairEnum in m_mapEnum.ToList())
				{
					ComboBoxItem cbEnum = new ComboBoxItem();
					string strEnum = "";
					if (m_subType != null && m_subType != "")
					{
						strEnum = MainWindow.s_pW.m_strDic.getWordByKey(pairEnum.Key, StringDic.conf_ctrlAttrTipDic + "_" + m_subType);
					}

					if (strEnum == "")
					{
						strEnum = pairEnum.Key;
					}
					cbEnum.Content = strEnum;
					cbEnum.ToolTip = pairEnum.Key;
					m_mapEnum[pairEnum.Key] = cbEnum;
					mx_valueEnum.Items.Add(cbEnum);
				}
			}
		}

		private void mx_valueEnum_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if((ComboBox)sender != null && ((ComboBox)sender).SelectedItem != null &&
				((ComboBox)sender).SelectedItem is ComboBoxItem)
			{
				ComboBoxItem selCb = (ComboBoxItem)(((ComboBox)sender).SelectedItem);

				m_isUseEvent = true;
				m_value = selCb.ToolTip.ToString();
				m_isUseEvent = false;
			}
		}
	}
}
