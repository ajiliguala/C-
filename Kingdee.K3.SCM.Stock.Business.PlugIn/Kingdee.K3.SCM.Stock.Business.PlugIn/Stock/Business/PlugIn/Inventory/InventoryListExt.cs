using System;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Inventory
{
	// Token: 0x0200000D RID: 13
	public class InventoryListExt : AbstractListPlugIn
	{
		// Token: 0x0600005F RID: 95 RVA: 0x000062CF File Offset: 0x000044CF
		public override void BeforeGetDataForTempTableAccess(BeforeGetDataForTempTableAccessArgs e)
		{
			base.BeforeGetDataForTempTableAccess(e);
			if (string.IsNullOrWhiteSpace(e.TableName))
			{
				return;
			}
			StockServiceHelper.RegexInvDetailQueryBaseData(this.View.Context, e.TableName);
		}
	}
}
