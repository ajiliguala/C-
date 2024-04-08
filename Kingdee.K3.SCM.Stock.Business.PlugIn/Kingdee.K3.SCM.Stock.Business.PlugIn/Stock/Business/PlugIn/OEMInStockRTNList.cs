using System;
using System.ComponentModel;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000017 RID: 23
	[Description("受托加工退料单 列表插件")]
	public class OEMInStockRTNList : AbstractListPlugIn
	{
		// Token: 0x060000B3 RID: 179 RVA: 0x00009ADC File Offset: 0x00007CDC
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FSTOCKORGID",
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
	}
}
