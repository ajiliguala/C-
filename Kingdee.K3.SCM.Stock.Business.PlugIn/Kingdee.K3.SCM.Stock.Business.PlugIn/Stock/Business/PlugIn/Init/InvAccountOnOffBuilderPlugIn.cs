using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Init
{
	// Token: 0x02000005 RID: 5
	[Description("关账界面构建插件")]
	public class InvAccountOnOffBuilderPlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x06000008 RID: 8 RVA: 0x0000218E File Offset: 0x0000038E
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (e.ControlAppearance.Key.Equals("FEntityAction"))
			{
				e.Control.Put("showFilterRow", true);
			}
		}
	}
}
