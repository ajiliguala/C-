using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Metadata.FormElement;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000055 RID: 85
	[Description("即时库存用户参数设置插件")]
	public class InvUserSettingEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060003D1 RID: 977 RVA: 0x0002E260 File Offset: 0x0002C460
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this.View == null || this.View.ParentFormView == null || this.View.ParentFormView.BillBusinessInfo == null)
			{
				return;
			}
			Form form = this.View.ParentFormView.BillBusinessInfo.GetForm();
			if (form == null)
			{
				return;
			}
			if (form.Id.Equals("STK_Inventory"))
			{
				this.View.LockField("FIsShowFilter", false);
				return;
			}
			this.View.LockField("FQueryType", false);
			this.View.LockField("FUseSchemeFilter", false);
		}
	}
}
