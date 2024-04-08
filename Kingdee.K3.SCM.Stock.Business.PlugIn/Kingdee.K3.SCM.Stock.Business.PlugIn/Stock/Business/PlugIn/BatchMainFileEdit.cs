using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000044 RID: 68
	public class BatchMainFileEdit : AbstractBillPlugIn
	{
		// Token: 0x060002A7 RID: 679 RVA: 0x00020F08 File Offset: 0x0001F108
		public override void OnInitialize(InitializeEventArgs e)
		{
			object customParameter = base.View.OpenParameter.GetCustomParameter("ShowMode");
			if (customParameter != null)
			{
				this.isQueryLotMaster = customParameter.Equals("QueryLotMaster");
			}
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x00020F40 File Offset: 0x0001F140
		public override void AfterBindData(EventArgs e)
		{
			if (this.isQueryLotMaster)
			{
				base.View.GetMainBarItem("tbSplitPrint").Visible = true;
				base.View.GetMainBarItem("tbPreView").Visible = true;
				base.View.GetMainBarItem("tbPrint").Visible = true;
				base.View.GetMainBarItem("tbAccessory").Visible = true;
				base.View.GetMainBarItem("tbClose").Visible = true;
				base.View.GetMainBarItem("tbSplitPrint").Enabled = true;
				base.View.GetMainBarItem("tbPreView").Enabled = true;
				base.View.GetMainBarItem("tbPrint").Enabled = true;
				base.View.GetMainBarItem("tbAccessory").Enabled = true;
				base.View.GetMainBarItem("tbClose").Enabled = true;
			}
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x00021048 File Offset: 0x0001F248
		public override void AfterLoadData(EventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FEntityTrace");
			DynamicObjectCollection value = entity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			if (value.Count > 0)
			{
				value.Sort<long>((DynamicObject p) => Convert.ToInt64(p["Id"]), null);
				for (int i = 0; i < value.Count<DynamicObject>(); i++)
				{
					value[i]["Seq"] = i + 1;
				}
			}
		}

		// Token: 0x060002AA RID: 682 RVA: 0x000210DC File Offset: 0x0001F2DC
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (!e.Entity.Key.Equals("FEntityExpiry") || e.Row > 0)
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetField("FProduceDate").Entity;
			DynamicObjectType dynamicObjectType = entity.DynamicObjectType;
			DynamicObjectCollection dynamicObjectCollection = entity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			if (dynamicObjectCollection == null)
			{
				dynamicObjectCollection = new DynamicObjectCollection(dynamicObjectType, null);
			}
			dynamicObjectCollection.Clear();
			long num = Convert.ToInt64(this.Model.DataObject[FormConst.MASTER_ID]);
			if (num < 1L)
			{
				return;
			}
			DynamicObject dynamicObject = this.Model.GetValue("FUseOrgId") as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) < 1L)
			{
				return;
			}
			DynamicObject dynamicObject2 = this.Model.GetValue("FMaterialId") as DynamicObject;
			if (dynamicObject2 == null)
			{
				return;
			}
			bool flag = Convert.ToBoolean(((DynamicObjectCollection)dynamicObject2["MaterialStock"])[0]["IsExpParToFlot"]);
			if (flag)
			{
				dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["ProduceDate"] = this.Model.GetValue("FHProduceDate");
				dynamicObject["ExpiryDate"] = this.Model.GetValue("FHExpiryDate");
				dynamicObject["Seq"] = "1";
				dynamicObjectCollection.Add(dynamicObject);
				return;
			}
			long num2 = Convert.ToInt64(dynamicObject["Id"]);
			DataTable lotExpiryInfo = StockServiceHelper.GetLotExpiryInfo(base.Context, num, num2);
			if (lotExpiryInfo == null || lotExpiryInfo.Rows == null || lotExpiryInfo.Rows.Count < 1)
			{
				return;
			}
			int num3 = 1;
			foreach (object obj in lotExpiryInfo.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				if (!(dataRow["FPRODUCEDATE"] is DBNull) && !(dataRow["FEXPIRYDATE"] is DBNull))
				{
					dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["ProduceDate"] = dataRow["FPRODUCEDATE"];
					dynamicObject["ExpiryDate"] = dataRow["FEXPIRYDATE"];
					dynamicObject["Seq"] = num3;
					dynamicObjectCollection.Add(dynamicObject);
					num3++;
				}
			}
		}

		// Token: 0x060002AB RID: 683 RVA: 0x00021364 File Offset: 0x0001F564
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_AttachmentMgr)
			{
				e.Option.SetVariableValue("ForceEnableAttachOperate", true);
			}
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0002139A File Offset: 0x0001F59A
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			base.View.GetControl<EntryGrid>("FEntityTrace").SetFireDoubleClickEvent(true);
			base.OnBillInitialize(e);
		}

		// Token: 0x060002AD RID: 685 RVA: 0x000213BC File Offset: 0x0001F5BC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.ToUpperInvariant().Equals("TBBIZBILL"))
			{
				int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FEntityTrace");
				this.ShowBill(entryCurrentRowIndex);
			}
		}

		// Token: 0x060002AE RID: 686 RVA: 0x000213FD File Offset: 0x0001F5FD
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			this.ShowBill(e.Row);
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0002140C File Offset: 0x0001F60C
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FBILLNO"))
				{
					return;
				}
				if (e.Row < 0)
				{
					return;
				}
				this.ShowBill(e.Row);
				e.Cancel = true;
			}
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x00021454 File Offset: 0x0001F654
		private void ShowBill(int rowIndex)
		{
			if (rowIndex < 0)
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FEntityTrace");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, rowIndex);
			if (entityDataObject == null)
			{
				return;
			}
			DynamicObject dynamicObject = entityDataObject["BillFormID"] as DynamicObject;
			string text = "";
			if (dynamicObject != null && dynamicObject["Id"] != null)
			{
				text = dynamicObject["Id"].ToString();
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			long num = Convert.ToInt64(entityDataObject["BillID"]);
			if (num < 1L)
			{
				return;
			}
			long num2 = 0L;
			if (StringUtils.EqualsIgnoreCase(text, "STK_TRANSFEROUT") || StringUtils.EqualsIgnoreCase(text, "STK_TRANSFERIN"))
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, text, true) as FormMetadata;
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FStockOrgID"));
				OQLFilter oqlfilter = new OQLFilter();
				oqlfilter.Add(new OQLFilterHeadEntityItem
				{
					EntityKey = "FBillHead",
					FilterString = string.Format(" FID = {0} ", num)
				});
				DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, formMetadata.BusinessInfo, list, oqlfilter);
				if (array != null && array.Length > 0)
				{
					num2 = Convert.ToInt64(array[0]["StockOrgID_Id"]);
				}
			}
			else if (StringUtils.EqualsIgnoreCase(text, "STK_TransferDirect"))
			{
				FormMetadata formMetadata2 = MetaDataServiceHelper.Load(base.View.Context, text, true) as FormMetadata;
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FStockOutOrgId"));
				OQLFilter oqlfilter2 = new OQLFilter();
				oqlfilter2.Add(new OQLFilterHeadEntityItem
				{
					EntityKey = "FBillHead",
					FilterString = string.Format(" FID = {0} ", num)
				});
				DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, formMetadata2.BusinessInfo, list2, oqlfilter2);
				if (array2 != null && array2.Length > 0)
				{
					num2 = Convert.ToInt64(array2[0]["StockOutOrgId_Id"]);
				}
			}
			else
			{
				dynamicObject = (this.Model.GetValue("FUseOrgId") as DynamicObject);
				num2 = Convert.ToInt64(dynamicObject["Id"]);
			}
			if (num2 < 1L)
			{
				return;
			}
			SCMCommon.ShowBizBillForm(this, text, num, num2, 0L);
		}

		// Token: 0x040000F7 RID: 247
		private bool isQueryLotMaster;
	}
}
