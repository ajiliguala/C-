using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockAlert
{
	// Token: 0x02000033 RID: 51
	[Description("仓库最大最小安全库存编辑插件")]
	public class StockAlertEdit : AbstractBillPlugIn
	{
		// Token: 0x06000218 RID: 536 RVA: 0x0001AB14 File Offset: 0x00018D14
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string text = "";
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FStockOrgID"))
				{
					if (!(fieldKey == "FStockId"))
					{
						return;
					}
					if (this.GetFieldFilter(e.FieldKey, out text))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = text;
							return;
						}
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
				}
				else if (this.GetFieldFilter(e.FieldKey, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0001ABF0 File Offset: 0x00018DF0
		private bool GetFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			if (fieldKey != null)
			{
				if (!(fieldKey == "FStockOrgID"))
				{
					if (fieldKey == "FStockId")
					{
						filter = "FAVAILABLEALERT =1";
					}
				}
				else
				{
					filter = string.Format("exists (select 1 from  T_SEC_USERORG tur where fuserid={0} and tur.FORGID=t0.FORGID)", base.Context.UserId.ToString());
				}
			}
			return true;
		}
	}
}
