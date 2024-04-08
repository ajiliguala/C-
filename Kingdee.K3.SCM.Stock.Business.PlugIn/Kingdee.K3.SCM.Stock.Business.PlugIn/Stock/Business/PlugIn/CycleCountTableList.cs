using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200004D RID: 77
	public class CycleCountTableList : AbstractListPlugIn
	{
		// Token: 0x06000367 RID: 871 RVA: 0x00029828 File Offset: 0x00027A28
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBQUERYCOUNTINPUT")
				{
					this.QueryCycleCountInput();
					return;
				}
				if (a == "TBQUERYCOUNTPLAN")
				{
					this.QuerySrcleBill("STK_CycleCountPlan");
					return;
				}
			}
			base.BarItemClick(e);
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0002987C File Offset: 0x00027A7C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SUBMIT"))
				{
					return;
				}
				e.Cancel = this.VaildateData();
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x000298C4 File Offset: 0x00027AC4
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			object customParameter = this.View.OpenParameter.GetCustomParameter("PlanIds");
			string text = "";
			if (customParameter != null)
			{
				text = customParameter.ToString().Replace("'", "");
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				e.AppendQueryFilter(string.Format(" FPlanId IN ({0})", text));
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x00029930 File Offset: 0x00027B30
		private long[] GetFIDsFun()
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			List<object> list = (from p in selectedRowsInfo
			select p.PrimaryKeyValue).ToList<object>();
			long[] array = new long[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = Convert.ToInt64(list[i]);
			}
			return array;
		}

		// Token: 0x0600036B RID: 875 RVA: 0x000299A0 File Offset: 0x00027BA0
		private void QueryCycleCountInput()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "STK_StockCountInput"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有物料盘点作业的查看权限！", "004023030002104", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			List<long> selBillOrgIds = this.GetSelBillOrgIds(fidsFun);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_StockCountInput";
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", selBillOrgIds);
			if (selBillOrgIds.Count == 1)
			{
				listShowParameter.UseOrgId = selBillOrgIds[0];
			}
			Common.SetFormOpenStyle(this.View, listShowParameter);
			listShowParameter.CustomParams.Add("source", "2");
			listShowParameter.CustomParams.Add("CountTableIds", string.Join<long>(",", fidsFun));
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600036C RID: 876 RVA: 0x00029AA0 File Offset: 0x00027CA0
		private List<long> GetSelBillOrgIds(long[] ids)
		{
			List<long> list = new List<long>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FStockOrgId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_CycleCountTable",
				FilterClauseWihtKey = string.Format("FID IN ({0})  ", string.Join<long>(",", ids)),
				SelectItems = list2
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				return list;
			}
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				long item = Convert.ToInt64(dynamicObjectCollection[i]["FStockOrgId"]);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x0600036D RID: 877 RVA: 0x00029B5C File Offset: 0x00027D5C
		private void QuerySrcleBill(string formID)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = formID
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有对应单据的查看权限！", "004023030002107", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FPlanId"));
			list.Add(new SelectorItemInfo("FStockOrgId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_CycleCountTable",
				FilterClauseWihtKey = string.Format("FID IN ({0})  ", string.Join<long>(",", fidsFun)),
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				return;
			}
			long[] array = new long[dynamicObjectCollection.Count];
			List<long> list2 = new List<long>();
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				long item = Convert.ToInt64(dynamicObjectCollection[i]["FStockOrgId"]);
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
				array[i] = Convert.ToInt64(dynamicObjectCollection[i]["FPlanId"]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formID;
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", list2);
			if (list2.Count == 1)
			{
				listShowParameter.UseOrgId = list2[0];
			}
			Common.SetFormOpenStyle(this.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", array));
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600036E RID: 878 RVA: 0x00029D44 File Offset: 0x00027F44
		private bool VaildateData()
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count<ListSelectedRow>() < 1)
			{
				return true;
			}
			List<string> list = (from p in selectedRowsInfo
			select p.PrimaryKeyValue.ToString()).ToList<string>();
			OperateResultCollection operateResultCollection = CycleCountTablePageService.ValidatePlanDate(base.Context, list);
			if (operateResultCollection.Count > 0)
			{
				this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
				return true;
			}
			return false;
		}
	}
}
