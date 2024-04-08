using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000021 RID: 33
	[Description("安全库存参数设置构建插件")]
	public class SaftyStockSetBuilder : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x0600014E RID: 334 RVA: 0x00012400 File Offset: 0x00010600
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.ControlAppearance.Key, "FPanelWebBrowse"))
			{
				e.Control["xtype"] = "kdwebbrowser";
			}
		}
	}
}
