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
	// Token: 0x0200007C RID: 124
	[Description("再订货点预警客户端插件")]
	public class ReorderPointAlarmMessagePlugln : AbstractWarnMessagePlugIn
	{
		// Token: 0x060005AD RID: 1453 RVA: 0x000459D8 File Offset: 0x00043BD8
		public override void ProcessWarnMessage(ProcessWarnMessageEventArgs e)
		{
			if (e.MsgDataKeyValueList != null && e.MsgDataKeyValueList.Count<WarnMessageDataKeyValue>() > 0)
			{
				string text = EarlyWarningServiceHelper.ProcessWarnMessage(base.Context, e.MsgDataKeyValueList, "2");
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

		// Token: 0x060005AE RID: 1454 RVA: 0x00045A64 File Offset: 0x00043C64
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
