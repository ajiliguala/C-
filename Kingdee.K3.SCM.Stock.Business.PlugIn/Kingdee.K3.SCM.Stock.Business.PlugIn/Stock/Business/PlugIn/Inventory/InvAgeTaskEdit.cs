using System;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Inventory
{
	// Token: 0x02000009 RID: 9
	[Description("库龄定时计算单据插件")]
	public class InvAgeTaskEdit : AbstractBillPlugIn
	{
		// Token: 0x0600003C RID: 60 RVA: 0x0000490C File Offset: 0x00002B0C
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBEDITSCHEDULE"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("修改执行计划", "004023030009257", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					base.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.ShowScheduleEditForm();
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00004984 File Offset: 0x00002B84
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (e.OperationResult == null || !e.OperationResult.IsSuccess)
			{
				return;
			}
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				a == "SAVE";
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00004A04 File Offset: 0x00002C04
		private void ShowScheduleEditForm()
		{
			string formId = "BOS_SCHEDULETYPE";
			DynamicObject dynamicObject = this.Model.GetValue("FScheduleId") as DynamicObject;
			if (dynamicObject == null)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("库龄定时计算对应的执行计划不存在，可能已经被删除，请删除重建该库龄定时计算！", "004023000022651", 5, new object[0]), "", 0);
				return;
			}
			string billId = this.Model.DataObject["Id"].ToString();
			if (!this.CheckPermission(base.Context, base.View.BusinessInfo.GetForm().Id, billId, "5c529714be1817"))
			{
				base.View.ShowMessage(ResManager.LoadKDString("对不起，您没有库龄定时计算的修改执行计划权限!", "004023000022652", 5, new object[0]), 0);
				return;
			}
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = formId;
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			billShowParameter.PKey = dynamicObject["Id"].ToString();
			billShowParameter.AllowNavigation = false;
			billShowParameter.Status = 2;
			billShowParameter.OpenStyle.ShowType = 6;
			base.View.ShowForm(billShowParameter, delegate(FormResult result)
			{
				this.View.Model.Load(billId);
				this.View.Refresh();
			});
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00004B58 File Offset: 0x00002D58
		private bool CheckPermission(Context ctx, string sFormId, string billId, string sPerItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(ctx, new BusinessObject
			{
				Id = sFormId,
				pkId = billId
			}, sPerItemId);
			return permissionAuthResult.Passed;
		}
	}
}
