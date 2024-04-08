using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
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
	// Token: 0x0200009F RID: 159
	[Description("其他出库单 表单插件")]
	public class MisDeliveryEdit : AbstractBillPlugIn
	{
		// Token: 0x06000967 RID: 2407 RVA: 0x0007DBAA File Offset: 0x0007BDAA
		public override void OnInitialize(InitializeEventArgs e)
		{
			this._baseDataOrgCtl = Common.GetSalBaseDataCtrolType(base.View.Context);
			base.OnInitialize(e);
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x0007DBCC File Offset: 0x0007BDCC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW"))
				{
					if (a == "COPYENTRYROW")
					{
						this.copyEntryRow = true;
					}
				}
				else
				{
					this.associatedCopyEntryRow = true;
				}
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x0007DC28 File Offset: 0x0007BE28
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW"))
				{
					if (a == "COPYENTRYROW")
					{
						this.copyEntryRow = false;
					}
				}
				else
				{
					this.associatedCopyEntryRow = false;
				}
			}
			base.AfterDoOperation(e);
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x0007DC7C File Offset: 0x0007BE7C
		public override void BeforeBindData(EventArgs e)
		{
			if (!StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "4"))
			{
				this.SetKPTypeAndKPAfterSave();
			}
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x0007DCAC File Offset: 0x0007BEAC
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "2"))
			{
				this.defaultStock = Common.GetDefaultVMIStock(this, "FOWNERIDHEAD", "FStockId", "0,6", false);
			}
			this.GetUseCustMatMappingParamater();
			base.View.GetControl("FCustMatID").Visible = (this.para_UseCustMatMapping && this.para_BillTypeUseCustMatMapping);
			base.View.GetControl("FCustMatName").Visible = (this.para_UseCustMatMapping && this.para_BillTypeUseCustMatMapping);
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x0007DD54 File Offset: 0x0007BF54
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetDefCurrency();
				this.SetBusinessTypeByBillType();
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				if (baseDataLongValue > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
				}
			}
			if (base.View.OpenParameter.CreateFrom == 1)
			{
				GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrId", "", "FOwnerTypeIdHead", "FOwnerIdHead");
				SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
			}
			this.GetUseCustMatMappingParamater();
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x0007DDFE File Offset: 0x0007BFFE
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetDefKeeperTypeAndKeeperValue(e.Row, "NewRow");
			}
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x0007DE28 File Offset: 0x0007C028
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			if (!this.ClearZeroRow())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x0007DE40 File Offset: 0x0007C040
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FOWNERIDHEAD":
			case "FEXTAUXUNITID":
			case "FOWNERID":
			case "FCUSTMATID":
			{
				string lotF8InvFilter;
				if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				string lotF8InvFilter;
				if (this.GetStockStatusFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FPRODUCTGROUPID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FPickOrgId") as DynamicObject;
				int orgId = (dynamicObject == null) ? 0 : Convert.ToInt32(dynamicObject["Id"]);
				string lotF8InvFilter;
				if (Common.GetProductGroupFieldFilter(orgId, out lotF8InvFilter))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FLOT":
			{
				string value = BillUtils.GetValue<string>(this.Model, "FStockDirect", -1, null, null);
				if (!string.IsNullOrWhiteSpace(value) && value.Equals("GENERAL"))
				{
					string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
					{
						MaterialFieldKey = "FMaterialId",
						StockOrgFieldKey = "FStockOrgId",
						OwnerTypeFieldKey = "FOwnerTypeId",
						OwnerFieldKey = "FOwnerId",
						KeeperTypeFieldKey = "FKeeperTypeId",
						KeeperFieldKey = "FKeeperId",
						AuxpropFieldKey = "FAuxPropId",
						BomFieldKey = "FBomId",
						StockFieldKey = "FStockId",
						StockLocFieldKey = "FStockLocId",
						StockStatusFieldKey = "FStockStatusId",
						MtoFieldKey = "FMtoNo",
						ProjectFieldKey = "FProjectNo"
					}, e.Row);
					if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = lotF8InvFilter;
							return;
						}
						IRegularFilterParameter listFilterParameter4 = e.ListFilterParameter;
						listFilterParameter4.Filter = listFilterParameter4.Filter + " AND " + lotF8InvFilter;
					}
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x0007E194 File Offset: 0x0007C394
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FOWNERIDHEAD":
			case "FEXTAUXUNITID":
			case "FOWNERID":
			case "FCUSTMATID":
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
				string text;
				if (this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text, e.Row))
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
			case "FPICKORGID":
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "4"))
				{
					if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOWNERTYPEIDHEAD")), "BD_OwnerOrg"))
					{
						DynamicObject dynamicObject = base.View.Model.GetValue("FOWNERIDHEAD") as DynamicObject;
						long num2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
						e.QueryBuilderParemeter.FilterClauseWihtKey = string.Format(" (FORGID = {0}) ", num2);
						return;
					}
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					long num3 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
					e.QueryBuilderParemeter.FilterClauseWihtKey = string.Format(" (FORGID = {0}) ", num3);
					return;
				}
				break;
			case "FPRODUCTGROUPID":
			{
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FPickOrgId") as DynamicObject;
				int orgId = (dynamicObject3 == null) ? 0 : Convert.ToInt32(dynamicObject3["Id"]);
				string text;
				if (Common.GetProductGroupFieldFilter(orgId, out text))
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

		// Token: 0x06000971 RID: 2417 RVA: 0x0007E488 File Offset: 0x0007C688
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSTOCKORGID":
				this.SetDefCurrency();
				return;
			case "FMATERIALID":
			{
				this.SetDefOwnerAndKeeperValue(e.Row);
				this.SetDefKeeperTypeAndKeeperValue(e.Row, "DataChange");
				long num2 = 0L;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
				string text = Convert.ToString(base.View.Model.GetValue("FBizType"));
				if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L && StringUtils.EqualsIgnoreCase(text, "2"))
				{
					DynamicObjectCollection dynamicObjectCollection = dynamicObject2["MaterialStock"] as DynamicObjectCollection;
					long num3 = Convert.ToInt64(dynamicObjectCollection[0]["StockId_Id"]);
					base.View.Model.SetValue("FStockId", num3, e.Row);
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockId", e.Row) as DynamicObject;
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FOWNERIDHEAD") as DynamicObject;
					if (dynamicObject4 != null)
					{
						Convert.ToInt64(dynamicObject4["Id"]);
					}
					if (dynamicObject3 == null || Convert.ToInt64(dynamicObject3["Id"]) == 0L || this.defaultStock != 0L)
					{
						base.View.Model.SetValue("FStockId", this.defaultStock, e.Row);
					}
					else
					{
						base.View.Model.SetValue("FStockLocID", dynamicObjectCollection[0]["StockPlaceId_Id"], e.Row);
					}
				}
				if (this.para_UseCustMatMapping && this.para_BillTypeUseCustMatMapping)
				{
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FCustId") as DynamicObject;
					long materialId = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
					DynamicObject dynamicObject6 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
					string text2 = (dynamicObject6 != null) ? Convert.ToString(dynamicObject6["Id"]) : "";
					bool flag = base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id);
					if ((flag && ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2)) || (!flag && !this.associatedCopyEntryRow && !this.copyEntryRow))
					{
						DynamicObject dynamicObject7 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
						long customerId = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
						long saleOrgId = (dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["Id"]);
						Common.SetRelativeCodeByMaterialId(this, "FCustMatId", materialId, customerId, saleOrgId, e.Row);
						return;
					}
				}
				break;
			}
			case "FOWNERIDHEAD":
			{
				string text3 = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeper(text3);
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "2"))
				{
					this.defaultStock = Common.GetDefaultVMIStock(this, "FOWNERIDHEAD", "FStockId", "0,6", true);
				}
				this.SetPickOrgControl(base.View.Model.GetValue("FOWNERTYPEIDHEAD").ToString(), text3);
				return;
			}
			case "FOWNERTYPEIDHEAD":
				if (!StringUtils.EqualsIgnoreCase(e.NewValue.ToString(), "BD_OwnerOrg"))
				{
					DynamicObject dynamicObject8 = base.View.Model.GetValue("FOWNERIDHEAD") as DynamicObject;
					long num4 = (dynamicObject8 == null) ? 0L : Convert.ToInt64(dynamicObject8["Id"]);
					this.SetPickOrgControl(e.NewValue.ToString(), num4.ToString());
				}
				Common.SynOwnerType(this, "FOwnerTypeIdHead", "FOwnerTypeId");
				return;
			case "FKEEPERTYPEID":
				if (!StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "4"))
				{
					string newKeeperTypeId = Convert.ToString(e.NewValue);
					this.SetKeeperValue(newKeeperTypeId, e.Row);
					return;
				}
				break;
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
				return;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				return;
			}
			case "FCUSTID":
				if (this.para_UseCustMatMapping && this.para_BillTypeUseCustMatMapping)
				{
					Common.SetCustMatWhenCustChange(this, "FEntity", "FStockOrgId", "FCUSTID", "FCUSTMATID", "FCustMatName");
					return;
				}
				break;
			case "FCUSTMATID":
				if (!this.copyEntryRow)
				{
					if (this.associatedCopyEntryRow)
					{
						return;
					}
					CustomerMaterialMappingArgs customerMaterialMappingArgs = new CustomerMaterialMappingArgs();
					DynamicObject dynamicObject9 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
					customerMaterialMappingArgs.CustMatId = ((dynamicObject9 == null) ? "" : dynamicObject9["Id"].ToString());
					DynamicObject dynamicObject10 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					customerMaterialMappingArgs.MainOrgId = ((dynamicObject10 == null) ? 0L : Convert.ToInt64(dynamicObject10["Id"]));
					customerMaterialMappingArgs.NeedOrgCtrl = this._baseDataOrgCtl["BD_MATERIAL"];
					customerMaterialMappingArgs.MaterialIdKey = "FMaterialId";
					customerMaterialMappingArgs.AuxpropIdKey = "FAuxpropId";
					customerMaterialMappingArgs.Row = e.Row;
					Common.SetMaterialIdAndAuxpropIdByCustMatId(this, customerMaterialMappingArgs);
				}
				break;

				return;
			}
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x0007EB80 File Offset: 0x0007CD80
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x0007EBE4 File Offset: 0x0007CDE4
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x0007EC1C File Offset: 0x0007CE1C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBSYNOWNER")
				{
					Common.SynHeadOwner(this, "FEntity", "FOwnerIdHead", "FOwnerId");
					return;
				}
				if (!(a == "TBSTOCKSPLIT"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("分仓", "004023030009277", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					base.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.SplitMisBill("StockId_Id");
			}
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0007ECDC File Offset: 0x0007CEDC
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 12)
			{
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				if (list != null && list.Count > 0)
				{
					e.Bills = (from c in list
					where !c.FormID.Equals("FA_CARD", StringComparison.OrdinalIgnoreCase)
					select c).ToList<ConvertBillElement>();
					return;
				}
			}
			else if (e.ConvertOperation == 13 && e.Bills is List<ConvertBillElement>)
			{
				long value = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
				string text = Convert.ToString(base.View.Model.GetValue("FBizType"));
				if (value > 0L && !text.Equals("4") && !text.Equals("1") && !text.Equals("3") && Common.HaveBOMViewPermission(base.Context, value))
				{
					Common.SetBomExpandBillToConvertForm(base.Context, (List<ConvertBillElement>)e.Bills);
				}
			}
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0007EDEC File Offset: 0x0007CFEC
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

		// Token: 0x06000977 RID: 2423 RVA: 0x0007EEC4 File Offset: 0x0007D0C4
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				Common.DoBomExpandDraw(base.View, Common.GetBomExpandBillFieldValue(base.View, "FStockOrgId", "", ""));
				base.View.UpdateView("FEntity");
				base.View.Model.DataChanged = true;
				this.SetKPTypeAndKPAfterSave();
			}
		}

		// Token: 0x06000978 RID: 2424 RVA: 0x0007EF68 File Offset: 0x0007D168
		private bool ClearZeroRow()
		{
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && Convert.ToBoolean(parameterData["IsClearZeroRow"]))
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["BillEntry"] as DynamicObjectCollection;
				int num = dynamicObjectCollection.Count - 1;
				for (int i = num; i >= 0; i--)
				{
					if (dynamicObjectCollection[i]["MaterialId"] != null && Convert.ToDecimal(dynamicObjectCollection[i]["Qty"]) == 0m)
					{
						this.Model.DeleteEntryRow("FEntity", i);
					}
				}
				if (this.Model.GetEntryRowCount("FEntity") == 0)
				{
					base.View.ShowErrMessage("", ResManager.LoadKDString("分录“明细”是必填项。", "004023000021872", 5, new object[0]), 0);
					return false;
				}
				base.View.UpdateView("FEntity");
			}
			return true;
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x0007F060 File Offset: 0x0007D260
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

		// Token: 0x0600097A RID: 2426 RVA: 0x0007F154 File Offset: 0x0007D354
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
			base.View.UpdateView("FEntity", row);
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x0007F240 File Offset: 0x0007D440
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

		// Token: 0x0600097C RID: 2428 RVA: 0x0007F2D0 File Offset: 0x0007D4D0
		private void SetDefCurrency()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrId", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x0007F310 File Offset: 0x0007D510
		private void SetDefOwnerAndKeeperValue(int row = -1)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			int num2 = row;
			int num3 = row;
			if (row == -1)
			{
				num2 = 0;
				num3 = base.View.Model.GetEntryRowCount("FEntity") - 1;
			}
			for (int i = num2; i <= num3; i++)
			{
				object value = this.Model.GetValue("FOwnerTypeId", i);
				if (value != null)
				{
					string text = value.ToString();
					this.Model.SetItemValueByNumber("FOwnerId", "", i);
					if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_OwnerOrg") && num > 0L)
					{
						this.Model.SetValue("FOwnerId", num.ToString(), i);
						base.View.GetFieldEditor("FOwnerId", row).Enabled = true;
					}
				}
				value = this.Model.GetValue("FKeeperTypeId", i);
				if (value != null)
				{
					string text = value.ToString();
					this.Model.SetItemValueByNumber("FKeeperId", "", i);
					if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_KeeperOrg") && num > 0L)
					{
						this.Model.SetValue("FKeeperId", num.ToString(), i);
						base.View.GetFieldEditor("FKeeperId", row).Enabled = true;
					}
				}
			}
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x0007F494 File Offset: 0x0007D694
		private void SetDefKeeperTypeAndKeeperValue(int row, string sType)
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			base.View.Model.SetValue("FOwnerTypeId", value, row);
			if (!StringUtils.EqualsIgnoreCase(sType, "NewRow"))
			{
				base.View.Model.SetValue("FOwnerId", num, row);
			}
			if (num == num2 && a == "BD_OwnerOrg")
			{
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", row);
				base.View.Model.SetValue("FKeeperId", num2, row);
				base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = true;
				base.View.GetFieldEditor("FKeeperId", row).Enabled = true;
				return;
			}
			if (!StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "4"))
			{
				base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = false;
				base.View.GetFieldEditor("FKeeperId", row).Enabled = false;
			}
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x0007F648 File Offset: 0x0007D848
		private void SetKeeperTypeAndKeeper(string newOwerValue)
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long value2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			string text = Convert.ToString(value2);
			string text2 = Convert.ToString(base.View.Model.GetValue("FBizType"));
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			if (newOwerValue == text && a == "BD_OwnerOrg")
			{
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					base.View.Model.SetValue("FOwnerTypeId", value, i);
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, i);
					}
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = true;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = true;
				}
				return;
			}
			for (int j = 0; j < entryRowCount; j++)
			{
				base.View.Model.SetValue("FOwnerTypeId", value, j);
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", j) as DynamicObject;
				if (a == "BD_OwnerOrg")
				{
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, j);
					}
				}
				else if (!string.IsNullOrEmpty(newOwerValue) && !newOwerValue.Equals("0"))
				{
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FOwnerId", j) as DynamicObject;
					long num = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
					if (num == 0L && base.View.GetFieldEditor("FOwnerId", j).Enabled && dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, j);
					}
				}
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", j);
				base.View.Model.SetValue("FKeeperId", text, j);
				if (!StringUtils.EqualsIgnoreCase(text2, "4"))
				{
					base.View.GetFieldEditor("FKeeperTypeId", j).Enabled = false;
					base.View.GetFieldEditor("FKeeperId", j).Enabled = false;
				}
			}
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x0007F968 File Offset: 0x0007DB68
		private void SetKeeperValue(string newKeeperTypeId, int row)
		{
			object value = base.View.Model.GetValue("FOWNERTYPEIDHEAD");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FOWNERIDHEAD") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			if (num == num2 && a == "BD_OwnerOrg")
			{
				base.View.GetFieldEditor("FKEEPERID", row).Enabled = true;
				return;
			}
			base.View.GetFieldEditor("FKEEPERID", row).Enabled = false;
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0007FA50 File Offset: 0x0007DC50
		private bool GetStockFieldFilter(string fieldKey, out string filter, int row)
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
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "2") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead")), "BD_Supplier"))
				{
					filter = Common.getVMIOwnerFilter();
				}
				break;
			case "FOWNERID":
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "2") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId", row)), "BD_Supplier"))
				{
					filter = Common.getVMIOwnerFilter();
				}
				break;
			case "FSTOCKID":
			{
				string arg = string.Empty;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockStatusId", row) as DynamicObject;
				arg = ((dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Number"]));
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FType"));
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = "BD_StockStatus",
					FilterClauseWihtKey = string.Format("FNumber='{0}'", arg),
					SelectItems = list
				};
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
				if (dynamicObjectCollection.Count > 0)
				{
					DynamicObject dynamicObject2 = dynamicObjectCollection[0];
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject2["FType"]);
				}
				break;
			}
			case "FMATERIALID":
			{
				filter = " FISINVENTORY = '1'";
				string text = base.View.Model.GetValue("FBizType") as string;
				if (text == "1")
				{
					filter += " AND FISASSET = '1' ";
				}
				else if (StringUtils.EqualsIgnoreCase(text, "2"))
				{
					filter += " and FIsVmiBusiness = '1' ";
				}
				else if (StringUtils.EqualsIgnoreCase(text, "3"))
				{
					filter += " AND FErpClsID='11' ";
				}
				break;
			}
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				long num2 = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
				if (num2 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num2.ToString();
				}
				break;
			}
			case "FSTOCKERGROUPID":
			{
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FStockerId") as DynamicObject;
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
			case "FCUSTMATID":
			{
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				long mainOrgId = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
				DynamicObject dynamicObject6 = base.View.Model.GetValue("FCustId") as DynamicObject;
				long custId = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"]);
				DynamicObject dynamicObject7 = base.View.Model.GetValue("FMaterialID", row) as DynamicObject;
				long materialId = (dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["Id"]);
				filter = Common.GetMapIdFilter(mainOrgId, custId, materialId, this._baseDataOrgCtl);
				break;
			}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x0007FF04 File Offset: 0x0007E104
		private void SetKPTypeAndKPAfterSave()
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerId", i) as DynamicObject;
				long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				if (num2 == num && a == "BD_OwnerOrg")
				{
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = true;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = true;
				}
				else
				{
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = false;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = false;
				}
			}
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x00080050 File Offset: 0x0007E250
		private void TakeDefaultStockStatus(string sStockStatus, long newStockValue, int row, string stockStatusType)
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FDefStockStatusId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_STOCK",
				FilterClauseWihtKey = string.Format("FStockId={0}", newStockValue),
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			long num = 0L;
			if (dynamicObjectCollection.Count > 0)
			{
				DynamicObject dynamicObject = dynamicObjectCollection[0];
				num = Convert.ToInt64(dynamicObject[0]);
			}
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FType"));
			QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
			{
				FormId = "BD_StockStatus",
				FilterClauseWihtKey = string.Format("FStockStatusId={0}", num),
				SelectItems = list2
			};
			DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter2, null);
			string a = "";
			if (dynamicObjectCollection2.Count > 0)
			{
				DynamicObject dynamicObject2 = dynamicObjectCollection2[0];
				a = Convert.ToString(dynamicObject2[0]);
			}
			DynamicObject dynamicObject3 = base.View.Model.GetValue(sStockStatus, row) as DynamicObject;
			if (a == stockStatusType && dynamicObject3 == null)
			{
				base.View.Model.SetValue(sStockStatus, num, row);
			}
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x000801A4 File Offset: 0x0007E3A4
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string value = BillUtils.GetValue<string>(this.Model, "FBizType", -1, null, null);
			if (!string.IsNullOrWhiteSpace(value) && (value == "1" || value == "3"))
			{
				filter = " FType IN ('0','8') ";
				return true;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockId", row) as DynamicObject;
			if (dynamicObject != null)
			{
				List<SelectorItemInfo> list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FStockStatusType"));
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = "BD_STOCK",
					FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject["Id"]),
					SelectItems = list
				};
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
				string text = "";
				if (dynamicObjectCollection != null)
				{
					DynamicObject dynamicObject2 = dynamicObjectCollection[0];
					text = Convert.ToString(dynamicObject2["FStockStatusType"]);
				}
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = "'" + text.Replace(",", "','") + "'";
					filter = string.Format(" FType IN ({0})", text);
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x000802F0 File Offset: 0x0007E4F0
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_OOSBillTypeParaSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BizType"]);
				this.para_BillTypeUseCustMatMapping = Convert.ToBoolean(dynamicObject["UseCustMatMapping"]);
			}
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00080358 File Offset: 0x0007E558
		private void SetPickOrgControl(string sOwnerTypeId, string sOwnerId)
		{
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "4"))
			{
				if (StringUtils.EqualsIgnoreCase(sOwnerTypeId, "BD_OwnerOrg"))
				{
					long num = string.IsNullOrWhiteSpace(sOwnerId) ? 0L : Convert.ToInt64(sOwnerId);
					base.View.Model.SetValue("FPickOrgId", num);
					return;
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FSTOCKORGID") as DynamicObject;
				long num2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				base.View.Model.SetValue("FPickOrgId", num2);
			}
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x00080480 File Offset: 0x0007E680
		private void SplitMisBill(string splitOrmPty)
		{
			if (base.View.Model.DataChanged)
			{
				base.View.ShowMessage(ResManager.LoadKDString("界面有变动,请先保存单据", "004023000023424", 5, new object[0]), 0);
				return;
			}
			DynamicObject dataObject = this.Model.DataObject;
			string[] splitKeys = splitOrmPty.Split(new char[]
			{
				'|'
			});
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = new Dictionary<long, IGrouping<long, DynamicObject>>();
			List<DynamicObject> list = new List<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			if (splitKeys.Length <= 1)
			{
				dictionary = (from w in entityDataObject
				where Convert.ToInt64(w[splitKeys[0]]) != 0L
				select w into g
				group g by Convert.ToInt64(g[splitKeys[0]])).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			}
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				base.View.ShowMessage(ResManager.LoadKDString("仓库都为空，不需要分仓", "004023000023425", 5, new object[0]), 0);
				return;
			}
			if (dictionary.Keys.Count<long>() == 1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("仓库相同，不需要分仓", "004023000023426", 5, new object[0]), 0);
				return;
			}
			int num = 0;
			foreach (KeyValuePair<long, IGrouping<long, DynamicObject>> keyValuePair in dictionary)
			{
				num++;
				if (num != 1)
				{
					DynamicObject dynamicObject = OrmUtils.Clone(dataObject, false, true) as DynamicObject;
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["BillEntry"] as DynamicObjectCollection;
					dynamicObjectCollection.Clear();
					int num2 = 1;
					foreach (DynamicObject dynamicObject2 in keyValuePair.Value)
					{
						DynamicObject dynamicObject3 = OrmUtils.Clone(dynamicObject2, false, true) as DynamicObject;
						long num3 = Convert.ToInt64(dynamicObject2["Id"]);
						BillUtils.SetDynamicObjectItemValue(dynamicObject3, "Seq", num2);
						BillUtils.SetDynamicObjectItemValue(dynamicObject3, "SrcMisdelEntryId", num3);
						BillUtils.SetDynamicObjectItemValue(dynamicObject3, "StockFlag", false);
						dynamicObjectCollection.Add(dynamicObject3);
						list2.Add(dynamicObject2);
						num2++;
					}
					BillUtils.SetDynamicObjectItemValue(dynamicObject, "BillNo", string.Empty);
					BillUtils.SetDynamicObjectItemValue(dynamicObject, "DocumentStatus", "Z");
					list.Add(dynamicObject);
				}
			}
			IEnumerable<DynamicObject> source = from data in list
			from tarEntry in (DynamicObjectCollection)data["BillEntry"]
			select tarEntry;
			List<long> list3 = (from s in source
			select Convert.ToInt64(s["SrcMisdelEntryId"])).ToList<long>();
			if (ListUtils.IsEmpty<long>(list3))
			{
				return;
			}
			new List<long>();
			foreach (DynamicObject dynamicObject4 in list2)
			{
				long item = Convert.ToInt64(dynamicObject4["Id"]);
				if (list3.Contains(item))
				{
					entityDataObject.Remove(dynamicObject4);
				}
			}
			this.SaveAndShowResult(list);
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x00080858 File Offset: 0x0007EA58
		private void SaveAndShowResult(List<DynamicObject> objs)
		{
			OperateOption operateOption = OperateOption.Create();
			OperateOptionExt.SetIgnoreInteractionFlag(operateOption, true);
			OperateOptionUtils.SetIgnoreScopeValidateFlag(operateOption, true);
			OperateOptionUtils.SetIgnoreWarning(operateOption, true);
			OperateOptionExt.AddInteractionFlag(operateOption, "Kingdee.K3.SCM.App.Core.AppBusinessService.UpdateStockService,Kingdee.K3.SCM.App.Core");
			OperateOptionExt.AddInteractionFlag(operateOption, "Kingdee.K3.SCM.App.Validator.SecQtyValidator;Kingdee.K3.SCM.App.Validator");
			OperateOptionExt.AddInteractionFlag(operateOption, "MisDeliveryBillSecQtyCheckError");
			OperateOptionExt.AddInteractionFlag(operateOption, "SAL_DOWNPRICECHECK");
			OperateOptionExt.AddInteractionFlag(operateOption, "KD_SAL_CHECKQTY");
			operateOption.SetVariableValue("IgnoreCheckSalAvailableQty", true);
			string text = "";
			IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, base.View.Model.BusinessInfo, base.View.Model.DataObject, operateOption, "");
			if (!operationResult.IsSuccess)
			{
				text = this.GetResultErrInfo(operationResult);
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("分仓后当前单据保存失败，原因：{0}", "004023000023428", 5, new object[0]), text), "", 0);
				return;
			}
			operationResult = BusinessDataServiceHelper.Draft(base.Context, base.View.BusinessInfo, objs.ToArray(), null, "");
			if (!operationResult.IsSuccess)
			{
				text = this.GetResultErrInfo(operationResult);
				text = string.Format(ResManager.LoadKDString("暂存分仓单据失败，原因：{0}", "004023000023427", 5, new object[0]), text);
			}
			if (ListUtils.IsEmpty<DynamicObject>(operationResult.SuccessDataEnity))
			{
				base.View.ShowErrMessage(text, "", 0);
				return;
			}
			operateOption = OperateOption.Create();
			OperateOptionExt.SetIgnoreInteractionFlag(operateOption, true);
			OperateOptionUtils.SetIgnoreScopeValidateFlag(operateOption, true);
			OperateOptionUtils.SetIgnoreWarning(operateOption, true);
			OperateOptionExt.AddInteractionFlag(operateOption, "Kingdee.K3.SCM.App.Core.AppBusinessService.UpdateStockService,Kingdee.K3.SCM.App.Core");
			OperateOptionExt.AddInteractionFlag(operateOption, "Kingdee.K3.SCM.App.Validator.SecQtyValidator;Kingdee.K3.SCM.App.Validator");
			OperateOptionExt.AddInteractionFlag(operateOption, "MisDeliveryBillSecQtyCheckError");
			OperateOptionExt.AddInteractionFlag(operateOption, "SAL_DOWNPRICECHECK");
			OperateOptionExt.AddInteractionFlag(operateOption, "KD_SAL_CHECKQTY");
			operateOption.SetVariableValue("IgnoreCheckSalAvailableQty", true);
			IOperationResult operationResult2 = BusinessDataServiceHelper.Save(base.Context, base.View.BusinessInfo, operationResult.SuccessDataEnity.ToArray<DynamicObject>(), operateOption, "");
			string text2;
			if (operationResult2.IsSuccess)
			{
				text2 = string.Format(ResManager.LoadKDString("分仓成功，共生成{0}张其他出库单", "004023030009455", 5, new object[0]), operationResult2.Rows.Count<ExtendedDataEntity>());
			}
			else
			{
				text2 = this.GetResultErrInfo(operationResult2);
				if (text2.Length > 0)
				{
					text2 = ResManager.LoadKDString("保存分仓生成的其他出库单失败，原因：", "004023030009456", 5, new object[0]) + text2;
				}
				else
				{
					text2 = ResManager.LoadKDString("保存分仓生成的其他出库单失败，原因未知", "004023030009457", 5, new object[0]);
				}
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = text + "\r\n" + text2;
			}
			else if (!string.IsNullOrWhiteSpace(text2))
			{
				text = text2;
			}
			base.View.ShowMessage(text, 0);
			base.View.UpdateView();
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x00080AFC File Offset: 0x0007ECFC
		private string GetResultErrInfo(IOperationResult result)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (result != null && result.InteractionContext != null)
			{
				stringBuilder.AppendLine(result.InteractionContext.SimpleMessage);
			}
			else
			{
				if (result.ValidationErrors != null && result.ValidationErrors.Count > 0)
				{
					foreach (ValidationErrorInfo validationErrorInfo in result.ValidationErrors)
					{
						stringBuilder.AppendLine(validationErrorInfo.Message);
					}
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(result.OperateResult))
				{
					for (int i = 0; i < result.OperateResult.Count; i++)
					{
						stringBuilder.AppendLine(result.OperateResult[i].Message);
					}
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x00080BD8 File Offset: 0x0007EDD8
		private void GetUseCustMatMappingParamater()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				long num = Convert.ToInt64(dynamicObject["Id"]);
				object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num, "SAL_SystemParameter", "UseCustMatMapping", false);
				this.para_UseCustMatMapping = (systemProfile != null && Convert.ToBoolean(systemProfile));
			}
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject2 = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_OOSBillTypeParaSetting", baseDataStringValue, true);
			if (dynamicObject2 != null)
			{
				this.para_BillTypeUseCustMatMapping = Convert.ToBoolean(dynamicObject2["UseCustMatMapping"]);
			}
		}

		// Token: 0x040003BD RID: 957
		private const string bizType_VMI = "2";

		// Token: 0x040003BE RID: 958
		private const string bizType_Fee = "3";

		// Token: 0x040003BF RID: 959
		private const string bizType_Adj = "4";

		// Token: 0x040003C0 RID: 960
		private const string bizType_Ass = "1";

		// Token: 0x040003C1 RID: 961
		private long defaultStock;

		// Token: 0x040003C2 RID: 962
		private long lastAuxpropId;

		// Token: 0x040003C3 RID: 963
		private bool para_UseCustMatMapping;

		// Token: 0x040003C4 RID: 964
		private bool para_BillTypeUseCustMatMapping;

		// Token: 0x040003C5 RID: 965
		private bool associatedCopyEntryRow;

		// Token: 0x040003C6 RID: 966
		private bool copyEntryRow;

		// Token: 0x040003C7 RID: 967
		private Dictionary<string, bool> _baseDataOrgCtl = new Dictionary<string, bool>();
	}
}
