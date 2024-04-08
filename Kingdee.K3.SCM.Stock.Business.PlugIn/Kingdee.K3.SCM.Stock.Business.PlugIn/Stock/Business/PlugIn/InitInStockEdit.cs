using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Business.Bill.Service.Tax;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200001B RID: 27
	public class InitInStockEdit : AbstractBillPlugIn
	{
		// Token: 0x060000F1 RID: 241 RVA: 0x0000DCA0 File Offset: 0x0000BEA0
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetBusinessTypeByBillType();
				this.SetDefaultOwner();
				this.SetDefLocalCurrencyAndExchangeType();
				this.SetProductDate(-1);
				if (this.isUseTaxCombination)
				{
					base.View.Model.SetValue("FIsIncludedTax", false);
				}
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				if (baseDataLongValue > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
				}
			}
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x0000DD34 File Offset: 0x0000BF34
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW") && !(a == "COPYENTRYROW"))
				{
					return;
				}
				this.isCopyEntryOperating = true;
			}
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000DD7C File Offset: 0x0000BF7C
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW") && !(a == "COPYENTRYROW"))
				{
					return;
				}
				this.isCopyEntryOperating = false;
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x0000DDC0 File Offset: 0x0000BFC0
		public override void AfterBindData(EventArgs e)
		{
			base.View.StyleManager.SetEnabled("FTaxCombination", "", this.isUseTaxCombination);
			base.View.StyleManager.SetEnabled("FIsIncludedTax", "", !this.isUseTaxCombination);
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				this.SetPushAndSelBillCalculate();
				this.SetAPNotJoinQty();
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0000DE54 File Offset: 0x0000C054
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			foreach (DynamicObject dynamicObject in (e.DataObject["InitInStockEntry"] as DynamicObjectCollection))
			{
				dynamicObject["APNotJoinQty"] = Convert.ToDecimal(dynamicObject["PriceUnitQty"]);
			}
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000DED0 File Offset: 0x0000C0D0
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			decimal num = Convert.ToDecimal(base.View.Model.GetValue("FPriceUnitQty", e.Row));
			if (e.EntityKey.ToUpperInvariant() == "FINITINSTOCKENTRY")
			{
				base.View.Model.SetValue("FAPNotJoinQty", num, e.NewRow);
			}
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000DF38 File Offset: 0x0000C138
		public void SetDefaultTaxRate(AbstractBillPlugIn billPlugIn, PriceDiscTaxArgs args, int row)
		{
			TaxRuleConditionParam taxRuleConditionParam = this.PrepareTaxRuleConditionParam(billPlugIn, 1, args, row);
			if (this.svc == null)
			{
				this.svc = new TaxService();
			}
			TaxRuleResult taxRuleResult = this.svc.GetTaxRuleResult(taxRuleConditionParam, billPlugIn.View.Context);
			DynamicObject defaultValue = taxRuleResult.DefaultValue;
			if (defaultValue != null)
			{
				billPlugIn.Model.SetValue(args.EntryTaxRateKey, Convert.ToDecimal(defaultValue["TaxRate"]), row);
				billPlugIn.View.InvokeFieldUpdateService(args.EntryTaxRateKey, row);
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000DFE0 File Offset: 0x0000C1E0
		private TaxRuleConditionParam PrepareTaxRuleConditionParam(AbstractBillPlugIn billPlugIn, TaxType taxType, PriceDiscTaxArgs args, int row)
		{
			TaxRuleConditionParam taxRuleConditionParam = new TaxRuleConditionParam();
			OrgField orgField = (from p in billPlugIn.View.BillBusinessInfo.GetFieldList()
			where p is OrgField && (p as OrgField).IsMainOrg == 1
			select p as OrgField).FirstOrDefault<OrgField>();
			if (orgField != null)
			{
				object value = billPlugIn.View.Model.GetValue(orgField.Key);
				if (value != null)
				{
					DynamicObject dynamicObject = value as DynamicObject;
					taxRuleConditionParam.MainBusinessOrg = ((dynamicObject == null) ? -1 : int.Parse(dynamicObject["Id"].ToString()));
				}
			}
			taxRuleConditionParam.TaxType = taxType;
			taxRuleConditionParam.Bill = billPlugIn.View.BillBusinessInfo.GetForm().Id;
			object value2 = billPlugIn.View.Model.GetValue(args.BillTypeKey);
			taxRuleConditionParam.BillType = ((value2 == null) ? null : value2.ToString());
			taxRuleConditionParam.PartnerType = 0;
			object value3 = billPlugIn.View.Model.GetValue(args.SupplierOrCustomerKey);
			if (value3 != null)
			{
				DynamicObject dynamicObject2 = (DynamicObject)value3;
				taxRuleConditionParam.SupplierOrCustomer = dynamicObject2["Id"].ToString();
				taxRuleConditionParam.TaxCategoryForSupplierOrCustomer = ((DynamicObjectCollection)dynamicObject2["SupplierFinance"])[0]["FTaxType_Id"].ToString();
			}
			DynamicObject dynamicObject3 = (DynamicObject)billPlugIn.View.Model.GetValue(args.MaterialKey, row);
			if (dynamicObject3 != null)
			{
				taxRuleConditionParam.Material = dynamicObject3["Id"].ToString();
				taxRuleConditionParam.TaxCategoryForMaterial = ((DynamicObjectCollection)dynamicObject3["MaterialBase"])[0]["TaxType_Id"].ToString();
			}
			return taxRuleConditionParam;
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0000E1B8 File Offset: 0x0000C3B8
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSUPPLIERID":
			case "FSUPPLYID":
				if (!this.isUseTaxCombination && StringUtils.EqualsIgnoreCase(e.Field.Key.ToUpperInvariant(), "FSUPPLIERID"))
				{
					for (int i = 0; i < base.View.Model.GetEntryRowCount("FInitInStockEntry"); i++)
					{
						this.SetDefaultTaxRate(this, this.priceDiscTaxArgs, i);
					}
				}
				this.DoSupplyChange(e);
				SCMCommon.SetDefaultProviderContact(base.Context, this, "FSupplyId", "FProviderContactID", "FSupplyAddress");
				return;
			case "FPURCHASERID":
			case "FSTOCKERID":
				if (e.Field.Key.ToUpperInvariant() == "FPURCHASERID")
				{
					Common.SetGroupValue(this, "FPurchaserId", "FPurchaserGroupId", "CGY");
					return;
				}
				Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
				return;
			case "FSTOCKORGID":
			{
				this.SetDefLocalCurrencyAndExchangeType();
				string a = base.View.Model.GetValue("FOwnerTypeIdHead").ToString();
				if (a == "BD_OwnerOrg")
				{
					base.View.Model.SetValue("FOWNERIDHEAD", e.NewValue);
					return;
				}
				break;
			}
			case "FEXCHANGETYPEID":
			case "FSETTLECURRID":
			case "FDATE":
				this.SetExchangeRate();
				return;
			case "FDISCOUNTRATE":
			{
				decimal d = ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue) ? 0m : Convert.ToDecimal(e.NewValue);
				decimal d2 = ObjectUtils.IsEmptyPrimaryKey(e.OldValue) ? 0m : Convert.ToDecimal(e.OldValue);
				if (d != d2)
				{
					Entity entity = base.View.BusinessInfo.GetEntity("FInitInStockEntry");
					DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entity, e.Row);
					DynamicObjectCollection dynamicObjectCollection = entityDataObject["Discount_Detail"] as DynamicObjectCollection;
					dynamicObjectCollection.Clear();
					return;
				}
				break;
			}
			case "FOWNERIDHEAD":
				this.SetDefaultOwner();
				return;
			case "FSETTLEORGID":
				this.SetDefLocalCurrencyAndExchangeType();
				return;
			case "FMATERIALID":
			{
				this.SetProductDate(e.Row);
				long num2 = 0L;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
				this.SetDefaultOwner(e.Row);
				return;
			}
			case "FPRICEUNITID":
			{
				bool flag = Convert.ToBoolean(this.Model.GetValue("FIsIncludedTax"));
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
				if (dynamicObject3 != null && e.NewValue != null && e.OldValue != null)
				{
					long num3 = Convert.ToInt64(e.NewValue);
					long num4 = Convert.ToInt64(e.OldValue);
					long num5 = Convert.ToInt64(dynamicObject3["Id"]);
					if (flag)
					{
						SCMCommon.UnitIdChangeReCalPrice(base.Context, this, "FTaxPrice", e.Row, num5, num3, num4);
						return;
					}
					SCMCommon.UnitIdChangeReCalPrice(base.Context, this, "FPrice", e.Row, num5, num3, num4);
					return;
				}
				break;
			}
			case "FDEMANDORGID":
				this.SetOwnerIdHeadByBusinessType();
				return;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				break;
			}

				return;
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000E654 File Offset: 0x0000C854
		private void SetSupplyAddress(DataChangedEventArgs e)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FSupplyId") as DynamicObject;
			if (dynamicObject != null)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject["SupplierLocation"] as DynamicObjectCollection;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					if (Convert.ToBoolean(dynamicObject2["IsDefaultSupply"]) && Convert.ToBoolean(dynamicObject2["IsUsed"]))
					{
						DynamicObject dynamicObject3 = dynamicObject2["ContactId"] as DynamicObject;
						if (dynamicObject3 != null)
						{
							this.Model.SetValue("FProviderContactID", dynamicObject3["Id"]);
							this.Model.SetValue("FSupplyAddress", dynamicObject2["Address"]);
							return;
						}
					}
				}
				this.Model.SetValue("FSupplyAddress", null);
				this.Model.SetValue("FProviderContactID", null);
				DynamicObject dynamicObject4 = (dynamicObject["SupplierBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>();
				if (dynamicObject4 != null)
				{
					this.Model.SetValue("FSupplyAddress", dynamicObject4["Address"]);
					return;
				}
			}
			else
			{
				this.Model.SetValue("FSupplyAddress", null);
				this.Model.SetValue("FProviderContactID", null);
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000E7C0 File Offset: 0x0000C9C0
		private void DoSupplyChange(DataChangedEventArgs e)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FSupplierId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num > 0L)
			{
				OQLFilter oqlfilter = new OQLFilter();
				oqlfilter.Add(new OQLFilterHeadEntityItem
				{
					FilterString = string.Format(" FSupplierId = {0} ", num.ToString())
				});
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FNumber"));
				list.Add(new SelectorItemInfo("FPayCondition"));
				list.Add(new SelectorItemInfo("FPayCurrencyId"));
				list.Add(new SelectorItemInfo("FSettleTypeId"));
				DynamicObject dynamicObject2 = BusinessDataServiceHelper.Load(base.Context, "BD_Supplier", list, oqlfilter).FirstOrDefault<DynamicObject>();
				object obj = null;
				object obj2 = null;
				object obj3 = null;
				if (((DynamicObjectCollection)dynamicObject2["SupplierBusiness"]).Count > 0)
				{
					obj2 = ((DynamicObjectCollection)dynamicObject2["SupplierBusiness"])[0]["SettleTypeId"];
				}
				if (((DynamicObjectCollection)dynamicObject2["SupplierFinance"]).Count > 0)
				{
					obj3 = ((DynamicObjectCollection)dynamicObject2["SupplierFinance"])[0]["PayCurrencyId"];
					obj = ((DynamicObjectCollection)dynamicObject2["SupplierFinance"])[0]["PayCondition"];
				}
				base.View.Model.SetValue("FSettleCurrId", (obj3 != null) ? ((DynamicObject)obj3)["Id"] : 0);
				this.SetExchangeRate();
				base.View.Model.SetValue("FPayConditionId", (obj != null) ? ((DynamicObject)obj)["Id"] : null);
				base.View.Model.SetValue("FSettleTypeId", (obj2 != null) ? ((DynamicObject)obj2)["Id"] : null);
				return;
			}
			base.View.Model.SetValue("FSettleCurrId", null);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000E9E4 File Offset: 0x0000CBE4
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.View.GetControl("FPTaxRate").Visible = !this.isUseTaxCombination;
			base.View.GetControl("FPTaxCombination").Visible = this.isUseTaxCombination;
			base.View.GetControl("FPFTaxPurePrice").Visible = !this.isUseTaxCombination;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0000EA48 File Offset: 0x0000CC48
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FSTOCKID":
			case "FPURCHASERID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FPURCHASERGROUPID":
			case "FMATERIALID":
			case "FSUPPLIERID":
			case "FEXTAUXUNITID":
			{
				string text;
				if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
					return;
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				string text2 = string.Format("  fforbidstatus='A' AND FDOCUMENTSTATUS='C' ", new object[0]);
				DynamicObject dynamicObject = this.Model.GetValue("fstockid", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					string text3 = Convert.ToString(dynamicObject["StockStatusType"]);
					if (!string.IsNullOrEmpty(text3))
					{
						text2 += string.Format(" and ftype in ({0})  ", text3);
					}
				}
				if (string.IsNullOrEmpty(e.Filter))
				{
					e.Filter = text2;
					return;
				}
				e.Filter = e.Filter + " AND " + text2;
				return;
			}
			case "FBOMID":
			{
				string text;
				if (this.GetBOMFilter(e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000EC4C File Offset: 0x0000CE4C
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSTOCKID":
			case "FPURCHASERID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FPURCHASERGROUPID":
			case "FMATERIALID":
			case "FSUPPLIERID":
			case "FEXTAUXUNITID":
			{
				string text;
				if (this.GetStockFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					return;
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				string text2 = string.Format("  fforbidstatus='A' AND FDOCUMENTSTATUS='C' ", new object[0]);
				DynamicObject dynamicObject = this.Model.GetValue("fstockid", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					string text3 = Convert.ToString(dynamicObject["StockStatusType"]);
					if (!string.IsNullOrEmpty(text3))
					{
						text2 += string.Format(" and ftype in ({0})  ", text3);
					}
				}
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = text2;
					return;
				}
				IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
				listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text2;
				return;
			}
			case "FBOMID":
			{
				string text;
				if (this.GetBOMFilter(e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + text;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0000EE7C File Offset: 0x0000D07C
		public override void BeforeBindData(EventArgs e)
		{
			base.View.GetControl("FPTaxRate").Visible = !this.isUseTaxCombination;
			base.View.GetControl("FPTaxCombination").Visible = this.isUseTaxCombination;
			base.View.GetControl("FPFTaxPurePrice").Visible = !this.isUseTaxCombination;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000EEE0 File Offset: 0x0000D0E0
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.PreparePriceDiscTaxArgs();
			this.isUseTaxCombination = SystemParameterServiceHelper.IsUseTaxCombination(base.Context);
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0000F004 File Offset: 0x0000D204
		public override void ToolBarItemClick(BarItemClickEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.BarItemKey, "TBPAYABLECLOSE"))
			{
				PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
				{
					Id = "STK_InitInStock"
				}, "5ae0446d314ec4");
				if (!permissionAuthResult.Passed)
				{
					string arg = "";
					OrgField orgField = (from p in base.View.BillBusinessInfo.GetFieldList()
					where p is OrgField && (p as OrgField).IsMainOrg == 1
					select p as OrgField).FirstOrDefault<OrgField>();
					if (orgField != null)
					{
						DynamicObject dynamicObject = base.View.Model.GetValue(orgField.Key) as DynamicObject;
						if (dynamicObject != null)
						{
							arg = Convert.ToString(dynamicObject["Name"]);
						}
					}
					base.View.ShowMessage(string.Format(ResManager.LoadKDString("您在【{0}】组织下没有【{1}】的【应付关闭】权限，请联系系统管理员！", "004023000038522", 5, new object[0]), arg, base.View.BusinessInfo.GetForm().Name[base.Context.UserLocale.LCID]), 0);
					e.Cancel = true;
					return;
				}
				if (Convert.ToString(base.View.Model.GetValue("FDocumentStatus")) != "C")
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("单据已审核才能进行应付关闭！", "004023000021307", 5, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				new List<DynamicObject>();
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FInitInStockEntry");
				int[] selectedRows = base.View.GetControl<EntryGrid>(entryEntity.Key).GetSelectedRows();
				if (selectedRows == null)
				{
					e.Cancel = true;
					return;
				}
				int currentRowIndex = selectedRows[0];
				DynamicObject currentRow = base.View.Model.GetEntityDataObject(entryEntity)[currentRowIndex];
				if ("B" == Convert.ToString(currentRow["PayableCloseStatus"]))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("应付已关闭，不能重复操作！", "004023000021308", 5, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "PUR_PAYABLECLOSEVIEW";
				dynamicFormShowParameter.ParentPageId = base.View.PageId;
				dynamicFormShowParameter.CustomComplexParams.Add("Args", currentRow);
				dynamicFormShowParameter.CustomComplexParams.Add("SettleOrg", this.Model.GetValue("FSettleOrgId"));
				dynamicFormShowParameter.CustomComplexParams.Add("FormId", base.View.UserParameterKey);
				base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
				{
					if (result.ReturnData == null)
					{
						return;
					}
					DateTime dateTime = (DateTime)result.ReturnData;
					if (1 == CommonServiceHelper.UpdateBillPayableCloseStatus(this.Context, "T_STK_INITINSTOCKENTRY_i", Convert.ToInt64(currentRow["id"]), "B", dateTime))
					{
						this.View.Model.SetValue("FPayableCloseStatus", "B", currentRowIndex);
						this.View.Model.SetValue("FPayableCloseDate", dateTime, currentRowIndex);
						this.View.Model.DataChanged = false;
						this.View.ShowMessage(ResManager.LoadKDString("应付关闭成功！", "004023000023762", 5, new object[0]), 0);
					}
				});
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x0000F300 File Offset: 0x0000D500
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x0000F364 File Offset: 0x0000D564
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
				base.View.UpdateView("FInitInStockEntry", e.Row);
			}
		}

		// Token: 0x06000104 RID: 260 RVA: 0x0000F3BC File Offset: 0x0000D5BC
		private void SetAPNotJoinQty()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FInitInStockEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				base.View.Model.SetValue("FAPNotJoinQty", Convert.ToDecimal(base.View.Model.GetValue("FPriceUnitQty", i)), i);
			}
		}

		// Token: 0x06000105 RID: 261 RVA: 0x0000F424 File Offset: 0x0000D624
		private void SetOwnerIdHeadByBusinessType()
		{
			object value = base.View.Model.GetValue("FDemandOrgId");
			base.View.Model.SetValue("FOwnerTypeIdHead", "BD_OwnerOrg");
			base.View.Model.SetValue("FOwnerIdHead", value);
		}

		// Token: 0x06000106 RID: 262 RVA: 0x0000F478 File Offset: 0x0000D678
		private void SetPushAndSelBillCalculate()
		{
			Field field = base.View.BusinessInfo.GetField("FAmount");
			base.View.Model.SummaryDataAndFill(field, "FBillAmount");
			Field field2 = base.View.BusinessInfo.GetField("FEntryTaxAmount");
			base.View.Model.SummaryDataAndFill(field2, "FBillTaxAmount");
			Field field3 = base.View.BusinessInfo.GetField("FAllAmount");
			base.View.Model.SummaryDataAndFill(field3, "FBillAllAmount");
			Field field4 = base.View.BusinessInfo.GetField("FAmount_LC");
			base.View.Model.SummaryDataAndFill(field4, "FBillAmount_LC");
			Field field5 = base.View.BusinessInfo.GetField("FTaxAmount_LC");
			base.View.Model.SummaryDataAndFill(field5, "FBillTaxAmount_LC");
			Field field6 = base.View.BusinessInfo.GetField("FAllAmount_LC");
			base.View.Model.SummaryDataAndFill(field6, "FBillAllAmount_LC");
		}

		// Token: 0x06000107 RID: 263 RVA: 0x0000F594 File Offset: 0x0000D794
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "PUR_InitInStockParam", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBusinessType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0000F5E4 File Offset: 0x0000D7E4
		private void SetExchangeRate()
		{
			DynamicObject dynamicObject = (DynamicObject)base.View.Model.GetValue("FLocalCurrId");
			DynamicObject dynamicObject2 = (DynamicObject)base.View.Model.GetValue("FExchangeTypeId");
			DynamicObject dynamicObject3 = (DynamicObject)base.View.Model.GetValue("FSettleCurrId");
			if (dynamicObject == null || dynamicObject2 == null || dynamicObject3 == null)
			{
				return;
			}
			long num = Convert.ToInt64(dynamicObject["Id"]);
			long num2 = Convert.ToInt64(dynamicObject2["Id"]);
			long num3 = Convert.ToInt64(dynamicObject3["Id"]);
			DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate"));
			if (num == num3 || dateTime == DateTime.MinValue)
			{
				base.View.Model.SetValue("FExchangeRate", 1);
				return;
			}
			KeyValuePair<decimal, int> exchangeRateAndDecimal = CommonServiceHelper.GetExchangeRateAndDecimal(base.Context, num3, num, num2, dateTime, dateTime);
			base.View.Model.SetValue("FExchangeRate", exchangeRateAndDecimal.Key);
			base.View.GetFieldEditor<DecimalFieldEditor>("FExchangeRate", 0).Scale = Convert.ToInt16(exchangeRateAndDecimal.Value);
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000F728 File Offset: 0x0000D928
		private void SetDefLocalCurrencyAndExchangeType()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("1", "FSettleOrgId", "FExchangeTypeId", "FLocalCurrId", "FSettleCurrId", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
			this.SetExchangeRate();
		}

		// Token: 0x0600010A RID: 266 RVA: 0x0000F76C File Offset: 0x0000D96C
		private void SetDefaultOwner(int rowIndex)
		{
			Convert.ToString(base.View.Model.GetValue("FBusinessType"));
			base.View.Model.SetValue("FOwnerTypeId", "BD_OwnerOrg", rowIndex);
			base.View.Model.SetValue("FOwnerId", base.View.Model.GetValue("FOwnerIdHead"), rowIndex);
		}

		// Token: 0x0600010B RID: 267 RVA: 0x0000F7DC File Offset: 0x0000D9DC
		private void SetDefaultOwner()
		{
			int entryRowCount = this.Model.GetEntryRowCount("FInitInStockEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				this.SetDefaultOwner(i);
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x0000F810 File Offset: 0x0000DA10
		private bool GetStockFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string text = base.View.Model.GetValue("FBusinessType").ToString();
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FPRICELISTID":
			case "FDISCOUNTLISTID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue(this.priceDiscTaxArgs.SupplierOrCustomerKey) as DynamicObject;
				DynamicObject dynamicObject2 = base.View.Model.GetValue(this.priceDiscTaxArgs.Provider) as DynamicObject;
				DynamicObject dynamicObject3 = base.View.Model.GetValue(this.priceDiscTaxArgs.SettleCurrKey) as DynamicObject;
				string a = base.View.Model.GetValue(this.priceDiscTaxArgs.PricePoint).ToString();
				long supMasterId = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["msterId"]);
				long providerId = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				long currencyId = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
				string isIncludedTax = Convert.ToBoolean(base.View.Model.GetValue(this.priceDiscTaxArgs.IsIncludeTaxKey)) ? "1" : "0";
				int businessTypeChangePriceListPriceType = Common.GetBusinessTypeChangePriceListPriceType((string)base.View.Model.GetValue(this.priceDiscTaxArgs.BusinessType));
				PurPriceFilterArgs purPriceFilterArgs = new PurPriceFilterArgs();
				purPriceFilterArgs.IsIncludedTax = isIncludedTax;
				purPriceFilterArgs.SupMasterId = supMasterId;
				purPriceFilterArgs.ProviderId = providerId;
				purPriceFilterArgs.PriceType = businessTypeChangePriceListPriceType.ToString();
				purPriceFilterArgs.CurrencyId = currencyId;
				if (a == "2")
				{
					purPriceFilterArgs.BillDate = Convert.ToDateTime(base.View.Model.GetValue(this.priceDiscTaxArgs.DateKey));
				}
				else
				{
					purPriceFilterArgs.BillDate = TimeServiceHelper.GetSystemDateTime(base.View.Context);
				}
				if (fieldKey.ToUpperInvariant() == "FDISCOUNTLISTID")
				{
					DynamicObject dynamicObject4 = base.View.Model.GetValue(this.priceDiscTaxArgs.PriceListKey) as DynamicObject;
					long priceListId = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
					purPriceFilterArgs.PriceListId = priceListId;
					filter = Common.GetDiscountListFilter(base.Context, purPriceFilterArgs);
				}
				else
				{
					filter = Common.GetPriceListFilter(base.Context, purPriceFilterArgs);
				}
				break;
			}
			case "FMATERIALID":
			{
				string key2;
				switch (key2 = text)
				{
				case "CG":
				case "JSCG":
				case "LSCG":
					filter = " FISPURCHASE = '1' AND FISINVENTORY = '1' ";
					break;
				case "WW":
					if (!this.isCopyEntryOperating)
					{
						filter = " FISSUBCONTRACT = '1' AND FISINVENTORY = '1' ";
					}
					break;
				case "ZCCG":
					filter = " FISASSET = '1' AND FISINVENTORY = '1' AND FERPCLSID = '10' ";
					break;
				case "FYCG":
					filter = " FISPURCHASE = '1' AND FISINVENTORY = '1' AND FERPCLSID = '11'  ";
					break;
				case "VMICG":
					filter = "  FISPURCHASE = '1' AND FISINVENTORY = '1' AND FISVMIBUSINESS='1' ";
					break;
				}
				break;
			}
			case "FSTOCKID":
			{
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockStatusId", row) as DynamicObject;
				if (dynamicObject5 != null && !string.IsNullOrWhiteSpace(dynamicObject5["Type"].ToString()))
				{
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject5["Type"]);
				}
				break;
			}
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject6 = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				long num3 = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"]);
				if (num3 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num3.ToString();
				}
				break;
			}
			case "FPURCHASERID":
			{
				DynamicObject dynamicObject7 = base.View.Model.GetValue("FPurchaserGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				long num4 = (dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["Id"]);
				if (num4 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num4.ToString();
				}
				break;
			}
			case "FSTOCKERGROUPID":
			{
				DynamicObject dynamicObject8 = base.View.Model.GetValue("FStockerId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject8 != null && Convert.ToInt64(dynamicObject8["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject8["Id"]));
				}
				break;
			}
			case "FPURCHASERGROUPID":
			{
				DynamicObject dynamicObject9 = base.View.Model.GetValue("FPurchaserId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject9 != null && Convert.ToInt64(dynamicObject9["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject9["Id"]));
				}
				break;
			}
			case "FSUPPLIERID":
				filter = Common.GetSupplyClassifyFilter(text);
				break;
			case "FEXTAUXUNITID":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FAuxUnitID", row);
				break;
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000FE98 File Offset: 0x0000E098
		private void SetProductDate(int rowIndex)
		{
			int num = rowIndex;
			int num2 = rowIndex + 1;
			if (rowIndex < 0)
			{
				num = 0;
				num2 = base.View.Model.GetEntryRowCount("FInitInStockEntry");
			}
			for (int i = num; i < num2; i++)
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialID", rowIndex) as DynamicObject;
				if (dynamicObject == null)
				{
					return;
				}
				dynamicObject = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
				if (Convert.ToBoolean(dynamicObject["IsKFPeriod"]))
				{
					base.View.Model.SetValue("FPRODUCEDATE", DateTime.Today, rowIndex);
				}
				else
				{
					base.View.Model.SetValue("FPRODUCEDATE", null, rowIndex);
				}
				base.View.InvokeFieldUpdateService("FPRODUCEDATE", rowIndex);
			}
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0000FF70 File Offset: 0x0000E170
		private void PreparePriceDiscTaxArgs()
		{
			this.priceDiscTaxArgs.BillTypeKey = "FBillTypeID";
			this.priceDiscTaxArgs.BusinessType = "FBusinessType";
			this.priceDiscTaxArgs.DateKey = "FDate";
			this.priceDiscTaxArgs.DeptKey = "FPurchaseDeptId";
			this.priceDiscTaxArgs.DiscountKey = "FDiscount";
			this.priceDiscTaxArgs.EntityKey = "FInitInStockEntry";
			this.priceDiscTaxArgs.EntryTaxRateKey = "FEntryTaxRate";
			this.priceDiscTaxArgs.GroupKey = "FPurchaserGroupId";
			this.priceDiscTaxArgs.IsIncludeTaxKey = "FIsIncludedTax";
			this.priceDiscTaxArgs.MaterialKey = "FMaterialId";
			this.priceDiscTaxArgs.OrgKey = "FPurchaseOrgId";
			this.priceDiscTaxArgs.PriceCoefficientKey = "FPriceCoefficient";
			this.priceDiscTaxArgs.PriceKey = "FPrice";
			this.priceDiscTaxArgs.QtyKey = "FPriceUnitQty";
			this.priceDiscTaxArgs.Provider = "FSupplyId";
			this.priceDiscTaxArgs.PurchaserOrSalesmanKey = "FPurchaserId";
			this.priceDiscTaxArgs.SettleCurrKey = "FSettleCurrId";
			this.priceDiscTaxArgs.SupplierOrCustomerKey = "FSupplierId";
			this.priceDiscTaxArgs.TaxPriceKey = "FTaxPrice";
			this.priceDiscTaxArgs.SysPrice = "FSysPrice";
			this.priceDiscTaxArgs.LowerPrice = "FDownPrice";
			this.priceDiscTaxArgs.UpperPrice = "FUpPrice";
			this.priceDiscTaxArgs.PriceCoefficientKey = "FPriceCoefficient";
		}

		// Token: 0x0600010F RID: 271 RVA: 0x000100F0 File Offset: 0x0000E2F0
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
			List<long> approvedBomIdByOrgId = MFGServiceHelperForSCM.GetApprovedBomIdByOrgId(base.View.Context, num, value, dynamicObject2);
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

		// Token: 0x06000110 RID: 272 RVA: 0x000101C8 File Offset: 0x0000E3C8
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
		}

		// Token: 0x06000111 RID: 273 RVA: 0x000102A0 File Offset: 0x0000E4A0
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

		// Token: 0x0400005E RID: 94
		private bool isUseTaxCombination;

		// Token: 0x0400005F RID: 95
		private PriceDiscTaxArgs priceDiscTaxArgs = new PriceDiscTaxArgs();

		// Token: 0x04000060 RID: 96
		private bool isCopyEntryOperating;

		// Token: 0x04000061 RID: 97
		private long lastAuxpropId;

		// Token: 0x04000062 RID: 98
		private TaxService svc;
	}
}
