using System;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.PurReceive
{
	// Token: 0x02000020 RID: 32
	public class StockTemplateEdit_PurReceive : StockBillTemplateEdit
	{
		// Token: 0x0600014C RID: 332 RVA: 0x00012370 File Offset: 0x00010570
		public override bool NeedCheckBillDate()
		{
			bool result = false;
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["PUR_ReceiveEntry"] as DynamicObjectCollection;
			if (dynamicObjectCollection != null)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (dynamicObject["StockFlag"] != null && Convert.ToBoolean(dynamicObject["StockFlag"]))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}
}
