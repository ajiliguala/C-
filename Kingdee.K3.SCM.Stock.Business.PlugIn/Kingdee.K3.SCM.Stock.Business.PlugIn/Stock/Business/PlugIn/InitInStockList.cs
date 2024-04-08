using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200001A RID: 26
	public class InitInStockList : AbstractListPlugIn
	{
		// Token: 0x060000EF RID: 239 RVA: 0x0000DBC4 File Offset: 0x0000BDC4
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = string.Empty;
			bool flag = Convert.ToBoolean(e.CustomFilter["FSelectAllOrg"]);
			string text2 = string.Empty;
			if (!flag)
			{
				text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			}
			else
			{
				List<long> isolationOrgList = this.ListView.Model.FilterParameter.IsolationOrgList;
				if (isolationOrgList != null && isolationOrgList.Count<long>() > 0)
				{
					text2 = string.Join<long>(",", isolationOrgList);
				}
			}
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FStockOrgId",
				PurchaseParameterKey = "GroupDataIsolation",
				PurchaseParameterObject = "PUR_SystemParameter",
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
