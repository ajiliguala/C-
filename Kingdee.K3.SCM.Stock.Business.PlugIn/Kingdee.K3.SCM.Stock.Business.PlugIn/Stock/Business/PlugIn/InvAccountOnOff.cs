using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Objects.Permission.Objects;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.Excel;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000095 RID: 149
	public class InvAccountOnOff : AbstractDynamicFormPlugIn
	{
		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060007EA RID: 2026 RVA: 0x00065C7C File Offset: 0x00063E7C
		public List<StockOrgOperateResult> OperateResult
		{
			get
			{
				return this.opResults;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060007EB RID: 2027 RVA: 0x00065C84 File Offset: 0x00063E84
		public bool IsOpenAccount
		{
			get
			{
				return this.isOpenAccount;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060007EC RID: 2028 RVA: 0x00065C8C File Offset: 0x00063E8C
		public Dictionary<string, bool> IgnoreCheckInfo
		{
			get
			{
				return this.ignoreCheckInfo;
			}
		}

		// Token: 0x060007ED RID: 2029 RVA: 0x00065C94 File Offset: 0x00063E94
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.isOpenAccount = false;
			object customParameter = this.View.OpenParameter.GetCustomParameter("Direct");
			if (customParameter != null)
			{
				string text = customParameter.ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					string text2 = ResManager.LoadKDString("关账", "004023030000241", 5, new object[0]);
					string text3 = ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]);
					this.isOpenAccount = text.Equals("O", StringComparison.OrdinalIgnoreCase);
					this.View.SetFormTitle(new LocaleValue(this.isOpenAccount ? text3 : text2, base.Context.UserLocale.LCID));
					this.View.SetInnerTitle(new LocaleValue(this.isOpenAccount ? text3 : text2, base.Context.UserLocale.LCID));
					this.View.GetMainBarItem("tbAction").Text = (this.isOpenAccount ? text3 : text2);
				}
			}
			this.bShowErr = false;
			this.ShowErrGrid(this.bShowErr);
			this.View.GetControl<EntryGrid>("FEntityAction").SetFireDoubleClickEvent(true);
			this._progressbar = this.View.GetControl<ProgressBar>("FProgressBar");
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "RecBalMidData", false);
			this.bRecordMidData = false;
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				this.bRecordMidData = Convert.ToBoolean(systemProfile);
			}
			if (string.IsNullOrWhiteSpace(this.View.BusinessInfo.GetForm().ParameterObjectId))
			{
				this.View.BusinessInfo.GetForm().ParameterObjectId = "STK_AccountUserParaSetting";
			}
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x00065E50 File Offset: 0x00064050
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.View.GetControl("FCLOSEDATE").Visible = !this.isOpenAccount;
			this.View.GetControl<Panel>("FPanelDate").Visible = !this.isOpenAccount;
			this.ShowHideErrTabDetail(null, InvAccountOnOff.ErrType.None, 0);
			this._progressbar.Visible = false;
			this.View.GetMainBarItem("tbExport").Visible = !this.isOpenAccount;
			this.View.GetMainBarItem("tbExportSetting").Visible = !this.isOpenAccount;
			this.View.GetMainBarItem("tbOption").Visible = !this.isOpenAccount;
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x00065F10 File Offset: 0x00064110
		public override void CreateNewData(BizDataEventArgs e)
		{
			DynamicObjectType dynamicObjectType = this.Model.BillBusinessInfo.GetDynamicObjectType();
			Entity entity = this.View.BusinessInfo.Entrys[1];
			DynamicObjectType dynamicObjectType2 = entity.DynamicObjectType;
			DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
			dynamicObject["CloseDate"] = TimeServiceHelper.GetSystemDateTime(this.View.Context).Date;
			DynamicObjectCollection value = entity.DynamicProperty.GetValue<DynamicObjectCollection>(dynamicObject);
			BusinessObject businessObject = new BusinessObject
			{
				Id = "STK_Account",
				PermissionControl = 1,
				SubSystemId = "STK"
			};
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, this.isOpenAccount ? "4cc4dea42de6441ebeb21c509358d73d" : "1046d14017fd45dbaff9b1fe4affe0c6");
			if (permissionOrg == null || permissionOrg.Count < 1)
			{
				e.BizDataObject = dynamicObject;
				return;
			}
			Dictionary<string, object> batchStockDate = StockServiceHelper.GetBatchStockDate(base.Context, permissionOrg);
			if (batchStockDate == null || batchStockDate.Keys.Count < 1)
			{
				e.BizDataObject = dynamicObject;
				return;
			}
			permissionOrg.Clear();
			foreach (string value2 in batchStockDate.Keys)
			{
				permissionOrg.Add(Convert.ToInt64(value2));
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			list.Add(new SelectorItemInfo("FName"));
			list.Add(new SelectorItemInfo("FNumber"));
			list.Add(new SelectorItemInfo("FDescription"));
			string text = this.GetInFilter(" FORGID", permissionOrg);
			text += string.Format(" AND FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' AND (FORGFUNCTIONS like'%103%') AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP WHERE BSP.FCATEGORY = 'STK' AND BSP.FORGID = FORGID AND BSP.FACCOUNTBOOKID = 0 AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') {0} ", this.isOpenAccount ? "AND EXISTS(SELECT 1 FROM T_STK_CLOSEPROFILE SCP WHERE SCP.FCATEGORY = 'STK' AND SCP.FORGID = FORGID )" : "");
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = text,
				OrderByClauseWihtKey = "FNumber",
				IsolationOrgList = null,
				RequiresDataPermission = true
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			DataTable stockOrgAcctLastCloseDate = CommonServiceHelper.GetStockOrgAcctLastCloseDate(base.Context, "");
			Dictionary<long, DateTime> dictionary = new Dictionary<long, DateTime>();
			foreach (object obj in stockOrgAcctLastCloseDate.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				if (!(dataRow["FCLOSEDATE"] is DBNull) && !string.IsNullOrWhiteSpace(dataRow["FCLOSEDATE"].ToString()))
				{
					dictionary[Convert.ToInt64(dataRow["FORGID"])] = Convert.ToDateTime(dataRow["FCLOSEDATE"]);
				}
			}
			int num = 0;
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					long key = Convert.ToInt64(dynamicObject2["FORGID"]);
					DynamicObject dynamicObject3 = new DynamicObject(dynamicObjectType2);
					dynamicObject3["Check"] = true;
					dynamicObject3["StockOrgNo"] = dynamicObject2["FNumber"].ToString();
					dynamicObject3["StockOrgName"] = ((dynamicObject2["FName"] == null || string.IsNullOrEmpty(dynamicObject2["FName"].ToString())) ? "" : dynamicObject2["FName"].ToString());
					dynamicObject3["StockOrgDesc"] = ((dynamicObject2["FDescription"] == null || string.IsNullOrEmpty(dynamicObject2["FDescription"].ToString())) ? "" : dynamicObject2["FDescription"].ToString());
					dynamicObject3["StockOrgID"] = dynamicObject2["FORGID"].ToString();
					dynamicObject3["Result"] = "";
					dynamicObject3["RetFlag"] = false;
					dynamicObject3["Seq"] = num++;
					if (dictionary.ContainsKey(key))
					{
						dynamicObject3["LastCloseDate"] = dictionary[key];
					}
					value.Add(dynamicObject3);
				}
			}
			e.BizDataObject = dynamicObject;
		}

		// Token: 0x060007F0 RID: 2032 RVA: 0x000663E0 File Offset: 0x000645E0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbAction"))
				{
					if (!(barItemKey == "tbErrDetail"))
					{
						if (barItemKey == "tbExit")
						{
							this.View.Close();
							return;
						}
						if (!(barItemKey == "tbExport"))
						{
							return;
						}
						this.ExportErrDataInfo();
					}
					else
					{
						if (!this.bShowErr)
						{
							this.ShowErrTypeInfo();
							return;
						}
						this.ShowErrGrid(false);
						return;
					}
				}
				else
				{
					string operateName = this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						e.Cancel = true;
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						return;
					}
					this.SetParaData();
					this.DoAction();
					this.isClicked = true;
					return;
				}
			}
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x000664DC File Offset: 0x000646DC
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbIgnoreMinus":
				this.DoSingleAction("Minus");
				return;
			case "tbIgnoreStkBillAudit":
				this.DoParaAction();
				return;
			case "tbIgnoreStkBillDraft":
				this.DoSingleAction("StkBillDraft");
				return;
			case "tbViewDetailRpt":
				this.ViewDetailRpt();
				return;
			case "tbShowBill":
			case "tbShowBillStkBillDraft":
			case "tbShowBillStkBillAudit":
			{
				int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex(e.ParentKey);
				string text = "FErrObjType";
				string text2 = "FErrObjKeyID";
				if (e.ParentKey.Equals("FStkDraftBillEntry", StringComparison.OrdinalIgnoreCase))
				{
					text = "FDraftErrObjType";
					text2 = "FDraftErrObjKeyID";
				}
				else if (e.ParentKey.Equals("FStkCountBillAuditEntry", StringComparison.OrdinalIgnoreCase))
				{
					text = "FCtbaErrObjType";
					text2 = "FCtbaErrObjKeyID";
				}
				this.ShowBillInfo(Convert.ToString(this.View.Model.GetValue(text, entryCurrentRowIndex)), Convert.ToString(this.View.Model.GetValue(text2, entryCurrentRowIndex)));
				break;
			}

				return;
			}
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x00066655 File Offset: 0x00064855
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			if (!this.isClicked)
			{
				return;
			}
			if (e.Key.Equals("FEntityAction", StringComparison.OrdinalIgnoreCase))
			{
				this.ShowErrTypeInfo();
				return;
			}
			if (e.Key.Equals("FEntityErrType", StringComparison.OrdinalIgnoreCase))
			{
				this.ShowErrInfo();
			}
		}

		// Token: 0x060007F3 RID: 2035 RVA: 0x00066694 File Offset: 0x00064894
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			if (e.Key.Equals("FEntityErrInfo", StringComparison.OrdinalIgnoreCase))
			{
				this.ShowBillInfo(Convert.ToString(this.View.Model.GetValue("FErrObjType", e.Row)), Convert.ToString(this.View.Model.GetValue("FErrObjKeyID", e.Row)));
				return;
			}
			if (e.Key.Equals("FStkDraftBillEntry", StringComparison.OrdinalIgnoreCase))
			{
				this.ShowBillInfo(Convert.ToString(this.View.Model.GetValue("FDraftErrObjType", e.Row)), Convert.ToString(this.View.Model.GetValue("FDraftErrObjKeyID", e.Row)));
				return;
			}
			if (e.Key.Equals("FStkCountBillAuditEntry", StringComparison.OrdinalIgnoreCase))
			{
				this.ShowBillInfo(Convert.ToString(this.View.Model.GetValue("FCtbaErrObjType", e.Row)), Convert.ToString(this.View.Model.GetValue("FCtbaErrObjKeyID", e.Row)));
			}
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x000667B8 File Offset: 0x000649B8
		private void SetParaData()
		{
			string text = "STK_AccountUserParaSetting";
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, this.View.BusinessInfo.GetForm().Id, "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("FIgnoreParaPlugIn"))
			{
				this._IgnoreParaPlugIn = Convert.ToBoolean(dynamicObject["FIgnoreParaPlugIn"]);
			}
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("FIgnoreMinusErr"))
			{
				this._IgnoreMinusErr = Convert.ToBoolean(dynamicObject["FIgnoreMinusErr"]);
			}
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("FIgnoreDraftBill"))
			{
				this._IgnoreDraftBill = Convert.ToBoolean(dynamicObject["FIgnoreDraftBill"]);
			}
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x000668AC File Offset: 0x00064AAC
		private void ShowBillInfo(string formId, string pkId)
		{
			if (string.IsNullOrEmpty(formId) || string.IsNullOrEmpty(pkId))
			{
				return;
			}
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(this.View.Context, formId);
			BusinessObject businessObject = new BusinessObject();
			businessObject.Id = formId;
			OperationStatus status = 2;
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, businessObject, "f323992d896745fbaab4a2717c79ce2e");
			if (!permissionAuthResult.Passed)
			{
				PermissionAuthResult permissionAuthResult2 = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
				if (!permissionAuthResult2.Passed)
				{
					string text = string.Format(ResManager.LoadKDString("您没有“{0}”的“查看”权限！", "004023030009034", 5, new object[0]), formMetaData.BusinessInfo.GetForm().Name);
					this.View.ShowMessage(text, 0);
					return;
				}
				status = 1;
			}
			FilterObjectByDataRuleParamenter filterObjectByDataRuleParamenter = new FilterObjectByDataRuleParamenter(formMetaData.BusinessInfo, new List<string>
			{
				pkId
			});
			List<string> list = PermissionServiceHelper.FilterObjectByDataRule(this.View.Context, filterObjectByDataRuleParamenter);
			if (!list.Contains(pkId))
			{
				string text2 = string.Format(ResManager.LoadKDString("您没有“{0}”的“查看”权限！", "004023030009034", 5, new object[0]), formMetaData.BusinessInfo.GetForm().Name);
				this.View.ShowMessage(text2, 0);
				return;
			}
			BillShowParameter billShowParameter = new BillShowParameter
			{
				FormId = formId,
				ParentPageId = this.View.PageId,
				Status = status,
				PKey = pkId
			};
			billShowParameter.OpenStyle.ShowType = 7;
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x00066A38 File Offset: 0x00064C38
		private void ExportErrDataInfo()
		{
			if (this.opResults == null || this.opResults.Count <= 0)
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("当前无异常校验信息", "004023030009033", 5, new object[0]), "", 0, null, 1);
				return;
			}
			DataSet dataSet = new DataSet();
			DataTable dataTable = new DataTable(ResManager.LoadKDString("异常数据", "004023030009037", 5, new object[0]));
			this.SetErrTableColumnsInfo(dataTable);
			DataTable dataTable2 = new DataTable(ResManager.LoadKDString("负库存数据", "004023030009038", 5, new object[0]));
			this.SetMinusTableColumnsInfo(dataTable2);
			IOrderedEnumerable<StockOrgOperateResult> orderedEnumerable = from p in this.opResults
			orderby p.StockOrgNumber
			select p;
			foreach (StockOrgOperateResult stockOrgOperateResult in orderedEnumerable)
			{
				if (stockOrgOperateResult.ErrInfo != null && stockOrgOperateResult.ErrInfo.Count > 0)
				{
					foreach (OperateErrorInfo errInfo in stockOrgOperateResult.ErrInfo)
					{
						dataTable.Rows.Add(this.GetNewRowData(stockOrgOperateResult, dataTable, errInfo));
					}
				}
				if (stockOrgOperateResult.MinusErrObject != null)
				{
					DynamicObjectCollection dynamicObjectCollection = stockOrgOperateResult.MinusErrObject["Entry"] as DynamicObjectCollection;
					if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
					{
						foreach (DynamicObject errInfo2 in dynamicObjectCollection)
						{
							dataTable2.Rows.Add(this.GetMinusNewRowData(stockOrgOperateResult, dataTable2, errInfo2));
						}
					}
				}
				if (stockOrgOperateResult.StkBillDraftErrInfo != null && stockOrgOperateResult.StkBillDraftErrInfo.Count > 0)
				{
					foreach (OperateErrorInfo errInfo3 in stockOrgOperateResult.StkBillDraftErrInfo)
					{
						dataTable.Rows.Add(this.GetNewRowData(stockOrgOperateResult, dataTable, errInfo3));
					}
				}
				if (stockOrgOperateResult.StkCountBillAuditErrInfo != null && stockOrgOperateResult.StkCountBillAuditErrInfo.Count > 0)
				{
					foreach (OperateErrorInfo errInfo4 in stockOrgOperateResult.StkCountBillAuditErrInfo)
					{
						dataTable.Rows.Add(this.GetNewRowData(stockOrgOperateResult, dataTable, errInfo4));
					}
				}
			}
			if (dataTable.Rows.Count > 0)
			{
				dataSet.Tables.Add(dataTable);
			}
			if (dataTable2.Rows.Count > 0)
			{
				dataSet.Tables.Add(dataTable2);
			}
			if (dataSet.Tables == null || dataSet.Tables.Count <= 0)
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("当前无异常校验信息", "004023030009033", 5, new object[0]), "", 0, null, 1);
				return;
			}
			string text = string.Format("{0}_{1}", this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0]), DateTime.Now.ToString("yyyyMMddHHmmssff"));
			text = this.RemoveIllegalChar(text);
			text = PathUtils.GetValidFileName(text);
			string text2 = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, text);
			string text3 = PathUtils.GetServerPath(KeyConst.TEMPFILEPATH, PathUtils.UrlEncode(text));
			if (this.GetSaveType(ViewUtils.GetFormId(this.View)) == null)
			{
				text2 += ".xlsx";
				text3 += ".xlsx";
			}
			else
			{
				text2 += ".xls";
				text3 += ".xls";
			}
			using (ExcelOperation excelOperation = new ExcelOperation(this.View))
			{
				excelOperation.BeginExport();
				excelOperation.DateSetToExcel(dataSet, true);
				excelOperation.EndExport(text2, 0);
				this.DownLoadFile(text3);
			}
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x00066EF4 File Offset: 0x000650F4
		private void SetErrTableColumnsInfo(DataTable dt)
		{
			List<Field> fieldList = this.View.BusinessInfo.GetFieldList();
			using (List<string>.Enumerator enumerator = this._exportCommonColumns.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string column = enumerator.Current;
					Field field = (from p in fieldList
					where p.Key.Equals(column)
					select p).FirstOrDefault<Field>();
					dt.Columns.Add(field.Name.ToString());
				}
			}
		}

		// Token: 0x060007F8 RID: 2040 RVA: 0x00066FF8 File Offset: 0x000651F8
		private void SetMinusTableColumnsInfo(DataTable dt)
		{
			List<Field> fieldList = this.View.BusinessInfo.GetFieldList();
			Field field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[0])
			select p).FirstOrDefault<Field>();
			dt.Columns.Add(field.Name.ToString());
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[1])
			select p).FirstOrDefault<Field>();
			dt.Columns.Add(field.Name.ToString());
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[2])
			select p).FirstOrDefault<Field>();
			dt.Columns.Add(field.Name.ToString());
			IOrderedEnumerable<Field> orderedEnumerable = from p in this.View.BusinessInfo.GetEntryEntity("FMinusEntry").Fields
			where p.FunControlExt != null
			orderby p.Tabindex
			select p;
			foreach (Field field2 in orderedEnumerable)
			{
				if (!field2.Key.Equals("FProjectNo"))
				{
					dt.Columns.Add(field2.Name.ToString());
				}
			}
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x000671C8 File Offset: 0x000653C8
		private DataRow GetNewRowData(StockOrgOperateResult opResult, DataTable dt, OperateErrorInfo errInfo)
		{
			DataRow dataRow = dt.NewRow();
			List<Field> fieldList = this.View.BusinessInfo.GetFieldList();
			Field field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[0])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = opResult.StockOrgNumber;
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[1])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = opResult.StockOrgName;
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[2])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = opResult.StockOrgDesc;
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[3])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = errInfo.ErrMsg;
			return dataRow;
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x000672F8 File Offset: 0x000654F8
		private DataRow GetMinusNewRowData(StockOrgOperateResult opResult, DataTable dt, DynamicObject errInfo)
		{
			DataRow dataRow = dt.NewRow();
			List<Field> fieldList = this.View.BusinessInfo.GetFieldList();
			Field field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[0])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = opResult.StockOrgNumber;
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[1])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = opResult.StockOrgName;
			field = (from p in fieldList
			where p.Key.Equals(this._exportCommonColumns[2])
			select p).FirstOrDefault<Field>();
			dataRow[field.Name.ToString()] = opResult.StockOrgDesc;
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FMinusEntry");
			foreach (Field field2 in entryEntity.Fields)
			{
				if (!field2.Key.Equals("FProjectNo"))
				{
					dataRow[field2.Name.ToString()] = errInfo[field2.PropertyName];
				}
			}
			return dataRow;
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x00067434 File Offset: 0x00065634
		private string RemoveIllegalChar(string fileName)
		{
			string[] array = new string[]
			{
				"/",
				"\\",
				"+"
			};
			foreach (string oldValue in array)
			{
				fileName = fileName.Replace(oldValue, "");
			}
			return fileName;
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x0006748C File Offset: 0x0006568C
		private void DownLoadFile(string url)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "BOS_FileDownLoad";
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.CustomParams.Add("IsExportData", "true");
			dynamicFormShowParameter.CustomParams.Add("url", url);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x000674E8 File Offset: 0x000656E8
		protected SaveFileType GetSaveType(string formId)
		{
			string text = UserParamterServiceHelper.Load(this.View.Context, "ExportSetting" + formId.ToUpper().GetHashCode().ToString(), this.View.Context.UserId, "");
			if (!string.IsNullOrWhiteSpace(text) && text != "<Root />")
			{
				JSONArray jsonArray = new JSONArray(text);
				string valuebyKey = this.GetValuebyKey(jsonArray, "filetype");
				if (!ObjectUtils.IsNullOrEmpty(valuebyKey))
				{
					SaveFileType result = 0;
					if (Enum.TryParse<SaveFileType>(valuebyKey, out result))
					{
						return result;
					}
				}
			}
			return 0;
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x0006757C File Offset: 0x0006577C
		private string GetValuebyKey(JSONArray jsonArray, string key)
		{
			string result = string.Empty;
			if (jsonArray == null)
			{
				return result;
			}
			for (int i = 0; i < jsonArray.Count; i++)
			{
				Dictionary<string, object> dictionary = jsonArray[i] as Dictionary<string, object>;
				if (dictionary["key"] != null && dictionary["key"].ToString() == key)
				{
					result = ObjectUtils.Object2String(dictionary["value"]);
					break;
				}
			}
			return result;
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x00067638 File Offset: 0x00065838
		private void DoParaAction()
		{
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["EntityAction"] as DynamicObjectCollection;
			int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FEntityAction");
			long orgId = Convert.ToInt64(dynamicObjectCollection[entryCurrentRowIndex]["StockOrgID"]);
			StockOrgOperateResult stockOrgOperateResult = (from p in this.opResults
			where p.StockOrgID == orgId
			select p).FirstOrDefault<StockOrgOperateResult>();
			List<int> list = (from p in stockOrgOperateResult.StkCountBillAuditErrInfo
			select p.ErrType).Distinct<int>().ToList<int>();
			if (list != null && list.Count<int>() > 1)
			{
				Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntityErrType");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				int entryCurrentRowIndex2 = this.View.Model.GetEntryCurrentRowIndex("FEntityErrType");
				DynamicObject curErrTypeData = entityDataObject[entryCurrentRowIndex2];
				if (curErrTypeData != null)
				{
					entityDataObject.Remove(curErrTypeData);
					List<OperateErrorInfo> stkCountBillAuditErrInfo = (from p in stockOrgOperateResult.StkCountBillAuditErrInfo
					where p.ErrType != Convert.ToInt32(curErrTypeData["ErrorType"])
					select p).ToList<OperateErrorInfo>();
					stockOrgOperateResult.StkCountBillAuditErrInfo = stkCountBillAuditErrInfo;
					this.View.UpdateView("FEntityErrType");
					this.View.SetEntityFocusRow("FEntityErrType", 0);
					return;
				}
			}
			else
			{
				this.DoSingleAction("CntBillAudit");
			}
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x000677D8 File Offset: 0x000659D8
		private void ViewDetailRpt()
		{
			object value = this.Model.GetValue("FCLOSEDATE");
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntityAction");
			long num = Convert.ToInt64(this.Model.GetValue("FStockOrgID", entryCurrentRowIndex));
			if (this.orgViewRptList == null)
			{
				BusinessObject businessObject = new BusinessObject
				{
					Id = "STK_StockDetailRpt",
					PermissionControl = 1,
					SubSystemId = "STK"
				};
				this.orgViewRptList = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
			}
			if (!this.orgViewRptList.Contains(num))
			{
				this.View.ShowMessage(ResManager.LoadKDString("您在该库存组织下没有物料收发明细报表的查看权限!", "004023030009357", 5, new object[0]), 0);
				return;
			}
			MoveReportShowParameter moveReportShowParameter = new MoveReportShowParameter();
			moveReportShowParameter.ParentPageId = this.View.PageId;
			moveReportShowParameter.MultiSelect = false;
			moveReportShowParameter.FormId = "STK_StockDetailRpt";
			moveReportShowParameter.Height = 700;
			moveReportShowParameter.Width = 950;
			moveReportShowParameter.IsShowFilter = false;
			moveReportShowParameter.CustomParams.Add("SourceBillFormId", "STK_Account");
			moveReportShowParameter.CustomParams.Add("SourceBillId", this.dTransId[num]);
			moveReportShowParameter.CustomParams.Add("SourceOrgIds", num.ToString());
			moveReportShowParameter.CustomParams.Add("SourceEndDate", value.ToString());
			this.View.ShowForm(moveReportShowParameter);
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x00067954 File Offset: 0x00065B54
		private void ClearEntity(string entityKey)
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity(entityKey);
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			this.View.UpdateView(entityKey);
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x0006799C File Offset: 0x00065B9C
		private void DoAction()
		{
			List<long> list = new List<long>();
			List<string> list2 = new List<string>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["EntityAction"] as DynamicObjectCollection;
			for (int i = 0; i < this.Model.GetEntryRowCount("FEntityAction"); i++)
			{
				if (Convert.ToBoolean(dynamicObjectCollection[i]["Check"]))
				{
					list.Add(Convert.ToInt64(dynamicObjectCollection[i]["StockOrgID"]));
					list2.Add(dynamicObjectCollection[i]["StockOrgNo"].ToString());
					this.Model.SetValue("FResult", "", i);
				}
			}
			this.StartDoOrgClose(list, list2, true);
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x00067A60 File Offset: 0x00065C60
		private void DoSingleAction(string singleActionType)
		{
			List<long> list = new List<long>();
			List<string> list2 = new List<string>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["EntityAction"] as DynamicObjectCollection;
			int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FEntityAction");
			long num = Convert.ToInt64(dynamicObjectCollection[entryCurrentRowIndex]["StockOrgID"]);
			list.Add(num);
			list2.Add(dynamicObjectCollection[entryCurrentRowIndex]["StockOrgNo"].ToString());
			this.Model.SetValue("FResult", "", entryCurrentRowIndex);
			this.ignoreCheckInfo[singleActionType + num] = true;
			this.StartDoOrgClose(list, list2, true);
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00067CF4 File Offset: 0x00065EF4
		private void StartDoOrgClose(List<long> orgIds, List<string> orgNums, bool isReDoAction)
		{
			if (this.isbBusiness)
			{
				this.View.ShowMessage(ResManager.LoadKDString("上次提交未执行完毕，请稍后再试", "004023030002134", 5, new object[0]), 0);
				return;
			}
			if (orgIds.Count < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先选择未成功处理过的库存组织", "004023030000247", 5, new object[0]), 0);
				return;
			}
			DateTime dateTime = DateTime.MinValue;
			object value = this.Model.GetValue("FCLOSEDATE");
			if (value != null)
			{
				dateTime = DateTime.Parse(value.ToString());
			}
			if (dateTime == DateTime.MinValue && !this.isOpenAccount)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先录入关账日期", "004023030000244", 5, new object[0]), 0);
				return;
			}
			if (dateTime >= DateTime.MaxValue.Date)
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("关账日期不能等于{0}，请修改关账日期。", "004023030009373", 5, new object[0]), DateTime.MaxValue.Date.ToShortDateString()), 0);
				return;
			}
			this.isbBusiness = true;
			string text = this.isOpenAccount ? ResManager.LoadKDString("正在处理库存组织反关账", "004023030006434", 5, new object[0]) : ResManager.LoadKDString("正在处理库存组织关账", "004023030006435", 5, new object[0]);
			ViewUtils.ShowProcessForm(this.View, delegate(FormResult r1)
			{
			}, text);
			this._progressbar.Start(1);
			this._progressbar.Visible = false;
			this._progressbar.SetValue(0);
			List<StockOrgOperateResult> ret = null;
			MainWorker.QuequeTask(base.Context, delegate()
			{
				ret = this.DoOrgClose(orgIds, orgNums, isReDoAction);
			}, delegate(AsynResult result)
			{
				CultureInfoUtils.SetCurrentLanguage(this.Context);
				if (!result.Success)
				{
					string message;
					if (result.Exception.InnerException != null)
					{
						if (result.Exception.InnerException.InnerException != null)
						{
							message = result.Exception.InnerException.InnerException.Message;
						}
						else
						{
							message = result.Exception.InnerException.Message;
						}
					}
					else
					{
						message = result.Exception.Message;
					}
					this.View.ShowErrMessage(message, string.Format(ResManager.LoadKDString("执行{0}失败", "004023030002137", 5, new object[0]), this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0])), 0);
				}
				else
				{
					this.SetTransId(ret);
					this.MergeOperateResult(ret);
					this.RefreshOrgSuccessFlag();
					this.ShowErrTypeInfo();
				}
				if (this._netResults != null)
				{
					NetworkCtrlServiceHelper.BatchCommitNetCtrl(this.Context, this._netResults);
					this._netResults = null;
				}
				this._progressbar.SetValue(100);
				this.View.Session["ProcessRateValue"] = 100;
				this.isbBusiness = false;
			});
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x00067EF0 File Offset: 0x000660F0
		private List<StockOrgOperateResult> DoOrgClose(List<long> orgIds, List<string> orgNums, bool isReDoAction)
		{
			DateTime dateTime = DateTime.MinValue;
			object value = this.Model.GetValue("FCLOSEDATE");
			if (value != null)
			{
				dateTime = DateTime.Parse(value.ToString());
			}
			List<StockOrgOperateResult> result = null;
			this._netResults = this.BatchStartNetCtl(orgNums);
			if (this._netResults != null && this._netResults.Count == orgNums.Count)
			{
				try
				{
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					if (isReDoAction)
					{
						this.ignoreCheckInfo.TryGetValue("Minus" + orgIds[0], out flag);
						this.ignoreCheckInfo.TryGetValue("CntBillAudit" + orgIds[0], out flag2);
						this.ignoreCheckInfo.TryGetValue("StkBillDraft" + orgIds[0], out flag3);
					}
					if (this._IgnoreParaPlugIn)
					{
						flag2 = true;
					}
					if (this._IgnoreMinusErr)
					{
						flag = true;
					}
					if (this._IgnoreDraftBill)
					{
						flag3 = true;
					}
					result = StockServiceHelper.InvAccountOnOff(base.Context, orgIds, dateTime, this.isOpenAccount, !flag, !flag2, !flag3, this.bRecordMidData);
				}
				catch (Exception ex)
				{
					this.View.ShowErrMessage(ex.Message, string.Format(ResManager.LoadKDString("执行{0}失败", "004023030002137", 5, new object[0]), this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0])), 0);
				}
			}
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, this._netResults);
			this._netResults = null;
			this.isbBusiness = false;
			return result;
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x000680B0 File Offset: 0x000662B0
		private void MergeOperateResult(List<StockOrgOperateResult> ret)
		{
			if (ret == null || ret.Count < 1)
			{
				return;
			}
			foreach (StockOrgOperateResult stockOrgOperateResult in ret)
			{
				bool flag = false;
				if (stockOrgOperateResult.OperateSuccess)
				{
					if (this.ignoreCheckInfo.ContainsKey("Minus" + stockOrgOperateResult.StockOrgID))
					{
						this.ignoreCheckInfo.Remove("Minus" + stockOrgOperateResult.StockOrgID);
					}
					if (this.ignoreCheckInfo.ContainsKey("CntBillAudit" + stockOrgOperateResult.StockOrgID))
					{
						this.ignoreCheckInfo.Remove("CntBillAudit" + stockOrgOperateResult.StockOrgID);
					}
					if (this.ignoreCheckInfo.ContainsKey("StkBillDraft" + stockOrgOperateResult.StockOrgID))
					{
						this.ignoreCheckInfo.Remove("StkBillDraft" + stockOrgOperateResult.StockOrgID);
					}
				}
				for (int i = 0; i < this.opResults.Count; i++)
				{
					if (this.opResults[i].StockOrgID == stockOrgOperateResult.StockOrgID)
					{
						this.opResults[i] = stockOrgOperateResult;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.opResults.Add(stockOrgOperateResult);
				}
			}
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x000682E0 File Offset: 0x000664E0
		private void ShowErrTypeInfo()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntityErrType");
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			if (this.opResults == null || this.opResults.Count < 1)
			{
				this.ClearEntity("FEntityErrInfo");
				this.ShowHideErrTabDetail(null, InvAccountOnOff.ErrType.None, 0);
				this.View.UpdateView("FEntityErrType");
				this.ShowErrGrid(true);
				return;
			}
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntityAction");
			long curStockOrgId = Convert.ToInt64(this.Model.GetValue("FStockOrgID", entryCurrentRowIndex));
			StockOrgOperateResult stockOrgOperateResult = this.opResults.SingleOrDefault((StockOrgOperateResult p) => p.StockOrgID == curStockOrgId);
			if (stockOrgOperateResult == null || stockOrgOperateResult.OperateSuccess)
			{
				this.ClearEntity("FEntityErrInfo");
				this.ShowHideErrTabDetail(null, InvAccountOnOff.ErrType.None, 0);
				this.View.UpdateView("FEntityErrType");
				this.ShowErrGrid(true);
				return;
			}
			if (stockOrgOperateResult.ErrInfo != null)
			{
				if (stockOrgOperateResult.ErrInfo.Exists((OperateErrorInfo p) => p.ErrType < Convert.ToInt32(InvAccountOnOff.ErrType.UnAuditBill)))
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["ErrorType"] = InvAccountOnOff.ErrType.OrgStatusErr;
					dynamicObject["ErrTypeName"] = string.Format(ResManager.LoadKDString("当前组织状态不符合{0}操作条件", "004023030002140", 5, new object[0]), this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0]));
					entityDataObject.Add(dynamicObject);
				}
			}
			if (stockOrgOperateResult.ErrInfo != null)
			{
				if (stockOrgOperateResult.ErrInfo.Exists((OperateErrorInfo p) => p.ErrType == Convert.ToInt32(InvAccountOnOff.ErrType.UnAuditBill)))
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["ErrorType"] = InvAccountOnOff.ErrType.UnAuditBill;
					dynamicObject["ErrTypeName"] = string.Format(ResManager.LoadKDString("当前组织存在未审核的库存单据", "004023030002143", 5, new object[0]), new object[0]);
					entityDataObject.Add(dynamicObject);
				}
			}
			if (stockOrgOperateResult.StkBillDraftErrInfo != null && stockOrgOperateResult.StkBillDraftErrInfo.Count > 0)
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["ErrorType"] = InvAccountOnOff.ErrType.StkDraftBill;
				dynamicObject["ErrTypeName"] = ResManager.LoadKDString("当前组织存在暂存的库存单据", "004023000022222", 5, new object[0]);
				entityDataObject.Add(dynamicObject);
			}
			if (stockOrgOperateResult.MinusErrObject != null)
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["ErrorType"] = InvAccountOnOff.ErrType.Minus;
				dynamicObject["ErrTypeName"] = string.Format(ResManager.LoadKDString("当前组织存在异常库存数据", "004023030002146", 5, new object[0]), new object[0]);
				entityDataObject.Add(dynamicObject);
			}
			Dictionary<int, string> invAccountParaData = this.GetInvAccountParaData(base.Context);
			if (stockOrgOperateResult.ErrInfo != null)
			{
				if (stockOrgOperateResult.ErrInfo.Exists((OperateErrorInfo p) => p.ErrType > Convert.ToInt32(InvAccountOnOff.ErrType.UnAuditBill)))
				{
					goto IL_354;
				}
			}
			if (stockOrgOperateResult.StkCountBillAuditErrInfo == null || stockOrgOperateResult.StkCountBillAuditErrInfo.Count <= 0)
			{
				goto IL_48B;
			}
			IL_354:
			List<int> list = null;
			if (stockOrgOperateResult.ErrInfo != null)
			{
				if (stockOrgOperateResult.ErrInfo.Exists((OperateErrorInfo p) => p.ErrType > Convert.ToInt32(InvAccountOnOff.ErrType.UnAuditBill)))
				{
					list = (from p in stockOrgOperateResult.ErrInfo
					where p.ErrType > Convert.ToInt32(InvAccountOnOff.ErrType.UnAuditBill)
					select p.ErrType).Distinct<int>().ToList<int>();
				}
			}
			if (stockOrgOperateResult.StkCountBillAuditErrInfo != null && stockOrgOperateResult.StkCountBillAuditErrInfo.Count > 0)
			{
				if (list != null && list.Count<int>() > 0)
				{
					list.AddRange((from p in stockOrgOperateResult.StkCountBillAuditErrInfo
					select p.ErrType).Distinct<int>().ToList<int>());
				}
				else
				{
					list = (from p in stockOrgOperateResult.StkCountBillAuditErrInfo
					select p.ErrType).Distinct<int>().ToList<int>();
				}
			}
			this.SetAccountParaDataErrType(entityDataObject, dynamicObjectType, list, invAccountParaData);
			IL_48B:
			this.View.UpdateView("FEntityErrType");
			this.View.SetEntityFocusRow("FEntityErrType", 0);
			this.ShowErrInfo();
		}

		// Token: 0x06000808 RID: 2056 RVA: 0x000687A0 File Offset: 0x000669A0
		private Dictionary<int, string> GetInvAccountParaData(Context ctx)
		{
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FID"));
			list.Add(new SelectorItemInfo("FVerifyDescription"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_AccountParaPlugIns",
				SelectItems = list,
				FilterClauseWihtKey = " FIsEnable = '1' ",
				OrderByClauseWihtKey = " FSeq ASC "
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (!dictionary.Keys.Contains(Convert.ToInt32(dynamicObject["FId"])))
					{
						dictionary.Add(Convert.ToInt32(dynamicObject["FId"]), Convert.ToString(dynamicObject["FVerifyDescription"]));
					}
				}
			}
			return dictionary;
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x000688AC File Offset: 0x00066AAC
		private void SetAccountParaDataErrType(DynamicObjectCollection errEntity, DynamicObjectType errTypeObjType, List<int> atParaPlugInsErrType, Dictionary<int, string> accountParaPlugIns)
		{
			if (atParaPlugInsErrType != null && atParaPlugInsErrType.Count<int>() > 0)
			{
				foreach (int num in accountParaPlugIns.Keys)
				{
					if (atParaPlugInsErrType.Contains(num))
					{
						DynamicObject dynamicObject = new DynamicObject(errTypeObjType);
						dynamicObject["ErrorType"] = num;
						dynamicObject["ErrTypeName"] = accountParaPlugIns[num];
						errEntity.Add(dynamicObject);
					}
				}
			}
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x0006895C File Offset: 0x00066B5C
		private void ShowErrInfo()
		{
			if (this.opResults == null || this.opResults.Count < 1)
			{
				this.ShowErrGrid(true);
				return;
			}
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntityAction");
			long curStockOrgId = Convert.ToInt64(this.Model.GetValue("FStockOrgID", entryCurrentRowIndex));
			StockOrgOperateResult opResult = this.opResults.SingleOrDefault((StockOrgOperateResult p) => p.StockOrgID == curStockOrgId);
			entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntityErrType");
			InvAccountOnOff.ErrType errType = InvAccountOnOff.ErrType.None;
			object value = this.Model.GetValue("FErrorType", entryCurrentRowIndex);
			if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
			{
				Enum.TryParse<InvAccountOnOff.ErrType>(value.ToString(), out errType);
			}
			this.RefreshErrEntity(opResult, errType, Convert.ToInt32(value));
			this.RefreshStkBillDraftEntry(opResult, errType);
			this.RefreshStkCntBillAuditEntry(opResult, errType, Convert.ToInt32(value));
			this.RefreshMinusEntry(opResult, errType);
			this.ShowHideErrTabDetail(opResult, errType, Convert.ToInt32(value));
			this.ShowErrGrid(true);
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x00068A98 File Offset: 0x00066C98
		private void RefreshOrgSuccessFlag()
		{
			InvAccountOnOff.<>c__DisplayClass41 CS$<>8__locals1 = new InvAccountOnOff.<>c__DisplayClass41();
			CS$<>8__locals1.entryDataObject = (this.View.Model.DataObject["EntityAction"] as DynamicObjectCollection);
			DateTime dateTime = DateTime.MinValue;
			object value = this.Model.GetValue("FCLOSEDATE");
			if (value != null)
			{
				dateTime = DateTime.Parse(value.ToString());
			}
			DataTable dataTable = null;
			if (this.isOpenAccount)
			{
				dataTable = CommonServiceHelper.GetStockOrgAcctLastCloseDate(base.Context, "");
			}
			int i;
			for (i = 0; i < CS$<>8__locals1.entryDataObject.Count; i++)
			{
				if (Convert.ToBoolean(CS$<>8__locals1.entryDataObject[i]["Check"]))
				{
					StockOrgOperateResult stockOrgOperateResult = this.opResults.SingleOrDefault((StockOrgOperateResult p) => p.StockOrgID == Convert.ToInt64(CS$<>8__locals1.entryDataObject[i]["StockOrgID"]));
					if (stockOrgOperateResult != null)
					{
						this.Model.SetValue("FResult", stockOrgOperateResult.OperateSuccess ? ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]) : ResManager.LoadKDString("失败", "004023030000253", 5, new object[0]), i);
						this.Model.SetValue("FRetFlag", stockOrgOperateResult.OperateSuccess, i);
						if (stockOrgOperateResult.OperateSuccess)
						{
							if (this.isOpenAccount)
							{
								DataRow[] array = dataTable.Select(string.Format("FORGID={0}", stockOrgOperateResult.StockOrgID));
								if (array.Count<DataRow>() > 0)
								{
									this.Model.SetValue("FLastCloseDate", array[0]["FCLOSEDATE"], i);
								}
								else
								{
									this.Model.SetValue("FLastCloseDate", null, i);
									this.Model.SetValue("FCheck", false, i);
								}
							}
							else
							{
								this.Model.SetValue("FLastCloseDate", dateTime, i);
							}
						}
						this.Model.WriteLog(new LogObject
						{
							ObjectTypeId = this.View.BusinessInfo.GetForm().Id,
							Description = string.Format(ResManager.LoadKDString("库存组织{0}{1}{2}{3}", "004023030000256", 5, new object[0]), new object[]
							{
								stockOrgOperateResult.StockOrgNumber,
								stockOrgOperateResult.StockOrgName,
								this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0]),
								stockOrgOperateResult.OperateSuccess ? ResManager.LoadKDString("成功", "004023030000250", 5, new object[0]) : ResManager.LoadKDString("失败", "004023030000253", 5, new object[0])
							}),
							Environment = 3,
							OperateName = (this.isOpenAccount ? ResManager.LoadKDString("反关账", "004023030000238", 5, new object[0]) : ResManager.LoadKDString("关账", "004023030000241", 5, new object[0])),
							SubSystemId = "21"
						});
					}
				}
			}
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00068E48 File Offset: 0x00067048
		private void RefreshErrEntity(StockOrgOperateResult opResult, InvAccountOnOff.ErrType errType, int objType = 0)
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntityErrInfo");
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			if (errType == InvAccountOnOff.ErrType.Minus || errType == InvAccountOnOff.ErrType.None || errType == InvAccountOnOff.ErrType.UnAuditStkCountBill || errType == InvAccountOnOff.ErrType.StkDraftBill)
			{
				this.View.UpdateView("FEntityErrInfo");
				return;
			}
			if (opResult == null || opResult.ErrInfo == null || opResult.ErrInfo.Count < 1)
			{
				this.View.UpdateView("FEntityErrInfo");
				return;
			}
			IEnumerable<OperateErrorInfo> enumerable;
			if (errType == InvAccountOnOff.ErrType.UnAuditBill)
			{
				enumerable = from p in opResult.ErrInfo
				where p.ErrType == Convert.ToInt32(InvAccountOnOff.ErrType.UnAuditBill)
				select p;
			}
			else if (errType > InvAccountOnOff.ErrType.UnAuditBill)
			{
				enumerable = from p in opResult.ErrInfo
				where p.ErrType == objType
				select p;
			}
			else
			{
				enumerable = from p in opResult.ErrInfo
				where p.ErrType < Convert.ToInt32(InvAccountOnOff.ErrType.OrgStatusErr)
				select p;
			}
			int num = 1;
			foreach (OperateErrorInfo operateErrorInfo in enumerable)
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["Seq"] = num;
				dynamicObject["ErrType"] = operateErrorInfo.ErrType;
				dynamicObject["ErrObjType"] = operateErrorInfo.ErrObjType;
				dynamicObject["ErrObjKeyField"] = operateErrorInfo.ErrObjKeyField;
				dynamicObject["ErrObjKeyID"] = operateErrorInfo.ErrObjKeyID;
				dynamicObject["ErrMsg"] = operateErrorInfo.ErrMsg;
				entityDataObject.Add(dynamicObject);
				num++;
			}
			this.View.UpdateView("FEntityErrInfo");
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x00069068 File Offset: 0x00067268
		private void RefreshStkCntBillAuditEntry(StockOrgOperateResult opResult, InvAccountOnOff.ErrType errtype, int objType = 0)
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FStkCountBillAuditEntry");
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			if (opResult != null && opResult.StkCountBillAuditErrInfo != null && opResult.StkCountBillAuditErrInfo.Count > 0)
			{
				int num = 1;
				IEnumerable<OperateErrorInfo> enumerable = from p in opResult.StkCountBillAuditErrInfo
				where p.ErrType == objType
				select p;
				foreach (OperateErrorInfo operateErrorInfo in enumerable)
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["Seq"] = num;
					dynamicObject["CtbaErrType"] = operateErrorInfo.ErrType;
					dynamicObject["CtbaErrObjType"] = operateErrorInfo.ErrObjType;
					dynamicObject["CtbaErrObjKeyField"] = operateErrorInfo.ErrObjKeyField;
					dynamicObject["CtbaErrObjKeyID"] = operateErrorInfo.ErrObjKeyID;
					dynamicObject["CtbaErrMsg"] = operateErrorInfo.ErrMsg;
					entityDataObject.Add(dynamicObject);
					num++;
				}
			}
			this.View.UpdateView("FStkCountBillAuditEntry");
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x000691D8 File Offset: 0x000673D8
		private void RefreshStkBillDraftEntry(StockOrgOperateResult opResult, InvAccountOnOff.ErrType errtype)
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FStkDraftBillEntry");
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			if (opResult != null && opResult.StkBillDraftErrInfo != null && opResult.StkBillDraftErrInfo.Count > 0)
			{
				int num = 1;
				foreach (OperateErrorInfo operateErrorInfo in opResult.StkBillDraftErrInfo)
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["Seq"] = num;
					dynamicObject["DraftErrType"] = operateErrorInfo.ErrType;
					dynamicObject["DraftErrObjType"] = operateErrorInfo.ErrObjType;
					dynamicObject["DraftErrObjKeyField"] = operateErrorInfo.ErrObjKeyField;
					dynamicObject["DraftErrObjKeyID"] = operateErrorInfo.ErrObjKeyID;
					dynamicObject["DraftErrMsg"] = operateErrorInfo.ErrMsg;
					entityDataObject.Add(dynamicObject);
					num++;
				}
			}
			this.View.UpdateView("FStkDraftBillEntry");
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x0006931C File Offset: 0x0006751C
		private void RefreshMinusEntry(StockOrgOperateResult opResult, InvAccountOnOff.ErrType errType)
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FMinusEntry");
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			entityDataObject.Clear();
			if (opResult != null && opResult.MinusErrObject != null && ((DynamicObjectCollection)opResult.MinusErrObject["Entry"]).Count > 0)
			{
				foreach (DynamicObject dynamicObject in ((DynamicObjectCollection)opResult.MinusErrObject["Entry"]))
				{
					DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectType);
					dynamicObject2["ErrMessage"] = dynamicObject["ErrMessage"];
					dynamicObject2["MaterialNumber"] = dynamicObject["MaterialNumber"];
					dynamicObject2["MaterialName"] = dynamicObject["MaterialName"];
					dynamicObject2["Specification"] = dynamicObject["Specification"];
					dynamicObject2["StockName"] = dynamicObject["StockName"];
					dynamicObject2["StockLocName"] = dynamicObject["StockLocName"];
					dynamicObject2["UnitName"] = dynamicObject["UnitName"];
					dynamicObject2["Qty"] = dynamicObject["Qty"];
					dynamicObject2["SecUnitName"] = dynamicObject["SecUnitName"];
					dynamicObject2["SecQty"] = dynamicObject["SecQty"];
					dynamicObject2["LotText"] = dynamicObject["LotText"];
					dynamicObject2["AuxPropName"] = dynamicObject["AuxPropName"];
					dynamicObject2["BOMNumber"] = dynamicObject["BOMNumber"];
					dynamicObject2["MtoNo"] = dynamicObject["MtoNo"];
					dynamicObject2["ProjectNo"] = dynamicObject["ProjectNo"];
					dynamicObject2["ProduceDate"] = dynamicObject["ProduceDate"];
					dynamicObject2["ExpiryDate"] = dynamicObject["ExpiryDate"];
					dynamicObject2["StockStatusName"] = dynamicObject["StockStatusName"];
					dynamicObject2["OwnerTypeName"] = dynamicObject["OwnerTypeName"];
					dynamicObject2["OwnerName"] = dynamicObject["OwnerName"];
					dynamicObject2["KeeperTypeName"] = dynamicObject["KeeperTypeName"];
					dynamicObject2["KeeperName"] = dynamicObject["KeeperName"];
					entityDataObject.Add(dynamicObject2);
				}
			}
			this.View.UpdateView("FMinusEntry");
		}

		// Token: 0x06000810 RID: 2064 RVA: 0x00069604 File Offset: 0x00067804
		private void ShowErrGrid(bool isVisible)
		{
			this.View.GetControl<SplitContainer>("FSplitContainer").HideSecondPanel(!isVisible);
			this.bShowErr = isVisible;
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x000696CC File Offset: 0x000678CC
		private void ShowHideErrTabDetail(StockOrgOperateResult opResult, InvAccountOnOff.ErrType errType, int objType = 0)
		{
			this.View.GetBarItem("FMinusEntry", "tbIgnoreMinus").Enabled = false;
			this.View.GetBarItem("FStkCountBillAuditEntry", "tbIgnoreStkBillAudit").Enabled = false;
			this.View.GetBarItem("FStkDraftBillEntry", "tbIgnoreStkBillDraft").Enabled = false;
			this.View.GetBarItem("FEntityErrInfo", "tbShowBill").Visible = false;
			this.View.GetBarItem("FStkDraftBillEntry", "tbShowBillStkBillDraft").Visible = false;
			this.View.GetBarItem("FStkCountBillAuditEntry", "tbShowBillStkBillAudit").Visible = false;
			if (errType == InvAccountOnOff.ErrType.Minus)
			{
				this.View.GetControl<EntryGrid>("FStkCountBillAuditEntry").Visible = false;
				this.View.GetControl<EntryGrid>("FStkDraftBillEntry").Visible = false;
				this.View.GetControl<EntryGrid>("FEntityErrInfo").Visible = false;
				this.View.GetControl<EntryGrid>("FMinusEntry").Visible = true;
				this.View.GetBarItem("FMinusEntry", "tbViewDetailRpt").Visible = true;
				if (opResult != null && opResult.MinusErrObject != null && Convert.ToInt32(opResult.MinusErrObject["ErrType"]) == 1)
				{
					this.View.GetBarItem("FMinusEntry", "tbIgnoreMinus").Enabled = true;
				}
			}
			else if (errType == InvAccountOnOff.ErrType.StkDraftBill)
			{
				this.View.GetControl<EntryGrid>("FStkCountBillAuditEntry").Visible = false;
				this.View.GetControl<EntryGrid>("FStkDraftBillEntry").Visible = true;
				this.View.GetControl<EntryGrid>("FEntityErrInfo").Visible = false;
				this.View.GetControl<EntryGrid>("FMinusEntry").Visible = false;
				if (opResult != null && opResult.StkBillDraftErrInfo != null && opResult.StkBillDraftErrInfo.Count > 0)
				{
					this.View.GetBarItem("FStkDraftBillEntry", "tbIgnoreStkBillDraft").Enabled = true;
					List<OperateErrorInfo> list = (from p in opResult.StkBillDraftErrInfo
					where !string.IsNullOrEmpty(Convert.ToString(p.ErrObjType)) && !string.IsNullOrEmpty(Convert.ToString(p.ErrObjKeyID))
					select p).ToList<OperateErrorInfo>();
					if (list != null && list.Count<OperateErrorInfo>() > 0)
					{
						this.View.GetBarItem("FStkDraftBillEntry", "tbShowBillStkBillDraft").Visible = true;
					}
				}
			}
			else
			{
				if (opResult != null && opResult.StkCountBillAuditErrInfo != null)
				{
					if ((from p in opResult.StkCountBillAuditErrInfo
					where p.ErrType == objType
					select p).ToList<OperateErrorInfo>().Count<OperateErrorInfo>() > 0)
					{
						this.View.GetControl<EntryGrid>("FStkCountBillAuditEntry").Visible = true;
						this.View.GetControl<EntryGrid>("FStkDraftBillEntry").Visible = false;
						this.View.GetControl<EntryGrid>("FEntityErrInfo").Visible = false;
						this.View.GetControl<EntryGrid>("FMinusEntry").Visible = false;
						this.View.GetBarItem("FStkCountBillAuditEntry", "tbIgnoreStkBillAudit").Enabled = true;
						List<OperateErrorInfo> list2 = (from p in opResult.StkCountBillAuditErrInfo
						where !string.IsNullOrEmpty(Convert.ToString(p.ErrObjType)) && !string.IsNullOrEmpty(Convert.ToString(p.ErrObjKeyID))
						select p).ToList<OperateErrorInfo>();
						if (list2 != null && list2.Count<OperateErrorInfo>() > 0)
						{
							this.View.GetBarItem("FStkCountBillAuditEntry", "tbShowBillStkBillAudit").Visible = true;
							goto IL_47C;
						}
						goto IL_47C;
					}
				}
				this.View.GetControl<EntryGrid>("FStkCountBillAuditEntry").Visible = false;
				this.View.GetControl<EntryGrid>("FStkDraftBillEntry").Visible = false;
				this.View.GetControl<EntryGrid>("FEntityErrInfo").Visible = true;
				this.View.GetControl<EntryGrid>("FMinusEntry").Visible = false;
				if (opResult != null && opResult.ErrInfo != null)
				{
					if ((from p in opResult.ErrInfo
					where p.ErrType == objType
					select p).ToList<OperateErrorInfo>().Count<OperateErrorInfo>() > 0)
					{
						List<OperateErrorInfo> list3 = (from p in opResult.ErrInfo
						where !string.IsNullOrEmpty(Convert.ToString(p.ErrObjType)) && !string.IsNullOrEmpty(Convert.ToString(p.ErrObjKeyID))
						select p).ToList<OperateErrorInfo>();
						if (list3 != null && list3.Count<OperateErrorInfo>() > 0)
						{
							this.View.GetBarItem("FEntityErrInfo", "tbShowBill").Visible = true;
						}
					}
				}
			}
			IL_47C:
			this.View.UpdateView("FTabErrDetail");
		}

		// Token: 0x06000812 RID: 2066 RVA: 0x00069B68 File Offset: 0x00067D68
		private List<NetworkCtrlResult> BatchStartNetCtl(List<string> orgNum)
		{
			if (orgNum != null)
			{
				List<NetworkCtrlResult> list = null;
				for (int i = 0; i < orgNum.Count; i++)
				{
					NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.AddNetCtrlObj(base.Context, new LocaleValue(base.GetType().Name, 2052), base.GetType().Name, base.GetType().Name + orgNum[i], 6, null, " ", true, true);
					NetworkCtrlServiceHelper.AddMutexNetCtrlObj(base.Context, networkCtrlObject.Id, networkCtrlObject.Id);
					NetWorkRunTimeParam netWorkRunTimeParam = new NetWorkRunTimeParam();
					NetworkCtrlResult networkCtrlResult = NetworkCtrlServiceHelper.BeginNetCtrl(base.Context, networkCtrlObject, netWorkRunTimeParam);
					if (!networkCtrlResult.StartSuccess)
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：库存组织编码[{0}]正处于关账或反关账，不允许操作！", "004023030002149", 5, new object[0]), orgNum[i]), "", 0);
						break;
					}
					if (list == null)
					{
						list = new List<NetworkCtrlResult>();
					}
					list.Add(networkCtrlResult);
				}
				return list;
			}
			return null;
		}

		// Token: 0x06000813 RID: 2067 RVA: 0x00069C63 File Offset: 0x00067E63
		private string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count < 1)
			{
				return string.Format(" {0} = -1 ", key);
			}
			return string.Format(" {0} in ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x06000814 RID: 2068 RVA: 0x00069C94 File Offset: 0x00067E94
		private void SetTransId(List<StockOrgOperateResult> ret)
		{
			if (ret == null || ret.Count < 1)
			{
				return;
			}
			foreach (StockOrgOperateResult stockOrgOperateResult in ret)
			{
				if (!this.dTransId.Keys.Contains(stockOrgOperateResult.StockOrgID))
				{
					this.dTransId.Add(stockOrgOperateResult.StockOrgID, stockOrgOperateResult.TransId);
				}
				else if (this.dTransId.Keys.Contains(stockOrgOperateResult.StockOrgID) && !this.dTransId[stockOrgOperateResult.StockOrgID].Equals(stockOrgOperateResult.TransId))
				{
					this.dTransId[stockOrgOperateResult.StockOrgID] = stockOrgOperateResult.TransId;
				}
			}
		}

		// Token: 0x040002DE RID: 734
		private const string ActionEntityKey = "FEntityAction";

		// Token: 0x040002DF RID: 735
		private const string ErrTypeEntityKey = "FEntityErrType";

		// Token: 0x040002E0 RID: 736
		private const string ErrDetailEntityKey = "FEntityErrInfo";

		// Token: 0x040002E1 RID: 737
		private const string MinusEntityKey = "FMinusEntry";

		// Token: 0x040002E2 RID: 738
		private const string StkCntBillAuditEntryKey = "FStkCountBillAuditEntry";

		// Token: 0x040002E3 RID: 739
		private const string StkBillDraftEntryKey = "FStkDraftBillEntry";

		// Token: 0x040002E4 RID: 740
		private const string MinusActionType = "Minus";

		// Token: 0x040002E5 RID: 741
		private const string StkCountBillAuditActionType = "CntBillAudit";

		// Token: 0x040002E6 RID: 742
		private const string StkBillDraftActionType = "StkBillDraft";

		// Token: 0x040002E7 RID: 743
		private const string STRSETUPINFO = "ExportSetting";

		// Token: 0x040002E8 RID: 744
		private bool isClicked;

		// Token: 0x040002E9 RID: 745
		private bool isOpenAccount;

		// Token: 0x040002EA RID: 746
		private bool isbBusiness;

		// Token: 0x040002EB RID: 747
		private bool bShowErr;

		// Token: 0x040002EC RID: 748
		private bool bRecordMidData;

		// Token: 0x040002ED RID: 749
		private List<StockOrgOperateResult> opResults = new List<StockOrgOperateResult>();

		// Token: 0x040002EE RID: 750
		private Dictionary<string, bool> ignoreCheckInfo = new Dictionary<string, bool>();

		// Token: 0x040002EF RID: 751
		private Dictionary<long, string> dTransId = new Dictionary<long, string>();

		// Token: 0x040002F0 RID: 752
		private List<long> orgViewRptList;

		// Token: 0x040002F1 RID: 753
		private ProgressBar _progressbar;

		// Token: 0x040002F2 RID: 754
		private List<NetworkCtrlResult> _netResults;

		// Token: 0x040002F3 RID: 755
		private readonly List<string> _exportCommonColumns = new List<string>
		{
			"FStockOrgNo",
			"FStockOrgName",
			"FStockOrgDesc",
			"FErrMessage"
		};

		// Token: 0x040002F4 RID: 756
		private bool _IgnoreMinusErr;

		// Token: 0x040002F5 RID: 757
		private bool _IgnoreDraftBill;

		// Token: 0x040002F6 RID: 758
		private bool _IgnoreParaPlugIn;

		// Token: 0x02000096 RID: 150
		protected enum ErrType
		{
			// Token: 0x0400030A RID: 778
			None = -1,
			// Token: 0x0400030B RID: 779
			NoOrg = 1,
			// Token: 0x0400030C RID: 780
			StartDateErr,
			// Token: 0x0400030D RID: 781
			CloseDateErr,
			// Token: 0x0400030E RID: 782
			NoInit,
			// Token: 0x0400030F RID: 783
			FinCloseDate,
			// Token: 0x04000310 RID: 784
			OrgStatusErr = 99,
			// Token: 0x04000311 RID: 785
			UnAuditBill,
			// Token: 0x04000312 RID: 786
			StkDraftBill = 200,
			// Token: 0x04000313 RID: 787
			UnAuditStkCountBill = 500,
			// Token: 0x04000314 RID: 788
			Minus = 1000
		}
	}
}
