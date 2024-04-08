using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.LockStock
{
	// Token: 0x02000010 RID: 16
	[Description("锁库界面表单构件插件")]
	public class LockStockOperateBuilderPlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x06000070 RID: 112 RVA: 0x00006A68 File Offset: 0x00004C68
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (e.ControlAppearance.Key.Equals("FEntityLock") || e.ControlAppearance.Key.Equals("FSubEntiry"))
			{
				e.Control.Put("showFilterRow", true);
			}
		}
	}
}
