using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A6 RID: 166
	public class StockTransferOutEdit : AbstractBillPlugIn
	{
		// Token: 0x06000A26 RID: 2598 RVA: 0x0008A5FC File Offset: 0x000887FC
		private void SetStockOrgDef()
		{
			OrganizationInfo currentOrganizationInfo = base.Context.CurrentOrganizationInfo;
			if (BDServiceHelper.IsInvInit(base.Context, currentOrganizationInfo.ID.ToString()))
			{
				base.View.Model.SetValue("FSTOCKORGID", currentOrganizationInfo.ID);
				return;
			}
			base.View.Model.SetValue("FSTOCKORGID", null);
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x0008A668 File Offset: 0x00088868
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBPUSH"))
				{
					if (a == "TBSYNCBASEDATA")
					{
						this.DoFillBaseData();
					}
				}
				else
				{
					object value = base.View.Model.GetValue("FISGENFORIOS");
					if (!ObjectUtils.IsNullOrEmpty(value))
					{
						bool flag = Convert.ToBoolean(value);
						if (flag)
						{
							e.Cancel = true;
							base.View.ShowErrMessage(ResManager.LoadKDString("内部交易单据不允许下推！", "004023000010995", 5, new object[0]), "", 0);
							return;
						}
					}
				}
			}
			base.BarItemClick(e);
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x0008A704 File Offset: 0x00088904
		public override void DataChanged(DataChangedEventArgs e)
		{
			string text = "";
			long num = 0L;
			long num2 = 0L;
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FMATERIALID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					text = dynamicObject["Number"].ToString();
				}
				this.Model.SetItemValueByNumber("FDestMaterialID", text, e.Row);
				base.View.InvokeFieldUpdateService("FDestMaterialID", e.Row);
				base.View.UpdateView("FDestMaterialID", e.Row);
				long num4 = 0L;
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject2 != null)
				{
					num4 = Convert.ToInt64(dynamicObject2["Id"]);
				}
				this.ChangeFlag = true;
				dynamicObject = (base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject);
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject, 0L, false, num4, false), e.Row);
				this.ChangeFlag = false;
				this.SynOwnerType("FOwnerTypeIdHead", "FOwnerTypeId", e.Row);
				this.SynOwnerType("FOwnerTypeInIdHead", "FOwnerTypeInId", e.Row);
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
				if (dynamicObject3 != null)
				{
					base.View.Model.SetValue("FOwnerID", Convert.ToInt64(dynamicObject3["Id"]), e.Row);
				}
				dynamicObject3 = (base.View.Model.GetValue(" FOwnerInIdHead") as DynamicObject);
				if (dynamicObject3 != null)
				{
					base.View.Model.SetValue(" FOwnerInID", Convert.ToInt64(dynamicObject3["Id"]), e.Row);
					return;
				}
				break;
			}
			case "FSTOCKINORGID":
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FSTKTRSOUTENTRY");
				string value = BillUtils.GetValue<string>(this.Model, "FOwnerTypeInIdHead", -1, "", null);
				if (e.NewValue != null)
				{
					num = Convert.ToInt64(e.NewValue);
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerInIdHead", -1) as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				if (value.Equals("BD_OwnerOrg") && num != num2)
				{
					this.Model.SetValue("FOwnerInIdHead", num, -1);
					base.View.InvokeFieldUpdateService("FOwnerInIdHead", -1);
				}
				else if (!value.Equals("BD_OwnerOrg"))
				{
					this.Model.SetValue("FOwnerInIdHead", 0, -1);
					base.View.InvokeFieldUpdateService("FOwnerInIdHead", -1);
					for (int i = 0; i < entryRowCount; i++)
					{
						this.Model.SetValue("FOwnerInID", 0, i);
					}
					this.SyncHeadOwnerToHeadInOwner();
				}
				for (int j = 0; j < entryRowCount; j++)
				{
					text = "";
					if (e.NewValue == null)
					{
						this.Model.SetValue("FDestMaterialID", null, j);
						this.Model.SetValue("FDESTBOMID", null, j);
						this.Model.SetValue("FDESTLOT", null, j);
						this.Model.SetValue("FDestProduceDate", null, j);
						this.Model.SetValue("FDESTEXPIRYDATE", null, j);
						this.Model.SetValue("FDestMTONO", "", j);
					}
					else
					{
						dynamicObject = (base.View.Model.GetValue("FMATERIALID", j) as DynamicObject);
						if (dynamicObject != null)
						{
							text = dynamicObject["Number"].ToString();
						}
						this.Model.SetItemValueByNumber("FDestMaterialID", text, j);
						base.View.InvokeFieldUpdateService("FDestMaterialID", j);
						this.SyncEntryMaterialRelateField(j);
						this.UpdateDestBom(j);
					}
				}
				return;
			}
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FSTOCKERGROUPID", "WHY");
				return;
			case "FBIZTYPE":
				this.SetComValue();
				this.SyncHeadOwnerToHeadInOwner();
				return;
			case "FOWNERTYPEIDHEAD":
				Common.SynOwnerType(this, "FOWNERTYPEIDHEAD", "FOWNERTYPEID");
				this.SyncHeadOwnerToHeadInOwner();
				return;
			case "FOWNERTYPEINIDHEAD":
			{
				Common.SynOwnerType(this, "FOWNERTYPEINIDHEAD", "FOWNERTYPEINID");
				if (e.NewValue == null || string.IsNullOrWhiteSpace(e.NewValue.ToString()) || !e.NewValue.ToString().Equals("BD_OwnerOrg") || (e.OldValue != null && !string.IsNullOrWhiteSpace(e.OldValue.ToString()) && e.NewValue.ToString().ToUpperInvariant().Equals(e.OldValue.ToString().ToUpperInvariant())))
				{
					this.Model.SetValue("FOwnerInIdHead", 0, -1);
					base.View.InvokeFieldUpdateService("FOwnerInIdHead", -1);
					return;
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockInOrgID", -1) as DynamicObject;
				if (dynamicObject != null)
				{
					num = Convert.ToInt64(dynamicObject["Id"]);
				}
				dynamicObject = (base.View.Model.GetValue("FOwnerInIdHead", -1) as DynamicObject);
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				if (num != num2)
				{
					this.Model.SetValue("FOwnerInIdHead", num, -1);
					base.View.InvokeFieldUpdateService("FOwnerInIdHead", -1);
					return;
				}
				break;
			}
			case "FOWNERINIDHEAD":
				this.SetCustAndSupplierValue();
				this.SynHeadToEntry("FOWNERINIDHEAD", "FOWNERINID");
				return;
			case "FOWNERIDHEAD":
				this.SetDefLocalCurrencyAndExchangeType();
				this.SetCustAndSupplierValue();
				this.SyncHeadOwnerToHeadInOwner();
				this.SynHeadToEntry("FOWNERIDHEAD", "FOWNERID");
				return;
			case "FTRANSFERDIRECT":
				this.SetCustAndSupplierValue();
				return;
			case "FOWNERID":
			{
				string text2 = base.View.Model.GetValue("FTransferBizType").ToString();
				string text3 = base.View.Model.GetValue("FOwnerTypeIdHead").ToString();
				string value2 = base.View.Model.GetValue("FOwnerTypeInIdHead").ToString();
				if (StringUtils.EqualsIgnoreCase(text2, "OverOrgTransfer") && text3.Equals(value2) && (StringUtils.EqualsIgnoreCase(text3, "BD_Supplier") || StringUtils.EqualsIgnoreCase(text3, "BD_Customer")))
				{
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FOwnerId", e.Row) as DynamicObject;
					if (dynamicObject4 == null || Convert.ToInt64(dynamicObject4["Id"]) == 0L)
					{
						base.View.Model.SetValue("FOwnerInID", 0, e.Row);
						return;
					}
					this.Model.SetItemValueByNumber("FOwnerInID", Convert.ToString(dynamicObject4["Number"]), e.Row);
					return;
				}
				break;
			}
			case "FSRCSTOCKID":
			{
				string a = base.View.Model.GetValue("FVESTONWAY").ToString();
				DynamicObject dynamicObject = base.View.Model.GetValue("FSRCSTOCKID", e.Row) as DynamicObject;
				if (a == "A")
				{
					SCMCommon.TakeDefaultStockStatus(this, "FDestStockStatusID", dynamicObject, e.Row, "4");
				}
				SCMCommon.TakeDefaultStockStatus(this, "FSrcStockStatusID", dynamicObject, e.Row, "'0','8'");
				return;
			}
			case "FDESTSTOCKID":
			{
				string a2 = base.View.Model.GetValue("FVESTONWAY").ToString();
				DynamicObject dynamicObject = base.View.Model.GetValue("FDESTSTOCKID", e.Row) as DynamicObject;
				if (a2 == "B")
				{
					SCMCommon.TakeDefaultStockStatus(this, "FDestStockStatusID", dynamicObject, e.Row, "4");
					return;
				}
				break;
			}
			case "FBOMID":
				this.UpdateDestBom(e.Row);
				return;
			case "FLOT":
				this.SyncEntryMaterialRelateField(e.Row);
				return;
			case "FDESTMATERIALID":
				if (!this.isDoFillMaterial)
				{
					long num5 = 0L;
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockInOrgID") as DynamicObject;
					if (dynamicObject5 != null)
					{
						num5 = Convert.ToInt64(dynamicObject5["Id"]);
					}
					DynamicObject dynamicObject = base.View.Model.GetValue("FDESTMATERIALID", e.Row) as DynamicObject;
					base.View.Model.SetValue("FDESTBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject, 0L, false, num5, false), e.Row);
					base.View.InvokeFieldUpdateService("FDESTBOMID", e.Row);
					return;
				}
				break;
			case "FPRODUCEDATE":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FDESTMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					DynamicObject dynamicObject6 = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
					if (dynamicObject6 != null && Convert.ToBoolean(dynamicObject6["IsKFPeriod"]))
					{
						object value3 = this.Model.GetValue(e.Field.Key, e.Row);
						this.Model.SetValue("FDestProduceDate", value3, e.Row);
					}
					else
					{
						this.Model.SetValue("FDestProduceDate", null, e.Row);
					}
				}
				else
				{
					this.Model.SetValue("FDestProduceDate", null, e.Row);
				}
				base.View.InvokeFieldUpdateService("FDestProduceDate", e.Row);
				return;
			}
			case "FEXPIRYDATE":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FDESTMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					DynamicObject dynamicObject7 = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
					if (dynamicObject7 != null && Convert.ToBoolean(dynamicObject7["IsKFPeriod"]))
					{
						object value4 = this.Model.GetValue(e.Field.Key, e.Row);
						this.Model.SetValue("FDESTEXPIRYDATE", value4, e.Row);
					}
					else
					{
						this.Model.SetValue("FDESTEXPIRYDATE", null, e.Row);
					}
				}
				else
				{
					this.Model.SetValue("FDESTEXPIRYDATE", null, e.Row);
				}
				base.View.InvokeFieldUpdateService("FDESTEXPIRYDATE", e.Row);
				return;
			}
			case "FMTONO":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FDESTMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					DynamicObject dynamicObject8 = ((DynamicObjectCollection)dynamicObject["MaterialPlan"])[0];
					if (dynamicObject8 != null && dynamicObject8["PlanMode"].ToString() != "0")
					{
						object value5 = this.Model.GetValue("FMTONO", e.Row);
						this.Model.SetValue("FDestMTONO", value5, e.Row);
					}
					else
					{
						this.Model.SetValue("FDestMTONO", "", e.Row);
					}
				}
				else
				{
					this.Model.SetValue("FDestMTONO", "", e.Row);
				}
				base.View.InvokeFieldUpdateService("FDestMTONO", e.Row);
				return;
			}
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				break;
			}

				return;
			}
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x0008B3CB File Offset: 0x000895CB
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			if (!this.ClearZeroRow())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x0008B3E4 File Offset: 0x000895E4
		private bool ClearZeroRow()
		{
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && Convert.ToBoolean(parameterData["IsClearZeroRow"]))
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
				int num = dynamicObjectCollection.Count - 1;
				for (int i = num; i >= 0; i--)
				{
					if (dynamicObjectCollection[i]["MaterialId"] != null && Convert.ToDecimal(dynamicObjectCollection[i]["FQty"]) == 0m)
					{
						this.Model.DeleteEntryRow("FSTKTRSOUTENTRY", i);
					}
				}
				if (this.Model.GetEntryRowCount("FSTKTRSOUTENTRY") == 0)
				{
					base.View.ShowErrMessage("", ResManager.LoadKDString("分录“明细”是必填项。", "004023000021872", 5, new object[0]), 0);
					return false;
				}
				base.View.UpdateView("FSTKTRSOUTENTRY");
			}
			return true;
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x0008B4DC File Offset: 0x000896DC
		private void SyncHeadOwnerToHeadInOwner()
		{
			string a = this.Model.GetValue("FOwnerTypeIdHead") as string;
			string a2 = this.Model.GetValue("FBizType") as string;
			if ((a != "BD_Supplier" && a != "BD_Customer") || a2 == "VMI")
			{
				return;
			}
			DynamicObject dynamicObject = this.Model.GetValue("FOwnerIdHead") as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) == 0L)
			{
				this.Model.SetValue("FOwnerInIdHead", 0);
				base.View.InvokeFieldUpdateService("FOwnerInIdHead", -1);
				return;
			}
			this.Model.SetItemValueByNumber("FOwnerInIdHead", Convert.ToString(dynamicObject["Number"]), -1);
			base.View.InvokeFieldUpdateService("FOwnerInIdHead", -1);
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x0008B5C4 File Offset: 0x000897C4
		private void SynOwnerType(string sourfield, string destfield, int row)
		{
			string text = base.View.Model.GetValue(sourfield) as string;
			base.View.BusinessInfo.GetField(destfield);
			base.View.Model.SetValue(destfield, text, row);
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x0008B610 File Offset: 0x00089810
		private void SynHeadToEntry(string sourfield, string destfield)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue(sourfield) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			Field field = base.View.BusinessInfo.GetField(destfield);
			int num2 = base.View.Model.GetEntryRowCount(field.EntityKey) - 1;
			if (this.CheckOwnerType(sourfield))
			{
				for (int i = 0; i <= num2; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue(destfield, num, i);
					}
				}
				return;
			}
			for (int j = 0; j <= num2; j++)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", j) as DynamicObject;
				DynamicObject dynamicObject3 = base.View.Model.GetValue(destfield, j) as DynamicObject;
				long num3 = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
				if (num3 == 0L && dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
				{
					base.View.Model.SetValue(destfield, num, j);
				}
			}
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x0008B784 File Offset: 0x00089984
		private void DoFillBaseData()
		{
			long num = 0L;
			long num2 = 0L;
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			dynamicObject = (this.Model.GetValue("FStockInOrgId") as DynamicObject);
			if (dynamicObject != null)
			{
				num2 = Convert.ToInt64(dynamicObject["Id"]);
			}
			if (num == 0L || num2 == 0L || num == num2)
			{
				return;
			}
			int num3 = 0;
			this.isDoFillMaterial = true;
			List<int> list = Common.FillTransBaseMapData(base.View, num, num2, "FMaterialId", "FDestMaterialID", "FNumber", "Number");
			this.isDoFillMaterial = false;
			if (list != null && list.Count > 0)
			{
				num3 = list.Count;
				foreach (int index in list)
				{
					this.SyncEntryMaterialRelateField(index);
				}
			}
			int num4 = 0;
			list = Common.FillTransBaseMapData(base.View, num, num2, "FBomId", "FDESTBOMID", "FNumber", "Number");
			if (list != null && list.Count > 0)
			{
				num4 = list.Count;
			}
			string text = string.Format(ResManager.LoadKDString("已填充【{0}】条物料内码，【{1}】条BOM内码。", "004023000039701", 5, new object[0]), num3, num4);
			base.View.ShowMessage(text, 0);
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x0008B900 File Offset: 0x00089B00
		private void SyncEntryMaterialRelateField(int index)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FDestMaterialId", index) as DynamicObject;
			DynamicObject dynamicObject2 = null;
			if (dynamicObject != null)
			{
				dynamicObject2 = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
			}
			if (dynamicObject2 != null && Convert.ToBoolean(dynamicObject2["IsBatchManage"]))
			{
				object value = this.Model.GetValue("FLot", index);
				this.Model.SetValue("FDestLot", value, index);
			}
			else
			{
				this.Model.SetValue("FDestLot", null, index);
			}
			base.View.InvokeFieldUpdateService("FDestLot", index);
			if (dynamicObject2 != null && Convert.ToBoolean(dynamicObject2["IsKFPeriod"]))
			{
				object value2 = this.Model.GetValue("FProduceDate", index);
				this.Model.SetValue("FDestProduceDate", value2, index);
				value2 = this.Model.GetValue("FEXPIRYDATE", index);
				this.Model.SetValue("FDESTEXPIRYDATE", value2, index);
			}
			else
			{
				this.Model.SetValue("FDestProduceDate", null, index);
				this.Model.SetValue("FDESTEXPIRYDATE", null, index);
			}
			dynamicObject2 = null;
			if (dynamicObject != null)
			{
				dynamicObject2 = ((DynamicObjectCollection)dynamicObject["MaterialPlan"])[0];
			}
			if (dynamicObject2 != null && dynamicObject2["PlanMode"].ToString() != "0")
			{
				object value3 = this.Model.GetValue("FMTONO", index);
				this.Model.SetValue("FDestMTONO", value3, index);
				return;
			}
			this.Model.SetValue("FDestMTONO", "", index);
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0008BAA0 File Offset: 0x00089CA0
		private bool CheckOwnerType(string sourfield)
		{
			string text = string.Empty;
			string a;
			if ((a = sourfield.ToUpperInvariant()) != null)
			{
				if (!(a == "FOWNERINIDHEAD"))
				{
					if (a == "FOWNERIDHEAD")
					{
						text = "FOwnerTypeIdHead";
					}
				}
				else
				{
					text = "FOwnerTypeInIdHead";
				}
			}
			return !ObjectUtils.IsNullOrEmpty(text) && base.View.Model.GetValue(text).ToString() == "BD_OwnerOrg";
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x0008BB14 File Offset: 0x00089D14
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				if (base.View.OpenParameter.CreateFrom != 1)
				{
					this.SetBusinessTypeByBillType();
					long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
					if (baseDataLongValue > 0L)
					{
						SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
					}
				}
				this.SetDefLocalCurrencyAndExchangeType();
			}
		}

		// Token: 0x06000A32 RID: 2610 RVA: 0x0008BB78 File Offset: 0x00089D78
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FOWNERIDHEAD":
			case "FOWNERID":
			case "FSTOCKINORGID":
			case "FOWNERINIDHEAD":
			case "FOWNERINID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FEXTAUXUNITID":
			case "FSRCSTOCKSTATUSID":
			{
				string lotF8InvFilter;
				if (this.GetFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrEmpty(e.ListFilterParameter.Filter) ? lotF8InvFilter : (e.ListFilterParameter.Filter + "AND" + lotF8InvFilter));
					return;
				}
				break;
			}
			case "FLOT":
			{
				string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
				{
					MaterialFieldKey = "FMaterialID",
					StockOrgFieldKey = "FStockOrgID",
					OwnerTypeFieldKey = "FOwnerTypeID",
					OwnerFieldKey = "FOwnerID",
					KeeperTypeFieldKey = "FKeeperTypeID",
					KeeperFieldKey = "FKeeperID",
					AuxpropFieldKey = "FAuxPropId",
					BomFieldKey = "FBOMID",
					StockFieldKey = "FSrcStockID",
					StockLocFieldKey = "FSrcStockLocId",
					StockStatusFieldKey = "FSrcStockStatusID",
					MtoFieldKey = "FMTONO",
					ProjectFieldKey = "FProjectNo"
				}, e.Row);
				if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + lotF8InvFilter;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x0008BDB4 File Offset: 0x00089FB4
		public override void AfterBindData(EventArgs e)
		{
			this.SetComValue();
			this.SetCustAndSupplierValue();
			bool flag = Convert.ToBoolean(base.View.Model.GetValue("FIsGenForIOS"));
			SCMCommon.SetSuiteProductStatusByPara(this, "FStockOrgId", flag);
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				string text = Convert.ToString(base.View.Model.GetValue("FSrcBillTypeId", 0));
				if (StringUtils.EqualsIgnoreCase(text, "STK_TRANSFERAPPLY"))
				{
					this.SyncHeadOwnerToHeadInOwner();
				}
			}
			this.LockFieldThirdBill();
		}

		// Token: 0x06000A34 RID: 2612 RVA: 0x0008BE61 File Offset: 0x0008A061
		public override void AfterSave(AfterSaveEventArgs e)
		{
			this.LockFieldThirdBill();
		}

		// Token: 0x06000A35 RID: 2613 RVA: 0x0008BE6C File Offset: 0x0008A06C
		private void LockFieldThirdBill()
		{
			string value = Convert.ToString(base.View.Model.GetValue("FThirdSrcBillNo")).Trim();
			if (!string.IsNullOrEmpty(value))
			{
				this.LockFieldHead();
				int entryRowCount = base.View.Model.GetEntryRowCount("FSTKTRSOUTENTRY");
				for (int i = 0; i < entryRowCount; i++)
				{
					this.LockField(i);
				}
			}
		}

		// Token: 0x06000A36 RID: 2614 RVA: 0x0008BED0 File Offset: 0x0008A0D0
		private void LockFieldHead()
		{
			BillUtils.LockField(base.View, "FBillNo", false, -1);
			BillUtils.LockField(base.View, "FStockOrgID", false, -1);
			BillUtils.LockField(base.View, "FStockInOrgID", false, -1);
			BillUtils.LockField(base.View, "FOwnerTypeIdHead", false, -1);
			BillUtils.LockField(base.View, "FOwnerIdHead", false, -1);
			BillUtils.LockField(base.View, "FOwnerTypeInIdHead", false, -1);
			BillUtils.LockField(base.View, "FOwnerInIdHead", false, -1);
			BillUtils.LockField(base.View, "FVESTONWAY", false, -1);
			BillUtils.LockField(base.View, "FTransferDirect", false, -1);
			BillUtils.LockField(base.View, "FTransferBizType", false, -1);
			BillUtils.LockField(base.View, "FDate", false, -1);
		}

		// Token: 0x06000A37 RID: 2615 RVA: 0x0008BFA4 File Offset: 0x0008A1A4
		private void LockField(int row)
		{
			base.View.GetFieldEditor("FMaterialID", row).Enabled = false;
			base.View.GetFieldEditor("FAuxPropId", row).Enabled = false;
			base.View.GetFieldEditor("FSrcStockID", row).Enabled = false;
			base.View.GetFieldEditor("FDestStockID", row).Enabled = false;
			base.View.GetFieldEditor("FUnitID", row).Enabled = false;
			base.View.GetFieldEditor("FQty", row).Enabled = false;
			base.View.GetFieldEditor("FLOT", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitId", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitQty", row).Enabled = false;
			base.View.GetFieldEditor("FProduceDate", row).Enabled = false;
			base.View.GetFieldEditor("FEXPIRYDATE", row).Enabled = false;
			base.View.GetFieldEditor("FSrcStockStatusID", row).Enabled = false;
			base.View.GetFieldEditor("FDestStockStatusID", row).Enabled = false;
			base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = false;
			base.View.GetFieldEditor("FKeeperTypeInID", row).Enabled = false;
			base.View.GetFieldEditor("FBusinessDate", row).Enabled = false;
		}

		// Token: 0x06000A38 RID: 2616 RVA: 0x0008C124 File Offset: 0x0008A324
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FOWNERIDHEAD":
			case "FOWNERID":
			case "FSTOCKINORGID":
			case "FOWNERINIDHEAD":
			case "FOWNERINID":
			case "FEXTAUXUNITID":
			case "FSRCSTOCKSTATUSID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					e.Filter = (string.IsNullOrEmpty(e.Filter) ? text : (e.Filter + "AND" + text));
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000A39 RID: 2617 RVA: 0x0008C244 File Offset: 0x0008A444
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			if (!base.View.Context.IsMultiOrg && e.Bills != null && e.Bills is List<ConvertBillElement>)
			{
				e.Bills = (from p in (List<ConvertBillElement>)e.Bills
				where !p.FormID.Equals("PLN_REQUIREMENTORDER")
				select p).ToList<ConvertBillElement>();
			}
			if (e.ConvertOperation == 13 && e.Bills is List<ConvertBillElement>)
			{
				long value = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
				if (value > 0L && Common.HaveBOMViewPermission(base.Context, value))
				{
					Common.SetBomExpandBillToConvertForm(base.Context, (List<ConvertBillElement>)e.Bills);
				}
			}
		}

		// Token: 0x06000A3A RID: 2618 RVA: 0x0008C30C File Offset: 0x0008A50C
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000A3B RID: 2619 RVA: 0x0008C370 File Offset: 0x0008A570
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x06000A3C RID: 2620 RVA: 0x0008C3A8 File Offset: 0x0008A5A8
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13 && e.SourceFormId == "ENG_PRODUCTSTRUCTURE")
			{
				List<string> list = new List<string>();
				SelBomBillParam bomExpandBillFieldValue = Common.GetBomExpandBillFieldValue(base.View, "FStockOrgId", "FOwnerTypeIdHead", "FOwnerIdHead");
				if (Common.ValidateBomExpandBillFieldValue(base.View, bomExpandBillFieldValue, list))
				{
					base.View.Session["SelInStockBillParam"] = bomExpandBillFieldValue;
					Common.SetBomExpandConvertRuleinfo(base.Context, base.View, e);
					return;
				}
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("【{0}】 字段为选单必录项！", "004023030004312", 5, new object[0]), string.Join(ResManager.LoadKDString("】,【", "004023030004315", 5, new object[0]), list)), "", 0);
			}
		}

		// Token: 0x06000A3D RID: 2621 RVA: 0x0008C4F0 File Offset: 0x0008A6F0
		public override void OnChangeConvertRuleEnumList(ChangeConvertRuleEnumListEventArgs e)
		{
			base.OnChangeConvertRuleEnumList(e);
			ConvertRuleElement convertRuleElement = e.Convertrules.FirstOrDefault<ConvertRuleElement>();
			if (convertRuleElement != null && convertRuleElement.SourceFormId.Equals("STK_TRANSFERAPPLY") && convertRuleElement.TargetFormId.Equals("STK_TRANSFEROUT"))
			{
				string text = Convert.ToString(base.View.Model.GetValue("FTransferDirect"));
				string a;
				if ((a = text) != null)
				{
					if (a == "GENERAL")
					{
						e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("STK_TRANSFERAPP_R-STK_TRANSFEROUT_R") || p.EnumId.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT_R"));
						e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("STK_TRANSFERAPP_R-STK_TRANSFEROUT_R") || p.Id.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT_R"));
						return;
					}
					if (!(a == "RETURN"))
					{
						return;
					}
					e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT"));
					e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT"));
				}
			}
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x0008C624 File Offset: 0x0008A824
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				Common.DoBomExpandDraw(base.View, Common.GetBomExpandBillFieldValue(base.View, "FStockOrgId", "", ""));
				base.View.UpdateView("FSTKTRSOUTENTRY");
				base.View.Model.DataChanged = true;
			}
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x0008C6C4 File Offset: 0x0008A8C4
		private bool GetBOMFilter(int row, out string filter)
		{
			filter = "";
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			if (dynamicObject == null)
			{
				return false;
			}
			long num = Convert.ToInt64(dynamicObject["msterID"]);
			long value = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			List<long> approvedBomIdByOrgId = MFGServiceHelperForSCM.GetApprovedBomIdByOrgId(base.View.Context, num, value, num2);
			if (!ListUtils.IsEmpty<long>(approvedBomIdByOrgId))
			{
				filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", approvedBomIdByOrgId));
			}
			else
			{
				filter = string.Format(" FID={0}", 0);
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000A40 RID: 2624 RVA: 0x0008C7B8 File Offset: 0x0008A9B8
		private void AuxpropDataChanged(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num == this.lastAuxpropId)
			{
				return;
			}
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMId", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, num, false, value2, false);
			if (bomDefaultValueByMaterial != value)
			{
				base.View.Model.SetValue("FBOMId", bomDefaultValueByMaterial, row);
			}
			this.lastAuxpropId = num;
			base.View.UpdateView("FSTKTRSOUTENTRY", row);
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x0008C8A4 File Offset: 0x0008AAA4
		private void AuxpropDataChanged(DynamicObject newAuxpropData, int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMId", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterialExceptApi = SCMCommon.GetBomDefaultValueByMaterialExceptApi(base.View, dynamicObject, newAuxpropData, false, value2, value, false);
			if (bomDefaultValueByMaterialExceptApi != value)
			{
				base.View.Model.SetValue("FBOMId", bomDefaultValueByMaterialExceptApi, row);
			}
		}

		// Token: 0x06000A42 RID: 2626 RVA: 0x0008C934 File Offset: 0x0008AB34
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_TransOutBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x06000A43 RID: 2627 RVA: 0x0008C984 File Offset: 0x0008AB84
		private void SetDefLocalCurrencyAndExchangeType()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrId", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x0008CA90 File Offset: 0x0008AC90
		private void SetComValue()
		{
			ComboFieldEditor comboFieldEditor = base.View.GetFieldEditor("FTransferBizType", 0) as ComboFieldEditor;
			if (comboFieldEditor == null)
			{
				return;
			}
			ComboField comboField = this.Model.BusinessInfo.GetElement("FTransferBizType") as ComboField;
			List<EnumItem> list = new List<EnumItem>();
			list = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
			(from p in list
			where p.Value == "InnerOrgTransfer"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item = (from p in list
			where p.Value == "OverOrgTransfer"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item2 = (from p in list
			where p.Value == "OverOrgSale"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item3 = (from p in list
			where p.Value == "OverOrgPurchase"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item4 = (from p in list
			where p.Value == "OverOrgPick"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item5 = (from p in list
			where p.Value == "OverOrgSubPick"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item6 = (from p in list
			where p.Value == "OverOrgMisDelivery"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item7 = (from p in list
			where p.Value == "OverOrgPrdIn"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item8 = (from p in list
			where p.Value == "OverOrgPrdOut"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item9 = (from p in list
			where p.Value == "OverOrgPurVMI"
			select p).FirstOrDefault<EnumItem>();
			if (!base.Context.IsMultiOrg)
			{
				string value = this.Model.GetValue("FBizType", 0) as string;
				if (!"VMI".Equals(value))
				{
					list.Remove(item);
				}
				list.Remove(item2);
				list.Remove(item3);
				list.Remove(item4);
				list.Remove(item5);
				list.Remove(item6);
				list.Remove(item7);
				list.Remove(item8);
				list.Remove(item9);
				comboFieldEditor.SetComboItems(list);
				return;
			}
			DynamicObjectCollection source = base.View.Model.DataObject["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
			if ((from p in source
			where !string.IsNullOrWhiteSpace(Convert.ToString(p["SrcBillNo"]))
			select p).Count<DynamicObject>() == 0)
			{
				list.Remove(item2);
				list.Remove(item3);
				list.Remove(item4);
				list.Remove(item5);
				list.Remove(item6);
				list.Remove(item7);
				list.Remove(item8);
				list.Remove(item9);
			}
			comboFieldEditor.SetComboItems(list);
		}

		// Token: 0x06000A45 RID: 2629 RVA: 0x0008CDC8 File Offset: 0x0008AFC8
		private bool GetFieldFilter(string fieldKey, out string filter, int row = -1)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FOWNERIDHEAD":
			case "FOWNERID":
			{
				string a = Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead"));
				string a2 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeId", row));
				if ((fieldKey.ToUpperInvariant().Equals("FOWNERIDHEAD") && a == "BD_OwnerOrg") || (fieldKey.ToUpperInvariant().Equals("FOWNERID") && a2 == "BD_OwnerOrg"))
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgID") as DynamicObject;
					long num2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
					bool bizRelation = CommonServiceHelper.GetBizRelation(base.Context, 112L, -1L);
					if (bizRelation)
					{
						filter = string.Format("  EXISTS (SELECT 1 FROM  t_org_bizrelation a inner join t_org_bizrelationEntry b on a.FBIZRELATIONID=b.FBIZRELATIONID\r\n                                    where a.FBRTYPEID={0} AND b.FRELATIONORGID={1} AND b.FORGID=t0.FORGID) OR t0.FORGID={1})", 112, num2);
					}
				}
				else if ((fieldKey.ToUpperInvariant().Equals("FOWNERIDHEAD") && a == "BD_Supplier") || (fieldKey.ToUpperInvariant().Equals("FOWNERID") && a2 == "BD_Supplier"))
				{
					string text = base.View.Model.GetValue("FBizType") as string;
					if (!string.IsNullOrWhiteSpace(text) && text == "VMI")
					{
						filter = string.Format(" FVmiBusiness = '1' ", new object[0]);
					}
				}
				break;
			}
			case "FOWNERINIDHEAD":
			case "FOWNERINID":
			{
				string a3 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeInIdHead"));
				string a4 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeInId", row));
				if ((fieldKey.ToUpperInvariant().Equals("FOWNERINIDHEAD") && a3 == "BD_OwnerOrg") || (fieldKey.ToUpperInvariant().Equals("FOWNERINID") && a4 == "BD_OwnerOrg"))
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockInOrgID") as DynamicObject;
					long num3 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
					bool bizRelation2 = CommonServiceHelper.GetBizRelation(base.Context, 112L, -1L);
					if (bizRelation2)
					{
						filter = string.Format("  EXISTS (SELECT 1 FROM  t_org_bizrelation a inner join t_org_bizrelationEntry b on a.FBIZRELATIONID=b.FBIZRELATIONID\r\n                                    where a.FBRTYPEID={0} AND b.FRELATIONORGID={1} AND b.FORGID=t0.FORGID) OR t0.FORGID={1})", 112, num3);
					}
				}
				else if ((fieldKey.ToUpperInvariant().Equals("FOWNERINIDHEAD") && a3 == "BD_Supplier") || (fieldKey.ToUpperInvariant().Equals("FOWNERINID") && a4 == "BD_Supplier"))
				{
					string text2 = base.View.Model.GetValue("FBizType") as string;
					if (!string.IsNullOrWhiteSpace(text2) && text2 == "VMI")
					{
						filter = string.Format(" FVmiBusiness = '1' ", new object[0]);
					}
				}
				break;
			}
			case "FSTOCKINORGID":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			case "FMATERIALID":
			{
				filter = " FIsInventory = '1'";
				string text3 = base.View.Model.GetValue("FBizType") as string;
				if (StringUtils.EqualsIgnoreCase(text3, "VMI"))
				{
					filter += " and FIsVmiBusiness = '1' ";
				}
				break;
			}
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FSTOCKERGROUPID") as DynamicObject;
				filter += " FIsUse='1' ";
				long num4 = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
				if (num4 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num4.ToString();
				}
				break;
			}
			case "FSTOCKERGROUPID":
			{
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FSTOCKERID") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject4 != null && Convert.ToInt64(dynamicObject4["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject4["Id"]));
				}
				break;
			}
			case "FEXTAUXUNITID":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
				break;
			case "FSRCSTOCKSTATUSID":
			{
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FSrcStockID", row) as DynamicObject;
				if (dynamicObject5 != null)
				{
					List<SelectorItemInfo> list = new List<SelectorItemInfo>();
					list.Add(new SelectorItemInfo("FStockStatusType"));
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "BD_STOCK",
						FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject5["Id"]),
						SelectItems = list
					};
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
					string text4 = "";
					if (dynamicObjectCollection != null)
					{
						DynamicObject dynamicObject6 = dynamicObjectCollection[0];
						text4 = Convert.ToString(dynamicObject6["FStockStatusType"]);
					}
					if (!string.IsNullOrWhiteSpace(text4))
					{
						text4 = "'" + text4.Replace(",", "','") + "'";
						filter = string.Format(" FType IN ({0})", text4);
					}
				}
				break;
			}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000A46 RID: 2630 RVA: 0x0008D434 File Offset: 0x0008B634
		private void UpdateDestBom(int row)
		{
			string text = "";
			DynamicObject dynamicObject = base.View.Model.GetValue("FDESTMATERIALID", row) as DynamicObject;
			if (dynamicObject == null)
			{
				text = "";
			}
			else
			{
				DynamicObjectCollection source = dynamicObject["MaterialInvPty"] as DynamicObjectCollection;
				if (source.SingleOrDefault((DynamicObject p) => Convert.ToBoolean(p["IsEnable"]) && Convert.ToInt64(p["InvPtyId_Id"]) == 10003L) == null)
				{
					text = "";
				}
				else
				{
					dynamicObject = (base.View.Model.GetValue("FBOMID", row) as DynamicObject);
					if (dynamicObject != null)
					{
						text = dynamicObject["Number"].ToString();
					}
				}
			}
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FDESTBOMID", row) as DynamicObject;
			if (this.ChangeFlag && dynamicObject2 != null)
			{
				return;
			}
			this.Model.SetItemValueByNumber("FDESTBOMID", text, row);
			base.View.InvokeFieldUpdateService("FDESTBOMID", row);
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x0008D52C File Offset: 0x0008B72C
		private void SetCustAndSupplierValue()
		{
			if (base.View.OpenParameter.Status != null)
			{
				return;
			}
			string a = base.View.Model.GetValue("FOwnerTypeIdHead") as string;
			string a2 = base.View.Model.GetValue("FOwnerTypeInIdHead") as string;
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerInIdHead") as DynamicObject;
			if (a == "BD_OwnerOrg" && a2 == "BD_OwnerOrg" && dynamicObject != null && dynamicObject2 != null)
			{
				string a3 = base.View.Model.GetValue("FTransferDirect") as string;
				if (a3 == "RETURN")
				{
					Dictionary<string, long> custAndSupplierValue = StockServiceHelper.GetCustAndSupplierValue(base.Context, dynamicObject2, dynamicObject);
					if (custAndSupplierValue != null)
					{
						base.View.Model.SetValue("FCUSTID", custAndSupplierValue["FCUSTID"]);
						base.View.Model.SetValue("FSUPPLIERID", custAndSupplierValue["FSUPPLIERID"]);
						return;
					}
				}
				else
				{
					Dictionary<string, long> custAndSupplierValue2 = StockServiceHelper.GetCustAndSupplierValue(base.Context, dynamicObject, dynamicObject2);
					if (custAndSupplierValue2 != null)
					{
						base.View.Model.SetValue("FCUSTID", custAndSupplierValue2["FCUSTID"]);
						base.View.Model.SetValue("FSUPPLIERID", custAndSupplierValue2["FSUPPLIERID"]);
						return;
					}
				}
			}
			else
			{
				base.View.Model.SetValue("FCUSTID", 0);
				base.View.Model.SetValue("FSUPPLIERID", 0);
			}
		}

		// Token: 0x0400040C RID: 1036
		private bool ChangeFlag;

		// Token: 0x0400040D RID: 1037
		private bool isDoFillMaterial;

		// Token: 0x0400040E RID: 1038
		private long lastAuxpropId;
	}
}
