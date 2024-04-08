using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.Core.FIN.HS;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000089 RID: 137
	[Description("库存单据核算成本信息动态加载插件")]
	public class StockBillHSCostEdit : AbstractBillPlugIn
	{
		// Token: 0x060006A5 RID: 1701 RVA: 0x000515C0 File Offset: 0x0004F7C0
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				this.hasPara = false;
				return;
			}
			this.hasPara = this.GetBillOpenParameter(out this.orgId, out this.acctsysid, out this.acctPolicyId);
			if (this.hasPara)
			{
				base.View.OpenParameter.Status = 1;
			}
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x0005161E File Offset: 0x0004F81E
		public override void AfterBindData(EventArgs e)
		{
			this.UpdateBillCostInfo();
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x00051626 File Offset: 0x0004F826
		public override void AfterCreateModelData(EventArgs e)
		{
			base.AfterCreateModelData(e);
			this.hasPara = false;
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x00051638 File Offset: 0x0004F838
		private void UpdateBillCostInfo()
		{
			GetHSCostBusinessServiceMeta hscostServiceMeta = this.GetHSCostServiceMeta();
			if (hscostServiceMeta == null)
			{
				return;
			}
			if (!this.hasPara || Convert.ToInt64(base.View.Model.DataObject["Id"]) <= 0L)
			{
				this.UpdateFieldVisible(hscostServiceMeta.HideFields, false);
				return;
			}
			AcctgResultRefreshBill billCostInfo = this.GetBillCostInfo(this.orgId, this.acctsysid, this.acctPolicyId);
			if (billCostInfo != null)
			{
				this.ApplyCostData(hscostServiceMeta, billCostInfo);
			}
			this.UpdateFieldVisible(hscostServiceMeta.HideFields, true);
			base.View.GetMainBarItem("tbSave").Enabled = false;
			base.View.GetMainBarItem("tbSplitSave").Enabled = false;
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x000516E8 File Offset: 0x0004F8E8
		private GetHSCostBusinessServiceMeta GetHSCostServiceMeta()
		{
			base.View.BusinessInfo.GetForm();
			Entity entity = base.View.BusinessInfo.GetEntity(0);
			if (entity.EntityServiceRules == null || entity.EntityServiceRules.Count < 1)
			{
				return null;
			}
			GetHSCostBusinessServiceMeta getHSCostBusinessServiceMeta = null;
			foreach (EntityServiceRule entityServiceRule in entity.EntityServiceRules)
			{
				if (entityServiceRule.WhenTrueBusinessServices != null && entityServiceRule.WhenTrueBusinessServices.Count > 0)
				{
					for (int i = 0; i < entityServiceRule.WhenTrueBusinessServices.Count; i++)
					{
						if (entityServiceRule.WhenTrueBusinessServices[i].ActionId == 69L)
						{
							getHSCostBusinessServiceMeta = (entityServiceRule.WhenTrueBusinessServices[i] as GetHSCostBusinessServiceMeta);
							break;
						}
					}
					if (getHSCostBusinessServiceMeta != null)
					{
						break;
					}
				}
			}
			return getHSCostBusinessServiceMeta;
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x000517CC File Offset: 0x0004F9CC
		private void ApplyCostData(GetHSCostBusinessServiceMeta service, AcctgResultRefreshBill costInfo)
		{
			if (service.CostFields == null || service.CostFields.Count < 1 || costInfo == null || costInfo.EntryDatas == null || costInfo.EntryDatas.Count < 1)
			{
				return;
			}
			this.UpdateCurrencyData(service, costInfo);
			int lcpriceDig = 6;
			int lcamountDig = 2;
			BaseDataField baseDataField = base.View.BillBusinessInfo.GetField(service.LocaleCurrencyField) as BaseDataField;
			if (baseDataField != null)
			{
				DynamicObject dynamicObject = this.Model.GetValue(baseDataField) as DynamicObject;
				if (dynamicObject != null)
				{
					lcpriceDig = Convert.ToInt32(dynamicObject["PriceDigits"]);
					lcamountDig = Convert.ToInt32(dynamicObject["AmountDigits"]);
				}
			}
			foreach (EntityCostField entityCostField in service.CostFields)
			{
				Field field = base.View.BillBusinessInfo.GetField(entityCostField.CostLocaleField);
				DynamicObjectCollection costEntity;
				if (field != null && costInfo.EntryDatas != null && costInfo.EntryDatas.TryGetValue(field.Entity.Key, out costEntity))
				{
					this.UpdateEntityCostData(entityCostField, costInfo, costEntity, lcpriceDig, lcamountDig);
				}
			}
			if (base.View.BillBusinessInfo.GetForm().Id.Equals("STK_InStock"))
			{
				object value = this.Model.GetValue("FBusinessType", 0);
				if (value != null && value.ToString().Equals("WW"))
				{
					DynamicObjectCollection costEntity;
					costInfo.ProcessExpenses.TryGetValue("FInStockEntry", out costEntity);
					this.UpdateCostFee(costInfo, costEntity, "FProcessFee", "FMaterialCosts", "FEntryCostAmount", lcpriceDig, lcamountDig);
					this.UpdateCostFee_LC(costEntity, "FProcessFee_LC", "FMaterialCosts_LC", "FCostAmount_LC");
				}
			}
			if (base.View.BillBusinessInfo.GetForm().Id.Equals("PUR_MRB"))
			{
				object value2 = this.Model.GetValue("FBusinessType", 0);
				if (value2 != null && value2.ToString().Equals("WW"))
				{
					DynamicObjectCollection costEntity;
					costInfo.ProcessExpenses.TryGetValue("FPURMRBENTRY", out costEntity);
					this.UpdateCostFee(costInfo, costEntity, "FProcessFee", "FMaterialCosts", "FENTRYCOSTAMOUNT", lcpriceDig, lcamountDig);
					this.UpdateCostFee_LC(costEntity, "FProcessFee_LC", "FMaterialCosts_LC", "FCOSTAMOUNT_LC");
				}
			}
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x00051A18 File Offset: 0x0004FC18
		private AcctgResultRefreshBill GetBillCostInfo(long orgId, long acctsysid, long acctPolicyId)
		{
			AcctgResultRefreshBill acctgResultRefreshBill = new AcctgResultRefreshBill();
			acctgResultRefreshBill.AcctgSysId = acctsysid;
			acctgResultRefreshBill.OrgId = orgId;
			acctgResultRefreshBill.AcctPolicyId = acctPolicyId;
			acctgResultRefreshBill.BillFromId = base.View.BillBusinessInfo.GetForm().Id;
			acctgResultRefreshBill.BillId = Convert.ToInt64(base.View.Model.DataObject["Id"]);
			return CommonServiceHelper.GetBillHSCostInfo(base.View.Context, acctgResultRefreshBill);
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x00051A94 File Offset: 0x0004FC94
		private bool GetBillOpenParameter(out long orgId, out long acctsysid, out long acctPolicyId)
		{
			bool flag = false;
			orgId = 0L;
			acctsysid = 0L;
			acctPolicyId = 0L;
			DynamicFormOpenParameter openParameter = base.View.OpenParameter;
			if (openParameter == null && base.View.ParentFormView != null && base.View.ParentFormView is IListView)
			{
				openParameter = base.View.ParentFormView.OpenParameter;
			}
			if (openParameter == null)
			{
				return flag;
			}
			object customParameter = openParameter.GetCustomParameter("FACCTSYSTEMID");
			if (customParameter != null)
			{
				long.TryParse(customParameter.ToString(), out acctsysid);
			}
			if (acctsysid == 0L)
			{
				if (base.View.ParentFormView == null || !(base.View.ParentFormView is IListView))
				{
					return flag;
				}
				openParameter = base.View.ParentFormView.OpenParameter;
				if (openParameter == null)
				{
					return flag;
				}
				customParameter = openParameter.GetCustomParameter("FACCTSYSTEMID");
				if (customParameter != null)
				{
					long.TryParse(customParameter.ToString(), out acctsysid);
				}
				if (acctsysid == 0L)
				{
					return flag;
				}
			}
			customParameter = openParameter.GetCustomParameter("FACCTORGID");
			if (customParameter != null)
			{
				long.TryParse(customParameter.ToString(), out orgId);
			}
			if (orgId == 0L)
			{
				return flag;
			}
			customParameter = openParameter.GetCustomParameter("FACCTPOLICYID");
			if (customParameter != null)
			{
				long.TryParse(customParameter.ToString(), out acctPolicyId);
			}
			return acctPolicyId != 0L || flag;
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x00051BC0 File Offset: 0x0004FDC0
		private void UpdateCurrencyData(GetHSCostBusinessServiceMeta service, AcctgResultRefreshBill costInfo)
		{
			BaseDataField baseDataField = base.View.BillBusinessInfo.GetField(service.LocaleCurrencyField) as BaseDataField;
			BaseDataField baseDataField2 = base.View.BillBusinessInfo.GetField(service.ExchangeRateTypeField) as BaseDataField;
			DecimalField decimalField = base.View.BillBusinessInfo.GetField(service.ExchangeRateField) as DecimalField;
			DynamicObject dynamicObject = base.View.Model.GetValue(baseDataField) as DynamicObject;
			if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) == costInfo.LocalCurrId)
			{
				return;
			}
			if (baseDataField != null)
			{
				this.Model.SetItemValueByID(service.LocaleCurrencyField, costInfo.LocalCurrId, 0);
			}
			if (baseDataField2 != null)
			{
				this.Model.SetItemValueByID(service.ExchangeRateTypeField, costInfo.RateTypeID, 0);
			}
			if (decimalField != null && costInfo.Rate != 0m)
			{
				base.View.Model.SetValue(service.ExchangeRateField, costInfo.Rate);
			}
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x00051CCC File Offset: 0x0004FECC
		private void UpdateEntityCostData(EntityCostField costFieldSet, AcctgResultRefreshBill costInfo, DynamicObjectCollection costEntity, int lcpriceDig, int lcamountDig)
		{
			if (costFieldSet == null || string.IsNullOrWhiteSpace(costFieldSet.CostLocaleField) || costEntity.Count < 1)
			{
				return;
			}
			Field field = base.View.BillBusinessInfo.GetField(costFieldSet.CostLocaleField);
			if (field.Entity is SubEntryEntity)
			{
				this.UpdateSubEntryEntityCostInfo(costFieldSet, costInfo, costEntity, lcpriceDig, lcamountDig);
				return;
			}
			if (field.Entity is EntryEntity)
			{
				this.UpdateEntryEntityCostInfo(costFieldSet, costInfo, costEntity, lcpriceDig, lcamountDig);
				return;
			}
			this.UpdateSingleEntityCostInfo(costFieldSet, costInfo, costEntity, lcpriceDig, lcamountDig);
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x00051D98 File Offset: 0x0004FF98
		private void UpdateSubEntryEntityCostInfo(EntityCostField costFieldSet, AcctgResultRefreshBill costInfo, DynamicObjectCollection costEntity, int lcpriceDig, int lcamountDig)
		{
			Field field = base.View.BillBusinessInfo.GetField(costFieldSet.CostPriceField);
			Field field2 = base.View.BillBusinessInfo.GetField(costFieldSet.CostPriceLocField);
			Field field3 = base.View.BillBusinessInfo.GetField(costFieldSet.CostField);
			Field field4 = base.View.BillBusinessInfo.GetField(costFieldSet.CostLocaleField);
			Field field5 = base.View.BillBusinessInfo.GetField(costFieldSet.CostQtyField);
			decimal rate = costInfo.Rate;
			DynamicObjectCollection value = ((SubEntryEntity)field4.Entity).ParentEntity.DynamicProperty.GetValue<DynamicObjectCollection>(base.View.Model.DataObject);
			int count = value.Count;
			DynamicObjectCollection subEntryData;
			for (int j = 0; j < count; j++)
			{
				subEntryData = field4.Entity.DynamicProperty.GetValue<DynamicObjectCollection>(value[j]);
				int count2 = subEntryData.Count;
				int i;
				for (i = 0; i < count2; i++)
				{
					DynamicObject dynamicObject = costEntity.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["FEntryId"]) == Convert.ToInt64(subEntryData[i]["Id"]));
					if (dynamicObject != null)
					{
						decimal num = Convert.ToDecimal(dynamicObject["FAmount_LC"]);
						if (field3 != null && rate != 0m)
						{
							base.View.Model.SetValue(field3.Key, num / rate, i);
							base.View.InvokeFieldUpdateService(field3.Key, i);
						}
						decimal num2 = 0m;
						if (field5 != null)
						{
							num2 = field5.DynamicProperty.GetValue<decimal>(subEntryData[i]);
						}
						if (field != null)
						{
							if (num2 != 0m && rate != 0m)
							{
								base.View.Model.SetValue(field.Key, num / num2 / rate, i);
							}
							else
							{
								base.View.Model.SetValue(field.Key, 0, i);
							}
						}
						base.View.Model.SetValue(field4.Key, num, i);
						base.View.InvokeFieldUpdateService(field4.Key, i);
						if (field2 != null)
						{
							if (num2 != 0m)
							{
								base.View.Model.SetValue(field2.Key, Math.Round(num / num2, lcpriceDig), i);
							}
							else
							{
								base.View.Model.SetValue(field2.Key, 0, i);
							}
						}
					}
				}
			}
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x000520C8 File Offset: 0x000502C8
		private void UpdateSingleEntityCostInfo(EntityCostField costFieldSet, AcctgResultRefreshBill costInfo, DynamicObjectCollection costEntity, int lcpriceDig, int lcamountDig)
		{
			decimal rate = costInfo.Rate;
			Field field = base.View.BillBusinessInfo.GetField(costFieldSet.CostPriceField);
			Field field2 = base.View.BillBusinessInfo.GetField(costFieldSet.CostPriceLocField);
			Field field3 = base.View.BillBusinessInfo.GetField(costFieldSet.CostField);
			Field field4 = base.View.BillBusinessInfo.GetField(costFieldSet.CostLocaleField);
			Field field5 = base.View.BillBusinessInfo.GetField(costFieldSet.CostQtyField);
			decimal num = 0m;
			DynamicObject dynamicObject;
			if (field4.Entity is HeadEntity)
			{
				dynamicObject = field4.Entity.DynamicProperty.GetValue<DynamicObject>(base.View.Model.DataObject);
			}
			else
			{
				dynamicObject = field4.Entity.DynamicProperty.GetValue<DynamicObjectCollection>(base.View.Model.DataObject)[0];
			}
			if (Convert.ToInt64(dynamicObject["Id"]) == Convert.ToInt64(costEntity[0]["FEntryId"]))
			{
				decimal num2 = Convert.ToDecimal(costEntity[0]["FAmount_LC"]);
				if (field3 != null && rate != 0m)
				{
					base.View.Model.SetValue(field3.Key, num2 / rate, 0);
					base.View.InvokeFieldUpdateService(field3.Key, 0);
				}
				if (field5 != null)
				{
					num = field5.DynamicProperty.GetValue<decimal>(dynamicObject);
				}
				if (field != null)
				{
					if (num != 0m && rate != 0m)
					{
						base.View.Model.SetValue(field.Key, num2 / num / rate, 0);
					}
					else
					{
						base.View.Model.SetValue(field.Key, 0, 0);
					}
				}
				base.View.Model.SetValue(field4.Key, num2, 0);
				base.View.InvokeFieldUpdateService(field4.Key, 0);
				if (field2 != null)
				{
					if (num != 0m)
					{
						base.View.Model.SetValue(field2.Key, Math.Round(num2 / num, lcpriceDig), 0);
						return;
					}
					base.View.Model.SetValue(field2.Key, 0, 0);
				}
			}
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x000523A0 File Offset: 0x000505A0
		private void UpdateEntryEntityCostInfo(EntityCostField costFieldSet, AcctgResultRefreshBill costInfo, DynamicObjectCollection costEntity, int lcpriceDig, int lcamountDig)
		{
			StockBillHSCostEdit.<>c__DisplayClass7 CS$<>8__locals1 = new StockBillHSCostEdit.<>c__DisplayClass7();
			Field field = base.View.BillBusinessInfo.GetField(costFieldSet.CostPriceField);
			Field field2 = base.View.BillBusinessInfo.GetField(costFieldSet.CostPriceLocField);
			Field field3 = base.View.BillBusinessInfo.GetField(costFieldSet.CostField);
			Field field4 = base.View.BillBusinessInfo.GetField(costFieldSet.CostLocaleField);
			Field field5 = base.View.BillBusinessInfo.GetField(costFieldSet.CostQtyField);
			decimal rate = costInfo.Rate;
			CS$<>8__locals1.entryData = field4.Entity.DynamicProperty.GetValue<DynamicObjectCollection>(base.View.Model.DataObject);
			int count = CS$<>8__locals1.entryData.Count;
			int i;
			for (i = 0; i < count; i++)
			{
				DynamicObject dynamicObject = costEntity.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["FEntryId"]) == Convert.ToInt64(CS$<>8__locals1.entryData[i]["Id"]));
				if (dynamicObject != null)
				{
					decimal num = Convert.ToDecimal(dynamicObject["FAmount_LC"]);
					if (field3 != null && rate != 0m)
					{
						base.View.Model.SetValue(field3.Key, num / rate, i);
					}
					decimal num2 = 0m;
					if (field5 != null)
					{
						num2 = field5.DynamicProperty.GetValue<decimal>(CS$<>8__locals1.entryData[i]);
					}
					if (field != null)
					{
						if (num2 != 0m && rate != 0m)
						{
							base.View.Model.SetValue(field.Key, num / num2 / rate, i);
						}
						else
						{
							base.View.Model.SetValue(field.Key, 0, i);
						}
					}
					base.View.Model.SetValue(field4.Key, num, i);
					base.View.InvokeFieldUpdateService(field4.Key, i);
					if (field2 != null)
					{
						if (num2 != 0m)
						{
							base.View.Model.SetValue(field2.Key, Math.Round(num / num2, lcpriceDig), i);
						}
						else
						{
							base.View.Model.SetValue(field2.Key, 0, i);
						}
					}
				}
			}
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x000526BC File Offset: 0x000508BC
		private void UpdateCostFee(AcctgResultRefreshBill costInfo, DynamicObjectCollection costEntity, string processFeeKey, string materialCostKey, string costAmountKey, int lcpriceDig, int lcamountDig)
		{
			StockBillHSCostEdit.<>c__DisplayClassd CS$<>8__locals1 = new StockBillHSCostEdit.<>c__DisplayClassd();
			Field field = base.View.BillBusinessInfo.GetField(processFeeKey);
			Field field2 = base.View.BillBusinessInfo.GetField(materialCostKey);
			Field field3 = base.View.BillBusinessInfo.GetField(costAmountKey);
			decimal rate = costInfo.Rate;
			CS$<>8__locals1.entryData = field2.Entity.DynamicProperty.GetValue<DynamicObjectCollection>(base.View.Model.DataObject);
			int count = CS$<>8__locals1.entryData.Count;
			int i;
			for (i = 0; i < count; i++)
			{
				decimal d = 0m;
				if (costEntity != null)
				{
					DynamicObject dynamicObject = costEntity.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["FEntryId"]) == Convert.ToInt64(CS$<>8__locals1.entryData[i]["Id"]));
					if (dynamicObject != null)
					{
						d = Convert.ToDecimal(dynamicObject["FAmount_LC"]);
					}
				}
				if (rate != 0m)
				{
					base.View.Model.SetValue(field.Key, d / rate, i);
					base.View.InvokeFieldUpdateService(field.Key, i);
					decimal value = field3.DynamicProperty.GetValue<decimal>(CS$<>8__locals1.entryData[i]);
					base.View.Model.SetValue(field2.Key, value - d / rate, i);
					base.View.InvokeFieldUpdateService(field2.Key, i);
				}
			}
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x000528D8 File Offset: 0x00050AD8
		private void UpdateCostFee_LC(DynamicObjectCollection costEntity, string processFeeKey_LC, string materialCostKey_LC, string costAmountKey_LC)
		{
			StockBillHSCostEdit.<>c__DisplayClass13 CS$<>8__locals1 = new StockBillHSCostEdit.<>c__DisplayClass13();
			Field field = base.View.BillBusinessInfo.GetField(processFeeKey_LC);
			Field field2 = base.View.BillBusinessInfo.GetField(materialCostKey_LC);
			Field field3 = base.View.BillBusinessInfo.GetField(costAmountKey_LC);
			CS$<>8__locals1.entryData = field2.Entity.DynamicProperty.GetValue<DynamicObjectCollection>(base.View.Model.DataObject);
			int count = CS$<>8__locals1.entryData.Count;
			int i;
			for (i = 0; i < count; i++)
			{
				decimal num = 0m;
				if (costEntity != null)
				{
					DynamicObject dynamicObject = costEntity.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["FEntryId"]) == Convert.ToInt64(CS$<>8__locals1.entryData[i]["Id"]));
					if (dynamicObject != null)
					{
						num = Convert.ToDecimal(dynamicObject["FAmount_LC"]);
					}
				}
				decimal value = field3.DynamicProperty.GetValue<decimal>(CS$<>8__locals1.entryData[i]);
				base.View.Model.SetValue(field.Key, num, i);
				base.View.InvokeFieldUpdateService(field.Key, i);
				base.View.Model.SetValue(field2.Key, value - num, i);
				base.View.InvokeFieldUpdateService(field2.Key, i);
			}
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x00052A84 File Offset: 0x00050C84
		private void UpdateFieldVisible(List<KeyValue> fields, bool useCostSet = false)
		{
			if (fields == null || fields.Count < 1)
			{
				return;
			}
			foreach (KeyValue keyValue in fields)
			{
				string[] array = keyValue.Value.Split(new char[]
				{
					';'
				});
				string text = useCostSet ? array[1] : array[0];
				if (text.Equals("1"))
				{
					base.View.StyleManager.SetVisible(keyValue.Key, "GetHSCostService", true);
				}
				else
				{
					base.View.StyleManager.SetVisible(keyValue.Key, "GetHSCostService", false);
				}
			}
		}

		// Token: 0x04000273 RID: 627
		private bool hasPara;

		// Token: 0x04000274 RID: 628
		private long acctsysid;

		// Token: 0x04000275 RID: 629
		private long orgId;

		// Token: 0x04000276 RID: 630
		private long acctPolicyId;
	}
}
