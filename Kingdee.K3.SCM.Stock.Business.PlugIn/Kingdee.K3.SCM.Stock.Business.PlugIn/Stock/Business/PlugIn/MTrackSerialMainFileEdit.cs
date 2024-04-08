using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000028 RID: 40
	[Description("生产追溯序列号主档表单插件")]
	public class MTrackSerialMainFileEdit : AbstractBillPlugIn
	{
		// Token: 0x06000183 RID: 387 RVA: 0x00012D4C File Offset: 0x00010F4C
		private bool VaildatePermission(string billFormId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = base.View.Model.SubSytemId
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}
	}
}
