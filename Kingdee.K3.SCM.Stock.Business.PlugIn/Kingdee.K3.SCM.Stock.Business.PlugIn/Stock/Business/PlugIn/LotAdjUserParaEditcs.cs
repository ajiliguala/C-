using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200001D RID: 29
	public class LotAdjUserParaEditcs : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000135 RID: 309 RVA: 0x000115A8 File Offset: 0x0000F7A8
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			Control control = this.View.GetControl("FTab_Business");
			if (control != null && this.View.ParentFormView != null && this.View.ParentFormView is IListView)
			{
				control.Visible = false;
			}
		}
	}
}
