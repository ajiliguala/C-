using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.Warn
{
	// Token: 0x020000A9 RID: 169
	public class WarnSelfLifeAlarmMessageShowPlugIn : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000A6F RID: 2671 RVA: 0x0008F050 File Offset: 0x0008D250
		public override void OnInitialize(InitializeEventArgs e)
		{
			if (!ObjectUtils.IsNullOrEmpty(this.View.ParentFormView.OpenParameter.GetCustomParameter("MessageList")))
			{
				this.messageList = (this.View.ParentFormView.OpenParameter.GetCustomParameter("MessageList") as ShowMergeMessageEventArgs);
			}
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x0008F0A4 File Offset: 0x0008D2A4
		public override void CreateNewData(BizDataEventArgs e)
		{
			DynamicObjectType dynamicObjectType = this.Model.BillBusinessInfo.GetDynamicObjectType();
			DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
			DynamicObjectCollection dynamicObjectCollection = dynamicObject["ShelfLiftSetEntity"] as DynamicObjectCollection;
			EntryEntity entryEntity = this.View.BillBusinessInfo.GetEntryEntity("FShelfLiftSetEntity");
			int num = 0;
			foreach (WarnMessageDataKeyValue warnMessageDataKeyValue in this.messageList.MsgDataKeyValueList)
			{
				DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
				dynamicObject2["Seq"] = num;
				dynamicObject2["MaterialId_Id"] = this.getItemPatValue("FMaterialId", warnMessageDataKeyValue.Items);
				dynamicObject2["StockId_Id"] = this.getItemPatValue("FStockId", warnMessageDataKeyValue.Items);
				dynamicObject2["StockUnitId_Id"] = this.getItemPatValue("FStockUnitId", warnMessageDataKeyValue.Items);
				dynamicObject2["Lot_Id"] = this.getItemPatValue("FLot", warnMessageDataKeyValue.Items);
				dynamicObject2["AuxPropId_Id"] = this.getItemPatValue("FAuxPropId", warnMessageDataKeyValue.Items);
				dynamicObject2["StockLocId_Id"] = this.getItemPatValue("FStockLocId", warnMessageDataKeyValue.Items);
				dynamicObject2["ProduceDate"] = Convert.ToDateTime(this.getItemPatValue("FPRODUCEDATE", warnMessageDataKeyValue.Items));
				dynamicObject2["ExpiryDate"] = Convert.ToDateTime(this.getItemPatValue("FExpiryDate", warnMessageDataKeyValue.Items));
				dynamicObject2["ExpiryDays"] = this.getItemPatValue("FExpiryDays", warnMessageDataKeyValue.Items);
				dynamicObject2["Qty"] = this.getItemPatValue("FStockQty", warnMessageDataKeyValue.Items);
				dynamicObjectCollection.Add(dynamicObject2);
				num++;
			}
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				DBServiceHelper.LoadReferenceObject(base.Context, dynamicObjectCollection.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			}
			e.BizDataObject = dynamicObject;
			base.CreateNewData(e);
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x0008F2EC File Offset: 0x0008D4EC
		private string getItemPatValue(string key, List<WarnMessageDataKeyValueItem> list)
		{
			foreach (WarnMessageDataKeyValueItem warnMessageDataKeyValueItem in list)
			{
				if (key.ToUpper() == warnMessageDataKeyValueItem.FieldName.ToUpper())
				{
					return warnMessageDataKeyValueItem.Value;
				}
			}
			return "";
		}

		// Token: 0x0400042B RID: 1067
		private ShowMergeMessageEventArgs messageList;
	}
}
