using System;
using System.Text;
using System.Text.RegularExpressions;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000AB RID: 171
	public class LotCodeRuleEdit : AbstractBillPlugIn
	{
		// Token: 0x06000A76 RID: 2678 RVA: 0x0008F3E8 File Offset: 0x0008D5E8
		public override void DataChanged(DataChangedEventArgs e)
		{
			DynamicObject dynamicObject = this.Model.GetValue("FLotPropertyId", e.Row) as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			string text = dynamicObject["Type"].ToString().ToUpperInvariant();
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FFORMAT"))
				{
					string key;
					if (!(a == "FLOTPROPERTYID"))
					{
						if (!(a == "FPROJECTVALUE"))
						{
							if (!(a == "FHEXSEED") && !(a == "FSEED"))
							{
								return;
							}
							DynamicObject dynamicObject2 = base.View.Model.GetValue("FLotPropertyId", e.Row) as DynamicObject;
							DynamicObject dynamicObject3 = base.View.Model.GetValue("FFormat", e.Row) as DynamicObject;
							if (dynamicObject2 != null && dynamicObject3 != null && dynamicObject2["Type"].ToString().ToUpperInvariant().Equals("FLOWNO") && dynamicObject3["Number"].ToString().ToUpperInvariant().Equals("HEX"))
							{
								if (e.Field.Key.ToUpperInvariant().Equals("FHEXSEED"))
								{
									base.View.Model.SetValue("FSeed", Convert.ToInt32(Convert.ToString(e.NewValue), 16), e.Row);
									return;
								}
								base.View.Model.SetValue("FHexSeed", Convert.ToString(Convert.ToInt64(e.NewValue), 16).ToUpperInvariant(), e.Row);
								return;
							}
							else
							{
								if (e.Field.Key.ToUpperInvariant().Equals("FHEXSEED"))
								{
									base.View.Model.SetValue("FSeed", e.NewValue, e.Row);
									return;
								}
								base.View.Model.SetValue("FHexSeed", e.NewValue, e.Row);
							}
						}
						else if (text == "CONST")
						{
							base.View.Model.SetValue("FLength", (e.NewValue == null) ? 0 : e.NewValue.ToString().Length, e.Row);
							return;
						}
					}
					else
						switch (key = text)
						{
						case "NUMBER":
						case "CONST":
						case "BASEDATA":
						case "ASSISTANTDATA":
						case "BILLTEXT":
							base.View.Model.SetItemValueByNumber("FFormat", "Normal", e.Row);
							base.View.Model.SetValue("FLength", 0, e.Row);
							return;
						case "DATETIME":
						case "CURRENTDATE":
							base.View.Model.SetItemValueByNumber("FFormat", "yyyy-MM-dd", e.Row);
							base.View.Model.SetValue("FLength", "yyyy-MM-dd".Length, e.Row);
							return;
						case "FLOWNO":
						case "ROWNO":
							base.View.Model.SetValue("FAddChar", "0", e.Row);
							base.View.Model.SetValue("FLength", 3, e.Row);
							return;

							return;
						}
				}
				else
				{
					DynamicObject dynamicObject4 = base.View.Model.GetValue(e.Field.Key, e.Row) as DynamicObject;
					if (text.Equals("DATETIME") || text.Equals("CURRENTDATE"))
					{
						if (dynamicObject4 == null || dynamicObject4["FormatValue"] == null)
						{
							base.View.Model.SetValue("FLength", 0, e.Row);
							base.View.Model.SetValue("FAddChar", "", e.Row);
							return;
						}
						if (dynamicObject4["FormatValue"].ToString() == "DayOfYear")
						{
							base.View.Model.SetValue("FLength", 3, e.Row);
							base.View.Model.SetValue("FAddChar", "0", e.Row);
							return;
						}
						if (dynamicObject4["FormatValue"].ToString() == "WeekOfYear")
						{
							base.View.Model.SetValue("FLength", 2, e.Row);
							base.View.Model.SetValue("FAddChar", "0", e.Row);
							return;
						}
						if (dynamicObject4["FormatValue"].ToString() == "Y33")
						{
							base.View.Model.SetValue("FLength", 1, e.Row);
							base.View.Model.SetValue("FAddChar", "", e.Row);
							base.View.Model.SetValue("FProjectValue", "M", e.Row);
							base.View.Model.SetValue("FSeed", "2020", e.Row);
							return;
						}
						if (dynamicObject4["FormatValue"].ToString() == "d")
						{
							base.View.Model.SetValue("FLength", 0, e.Row);
							base.View.Model.SetValue("FAddChar", "", e.Row);
							return;
						}
						base.View.Model.SetValue("FLength", dynamicObject4["FormatValue"].ToString().Length, e.Row);
						base.View.Model.SetValue("FAddChar", "", e.Row);
						return;
					}
					else if (text.Equals("FLOWNO"))
					{
						if (Convert.ToString(e.NewValue).Equals("23"))
						{
							base.View.GetControl("FSeed").Visible = false;
							base.View.GetControl("FHexSeed").Visible = true;
							return;
						}
						if (Convert.ToString(e.NewValue).Equals("22") && Convert.ToString(e.OldValue).Equals("23"))
						{
							base.View.GetControl("FSeed").Visible = true;
							base.View.GetControl("FHexSeed").Visible = false;
							return;
						}
					}
				}
			}
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x0008FB5C File Offset: 0x0008DD5C
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FPROJECTVALUE"))
				{
					if (!(a == "FSEED"))
					{
						if (!(a == "FHEXSEED"))
						{
							return;
						}
						DynamicObject dynamicObject = this.Model.GetValue("FLotPropertyId", e.Row) as DynamicObject;
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FFormat", e.Row) as DynamicObject;
						IntegerField integerField = base.View.BusinessInfo.GetField("FSeed") as IntegerField;
						if (dynamicObject != null && dynamicObject["Type"].ToString().ToUpperInvariant().Equals("FLOWNO") && dynamicObject2 != null && dynamicObject2["Number"].ToString().ToUpperInvariant().Equals("HEX"))
						{
							if (this.IsIllegalHexadecimal(Convert.ToString(e.Value)))
							{
								base.View.ShowWarnningMessage(ResManager.LoadKDString("格式为十六进制时，只允许录入字符（A、B、C、D、E、F）和数字（0~9）。", "004023030009700", 5, new object[0]), "", 0, null, 1);
								e.Cancel = true;
								return;
							}
							if (string.IsNullOrEmpty(Convert.ToString(e.Value)) || integerField.CheckScope(Convert.ToInt64(Convert.ToString(e.Value), 16)))
							{
								base.View.ShowWarnningMessage(string.Format(ResManager.LoadKDString("数据值录入范围为({0},{1})。", "004023030009701", 5, new object[0]), Convert.ToString(Convert.ToInt64(integerField.GetMinDataScope()), 16).ToUpperInvariant(), Convert.ToString(Convert.ToInt64(integerField.GetMaxDataScope()), 16).ToUpperInvariant()), "", 0, null, 1);
								e.Cancel = true;
								return;
							}
						}
						else if (!StringUtils.IsNumeric(Convert.ToString(e.Value)) || integerField.CheckScope(Convert.ToInt64(e.Value)))
						{
							e.Cancel = true;
							return;
						}
						e.Value = Convert.ToString(e.Value).ToUpperInvariant();
					}
					else if (this.IsYear33(e.Row))
					{
						if (!StringUtils.IsNumeric(Convert.ToString(e.Value)))
						{
							base.View.ShowWarnningMessage(ResManager.LoadKDString("格式为“1位年份_33字符”时,起始值为4位数字范围为2000~2999。", "004023030009365", 5, new object[0]), "", 0, null, 1);
							e.Cancel = true;
							return;
						}
						if (Convert.ToInt32(e.Value) > 2999 || Convert.ToInt32(e.Value) < 2000)
						{
							base.View.ShowWarnningMessage(ResManager.LoadKDString("格式为“1位年份_33字符”时,起始值为4位数字，范围为2000~2999。", "004023030009364", 5, new object[0]), "", 0, null, 1);
							e.Cancel = true;
							return;
						}
					}
				}
				else if (this.IsYear33(e.Row))
				{
					if (Convert.ToString(e.Value).Length != 1 || !LotCodeRuleEdit.IsNumAndEnCh(Convert.ToString(e.Value)))
					{
						base.View.ShowWarnningMessage(ResManager.LoadKDString("格式为“1位年份_33字符”时,设置值的合法值范围为：0~9，不含I、O、Z的字母，只允许输入1个数字或字母。", "004023030009363", 5, new object[0]), "", 0, null, 1);
						e.Cancel = true;
						return;
					}
					if (StringUtils.EqualsIgnoreCase(Convert.ToString(e.Value), "I") || StringUtils.EqualsIgnoreCase(Convert.ToString(e.Value), "O") || StringUtils.EqualsIgnoreCase(Convert.ToString(e.Value), "Z"))
					{
						base.View.ShowWarnningMessage(ResManager.LoadKDString("格式为“1位年份_33字符”时,设置值的合法值范围为：0~9，不含I、O、Z的字母，只允许输入1个数字或字母。", "004023030009363", 5, new object[0]), "", 0, null, 1);
						e.Cancel = true;
						return;
					}
					e.Value = Convert.ToString(e.Value).ToUpperInvariant();
					return;
				}
			}
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x0008FF28 File Offset: 0x0008E128
		public override void AfterBindData(EventArgs e)
		{
			if (base.View.OpenParameter.Status != null)
			{
				this.SetBillNoExample();
			}
			else
			{
				base.View.GetControl("FCodeLen").Text = ResManager.LoadKDString("编码总长度：", "004023030000355", 5, new object[0]);
				base.View.GetControl("FExample").Text = ResManager.LoadKDString("编码示例：", "004023030000358", 5, new object[0]);
			}
			this.initHexSeed();
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x0008FFAC File Offset: 0x0008E1AC
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBSPLITSAVE") && !(a == "TBSAVE") && !(a == "TBSPLITSUBMIT") && !(a == "TBSUBMIT"))
				{
					return;
				}
				this.SetBillNoExample();
				this.Model.DataChanged = false;
			}
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00090010 File Offset: 0x0008E210
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (a == "FLOTPROPERTYID")
				{
					e.IsShowApproved = false;
					return;
				}
				if (!(a == "FFORMAT"))
				{
					return;
				}
				string text;
				if (this.GetFieldFilter(e.FieldKey, e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
				}
				e.ListFilterParameter.OrderBy = " FSeq ASC ";
			}
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x000900B4 File Offset: 0x0008E2B4
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (a == "FLOTPROPERTYID")
				{
					e.IsShowApproved = false;
					return;
				}
				if (!(a == "FFORMAT"))
				{
					return;
				}
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
				}
			}
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00090138 File Offset: 0x0008E338
		private void initHexSeed()
		{
			bool flag = false;
			int entryRowCount = base.View.Model.GetEntryRowCount("FLotCodeRuleEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FLotPropertyId", i) as DynamicObject;
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FFormat", i) as DynamicObject;
				if (dynamicObject != null && dynamicObject2 != null && dynamicObject["Type"].ToString().ToUpperInvariant().Equals("FLOWNO") && dynamicObject2["Number"].ToString().ToUpperInvariant().Equals("HEX"))
				{
					base.View.Model.SetValue("FHexSeed", Convert.ToString(Convert.ToInt64(base.View.Model.GetValue("FSeed", i)), 16).ToUpperInvariant(), i);
					flag = true;
				}
				else
				{
					base.View.Model.SetValue("FHexSeed", base.View.Model.GetValue("FSeed", i), i);
				}
			}
			if (flag)
			{
				base.View.GetControl("FSeed").Visible = false;
				base.View.GetControl("FHexSeed").Visible = true;
				return;
			}
			base.View.GetControl("FSeed").Visible = true;
			base.View.GetControl("FHexSeed").Visible = false;
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x000902CC File Offset: 0x0008E4CC
		public bool IsIllegalHexadecimal(string str)
		{
			return Regex.IsMatch(str, "([^A-Fa-f0-9]|\\s+?)+");
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x000902DC File Offset: 0x0008E4DC
		public static bool IsNumAndEnCh(string input)
		{
			string pattern = "^[A-Za-z0-9]+$";
			Regex regex = new Regex(pattern);
			return regex.IsMatch(input);
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x00090300 File Offset: 0x0008E500
		private bool IsYear33(int row)
		{
			bool result = false;
			DynamicObject dynamicObject = base.View.Model.GetValue("FFORMAT", row) as DynamicObject;
			if (dynamicObject != null && Convert.ToString(dynamicObject["FormatValue"]).Equals("Y33"))
			{
				result = true;
			}
			return result;
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x00090350 File Offset: 0x0008E550
		private bool GetFieldFilter(string fieldKey, int row, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FFORMAT")
			{
				DynamicObject dynamicObject = this.Model.GetValue("FLotPropertyId", row) as DynamicObject;
				if (dynamicObject != null)
				{
					string text = dynamicObject["Type"].ToString();
					string key;
					switch (key = text.ToUpperInvariant())
					{
					case "NUMBER":
					case "CONST":
					case "BASEDATA":
					case "ASSISTANTDATA":
					case "BILLTEXT":
						filter = string.Format(" FItemType = 'Text' ", new object[0]);
						break;
					case "DATETIME":
					case "CURRENTDATE":
						filter = string.Format(" FItemType = 'DateTime' )", new object[0]);
						break;
					case "FLOWNO":
						filter = string.Format(" FItemType = 'FlowNo' )", new object[0]);
						break;
					}
				}
			}
			return true;
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x000904B4 File Offset: 0x0008E6B4
		private void SetBillNoExample()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Entity entity = base.View.BusinessInfo.GetEntity("FLotCodeRuleEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			string text = "";
			if (entityDataObject != null && entityDataObject.Count > 0)
			{
				for (int i = 0; i < entityDataObject.Count; i++)
				{
					DynamicObject dynamicObject = entityDataObject[i];
					DynamicObject dynamicObject2 = dynamicObject["LotPropertyId"] as DynamicObject;
					if (Convert.ToBoolean(dynamicObject["CodeElement"]))
					{
						text = ((dynamicObject["Seperator"] == null || string.IsNullOrWhiteSpace(dynamicObject["Seperator"].ToString())) ? "" : dynamicObject["Seperator"].ToString());
						if (dynamicObject2 != null)
						{
							string text2 = dynamicObject2["Type"].ToString();
							string key;
							switch (key = text2.ToUpperInvariant())
							{
							case "CONST":
							{
								string fixTextValue = this.GetFixTextValue(dynamicObject);
								stringBuilder.Append(fixTextValue + text);
								break;
							}
							case "FLOWNO":
							{
								string billFlowNo = this.GetBillFlowNo(dynamicObject);
								stringBuilder.Append(billFlowNo + text);
								break;
							}
							case "BILLTEXT":
							{
								string textFieldValue = this.GetTextFieldValue(dynamicObject);
								stringBuilder.Append(textFieldValue + text);
								break;
							}
							case "DATETIME":
							case "CURRENTDATE":
							{
								string dateFieldValue = this.GetDateFieldValue(dynamicObject);
								stringBuilder.Append(dateFieldValue + text);
								break;
							}
							case "BASEDATA":
							case "ASSISTANTDATA":
							{
								string baseDataFieldValue = this.GetBaseDataFieldValue(dynamicObject);
								stringBuilder.Append(baseDataFieldValue + text);
								break;
							}
							case "NUMBER":
							{
								string numberFieldValue = this.GetNumberFieldValue(dynamicObject);
								stringBuilder.Append(numberFieldValue + text);
								break;
							}
							case "ROWNO":
							{
								string rowNoFieldValue = this.GetRowNoFieldValue(dynamicObject);
								stringBuilder.Append(rowNoFieldValue + text);
								break;
							}
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text) && stringBuilder.ToString().Length > 0)
			{
				stringBuilder.Remove(stringBuilder.ToString().Length - 1, 1);
			}
			base.View.GetControl("FCodeLen").Text = string.Format(ResManager.LoadKDString("编码总长度：{0}", "004023030000361", 5, new object[0]), stringBuilder.ToString().Length);
			base.View.GetControl("FExample").Text = string.Format(ResManager.LoadKDString("编码示例：{0}", "004023030000364", 5, new object[0]), stringBuilder.ToString());
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x000907F0 File Offset: 0x0008E9F0
		private string GetBillFlowNo(DynamicObject dyObj)
		{
			string text = "";
			string text2 = (dyObj["Seed"] == null) ? "1" : dyObj["Seed"].ToString();
			if (dyObj["Format"] != null && ((DynamicObject)dyObj["Format"])["FormatValue"] != null && Convert.ToString(((DynamicObject)dyObj["Format"])["FormatValue"]).ToUpperInvariant().Equals("HEX"))
			{
				text2 = Convert.ToString(Convert.ToInt64(text2), 16).ToUpperInvariant();
			}
			bool flag = dyObj["AddStyle"].ToString() == "True";
			string a = (dyObj["AddChar"] == null) ? "" : dyObj["AddChar"].ToString();
			char c = (a == "") ? '\0' : Convert.ToChar(dyObj["AddChar"]);
			dyObj["CutStyle"].ToString() == "True";
			int num = Convert.ToInt32(dyObj["Length"].ToString());
			if (c != '\0')
			{
				if (num < text2.Length)
				{
					return text2;
				}
				if (!flag)
				{
					text = new string(c, num - text2.Length);
				}
				text += text2;
				if (flag)
				{
					text += new string(c, num - text2.Length);
				}
			}
			else
			{
				text = text2;
			}
			return text;
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x0009097C File Offset: 0x0008EB7C
		private string GetFixTextValue(DynamicObject dyObj)
		{
			string result = "";
			if (dyObj == null)
			{
				return result;
			}
			string sFormat = (dyObj["Format"] == null || ((DynamicObject)dyObj["Format"])["FormatValue"] == null) ? "" : ((DynamicObject)dyObj["Format"])["FormatValue"].ToString();
			string srcValue = (dyObj["ProjectValue"] == null) ? "" : dyObj["ProjectValue"].ToString();
			return this.ConvertFormat(sFormat, srcValue);
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x00090A14 File Offset: 0x0008EC14
		private string GetTextFieldValue(DynamicObject dyObj)
		{
			string text = ResManager.LoadKDString("文本", "004023000017221", 5, new object[0]);
			if (dyObj == null)
			{
				return text;
			}
			string sFormat = (dyObj["Format"] == null || ((DynamicObject)dyObj["Format"])["FormatValue"] == null) ? "" : ((DynamicObject)dyObj["Format"])["FormatValue"].ToString();
			return this.ConvertFormat(sFormat, text);
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x00090A98 File Offset: 0x0008EC98
		private string GetDateFieldValue(DynamicObject dyObj)
		{
			string text = "yyyy-MM-dd";
			if (dyObj == null)
			{
				return text;
			}
			text = ((dyObj["Format"] == null || ((DynamicObject)dyObj["Format"])["FormatValue"] == null) ? text : ((DynamicObject)dyObj["Format"])["FormatValue"].ToString());
			if (text == "DayOfYear")
			{
				text = "N";
			}
			else if (text == "WeekOfYear")
			{
				text = "N";
			}
			else if (text == "M")
			{
				text = "A";
			}
			else if (text == "Y33")
			{
				text = ((dyObj["ProjectValue"] == null) ? "M" : dyObj["ProjectValue"].ToString());
			}
			return text;
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x00090B70 File Offset: 0x0008ED70
		private string GetBaseDataFieldValue(DynamicObject dyObj)
		{
			DynamicObject dynamicObject = ((DynamicObject)dyObj["LotPropertyId"])["SourceId"] as DynamicObject;
			string srcValue = (dynamicObject == null) ? ((DynamicObject)dyObj["LotPropertyId"])["Name"].ToString() : dynamicObject["Name"].ToString();
			string sFormat = (dyObj["Format"] == null || ((DynamicObject)dyObj["Format"])["FormatValue"] == null) ? "" : ((DynamicObject)dyObj["Format"])["FormatValue"].ToString();
			return this.ConvertFormat(sFormat, srcValue);
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x00090C2C File Offset: 0x0008EE2C
		private string GetNumberFieldValue(DynamicObject dyObj)
		{
			string srcValue = ((DynamicObject)dyObj["LotPropertyId"])["Name"].ToString();
			string sFormat = (dyObj["Format"] == null || ((DynamicObject)dyObj["Format"])["FormatValue"] == null) ? "" : ((DynamicObject)dyObj["Format"])["FormatValue"].ToString();
			return this.ConvertFormat(sFormat, srcValue);
		}

		// Token: 0x06000A88 RID: 2696 RVA: 0x00090CB4 File Offset: 0x0008EEB4
		private string GetRowNoFieldValue(DynamicObject dyObj)
		{
			return ResManager.LoadKDString("行号", "004023000022541", 5, new object[0]);
		}

		// Token: 0x06000A89 RID: 2697 RVA: 0x00090CD9 File Offset: 0x0008EED9
		private string ConvertFormat(string sFormat, string srcValue)
		{
			if (sFormat == "AllUpperCase")
			{
				return srcValue.ToUpperInvariant();
			}
			if (sFormat == "AllLowerCase")
			{
				return srcValue.ToLowerInvariant();
			}
			return srcValue;
		}
	}
}
