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
	// Token: 0x02000064 RID: 100
	[Description("最小库存预警客户端插件")]
	public class MinStockAlarmMessagePlugln : AbstractWarnMessagePlugIn
	{
		// Token: 0x06000456 RID: 1110 RVA: 0x00033D83 File Offset: 0x00031F83
		public override void ShowWarnMessage(ShowWarnMessageEventArgs e)
		{
			base.ShowWarnMessage(e);
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00033D8C File Offset: 0x00031F8C
		public override void ProcessWarnMessage(ProcessWarnMessageEventArgs e)
		{
			if (e.MsgDataKeyValueList != null && e.MsgDataKeyValueList.Count<WarnMessageDataKeyValue>() > 0)
			{
				string text = EarlyWarningServiceHelper.ProcessWarnMessage(base.Context, e.MsgDataKeyValueList, "3");
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

		// Token: 0x06000458 RID: 1112 RVA: 0x00033E18 File Offset: 0x00032018
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
