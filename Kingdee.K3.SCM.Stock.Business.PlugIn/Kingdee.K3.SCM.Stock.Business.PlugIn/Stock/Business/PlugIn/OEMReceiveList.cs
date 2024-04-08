using System;
using System.ComponentModel;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000019 RID: 25
	[Description("受托加工收料单 列表插件")]
	public class OEMReceiveList : AbstractListPlugIn
	{
		// Token: 0x060000ED RID: 237 RVA: 0x0000DB40 File Offset: 0x0000BD40
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = Convert.ToString(e.CustomFilter["OrgList"]);
			BusinessGroupDataIsolationArgs businessGroupDataIsolationArgs = new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FSTOCKORGID",
				PurchaseParameterKey = "GroupDataIsolation",
				PurchaseParameterObject = "STK_StockParameter",
				BusinessGroupKey = "FSTOCKERGROUPID",
				OperatorType = "WHY"
			};
			string text2 = SCMCommon.GetfilterGroupDataIsolation(this, text, businessGroupDataIsolationArgs);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))
			{
				e.AppendQueryFilter(text2);
			}
		}
	}
}
