using System;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.PurMrb
{
	// Token: 0x0200001F RID: 31
	public class StockTemplateEdit_PurMrb : StockBillTemplateEdit
	{
		// Token: 0x0600014A RID: 330 RVA: 0x000120D8 File Offset: 0x000102D8
		public override bool NeedCheckBillDate()
		{
			object value = this.Model.GetValue("FDate");
			if (base.OldDate != null && base.OldDate == Convert.ToDateTime(value))
			{
				return false;
			}
			bool result = false;
			string a = "";
			DynamicObject dataObject = this.Model.DataObject;
			object obj = dataObject["BusinessType"];
			if (obj != null)
			{
				a = obj.ToString();
			}
			bool flag = false;
			obj = dataObject["MRTYPE"];
			string a2 = "";
			if (obj != null)
			{
				a2 = obj.ToString();
			}
			DynamicObjectCollection dynamicObjectCollection = dataObject["PUR_MRBFIN"] as DynamicObjectCollection;
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				obj = dynamicObjectCollection[0]["ISGENFORIOS"];
				if (obj != null)
				{
					flag = Convert.ToBoolean(obj);
				}
			}
			if (flag || a2 != "B")
			{
				result = false;
			}
			else
			{
				dynamicObjectCollection = (dataObject["PUR_MRBENTRY"] as DynamicObjectCollection);
				if (dynamicObjectCollection != null)
				{
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						DynamicObject dynamicObject2 = dynamicObject["MATERIALID"] as DynamicObject;
						if (dynamicObject2 != null && dynamicObject2["MaterialBase"] != null)
						{
							DynamicObject dynamicObject3 = ((DynamicObjectCollection)dynamicObject2["MaterialBase"])[0];
							if (dynamicObject3 != null && dynamicObject3["IsInventory"] != null)
							{
								bool flag2 = Convert.ToBoolean(dynamicObject3["IsInventory"]);
								long num = 0L;
								long num2 = 0L;
								obj = (dynamicObject["StockStatusId"] as DynamicObject);
								if (obj != null)
								{
									num = Convert.ToInt64(((DynamicObject)obj)["Id"]);
								}
								obj = (dynamicObject["ReceiveStockStatusId"] as DynamicObject);
								if (obj != null)
								{
									num2 = Convert.ToInt64(((DynamicObject)obj)["Id"]);
								}
								bool flag3 = false;
								if (dynamicObject["ReceiveStockFlag"] != null && Convert.ToBoolean(dynamicObject["ReceiveStockFlag"]))
								{
									flag3 = true;
								}
								if (flag3 || (a != "ZCCG" && flag2 && num != num2 && !flag))
								{
									result = true;
									break;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
