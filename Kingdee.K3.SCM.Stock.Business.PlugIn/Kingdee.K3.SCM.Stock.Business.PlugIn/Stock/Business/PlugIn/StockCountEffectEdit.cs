using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Common.BusinessEntity.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200008A RID: 138
	public class StockCountEffectEdit : AbstractBillPlugIn
	{
		// Token: 0x060006B6 RID: 1718 RVA: 0x00052B58 File Offset: 0x00050D58
		public override void AfterCreateModelData(EventArgs e)
		{
			long num = 0L;
			if (base.View.BusinessInfo.GetForm().Id.Equals("STK_StockCountLoss") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI"))
			{
				this.SetKeeperTypeAndKeeperLock();
			}
			if (base.View.OpenParameter.Status != null)
			{
				return;
			}
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				num = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				if (num > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FSTOCKERID", "WHY", num);
				}
			}
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1)
			{
				return;
			}
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			this.Model.GetValue("FOwnerTypeId");
			if (this.Model.GetValue("FKeeperTypeId") != null)
			{
				this.Model.SetValue("FKeeperId", num);
			}
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x00052C9C File Offset: 0x00050E9C
		public override void BeforeBindData(EventArgs e)
		{
			if (base.View.BusinessInfo.GetForm().Id.Equals("STK_StockCountLoss") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI"))
			{
				this.LockKPTypeAndKPAfterSave();
			}
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x00052CF6 File Offset: 0x00050EF6
		public override void AfterBindData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				this.SetLocalCurrency();
			}
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x00052D10 File Offset: 0x00050F10
		private void LockFieldThirdBill()
		{
			string value = Convert.ToString(base.View.Model.GetValue("FThirdSrcBillNo")).Trim();
			if (!string.IsNullOrEmpty(value))
			{
				this.LockFieldHead();
				int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
				for (int i = 0; i < entryRowCount; i++)
				{
					this.LockField(i);
				}
			}
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x00052D74 File Offset: 0x00050F74
		private void LockFieldHead()
		{
			BillUtils.LockField(base.View, "FOwnerTypeIdHead", false, -1);
			BillUtils.LockField(base.View, "FOwnerIdHead", false, -1);
			BillUtils.LockField(base.View, "FDate", false, -1);
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x00052DAC File Offset: 0x00050FAC
		private void LockField(int row)
		{
			base.View.GetFieldEditor("FMaterialId", row).Enabled = false;
			base.View.GetFieldEditor("FAuxPropId", row).Enabled = false;
			base.View.GetFieldEditor("FStockId", row).Enabled = false;
			base.View.GetFieldEditor("FUnitID", row).Enabled = false;
			base.View.GetFieldEditor("FCountQty", row).Enabled = false;
			base.View.GetFieldEditor("FLOT", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitId", row).Enabled = false;
			base.View.GetFieldEditor("FExtAuxUnitQty", row).Enabled = false;
			base.View.GetFieldEditor("FProduceDate", row).Enabled = false;
			base.View.GetFieldEditor("FExpiryDate", row).Enabled = false;
			base.View.GetFieldEditor("FStockStatusId", row).Enabled = false;
			base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = false;
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x00052ECD File Offset: 0x000510CD
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			this.SetDefKeeperTypeAndKeeperValue(e.Row, "NewRow");
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x00052EE0 File Offset: 0x000510E0
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSTOCKORGID":
				this.SetLocalCurrency();
				return;
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FSTOCKERGROUPID", "WHY");
				return;
			case "FMATERIALID":
			{
				this.IsChangeMaterial = true;
				this.ClearRowData(e.Row);
				this.SetDefKeeperTypeAndKeeperValue(e.Row, "DataChange");
				long num2 = 0L;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L)
				{
					base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
					long num3 = Convert.ToInt64(((DynamicObjectCollection)dynamicObject2["MaterialBase"])[0]["BaseUnitId_Id"]);
					base.View.Model.SetValue("FBaseUnitID", num3, e.Row);
					DynamicObjectCollection dynamicObjectCollection = dynamicObject2["MaterialStock"] as DynamicObjectCollection;
					long num4 = Convert.ToInt64(dynamicObjectCollection[0]["StoreUnitID_Id"]);
					base.View.Model.SetValue("FUnitID", num4, e.Row);
					long num5 = Convert.ToInt64(dynamicObjectCollection[0]["AuxUnitID_Id"]);
					base.View.Model.SetValue("FSecUnitId", num5, e.Row);
					base.View.Model.SetValue("FExtAuxUnitId", num5, e.Row);
					long num6 = Convert.ToInt64(dynamicObjectCollection[0]["SNUnit_Id"]);
					base.View.Model.SetValue("FSNUnitID", num6, e.Row);
					long num7 = Convert.ToInt64(dynamicObjectCollection[0]["StockId_Id"]);
					base.View.Model.SetValue("FStockId", num7, e.Row);
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockId", e.Row) as DynamicObject;
					if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) > 0L)
					{
						base.View.Model.SetValue("FStockLocID", dynamicObjectCollection[0]["StockPlaceId_Id"], e.Row);
					}
				}
				this.SetAcctQty(e.Row);
				this.IsChangeMaterial = false;
				return;
			}
			case "FOWNERIDHEAD":
			{
				string keeperTypeAndKeeper = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeper(keeperTypeAndKeeper);
				return;
			}
			case "FOWNERTYPEIDHEAD":
				Common.SynOwnerType(this, "FOwnerTypeIdHead", "FOwnerTypeId");
				return;
			case "FKEEPERTYPEID":
				if (base.View.BusinessInfo.GetForm().Id.Equals("STK_StockCountLoss") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI"))
				{
					string newKeeperTypeId = Convert.ToString(e.NewValue);
					this.SetKeeperLock(newKeeperTypeId, e.Row);
				}
				if (!this.IsChangeMaterial)
				{
					this.SetAcctQty(e.Row);
					return;
				}
				break;
			case "FUNITID":
			case "FSTOCKID":
			case "FSTOCKLOCID":
			case "FSTOCKSTATUSID":
			case "FLOT":
			case "FKEEPERID":
			case "FOWNERID":
			case "FBOMID":
			case "FPRODUCEDATE":
			case "FEXPIRYDATE":
			case "FMTONO":
			case "FPROJECTNO":
				if (!this.IsChangeMaterial)
				{
					this.SetAcctQty(e.Row);
					return;
				}
				break;
			case "FAUXPROPID":
			{
				if (!this.IsChangeMaterial)
				{
					this.SetAcctQty(e.Row);
				}
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				break;
			}

				return;
			}
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00053420 File Offset: 0x00051620
		private void ClearRowData(int row)
		{
			base.View.Model.SetValue("FUnitID", 0, row);
			base.View.Model.SetValue("FBaseUnitID", 0, row);
			base.View.Model.SetValue("FSecUnitId", 0, row);
			base.View.Model.SetValue("FExtAuxUnitId", 0, row);
			base.View.Model.SetValue("FSNUnitID", 0, row);
			base.View.Model.SetValue("FAuxPropId", 0, row);
			base.View.Model.SetValue("FBOMID", 0, row);
			base.View.Model.SetValue("FLot", "", row);
			base.View.Model.SetValue("FProjectNo", "", row);
			base.View.Model.SetValue("FProduceDate", null, row);
			base.View.Model.SetValue("FExpiryDate", null, row);
			base.View.Model.SetValue("FMtoNo", "", row);
			base.View.Model.SetValue("FSecAcctQty", 0, row);
			base.View.Model.SetValue("FSecCountQty", 0, row);
			base.View.Model.SetValue("FSecLossQty", 0, row);
			base.View.Model.SetValue("FSecGainQty", 0, row);
			base.View.Model.SetValue("FExtSecAcctQty", 0, row);
			base.View.Model.SetValue("FExtAuxUnitQty", 0, row);
			base.View.Model.SetValue("FExtSecLOSSQty", 0, row);
			base.View.Model.SetValue("FExtSecGAINQty", 0, row);
			base.View.Model.SetValue("FSNQty", 0, row);
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x0005366C File Offset: 0x0005186C
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERGROUPID":
			case "FSTOCKERID":
			case "FEXTAUXUNITID":
			{
				string text;
				if (this.GetStockFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				string text;
				if (this.GetStockStatusFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
						listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
					}
				}
				break;
			}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x000537D0 File Offset: 0x000519D0
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERGROUPID":
			case "FSTOCKERID":
			case "FEXTAUXUNITID":
			{
				string text;
				if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
					}
					else
					{
						e.Filter = e.Filter + " AND " + text;
					}
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
					}
					else
					{
						e.Filter = e.Filter + " AND " + text;
					}
				}
				break;
			}
			}
			base.BeforeSetItemValueByNumber(e);
		}

		// Token: 0x060006C1 RID: 1729 RVA: 0x00053914 File Offset: 0x00051B14
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060006C2 RID: 1730 RVA: 0x00053978 File Offset: 0x00051B78
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x000539B0 File Offset: 0x00051BB0
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Operation.FormOperation.Operation, "UnAudit"))
			{
				if (!StringUtils.EqualsIgnoreCase(base.View.Model.GetValue("FDocumentStatus").ToString(), "C") && !StringUtils.EqualsIgnoreCase(base.View.Model.GetValue("FDocumentStatus").ToString(), "B"))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("单据在提交后才可以执行反审核操作!", "004046030002275", 5, new object[0]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				object value = base.View.Model.GetValue("FStkCountSchemeNo");
				if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("物料盘点作业审核生成的单据,不允许反审核!", "004046030002281", 5, new object[0]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x00053AD0 File Offset: 0x00051CD0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBQUERYCOUNTINPUT"))
				{
					return;
				}
				this.QueryCycleCountInput();
			}
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x00053B00 File Offset: 0x00051D00
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			if (!e.Context.IsMultiOrg)
			{
				if (StockServiceHelper.GetUpdateStockDate(e.Context, e.Context.CurrentOrganizationInfo.ID) == null)
				{
					e.CancelMessage = ResManager.LoadKDString("请先在【启用库存管理】中设置库存启用日期,结束初始化，再进行盘点业务处理!", "004023030009247", 5, new object[0]);
					e.Cancel = true;
					return;
				}
				List<long> list = new List<long>();
				list.Add(e.Context.CurrentOrganizationInfo.ID);
				Dictionary<long, DateTime> invEndInitialDate = CommonServiceHelper.GetInvEndInitialDate(e.Context, list);
				if (invEndInitialDate == null || invEndInitialDate.Count < 1)
				{
					e.CancelMessage = ResManager.LoadKDString("库存组织未结束初始化，请先结束初始化！", "004024030002389", 5, new object[0]);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x00053BB8 File Offset: 0x00051DB8
		private void LockKPTypeAndKPAfterSave()
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerId", i) as DynamicObject;
				long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				if (num2 == num && a == "BD_OwnerOrg")
				{
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = true;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = false;
				}
				else
				{
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = false;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = false;
				}
			}
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x00053D04 File Offset: 0x00051F04
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

		// Token: 0x060006C8 RID: 1736 RVA: 0x00053DF8 File Offset: 0x00051FF8
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
			base.View.UpdateView("FBillEntry", row);
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x00053EE4 File Offset: 0x000520E4
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

		// Token: 0x060006CA RID: 1738 RVA: 0x00053F74 File Offset: 0x00052174
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
			if (base.View.BusinessInfo.GetForm().Id.Equals("STK_StockCountLoss") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI"))
			{
				if (num == num2 && a == "BD_OwnerOrg")
				{
					base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = true;
					base.View.GetFieldEditor("FKeeperId", row).Enabled = false;
					return;
				}
				base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = false;
			}
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x000540F7 File Offset: 0x000522F7
		private void SetKeeperTypeAndKeeperLock()
		{
			base.View.GetFieldEditor("FKeeperTypeId", 0).Enabled = true;
			base.View.GetFieldEditor("FKeeperId", 0).Enabled = false;
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x00054128 File Offset: 0x00052328
		private void SetKeeperTypeAndKeeper(string newOwerValue)
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			string text = "";
			if (base.View.BusinessInfo.GetForm().Id.Equals("STK_StockCountLoss"))
			{
				text = Convert.ToString(base.View.Model.GetValue("FBusinessType"));
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long value2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			string text2 = Convert.ToString(value2);
			int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
			if (newOwerValue == text2 && a == "BD_OwnerOrg")
			{
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					base.View.Model.SetValue("FOwnerTypeId", value, i);
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, i);
					}
					base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", i);
					base.View.Model.SetValue("FKeeperId", text2, i);
					if (StringUtils.EqualsIgnoreCase(text, "VMI"))
					{
						base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = true;
					}
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
				base.View.Model.SetValue("FKeeperId", text2, j);
				if (StringUtils.EqualsIgnoreCase(text, "VMI"))
				{
					base.View.GetFieldEditor("FKeeperTypeId", j).Enabled = false;
					base.View.GetFieldEditor("FKeeperId", j).Enabled = false;
				}
			}
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x0005449A File Offset: 0x0005269A
		private void SetKeeperLock(string newKeeperTypeId, int row)
		{
			if (newKeeperTypeId == "BD_KeeperOrg")
			{
				base.View.GetFieldEditor("FKeeperId", row).Enabled = false;
				return;
			}
			base.View.GetFieldEditor("FKeeperId", row).Enabled = true;
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x000544D8 File Offset: 0x000526D8
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
								DynamicObject dynamicObject = base.View.Model.GetValue("FStockerId") as DynamicObject;
								filter += " FIsUse='1' ";
								if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
								{
									filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject["Id"]));
								}
							}
						}
						else
						{
							DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
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
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockStatusId", row) as DynamicObject;
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

		// Token: 0x060006CF RID: 1743 RVA: 0x00054728 File Offset: 0x00052928
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockId", row) as DynamicObject;
			if (dynamicObject == null)
			{
				return false;
			}
			string text = dynamicObject["StockStatusType"].ToString();
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = "'" + text.Replace(",", "','") + "'";
				filter = string.Format(" FType IN ({0})", text);
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x000547CC File Offset: 0x000529CC
		private void SetAcctQty(int row)
		{
			object value = this.Model.GetValue("FServiceContext", row);
			if (value != null && value.ToString().Equals("QueryStockUpdate"))
			{
				return;
			}
			STK_Inventory stk_Inventory = new STK_Inventory();
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject == null)
			{
				this.ClearAcctQty(row);
				return;
			}
			stk_Inventory.StockOrgId = Convert.ToInt64(dynamicObject["Id"]);
			dynamicObject = (base.View.Model.GetValue("FMaterialId", row) as DynamicObject);
			DynamicObject dynamicObject2 = dynamicObject;
			if (dynamicObject == null)
			{
				this.ClearAcctQty(row);
				return;
			}
			stk_Inventory.MaterialId = Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]);
			dynamicObject = (base.View.Model.GetValue("FUnitID", row) as DynamicObject);
			if (dynamicObject == null)
			{
				this.ClearAcctQty(row);
				return;
			}
			stk_Inventory.StockUnitId = Convert.ToInt64(dynamicObject["Id"]);
			stk_Inventory.AuxPropId = 0L;
			dynamicObject = (base.View.Model.GetValue("FAuxPropId", row) as DynamicObject);
			long num = 0L;
			if (dynamicObject != null)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject2["MaterialAuxPty"] as DynamicObjectCollection;
				if (dynamicObjectCollection != null)
				{
					if (dynamicObjectCollection.Count((DynamicObject p) => Convert.ToBoolean(p["IsEnable1"])) > 0)
					{
						string key = Common.FlexValToString("BD_FLEXSITEMDETAILV", dynamicObject);
						if (!this._auxProps.TryGetValue(key, out num))
						{
							num = FlexServiceHelper.GetFlexDataId(base.Context, dynamicObject, "BD_FLEXSITEMDETAILV");
							this._auxProps[key] = num;
						}
						if (num == 0L)
						{
							this.ClearAcctQty(row);
							return;
						}
						stk_Inventory.AuxPropId = num;
					}
				}
			}
			dynamicObject = (base.View.Model.GetValue("FKeeperId", row) as DynamicObject);
			stk_Inventory.KeeperId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]));
			object value2 = base.View.Model.GetValue("FKeeperTypeId", row);
			stk_Inventory.KeeperTypeId = ((value2 == null) ? string.Empty : value2.ToString());
			dynamicObject = (base.View.Model.GetValue("FOwnerid", row) as DynamicObject);
			stk_Inventory.OwnerId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]));
			value2 = base.View.Model.GetValue("FOwnerTypeId", row);
			stk_Inventory.OwnerTypeId = ((value2 == null) ? string.Empty : value2.ToString());
			dynamicObject = (base.View.Model.GetValue("FStockId", row) as DynamicObject);
			DynamicObject dynamicObject3 = dynamicObject;
			stk_Inventory.StockId = ((dynamicObject == null) ? 0L : this.GetDynamicValue(dynamicObject));
			stk_Inventory.StockLocId = 0L;
			dynamicObject = (base.View.Model.GetValue("FStockLocId", row) as DynamicObject);
			long num2 = 0L;
			if (dynamicObject3 != null && dynamicObject != null && Convert.ToBoolean(dynamicObject3["IsOpenLocation"]))
			{
				string key2 = Common.FlexValToString("BD_FLEXVALUESDETAIL", dynamicObject);
				if (!this._stockLocs.TryGetValue(key2, out num2))
				{
					num2 = FlexServiceHelper.GetFlexDataId(base.Context, dynamicObject, "BD_FLEXVALUESDETAIL");
					this._stockLocs[key2] = num2;
				}
				if (num2 == 0L)
				{
					this.ClearAcctQty(row);
					return;
				}
				stk_Inventory.StockLocId = num2;
			}
			dynamicObject = (base.View.Model.GetValue("FStockStatusId", row) as DynamicObject);
			stk_Inventory.StockStatusId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			stk_Inventory.StockStatusType = ((dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Type"]));
			dynamicObject = (base.View.Model.GetValue("FBomId", row) as DynamicObject);
			stk_Inventory.BOMID = ((dynamicObject == null) ? 0L : this.GetDynamicValue(dynamicObject));
			value2 = base.View.Model.GetValue("FLOT", row);
			stk_Inventory.LotText = ((value2 == null) ? string.Empty : value2.ToString());
			value2 = base.View.Model.GetValue("FMtoNo", row);
			stk_Inventory.MTONo = ((value2 == null) ? string.Empty : value2.ToString());
			value2 = base.View.Model.GetValue("FProjectNo", row);
			stk_Inventory.ProjectNo = ((value2 == null) ? string.Empty : value2.ToString());
			value2 = base.View.Model.GetValue("FProduceDate", row);
			stk_Inventory.ProduceDate = ((value2 == null) ? DateTime.Parse("1900/01/01") : DateTime.Parse(value2.ToString()));
			value2 = base.View.Model.GetValue("FExpiryDate", row);
			stk_Inventory.ExpiryDate = ((value2 == null) ? DateTime.Parse("1900/01/01") : DateTime.Parse(value2.ToString()));
			stk_Inventory = CommonServiceHelper.GetInventoryData(base.View.Context, stk_Inventory);
			base.View.Model.SetValue("FAcctQty", stk_Inventory.Qty, row);
			base.View.InvokeFieldUpdateService("FAcctQty", row);
			base.View.Model.SetValue("FBaseAcctQty", stk_Inventory.BaseQty, row);
			base.View.InvokeFieldUpdateService("FBaseAcctQty", row);
			base.View.Model.SetValue("FSecAcctQty", stk_Inventory.SecQty, row);
			base.View.InvokeFieldUpdateService("FSecAcctQty", row);
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x00054D54 File Offset: 0x00052F54
		private void ClearAcctQty(int row)
		{
			base.View.Model.SetValue("FAcctQty", 0, row);
			base.View.InvokeFieldUpdateService("FAcctQty", row);
			base.View.Model.SetValue("FBaseAcctQty", 0, row);
			base.View.InvokeFieldUpdateService("FBaseAcctQty", row);
			base.View.Model.SetValue("FSecAcctQty", 0, row);
			base.View.InvokeFieldUpdateService("FSecAcctQty", row);
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x00054DE8 File Offset: 0x00052FE8
		private void SetLocalCurrency()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrId", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x00054E28 File Offset: 0x00053028
		private void QueryCycleCountInput()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = "STK_StockCountInput"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有物料盘点作业的查看权限！", "004023030002104", 5, new object[0]), "", 0);
				return;
			}
			long num = Convert.ToInt64(this.Model.DataObject["Id"]);
			string text = this.Model.GetValue("FSourceType").ToString();
			if (num < 1L || text.Equals("0"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("手工录入单据，没有相关的物料盘点作业单据！", "004046030002284", 5, new object[0]), "", 0);
				return;
			}
			string arg = base.View.OpenParameter.FormId.ToUpperInvariant().Equals("STK_StockCountGain".ToUpperInvariant()) ? "T_STK_STKCOUNTGAINENTRY" : "T_STK_STKCOUNTLOSSENTRY";
			string filter = string.Format(" EXISTS (select 1 from {1} E INNER JOIN {1}_LK  L ON E.FENTRYID=L.FENTRYID\r\n                                        WHERE  E.FID ={0} AND L.FSBILLID=FID) ", num, arg);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_StockCountInput";
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			listShowParameter.CustomParams.Add("source", text);
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = filter;
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x00054FD0 File Offset: 0x000531D0
		private long GetDynamicValue(DynamicObject obj)
		{
			if (obj == null)
			{
				return 0L;
			}
			if (obj.DynamicObjectType.Properties.ContainsKey(FormConst.MASTER_ID))
			{
				return Convert.ToInt64(obj[FormConst.MASTER_ID]);
			}
			if (obj.DynamicObjectType.Properties.ContainsKey("Id"))
			{
				return Convert.ToInt64(obj["Id"]);
			}
			return 0L;
		}

		// Token: 0x04000277 RID: 631
		private const string businessType_VMI = "VMI";

		// Token: 0x04000278 RID: 632
		private bool IsChangeMaterial;

		// Token: 0x04000279 RID: 633
		private Dictionary<string, long> _auxProps = new Dictionary<string, long>();

		// Token: 0x0400027A RID: 634
		private Dictionary<string, long> _stockLocs = new Dictionary<string, long>();

		// Token: 0x0400027B RID: 635
		private long lastAuxpropId;
	}
}
