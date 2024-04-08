using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000053 RID: 83
	[Description("初始库存列表插件")]
	public class InvInitBillList : AbstractListPlugIn
	{
		// Token: 0x060003A1 RID: 929 RVA: 0x0002B70C File Offset: 0x0002990C
		public override void PrepareFilterParameter(FilterArgs e)
		{
			List<string> appendLoadFieldList = e.AppendLoadFieldList;
			ColumnField columnField = (from P in this.ListModel.FilterParameter.ColumnInfo
			where P.Key == "FStockLocId"
			select P).FirstOrDefault<ColumnField>();
			if (columnField != null)
			{
				appendLoadFieldList.Add("FSubStockId");
			}
		}
	}
}
