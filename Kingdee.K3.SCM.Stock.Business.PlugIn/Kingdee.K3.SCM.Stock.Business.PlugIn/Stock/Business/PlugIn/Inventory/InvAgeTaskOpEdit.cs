using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Inventory
{
	// Token: 0x0200000B RID: 11
	[Description("库龄定时计算操作表单插件")]
	public class InvAgeTaskOpEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00005658 File Offset: 0x00003858
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "STK_InvAgeTask", true) as FormMetadata;
			this._biTask = formMetadata.BusinessInfo;
			formMetadata = (MetaDataServiceHelper.Load(base.Context, "BOS_SCHEDULETYPE", true) as FormMetadata);
			this._biSchedule = formMetadata.BusinessInfo;
			formMetadata = (MetaDataServiceHelper.Load(base.Context, "ORG_Organizations", true) as FormMetadata);
			this._bizOrg = formMetadata.BusinessInfo;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000056D8 File Offset: 0x000038D8
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			DateTime date = TimeServiceHelper.GetSystemDateTime(this.View.Context).Date;
			this._task = new DynamicObject(this._biTask.GetDynamicObjectType());
			this._task["FCreatorId_Id"] = base.Context.UserId;
			this._task["FCreateDate"] = date;
			this._task["FModifierId_Id"] = base.Context.UserId;
			this._task["FModifyDate"] = date;
			this._task["DocumentStatus"] = "A";
			this._task["ForbidStatus"] = "A";
			this._scheduleType = new DynamicObject(this._biSchedule.GetDynamicObjectType());
			this._scheduleInfo = new DynamicObject((this._scheduleType["SCHEDULEINFO"] as DynamicObjectCollection).DynamicCollectionItemPropertyType);
			this._scheduleType["CREATETIME"] = date;
			this._scheduleType["SCHEDULECLASS"] = "Kingdee.K3.SCM.App.Core.InvAge.InvAgeAnalizeService,Kingdee.K3.SCM.App.Core";
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00005818 File Offset: 0x00003A18
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBNEW")
				{
					this.Model.CreateNewData();
					this.View.Refresh();
					return;
				}
				if (!(a == "TBSAVE"))
				{
					if (!(a == "TBLIST"))
					{
						return;
					}
					this.ShowTaskList();
				}
				else if (this.CheckAndGetData())
				{
					this.SaveTaskInfo();
					return;
				}
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00005890 File Offset: 0x00003A90
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FSCHEMEID"))
				{
					return;
				}
				string text;
				if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
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
				e.EnableUICache = false;
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000591C File Offset: 0x00003B1C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpper()) != null)
			{
				if (!(a == "FSCHEMEID"))
				{
					return;
				}
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
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

		// Token: 0x06000057 RID: 87 RVA: 0x00005990 File Offset: 0x00003B90
		private bool GetFieldFilter(string fieldKey, out string filter, int rowIndex)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FSCHEMEID")
			{
				filter = " NOT EXISTS(SELECT 1 FROM T_STK_INVAGETASK TS WHERE TS.FSCHEMEID = FID) ";
			}
			return true;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x000059D0 File Offset: 0x00003BD0
		private bool CheckAndGetData()
		{
			bool result = true;
			StringBuilder stringBuilder = new StringBuilder();
			DynamicObject dynamicObject = this.Model.GetValue("FSchemeId") as DynamicObject;
			if (dynamicObject == null)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("库龄计算方案为必录项", "004023000022650", 5, new object[0]));
				result = false;
			}
			else
			{
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
				queryBuilderParemeter.FormId = "STK_InvAgeTask";
				queryBuilderParemeter.SelectItems = new List<SelectorItemInfo>();
				queryBuilderParemeter.SelectItems.Add(new SelectorItemInfo("FNumber"));
				queryBuilderParemeter.SelectItems.Add(new SelectorItemInfo("FName"));
				queryBuilderParemeter.FilterClauseWihtKey = string.Format("FSchemeId = {0} ", dynamicObject["Id"]);
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(this.View.Context, queryBuilderParemeter, null);
				if (dynamicObjectCollection.Count > 0)
				{
					stringBuilder.AppendLine().AppendFormat(ResManager.LoadKDString("选择的库龄计算方案已经存在定时计算[{0}][{1}]", "004023030009446", 5, new object[0]), dynamicObjectCollection[0]["FNumber"], dynamicObjectCollection[0]["FName"]);
					result = false;
				}
				else
				{
					this._task["SchemeId"] = dynamicObject;
					this._task["SchemeId_Id"] = dynamicObject["Id"];
				}
			}
			object value = this.Model.GetValue("FBeginTimeRule");
			string text = Convert.ToString(value);
			this._task["BeginTimeRule"] = text;
			DateTime minValue = DateTime.MinValue;
			if (text == "11")
			{
				value = this.Model.GetValue("FSepcifyDate");
				if (value != null)
				{
					DateTime.TryParse(value.ToString(), out minValue);
				}
				if (minValue == DateTime.MinValue)
				{
					result = false;
					stringBuilder.AppendLine(ResManager.LoadKDString("库龄查询规则为指定日期时库龄查询日期为必录项", "004023000022643", 5, new object[0]));
				}
				else if (!this.CheckOrgStartDate(stringBuilder, minValue))
				{
					result = false;
				}
				else
				{
					this._task["SpecifyDate"] = minValue;
				}
			}
			LocaleValue localeValue = this.Model.GetValue("FName") as LocaleValue;
			if (string.IsNullOrWhiteSpace(localeValue.ToString()))
			{
				result = false;
				stringBuilder.AppendLine(ResManager.LoadKDString("任务名称为必录项", "004023000022644", 5, new object[0]));
			}
			this._task["Name"] = localeValue;
			this._scheduleType["NAME"] = localeValue;
			this._scheduleType["DESCRIPTION"] = this.Model.GetValue("FDESCRIPTION");
			minValue = DateTime.MinValue;
			value = this.Model.GetValue("FEXECUTETIME");
			if (value != null)
			{
				DateTime.TryParse(value.ToString(), out minValue);
			}
			if (minValue == DateTime.MinValue)
			{
				result = false;
				stringBuilder.AppendLine(ResManager.LoadKDString("执行时间为必录项", "004023000022645", 5, new object[0]));
			}
			else
			{
				this._scheduleInfo["EXECUTETIME"] = minValue;
			}
			minValue = DateTime.MinValue;
			value = this.Model.GetValue("FBEGINTIME");
			if (value != null)
			{
				DateTime.TryParse(value.ToString(), out minValue);
			}
			if (minValue == DateTime.MinValue)
			{
				result = false;
				stringBuilder.AppendLine(ResManager.LoadKDString("开始时间为必录项", "004023000022646", 5, new object[0]));
			}
			else
			{
				this._scheduleInfo["BEGINTIME"] = minValue;
			}
			minValue = DateTime.MinValue;
			value = this.Model.GetValue("FENDTIME");
			if (value != null)
			{
				DateTime.TryParse(value.ToString(), out minValue);
			}
			if (minValue == DateTime.MinValue)
			{
				result = false;
				stringBuilder.AppendLine(ResManager.LoadKDString("结束时间为必录项", "004023000022647", 5, new object[0]));
			}
			else
			{
				this._scheduleInfo["ENDTIME"] = minValue;
			}
			int num = Convert.ToInt32(this.Model.GetValue("FEXECUTEINTERVALUNIT"));
			this._scheduleInfo["EXECUTEINTERVALUNIT"] = num;
			if (num == 6)
			{
				string text2 = Convert.ToString(this.Model.GetValue("FCRON"));
				if (string.IsNullOrWhiteSpace(text2))
				{
					result = false;
					stringBuilder.AppendLine(ResManager.LoadKDString("CRON表达式不允许为空！", "004023030009453", 5, new object[0]));
				}
				else
				{
					if (!CronExpression.IsValidExpression(text2))
					{
						result = false;
						stringBuilder.AppendLine(ResManager.LoadKDString("CRON表达式不正确！", "004023030009454", 5, new object[0]));
					}
					this._scheduleInfo["FCRON"] = text2;
					this._scheduleInfo["EXECUTEINTERVAL"] = 0;
				}
			}
			else
			{
				int num2 = Convert.ToInt32(this.Model.GetValue("FEXECUTEINTERVAL"));
				if (num2 < 1)
				{
					result = false;
					stringBuilder.AppendLine(ResManager.LoadKDString("执行间隔必须大于0", "004023000022648", 5, new object[0]));
				}
				else
				{
					this._scheduleInfo["EXECUTEINTERVAL"] = num2;
					this._scheduleInfo["FCRON"] = " ";
				}
			}
			this._scheduleInfo["ISASYNCJOB"] = false;
			this._scheduleInfo["STATUS"] = 0.ToString("D");
			this._scheduleInfo["FISAutoExecute"] = false;
			this._scheduleInfo["FAutoRecoveryTime"] = 1440;
			(this._scheduleType["SCHEDULEINFO"] as DynamicObjectCollection).Clear();
			(this._scheduleType["SCHEDULEINFO"] as DynamicObjectCollection).Add(this._scheduleInfo);
			if (stringBuilder.Length > 0)
			{
				this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
			}
			return result;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00005FB0 File Offset: 0x000041B0
		private bool CheckOrgStartDate(StringBuilder sb, DateTime dtValue)
		{
			bool result = true;
			DynamicObject dynamicObject = this.Model.GetValue("FSchemeId") as DynamicObject;
			if (dynamicObject == null)
			{
				return true;
			}
			DynamicObjectCollection dynamicObjectCollection = dynamicObject["EntityOrg"] as DynamicObjectCollection;
			if (dynamicObjectCollection.Count < 1)
			{
				return true;
			}
			List<long> list = new List<long>();
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				list.Add(Convert.ToInt64(dynamicObject2["StockOrg_Id"]));
			}
			Dictionary<string, object> batchStockDate = StockServiceHelper.GetBatchStockDate(this.View.Context, list);
			DateTime dateTime = DateTime.MaxValue;
			foreach (object obj in batchStockDate.Values)
			{
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					DateTime dateTime2 = Convert.ToDateTime(obj);
					if (dateTime2 < dateTime)
					{
						dateTime = dateTime2;
					}
				}
			}
			if (dtValue < dateTime)
			{
				result = false;
				sb.AppendLine().AppendFormat(ResManager.LoadKDString("库龄查询日期不能小于库存组织范围中的最小启用日期{0}", "004023000022649", 5, new object[0]), dateTime);
			}
			return result;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00006108 File Offset: 0x00004308
		private void SaveTaskInfo()
		{
			IOperationResult operationResult = BusinessDataServiceHelper.Save(this.View.Context, this._biSchedule, this._scheduleType, null, "");
			if (operationResult.IsSuccess)
			{
				this._task["ScheduleId"] = this._scheduleType;
				this._task["ScheduleId_Id"] = this._scheduleType["Id"];
				BusinessDataServiceHelper.Save(this.View.Context, this._biTask, this._task, null, "");
				this.View.Model.CreateNewData();
				this.View.Refresh();
				return;
			}
			if (operationResult.ValidationErrors != null && operationResult.ValidationErrors.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
				{
					stringBuilder.AppendLine(validationErrorInfo.Message);
				}
				this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
				return;
			}
			this.View.ShowErrMessage(ResManager.LoadKDString("由于未知原因，执行计划创建失败！", "004023000023290", 5, new object[0]), "", 0);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00006260 File Offset: 0x00004460
		private void ShowTaskList()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_InvAgeTask";
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsLookUp = false;
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x04000022 RID: 34
		private DynamicObject _task;

		// Token: 0x04000023 RID: 35
		private DynamicObject _scheduleType;

		// Token: 0x04000024 RID: 36
		private DynamicObject _scheduleInfo;

		// Token: 0x04000025 RID: 37
		private BusinessInfo _biTask;

		// Token: 0x04000026 RID: 38
		private BusinessInfo _biSchedule;

		// Token: 0x04000027 RID: 39
		private BusinessInfo _bizOrg;
	}
}
