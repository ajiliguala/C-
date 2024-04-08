using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Business.Bill.Service.Tax;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Import;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG.SUB;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Business.PUR;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200009B RID: 155
	public class InStockEdit : AbstractBillPlugIn
	{
		// Token: 0x060008AE RID: 2222 RVA: 0x000718CD File Offset: 0x0006FACD
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			this.callSys = Convert.ToString(e.OpenParameter.SubSystemId);
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x000718E8 File Offset: 0x0006FAE8
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				if (StringUtils.EqualsIgnoreCase(this.callSys, "SUB"))
				{
					base.View.Model.SetValue("FBillTypeId", "0a2c1694596d440882adb080a7a8ca1b");
				}
				this.SetBusinessTypeByBillType();
				this.SetOwnerIdHeadByBusinessType();
				this.SetDefaultOwner();
				this.SetDefaultKeeper();
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
				long baseDataLongValue2 = SCMCommon.GetBaseDataLongValue(this, "FPurchaseOrgId", -1);
				if (baseDataLongValue2 > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FPurchaserId", "CGY", baseDataLongValue2);
				}
			}
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x000719DC File Offset: 0x0006FBDC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW"))
				{
					if (a == "COPYENTRYROW")
					{
						this.isCopyEntryOperating = true;
						return;
					}
					if (!(a == "MUTILASSOCIATEDCOPYENTRYROW"))
					{
						if (!(a == "SAVE"))
						{
							return;
						}
						if (base.View.OpenParameter.Status == 3)
						{
							if (this.isDisassemblySuccess)
							{
								base.View.ShowErrMessage(ResManager.LoadKDString("当前单据已经拆分成功，请到列表查看拆分的新单数据！", "004023000021303", 5, new object[0]), "", 0);
								e.Cancel = true;
							}
							this.dycBeforeDisData = (ObjectUtils.CreateCopy(base.View.Model.DataObject) as DynamicObject);
							DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["InStockEntry"] as DynamicObjectCollection;
							string text = "";
							int num = 0;
							int num2 = 0;
							for (int i = 0; i < dynamicObjectCollection.Count; i++)
							{
								decimal d = Convert.ToDecimal(dynamicObjectCollection[i]["PriceUnitQty"]);
								decimal d2 = Convert.ToDecimal(dynamicObjectCollection[i]["DisPriceQty"]);
								if (d2 < 0m || d2 > d)
								{
									text = ((text.Length == 0) ? Convert.ToString(i + 1) : (text + "," + Convert.ToString(i + 1)));
								}
								else if (d2 == d)
								{
									num2++;
								}
								else if (d2 == 0m)
								{
									num++;
								}
							}
							if (text.Length > 0)
							{
								base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录的拆单数量（计价单位）不合法：【不能大于原计价数量】或【不能为负数】！", "004023000021304", 5, new object[0]), text), "", 0);
								e.Cancel = true;
							}
							if (num == dynamicObjectCollection.Count)
							{
								base.View.ShowErrMessage(ResManager.LoadKDString("拆单数量（计价单位）不合法：【不允许所有分录行拆单数量都为0】！", "004023000021305", 5, new object[0]), "", 0);
								e.Cancel = true;
							}
							if (num2 == dynamicObjectCollection.Count)
							{
								base.View.ShowErrMessage(ResManager.LoadKDString("拆单数量（计价单位）不合法：【不允许所有分录行拆单数量都全部拆完】！", "004023000021306", 5, new object[0]), "", 0);
								e.Cancel = true;
							}
						}
					}
					else
					{
						this.isCopyEntryOperating = true;
						if (base.View.Session != null)
						{
							base.View.Session["mutilAssociatedCopyEntryRowing"] = "1";
							return;
						}
					}
				}
				else
				{
					int[] selectedRows = base.View.GetControl<EntryGrid>("FInStockEntry").GetSelectedRows();
					if (selectedRows != null && selectedRows.Count<int>() > 0 && selectedRows[0] != -1)
					{
						if (selectedRows.Count<int>() > 1)
						{
							base.View.ShowWarnningMessage(ResManager.LoadKDString("只能选择一行分录进行关联复制！", "004005000011993", 5, new object[0]), "", 0, null, 1);
							return;
						}
						this.isCopyEntryOperating = true;
						long num3 = Convert.ToInt64(this.Model.GetValue("fRECSUBENTRYID", selectedRows[0]));
						if (num3 == -1L || num3 == -2L)
						{
							this.isCopyEntryOperating = false;
							e.Cancel = true;
							base.View.ShowErrMessage(ResManager.LoadKDString("不能对此种收料质检入库进行关联复制！", "004005000011993", 5, new object[0]), "", 0);
							return;
						}
					}
				}
			}
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x00071D5C File Offset: 0x0006FF5C
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "TAKEREFERENCEPRICE"))
				{
					if (!(a == "ASSOCIATEDCOPYENTRYROW") && !(a == "COPYENTRYROW"))
					{
						if (!(a == "SAVE"))
						{
							return;
						}
						if (base.View.OpenParameter.Status != 3)
						{
							return;
						}
						if (e.OperationResult.IsSuccess)
						{
							this.isDisassemblySuccess = true;
							return;
						}
						if (this.dycBeforeDisData != null)
						{
							base.View.Model.DataObject = this.dycBeforeDisData;
							base.View.UpdateView();
							return;
						}
						return;
					}
				}
				else
				{
					new List<object>();
					List<long> list = new List<long>();
					List<long> list2 = new List<long>();
					List<long> list3 = new List<long>();
					List<long> list4 = new List<long>();
					using (TakeReferencePrice takeReferencePrice = new TakeReferencePrice(base.View.Context))
					{
						long num = 0L;
						long num2 = 0L;
						DynamicObject dataObject = base.View.Model.DataObject;
						DynamicObject dynamicObject = base.View.Model.GetValue("FSettleCurrId") as DynamicObject;
						if (dynamicObject != null)
						{
							num = Convert.ToInt64(dynamicObject["Id"]);
							list2.Add(num);
						}
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
						if (dynamicObject2 != null)
						{
							num2 = Convert.ToInt64(dynamicObject2["Id"]);
						}
						object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num2, "PUR_SystemParameter", "ReferencePriceSource", "1");
						string text = Convert.ToString(systemProfile);
						if (text == "1")
						{
							return;
						}
						list4.Add(num2);
						bool flag = Convert.ToBoolean(base.View.Model.GetValue("FIsIncludedTax"));
						DynamicObjectCollection dynamicObjectCollection = dataObject["InStockEntry"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
						{
							list3.Add(Convert.ToInt64(dynamicObject3["AuxPropId_Id"]));
							list.Add(Convert.ToInt64(dynamicObject3["MaterialId_Id"]));
						}
						DynamicObjectCollection referencePriceLists = takeReferencePrice.GetReferencePriceLists(base.View.Context, list, list2, list4, text);
						takeReferencePrice.TakeReferencePriceEdit(this, referencePriceLists, dynamicObjectCollection, num, flag);
						return;
					}
				}
				this.isCopyEntryOperating = false;
				return;
			}
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x0007200C File Offset: 0x0007020C
		public override void AfterBindData(EventArgs e)
		{
			if ((base.View.OpenParameter.CreateFrom == 1 || base.View.OpenParameter.CreateFrom == 2) && !this.hasTipPOHasNotAuditReturn && !(base.View is IImportView) && base.View.Context.ServiceType != 1 && base.View.ClientType != 32)
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["InStockEntry"] as DynamicObjectCollection;
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
				{
					List<long> list = new List<long>();
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						if (Convert.ToString(dynamicObject["SRCBILLTYPEID"]) == "PUR_PurchaseOrder")
						{
							list.Add(Convert.ToInt64(dynamicObject["POORDERENTRYID"]));
						}
					}
					if (list.Count > 0)
					{
						list = list.Distinct<long>().ToList<long>();
						if (PurchaseNewServiceHelper.CheckPOHasNotAuditReturn(base.Context, list))
						{
							base.View.ShowWarnningMessage(ResManager.LoadKDString("温馨提示：上游订单关联存在未审核的采购退料单，本次入库时将包含这部分退料数量！", "004023000025462", 5, new object[0]), "", 0, null, 1);
							this.hasTipPOHasNotAuditReturn = true;
						}
					}
				}
			}
			base.View.StyleManager.SetEnabled("FTaxCombination", "", this.isUseTaxCombination);
			base.View.StyleManager.SetEnabled("FIsIncludedTax", "", !this.isUseTaxCombination);
			if (StringUtils.EqualsIgnoreCase(this.callSys, "SUB"))
			{
				base.View.StyleManager.SetEnabled("FBillTypeId", "", false);
			}
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				this.SetPushAndSelBillCalculate();
			}
			if (base.View.OpenParameter.Status == 3)
			{
				this.SetEntryDisassemblyStatus();
			}
			this.UpdateAfterBindData();
			this.LockHeadPriceListByPurSystemParam();
			int entryRowCount = base.View.Model.GetEntryRowCount("FInStockEntry");
			if (entryRowCount <= 0)
			{
				base.View.GetControl("FSalOutStockBillNo").Visible = false;
				return;
			}
			long num = 0L;
			for (int i = 0; i < entryRowCount; i++)
			{
				num = Convert.ToInt64(base.View.Model.GetValue("FSalOutStockEntryId", i));
				if (num > 0L)
				{
					break;
				}
			}
			if (num > 0L)
			{
				base.View.GetControl("FSalOutStockBillNo").Visible = true;
				return;
			}
			base.View.GetControl("FSalOutStockBillNo").Visible = false;
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x000722E8 File Offset: 0x000704E8
		private void LockHeadPriceListByPurSystemParam()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FPurchaseOrgId") as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			long num = Convert.ToInt64(dynamicObject["Id"]);
			string text = Convert.ToString(CommonServiceHelper.GetSystemProfile(base.View.Context, num, "PUR_SystemParameter", "PriceBills", string.Empty));
			if (text.Contains(base.View.OpenParameter.FormId))
			{
				base.View.StyleManager.SetEnabled("FPriceListId", null, false);
			}
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x0007237C File Offset: 0x0007057C
		private void SetEntryDisassemblyStatus()
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["InStockEntry"] as DynamicObjectCollection;
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				DynamicObject dynamicObject = dynamicObjectCollection[i]["MaterialId"] as DynamicObject;
				DynamicObjectCollection dynamicObjectCollection2 = (dynamicObject != null) ? (dynamicObject["MaterialStock"] as DynamicObjectCollection) : null;
				if (dynamicObjectCollection2 != null && dynamicObject != null && Convert.ToBoolean(dynamicObjectCollection2[0]["IsSNManage"]))
				{
					base.View.GetFieldEditor("FDisPriceQty", i).Enabled = false;
				}
			}
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x00072424 File Offset: 0x00070624
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["InStockEntry"] as DynamicObjectCollection;
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				dynamicObjectCollection[i]["APNotJoinQty"] = Convert.ToDecimal(dynamicObjectCollection[i]["PriceUnitQty"]);
				DynamicObjectCollection dynamicObjectCollection2 = dynamicObjectCollection[i]["InStockPurCost"] as DynamicObjectCollection;
				dynamicObjectCollection2.Clear();
			}
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x000724B4 File Offset: 0x000706B4
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			decimal num = Convert.ToDecimal(base.View.Model.GetValue("FPriceUnitQty", e.Row));
			if (e.EntityKey.ToUpperInvariant() == "FINSTOCKENTRY")
			{
				base.View.Model.SetValue("FAPNotJoinQty", num, e.NewRow);
			}
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x0007251C File Offset: 0x0007071C
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

		// Token: 0x060008B8 RID: 2232 RVA: 0x000725C4 File Offset: 0x000707C4
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

		// Token: 0x060008B9 RID: 2233 RVA: 0x0007279C File Offset: 0x0007099C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			string empty3 = string.Empty;
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSUPPLIERID":
			case "FSUPPLYID":
				if (!this.isUseTaxCombination && StringUtils.EqualsIgnoreCase(e.Field.Key.ToUpperInvariant(), "FSUPPLIERID"))
				{
					for (int i = 0; i < base.View.Model.GetEntryRowCount("FInStockEntry"); i++)
					{
						this.SetDefaultTaxRate(this, this.priceDiscTaxArgs, i);
					}
				}
				this.DoSupplyChange(e);
				SCMCommon.SetDefProviderContactAndGetAddress(this, "FSupplyId", "FProviderContactID", "FSupplyAddress", "FSupplyEMail", ref empty, ref empty2, ref empty3);
				return;
			case "FPROVIDERCONTACTID":
				SCMCommon.SetContactAddressAndDeliveryEntryAddress(this, "FSupplyId", "FProviderContactID", "FSupplyAddress", "FSupplyEMail");
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
				this.SetExchangeRate();
				return;
			case "FDATE":
				this.SetExchRateWhenDateChanged();
				return;
			case "FPRICELISTID":
			{
				DynamicObject dynamicObject = e.NewValue as DynamicObject;
				if (dynamicObject == null || 0L == Convert.ToInt64(dynamicObject["Id"]))
				{
					for (int j = 0; j < base.View.Model.GetEntryRowCount(this.priceDiscTaxArgs.EntityKey); j++)
					{
						base.View.Model.SetValue(this.priceDiscTaxArgs.PriceCoefficientKey, 1, j);
					}
					return;
				}
				break;
			}
			case "FDISCOUNTRATE":
			{
				decimal d = ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue) ? 0m : Convert.ToDecimal(e.NewValue);
				decimal d2 = ObjectUtils.IsEmptyPrimaryKey(e.OldValue) ? 0m : Convert.ToDecimal(e.OldValue);
				if (d != d2)
				{
					Entity entity = base.View.BusinessInfo.GetEntity("FInStockEntry");
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
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject2 != null)
				{
					num2 = Convert.ToInt64(dynamicObject2["Id"]);
				}
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject3, 0L, false, num2, false), e.Row);
				this.SetDefaultKeeper(e.Row);
				this.SetDefaultOwner(e.Row);
				PurBusCommon.SetRowTypeByMat(this, dynamicObject3, e.Row);
				return;
			}
			case "FPRICEUNITID":
			{
				bool flag = Convert.ToBoolean(this.Model.GetValue("FIsIncludedTax"));
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
				if (dynamicObject4 != null && e.NewValue != null && e.OldValue != null)
				{
					long num3 = Convert.ToInt64(e.NewValue);
					long num4 = Convert.ToInt64(e.OldValue);
					long num5 = Convert.ToInt64(dynamicObject4["Id"]);
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
			case "FPURCHASEORGID":
				this.LockHeadPriceListByPurSystemParam();
				return;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				return;
			}
			case "FSTOCKID":
			{
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockId", e.Row) as DynamicObject;
				base.View.Model.SetValue("FStockStatusID", null, e.Row);
				SCMCommon.TakeDefaultStockStatusOther(this, "FStockStatusID", dynamicObject5, e.Row, "'0','1','2','3','4','5','6','7','8'");
				break;
			}

				return;
			}
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x00072DA8 File Offset: 0x00070FA8
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

		// Token: 0x060008BB RID: 2235 RVA: 0x00072F14 File Offset: 0x00071114
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
				this.bChangeFromSupplier = true;
				base.View.Model.SetValue("FSettleCurrId", (obj3 != null) ? ((DynamicObject)obj3)["Id"] : 0);
				this.SetExchangeRate();
				this.bChangeFromSupplier = false;
				base.View.Model.SetValue("FPayConditionId", (obj != null) ? ((DynamicObject)obj)["Id"] : null);
				base.View.Model.SetValue("FSettleTypeId", (obj2 != null) ? ((DynamicObject)obj2)["Id"] : null);
				return;
			}
			base.View.Model.SetValue("FSettleCurrId", null);
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x00073148 File Offset: 0x00071348
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TB_BACKFLUSH")
				{
					this.ShowBackFlush();
					return;
				}
				if (a == "TBEXPAPPORTION")
				{
					this.ShowExpenseApportion();
					return;
				}
				if (!(a == "TBPUSH"))
				{
					return;
				}
				object value = base.View.Model.GetValue("FISGENFORIOS");
				if (ObjectUtils.IsNullOrEmpty(value))
				{
					return;
				}
				bool flag = Convert.ToBoolean(value);
				if (flag)
				{
					e.Cancel = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("内部交易单据不允许下推！", "004023000010995", 5, new object[0]), "", 0);
				}
			}
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x000731F4 File Offset: 0x000713F4
		public override void AfterUpdateViewState(EventArgs e)
		{
			this.UpdateAfterBindData();
			base.View.GetControl("FPTaxRate").Visible = !this.isUseTaxCombination;
			base.View.GetControl("FPTaxCombination").Visible = this.isUseTaxCombination;
			base.View.GetControl("FPFTaxPurePrice").Visible = !this.isUseTaxCombination;
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x00073260 File Offset: 0x00071460
		private void UpdateAfterBindData()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeId");
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(baseDataStringValue) && StringUtils.EqualsIgnoreCase(baseDataStringValue, "5b91410d323043f3b4f3a7079aad3c68"))
			{
				base.View.GetMainBarItem("tbTakeReferencePrice").Enabled = true;
				return;
			}
			base.View.GetMainBarItem("tbTakeReferencePrice").Enabled = false;
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x000732BC File Offset: 0x000714BC
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
			case "FPRICELISTID":
			case "FDISCOUNTLISTID":
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
				break;
			}

				return;
			}
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x00073494 File Offset: 0x00071694
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
			case "FPRICELISTID":
			case "FDISCOUNTLISTID":
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
				break;
			}

				return;
			}
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x00073688 File Offset: 0x00071888
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			if (this.isDisCountCanGo)
			{
				SCMCommon.SetAllotHolisticDiscountOperation(e, base.View, ref this.isDisCountCanGo);
				if (e.Cancel)
				{
					return;
				}
			}
			else
			{
				this.isDisCountCanGo = true;
			}
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x000736B4 File Offset: 0x000718B4
		public override void BeforeBindData(EventArgs e)
		{
			base.View.GetControl("FPTaxRate").Visible = !this.isUseTaxCombination;
			base.View.GetControl("FPTaxCombination").Visible = this.isUseTaxCombination;
			base.View.GetControl("FPFTaxPurePrice").Visible = !this.isUseTaxCombination;
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x000738A4 File Offset: 0x00071AA4
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FPRICETIMEPOINT") && !(a == "FSETTLECURRID") && !(a == "FDATE") && !(a == "FPRICELISTID"))
				{
					return;
				}
				if (!this.afterShowMessage)
				{
					if (e.Key == this.priceDiscTaxArgs.DateKey && Convert.ToString(base.View.Model.GetValue(this.priceDiscTaxArgs.PricePoint)) != "2")
					{
						return;
					}
					bool isClearPriceList = false;
					bool isClearDiscList = false;
					bool flag = false;
					bool flag2 = false;
					string text;
					Common.ChangePriceDiscListRelated(this, this.priceDiscTaxArgs, e, out text, out isClearPriceList, out isClearDiscList);
					DynamicObject dynamicObject = base.View.Model.GetValue(this.priceDiscTaxArgs.PriceListKey) as DynamicObject;
					long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
					if ((e.Key == this.priceDiscTaxArgs.DateKey || e.Key == this.priceDiscTaxArgs.PricePoint) && !isClearPriceList && num > 0L)
					{
						flag = SCMCommon.IsReGetPrice(this, this.priceDiscTaxArgs, e.Key, e.Value);
						text += (flag ? string.Format(ResManager.LoadKDString("日期超出价目表分录有效期，将会重新取价。{0}", "004023030002116", 5, new object[0]), Environment.NewLine) : string.Empty);
					}
					DynamicObject dynamicObject2 = base.View.Model.GetValue(this.priceDiscTaxArgs.DiscountListKey) as DynamicObject;
					long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
					if ((e.Key == this.priceDiscTaxArgs.DateKey || e.Key == this.priceDiscTaxArgs.PricePoint) && !isClearDiscList && num2 > 0L)
					{
						flag2 = SCMCommon.IsReGetDiscount(this, this.priceDiscTaxArgs, "InStockEntry", e.Key, e.Value);
						text += (flag2 ? string.Format(ResManager.LoadKDString("日期超出折扣表分录有效期，将会重新取折扣。{0}", "004023030002119", 5, new object[0]), Environment.NewLine) : string.Empty);
					}
					if (SCMCommon.GetBaseDataStringValue(this, "FBillTypeID") != "5b91410d323043f3b4f3a7079aad3c68")
					{
						if (isClearDiscList)
						{
							base.View.Model.SetValue(this.priceDiscTaxArgs.DiscountListKey, null);
							base.View.InvokeFieldUpdateService(this.priceDiscTaxArgs.DiscountListKey, 0);
						}
						if (isClearPriceList)
						{
							base.View.Model.SetValue(this.priceDiscTaxArgs.PriceListKey, null);
							base.View.InvokeFieldUpdateService(this.priceDiscTaxArgs.PriceListKey, 0);
						}
						return;
					}
					if ((isClearPriceList || isClearDiscList || flag || flag2) && !this.bChangeFromSupplier)
					{
						e.Cancel = true;
						text = string.Format(ResManager.LoadKDString("{0}{1}是否确认修改？", "004023030002122", 5, new object[0]), text, Environment.NewLine);
						base.View.ShowMessage(text, 1, delegate(MessageBoxResult result)
						{
							this.afterShowMessage = true;
							if (1 == result)
							{
								this.View.Model.SetValue(e.Key, e.Value, e.Row);
								this.View.InvokeFieldUpdateService(e.Key, e.Row);
								if (isClearDiscList)
								{
									this.View.Model.SetValue(this.priceDiscTaxArgs.DiscountListKey, null);
									this.View.InvokeFieldUpdateService(this.priceDiscTaxArgs.DiscountListKey, 0);
								}
								if (isClearPriceList)
								{
									this.View.Model.SetValue(this.priceDiscTaxArgs.PriceListKey, null);
									this.View.InvokeFieldUpdateService(this.priceDiscTaxArgs.PriceListKey, 0);
								}
							}
							this.afterShowMessage = false;
						}, "", 1);
					}
				}
			}
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x00073C7F File Offset: 0x00071E7F
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.PreparePriceDiscTaxArgs();
			this.isUseTaxCombination = SystemParameterServiceHelper.IsUseTaxCombination(base.Context);
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x00073CB0 File Offset: 0x00071EB0
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation == 12 && !ListUtils.IsEmpty<ConvertBillElement>(list))
			{
				list = (from w in list
				where !StringUtils.EqualsIgnoreCase(w.FormID, "QT_LotSNRelation")
				select w).ToList<ConvertBillElement>();
			}
			e.Bills = list;
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x00073D14 File Offset: 0x00071F14
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			if (StringUtils.EqualsIgnoreCase(e.BarItemKey, "tbLotSNR"))
			{
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FInStockEntry");
				DynamicObject selObj = InStockEdit.GetSelectedRowDatas(base.View, entryEntity.Key).FirstOrDefault<DynamicObject>();
				this.ProcessLotSNR(selObj);
			}
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x00073D70 File Offset: 0x00071F70
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			if (e.FieldKey.ToUpperInvariant() == "FSALOUTSTOCKBILLNO")
			{
				if (e.Row < 0)
				{
					return;
				}
				string value = BillUtils.GetValue<string>(base.View.Model, "FSalOutStockBillNo", e.Row, null, null);
				string text = Convert.ToString(base.View.Model.GetValue("FSalOutStockEntryId", e.Row));
				if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(text))
				{
					PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
					{
						Id = "SAL_OUTSTOCK"
					}, "6e44119a58cb4a8e86f6c385e14a17ad");
					if (!permissionAuthResult.Passed)
					{
						base.View.ShowMessage(ResManager.LoadKDString("你没有当前单据查看权限，请设置！", "004104000013505", 5, new object[0]), 0);
						return;
					}
					ListShowParameter listShowParameter = new ListShowParameter();
					listShowParameter.ListFilterParameter = new ListRegularFilterParameter
					{
						Filter = string.Format(" t2.FENTRYID={0} ", text)
					};
					listShowParameter.FormId = "SAL_OUTSTOCK";
					listShowParameter.IsLookUp = false;
					listShowParameter.IsIsolationOrg = false;
					base.View.ShowForm(listShowParameter);
				}
				e.Cancel = true;
			}
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x00073FAC File Offset: 0x000721AC
		public override void ToolBarItemClick(BarItemClickEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.BarItemKey, "TBPAYABLECLOSE"))
			{
				PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
				{
					Id = "STK_InStock"
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
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("finstockentry");
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
					if (1 == CommonServiceHelper.UpdateBillPayableCloseStatus(this.Context, "t_stk_instockentry_i", Convert.ToInt64(currentRow["id"]), "B", dateTime))
					{
						this.View.Model.SetValue("FPayableCloseStatus", "B", currentRowIndex);
						this.View.Model.SetValue("FPayableCloseDate", dateTime, currentRowIndex);
						this.View.Model.DataChanged = false;
						this.View.ShowMessage(ResManager.LoadKDString("应付关闭成功！", "004023000023762", 5, new object[0]), 0);
					}
				});
			}
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x000742A8 File Offset: 0x000724A8
		public static IEnumerable<DynamicObject> GetSelectedRowDatas(IBillView view, string entryKey)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			EntryEntity entryEntity = view.BusinessInfo.GetEntryEntity(entryKey);
			int[] selectedRows = view.GetControl<EntryGrid>(entryEntity.Key).GetSelectedRows();
			DynamicObjectCollection entityDataObject = view.Model.GetEntityDataObject(entryEntity);
			foreach (int num in selectedRows)
			{
				if (num >= 0)
				{
					list.Add(entityDataObject[num]);
				}
			}
			return list;
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x00074318 File Offset: 0x00072518
		private void ProcessLotSNR(DynamicObject selObj)
		{
			long num = Convert.ToInt64(selObj["Id"]);
			int num2 = Convert.ToInt32(selObj["Seq"]);
			ListSelectedRow listSelectedRow = new ListSelectedRow(BillUtils.GetValue<long>(base.View.Model, "FID", -1, 0L, null).ToString(), num.ToString(), num2, "STK_InStock");
			Tuple<List<DynamicObject>, long> lotSNRBySrcBillInfo = MFGServiceHelperForSCM.GetLotSNRBySrcBillInfo(base.Context, "STK_InStock", num, listSelectedRow);
			BillShowParameter billShowParameter = new BillShowParameter();
			if (lotSNRBySrcBillInfo.Item2 > 0L)
			{
				billShowParameter.PKey = lotSNRBySrcBillInfo.Item2.ToString();
				billShowParameter.Status = 2;
			}
			else if (!ListUtils.IsEmpty<DynamicObject>(lotSNRBySrcBillInfo.Item1))
			{
				billShowParameter.CustomComplexParams.Add("LotSNR", lotSNRBySrcBillInfo.Item1);
				billShowParameter.Status = 0;
			}
			billShowParameter.FormId = "QT_LotSNRelation";
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			billShowParameter.OpenStyle.ShowType = 4;
			billShowParameter.CustomParams.Add("showbeforesave", "1");
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x00074458 File Offset: 0x00072658
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x000744BC File Offset: 0x000726BC
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x000744F4 File Offset: 0x000726F4
		private void SetOwnerIdHeadByBusinessType()
		{
			string a = Convert.ToString(base.View.Model.GetValue("FBusinessType", 0));
			if (a != "VMICG")
			{
				object value = base.View.Model.GetValue("FDemandOrgId");
				base.View.Model.SetValue("FOwnerTypeIdHead", "BD_OwnerOrg");
				base.View.Model.SetValue("FOwnerIdHead", value);
				return;
			}
			base.View.Model.SetValue("FOwnerTypeIdHead", "BD_Supplier");
			base.View.Model.SetValue("FOwnerIdHead", base.View.Model.GetValue("FSupplierId"));
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x000745B8 File Offset: 0x000727B8
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

		// Token: 0x060008CF RID: 2255 RVA: 0x000746D4 File Offset: 0x000728D4
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "PUR_InStockParam", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBusinessType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x00074724 File Offset: 0x00072924
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

		// Token: 0x060008D1 RID: 2257 RVA: 0x00074868 File Offset: 0x00072A68
		public void ShowExpenseApportion()
		{
			if (base.View.Model.GetPKValue() != null)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.ParentPageId = base.View.PageId;
				dynamicFormShowParameter.MultiSelect = false;
				dynamicFormShowParameter.FormId = "SCM_ExpenseApportion";
				dynamicFormShowParameter.Height = 400;
				dynamicFormShowParameter.Width = 600;
				base.View.ShowForm(dynamicFormShowParameter);
				return;
			}
			base.View.ShowMessage(ResManager.LoadKDString("请先保存物料!", "004023030000229", 5, new object[0]), 0, "", 0);
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x000748FC File Offset: 0x00072AFC
		private void SetDefLocalCurrencyAndExchangeType()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("1", "FSettleOrgId", "FExchangeTypeId", "FLocalCurrId", "FSettleCurrId", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
			this.SetExchangeRate();
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x00074940 File Offset: 0x00072B40
		private void SetDefaultOwner(int rowIndex)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FBusinessType"));
			if (StringUtils.EqualsIgnoreCase(text, "VMICG"))
			{
				base.View.Model.SetValue("FOwnerTypeId", "BD_Supplier", rowIndex);
				base.View.Model.SetValue("FOwnerId", base.View.Model.GetValue("FOwnerIdHead"), rowIndex);
				return;
			}
			base.View.Model.SetValue("FOwnerTypeId", "BD_OwnerOrg", rowIndex);
			base.View.Model.SetValue("FOwnerId", base.View.Model.GetValue("FOwnerIdHead"), rowIndex);
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x00074A04 File Offset: 0x00072C04
		private void SetDefaultKeeper(int rowIndex)
		{
			base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", rowIndex);
			base.View.Model.SetValue("FKeeperId", base.View.Model.GetValue("FStockOrgId"), rowIndex);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x00074A58 File Offset: 0x00072C58
		private void SetDefaultOwner()
		{
			int entryRowCount = this.Model.GetEntryRowCount("FInStockEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				this.SetDefaultOwner(i);
			}
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x00074A8C File Offset: 0x00072C8C
		private void SetDefaultKeeper()
		{
			int entryRowCount = this.Model.GetEntryRowCount("FInStockEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				this.SetDefaultKeeper(i);
			}
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x00074AC0 File Offset: 0x00072CC0
		private void SetOwnerIdAndKeeperIdWhenSelBill()
		{
			base.View.Model.GetValue("FSRCBillTypeId", 0);
			object value = base.View.Model.GetValue("FSRCBillNo", 0);
			if (value != null && value.ToString().Length > 0)
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FInStockEntry");
				int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FInStockEntry");
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerId", i) as DynamicObject;
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FKeeperId", i) as DynamicObject;
					this.Model.SetEntryCurrentRowIndex("FInStockEntry", i);
					if (dynamicObject2 == null || Convert.ToInt64(dynamicObject2["Id"]) == 0L)
					{
						base.View.Model.SetValue("FOwnerTypeId", "BD_OwnerOrg", i);
						base.View.Model.SetValue("FOwnerId", num, i);
					}
					if (dynamicObject3 == null || Convert.ToInt64(dynamicObject3["Id"]) == 0L)
					{
						base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", i);
						base.View.Model.SetValue("FKeeperId", num, i);
					}
				}
				this.Model.SetEntryCurrentRowIndex("FInStockEntry", entryCurrentRowIndex);
			}
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x00074C8C File Offset: 0x00072E8C
		private bool GetStockFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string text = Convert.ToString(base.View.Model.GetValue("FBusinessType"));
			string key;
			if ((key = fieldKey.ToUpperInvariant()) != null)
			{
				if (<PrivateImplementationDetails>{41CA3D9E-4578-4F96-BF53-45396DDC784D}.$$method0x600075e-1 == null)
				{
					<PrivateImplementationDetails>{41CA3D9E-4578-4F96-BF53-45396DDC784D}.$$method0x600075e-1 = new Dictionary<string, int>(10)
					{
						{
							"FPRICELISTID",
							0
						},
						{
							"FDISCOUNTLISTID",
							1
						},
						{
							"FMATERIALID",
							2
						},
						{
							"FSTOCKID",
							3
						},
						{
							"FSTOCKERID",
							4
						},
						{
							"FPURCHASERID",
							5
						},
						{
							"FSTOCKERGROUPID",
							6
						},
						{
							"FPURCHASERGROUPID",
							7
						},
						{
							"FSUPPLIERID",
							8
						},
						{
							"FEXTAUXUNITID",
							9
						}
					};
				}
				int num;
				if (<PrivateImplementationDetails>{41CA3D9E-4578-4F96-BF53-45396DDC784D}.$$method0x600075e-1.TryGetValue(key, out num))
				{
					switch (num)
					{
					case 0:
					case 1:
					{
						DynamicObject dynamicObject = base.View.Model.GetValue(this.priceDiscTaxArgs.SupplierOrCustomerKey) as DynamicObject;
						DynamicObject dynamicObject2 = base.View.Model.GetValue(this.priceDiscTaxArgs.Provider) as DynamicObject;
						DynamicObject dynamicObject3 = base.View.Model.GetValue(this.priceDiscTaxArgs.SettleCurrKey) as DynamicObject;
						string a = Convert.ToString(base.View.Model.GetValue(this.priceDiscTaxArgs.PricePoint));
						long supMasterId = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["msterId"]);
						long providerId = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
						long currencyId = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
						string isIncludedTax = Convert.ToBoolean(base.View.Model.GetValue(this.priceDiscTaxArgs.IsIncludeTaxKey)) ? "1" : "0";
						int businessTypeChangePriceListPriceType = Common.GetBusinessTypeChangePriceListPriceType(Convert.ToString(base.View.Model.GetValue(this.priceDiscTaxArgs.BusinessType)));
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
							goto IL_7FF;
						}
						filter = Common.GetPriceListFilter(base.Context, purPriceFilterArgs);
						goto IL_7FF;
					}
					case 2:
					{
						string key2;
						switch (key2 = text)
						{
						case "CG":
							filter = this.AppendComingCheckByIsCopyEntryOperating(" FISPURCHASE = '1' AND ( FISINVENTORY = '1' OR FErpClsID = '6' ) AND FERPCLSID NOT IN ('10')  ");
							break;
						case "JSCG":
						case "LSCG":
							filter = this.AppendComingCheckByIsCopyEntryOperating(" FISPURCHASE = '1' AND ( FISINVENTORY = '1' OR FErpClsID = '6' ) ");
							break;
						case "WW":
							if (!this.isCopyEntryOperating)
							{
								filter = " FISSUBCONTRACT = '1' AND ( FISINVENTORY = '1' OR FErpClsID = '6' ) ";
								bool flag = false;
								if (base.View.Session != null && base.View.Session.ContainsKey("mutilAssociatedCopyEntryRowing"))
								{
									flag = (Convert.ToString(base.View.Session["mutilAssociatedCopyEntryRowing"]) == "1");
								}
								if (base.Context.ServiceType != 1 && !flag)
								{
									filter += " AND FCheckIncoming='0' ";
								}
							}
							break;
						case "ZCCG":
							filter = this.AppendComingCheckByIsCopyEntryOperating(" FISASSET = '1' AND ( FISINVENTORY = '1' OR FErpClsID = '6' ) AND ( FERPCLSID = '10' OR FErpClsID = '6' ) ");
							break;
						case "FYCG":
							filter = this.AppendComingCheckByIsCopyEntryOperating(" FISPURCHASE = '1' AND ( FISINVENTORY = '1' OR FErpClsID = '6' ) AND ( FERPCLSID = '11' OR FErpClsID = '6' ) ");
							break;
						case "VMICG":
							filter = this.AppendComingCheckByIsCopyEntryOperating(" FISPURCHASE = '1' AND ( FISINVENTORY = '1' OR FErpClsID = '6' ) AND FISVMIBUSINESS='1'  ");
							break;
						case "DRPSALE":
							filter = this.AppendComingCheckByIsCopyEntryOperating("");
							break;
						}
						try
						{
							string materialListFilter = this.GetMaterialListFilter();
							if (string.IsNullOrEmpty(filter))
							{
								filter = materialListFilter;
							}
							else if (materialListFilter.Length > 0)
							{
								filter = filter + " AND " + materialListFilter;
							}
							goto IL_7FF;
						}
						catch (Exception ex)
						{
							Logger.Error("SCM", "获得物料列表的过滤条件", ex);
							goto IL_7FF;
						}
						break;
					}
					case 3:
						break;
					case 4:
					{
						DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
						filter += " FIsUse='1' ";
						long num3 = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
						if (num3 != 0L)
						{
							filter = filter + "And FOPERATORGROUPID = " + num3.ToString();
							goto IL_7FF;
						}
						goto IL_7FF;
					}
					case 5:
					{
						DynamicObject dynamicObject6 = base.View.Model.GetValue("FPurchaserGroupId") as DynamicObject;
						filter += " FIsUse='1' ";
						long num4 = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"]);
						if (num4 != 0L)
						{
							filter = filter + "And FOPERATORGROUPID = " + num4.ToString();
							goto IL_7FF;
						}
						goto IL_7FF;
					}
					case 6:
					{
						DynamicObject dynamicObject7 = base.View.Model.GetValue("FStockerId") as DynamicObject;
						filter += " FIsUse='1' ";
						if (dynamicObject7 != null && Convert.ToInt64(dynamicObject7["Id"]) > 0L)
						{
							filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject7["Id"]));
							goto IL_7FF;
						}
						goto IL_7FF;
					}
					case 7:
					{
						DynamicObject dynamicObject8 = base.View.Model.GetValue("FPurchaserId") as DynamicObject;
						filter += " FIsUse='1' ";
						if (dynamicObject8 != null && Convert.ToInt64(dynamicObject8["Id"]) > 0L)
						{
							filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject8["Id"]));
							goto IL_7FF;
						}
						goto IL_7FF;
					}
					case 8:
					{
						filter = Common.GetSupplyClassifyFilter(text);
						try
						{
							string supplierListFilter = this.GetSupplierListFilter();
							if (string.IsNullOrEmpty(filter))
							{
								filter = supplierListFilter;
							}
							else if (supplierListFilter.Length > 0)
							{
								filter = filter + " AND " + supplierListFilter;
							}
						}
						catch (Exception ex2)
						{
							Logger.Error("SCM", "获得供应商列表的过滤条件", ex2);
						}
						string text2 = Convert.ToString(base.View.Model.GetValue("FBusinessType"));
						if (!StringUtils.EqualsIgnoreCase(text2, "DRPSALE"))
						{
							goto IL_7FF;
						}
						if (ObjectUtils.IsNullOrEmpty(filter))
						{
							filter = " FCORRESPONDORGID <> 0 ";
							goto IL_7FF;
						}
						filter += " AND  FCORRESPONDORGID <> 0 ";
						goto IL_7FF;
					}
					case 9:
						filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FAuxUnitID", row);
						goto IL_7FF;
					default:
						goto IL_7FF;
					}
					DynamicObject dynamicObject9 = base.View.Model.GetValue("FStockStatusId", row) as DynamicObject;
					if (dynamicObject9 != null && !string.IsNullOrWhiteSpace(Convert.ToString(dynamicObject9["Type"])))
					{
						filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject9["Type"]);
					}
				}
			}
			IL_7FF:
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x000754C0 File Offset: 0x000736C0
		private string AppendComingCheckByIsCopyEntryOperating(string filter)
		{
			bool flag = false;
			if (base.View.Session != null && base.View.Session.ContainsKey("mutilAssociatedCopyEntryRowing"))
			{
				flag = (Convert.ToString(base.View.Session["mutilAssociatedCopyEntryRowing"]) == "1");
			}
			if (base.Context.ServiceType != 1 && !this.isCopyEntryOperating && !flag)
			{
				if (!string.IsNullOrWhiteSpace(filter))
				{
					filter += "  AND FCheckIncoming='0' ";
				}
				else
				{
					filter = "  FCheckIncoming='0' ";
				}
			}
			return filter;
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x00075550 File Offset: 0x00073750
		private void SetProductDate(int rowIndex)
		{
			int num = rowIndex;
			int num2 = rowIndex + 1;
			if (rowIndex < 0)
			{
				num = 0;
				num2 = base.View.Model.GetEntryRowCount("FInStockEntry");
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

		// Token: 0x060008DB RID: 2267 RVA: 0x00075628 File Offset: 0x00073828
		private void PreparePriceDiscTaxArgs()
		{
			this.priceDiscTaxArgs.BillTypeKey = "FBillTypeID";
			this.priceDiscTaxArgs.BusinessType = "FBusinessType";
			this.priceDiscTaxArgs.DateKey = "FDate";
			this.priceDiscTaxArgs.DeptKey = "FPurchaseDeptId";
			this.priceDiscTaxArgs.DiscountKey = "FDiscount";
			this.priceDiscTaxArgs.DiscountListKey = "FDiscountListId";
			this.priceDiscTaxArgs.EntityKey = "FInStockEntry";
			this.priceDiscTaxArgs.EntryTaxRateKey = "FEntryTaxRate";
			this.priceDiscTaxArgs.GroupKey = "FPurchaserGroupId";
			this.priceDiscTaxArgs.IsIncludeTaxKey = "FIsIncludedTax";
			this.priceDiscTaxArgs.MaterialKey = "FMaterialId";
			this.priceDiscTaxArgs.OrgKey = "FPurchaseOrgId";
			this.priceDiscTaxArgs.PriceCoefficientKey = "FPriceCoefficient";
			this.priceDiscTaxArgs.PriceKey = "FPrice";
			this.priceDiscTaxArgs.PriceListKey = "FPriceListId";
			this.priceDiscTaxArgs.PricePoint = "FPriceTimePoint";
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

		// Token: 0x060008DC RID: 2268 RVA: 0x000757D8 File Offset: 0x000739D8
		private void ComputeTax(DynamicObject parentObj, TaxDetailSubEntryEntity subEntity, int row)
		{
			decimal num = 1m;
			decimal d = 0m;
			decimal num2 = 0m;
			int num3 = 0;
			decimal d2 = 0m;
			decimal num4 = Convert.ToDecimal(parentObj[base.View.BillBusinessInfo.GetField(subEntity.RelateQty).PropertyName]);
			decimal d3 = Convert.ToDecimal(parentObj[base.View.BillBusinessInfo.GetField(subEntity.RelateUnitPrice).PropertyName]);
			if (!string.IsNullOrWhiteSpace(subEntity.RelateCostCoefficient))
			{
				num = Convert.ToDecimal(parentObj[base.View.BillBusinessInfo.GetField(subEntity.RelateCostCoefficient).PropertyName]);
				if (num == 0m)
				{
					num = 1m;
				}
			}
			if (!string.IsNullOrWhiteSpace(subEntity.RelateRowDiscount))
			{
				d = Convert.ToDecimal(parentObj[base.View.BillBusinessInfo.GetField(subEntity.RelateRowDiscount).PropertyName]);
			}
			DynamicObject dyn = (DynamicObject)parentObj[base.View.BillBusinessInfo.GetField(subEntity.RelateTaxCombination).PropertyName];
			if (num4 == 0m || d3 == 0m)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)parentObj[subEntity.EntryName];
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				decimal d4 = 0m;
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					decimal d5 = Convert.ToDecimal(dynamicObject[Extension.GetValue(subEntity.EntityNameMaps, "TaxRate")]) / 100m;
					bool flag = Convert.ToBoolean(dynamicObject[Extension.GetValue(subEntity.EntityNameMaps, "SellerWithholding")]);
					string curTaxId = dynamicObject[Extension.GetValue(subEntity.EntityNameMaps, "TaxRateId") + "_Id"].ToString();
					this.GetTaxBenchMarkAndtaxBenchMarkCorrValue(dyn, curTaxId, out num3, out d2);
					decimal num5;
					if (num3 == 0)
					{
						num5 = (d3 / num * num4 - d) * d5;
					}
					else
					{
						num5 = ((d3 + d4 / num4) / num * num4 - d) * d5;
					}
					num5 *= ++d2;
					if (flag)
					{
						num2 += num5;
					}
					dynamicObject[Extension.GetValue(subEntity.EntityNameMaps, "TaxAmount")] = num5;
					dynamicObject[Extension.GetValue(subEntity.EntityNameMaps, "CostAmount")] = num5 * Convert.ToDecimal(dynamicObject[Extension.GetValue(subEntity.EntityNameMaps, "CostPercent")]) / 100.0m;
					d4 += num5;
				}
			}
			base.View.Model.SetValue(subEntity.RelateTaxAmount, num2, row);
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x00075B60 File Offset: 0x00073D60
		private void GetTaxBenchMarkAndtaxBenchMarkCorrValue(DynamicObject dyn, string curTaxId, out int taxBenchMark, out decimal taxBenchMarkCorrValue)
		{
			taxBenchMark = 0;
			taxBenchMarkCorrValue = 0m;
			if (dyn != null)
			{
				DynamicObjectCollection dynamicObjectCollection = dyn["BD_TAXMIXENTRY"] as DynamicObjectCollection;
				if (dynamicObjectCollection != null)
				{
					IEnumerable<DynamicObject> source = from p in dynamicObjectCollection
					where p["TaxRateId_Id"].ToString() == curTaxId
					select p;
					DynamicObject dynamicObject = source.FirstOrDefault<DynamicObject>();
					if (dynamicObject != null)
					{
						if (((DynamicObject)dynamicObject["TaxBenchMark"])["Id"].ToString().Equals("d78902778db94e969604fbf464edcb31", StringComparison.InvariantCultureIgnoreCase))
						{
							taxBenchMark = 0;
						}
						else
						{
							taxBenchMark = 1;
						}
						if (dynamicObject["TaxBenchMarkCorrValue"] != null && string.IsNullOrEmpty(dynamicObject["TaxBenchMarkCorrValue"].ToString()))
						{
							taxBenchMarkCorrValue = Convert.ToDecimal(dynamicObject["TaxBenchMarkCorrValue"]);
						}
					}
				}
			}
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x00075C40 File Offset: 0x00073E40
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

		// Token: 0x060008DF RID: 2271 RVA: 0x00075D34 File Offset: 0x00073F34
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
			base.View.UpdateView("FInStockEntry", row);
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x00075E20 File Offset: 0x00074020
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

		// Token: 0x060008E1 RID: 2273 RVA: 0x00075EB0 File Offset: 0x000740B0
		private void SetExchRateWhenDateChanged()
		{
			bool flag = false;
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["InStockEntry"] as DynamicObjectCollection;
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (!string.IsNullOrWhiteSpace(Convert.ToString(dynamicObject["SRCBillNo"])))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject2 == null)
				{
					return;
				}
				long num = Convert.ToInt64(dynamicObject2["Id"]);
				bool flag2 = Convert.ToBoolean(CommonServiceHelper.GetSystemProfile(base.Context, num, "PUR_SystemParameter", "SrcInstocGetCurrExRate", false));
				if (flag2)
				{
					this.SetExchangeRate();
					return;
				}
			}
			else
			{
				this.SetExchangeRate();
			}
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x00075FA4 File Offset: 0x000741A4
		private void ShowBackFlush()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = "SUB_BackFlush"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有委外倒冲领料工作台的查看权限！", "004023030002125", 5, new object[0]), "", 0);
				return;
			}
			string text = "b," + string.Join(",", new List<string>
			{
				Convert.ToString(base.View.Model.GetPKValue())
			});
			SUBBackFlushSearchParam subbackFlushSearchParam = new SUBBackFlushSearchParam();
			subbackFlushSearchParam.StartTime = KDTimeZone.MinSystemDateTime;
			subbackFlushSearchParam.EndTime = TimeServiceHelper.GetSystemDateTime(base.View.Context);
			subbackFlushSearchParam.BillType = "STK_InStock";
			subbackFlushSearchParam.Option = OperateOption.Create();
			subbackFlushSearchParam.Option.SetVariableValue("ids", text);
			List<DynamicObject> subBackFlushItems = MFGServiceHelperForSCM.GetSubBackFlushItems(base.Context, subbackFlushSearchParam);
			if (subBackFlushItems.Count <= 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前单据没有对应的倒冲物料！", "004023030002131", 5, new object[0]), "", 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "SUB_BackFlush";
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.CustomParams.Add("ids", text);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060008E3 RID: 2275 RVA: 0x00076110 File Offset: 0x00074310
		public bool ChangeCataLogSupportAuxProp()
		{
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject == null)
			{
				return false;
			}
			long num = Convert.ToInt64(dynamicObject["Id"]);
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num, "PUR_SystemParameter", "CataLogSupportAuxProp", false);
			return systemProfile != null && Convert.ToBoolean(systemProfile);
		}

		// Token: 0x060008E4 RID: 2276 RVA: 0x00076178 File Offset: 0x00074378
		private List<DynamicObject> GetEnableCataLogSupportOrPriceTableByPurcharseBill(bool useCahe = false)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FBillTypeID") as DynamicObject;
			DynamicObject dynamicObject2 = this.Model.GetValue("FStockOrgId") as DynamicObject;
			string text = "6";
			long num;
			if (dynamicObject2 == null)
			{
				num = 0L;
			}
			else
			{
				num = Convert.ToInt64(dynamicObject2["Id"]);
			}
			List<DynamicObject> result = new List<DynamicObject>();
			if (num > 0L && dynamicObject != null)
			{
				result = PurchaseServiceHelper.GetEnableCataLogSupportOrPriceTableEntitysByBillType(base.Context, num, text, Convert.ToString(dynamicObject["Id"]), useCahe);
			}
			return result;
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x00076208 File Offset: 0x00074408
		private string GetMaterialListFilter()
		{
			string text = string.Empty;
			List<DynamicObject> enableCataLogSupportOrPriceTableByPurcharseBill = this.GetEnableCataLogSupportOrPriceTableByPurcharseBill(false);
			bool flag = false;
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, "PUR_USERORDERPARAM", true) as FormMetadata;
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.View.Context, formMetadata.BusinessInfo, base.View.Context.UserId, "PUR_PurchaseOrder", "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("FiltBySupplyList") && dynamicObject["FiltBySupplyList"] != null && !string.IsNullOrWhiteSpace(dynamicObject["FiltBySupplyList"].ToString()))
			{
				flag = Convert.ToBoolean(dynamicObject["FiltBySupplyList"]);
			}
			if (flag || (enableCataLogSupportOrPriceTableByPurcharseBill != null && enableCataLogSupportOrPriceTableByPurcharseBill.Count > 0))
			{
				DynamicObject dynamicObject2 = this.Model.GetValue("FStockOrgId") as DynamicObject;
				DynamicObject dynamicObject3 = this.Model.GetValue("FSupplierId") as DynamicObject;
				DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
				DateTime.TryParse(Convert.ToString(this.Model.GetValue("FDate")), out systemDateTime);
				long num;
				if (dynamicObject2 == null)
				{
					num = 0L;
				}
				else
				{
					num = Convert.ToInt64(dynamicObject2["Id"]);
				}
				long num2;
				if (dynamicObject3 == null)
				{
					num2 = 0L;
				}
				else
				{
					num2 = Convert.ToInt64(dynamicObject3["Id"]);
				}
				if (enableCataLogSupportOrPriceTableByPurcharseBill != null && enableCataLogSupportOrPriceTableByPurcharseBill.Count > 0)
				{
					if (num2 > 0L)
					{
						int num3 = Convert.ToInt32(enableCataLogSupportOrPriceTableByPurcharseBill[0]["ControlSource"]);
						int num4 = Convert.ToInt32(enableCataLogSupportOrPriceTableByPurcharseBill[0]["ControlStrength"]);
						if (num3 == 1)
						{
							bool flag2 = PurchaseServiceHelper.CheckMateriaExistPurseCatelog(base.Context, num2, num, systemDateTime);
							if (num4 == 2 || flag2)
							{
								text = " (exists ( select  1 from  ( select distinct  b.FMATERIALID from t_pur_catalog a\r\n                                     inner join t_pur_catalogentry b on a.fid=b.fid and b.FETYSUPPLIERID={0}\r\n                                     where a.FPURCHASEORGID={1} and a.{3} and a.FFORBIDSTATUS='A' and b.FFORBIDSTATUS='A' \r\n                                     and b.FEFFECTIVEDATE<={2} and b.FEXPIRYDATE>={2} ) c where c.FMATERIALID=FMATERIALID)) ";
								string text2 = "{ts '" + systemDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'}";
								text = string.Format(text, new object[]
								{
									num2,
									num,
									text2,
									Common.GetUsefulPurCatalogDocStatusFilter(base.Context)
								});
							}
						}
						else
						{
							bool flag3 = PurchaseServiceHelper.CheckMateriaExistPurchasePriceList(base.Context, num2, num, systemDateTime);
							bool flag4 = PurchaseServiceHelper.IsBaseDataShare(base.Context, "BD_Supplier");
							if (num4 == 2 || flag3)
							{
								text = " (exists ( select  1 from  (select distinct t1.FMATERIALID from T_PUR_PRICELIST t inner join T_PUR_PRICELISTENTRY t1 on t.fid=t1.fid \r\n                              where \r\n                              t1.FEFFECTIVEDATE<={2} \r\n                              and t1.FEXPIRYDATE>{2}\r\n                              and t.FFORBIDSTATUS='A'\r\n                              and t1.FROWAUDITSTATUS='A'\r\n                              and t.FDOCUMENTSTATUS='C'\r\n                              and t1. FDISABLESTATUS='B'\r\n                              and t.FCREATEORGID={1}\r\n                              and t.FSUPPLIERID={0}\r\n                              UNION\r\n                              select distinct t4.FMATERIALID from T_PUR_PRICELIST t \r\n\t\t\t\t\t\t\t  inner join T_PUR_PRICELISTENTRY t1 on t.fid=t1.fid \r\n                              inner join T_PUR_PRICELIST_ISSUE t2 on t2.FID=t.FID\r\n                              inner join T_BD_SUPPLIER t3 on t3.FMASTERID=t.FSUPPLIERMASTERID {3}\r\n\t\t\t\t\t\t\t  inner join T_BD_MATERIAL t4 on t4.FMASTERID=t1.FMMASTERID and t4.FUSEORGID=t2.FISSUEORGID \r\n                              where \r\n                              t1.FEFFECTIVEDATE<={2} \r\n                              and t1.FEXPIRYDATE>{2}\r\n                              and t.FFORBIDSTATUS='A'\r\n                              and t1.FROWAUDITSTATUS='A'\r\n                              and t.FDOCUMENTSTATUS='C'\r\n                              and t1. FDISABLESTATUS='B'\r\n                              and t2.FISSUEORGID={1}\r\n                              and t3.FSUPPLIERID={0}\r\n                               ) c where c.FMATERIALID=FMATERIALID)) ";
								string text3 = "{ts '" + systemDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'}";
								text = string.Format(text, new object[]
								{
									num2,
									num,
									text3,
									flag4 ? "" : "and t3.FUSEORGID=t2.FISSUEORGID"
								});
							}
						}
					}
				}
				else if (flag)
				{
					text = " (exists ( select  1 from  ( select distinct  b.FMATERIALID from t_pur_catalog a\r\n                                     inner join t_pur_catalogentry b on a.fid=b.fid and b.FETYSUPPLIERID={0}\r\n                                     where a.FPURCHASEORGID={1} and a.{3} and a.FFORBIDSTATUS='A' and b.FFORBIDSTATUS='A' \r\n                                     and b.FEFFECTIVEDATE<={2} and b.FEXPIRYDATE>={2} ) c where c.FMATERIALID=FMATERIALID)) ";
					string text4 = "{ts '" + systemDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'}";
					text = string.Format(text, new object[]
					{
						num2,
						num,
						text4,
						Common.GetUsefulPurCatalogDocStatusFilter(base.Context)
					});
				}
			}
			return text;
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x00076554 File Offset: 0x00074754
		private string GetSupplierListFilter()
		{
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FInStockEntry");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (entityDataObject.Count((DynamicObject n) => n["MATERIALID"] != null) <= 0)
			{
				return "";
			}
			List<DynamicObject> enableCataLogSupportOrPriceTableByPurcharseBill = this.GetEnableCataLogSupportOrPriceTableByPurcharseBill(false);
			string text = string.Empty;
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			this.GetDic();
			long num;
			if (dynamicObject == null)
			{
				num = 0L;
			}
			else
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			if (enableCataLogSupportOrPriceTableByPurcharseBill != null && enableCataLogSupportOrPriceTableByPurcharseBill.Count > 0 && num > 0L)
			{
				int num2 = Convert.ToInt32(enableCataLogSupportOrPriceTableByPurcharseBill[0]["ControlSource"]);
				int num3 = Convert.ToInt32(enableCataLogSupportOrPriceTableByPurcharseBill[0]["ControlStrength"]);
				if (num2 == 1)
				{
					List<string> intersectList = Common.GetIntersectList(this.MaterialSupplierDic);
					if (num3 == 2 || intersectList.Count > 0)
					{
						string arg = string.Join(",", intersectList);
						if (intersectList.Count <= 0)
						{
							text = "1=2";
						}
						else
						{
							text = " FSupplierId in ({0})";
							text = string.Format(text, arg);
						}
					}
				}
				else
				{
					List<string> intersectList2 = Common.GetIntersectList(this.PriceListMaterialSupplierDic);
					if (num3 == 2 || intersectList2.Count > 0)
					{
						string arg2 = string.Join(",", intersectList2);
						if (intersectList2.Count <= 0)
						{
							text = "1=2";
						}
						else
						{
							text = " FSupplierId in ({0})";
							text = string.Format(text, arg2);
						}
					}
				}
			}
			return text;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x00076700 File Offset: 0x00074900
		private void GetDic()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			List<DynamicObject> enableCataLogSupportOrPriceTableByPurcharseBill = this.GetEnableCataLogSupportOrPriceTableByPurcharseBill(false);
			bool flag = this.ChangeCataLogSupportAuxProp();
			long num;
			if (dynamicObject == null)
			{
				num = 0L;
			}
			else
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			int entryRowCount = base.View.Model.GetEntryRowCount("FInStockEntry");
			if (entryRowCount > 0)
			{
				Entity entity = base.View.Model.BusinessInfo.GetEntity("FInStockEntry");
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
				if (entityDataObject.Count((DynamicObject n) => n["MATERIALID"] != null) <= 0)
				{
					return;
				}
				if (enableCataLogSupportOrPriceTableByPurcharseBill != null && enableCataLogSupportOrPriceTableByPurcharseBill.Count > 0)
				{
					DateTime now = DateTime.Now;
					DateTime.TryParse(Convert.ToString(this.Model.GetValue("FDate")), out now);
					if (enableCataLogSupportOrPriceTableByPurcharseBill[0]["ControlSource"].ToString() == "1")
					{
						DynamicObjectCollection suppliersFromCateLog = PurchaseServiceHelper.GetSuppliersFromCateLog(base.Context, entityDataObject, num, now, flag);
						this.MaterialSupplierDic.Clear();
						if (suppliersFromCateLog != null)
						{
							foreach (DynamicObject dynamicObject2 in suppliersFromCateLog)
							{
								if (dynamicObject2 != null && !(dynamicObject2["isEnable"].ToString() == "0"))
								{
									int key = Convert.ToInt32(dynamicObject2["rowNumber"]);
									string item = dynamicObject2["FETYSUPPLIERID"].ToString();
									if (!this.MaterialSupplierDic.ContainsKey(key))
									{
										this.MaterialSupplierDic[key] = new List<string>();
									}
									this.MaterialSupplierDic[key].Add(item);
								}
							}
							for (int i = 0; i < entityDataObject.Count; i++)
							{
								if (entityDataObject[i]["MATERIALID"] != null && !this.MaterialSupplierDic.ContainsKey(i + 1))
								{
									this.MaterialSupplierDic[i + 1] = new List<string>();
								}
							}
							return;
						}
					}
					else
					{
						DynamicObjectCollection suppliersFromPurchsePriceList = PurchaseServiceHelper.GetSuppliersFromPurchsePriceList(base.Context, entityDataObject, num, now, flag);
						this.PriceListMaterialSupplierDic.Clear();
						if (suppliersFromPurchsePriceList != null)
						{
							foreach (DynamicObject dynamicObject3 in suppliersFromPurchsePriceList)
							{
								if (dynamicObject3 != null && !(dynamicObject3["isEnable"].ToString() == "0"))
								{
									int key2 = Convert.ToInt32(dynamicObject3["rowNumber"]);
									string item2 = dynamicObject3["FSUPPLIERID"].ToString();
									if (!this.PriceListMaterialSupplierDic.ContainsKey(key2))
									{
										this.PriceListMaterialSupplierDic[key2] = new List<string>();
									}
									this.PriceListMaterialSupplierDic[key2].Add(item2);
								}
							}
							for (int j = 0; j < entityDataObject.Count; j++)
							{
								if (entityDataObject[j]["MATERIALID"] != null && !this.PriceListMaterialSupplierDic.ContainsKey(j + 1))
								{
									this.PriceListMaterialSupplierDic[j + 1] = new List<string>();
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x00076A84 File Offset: 0x00074C84
		private void SetBillOpenParam(string billNo, string formId, ref BillShowParameter billShowPara)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = formId
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowMessage(ResManager.LoadKDString("你没有当前单据查看权限，请设置！", "004072000039101", 5, new object[0]), 0);
				return;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			string pkey = "";
			string defaultBillTypeId = "";
			if (billNo.Trim().Length > 0)
			{
				billNo = string.Format(" '{0}'", billNo);
				IQueryService service = ServiceFactory.GetService<IQueryService>(base.Context);
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = formId,
					SelectItems = SelectorItemInfo.CreateItems("FBILLNO,FID,FBILLTYPEID"),
					FilterClauseWihtKey = string.Format(" FBILLNO IN ({0}) ", billNo)
				};
				DynamicObjectCollection dynamicObjectCollection = service.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (dynamicObject != null)
					{
						list.Add(Convert.ToString(dynamicObject["FID"]));
						list2.Add(Convert.ToString(dynamicObject["FBILLTYPEID"]));
					}
				}
			}
			if (list.Count > 0)
			{
				pkey = string.Join(",", list.ToArray());
				defaultBillTypeId = list2[0];
			}
			billShowPara.FormId = formId;
			billShowPara.PKey = pkey;
			billShowPara.DefaultBillTypeId = defaultBillTypeId;
		}

		// Token: 0x04000366 RID: 870
		private bool isUseTaxCombination;

		// Token: 0x04000367 RID: 871
		private bool hasTipPOHasNotAuditReturn;

		// Token: 0x04000368 RID: 872
		private bool afterShowMessage;

		// Token: 0x04000369 RID: 873
		private string callSys = "";

		// Token: 0x0400036A RID: 874
		private bool bChangeFromSupplier;

		// Token: 0x0400036B RID: 875
		private PriceDiscTaxArgs priceDiscTaxArgs = new PriceDiscTaxArgs();

		// Token: 0x0400036C RID: 876
		private bool isCopyEntryOperating;

		// Token: 0x0400036D RID: 877
		private bool isDisassemblySuccess;

		// Token: 0x0400036E RID: 878
		private DynamicObject dycBeforeDisData;

		// Token: 0x0400036F RID: 879
		private long lastAuxpropId;

		// Token: 0x04000370 RID: 880
		private bool isDisCountCanGo = true;

		// Token: 0x04000371 RID: 881
		private TaxService svc;

		// Token: 0x04000372 RID: 882
		private Dictionary<int, List<string>> MaterialSupplierDic = new Dictionary<int, List<string>>();

		// Token: 0x04000373 RID: 883
		private Dictionary<int, List<string>> PriceListMaterialSupplierDic = new Dictionary<int, List<string>>();
	}
}
