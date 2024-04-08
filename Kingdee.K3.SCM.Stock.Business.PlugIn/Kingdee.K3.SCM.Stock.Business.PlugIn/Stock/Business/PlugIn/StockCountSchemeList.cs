using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200008E RID: 142
	public class StockCountSchemeList : AbstractListPlugIn
	{
		// Token: 0x0600072E RID: 1838 RVA: 0x0005AFB0 File Offset: 0x000591B0
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (string.IsNullOrWhiteSpace(e.SortString))
			{
				e.SortString = " FCreateDate DESC ";
			}
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x0005AFD4 File Offset: 0x000591D4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBQUERYCOUNTINPUT"))
				{
					return;
				}
				this.QueryCycleCountInput();
			}
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x0005B004 File Offset: 0x00059204
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
			listShowParameter.CustomParams.Add("source", "1");
			listShowParameter.CustomParams.Add("CountTableIds", string.Join<long>(",", fidsFun));
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x0005B10C File Offset: 0x0005930C
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

		// Token: 0x06000732 RID: 1842 RVA: 0x0005B17C File Offset: 0x0005937C
		private List<long> GetSelBillOrgIds(long[] ids)
		{
			List<long> list = new List<long>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FStockOrgId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_StockCountScheme",
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
