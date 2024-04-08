using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200009D RID: 157
	public class LockStockList : AbstractListPlugIn
	{
		// Token: 0x0600092B RID: 2347 RVA: 0x0007A644 File Offset: 0x00078844
		public override void OnInitialize(InitializeEventArgs e)
		{
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "MFG_PLNParam", "IsEnableReserve", false);
			this.isEnableReserve = Convert.ToBoolean(systemProfile);
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x0007A67C File Offset: 0x0007887C
		public override void PrepareFilterParameter(FilterArgs e)
		{
			if (this.View.OpenParameter.GetCustomParameter("Parameters") != null)
			{
				string text = this.View.OpenParameter.GetCustomParameter("Parameters").ToString();
				text = text.Replace(",", "','");
				e.AppendQueryFilter(string.Format("FBillDetailID in ('{0}')", text));
				this.isHideButton = false;
			}
			if (this.View.OpenParameter.GetCustomParameter("InvDetailIds") != null)
			{
				string arg = this.View.OpenParameter.GetCustomParameter("InvDetailIds").ToString();
				e.FilterString = string.Format("FINVDETAILID IN ('{0}')", arg);
			}
			if (!this.isEnableReserve)
			{
				e.AppendQueryFilter("FLINKTYPE = '4'");
			}
			this.AddMustSelFields();
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x0007A740 File Offset: 0x00078940
		public override void AfterBindData(EventArgs e)
		{
			this.ListView.GetMainBarItem("Btn_LOCK").Visible = this.isHideButton;
			this.ListView.GetMainBarItem("Btn_UNLOCK").Visible = this.isHideButton;
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0007A778 File Offset: 0x00078978
		public override void FormatCellValue(FormatCellValueArgs args)
		{
			string key = args.Header.Key;
			if (StringUtils.EqualsIgnoreCase(key, "FLOCKQTY"))
			{
				return;
			}
			base.FormatCellValue(args);
		}

		// Token: 0x0600092F RID: 2351 RVA: 0x0007A86C File Offset: 0x00078A6C
		private void AddMustSelFields()
		{
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			bool flag = columnInfo.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FLockQty"));
			if (!flag)
			{
				return;
			}
			List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
			Field field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FBaseLockQty"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FBaseLockQty")) == null && field != null && flag)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 106,
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
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNum"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNum")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 106,
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
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNom"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FStoreurNom")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 106,
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
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FUnitId"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FUnitId")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 56,
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
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FUnitRoundType"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FUnitRoundType")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 56,
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

		// Token: 0x06000930 RID: 2352 RVA: 0x0007ADE0 File Offset: 0x00078FE0
		private void FormatQty(FormatCellValueArgs args)
		{
			string key = args.Header.Key;
			string text = "";
			if (StringUtils.EqualsIgnoreCase(key, "FLOCKQTY"))
			{
				text = "FBaseLockQty";
			}
			decimal num = (args.DataRow["FStoreurNum"] is DBNull) ? 0m : Convert.ToDecimal(args.DataRow["FStoreurNum"]);
			decimal num2 = (args.DataRow["FStoreurNom"] is DBNull) ? 0m : Convert.ToDecimal(args.DataRow["FStoreurNom"]);
			int num3 = Convert.ToInt32(args.DataRow["FUnitID_FPrecision"]);
			int num4 = (args.DataRow["FUnitRoundType"] is DBNull) ? 0 : Convert.ToInt32(args.DataRow["FUnitRoundType"]);
			decimal d = (args.DataRow[text] is DBNull) ? 0m : Convert.ToDecimal(args.DataRow[text]);
			if (num != 0m && num2 != 0m)
			{
				decimal num5 = d * num2 / num;
				switch (num4)
				{
				case 2:
					num5 = MathUtil.Round(num5, num3, 2);
					goto IL_174;
				case 3:
					num5 = MathUtil.Round(num5, num3, 3);
					goto IL_174;
				}
				num5 = MathUtil.Round(num5, num3, 0);
				IL_174:
				args.FormateValue = num5.ToString();
				return;
			}
			base.FormatCellValue(args);
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x0007AF78 File Offset: 0x00079178
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "BTN_LOCK"))
				{
					if (!(a == "BTN_UNLOCK"))
					{
						if (!(a == "BTN_QUERYLOG"))
						{
							return;
						}
						this.ShowQueryLog();
					}
					else
					{
						string operateName = ResManager.LoadKDString("解锁", "004023030009246", 5, new object[0]);
						string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
						if (!string.IsNullOrWhiteSpace(onlyViewMsg))
						{
							e.Cancel = true;
							this.View.ShowErrMessage(onlyViewMsg, "", 0);
							return;
						}
						if (!this.CheckPermission(e))
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("没有锁库权限!", "004023030002179", 5, new object[0]), ResManager.LoadKDString("权限错误", "004023030002161", 5, new object[0]), 0);
							return;
						}
						this.UnLockForm();
						return;
					}
				}
				else
				{
					string operateName = ResManager.LoadKDString("锁库", "004023030009245", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						e.Cancel = true;
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						return;
					}
					if (!this.CheckPermission(e))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("没有锁库权限!", "004023030002179", 5, new object[0]), ResManager.LoadKDString("权限错误", "004023030002161", 5, new object[0]), 0);
						return;
					}
					this.ShowLockForm("STK_LockStockOperate", null);
					return;
				}
			}
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x0007B0FC File Offset: 0x000792FC
		private void ShowQueryLog()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "STK_LOCKSTOCKLOG"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("没有锁库日志的查看权限!", "004023030034095", 5, new object[0]), "", 0, null, 1);
				return;
			}
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), 0);
				return;
			}
			if (selectedRowsInfo.Count > 2000)
			{
				this.View.ShowMessage(ResManager.LoadKDString("最大一次查询2000行明细数据，请调整。", "004023030034096", 5, new object[0]), 0);
				return;
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_LOCKSTOCKLOG";
			listShowParameter.Caption = ResManager.LoadKDString("锁库日志", "004023030009006", 5, new object[0]);
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.CustomParams.Add("IsFromStockLock", "True");
			listShowParameter.CustomComplexParams.Add("Fids", (from p in selectedRowsInfo
			select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>());
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsLookUp = false;
			listShowParameter.IsIsolationOrg = false;
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x0007B29C File Offset: 0x0007949C
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			string text = StringUtils.EqualsIgnoreCase(e.BarItemKey, "BTN_LOCK") ? "6ff9853429aa4384892b4f5d0f86dd1b" : (StringUtils.EqualsIgnoreCase(e.BarItemKey, "BTN_UNLOCK") ? "f483e76da3ba4fdb96052059b5d71c6c" : "");
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "STK_LockStock"
			}, text);
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x0007B348 File Offset: 0x00079548
		private bool CheckUnlockPermission(out List<long> permittedLockIds)
		{
			permittedLockIds = new List<long>();
			string text = "f483e76da3ba4fdb96052059b5d71c6c";
			string id = "STK_LockStock";
			List<long> list = (from p in this.ListView.SelectedRowsInfo
			select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>();
			Dictionary<long, long> lockStockOrgInfo = StockServiceHelper.GetLockStockOrgInfo(base.Context, list);
			if (lockStockOrgInfo.Keys.Count < 1)
			{
				return true;
			}
			List<BusinessObject> list2 = new List<BusinessObject>();
			foreach (long num in lockStockOrgInfo.Values.Distinct<long>())
			{
				list2.Add(new BusinessObject(num)
				{
					Id = id,
					pkId = num.ToString()
				});
			}
			List<PermissionAuthResult> list3 = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, list2, text);
			if (list3 == null || list3.Count < 1)
			{
				return false;
			}
			foreach (long num2 in lockStockOrgInfo.Keys)
			{
				long orgId = lockStockOrgInfo[num2];
				PermissionAuthResult permissionAuthResult = list3.FirstOrDefault((PermissionAuthResult p) => Convert.ToInt64(p.Id) == orgId && p.Passed);
				if (permissionAuthResult != null)
				{
					permittedLockIds.Add(num2);
				}
			}
			return permittedLockIds.Count > 0;
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x0007B4D0 File Offset: 0x000796D0
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			this.UnLockForm();
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x0007B4D8 File Offset: 0x000796D8
		private void UnLockForm()
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("解锁必须选中一行!", "004023030000304", 5, new object[0]), 0);
				return;
			}
			List<long> list = null;
			if (!this.CheckUnlockPermission(out list))
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("没有该操作权限!", "004023030002158", 5, new object[0]), ResManager.LoadKDString("权限错误", "004023030002161", 5, new object[0]), 0, null, 1);
				return;
			}
			if (list == null || list.Count < 1)
			{
				return;
			}
			if (this.BatchStartNetCtl(list))
			{
				this.ShowLockForm("STK_UnLockStockOperate", list);
			}
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x0007B588 File Offset: 0x00079788
		private bool BatchStartNetCtl(List<long> selectIds)
		{
			if (selectIds.Count <= 0)
			{
				return true;
			}
			new List<NetworkCtrlResult>();
			foreach (long num in selectIds)
			{
				NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.AddNetCtrlObj(base.Context, new LocaleValue(base.GetType().Name, 2052), base.GetType().Name, base.GetType().Name + num, 6, null, " ", true, true);
				NetworkCtrlServiceHelper.AddMutexNetCtrlObj(base.Context, networkCtrlObject.Id, networkCtrlObject.Id);
				NetWorkRunTimeParam netWorkRunTimeParam = new NetWorkRunTimeParam();
				NetworkCtrlResult networkCtrlResult = NetworkCtrlServiceHelper.BeginNetCtrl(base.Context, networkCtrlObject, netWorkRunTimeParam);
				if (!networkCtrlResult.StartSuccess)
				{
					this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：库存锁库编码【{0}】已经被打开解锁，不允许操作！", "004023030000307", 5, new object[0]), num), "", 0);
					return false;
				}
				if (this.netResultList == null)
				{
					this.netResultList = new List<NetworkCtrlResult>();
				}
				this.netResultList.Add(networkCtrlResult);
			}
			return true;
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0007B6C4 File Offset: 0x000798C4
		private void ShowLockForm(string formID, List<long> para)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = formID;
			dynamicFormShowParameter.CustomParams.Add("OpType", "StockLock");
			if (para != null && para.Count > 0)
			{
				dynamicFormShowParameter.CustomParams.Add("Parameters", string.Join<long>(",", para));
			}
			this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.AfterShowLock));
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0007B74A File Offset: 0x0007994A
		protected void AfterShowLock(FormResult result)
		{
			if (this.netResultList != null)
			{
				NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, this.netResultList);
				this.netResultList = null;
			}
			if (result.ReturnData != null)
			{
				this.View.Refresh();
			}
		}

		// Token: 0x040003A0 RID: 928
		private List<NetworkCtrlResult> netResultList = new List<NetworkCtrlResult>();

		// Token: 0x040003A1 RID: 929
		private bool isEnableReserve;

		// Token: 0x040003A2 RID: 930
		private bool isHideButton = true;
	}
}
