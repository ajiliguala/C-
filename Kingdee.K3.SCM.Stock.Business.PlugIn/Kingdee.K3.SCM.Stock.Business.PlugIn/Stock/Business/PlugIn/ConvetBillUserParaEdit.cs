using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000004 RID: 4
	public class ConvetBillUserParaEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000006 RID: 6 RVA: 0x00002138 File Offset: 0x00000338
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
