using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000050 RID: 80
	public class InvCheckResult : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600038D RID: 909 RVA: 0x0002AE44 File Offset: 0x00029044
		public override void AfterBindData(EventArgs e)
		{
			if (this.Model.DataObject["ErrType"] != null && !string.IsNullOrWhiteSpace(this.Model.DataObject["ErrType"].ToString()))
			{
				this.errType = Convert.ToInt32(this.Model.DataObject["ErrType"].ToString());
			}
			if (this.errType == 1)
			{
				this.View.GetControl<Control>("FBtnOk").Text = ResManager.LoadKDString("是", "004023030005539", 5, new object[0]);
				this.View.GetControl<Control>("FBtnNo").Visible = true;
			}
			else
			{
				this.View.GetControl<Control>("FBtnOk").Text = ResManager.LoadKDString("确定", "004023030002152", 5, new object[0]);
				this.View.GetControl<Control>("FBtnNo").Visible = false;
			}
			this.View.GetControl<Control>("FTitle").SetValue(this.errTtitle);
			base.AfterBindData(e);
		}

		// Token: 0x0600038E RID: 910 RVA: 0x0002AF60 File Offset: 0x00029160
		public override void BeforeBindData(EventArgs e)
		{
			object obj = null;
			if (this.View != null && this.View.ParentFormView != null && this.View.ParentFormView.Session != null)
			{
				this.View.ParentFormView.Session.TryGetValue("_OperationResultKey_", out obj);
			}
			IInteractionResult interactionResult = obj as IInteractionResult;
			if (interactionResult == null || !interactionResult.Sponsor.StartsWith("Kingdee.K3.SCM.App.Core.AppBusinessService.UpdateStockService,Kingdee.K3.SCM.App.Core"))
			{
				return;
			}
			this.Model.DataObject = interactionResult.InteractionContext.Option.GetVariableValue<DynamicObject>("STK_InvCheckResult");
			this.errTtitle = interactionResult.InteractionContext.Option.GetVariableValue<string>("_FormTitle_");
			base.BeforeBindData(e);
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0002B014 File Offset: 0x00029214
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			InteractionFormResult interactionFormResult = new InteractionFormResult(StringUtils.EqualsIgnoreCase(e.Key, "FBtnOk") && this.errType == 1, this.Model.DataObject);
			this.View.ReturnToParentWindow(interactionFormResult);
			this.View.Close();
		}

		// Token: 0x04000134 RID: 308
		private int errType;

		// Token: 0x04000135 RID: 309
		private string errTtitle = "";
	}
}
