using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockCount
{
	// Token: 0x02000059 RID: 89
	public class StockCountInputBuilderPlugIn : AbstractDynamicWebFormBuilderPlugIn
	{
		// Token: 0x060003F5 RID: 1013 RVA: 0x0002FCE4 File Offset: 0x0002DEE4
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (e.ControlAppearance.Key.Equals("FBillEntry"))
			{
				e.Control.Put("showFilterRow", true);
			}
		}
	}
}
