using System;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A8 RID: 168
	public class WarnSelfLifeAlarmMessagePlugIn : AbstractWarnMessagePlugIn
	{
		// Token: 0x06000A6B RID: 2667 RVA: 0x0008EFCE File Offset: 0x0008D1CE
		public override void ShowWarnMessage(ShowWarnMessageEventArgs e)
		{
			base.ShowWarnMessage(e);
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x0008EFD8 File Offset: 0x0008D1D8
		public override void ShowMergeMessage(ShowMergeMessageEventArgs e)
		{
			if (e.MsgDataKeyValueList != null && e.MsgDataKeyValueList.Count > 0)
			{
				e.IsShowByPlugIn = true;
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "STK_ShelfLiftSetInfo";
				base.ParentView.OpenParameter.SetCustomParameter("MessageList", e);
				base.ParentView.ShowForm(dynamicFormShowParameter);
			}
			base.ShowMergeMessage(e);
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x0008F03C File Offset: 0x0008D23C
		public override void ProcessWarnMessage(ProcessWarnMessageEventArgs e)
		{
			base.ProcessWarnMessage(e);
		}
	}
}
