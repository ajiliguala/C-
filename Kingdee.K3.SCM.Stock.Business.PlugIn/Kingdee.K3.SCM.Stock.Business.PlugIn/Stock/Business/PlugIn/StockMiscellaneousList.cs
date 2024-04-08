using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000085 RID: 133
	public class StockMiscellaneousList : AbstractListPlugIn
	{
		// Token: 0x06000660 RID: 1632 RVA: 0x0004DA40 File Offset: 0x0004BC40
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FStockOrgId",
				PurchaseParameterKey = "GroupDataIsolation",
				PurchaseParameterObject = "STK_StockParameter",
				BusinessGroupKey = "FSTOCKERGROUPID",
				OperatorType = "WHY"
			});
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.AppendQueryFilter(text);
			}
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0004DACC File Offset: 0x0004BCCC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.ToUpperInvariant() == "BTN_SYNCREFAMOUNT")
			{
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				if (selectedRowsInfo != null && selectedRowsInfo.Count > 0)
				{
					List<long> list = (from m in selectedRowsInfo
					select Convert.ToInt64(m.PrimaryKeyValue)).Distinct<long>().ToList<long>();
					if (!MisBillServiceHelper.UpdateMisCellaneousRefAmount(base.Context, list))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("参考总成本刷新失败", "004023030009409", 5, new object[0]), "", 0);
						return;
					}
					this.View.Refresh();
					this.View.ShowMessage(ResManager.LoadKDString("参考总成本刷新成功", "004023030009418", 5, new object[0]), 0);
					return;
				}
				else
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("请至少选择一条数据", "004023030009412", 5, new object[0]), "", 0);
				}
			}
		}
	}
}
