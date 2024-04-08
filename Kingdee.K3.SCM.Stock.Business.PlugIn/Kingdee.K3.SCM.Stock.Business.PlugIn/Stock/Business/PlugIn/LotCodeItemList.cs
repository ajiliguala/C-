using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000072 RID: 114
	public class LotCodeItemList : AbstractListPlugIn
	{
		// Token: 0x0600052B RID: 1323 RVA: 0x0003FA3C File Offset: 0x0003DC3C
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				a == "";
			}
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x0003FA64 File Offset: 0x0003DC64
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}
	}
}
