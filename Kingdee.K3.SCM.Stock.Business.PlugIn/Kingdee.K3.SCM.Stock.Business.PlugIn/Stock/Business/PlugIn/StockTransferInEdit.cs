using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A5 RID: 165
	public class StockTransferInEdit : AbstractBillPlugIn
	{
		// Token: 0x060009FC RID: 2556 RVA: 0x000883C0 File Offset: 0x000865C0
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

		// Token: 0x060009FD RID: 2557 RVA: 0x0008842C File Offset: 0x0008662C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null && a == "TBPUSH")
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
			base.BarItemClick(e);
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x000884B3 File Offset: 0x000866B3
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x000884BC File Offset: 0x000866BC
		public override void AfterBindData(EventArgs e)
		{
			this.SetComValue();
			this.SetCustAndSupplierValue();
			bool flag = Convert.ToBoolean(base.View.Model.GetValue("FIsGenForIOS"));
			SCMCommon.SetSuiteProductStatusByPara(this, "FStockOrgId", flag);
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x000884FC File Offset: 0x000866FC
		private void LockFieldHead()
		{
			BillUtils.LockField(base.View, "FDate", false, -1);
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x00088510 File Offset: 0x00086710
		private void LockField(int row)
		{
			base.View.GetFieldEditor("FUnitID", row).Enabled = false;
			base.View.GetFieldEditor("FPlanTransferQty", row).Enabled = false;
			base.View.GetFieldEditor("FQty", row).Enabled = false;
			base.View.GetFieldEditor("FPathLossQty", row).Enabled = false;
			base.View.GetFieldEditor("FPathLossRespParty", row).Enabled = false;
			base.View.GetFieldEditor("FDestLot", row).Enabled = false;
			base.View.GetFieldEditor("FDestStockID", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitId", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitQty", row).Enabled = false;
			base.View.GetFieldEditor("FWayAuxUnitQty", row).Enabled = false;
			base.View.GetFieldEditor("FProduceDate", row).Enabled = false;
			base.View.GetFieldEditor("FEXPIRYDATE", row).Enabled = false;
			base.View.GetFieldEditor("FDestStockStatusID", row).Enabled = false;
			base.View.GetFieldEditor("FBusinessDate", row).Enabled = false;
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x0008865F File Offset: 0x0008685F
		public override void AfterCreateNewData(EventArgs e)
		{
			this.SetTransMode();
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x00088668 File Offset: 0x00086868
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

		// Token: 0x06000A04 RID: 2564 RVA: 0x000886CC File Offset: 0x000868CC
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_TransInBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x000887EC File Offset: 0x000869EC
		private void SetComValue()
		{
			DynamicObjectCollection source = base.View.Model.DataObject["STK_STKTRANSFERINENTRY"] as DynamicObjectCollection;
			int num = (from p in source
			where !string.IsNullOrWhiteSpace(Convert.ToString(p["SrcBillNo"]))
			select p).Count<DynamicObject>();
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
			if (num == 0)
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

		// Token: 0x06000A06 RID: 2566 RVA: 0x00088B42 File Offset: 0x00086D42
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (e.Operation.FormOperation.Operation.Equals("BatchFill", StringComparison.OrdinalIgnoreCase))
			{
				this.batchFillOn = true;
				this.batchFillMessage.Clear();
			}
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x00088B74 File Offset: 0x00086D74
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			if (e.Operation.Operation.Equals("BatchFill", StringComparison.OrdinalIgnoreCase))
			{
				this.batchFillOn = false;
				if (this.batchFillMessage.Length > 0)
				{
					base.View.ShowErrMessage(this.batchFillMessage.ToString(), "", 0);
				}
			}
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x00088BCC File Offset: 0x00086DCC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FMATERIALID":
			{
				long num2 = 0L;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
				return;
			}
			case "FSTOCKORGID":
				this.SetDefLocalCurrencyAndExchangeType();
				return;
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FSTOCKERGROUPID", "WHY");
				return;
			case "FOWNERIDHEAD":
				this.SetDefLocalCurrencyAndExchangeType();
				this.SetCustAndSupplierValue();
				return;
			case "FOWNEROUTIDHEAD":
				this.SetCustAndSupplierValue();
				return;
			case "FTRANSFERDIRECT":
				this.SetCustAndSupplierValue();
				return;
			case "FSRCSTOCKID":
			{
				object value = base.View.Model.GetValue("FTransferMode");
				if (value != null)
				{
					Convert.ToString(value);
				}
				DynamicObject dyNewStock = base.View.Model.GetValue("FSrcStockID") as DynamicObject;
				this.TakeDefaultStockStatus("FSrcStockStatusID", dyNewStock, e.Row, "4");
				return;
			}
			case "FDESTSTOCKID":
			{
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FDestStockID") as DynamicObject;
				base.View.Model.SetValue("FDestStockStatusID", null, e.Row);
				SCMCommon.TakeDefaultStockStatusOther(this, "FDestStockStatusID", dynamicObject3, e.Row, "'0','8'");
				return;
			}
			case "FISPATHLOSS":
				this.SwitchTransInAndLossSerial(Convert.ToBoolean(e.NewValue));
				return;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				return;
			}
			case "FBIZTYPE":
				this.SetComValue();
				break;

				return;
			}
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x00088E74 File Offset: 0x00087074
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			bool flag = false;
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FOWNERIDHEAD":
			case "FSTOCKOUTORGID":
			case "FOWNEROUTIDHEAD":
			case "FEXTAUXUNITID":
			case "FOWNERID":
			case "FOWNEROUTID":
			{
				string text;
				if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrEmpty(e.ListFilterParameter.Filter) ? text : (e.ListFilterParameter.Filter + "AND" + text));
					return;
				}
				break;
			}
			case "FDESTSTOCKPLACEID":
			{
				int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FSTKTRSINENTRY");
				DynamicObject dynamicObject = base.View.Model.GetValue("FDestStockID", entryCurrentRowIndex) as DynamicObject;
				if (dynamicObject != null)
				{
					flag = Convert.ToBoolean(dynamicObject["IsOpenLocation"]);
					if (!flag)
					{
						base.View.ShowMessage(ResManager.LoadKDString("调入仓库未启用仓位管理", "004023030000397", 5, new object[0]), 0);
					}
				}
				else
				{
					base.View.ShowMessage(ResManager.LoadKDString("选调入仓位前必须先选调入仓库", "004023030000400", 5, new object[0]), 0);
				}
				if (!flag)
				{
					e.Cancel = true;
					return;
				}
				e.ListFilterParameter.Filter = " FSTOCKID = " + dynamicObject["ID"];
				return;
			}
			case "FSRCSTOCKPLACEID":
			{
				int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FSTKTRSINENTRY");
				DynamicObject dynamicObject = base.View.Model.GetValue("FSrcStockID", entryCurrentRowIndex) as DynamicObject;
				if (dynamicObject != null)
				{
					flag = Convert.ToBoolean(dynamicObject["IsOpenLocation"]);
					if (!flag)
					{
						base.View.ShowMessage(ResManager.LoadKDString("调出仓库未启用仓位管理", "004023030000403", 5, new object[0]), 0);
					}
				}
				else
				{
					base.View.ShowMessage(ResManager.LoadKDString("选调出仓位前必须先选调入仓库", "004023030000406", 5, new object[0]), 0);
				}
				if (!flag)
				{
					e.Cancel = true;
					return;
				}
				e.ListFilterParameter.Filter = " FSTOCKID = " + dynamicObject["ID"];
				return;
			}
			case "FMATERIALID":
			case "FSRCSTOCKID":
			case "FDESTSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FDESTSTOCKSTATUSID":
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
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x000891C0 File Offset: 0x000873C0
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FSTKTRSINENTRY");
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FDESTSTOCKID"))
				{
					if (a == "FSRCSTOCKID")
					{
						base.View.Model.SetValue("FSrcStockPlaceID", null, entryCurrentRowIndex);
					}
				}
				else
				{
					base.View.Model.SetValue("FDestStockPlaceID", null, entryCurrentRowIndex);
				}
			}
			base.AfterF7Select(e);
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x00089240 File Offset: 0x00087440
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FOWNERIDHEAD":
			case "FSTOCKOUTORGID":
			case "FOWNEROUTIDHEAD":
			case "FEXTAUXUNITID":
			case "FOWNERID":
			case "FOWNEROUTID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					e.Filter = (string.IsNullOrEmpty(e.Filter) ? text : (e.Filter + "AND" + text));
					return;
				}
				break;
			}
			case "FMATERIALID":
			case "FSRCSTOCKID":
			case "FDESTSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FDESTSTOCKSTATUSID":
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
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x000893D8 File Offset: 0x000875D8
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x0008943C File Offset: 0x0008763C
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x00089474 File Offset: 0x00087674
		private void SetDefLocalCurrencyAndExchangeType()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrID", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x000894B4 File Offset: 0x000876B4
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
			case "FOWNEROUTIDHEAD":
			case "FOWNEROUTID":
			{
				string a3 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeInIdHead"));
				string a4 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeInId", row));
				if ((fieldKey.ToUpperInvariant().Equals("FOWNEROUTIDHEAD") && a3 == "BD_OwnerOrg") || (fieldKey.ToUpperInvariant().Equals("FOWNEROUTID") && a4 == "BD_OwnerOrg"))
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOutOrgID") as DynamicObject;
					long num3 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
					bool bizRelation2 = CommonServiceHelper.GetBizRelation(base.Context, 112L, -1L);
					if (bizRelation2)
					{
						filter = string.Format("  EXISTS (SELECT 1 FROM  t_org_bizrelation a inner join t_org_bizrelationEntry b on a.FBIZRELATIONID=b.FBIZRELATIONID\r\n                                        where a.FBRTYPEID={0} AND b.FRELATIONORGID={1} AND b.FORGID=t0.FORGID) OR t0.FORGID={1})", 112, num3);
					}
				}
				else if ((fieldKey.ToUpperInvariant().Equals("FOWNEROUTIDHEAD") && a3 == "BD_Supplier") || (fieldKey.ToUpperInvariant().Equals("FOWNEROUTID") && a4 == "BD_Supplier"))
				{
					string text2 = base.View.Model.GetValue("FBizType") as string;
					if (!string.IsNullOrWhiteSpace(text2) && text2 == "VMI")
					{
						filter = string.Format(" FVmiBusiness = '1' ", new object[0]);
					}
				}
				break;
			}
			case "FSTOCKOUTORGID":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			case "FEXTAUXUNITID":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
				break;
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x00089884 File Offset: 0x00087A84
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
			case "FSRCSTOCKID":
			{
				string arg = string.Empty;
				DynamicObject dynamicObject = base.View.Model.GetValue("FSrcStockStatusID", row) as DynamicObject;
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
			case "FDESTSTOCKID":
			{
				string arg2 = string.Empty;
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FDestStockStatusID", row) as DynamicObject;
				arg2 = ((dynamicObject3 == null) ? "" : Convert.ToString(dynamicObject3["Number"]));
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FType"));
				QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
				{
					FormId = "BD_StockStatus",
					FilterClauseWihtKey = string.Format("FNumber='{0}'", arg2),
					SelectItems = list2
				};
				DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter2, null);
				if (dynamicObjectCollection2.Count > 0)
				{
					DynamicObject dynamicObject4 = dynamicObjectCollection2[0];
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject4["FType"]);
				}
				break;
			}
			case "FMATERIALID":
			{
				filter = " FIsInventory = '1'";
				string text = base.View.Model.GetValue("FBizType") as string;
				if (StringUtils.EqualsIgnoreCase(text, "VMI"))
				{
					filter += " and FIsVmiBusiness = '1' ";
				}
				break;
			}
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FSTOCKERGROUPID") as DynamicObject;
				filter += " FIsUse='1' ";
				long num2 = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
				if (num2 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num2.ToString();
				}
				break;
			}
			case "FSTOCKERGROUPID":
			{
				DynamicObject dynamicObject6 = base.View.Model.GetValue("FSTOCKERID") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject6 != null && Convert.ToInt64(dynamicObject6["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject6["Id"]));
				}
				break;
			}
			case "FDESTSTOCKSTATUSID":
			{
				DynamicObject dynamicObject7 = base.View.Model.GetValue("FDestStockID", row) as DynamicObject;
				if (dynamicObject7 != null)
				{
					List<SelectorItemInfo> list = new List<SelectorItemInfo>();
					list.Add(new SelectorItemInfo("FStockStatusType"));
					QueryBuilderParemeter queryBuilderParemeter3 = new QueryBuilderParemeter
					{
						FormId = "BD_STOCK",
						FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject7["Id"]),
						SelectItems = list
					};
					DynamicObjectCollection dynamicObjectCollection3 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter3, null);
					string text2 = "";
					if (dynamicObjectCollection3 != null)
					{
						DynamicObject dynamicObject8 = dynamicObjectCollection3[0];
						text2 = Convert.ToString(dynamicObject8["FStockStatusType"]);
					}
					if (!string.IsNullOrWhiteSpace(text2))
					{
						text2 = "'" + text2.Replace(",", "','") + "'";
						filter = string.Format(" FType IN ({0})", text2);
					}
				}
				break;
			}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x00089CF8 File Offset: 0x00087EF8
		private void TakeDefaultStockStatus(string sStockStatus, DynamicObject dyNewStock, int row, string stockStatusType)
		{
			if (dyNewStock == null)
			{
				return;
			}
			long num = Convert.ToInt64(dyNewStock["DefStockStatusId_Id"]);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FType"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_StockStatus",
				FilterClauseWihtKey = string.Format("FStockStatusId={0}", num),
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			string a = "";
			if (dynamicObjectCollection.Count > 0)
			{
				DynamicObject dynamicObject = dynamicObjectCollection[0];
				a = Convert.ToString(dynamicObject[0]);
			}
			if (a == stockStatusType)
			{
				base.View.Model.SetValue(sStockStatus, num, row);
			}
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x00089DD0 File Offset: 0x00087FD0
		private void SetTransMode()
		{
			ComboFieldEditor comboFieldEditor = base.View.GetFieldEditor("FTransferMode", 0) as ComboFieldEditor;
			if (comboFieldEditor == null)
			{
				return;
			}
			ComboField comboField = this.Model.BusinessInfo.GetElement("FTransferMode") as ComboField;
			List<EnumItem> list = new List<EnumItem>();
			list = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
			EnumItem item = (from p in list
			where p.Value == "DIRECT"
			select p).FirstOrDefault<EnumItem>();
			list.Remove(item);
			comboFieldEditor.SetComboItems(list);
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x00089E72 File Offset: 0x00088072
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x00089E74 File Offset: 0x00088074
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

		// Token: 0x06000A15 RID: 2581 RVA: 0x00089F68 File Offset: 0x00088168
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
			base.View.UpdateView("FSTKTRSINENTRY", row);
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x0008A054 File Offset: 0x00088254
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

		// Token: 0x06000A17 RID: 2583 RVA: 0x0008A0E4 File Offset: 0x000882E4
		private void SwitchTransInAndLossSerial(bool newValue)
		{
			Field field = base.View.BusinessInfo.GetField("FMaterialId");
			Field field2 = base.View.BusinessInfo.GetField("FUnitId");
			Field field3 = base.View.BusinessInfo.GetField("FPathLossQty");
			Field field4 = base.View.BusinessInfo.GetField("FQty");
			Field field5 = base.View.BusinessInfo.GetField("FSNUnitID");
			decimal num = 0m;
			decimal num2 = 0m;
			int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FSTKTRSINENTRY");
			Entity entity = base.View.BusinessInfo.GetEntity("FSTKTRSINENTRY");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
			foreach (DynamicObject dynamicObject in ((DynamicObjectCollection)entityDataObject["STK_STKTRANSFERINSERIAL"]))
			{
				if (dynamicObject != null && dynamicObject["SerialNo"] != null && !string.IsNullOrWhiteSpace(dynamicObject["SerialNo"].ToString()))
				{
					if (Convert.ToBoolean(dynamicObject["IsPathLoss"]))
					{
						num = ++num;
					}
					else
					{
						num2 = ++num2;
					}
				}
			}
			decimal num3 = 0m;
			decimal num4 = 0m;
			if (num > 0m || num2 > 0m)
			{
				GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs();
				DynamicObject value = field.DynamicProperty.GetValue<DynamicObject>(entityDataObject);
				if (value == null)
				{
					return;
				}
				getUnitConvertRateArgs.MaterialId = Convert.ToInt64(value["Id"]);
				value = field2.DynamicProperty.GetValue<DynamicObject>(entityDataObject);
				if (value == null)
				{
					return;
				}
				getUnitConvertRateArgs.DestUnitId = Convert.ToInt64(value["Id"]);
				value = field5.DynamicProperty.GetValue<DynamicObject>(entityDataObject);
				if (value == null)
				{
					return;
				}
				getUnitConvertRateArgs.SourceUnitId = Convert.ToInt64(value["Id"]);
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, getUnitConvertRateArgs);
				num3 = ((num == 0m) ? 0m : unitConvertRate.ConvertQty(num, ""));
				num4 = ((num2 == 0m) ? 0m : unitConvertRate.ConvertQty(num2, ""));
			}
			if (newValue)
			{
				this.Model.SetValue(field3, entityDataObject, num3);
				base.View.InvokeFieldUpdateService(field3.Key, entryCurrentRowIndex);
				this.Model.SetValue(field4, entityDataObject, num4);
				base.View.InvokeFieldUpdateService(field4.Key, entryCurrentRowIndex);
				return;
			}
			this.Model.SetValue(field4, entityDataObject, num4);
			base.View.InvokeFieldUpdateService(field4.Key, entryCurrentRowIndex);
			this.Model.SetValue(field3, entityDataObject, num3);
			base.View.InvokeFieldUpdateService(field3.Key, entryCurrentRowIndex);
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x0008A408 File Offset: 0x00088608
		private void SetCustAndSupplierValue()
		{
			if (base.View.OpenParameter.Status != null)
			{
				return;
			}
			string a = base.View.Model.GetValue("FOwnerTypeOutIdHead") as string;
			string a2 = base.View.Model.GetValue("FOwnerTypeIdHead") as string;
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerOutIdHead") as DynamicObject;
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
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

		// Token: 0x040003FC RID: 1020
		private bool batchFillOn;

		// Token: 0x040003FD RID: 1021
		private StringBuilder batchFillMessage = new StringBuilder();

		// Token: 0x040003FE RID: 1022
		private bool hasAsked;

		// Token: 0x040003FF RID: 1023
		private long lastAuxpropId;
	}
}
