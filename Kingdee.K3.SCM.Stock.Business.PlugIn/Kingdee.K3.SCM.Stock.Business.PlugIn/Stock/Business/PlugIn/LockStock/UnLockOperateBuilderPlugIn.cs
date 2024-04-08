using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.LockStock
{
	// Token: 0x02000011 RID: 17
	[Description("解锁界面表单构件插件")]
	public class UnLockOperateBuilderPlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x06000072 RID: 114 RVA: 0x00006AC1 File Offset: 0x00004CC1
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (e.ControlAppearance.Key.Equals("FEntity"))
			{
				e.Control.Put("showFilterRow", true);
			}
		}
	}
}
