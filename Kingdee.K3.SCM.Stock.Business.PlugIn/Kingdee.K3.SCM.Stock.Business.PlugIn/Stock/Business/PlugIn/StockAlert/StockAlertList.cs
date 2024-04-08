using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockAlert
{
	// Token: 0x02000034 RID: 52
	[Description("仓库最大最小安全库存列表插件")]
	public class StockAlertList : AbstractListPlugIn
	{
		// Token: 0x0600021B RID: 539 RVA: 0x0001AC60 File Offset: 0x00018E60
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			object obj = e.CustomFilter["CustOrgList"];
			if (obj == null || obj == "")
			{
				string arg = base.Context.CurrentOrganizationInfo.ID.ToString();
				e.AppendQueryFilter(string.Format("FSTOCKORG IN ({0})", arg));
			}
			if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
			{
				e.AppendQueryFilter(string.Format("FSTOCKORG IN ({0})", obj));
			}
		}

		// Token: 0x0600021C RID: 540 RVA: 0x0001ACDC File Offset: 0x00018EDC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbSplitNew"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("新增", "004023030009256", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (this.IsCanNew("BD_StockAlert"))
				{
					this.ShowBatchNewView();
					return;
				}
				this.View.ShowErrMessage(ResManager.LoadKDString("没有新增权限", "004023030002314", 5, new object[0]), "", 0);
			}
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0001AD8C File Offset: 0x00018F8C
		private bool IsCanNew(string formId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = formId
			}, "fce8b1aca2144beeb3c6655eaf78bc34");
			return permissionAuthResult.Passed;
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0001ADCC File Offset: 0x00018FCC
		private void ShowBatchNewView()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "BD_StockAlertNew";
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult formResult)
			{
				this.View.Refresh();
			});
		}
	}
}
