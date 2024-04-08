using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000002 RID: 2
	[Description("受托加工入库单参数模板客户端插件")]
	public class OEMInStockUserParamEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override void AfterBindData(EventArgs e)
		{
			long num = Convert.ToInt64(this.View.Model.GetValue("FExpandType"));
			this.View.StyleManager.SetEnabled("FExpandLevel", "FExpandLevel", num == 3L);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002098 File Offset: 0x00000298
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FEXPANDTYPE"))
				{
					return;
				}
				this.View.StyleManager.SetEnabled("FExpandLevel", "FExpandLevel", e.NewValue.ToString() == "3");
			}
		}
	}
}
