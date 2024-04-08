using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200000F RID: 15
	[Description("锁库日志列表插件")]
	public class LockStockLogList : AbstractListPlugIn
	{
		// Token: 0x06000063 RID: 99 RVA: 0x0000633C File Offset: 0x0000453C
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object customParameter = this.ListView.OpenParameter.GetCustomParameter("IsFromStockLock");
			if (customParameter != null)
			{
				this.isFromStockLockQuery = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00006388 File Offset: 0x00004588
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (this.isFromStockLockQuery)
			{
				List<long> list = this.ListView.OpenParameter.GetCustomParameter("Fids") as List<long>;
				if (list != null && list.Count<long>() > 0)
				{
					if (list.Count<long>() == 1)
					{
						e.FilterString = string.Format(" FLockStockId = {0} ", list[0]);
					}
					else if (list.Count<long>() < 50)
					{
						e.FilterString = string.Format(" FLockStockId IN ({0}) ", string.Join<long>(",", list));
					}
					else
					{
						e.FilterString = string.Format(" FLockStockId IN (SELECT /*+ cardinality(LOGS {0})*/ LOGS.FID FROM table(fn_StrSplit(@FID, ',', 1)) LOGS) ", list.Distinct<long>().Count<long>());
						e.SqlParams.Add(new SqlParam("@FID", 161, list.Distinct<long>().ToArray<long>()));
					}
					if (string.IsNullOrWhiteSpace(e.SortString))
					{
						e.SortString = " FLockStockId DESC , FOperateDate DESC ";
					}
				}
			}
			if (string.IsNullOrWhiteSpace(e.SortString))
			{
				e.SortString = " FOperateDate DESC ";
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00006498 File Offset: 0x00004698
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBUNLOCK"))
				{
					return;
				}
				e.Cancel = !this.DOUNLOCK(e);
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00006548 File Offset: 0x00004748
		private bool DOUNLOCK(BarItemClickEventArgs e)
		{
			string operateName = ResManager.LoadKDString("解锁", "004023030009246", 5, new object[0]);
			string text = Common.GetOnlyViewMsg(base.Context, operateName);
			if (!string.IsNullOrWhiteSpace(text))
			{
				this.View.ShowErrMessage(text, "", 0);
				return false;
			}
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo == null || selectedRowsInfo.Count == 0)
			{
				text = ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			List<long> list = (from p in selectedRowsInfo
			select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>();
			if (list == null || list.Count<long>() < 1)
			{
				text = ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			DynamicObjectCollection stockLockLogInfo = StockServiceHelper.GetStockLockLogInfo(base.Context, list);
			if (stockLockLogInfo == null || stockLockLogInfo.Count == 0)
			{
				text = ResManager.LoadKDString("已不存在锁库数据，请调整勾选数据！", "00444711000025771", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			List<DynamicObject> list2 = (from p in stockLockLogInfo
			where Convert.ToString(p["FOPERATETYPE"]).Equals("Lock")
			select p).ToList<DynamicObject>();
			if (list2 == null || list2.Count == 0)
			{
				text = ResManager.LoadKDString("没有勾选操作类型为“锁库”数据，请先选择数据！", "00444711000025772", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			List<long> lstOrgids = this.GetUnLockPermissionOrg();
			List<DynamicObject> list3 = (from p in list2
			where lstOrgids.Contains(Convert.ToInt64(p["FSTOCKORGID"]))
			select p).ToList<DynamicObject>();
			if (list3 == null || list3.Count == 0)
			{
				text = ResManager.LoadKDString("没有对应组织下的解锁操作权限！", "00444711030045647", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			List<DynamicObject> list4 = (from p in list3
			where Convert.ToInt64(p["FENTRYID"]) > 0L
			select p).ToList<DynamicObject>();
			if (list4 == null || list4.Count == 0)
			{
				text = ResManager.LoadKDString("已不存在锁库数据，请调整勾选数据！", "00444711000025771", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			List<long> list5 = (from p in list4
			select Convert.ToInt64(p["FENTRYID"])).Distinct<long>().ToList<long>();
			if (this.BatchStartNetCtl(list5))
			{
				this.ShowLockForm("STK_UnLockStockOperate", list5, list4);
				return true;
			}
			return false;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00006804 File Offset: 0x00004A04
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
					this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("网络冲突：锁库日志【{0}】已经被打开解锁，不允许操作！", "00444711000025774", 5, new object[0]), num), "", 0);
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

		// Token: 0x06000068 RID: 104 RVA: 0x00006940 File Offset: 0x00004B40
		protected List<long> GetUnLockPermissionOrg()
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = "STK_LockStock",
				PermissionControl = 1,
				SubSystemId = "21"
			};
			return PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "f483e76da3ba4fdb96052059b5d71c6c");
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00006988 File Offset: 0x00004B88
		private void ShowLockForm(string formID, List<long> para, List<DynamicObject> dyData)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = formID;
			dynamicFormShowParameter.CustomParams.Add("OpType", "StockLockLog");
			if (para != null && para.Count > 0)
			{
				dynamicFormShowParameter.CustomParams.Add("Parameters", string.Join<long>(",", para));
			}
			dynamicFormShowParameter.CustomComplexParams.Add("StockLockLogData", dyData);
			this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.AfterShowLock));
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00006A1F File Offset: 0x00004C1F
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

		// Token: 0x04000028 RID: 40
		private bool isFromStockLockQuery;

		// Token: 0x04000029 RID: 41
		private List<NetworkCtrlResult> netResultList = new List<NetworkCtrlResult>();
	}
}
