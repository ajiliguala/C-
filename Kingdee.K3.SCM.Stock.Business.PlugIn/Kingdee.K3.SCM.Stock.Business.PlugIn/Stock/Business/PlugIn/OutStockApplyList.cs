using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.ConvertElement;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000015 RID: 21
	[Description("出库申请单序时簿插件")]
	public class OutStockApplyList : AbstractListPlugIn
	{
		// Token: 0x06000098 RID: 152 RVA: 0x00008844 File Offset: 0x00006A44
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 12)
			{
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				if (list != null && list.Count > 0)
				{
					e.Bills = (from c in list
					where !c.FormID.Equals("FA_CARD", StringComparison.OrdinalIgnoreCase)
					select c).ToList<ConvertBillElement>();
				}
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x000088A8 File Offset: 0x00006AA8
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
		}
	}
}
