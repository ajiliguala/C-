using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.LockStock
{
	// Token: 0x0200000E RID: 14
	[Description("即时库存锁库界面表单构件插件")]
	public class LockOperateBuilderPlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00006304 File Offset: 0x00004504
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (e.ControlAppearance.Key.Equals("FEntity"))
			{
				e.Control.Put("showFilterRow", true);
			}
		}
	}
}
