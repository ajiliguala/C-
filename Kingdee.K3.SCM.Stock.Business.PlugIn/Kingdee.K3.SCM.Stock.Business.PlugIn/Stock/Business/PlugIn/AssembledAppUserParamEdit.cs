using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000043 RID: 67
	public class AssembledAppUserParamEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060002A4 RID: 676 RVA: 0x00020E34 File Offset: 0x0001F034
		public override void BeforeBindData(EventArgs e)
		{
			string a = Convert.ToString(this.View.Model.GetValue("FExpandType"));
			if (a == "3")
			{
				this.View.GetControl("FExpandLevel").Enabled = true;
				return;
			}
			this.View.GetControl("FExpandLevel").Enabled = false;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x00020E98 File Offset: 0x0001F098
		public override void DataChanged(DataChangedEventArgs e)
		{
			if (e.Field.Key.ToUpperInvariant() == "FEXPANDTYPE")
			{
				if (e.NewValue.Equals("3"))
				{
					this.View.GetControl("FExpandLevel").Enabled = true;
					return;
				}
				this.View.GetControl("FExpandLevel").Enabled = false;
			}
		}
	}
}
