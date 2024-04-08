using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
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

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A3 RID: 163
	public class StockMiscellaneousEdit : AbstractBillPlugIn
	{
		// Token: 0x060009CA RID: 2506 RVA: 0x0008437A File Offset: 0x0008257A
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetDefKeeperTypeAndKeeperValue(e.Row, "NewRow");
			}
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x000843A4 File Offset: 0x000825A4
		public override void BeforeBindData(EventArgs e)
		{
			this.SetKPTypeAndKPAfterSave();
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x000843AC File Offset: 0x000825AC
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetDefLocalCurrency();
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				if (baseDataLongValue > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
				}
			}
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				string text = Convert.ToString(base.View.Model.GetValue("FSrcBillTypeId", 0));
				if (StringUtils.EqualsIgnoreCase(text, "QM_InspectBill") || StringUtils.EqualsIgnoreCase(text, "QM_DefectProcessBill") || StringUtils.EqualsIgnoreCase(text, "QM_MRBReviewBill"))
				{
					this.SetDefLocalCurrency();
				}
			}
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x00084488 File Offset: 0x00082688
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FMATERIALID":
			{
				this.SetDefKeeperTypeAndKeeperValue(e.Row, "DataChange");
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
			case "FOWNERIDHEAD":
			{
				string keeperTypeAndKeeper = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeper(keeperTypeAndKeeper);
				this.SetDefLocalCurrency();
				return;
			}
			case "FKEEPERTYPEID":
			{
				string newKeeperTypeId = Convert.ToString(e.NewValue);
				this.SetKeeperValue(newKeeperTypeId, e.Row);
				return;
			}
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
				return;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				return;
			}
			case "FOWNERTYPEIDHEAD":
				Common.SynOwnerType(this, "FOwnerTypeIdHead", "FOwnerTypeId");
				break;

				return;
			}
		}

		// Token: 0x060009CE RID: 2510 RVA: 0x00084650 File Offset: 0x00082850
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "UNCANCEL"))
				{
					if (!(a == "CANCEL"))
					{
						return;
					}
					if (e.OperationResult.OperateResult.Count > 0)
					{
						e.OperationResult.OperateResult[0].Message = (e.ExecuteResult ? ResManager.LoadKDString("单据作废成功!", "004023030000388", 5, new object[0]) : ResManager.LoadKDString("单据作废失败!", "004023030000391", 5, new object[0]));
					}
				}
				else if (e.OperationResult.OperateResult.Count > 0)
				{
					e.OperationResult.OperateResult[0].Message = (e.ExecuteResult ? ResManager.LoadKDString("单据反作废成功!", "004023030000382", 5, new object[0]) : ResManager.LoadKDString("单据反作废失败!", "004023030000385", 5, new object[0]));
					return;
				}
			}
		}

		// Token: 0x060009CF RID: 2511 RVA: 0x00084754 File Offset: 0x00082954
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSTOCKPLACEID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FSTOCKID", e.Row) as DynamicObject;
				if (dynamicObject == null || dynamicObject["Id"].ToString() == "0")
				{
					e.Cancel = true;
					base.View.ShowMessage(ResManager.LoadKDString("请先选择仓库!", "004023030000394", 5, new object[0]), 0, "", 0);
					return;
				}
				break;
			}
			case "FOWNERIDH":
			{
				string lotF8InvFilter;
				if (this.GetFieldFilter(e.FieldKey, out lotF8InvFilter))
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
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FEXTAUXUNITID":
			{
				string lotF8InvFilter;
				if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
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
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + lotF8InvFilter;
					return;
				}
				break;
			}
			case "FLOT":
			{
				string value = BillUtils.GetValue<string>(this.Model, "FStockDirect", -1, null, null);
				if (!string.IsNullOrWhiteSpace(value) && value.Equals("RETURN"))
				{
					string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
					{
						MaterialFieldKey = "FMATERIALID",
						StockOrgFieldKey = "FStockOrgId",
						OwnerTypeFieldKey = "FOWNERTYPEID",
						OwnerFieldKey = "FOWNERID",
						KeeperTypeFieldKey = "FKEEPERTYPEID",
						KeeperFieldKey = "FKEEPERID",
						AuxpropFieldKey = "FAuxPropId",
						BomFieldKey = "FBOMID",
						StockFieldKey = "FSTOCKID",
						StockLocFieldKey = "FStockLocId",
						StockStatusFieldKey = "FSTOCKSTATUSID",
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
						IRegularFilterParameter listFilterParameter4 = e.ListFilterParameter;
						listFilterParameter4.Filter = listFilterParameter4.Filter + " AND " + lotF8InvFilter;
					}
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060009D0 RID: 2512 RVA: 0x00084ABC File Offset: 0x00082CBC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FSTOCKPLACEID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FSTOCKID", e.Row) as DynamicObject;
				if (dynamicObject == null || dynamicObject["Id"].ToString().Length > 0)
				{
					base.View.Model.SetValue("FSTOCKPLACEID", null);
					base.View.ShowMessage(ResManager.LoadKDString("请先选择仓库!", "004023030000394", 5, new object[0]), 0, "", 0);
					return;
				}
				break;
			}
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
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
				string text;
				if (this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text, e.Row))
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

		// Token: 0x060009D1 RID: 2513 RVA: 0x00084C80 File Offset: 0x00082E80
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x00084CE4 File Offset: 0x00082EE4
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x00084D1C File Offset: 0x00082F1C
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 13 && e.Bills is List<ConvertBillElement>)
			{
				long value = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
				if (value > 0L && Common.HaveBOMViewPermission(base.Context, value))
				{
					Common.SetBomExpandBillToConvertForm(base.Context, (List<ConvertBillElement>)e.Bills);
				}
			}
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x00084D8C File Offset: 0x00082F8C
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

		// Token: 0x060009D5 RID: 2517 RVA: 0x00084E64 File Offset: 0x00083064
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

		// Token: 0x060009D6 RID: 2518 RVA: 0x00084F08 File Offset: 0x00083108
		private void SetDefLocalCurrency()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrId", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x00084F48 File Offset: 0x00083148
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

		// Token: 0x060009D8 RID: 2520 RVA: 0x0008503C File Offset: 0x0008323C
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

		// Token: 0x060009D9 RID: 2521 RVA: 0x00085128 File Offset: 0x00083328
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

		// Token: 0x060009DA RID: 2522 RVA: 0x000851B8 File Offset: 0x000833B8
		private bool GetFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FOWNERIDH")
			{
				string text = Convert.ToString(base.View.Model.GetValue("FOWNERTYPEIDHEAD"));
				if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_OwnerOrg"))
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgID") as DynamicObject;
					string arg = string.Empty;
					if (dynamicObject == null)
					{
						return false;
					}
					arg = dynamicObject["Id"].ToString();
					filter = string.Format(" Exists (SELECT 1 FROM V_SCM_OwnerOrg WHERE FFORBIDSTATUS='A' \r\n                                               AND FNUMBER='{0}' AND t0.FORGID=V_SCM_OwnerOrg.FORGID)", arg);
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00085270 File Offset: 0x00083470
		private void SetDefKeeperTypeAndKeeperValue(int row, string sType)
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
			base.View.Model.SetValue("FOWNERTYPEID", value, row);
			if (!StringUtils.EqualsIgnoreCase(sType, "NewRow"))
			{
				base.View.Model.SetValue("FOwnerId", num, row);
			}
			if (num == num2 && a == "BD_OwnerOrg")
			{
				base.View.GetFieldEditor("FKEEPERTYPEID", row).Enabled = true;
				base.View.GetFieldEditor("FKEEPERID", row).Enabled = true;
				return;
			}
			base.View.GetFieldEditor("FKEEPERTYPEID", row).Enabled = false;
			base.View.GetFieldEditor("FKEEPERID", row).Enabled = false;
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x000853C4 File Offset: 0x000835C4
		private void SetKeeperTypeAndKeeper(string newOwerValue)
		{
			object value = base.View.Model.GetValue("FOWNERTYPEIDHEAD");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long value2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			string text = Convert.ToString(value2);
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
					if (num == 0L && base.View.GetControl("FOwnerId").Enabled && dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, j);
					}
				}
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", j);
				base.View.Model.SetValue("FKeeperId", text, j);
				base.View.GetFieldEditor("FKeeperTypeId", j).Enabled = false;
				base.View.GetFieldEditor("FKeeperId", j).Enabled = false;
			}
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x000856B8 File Offset: 0x000838B8
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

		// Token: 0x060009DE RID: 2526 RVA: 0x000857A0 File Offset: 0x000839A0
		private bool GetStockFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKID"))
				{
					if (!(a == "FMATERIALID"))
					{
						if (!(a == "FSTOCKERID"))
						{
							if (!(a == "FSTOCKERGROUPID"))
							{
								if (a == "FEXTAUXUNITID")
								{
									filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
								}
							}
							else
							{
								DynamicObject dynamicObject = base.View.Model.GetValue("FSTOCKERID") as DynamicObject;
								filter += " FIsUse='1' ";
								if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
								{
									filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject["Id"]));
								}
							}
						}
						else
						{
							DynamicObject dynamicObject2 = base.View.Model.GetValue("FSTOCKERGROUPID") as DynamicObject;
							filter += " FIsUse='1' ";
							long num = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
							if (num != 0L)
							{
								filter = filter + "And FOPERATORGROUPID = " + num.ToString();
							}
						}
					}
					else
					{
						filter = " FIsInventory = '1'";
					}
				}
				else
				{
					string arg = string.Empty;
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FSTOCKSTATUSID", row) as DynamicObject;
					arg = ((dynamicObject3 == null) ? "" : Convert.ToString(dynamicObject3["Number"]));
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
						DynamicObject dynamicObject4 = dynamicObjectCollection[0];
						filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject4["FType"]);
					}
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x000859F0 File Offset: 0x00083BF0
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

		// Token: 0x060009E0 RID: 2528 RVA: 0x00085B3C File Offset: 0x00083D3C
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

		// Token: 0x060009E1 RID: 2529 RVA: 0x00085C90 File Offset: 0x00083E90
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FSTOCKID", row) as DynamicObject;
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

		// Token: 0x040003F3 RID: 1011
		private long lastAuxpropId;
	}
}
