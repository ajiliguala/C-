using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200007E RID: 126
	[Description("安全库存预警客户端端插件")]
	public class SafetyStockAlarmMessagePlugln : AbstractWarnMessagePlugIn
	{
		// Token: 0x060005B4 RID: 1460 RVA: 0x00045EF8 File Offset: 0x000440F8
		public override void ShowWarnMessage(ShowWarnMessageEventArgs e)
		{
			base.ShowWarnMessage(e);
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00045F04 File Offset: 0x00044104
		public override void ProcessWarnMessage(ProcessWarnMessageEventArgs e)
		{
			if (e.MsgDataKeyValueList != null && e.MsgDataKeyValueList.Count<WarnMessageDataKeyValue>() > 0)
			{
				string text = EarlyWarningServiceHelper.ProcessWarnMessage(base.Context, e.MsgDataKeyValueList, "1");
				if (!string.IsNullOrWhiteSpace(text))
				{
					e.Result.IsShowMessage = false;
					e.IsProcessByPlugin = true;
					base.ParentView.ShowMessage(text, 0);
				}
				else
				{
					e.Result.IsSuccess = false;
				}
			}
			base.ProcessWarnMessage(e);
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00045F90 File Offset: 0x00044190
		public override void BeforeProcessWarnMessage(BeforeProcessWarnMessageEventArgs e)
		{
			base.BeforeProcessWarnMessage(e);
			List<FormOperation> list = (from x in base.WarnDataSourceBusinessInfo.GetForm().FormOperations
			where x.Operation == "Edit"
			select x).ToList<FormOperation>();
			if (list != null && list.Count > 0)
			{
				e.PermissionItemId = list.First<FormOperation>().PermissionItemId;
			}
		}
	}
}
