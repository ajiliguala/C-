using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Inventory
{
	// Token: 0x0200000A RID: 10
	[Description("库龄定时计算列表插件")]
	public class InvAgeTaskList : AbstractListPlugIn
	{
		// Token: 0x06000041 RID: 65 RVA: 0x00004B91 File Offset: 0x00002D91
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			this.AddMustSelFields();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004BA0 File Offset: 0x00002DA0
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBNEWTASK"))
				{
					if (!(a == "TBSETTOSTANBY"))
					{
						if (!(a == "TBSETTOSTOP"))
						{
							if (!(a == "TBTEST"))
							{
								return;
							}
							string operateName = ResManager.LoadKDString("立即执行", "004023030009260", 5, new object[0]);
							string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
							if (!string.IsNullOrWhiteSpace(onlyViewMsg))
							{
								this.View.ShowErrMessage(onlyViewMsg, "", 0);
								return;
							}
							this.TestSchedule();
						}
						else
						{
							string operateName = ResManager.LoadKDString("恢复任务为停止状态", "004023030009259", 5, new object[0]);
							string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
							if (!string.IsNullOrWhiteSpace(onlyViewMsg))
							{
								this.View.ShowErrMessage(onlyViewMsg, "", 0);
								return;
							}
							this.SetTaskScheduleStatus("1");
							return;
						}
					}
					else
					{
						string operateName = ResManager.LoadKDString("恢复任务为准备状态", "004023030009258", 5, new object[0]);
						string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
						if (!string.IsNullOrWhiteSpace(onlyViewMsg))
						{
							this.View.ShowErrMessage(onlyViewMsg, "", 0);
							return;
						}
						this.SetTaskScheduleStatus("0");
						return;
					}
				}
				else
				{
					string operateName = ResManager.LoadKDString("新增", "004023030009256", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						return;
					}
					if (InvAgeTaskList.CheckPermission(base.Context, "STK_InvAgeTask", "", "fce8b1aca2144beeb3c6655eaf78bc34"))
					{
						this.ShowNewTaskForm();
						return;
					}
					this.View.ShowMessage(ResManager.LoadKDString("对不起，您没有库龄定时计算的新增权限!", "004023000022704", 5, new object[0]), 0);
					return;
				}
			}
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00004D94 File Offset: 0x00002F94
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (e.OperationResult == null || !e.OperationResult.IsSuccess)
			{
				return;
			}
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "DELETE"))
				{
					return;
				}
				string[] array = (from i in this.ListView.SelectedRowsInfo
				select Convert.ToString(i.DataRow["FSCHEDULEID_ID"])).ToArray<string>();
				if (array.Length < 1)
				{
					this.View.ShowMessage(ResManager.LoadKDString("请先选中要操作的数据！", "004023000022653", 5, new object[0]), 0);
					return;
				}
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BOS_SCHEDULETYPE", true) as FormMetadata;
				BusinessDataServiceHelper.Delete(base.Context, formMetadata.BusinessInfo, array, null, "");
				string[] array2 = (from i in this.ListView.SelectedRowsInfo
				select Convert.ToString(i.DataRow["FID"])).ToArray<string>();
				if (array.Length < 1)
				{
					return;
				}
				foreach (string arg in array2)
				{
					string text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGERET{0}') DROP TABLE TINVAGERET{0} ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGERET{0}A') DROP TABLE TINVAGERET{0}A ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGERET{0}F_0') DROP TABLE TINVAGERET{0}F_0 ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGERET{0}I_0') DROP TABLE TINVAGERET{0}I_0 ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGERET{0}O_0') DROP TABLE TINVAGERET{0}O_0 ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGEBAL{0}') DROP TABLE TINVAGEBAL{0} ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGE{0}_0TM') DROP TABLE TINVAGE{0}_0TM ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
					text = string.Format("IF EXISTS (SELECT 1 FROM KSQL_USERTABLES WHERE KSQL_TABNAME = 'TINVAGE{0}_0TR') DROP TABLE TINVAGE{0}_0TR ", arg);
					DBServiceHelper.Execute(this.View.Context, text);
				}
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004FFC File Offset: 0x000031FC
		private void AddMustSelFields()
		{
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
			string selKey = "FScheduleId";
			Field field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, selKey));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, selKey)) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = selKey,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 167,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00005124 File Offset: 0x00003324
		private void ShowNewTaskForm()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = "STK_InvAgeTaskOp";
			dynamicFormShowParameter.Height = 750;
			dynamicFormShowParameter.Width = 1000;
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00005184 File Offset: 0x00003384
		public static bool CheckPermission(Context ctx, string sFormId, string billId, string sPerItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(ctx, new BusinessObject
			{
				Id = sFormId,
				pkId = billId
			}, sPerItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00005284 File Offset: 0x00003484
		private void SetTaskScheduleStatus(string status)
		{
			string[] scheduleTypeIds = (from i in this.ListView.SelectedRowsInfo
			select Convert.ToString(i.DataRow["FSCHEDULEID_ID"])).ToArray<string>();
			if (scheduleTypeIds.Length < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先选中要操作的数据！", "004023000022653", 5, new object[0]), 0);
				return;
			}
			List<string> ids = (from p in this.ListView.SelectedRowsInfo
			select p.PrimaryKeyValue).Distinct<string>().ToList<string>();
			if (!InvAgeTaskList.CheckPermission(base.Context, this.View.BillBusinessInfo.GetForm().Id, ids[0], "575bdf79c43af4"))
			{
				this.View.ShowMessage(ResManager.LoadKDString("对不起，您没有库龄定时计算的设置任务状态权限!", "004023000022654", 5, new object[0]), 0);
				return;
			}
			if (status == "1")
			{
				this.SetScheduleState(status, scheduleTypeIds);
				return;
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = this.Model.BillBusinessInfo.GetForm().Id;
			queryBuilderParemeter.SelectItems = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FSCHEDULENAME"),
				new SelectorItemInfo("FID")
			};
			queryBuilderParemeter.FilterClauseWihtKey = string.Format(" FID IN ({0}) AND FPROCESSSTATUS = 'R' ", string.Join(",", ids));
			DynamicObjectCollection bills = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (bills != null && bills.Count > 0)
			{
				string arg = string.Join<object>(",", from p in bills
				select p["FSCHEDULENAME"]);
				string text = string.Format(ResManager.LoadKDString("库龄定时计算【{0}】的执行状态为运行中，恢复为准备状态并重新执行可能会导致运算报错或结果错误，是否继续？", "004023030009074", 5, new object[0]), arg);
				this.View.ShowMessage(text, 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						ids = (from p in bills
						select p["FID"].ToString()).ToList<string>();
						string text2 = string.Format("UPDATE T_STK_INVAGETASK SET FPROCESSSTATUS = ' ' WHERE FID IN ({0}) AND  FPROCESSSTATUS = 'R'", string.Join(",", ids));
						DBServiceHelper.Execute(this.Context, text2);
						this.SetScheduleState(status, scheduleTypeIds);
					}
				}, "", 0);
				return;
			}
			this.SetScheduleState(status, scheduleTypeIds);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000054FC File Offset: 0x000036FC
		private void SetScheduleState(string status, string[] scheduleTypeIds)
		{
			BusinessDataServiceHelper.SetState(base.Context, "T_BAS_SCHEDULEINFO", "FSTATUS", status, "FSCHEDULETYPEID", scheduleTypeIds);
			this.CleanScheduleCache(scheduleTypeIds);
			this.ListView.Refresh();
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00005544 File Offset: 0x00003744
		private void TestSchedule()
		{
			string[] array = (from i in this.ListView.SelectedRowsInfo
			select Convert.ToString(i.DataRow["FSCHEDULEID_ID"])).ToArray<string>();
			if (array.Length < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先选中要操作的数据！", "004023000022653", 5, new object[0]), 0);
				return;
			}
			Schedule scheduleByTypeId = ScheduleBusinessServiceHelper.GetScheduleByTypeId(base.Context, array[0]);
			if (scheduleByTypeId != null && !string.IsNullOrEmpty(scheduleByTypeId.ScheduleId))
			{
				scheduleByTypeId.IsDebug = true;
				PaaSScheduleServiceHelper.RunSchedule(base.Context, scheduleByTypeId);
				this.CleanScheduleCache(array);
				this.ListView.Refresh();
				return;
			}
			this.View.ShowMessage(ResManager.LoadKDString("库龄定时计算对应的执行计划不存在，可能已经被删除，请删除重建该库龄定时计算！", "004023000022651", 5, new object[0]), 0);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00005614 File Offset: 0x00003814
		private void CleanScheduleCache(string[] scheduleTypeIds)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BOS_SCHEDULETYPE", true) as FormMetadata;
			BusinessDataServiceHelper.ClearCache(base.Context, formMetadata.BusinessInfo.GetDynamicObjectType(), scheduleTypeIds);
		}
	}
}
