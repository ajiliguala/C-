using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
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
	// Token: 0x02000068 RID: 104
	public class CycleCountPlanList : AbstractListPlugIn
	{
		// Token: 0x06000483 RID: 1155 RVA: 0x0003600C File Offset: 0x0003420C
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBGENERATECOUNTTABLE"))
				{
					if (!(a == "TBSTARTSERVICE"))
					{
						if (!(a == "TBSTOPSERVICE"))
						{
							if (!(a == "TBQUERYCOUNTTABLE"))
							{
								return;
							}
							this.QueryCountTable();
						}
						else
						{
							long[] fidsFun = this.GetFIDsFun();
							if (fidsFun == null || fidsFun.ToList<long>().Count == 0)
							{
								this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), 0);
								return;
							}
							StockServiceHelper.StartStopAutoCycleCountPlan(base.Context, fidsFun, false);
							this.ListView.ShowMessage(ResManager.LoadKDString("停用自动计划成功！", "004023030002080", 5, new object[0]), 0);
							return;
						}
					}
					else
					{
						long[] fidsFun = this.GetFIDsFun();
						if (fidsFun == null || fidsFun.ToList<long>().Count == 0)
						{
							this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), 0);
							return;
						}
						StockServiceHelper.StartStopAutoCycleCountPlan(base.Context, fidsFun, true);
						this.ListView.ShowMessage(ResManager.LoadKDString("启用自动计划成功！", "004023030002074", 5, new object[0]), 0);
						return;
					}
				}
				else
				{
					long[] fidsFun2 = this.GetFIDsFun();
					if (fidsFun2 == null || fidsFun2.ToList<long>().Count == 0)
					{
						this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), 0);
						return;
					}
					OperateResultCollection operateResultCollection = StockServiceHelper.GenerateCycleCountTable(base.Context, fidsFun2);
					this.ListView.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
					return;
				}
			}
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x000361A4 File Offset: 0x000343A4
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

		// Token: 0x06000485 RID: 1157 RVA: 0x00036214 File Offset: 0x00034414
		private void QueryCountTable()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "STK_CycleCountTable"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有物料盘点表的查看权限！", "004023030002083", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			List<long> selBillOrgIds = this.GetSelBillOrgIds(fidsFun);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_CycleCountTable";
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", selBillOrgIds);
			if (selBillOrgIds.Count == 1)
			{
				listShowParameter.UseOrgId = selBillOrgIds[0];
			}
			Common.SetFormOpenStyle(this.View, listShowParameter);
			listShowParameter.CustomParams.Add("PlanIds", string.Join<long>(",", fidsFun));
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x00036300 File Offset: 0x00034500
		private List<long> GetSelBillOrgIds(long[] ids)
		{
			List<long> list = new List<long>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FStockOrgId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_CycleCountPlan",
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
	}
}
