using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000003 RID: 3
	public class StockBillUserParamEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000020FE File Offset: 0x000002FE
		public override void AfterBindData(EventArgs e)
		{
			if (!this.View.Context.IsMultiOrg)
			{
				this.View.StyleManager.SetVisible("FShowIosBill", "SingleOrgHide", false);
			}
		}
	}
}
