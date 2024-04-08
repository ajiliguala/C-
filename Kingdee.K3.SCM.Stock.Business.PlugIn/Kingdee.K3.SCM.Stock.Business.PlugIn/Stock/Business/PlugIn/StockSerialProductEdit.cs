using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000088 RID: 136
	[Description("预生成序列号表单插件")]
	public class StockSerialProductEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000682 RID: 1666 RVA: 0x0004F3BC File Offset: 0x0004D5BC
		public override void OnInitialize(InitializeEventArgs e)
		{
			if (e.Paramter.GetCustomParameter("bType") == null)
			{
				return;
			}
			this.bType = Convert.ToBoolean(e.Paramter.GetCustomParameter("bType"));
			object obj = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "SerialManageLevel", null);
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				this._canMulOrg = !obj.ToString().Equals("A");
			}
			this.bMTrack = false;
			obj = e.Paramter.GetCustomParameter("bMTrack");
			if (obj != null)
			{
				this.bMTrack = Convert.ToBoolean(obj);
			}
		}

		// Token: 0x06000683 RID: 1667 RVA: 0x0004F460 File Offset: 0x0004D660
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.RegexOrgEntry();
			this.View.Model.SetValue("FOrgId", base.Context.CurrentOrganizationInfo.ID, 0);
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x0004F49C File Offset: 0x0004D69C
		private void RegexOrgEntry()
		{
			if (!this._canMulOrg && this.Model.GetEntryRowCount("FOrgEntity") > 1)
			{
				DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["OrgEntity"] as DynamicObjectCollection;
				for (int i = dynamicObjectCollection.Count; i > 0; i--)
				{
					dynamicObjectCollection.RemoveAt(i);
				}
				this.View.UpdateView("FOrgEntity");
			}
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x0004F50C File Offset: 0x0004D70C
		public override void AfterBindData(EventArgs e)
		{
			this.View.GetControl("FCodeRule").Visible = this.bType;
			this.View.GetControl("FNumber").Visible = this.bType;
			this.View.GetControl("FCodeLen").Visible = this.bType;
			this.View.GetControl("FExample").Visible = this.bType;
			this.View.GetControl("FSerialNumber").Enabled = !this.bType;
			this.View.GetControl("FLot").Enabled = !this.bType;
			this.SetGenSerialByCouVisible();
			this.SetFormEnabled(true);
			this.SetDefOriVal();
			this.View.UpdateView("FOriVal");
			this.SetBillNoExampleByCou();
			this.View.UpdateView("FExampleByCou");
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0004F5FA File Offset: 0x0004D7FA
		public override void OnLoad(EventArgs e)
		{
			this.View.GetMainBarItem("tbSave").Enabled = true;
			this._saved = false;
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0004F61C File Offset: 0x0004D81C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FCODERULE":
			{
				string empty = string.Empty;
				if (e.NewValue == null)
				{
					this.View.Model.SetValue("FCodeLen", 0);
					this.View.Model.SetValue("FExample", empty);
					return;
				}
				this.SetBillNoExample(Convert.ToInt64(e.NewValue));
				return;
			}
			case "FPREFIX":
			case "FSUFFIX":
				this.SetDefOriVal();
				this.SetBillNoExampleByCou();
				return;
			case "FSTRIDE":
			case "FSERLEN":
			case "FFLOWNOTYPE":
				this.SetBillNoExampleByCou();
				return;
			case "FORIVAL":
				this.SetBillNoExampleByCou();
				this.View.Model.SetValue("FHexOriVal", Convert.ToString(Convert.ToInt64(this.View.Model.GetValue("FOriVal")), 16));
				return;
			case "FHEXORIVAL":
				this.SetBillNoExampleByCou();
				this.View.Model.SetValue("FOriVal", Convert.ToInt64(Convert.ToString(this.View.Model.GetValue("FHexOriVal")), 16));
				break;

				return;
			}
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x0004F7DC File Offset: 0x0004D9DC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FORGID") && !(a == "FMATERIALID") && !(a == "FLOT"))
				{
					return;
				}
				string fieldFilter = this.GetFieldFilter(e.FieldKey);
				if (!string.IsNullOrWhiteSpace(fieldFilter))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = fieldFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + fieldFilter;
				}
			}
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x0004F870 File Offset: 0x0004DA70
		private string GetFieldFilter(string fieldKey)
		{
			string text = "";
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FORGID"))
				{
					if (!(a == "FMATERIALID"))
					{
						if (a == "FLOT")
						{
							DynamicObject dynamicObject = this.Model.GetValue("FMaterialId") as DynamicObject;
							if (dynamicObject != null)
							{
								long num;
								if (dynamicObject.DynamicObjectType.Properties.ContainsKey(FormConst.MASTER_ID))
								{
									num = Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]);
								}
								else
								{
									num = Convert.ToInt64(dynamicObject["Id"]);
								}
								text = string.Format(" FMaterialId = {0} ", num);
							}
						}
					}
					else
					{
						if (this.bType)
						{
							text = " FSNCREATETIME = '2'  ";
						}
						if (this.bMTrack)
						{
							if (string.IsNullOrWhiteSpace(text))
							{
								text = " FIsSNManage = '0' AND FIsSNPRDTracy = '1' ";
							}
							else
							{
								text += " AND FIsSNManage = '0' AND FIsSNPRDTracy = '1' ";
							}
						}
						else if (string.IsNullOrWhiteSpace(text))
						{
							text = " FIsSNManage = '1' ";
						}
						else
						{
							text += " AND (FIsSNManage = '1') ";
						}
					}
				}
				else
				{
					text = string.Format(" EXISTS (SELECT 1 FROM T_SEC_USERORG UO where UO.FUSERID={0} AND UO.FORGID=FORGID)", base.Context.UserId);
				}
			}
			return text;
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x0004F9A0 File Offset: 0x0004DBA0
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FORGID") && !(a == "FMATERIALID") && !(a == "FLOT"))
				{
					return;
				}
				string fieldFilter = this.GetFieldFilter(e.BaseDataFieldKey);
				if (!string.IsNullOrWhiteSpace(fieldFilter))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = fieldFilter;
						return;
					}
					e.Filter = e.Filter + " AND " + fieldFilter;
				}
			}
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0004FA30 File Offset: 0x0004DC30
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			if (e.BarItemKey.ToUpperInvariant() == "TBNEW")
			{
				this.Model.CreateNewData();
				this.View.UpdateView();
			}
			if (e.BarItemKey.ToUpperInvariant() == "TBSAVE")
			{
				OperateResultCollection operateResultCollection = null;
				bool flag = false;
				DynamicObject dynamicObject = this.View.Model.GetValue("FMaterialId") as DynamicObject;
				if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("物料不能为空!", "004023030002317", 5, new object[0]), "", 4);
					return;
				}
				DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["OrgEntity"] as DynamicObjectCollection;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["OrgId_Id"]) > 0L)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("请至少录入一条适用组织!", "004023030002320", 5, new object[0]), "", 4);
					return;
				}
				if (!this.bType)
				{
					dynamicObjectCollection = (this.View.Model.DataObject["SerialEntity"] as DynamicObjectCollection);
					if (!this.CheckSerialNoFill(dynamicObjectCollection))
					{
						return;
					}
					Dictionary<string, object> dictionary = StockServiceHelper.SaveSerialMain(base.Context, this.View.Model.DataObject, null, true);
					if (dictionary != null)
					{
						operateResultCollection = (dictionary["errorinfo"] as OperateResultCollection);
						List<string> serialEntityFlag = dictionary["serialinfo"] as List<string>;
						this.SetSerialEntityFlag(serialEntityFlag);
					}
				}
				else
				{
					string text = string.Empty;
					DynamicObject dynamicObject3 = this.View.Model.GetValue("FCodeRule") as DynamicObject;
					if (dynamicObject3 == null)
					{
						text = ResManager.LoadKDString("序列号生成失败,请先录入编码规则!", "004023030002326", 5, new object[0]);
						this.View.ShowErrMessage(text, "", 4);
						return;
					}
					int num = Convert.ToInt32(this.View.Model.GetValue("FNumber"));
					if (num <= 0)
					{
						text = ResManager.LoadKDString("序列号生成失败,请输入大于0的整数!", "004023030002329", 5, new object[0]);
						this.View.ShowErrMessage(text, "", 4);
						return;
					}
					Dictionary<string, object> dictionary2 = StockServiceHelper.SaveAutoSerialMain(base.Context, this.View.Model.DataObject, Convert.ToInt64(dynamicObject3["Id"]), num, true);
					if (dictionary2 != null)
					{
						operateResultCollection = (dictionary2["errorinfo"] as OperateResultCollection);
						List<string> lstSerials = dictionary2["serialinfo"] as List<string>;
						this.FillSerialEntity(lstSerials);
					}
				}
				if (operateResultCollection != null && operateResultCollection.Count > 0)
				{
					this._saved = operateResultCollection.Any((OperateResult p) => p.SuccessStatus);
					this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
				}
				else
				{
					this._saved = true;
				}
				if (this._saved)
				{
					this.SetFormEnabled(false);
				}
			}
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x0004FD74 File Offset: 0x0004DF74
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x0004FD80 File Offset: 0x0004DF80
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.ToUpperInvariant() == "TBCLOSE")
			{
				FormResult formResult = new FormResult(this._saved);
				this.View.ReturnToParentWindow(formResult);
				this.View.Close();
			}
			base.BarItemClick(e);
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0004FDD4 File Offset: 0x0004DFD4
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterEntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBFILLSERIALBYCOU"))
				{
					if (!(a == "TBCHECKREPEAT"))
					{
						return;
					}
					this.DoCheckRepatData();
				}
				else
				{
					List<string> serialsByCou = this.GetSerialsByCou();
					if (serialsByCou != null && serialsByCou.Count<string>() > 0)
					{
						this.FillSerialEntity(serialsByCou);
						this.SetDefOriVal();
						this.SetBillNoExampleByCou();
						return;
					}
				}
			}
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x0004FE40 File Offset: 0x0004E040
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FHEXORIVAL"))
				{
					return;
				}
				IntegerField integerField = this.View.BusinessInfo.GetField("FOriVal") as IntegerField;
				if (this.IsIllegalHexadecimal(Convert.ToString(e.Value)))
				{
					this.View.ShowWarnningMessage(ResManager.LoadKDString("格式为十六进制时，只允许录入字符（A、B、C、D、E、F）和数字（0~9）。", "004023030009700", 5, new object[0]), "", 0, null, 1);
					e.Cancel = true;
					return;
				}
				if (string.IsNullOrEmpty(Convert.ToString(e.Value)) || integerField.CheckScope(Convert.ToInt64(Convert.ToString(e.Value), 16)))
				{
					this.View.ShowWarnningMessage(string.Format(ResManager.LoadKDString("数据值录入范围为({0},{1})。", "004023030009701", 5, new object[0]), Convert.ToString(Convert.ToInt64(integerField.GetMinDataScope()), 16).ToUpperInvariant(), Convert.ToString(Convert.ToInt64(integerField.GetMaxDataScope()), 16).ToUpperInvariant()), "", 0, null, 1);
					e.Cancel = true;
					return;
				}
				e.Value = Convert.ToString(e.Value).ToUpperInvariant();
			}
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x0004FF80 File Offset: 0x0004E180
		private void SetFormEnabled(bool enable)
		{
			this.View.GetMainBarItem("tbSave").Enabled = enable;
			this.View.GetBarItem("FSerialEntity", "tbSplitNewLine").Enabled = (enable & !this.bType);
			this.View.GetBarItem("FSerialEntity", "tbNewLine").Enabled = (enable & !this.bType);
			this.View.GetBarItem("FSerialEntity", "tbInsertLine").Enabled = (enable & !this.bType);
			this.View.GetBarItem("FSerialEntity", "tbDeleteLine").Enabled = (enable & !this.bType);
			this.View.GetBarItem("FSerialEntity", "tbBatchFill").Enabled = (enable & !this.bType);
			this.View.GetControl("FSerialNumber").Enabled = (enable & !this.bType);
			this.View.GetBarItem("FSerialEntity", "tbFillSerialByCou").Enabled = (enable & !this.bType);
			this.View.GetBarItem("FSerialEntity", "tbCheckRepeat").Enabled = (enable & !this.bType);
			FieldEditor fieldEditor = this.View.GetFieldEditor("FMaterialId", -1);
			fieldEditor.Enabled = enable;
			fieldEditor = this.View.GetFieldEditor("FCodeRule", -1);
			fieldEditor.Enabled = enable;
			fieldEditor = this.View.GetFieldEditor("FNumber", -1);
			fieldEditor.Enabled = enable;
			this.View.GetControl("FOrgId").Enabled = enable;
			this.View.GetBarItem("FOrgEntity", "tbOrgSplitNewLine").Enabled = (enable & this._canMulOrg);
			this.View.GetBarItem("FOrgEntity", "tbOrgNewLine").Enabled = (enable & this._canMulOrg);
			this.View.GetBarItem("FOrgEntity", "tbOrgInsertLine").Enabled = (enable & this._canMulOrg);
			this.View.GetBarItem("FOrgEntity", "tbOrgDeleteLine").Enabled = (enable & this._canMulOrg);
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x000501B0 File Offset: 0x0004E3B0
		private bool CheckSerialNoFill(DynamicObjectCollection doc)
		{
			List<string> list = new List<string>();
			bool flag = true;
			Field field = this.View.BusinessInfo.GetField("FIsRepeat");
			DynamicObject dynamicObject = this.View.Model.GetValue("FMaterialId") as DynamicObject;
			OperateResultCollection operateResultCollection = new OperateResultCollection();
			string format = ResManager.LoadKDString("存在重复序列号,请检查录入的序列号数据！序列号【{0}】,物料编码【{1}】,物料名称【{2}】", "004023000016829", 5, new object[0]);
			foreach (DynamicObject dynamicObject2 in doc)
			{
				string text = (dynamicObject2["SerialNumber"] == null) ? "" : dynamicObject2["SerialNumber"].ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (list.Contains(text.Trim(), StringComparer.CurrentCultureIgnoreCase))
					{
						flag = false;
						this.View.Model.SetValue(field, dynamicObject2, this._repeatFlagTab);
						operateResultCollection.Add(new OperateResult
						{
							Name = ResManager.LoadKDString("序列号重复校验", "004023000016828", 5, new object[0]),
							Message = string.Format(format, text, (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["NUMBER"]), (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["NAME"])),
							SuccessStatus = false
						});
					}
					else
					{
						list.Add(text.Trim());
						this.View.Model.SetValue(field, dynamicObject2, null);
					}
				}
				else
				{
					this.View.Model.SetValue(field, dynamicObject2, null);
				}
			}
			if (!flag)
			{
				this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
			}
			else if (list.Count == 0)
			{
				flag = false;
				this.View.ShowErrMessage(ResManager.LoadKDString("序列号不能为空!", "004023030002323", 5, new object[0]), "", 4);
			}
			return flag;
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x000503C4 File Offset: 0x0004E5C4
		private void SetBillNoExample(long id)
		{
			string text = string.Format(" FID={0} )", id);
			OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, "BD_LotCodeRule", null, oqlfilter);
			if (array == null || array.Count<DynamicObject>() < 1)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			DynamicObjectCollection dynamicObjectCollection = array[0]["BD_LotCodeRuleEntry"] as DynamicObjectCollection;
			string text2 = "";
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				for (int i = 0; i < dynamicObjectCollection.Count; i++)
				{
					DynamicObject dynamicObject = dynamicObjectCollection[i];
					DynamicObject dynamicObject2 = dynamicObject["LotPropertyId"] as DynamicObject;
					text2 = ((dynamicObject["Seperator"] == null || string.IsNullOrWhiteSpace(dynamicObject["Seperator"].ToString())) ? "" : dynamicObject["Seperator"].ToString());
					if (dynamicObject2 != null)
					{
						string text3 = dynamicObject2["Type"].ToString();
						string a;
						if ((a = text3.ToUpperInvariant()) != null)
						{
							if (!(a == "BILLTEXT"))
							{
								if (!(a == "CONST"))
								{
									if (!(a == "FLOWNO"))
									{
										if (a == "CURRENTDATE")
										{
											string dateFieldValue = this.GetDateFieldValue(dynamicObject);
											stringBuilder.Append(dateFieldValue + text2);
										}
									}
									else
									{
										string billFlowNo = this.GetBillFlowNo(dynamicObject);
										stringBuilder.Append(billFlowNo + text2);
									}
								}
								else
								{
									string fixTextValue = this.GetFixTextValue(dynamicObject);
									stringBuilder.Append(fixTextValue + text2);
								}
							}
							else
							{
								string billTextValue = this.GetBillTextValue(dynamicObject);
								stringBuilder.Append(billTextValue + text2);
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text2) && stringBuilder.ToString().Length > 0)
			{
				stringBuilder.Remove(stringBuilder.ToString().Length - 1, 1);
			}
			this.View.Model.SetValue("FCodeLen", stringBuilder.ToString().Length);
			this.View.Model.SetValue("FExample", stringBuilder.ToString());
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x000505FC File Offset: 0x0004E7FC
		private string GetBillFlowNo(DynamicObject dyObj)
		{
			string text = "";
			string text2 = (dyObj["Seed"] == null) ? "1" : dyObj["Seed"].ToString();
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

		// Token: 0x06000694 RID: 1684 RVA: 0x0005071C File Offset: 0x0004E91C
		private string GetBillTextValue(DynamicObject dyObj)
		{
			string srcValue = ResManager.LoadKDString("文本", "004023000017221", 5, new object[0]);
			string sFormat = (dyObj["Format"] == null || ((DynamicObject)dyObj["Format"])["FormatValue"] == null) ? "" : ((DynamicObject)dyObj["Format"])["FormatValue"].ToString();
			return this.ConvertFormat(sFormat, srcValue);
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x0005079C File Offset: 0x0004E99C
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

		// Token: 0x06000696 RID: 1686 RVA: 0x00050834 File Offset: 0x0004EA34
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

		// Token: 0x06000697 RID: 1687 RVA: 0x0005090A File Offset: 0x0004EB0A
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

		// Token: 0x06000698 RID: 1688 RVA: 0x00050938 File Offset: 0x0004EB38
		private void FillSerialEntity(List<string> lstSerials)
		{
			if (lstSerials == null)
			{
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FSerialEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			int num = entityDataObject.Count<DynamicObject>() - 1;
			if (num >= 0)
			{
				int num2 = num;
				while (num2 >= 0 && string.IsNullOrEmpty(Convert.ToString(entityDataObject[num2]["SerialNumber"])))
				{
					entityDataObject.Remove(entityDataObject[num2]);
					num2--;
				}
			}
			num = entityDataObject.Count<DynamicObject>() + 1;
			foreach (string text in lstSerials)
			{
				DynamicObject dynamicObject = new DynamicObject(entity.DynamicObjectType);
				dynamicObject["Seq"] = num;
				dynamicObject["SerialNumber"] = text;
				num++;
				entityDataObject.Add(dynamicObject);
			}
			this.View.UpdateView("FSerialEntity");
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x00050A44 File Offset: 0x0004EC44
		private void ClearEntity(string key)
		{
			Entity entity = this.View.BusinessInfo.GetEntity(key);
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			entityDataObject.Clear();
			DynamicObject dynamicObject = new DynamicObject(entity.DynamicObjectType);
			dynamicObject["Seq"] = 1;
			entityDataObject.Add(dynamicObject);
			this.View.UpdateView(key);
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x00050AAC File Offset: 0x0004ECAC
		private void SetGenSerialByCouVisible()
		{
			this.View.GetControl("FGenSerialByCouPanel").Visible = !this.bType;
			this.View.GetBarItem("FSerialEntity", "tbFillSerialByCou").Visible = !this.bType;
			this.View.GetControl("FIsRepeat").Visible = !this.bType;
			this.View.GetBarItem("FSerialEntity", "tbCheckRepeat").Visible = !this.bType;
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x00050B3C File Offset: 0x0004ED3C
		private void SetDefOriVal()
		{
			GenSerialByOriValInfo genSerialByOriValInfo = new GenSerialByOriValInfo
			{
				Prefix = Convert.ToString(this.View.Model.GetValue("FPrefix")),
				Suffix = Convert.ToString(this.View.Model.GetValue("FSuffix"))
			};
			long num = SerialServiceHelper.GetSerialMasterByOriValInfoMaxNum(this.View.Context, genSerialByOriValInfo) + 1L;
			if (Convert.ToString(num).Length <= Convert.ToInt32(this.View.Model.GetValue("FSerLen")))
			{
				this.View.Model.SetValue("FOriVal", num);
				return;
			}
			this.View.Model.SetValue("FOriVal", 1);
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x00050C04 File Offset: 0x0004EE04
		private void SetBillNoExampleByCou()
		{
			long num = Convert.ToInt64(this.View.Model.GetValue("FOriVal"));
			long num2 = (long)Convert.ToInt32(this.Model.GetValue("FStride"));
			int num3 = Convert.ToInt32(this.Model.GetValue("FSerLen"));
			if (num < 0L || num2 < 1L || num3 < 0)
			{
				this.Model.SetValue("FExampleByCou", "");
				return;
			}
			string text = Convert.ToString(this.View.Model.GetValue("FOriVal"));
			if (Convert.ToString(this.View.Model.GetValue("FFlowNoType")).Equals("H"))
			{
				text = Convert.ToString(num, 16);
			}
			string str = Convert.ToString(this.Model.GetValue("FPrefix"));
			string str2 = Convert.ToString(this.Model.GetValue("FSuffix"));
			new StringBuilder();
			string str3 = "";
			if (num3 >= text.Length)
			{
				str3 = new string('0', num3 - text.Length) + text.ToUpperInvariant();
			}
			string text2 = str + str3 + str2;
			this.Model.SetValue("FExampleByCou", text2);
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x00050D46 File Offset: 0x0004EF46
		private bool IsIllegalHexadecimal(string str)
		{
			return Regex.IsMatch(str, "([^A-Fa-f0-9]|\\s+?)+");
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x00050D54 File Offset: 0x0004EF54
		private List<string> GetSerialsByCou()
		{
			List<string> result = new List<string>();
			int num = Convert.ToInt32(this.Model.GetValue("FNumberByCou"));
			if (num < 1)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("数量必须大于0！", "004023030009572", 5, new object[0]), "", 0);
				return result;
			}
			new Dictionary<string, object>();
			if (Convert.ToString(this.View.Model.GetValue("FFlowNoType")).Equals("H"))
			{
				this.View.BusinessInfo.GetField("FOriVal");
				if (this.IsIllegalHexadecimal(Convert.ToString(this.View.Model.GetValue("FHexOriVal"))))
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("格式为十六进制时，只允许录入字符（A、B、C、D、E、F）和数字（0~9）。", "004023030009700", 5, new object[0]), "", 0);
					return result;
				}
			}
			int num2 = Convert.ToInt32(this.View.Model.GetValue("FSerLen"));
			string text = Convert.ToString(this.View.Model.GetValue("FOriVal"));
			long oriVal = Convert.ToInt64(this.View.Model.GetValue("FOriVal"));
			if (Convert.ToString(this.View.Model.GetValue("FFlowNoType")).Equals("H"))
			{
				text = Convert.ToString(this.View.Model.GetValue("FHexOriVal"));
				oriVal = Convert.ToInt64(Convert.ToString(this.View.Model.GetValue("FHexOriVal")), 16);
			}
			if (text.Length > num2)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("流水号长度必须大于流水号起始值长度！", "004023000016735", 5, new object[0]), "", 0);
				return result;
			}
			int num3 = 80;
			Field field = this.View.BusinessInfo.GetField("FSerialNumber");
			if (field != null && (TextField)field != null)
			{
				num3 = ((TextField)field).Editlen;
				if (num3 > 255)
				{
					num3 = 255;
				}
				else if (num3 < 80)
				{
					num3 = 80;
				}
			}
			string text2 = Convert.ToString(this.View.Model.GetValue("FPrefix"));
			string text3 = Convert.ToString(this.View.Model.GetValue("FSuffix"));
			if (num2 + text2.Length + text3.Length > num3)
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("前缀的长度+流水号长度+后缀的长度，要小于等于{0}！", "004023030043823", 5, new object[0]), num3), "", 0);
				return result;
			}
			GenSerialByOriValInfo genSerialByOriValInfo = new GenSerialByOriValInfo
			{
				Prefix = text2,
				Suffix = text3,
				OriVal = oriVal,
				Stride = Convert.ToInt64(this.View.Model.GetValue("FStride")),
				FlowType = Convert.ToString(this.View.Model.GetValue("FFlowNoType")),
				Length = num2,
				Count = num,
				Stuff = '0'
			};
			List<string> list = SerialServiceHelper.GenSerialMasterByOriValInfo(this.View.Context, genSerialByOriValInfo);
			if (list != null && list.Count > 0)
			{
				result = list;
			}
			return result;
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0005109C File Offset: 0x0004F29C
		private void SetSerialEntityFlag(List<string> lstSerials)
		{
			if (lstSerials == null)
			{
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FSerialEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			if (entityDataObject == null || entityDataObject.Count<DynamicObject>() <= 0)
			{
				return;
			}
			string text = string.Empty;
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				text = Convert.ToString(dynamicObject["SerialNumber"]);
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (lstSerials != null && lstSerials.Count<string>() > 0 && lstSerials.Contains(text))
					{
						lstSerials.Remove(text);
					}
					else
					{
						dynamicObject["IsRepeat"] = this._repeatFlagMain;
					}
				}
			}
			this.View.UpdateView("FSerialEntity");
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x00051194 File Offset: 0x0004F394
		private void DoCheckRepatData()
		{
			bool flag = false;
			DynamicObject dynamicObject = this.View.Model.GetValue("FMaterialId") as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("物料不能为空!", "004023030002317", 5, new object[0]), "", 4);
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["OrgEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["OrgId_Id"]) > 0L)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请至少录入一条适用组织!", "004023030002320", 5, new object[0]), "", 4);
				return;
			}
			dynamicObjectCollection = (this.View.Model.DataObject["SerialEntity"] as DynamicObjectCollection);
			List<string> list = this.CheckSerialsRepeat(dynamicObjectCollection);
			if (list != null && list.Count > 0)
			{
				List<string> list2 = new List<string>();
				DynamicObjectCollection dynamicObjectCollection2 = this.View.Model.DataObject["OrgEntity"] as DynamicObjectCollection;
				List<long> list3 = new List<long>();
				foreach (DynamicObject dynamicObject3 in dynamicObjectCollection2)
				{
					long num = Convert.ToInt64(dynamicObject3["OrgId_Id"]);
					if (num >= 1L && !list3.Contains(num))
					{
						list3.Add(num);
					}
				}
				if (list3.Count < 1)
				{
					return;
				}
				list2 = StockServiceHelper.CheckSerialMainRepeat(base.Context, dynamicObjectCollection, list3, Convert.ToInt64(dynamicObject["msterId"]));
				if (list2 != null && list2.Count<string>() > 0)
				{
					Field field = this.View.BusinessInfo.GetField("FIsRepeat");
					List<DynamicObject> list4 = (from p in dynamicObjectCollection
					where string.IsNullOrWhiteSpace(Convert.ToString(p["IsRepeat"]))
					select p).ToList<DynamicObject>();
					foreach (DynamicObject dynamicObject4 in list4)
					{
						string item = (dynamicObject4["SerialNumber"] == null) ? "" : dynamicObject4["SerialNumber"].ToString();
						if (list2.Contains(item))
						{
							this.View.Model.SetValue(field, dynamicObject4, this._repeatFlagMain);
						}
					}
				}
			}
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x00051474 File Offset: 0x0004F674
		private List<string> CheckSerialsRepeat(DynamicObjectCollection dataCollection)
		{
			List<string> list = new List<string>();
			Field field = this.View.BusinessInfo.GetField("FIsRepeat");
			foreach (DynamicObject dynamicObject in dataCollection)
			{
				string text = (dynamicObject["SerialNumber"] == null) ? "" : dynamicObject["SerialNumber"].ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (list.Contains(text.Trim(), StringComparer.CurrentCultureIgnoreCase))
					{
						this.View.Model.SetValue(field, dynamicObject, this._repeatFlagTab);
					}
					else
					{
						list.Add(text.Trim());
						this.View.Model.SetValue(field, dynamicObject, null);
					}
				}
				else
				{
					this.View.Model.SetValue(field, dynamicObject, null);
				}
			}
			return list;
		}

		// Token: 0x0400026B RID: 619
		private bool bType = true;

		// Token: 0x0400026C RID: 620
		private bool _saved;

		// Token: 0x0400026D RID: 621
		private bool _canMulOrg;

		// Token: 0x0400026E RID: 622
		private bool bMTrack;

		// Token: 0x0400026F RID: 623
		private readonly string _repeatFlagTab = ResManager.LoadKDString("表内重复", "004023000016826", 5, new object[0]);

		// Token: 0x04000270 RID: 624
		private readonly string _repeatFlagMain = ResManager.LoadKDString("主档重复", "004023000016827", 5, new object[0]);
	}
}
