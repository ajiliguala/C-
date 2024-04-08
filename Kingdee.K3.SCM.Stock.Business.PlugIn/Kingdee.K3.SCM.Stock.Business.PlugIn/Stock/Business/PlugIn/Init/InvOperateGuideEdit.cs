using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.SystemParameter;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Init
{
	// Token: 0x02000007 RID: 7
	[Description("库存工作台插件")]
	public class InvOperateGuideEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002664 File Offset: 0x00000864
		private Dictionary<string, decimal> DctGuideTime
		{
			get
			{
				if (this.dctGuideTime == null)
				{
					this.dctGuideTime = new Dictionary<string, decimal>();
					this.dctGuideTime["STK_OPERATEGUIDE1"] = 6m;
					this.dctGuideTime["STK_OPERATEGUIDE2"] = 9m;
					this.dctGuideTime["STK_OPERATEGUIDE3"] = 12m;
					this.dctGuideTime["STK_OPERATEGUIDE4"] = 3m;
					this.dctGuideTime["STK_OPERATEGUIDE5"] = 1m;
					this.dctGuideTime["STK_OPERATEGUIDE6"] = 12m;
					this.dctGuideTime["STK_OPERATEGUIDE7"] = 12m;
				}
				return this.dctGuideTime;
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000272C File Offset: 0x0000092C
		public override void AfterBindData(EventArgs e)
		{
			this.InitBtnFinishText();
			this.iOrgId = this.GetInitOrgID();
			List<long> list = this.GetStockOrgList();
			if (!list.Contains(this.iOrgId))
			{
				this.iOrgId = 0L;
			}
			else
			{
				list = this.GetGuidePermissionOrgIds();
				if (!list.Contains(this.iOrgId))
				{
					this.iOrgId = 0L;
				}
			}
			this.View.Model.SetValue("FOrgId", this.iOrgId);
			this.View.UpdateView("FOrgId");
			this.SetBtnStyleByOperateResult();
			this.RefreshStaStkStatus();
			this.RefreshEndInitStatus();
			base.AfterBindData(e);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000027D0 File Offset: 0x000009D0
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (e.Key.ToUpperInvariant() == "FORGID" && e.Value != null)
			{
				int num = Convert.ToInt32((e.Value as DynamicObject)["ID"]);
				List<long> list = this.GetGuidePermissionOrgIds();
				if (!list.Contains((long)num))
				{
					e.Cancel = true;
				}
				else
				{
					list = this.GetStockOrgList();
					if (!list.Contains((long)num))
					{
						e.Cancel = true;
					}
				}
			}
			base.BeforeUpdateValue(e);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002850 File Offset: 0x00000A50
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpper()) != null && a == "FORGID")
			{
				this.iOrgId = (long)Convert.ToInt32(e.NewValue);
				this.SetBtnStyleByOperateResult();
				this.RefreshStaStkStatus();
				this.RefreshEndInitStatus();
			}
			base.DataChanged(e);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000028AC File Offset: 0x00000AAC
		public override void AfterButtonClick(AfterButtonClickEventArgs e)
		{
			base.AfterButtonClick(e);
			string key;
			switch (key = e.Key.ToUpperInvariant())
			{
			case "FBTN1":
				this.View.GetControl(e.Key).Text = this.dctKeyFinishText["FBTN1"];
				this.SetFinishBtnBackImage(e.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE1");
				return;
			case "FBTN2":
				this.View.GetControl(e.Key).Text = this.dctKeyFinishText["FBTN2"];
				this.SetFinishBtnBackImage(e.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE2");
				return;
			case "FLKITEMSETUNIT":
				this.ListShowParameter("BD_UNIT");
				return;
			case "FLKITEMSETLOC":
				this.ListShowParameter("BD_FLEXVALUES");
				return;
			case "FLKITEMSETSTOCK":
				this.ListShowParameter("BD_STOCK");
				return;
			case "FLKITEMSETLOT":
				this.ListShowParameter("BD_LotCodeRule");
				return;
			case "FLKITEMSETAUX":
				this.ListShowParameter("BD_FLEXAUXPROPERTY");
				return;
			case "FLKITEMSETMAT":
				this.ListShowParameter("BD_MATERIAL");
				return;
			case "FLKITEMSETSTAFF":
				this.ListShowParameter("BD_OPERATOR");
				return;
			case "FLKITEM3":
				break;
			case "FBTN3":
				this.View.GetControl(e.Key).Text = this.dctKeyFinishText["FBTN3"];
				this.SetFinishBtnBackImage(e.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE3");
				return;
			case "FLKITEMPARAMETERSET":
				this.ShowSysParamForm();
				this.View.GetControl("FBtn4").Text = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.SetFinishBtnBackImage("FBtn4");
				this.SaveFinishResult("STK_OPERATEGUIDE4");
				return;
			case "FBTN4":
				this.View.GetControl(e.Key).Text = this.dctKeyFinishText["FBTN4"];
				this.SetFinishBtnBackImage(e.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE4");
				return;
			case "FLKITEMSTASTK":
				this.ShowStaForm("STK_StartStock", new Action<FormResult>(this.RefreshStaStkStatus));
				return;
			case "FBTN5":
				if (this.iOrgId > 0L)
				{
					this.RefreshStaStkStatus();
					return;
				}
				this.View.GetControl(e.Key).Text = this.dctKeyFinishText["FBTN5"];
				this.SetFinishBtnBackImage(e.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE5");
				return;
			case "FLKITEMSTOCKMODIFY":
				this.ShowBillForm("STK_InvInit");
				return;
			case "FLKITEMCLOSESTOCK":
				this.ShowCloseForm("STK_Init", new Action<FormResult>(this.RefreshEndInitStatus));
				return;
			case "FBTN6":
				if (this.iOrgId > 0L)
				{
					this.RefreshEndInitStatus();
					return;
				}
				this.View.GetControl(e.Key).Text = this.dctKeyFinishText["FBTN6"];
				this.SetFinishBtnBackImage(e.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE6");
				return;
			case "FBTN7":
				this.FinishAll();
				break;

				return;
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002CB8 File Offset: 0x00000EB8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FORGID"))
				{
					return;
				}
				List<long> guidePermissionOrgIds = this.GetGuidePermissionOrgIds();
				ExtJoinTableDescription item = new ExtJoinTableDescription
				{
					TableName = string.Format("(select /*+ cardinality(b {0})*/fid from table(fn_StrSplit(@FORGIDS, ',', 1)) b)", guidePermissionOrgIds.Count),
					TableNameAs = "sp",
					FieldName = "FID",
					ScourceKey = "FOrgID"
				};
				List<SqlParam> list = new List<SqlParam>();
				list.Add(new SqlParam("@FORGIDS", 161, guidePermissionOrgIds.Distinct<long>().ToArray<long>()));
				((ListShowParameter)e.DynamicFormShowParameter).SqlParams = list;
				((ListShowParameter)e.DynamicFormShowParameter).ExtJoinTables.Add(item);
				e.ListFilterParameter.Filter = string.Format(" FORGFUNCTIONS LIKE '%{0}%'  AND NOT EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\n                                                                        WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\n                                                                        AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ", 103L.ToString());
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002DA8 File Offset: 0x00000FA8
		private void InitBtnFinishText()
		{
			if (this.dctKeyFinishText.Keys.Count == 0)
			{
				this.dctKeyFinishText["FBTN1"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.dctKeyFinishText["FBTN2"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.dctKeyFinishText["FBTN3"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.dctKeyFinishText["FBTN4"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.dctKeyFinishText["FBTN5"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.dctKeyFinishText["FBTN6"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
				this.dctKeyFinishText["FBTN7"] = ResManager.LoadKDString("已完成", "00444711030009546", 5, new object[0]);
			}
			if (this.dctKeyUnFinishText.Keys.Count == 0)
			{
				this.dctKeyUnFinishText["FBTN1"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
				this.dctKeyUnFinishText["FBTN2"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
				this.dctKeyUnFinishText["FBTN3"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
				this.dctKeyUnFinishText["FBTN4"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
				this.dctKeyUnFinishText["FBTN5"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
				this.dctKeyUnFinishText["FBTN6"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
				this.dctKeyUnFinishText["FBTN7"] = ResManager.LoadKDString("未完成", "00444711030009548", 5, new object[0]);
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002FF4 File Offset: 0x000011F4
		private List<long> GetStockOrgList()
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = string.Format(" FORGFUNCTIONS LIKE '%{0}%' AND NOT EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\n                                                                        WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\n                                                                        AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1')  ", 103L.ToString())
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			List<long> list2 = new List<long>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				list2.Add(Convert.ToInt64(dynamicObject["FORGID"]));
			}
			return list2;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000030B8 File Offset: 0x000012B8
		private List<long> GetGuidePermissionOrgIds()
		{
			BusinessObject businessObject = new BusinessObject();
			businessObject.Id = "STK_OperateGuide";
			businessObject.PermissionControl = 1;
			businessObject.SubSystemId = "STK";
			return PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000030FC File Offset: 0x000012FC
		private void SetBtnStyleByOperateResult()
		{
			string text = string.Format("SELECT FKEY FROM T_BAS_SYSTEMPROFILE WHERE FCATEGORY='STK' AND FORGID= {0} AND FKEY LIKE 'STK_OPERATEGUIDE%'", this.iOrgId);
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, new SqlParam[0]);
			decimal d = 0m;
			List<string> list = new List<string>
			{
				"FBTN1",
				"FBTN2",
				"FBTN3",
				"FBTN4",
				"FBTN5",
				"FBTN6",
				"FBTN7"
			};
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				string text2 = dynamicObject["Fkey"].ToString();
				d += this.DctGuideTime[text2];
				if (text2.Length == 17)
				{
					string s = text2.Substring(16);
					int num = 0;
					if (int.TryParse(s, out num) && num >= 1 && num <= 7)
					{
						string text3 = "FBTN" + num;
						list.Remove(text3);
						this.View.GetControl(text3).Text = this.dctKeyFinishText[text3];
						this.SetFinishBtnBackImage(text3);
					}
				}
			}
			foreach (string text4 in list)
			{
				this.View.GetControl(text4).Text = this.dctKeyUnFinishText[text4];
				this.SetUnFinishBtnBackImage(text4);
			}
			ProgressBar control = this.View.GetControl<ProgressBar>("FBar");
			control.InvokeControlMethod("SetCurrentValue", new object[]
			{
				MathUtil.Round(d * 100m / 55m, 0, 0)
			});
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00003324 File Offset: 0x00001524
		private void SetFinishBtnBackImage(string strKey)
		{
			JSONObject jsonobject = new JSONObject();
			jsonobject["background-image"] = "url('../images/biz/default/InitImplementation/GL_InitGuide/btnItem3.png')";
			this.View.GetControl<Button>(strKey).SetCustomPropertyValue("InlineStyle", jsonobject);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00003360 File Offset: 0x00001560
		private void SetUnFinishBtnBackImage(string strKey)
		{
			JSONObject jsonobject = new JSONObject();
			jsonobject["background-image"] = "url('../images/biz/default/InitImplementation/GL_InitGuide/btnItem4.png')";
			this.View.GetControl<Button>(strKey).SetCustomPropertyValue("InlineStyle", jsonobject);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000339C File Offset: 0x0000159C
		private void SaveFinishResult(string strValue)
		{
			if (DBServiceHelper.ExecuteScalar<int>(base.Context, "SELECT 1 FROM T_BAS_SYSTEMPROFILE WHERE FCATEGORY='STK' AND FORGID=@FORGID AND FKEY=@FVALUE", 0, new SqlParam[]
			{
				new SqlParam("@FORGID", 11, this.iOrgId),
				new SqlParam("@FVALUE", 16, strValue)
			}) == 0)
			{
				DBServiceHelper.Execute(base.Context, "INSERT INTO T_BAS_SYSTEMPROFILE(FCATEGORY,FORGID,FKEY) VALUES('STK',@FORGID,@FVALUE)", new List<SqlParam>
				{
					new SqlParam("@FORGID", 11, this.iOrgId),
					new SqlParam("@FVALUE", 16, strValue)
				});
				this.SetProgressBarValue();
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00003440 File Offset: 0x00001640
		private void SetProgressBarValue()
		{
			string text = string.Format("SELECT FKEY FROM T_BAS_SYSTEMPROFILE WHERE FCATEGORY='STK' AND FORGID= {0} AND FKEY LIKE 'STK_OPERATEGUIDE%'", this.iOrgId);
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, new SqlParam[0]);
			decimal d = 0m;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				string key = dynamicObject["Fkey"].ToString();
				d += this.DctGuideTime[key];
			}
			ProgressBar control = this.View.GetControl<ProgressBar>("FBar");
			control.InvokeControlMethod("SetCurrentValue", new object[]
			{
				MathUtil.Round(d * 100m / 55m, 0, 0)
			});
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00003530 File Offset: 0x00001730
		private void ListShowParameter(string FormId)
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = FormId;
			listShowParameter.ObjectTypeId = FormId;
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.PageId = Guid.NewGuid().ToString();
			listShowParameter.CustomComplexParams.Add("OrgId", this.iOrgId);
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000035B0 File Offset: 0x000017B0
		private void ShowBillForm(string FormId)
		{
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = FormId;
			billShowParameter.ParentPageId = this.View.PageId;
			billShowParameter.Status = 0;
			billShowParameter.OpenStyle.ShowType = 7;
			billShowParameter.CustomComplexParams.Add("OrgId", this.iOrgId);
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003618 File Offset: 0x00001818
		private void ShowSysParamForm()
		{
			SystemParameterShowParameter systemParameterShowParameter = new SystemParameterShowParameter();
			systemParameterShowParameter.FormId = "BOS_ParameterSetBase";
			systemParameterShowParameter.ObjectTypeId = "STK_StockParameter";
			systemParameterShowParameter.OpenStyle.ShowType = 7;
			systemParameterShowParameter.Caption = ResManager.LoadKDString("库存管理系统参数", "00444711030009554", 5, new object[0]);
			systemParameterShowParameter.PageId = Guid.NewGuid().ToString();
			systemParameterShowParameter.CustomComplexParams.Add("OrgId", Convert.ToString(this.iOrgId));
			this.View.ShowForm(systemParameterShowParameter);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000036AC File Offset: 0x000018AC
		private void FinishAll()
		{
			foreach (KeyValuePair<string, string> keyValuePair in this.dctKeyFinishText)
			{
				string key;
				if ((key = keyValuePair.Key) != null)
				{
					if (!(key == "FBTN5"))
					{
						if (key == "FBTN6")
						{
							bool flag = BDServiceHelper.IsInvInit(this.View.Context, Convert.ToString(this.iOrgId));
							if (flag)
							{
								this.View.GetControl(keyValuePair.Key).Text = keyValuePair.Value;
								this.SetFinishBtnBackImage(keyValuePair.Key);
								this.SaveFinishResult("STK_OPERATEGUIDE" + keyValuePair.Key.Substring(4));
								continue;
							}
							continue;
						}
					}
					else
					{
						object updateStockDate = StockServiceHelper.GetUpdateStockDate(this.View.Context, this.iOrgId);
						if (updateStockDate != null)
						{
							this.View.GetControl(keyValuePair.Key).Text = keyValuePair.Value;
							this.SetFinishBtnBackImage(keyValuePair.Key);
							this.SaveFinishResult("STK_OPERATEGUIDE" + keyValuePair.Key.Substring(4));
							continue;
						}
						continue;
					}
				}
				this.View.GetControl(keyValuePair.Key).Text = keyValuePair.Value;
				this.SetFinishBtnBackImage(keyValuePair.Key);
				this.SaveFinishResult("STK_OPERATEGUIDE" + keyValuePair.Key.Substring(4));
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003858 File Offset: 0x00001A58
		private void ShowCloseForm(string strFormId, Action<FormResult> action = null)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = strFormId;
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.PageId = Guid.NewGuid().ToString();
			dynamicFormShowParameter.CustomComplexParams.Add("Direct", "C");
			this.View.ShowForm(dynamicFormShowParameter, action);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000038BC File Offset: 0x00001ABC
		private void ShowStaForm(string strFormId, Action<FormResult> action = null)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = strFormId;
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.PageId = Guid.NewGuid().ToString();
			this.View.ShowForm(dynamicFormShowParameter, action);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00003908 File Offset: 0x00001B08
		private void RefreshStaStkStatus()
		{
			if (this.iOrgId > 0L)
			{
				object updateStockDate = StockServiceHelper.GetUpdateStockDate(this.View.Context, this.iOrgId);
				if (updateStockDate != null)
				{
					this.View.GetControl("FBtn5").Text = this.dctKeyFinishText["FBTN5"];
					this.SetFinishBtnBackImage("FBtn5");
					this.SaveFinishResult("STK_OPERATEGUIDE5");
					return;
				}
				this.View.GetControl("FBtn5").Text = this.dctKeyUnFinishText["FBTN5"];
				this.SetUnFinishBtnBackImage("FBtn5");
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000039A8 File Offset: 0x00001BA8
		private void RefreshEndInitStatus()
		{
			if (this.iOrgId > 0L)
			{
				bool flag = BDServiceHelper.IsInvInit(this.View.Context, Convert.ToString(this.iOrgId));
				if (flag)
				{
					this.View.GetControl("FBtn6").Text = this.dctKeyFinishText["FBTN6"];
					this.SetFinishBtnBackImage("FBtn6");
					this.SaveFinishResult("STK_OPERATEGUIDE6");
					return;
				}
				this.View.GetControl("FBtn6").Text = this.dctKeyUnFinishText["FBTN6"];
				this.SetUnFinishBtnBackImage("FBtn6");
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003A4D File Offset: 0x00001C4D
		private void RefreshStaStkStatus(FormResult formResult)
		{
			this.RefreshEndInitStatus();
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003A55 File Offset: 0x00001C55
		private void RefreshEndInitStatus(FormResult formResult)
		{
			this.RefreshEndInitStatus();
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003A60 File Offset: 0x00001C60
		private long GetInitOrgID()
		{
			long result;
			try
			{
				string text = (this.View.OpenParameter.GetCustomParameter("SelectOrgID") != null) ? this.View.OpenParameter.GetCustomParameter("SelectOrgID").ToString() : string.Empty;
				if (!string.IsNullOrEmpty(text))
				{
					result = long.Parse(text);
				}
				else
				{
					result = base.Context.CurrentOrganizationInfo.ID;
				}
			}
			catch
			{
				result = base.Context.CurrentOrganizationInfo.ID;
			}
			return result;
		}

		// Token: 0x04000009 RID: 9
		private const string FONT_MSYH = "微软雅黑";

		// Token: 0x0400000A RID: 10
		private const string C_FORMID = "STK_OperateGuide";

		// Token: 0x0400000B RID: 11
		private int FONT_BIG_SIZE = 20;

		// Token: 0x0400000C RID: 12
		private int FONT_MID_SIZE = 14;

		// Token: 0x0400000D RID: 13
		private int FONT_SMALL_SIZE = 13;

		// Token: 0x0400000E RID: 14
		private Dictionary<string, string> dctKeyFinishText = new Dictionary<string, string>();

		// Token: 0x0400000F RID: 15
		private Dictionary<string, string> dctKeyUnFinishText = new Dictionary<string, string>();

		// Token: 0x04000010 RID: 16
		private Dictionary<string, decimal> dctGuideTime;

		// Token: 0x04000011 RID: 17
		private long iOrgId = 1L;
	}
}
