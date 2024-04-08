using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200008C RID: 140
	public class StockCountInputEdit : AbstractBillPlugIn
	{
		// Token: 0x060006E0 RID: 1760 RVA: 0x00055654 File Offset: 0x00053854
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object systemProfile = BDCommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "ControlSerialNo", "");
			this._useSerialPara = Convert.ToBoolean(systemProfile);
			if (this._useSerialPara)
			{
				systemProfile = BDCommonServiceHelper.GetSystemProfile(base.Context, 0L, "STK_StockParameter", "SerialManageLevel", "O");
				this._snManageLevel = systemProfile.ToString();
			}
			this._snEntity = base.View.BusinessInfo.GetEntryEntity("FEntrySerial");
			this._parEntryEntity = base.View.BusinessInfo.GetEntryEntity("FBillEntry");
			this._baseDataOrgCtl = Common.GetInvBaseDataCtrolType(base.View.Context);
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x00055710 File Offset: 0x00053910
		public override void AfterBindData(EventArgs e)
		{
			this.strSource = base.View.Model.GetValue("FSource").ToString();
			base.View.GetBarItem("FBillEntry", "tbGetBackUpQty").Enabled = (this.strSource == CountSource.CYCLECOUNTPLAN);
			base.View.GetControl("FLable").SetCustomPropertyValue("visible", !(this.strSource == CountSource.COUNTSCHEME));
			if (this.strSource.Equals("1"))
			{
				base.View.GetMainBarItem("TBQUERYCOUNTTABLE").Visible = false;
			}
			else
			{
				base.View.GetMainBarItem("TBQUERYCOUNTSCHEME").Visible = false;
			}
			this.MarkMinusAcctRow();
			this.dataChanged = false;
			this.SetSerialVisible();
			this.SetSnLock();
			this.oldStockLoc = null;
			this.oldAuxProp = null;
			this.lastAuxpropId = 0L;
			this.stockLocRowIndex = -1;
			this.auxPropRowIndex = -1;
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0005581C File Offset: 0x00053A1C
		public override void AfterUpdateViewState(EventArgs e)
		{
			this.dataChanged = false;
			base.View.GetBarItem("FBillEntry", "tbGetBackUpQty").Enabled = (this.strSource == CountSource.CYCLECOUNTPLAN);
			base.View.GetControl("FLable").SetCustomPropertyValue("visible", !(this.strSource == CountSource.COUNTSCHEME));
			if (this.strSource.Equals("1"))
			{
				base.View.GetMainBarItem("TBQUERYCOUNTTABLE").Visible = false;
			}
			else
			{
				base.View.GetMainBarItem("TBQUERYCOUNTSCHEME").Visible = false;
			}
			this.SetSerialVisible();
			this.SetSnLock();
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x000558DC File Offset: 0x00053ADC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToString().ToUpperInvariant()) != null)
			{
				if (a == "DELETESNENTRY")
				{
					this.RefreshSnCount();
					return;
				}
				if (!(a == "SAVE"))
				{
					return;
				}
				if (e.OperationResult.IsSuccess && this.isGetBackUpQty)
				{
					this.DoGetbackUpQty();
					this.isGetBackUpQty = false;
				}
				this.dataChanged = false;
			}
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x00055954 File Offset: 0x00053B54
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			object value = this.Model.GetValue(e.Key, e.Row);
			if (e.Key.ToUpperInvariant() == "FSTOCKLOCID")
			{
				if (e.Row != this.stockLocRowIndex)
				{
					if (value != null)
					{
						this.oldStockLoc = (OrmUtils.Clone(value as DynamicObject, false, true) as DynamicObject);
					}
					else
					{
						this.oldStockLoc = null;
					}
					this.stockLocRowIndex = e.Row;
					return;
				}
			}
			else if (e.Key.ToUpperInvariant() == "FAUXPROPID")
			{
				if (e.Row != this.auxPropRowIndex)
				{
					if (value != null)
					{
						this.oldAuxProp = (OrmUtils.Clone(value as DynamicObject, false, true) as DynamicObject);
					}
					else
					{
						this.oldAuxProp = null;
					}
					this.auxPropRowIndex = e.Row;
					return;
				}
			}
			else if (e.Key.ToUpperInvariant().Equals("FSERIALNO"))
			{
				if (e.Value == null || string.IsNullOrWhiteSpace(e.Value.ToString()))
				{
					return;
				}
				SimpleSerialSnap simpleSerialSnap = null;
				int num = -1;
				e.Cancel = !this.ValidateInputSerial(e.Row, e.Value.ToString(), ref num, out simpleSerialSnap);
				return;
			}
			else
			{
				e.Cancel = this.IsChangeSysEntryInvField(e.Key, e.Row, value, e.Value);
			}
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x00055AAC File Offset: 0x00053CAC
		public override void DataChanged(DataChangedEventArgs e)
		{
			this.dataChanged = true;
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
				this.SetSnLock();
				return;
			}
			case "FKEEPERTYPEID":
			{
				string a = Convert.ToString(e.NewValue);
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject3 != null)
				{
					Convert.ToInt64(dynamicObject3["Id"]);
				}
				if (a == "BD_KeeperOrg")
				{
					base.View.Model.SetValue("FKeeperId", dynamicObject3, e.Row);
					return;
				}
				break;
			}
			case "FOWNERID":
			{
				object value = base.View.Model.GetValue("FOwnerTypeId", e.Row);
				string a2 = "";
				if (value != null)
				{
					a2 = Convert.ToString(value);
				}
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FOwnerId", e.Row) as DynamicObject;
				long num3 = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				long num4 = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
				if (a2 == "BD_OwnerOrg" && num3 != num4)
				{
					base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", e.Row);
					base.View.Model.SetValue("FKeeperId", num4, e.Row);
					base.View.GetFieldEditor("FKeeperTypeId", e.Row).Enabled = false;
					return;
				}
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", e.Row);
				base.View.Model.SetValue("FKeeperId", num4, e.Row);
				base.View.GetFieldEditor("FKeeperTypeId", e.Row).Enabled = true;
				return;
			}
			case "FSTOCKLOCID":
				if (this.stockLocRowIndex == e.Row)
				{
					this.IsChangeSysEntryInvField(e.Field.Key, e.Row, this.oldStockLoc, this.Model.GetValue(e.Field.Key, e.Row));
					return;
				}
				break;
			case "FAUXPROPID":
				if (this.auxPropRowIndex == e.Row)
				{
					this.IsChangeSysEntryInvField(e.Field.Key, e.Row, this.oldAuxProp, this.Model.GetValue(e.Field.Key, e.Row));
				}
				if (!Convert.ToBoolean(this.Model.GetValue("FIsSystem", e.Row)))
				{
					DynamicObject newAuxpropData = e.OldValue as DynamicObject;
					this.AuxpropDataChanged(newAuxpropData, e.Row);
					return;
				}
				break;
			case "FSERIALNO":
				if (e.NewValue == null || string.IsNullOrWhiteSpace(e.NewValue.ToString()))
				{
					this.Model.SetValue("FStatus", false, e.Row);
					this.Model.SetValue("FIsGain", false, e.Row);
					this.Model.SetValue("FIsLoss", false, e.Row);
					this.RefreshSnCount();
					return;
				}
				if (!BillUtils.GetValue<bool>(this.Model, "FIsAccount", e.Row, false, null))
				{
					this.Model.SetValue("FStatus", true, e.Row);
					this.Model.SetValue("FIsGain", true, e.Row);
					this.Model.SetValue("FIsLoss", false, e.Row);
					this.RefreshSnCount();
					return;
				}
				break;
			case "FISGAIN":
			case "FISLOSS":
				this.RefreshSnCount();
				break;

				return;
			}
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x00055FC5 File Offset: 0x000541C5
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (e.Key.ToUpperInvariant().Equals(this._parEntryEntity.Key.ToUpperInvariant()))
			{
				this.SetSnLock();
			}
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x00055FF8 File Offset: 0x000541F8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				string text;
				if (!(a == "FOWNERID"))
				{
					if (!(a == "FSTOCKID") && !(a == "FEXTAUXUNITID"))
					{
						if (!(a == "FPSTOCKSTATUSID"))
						{
							return;
						}
						if (this.GetStockStatusFieldFilter(e.FieldKey, out text, e.Row))
						{
							if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
							{
								e.ListFilterParameter.Filter = text;
								return;
							}
							IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
							listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
						}
					}
					else if (this.GetStockFieldFilter(e.FieldKey, out text, e.Row))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = text;
							return;
						}
						IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
						listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
						return;
					}
				}
				else if (this.GetOwnerFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0005614C File Offset: 0x0005434C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				string text;
				if (!(a == "FOWNERID"))
				{
					if (!(a == "FSTOCKID") && !(a == "FEXTAUXUNITID"))
					{
						if (!(a == "FPSTOCKSTATUSID"))
						{
							return;
						}
						if (this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text, e.Row))
						{
							if (string.IsNullOrEmpty(e.Filter))
							{
								e.Filter = text;
								return;
							}
							e.Filter = e.Filter + " AND " + text;
						}
					}
					else if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row))
					{
						if (string.IsNullOrEmpty(e.Filter))
						{
							e.Filter = text;
							return;
						}
						e.Filter = e.Filter + " AND " + text;
						return;
					}
				}
				else if (this.GetOwnerFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
					return;
				}
			}
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x00056270 File Offset: 0x00054470
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x000562D4 File Offset: 0x000544D4
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x00056360 File Offset: 0x00054560
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "DELETEENTRY"))
				{
					if (!(a == "GETBACKUPQTY"))
					{
						if (!(a == "DELETESNENTRY"))
						{
							return;
						}
						if (!this.CheckSNRowDelete())
						{
							e.Cancel = true;
						}
					}
					else if (!this.isGetBackUpQty)
					{
						if (this.dataChanged)
						{
							base.View.ShowMessage(ResManager.LoadKDString("数据已发生变化，获取账存将丢失修改内容，是否保存并继续获取账存？", "004023030002287", 5, new object[0]), 4, delegate(MessageBoxResult result)
							{
								if (result == 6)
								{
									this.isGetBackUpQty = true;
									this.View.InvokeFormOperation("Save");
									e.Cancel = true;
									return;
								}
								e.Cancel = true;
							}, "", 0);
							return;
						}
						this.isGetBackUpQty = true;
						this.DoGetbackUpQty();
						this.isGetBackUpQty = false;
						return;
					}
				}
				else if (!this.CheckRowDelete())
				{
					e.Cancel = true;
					return;
				}
			}
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x00056457 File Offset: 0x00054657
		public override void AfterConfirmOperation(AfterConfirmOperationEventArgs e)
		{
			if (e.InteractionResult.Sponsor != "StockCountInputSaveZeroCountQtySpensor")
			{
				return;
			}
			if (!e.InteractionResult.InteractionContext.K3DisplayerModel.IsOK)
			{
				this.isGetBackUpQty = false;
			}
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x00056490 File Offset: 0x00054690
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string key;
			switch (key = e.BarItemKey.ToUpperInvariant())
			{
			case "TBQUERYCOUNTGAIN":
				this.QueryTargetleBill("STK_StockCountGain", "T_STK_STKCOUNTGAINENTRY");
				return;
			case "TBQUERYCOUNTLOSS":
				this.QueryTargetleBill("STK_StockCountLoss", "T_STK_STKCOUNTLOSSENTRY");
				return;
			case "TBQUERYCOUNTSCHEME":
				this.QuerySrcleBill("STK_StockCountScheme", this.strSource);
				return;
			case "TBQUERYCOUNTTABLE":
				this.QuerySrcleBill("STK_CycleCountTable", this.strSource);
				return;
			case "TBVIEWSN":
				this.ViewInputSerial();
				return;
			case "TBDIFRPT":
				this.ShowDiffReport();
				break;

				return;
			}
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x00056590 File Offset: 0x00054790
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			if (e.ParentKey.Equals("FEntrySerial"))
			{
				base.EntryBarItemClick(e);
				return;
			}
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBSELSN")
				{
					this.SelSerial();
					return;
				}
				if (a == "TBSCANSN")
				{
					this.ScanSerial();
					return;
				}
				if (a == "TBSNMASTER")
				{
					this.ViewSerialMaster();
					return;
				}
				if (a == "TBFINISHSCAN")
				{
					this.FinishScanSerial();
					return;
				}
			}
			base.EntryBarItemClick(e);
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x00056620 File Offset: 0x00054820
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

		// Token: 0x060006F0 RID: 1776 RVA: 0x00056714 File Offset: 0x00054914
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

		// Token: 0x060006F1 RID: 1777 RVA: 0x00056800 File Offset: 0x00054A00
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

		// Token: 0x060006F2 RID: 1778 RVA: 0x00056890 File Offset: 0x00054A90
		private void ViewSerialMaster()
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntrySerial");
			if (entryCurrentRowIndex < 0)
			{
				return;
			}
			OperationStatus operationStatus = 0;
			if (this.VaildatePermission("BD_SerialMainFile", "f323992d896745fbaab4a2717c79ce2e"))
			{
				operationStatus = 2;
			}
			else if (this.VaildatePermission("BD_SerialMainFile", "6e44119a58cb4a8e86f6c385e14a17ad"))
			{
				operationStatus = 1;
			}
			if (operationStatus == null)
			{
				base.View.ShowMessage(ResManager.LoadKDString("对不起，您没有序列号主档的查看权限!", "004023000012234", 5, new object[0]), 0);
				return;
			}
			DynamicObject value = BillUtils.GetValue<DynamicObject>(this.Model, "FStockOrgId", -1, null, null);
			long num = 0L;
			if (value != null)
			{
				num = Convert.ToInt64(value["Id"]);
			}
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(this._snEntity, entryCurrentRowIndex);
			if (entityDataObject == null || entityDataObject.Parent == null)
			{
				return;
			}
			string value2 = BillUtils.GetValue<string>(this.Model, "FSerialNo", entryCurrentRowIndex, null, null);
			if (string.IsNullOrEmpty(value2))
			{
				return;
			}
			DynamicObject dynamicObject = ((DynamicObject)entityDataObject.Parent)["MaterialId"] as DynamicObject;
			long num2 = 0L;
			if (dynamicObject != null)
			{
				num2 = Convert.ToInt64(dynamicObject[FormConst.MASTER_ID]);
			}
			long serialMainFileKey = SerialServiceHelper.GetSerialMainFileKey(base.View.Context, num, num2, value2, false);
			if (serialMainFileKey == 0L)
			{
				return;
			}
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "BD_SerialMainFile";
			billShowParameter.SyncCallBackAction = true;
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.PageId = Guid.NewGuid().ToString();
			billShowParameter.Status = operationStatus;
			billShowParameter.PKey = serialMainFileKey.ToString();
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x00056A34 File Offset: 0x00054C34
		private void SetSerialVisible()
		{
			bool visible = this._useSerialPara;
			string value = BillUtils.GetValue<string>(this.Model, "FSource", -1, null, null);
			if (!string.IsNullOrWhiteSpace(this.strSource) && value.Equals("1"))
			{
				long num = Convert.ToInt64(this.Model.GetValue("FSchemeId"));
				if (num > 0L)
				{
					OQLFilter oqlfilter = new OQLFilter();
					oqlfilter.Add(new OQLFilterHeadEntityItem
					{
						EntityKey = "FBillHead",
						FilterString = string.Format(" FID = {0}", num)
					});
					DynamicObject[] array = BusinessDataServiceHelper.Load(base.View.Context, "STK_StockCountScheme", null, oqlfilter);
					string text = (array[0]["BackUpType"] == null) ? "" : array[0]["BackUpType"].ToString();
					if (string.IsNullOrWhiteSpace(text) || StringUtils.EqualsIgnoreCase(text, "CloseDate"))
					{
						visible = false;
					}
				}
			}
			Control control = base.View.GetControl("FTab1_PSN");
			if (control != null)
			{
				control.Visible = visible;
			}
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x00056B50 File Offset: 0x00054D50
		private void ScanSerial()
		{
			List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>();
			if (!this.GetScanSerialPara(paras))
			{
				return;
			}
			this.ShowScanSerialForm(paras);
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x00056B74 File Offset: 0x00054D74
		private bool GetScanSerialPara(List<KeyValuePair<string, string>> paras)
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FBillEntry");
			DynamicObject dynamicObject = this.Model.GetValue("FMaterialId", entryCurrentRowIndex) as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) == 0L)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请先选中录入了物料的单据分录再使用序列号扫描功能！", "004023000012235", 5, new object[0]), "", 0);
				return false;
			}
			DynamicObject dynamicObject2 = this.Model.GetValue("FStockOrgId") as DynamicObject;
			paras.Add(new KeyValuePair<string, string>("UseOrgId", dynamicObject2["Id"].ToString()));
			paras.Add(new KeyValuePair<string, string>("UseOrgNumber", dynamicObject2["Number"].ToString()));
			paras.Add(new KeyValuePair<string, string>("UseOrgName", dynamicObject2["Name"].ToString()));
			paras.Add(new KeyValuePair<string, string>("MaterialMasterId", dynamicObject[FormConst.MASTER_ID].ToString()));
			paras.Add(new KeyValuePair<string, string>("MaterialNumber", dynamicObject["Number"].ToString()));
			paras.Add(new KeyValuePair<string, string>("MaterialName", dynamicObject["Name"].ToString()));
			paras.Add(new KeyValuePair<string, string>("MaterialModel", dynamicObject["Specification"].ToString()));
			paras.Add(new KeyValuePair<string, string>("FilterByMaterial", "1"));
			return true;
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00056CF8 File Offset: 0x00054EF8
		private void ShowScanSerialForm(List<KeyValuePair<string, string>> paras)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "STK_ScanSerial";
			dynamicFormShowParameter.Height = 718;
			dynamicFormShowParameter.Width = 931;
			foreach (KeyValuePair<string, string> keyValuePair in paras)
			{
				dynamicFormShowParameter.CustomParams.Add(keyValuePair.Key, keyValuePair.Value);
			}
			this._isScanning = true;
			base.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.ApplyScanSNReturnData));
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x00056DC4 File Offset: 0x00054FC4
		private void ApplyScanSNReturnData(FormResult ret)
		{
			base.View.Session.Remove("FormInputParam");
			if (ret == null || ret.ReturnData == null)
			{
				return;
			}
			List<SimpleSerialSnap> list = null;
			if (ret.ReturnData is List<KeyValuePair<SimpleSerialSnap, string>>)
			{
				List<KeyValuePair<SimpleSerialSnap, string>> list2 = ret.ReturnData as List<KeyValuePair<SimpleSerialSnap, string>>;
				if (list2 != null && list2.Count > 0)
				{
					list = (from p in list2
					select p.Key).ToList<SimpleSerialSnap>();
				}
			}
			else if (ret.ReturnData is List<SimpleSerialSnap>)
			{
				list = (ret.ReturnData as List<SimpleSerialSnap>);
			}
			if (list == null || list.Count < 1)
			{
				return;
			}
			this.WriteBackScanSerialData(list);
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x00056E94 File Offset: 0x00055094
		private void WriteBackScanSerialData(List<SimpleSerialSnap> rets)
		{
			List<SimpleSerialSnap> list = new List<SimpleSerialSnap>();
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(this._parEntryEntity.Key);
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(this._parEntryEntity, entryCurrentRowIndex);
			DynamicObjectCollection dynamicObjectCollection = this._snEntity.DynamicProperty.GetValue(entityDataObject) as DynamicObjectCollection;
			DynamicObject obj = entityDataObject["MaterialId"] as DynamicObject;
			long dynamicValue = this.GetDynamicValue(obj);
			if (dynamicValue < 1L)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请先录入物料再录入序列号！", "004023000012239", 5, new object[0]), "", 0);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			using (List<SimpleSerialSnap>.Enumerator enumerator = rets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SimpleSerialSnap sn = enumerator.Current;
					if (list.Any((SimpleSerialSnap p) => p.Number.Equals(sn.Number, StringComparison.OrdinalIgnoreCase)))
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("您录入的序列号【{0}】在集合内部重复！", "004023000012240", 5, new object[0]), sn.Number));
					}
					else if (dynamicValue != sn.MaterialMasterId)
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("您录入的序列号【{0}】所属物料与当前行不一致！", "004023000012241", 5, new object[0]), sn.Number));
					}
					else
					{
						list.Add(sn);
					}
				}
			}
			List<DynamicObject> list2 = new List<DynamicObject>();
			DynamicObjectType dynamicObjectType = this._snEntity.DynamicObjectType;
			foreach (SimpleSerialSnap simpleSerialSnap in list)
			{
				int num = -1;
				if (this.ValidateInputSerial(entityDataObject, simpleSerialSnap, ref num, true, stringBuilder, false))
				{
					DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
					dynamicObject["SerialNo"] = simpleSerialSnap.Number;
					dynamicObject["IsAccount"] = false;
					dynamicObject["Status"] = true;
					dynamicObject["IsGain"] = true;
					dynamicObject["IsLoss"] = false;
					dynamicObject["IsScaning"] = this._isScanning;
					list2.Add(dynamicObject);
				}
				else if (num >= 0)
				{
					dynamicObjectCollection[num]["IsScaning"] = this._isScanning;
					dynamicObjectCollection[num]["Status"] = true;
					dynamicObjectCollection[num]["IsLoss"] = false;
				}
			}
			if (list2.Count > 0)
			{
				List<int> list3 = new List<int>();
				int num2 = 0;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					if (dynamicObject2 == null || dynamicObject2["SerialNo"] == null || string.IsNullOrWhiteSpace(dynamicObject2["SerialNo"].ToString()))
					{
						list3.Add(num2);
					}
					num2++;
				}
				int count = dynamicObjectCollection.Count;
				int num3 = list2.Count - list3.Count;
				if (num3 > 0)
				{
					this.Model.BatchCreateNewEntryRow(this._snEntity.Key, num3);
					for (int i = 0; i < num3; i++)
					{
						list3.Add(count + i);
					}
				}
				int num4 = 0;
				foreach (int num5 in list3)
				{
					int num6 = num5 + 1;
					if (num6 >= count)
					{
						list2[num4]["Seq"] = num6;
					}
					dynamicObjectCollection[num5] = list2[num4];
					num4++;
				}
			}
			if (stringBuilder.Length > 0)
			{
				base.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
			}
			this.RefreshSnCount();
			if (this._isScanning)
			{
				base.View.GetBarItem("FEntrySerial", "tbFinishScan").Enabled = true;
				base.View.UpdateView(this._snEntity.Key);
			}
			this._isScanning = false;
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x00057354 File Offset: 0x00055554
		private void FinishScanSerial()
		{
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this._snEntity);
			DynamicProperty dynamicProperty = base.View.BusinessInfo.GetField("FSerialNo").DynamicProperty;
			DynamicProperty dynamicProperty2 = base.View.BusinessInfo.GetField("FStatus").DynamicProperty;
			DynamicProperty dynamicProperty3 = base.View.BusinessInfo.GetField("FIsAccount").DynamicProperty;
			DynamicProperty dynamicProperty4 = base.View.BusinessInfo.GetField("FIsScaning").DynamicProperty;
			for (int i = entityDataObject.Count - 1; i >= 0; i--)
			{
				DynamicObject dynamicObject = entityDataObject[i];
				string value = dynamicProperty.GetValue<string>(dynamicObject);
				if (string.IsNullOrWhiteSpace(value))
				{
					entityDataObject.RemoveAt(i);
				}
				else
				{
					bool value2 = dynamicProperty4.GetValue<bool>(dynamicObject);
					bool value3 = dynamicProperty3.GetValue<bool>(dynamicObject);
					if (!value2 && !value3)
					{
						entityDataObject.RemoveAt(i);
					}
					else
					{
						bool flag = dynamicProperty2.GetValue<bool>(dynamicObject);
						bool flag2 = false;
						bool flag3 = false;
						if (value3)
						{
							if (value2)
							{
								flag = true;
							}
							else
							{
								flag = false;
								flag3 = true;
							}
						}
						else
						{
							flag = true;
							flag2 = true;
						}
						dynamicObject["IsScaning"] = false;
						dynamicObject["Status"] = flag;
						dynamicObject["IsGain"] = flag2;
						dynamicObject["IsLoss"] = flag3;
					}
				}
			}
			base.View.GetBarItem("FEntrySerial", "tbFinishScan").Enabled = false;
			this.RefreshSnCount();
			base.View.UpdateView(this._snEntity.Key);
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x000574FC File Offset: 0x000556FC
		private void SelSerial()
		{
			List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>();
			if (!this.GetSelectSerialPara(paras))
			{
				return;
			}
			this.ShowSerialSelectForm(paras);
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x00057520 File Offset: 0x00055720
		private bool GetSelectSerialPara(List<KeyValuePair<string, string>> paras)
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FBillEntry");
			if (entryCurrentRowIndex < 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请先选中录入了物料的单据分录再使用序列号选择功能！", "004023000012236", 5, new object[0]), "", 0);
				return false;
			}
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(this._parEntryEntity, entryCurrentRowIndex);
			if (entityDataObject == null)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请先选中录入了物料的单据分录再使用序列号选择功能！", "004023000012236", 5, new object[0]), "", 0);
				return false;
			}
			Field field = base.View.BusinessInfo.GetField("FMaterialId");
			DynamicObject dynamicObject = field.DynamicProperty.GetValue(entityDataObject) as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) < 1L)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请先录入物料再使用序列号选择功能！", "004023000012237", 5, new object[0]), "", 0);
				return false;
			}
			DynamicObject dynamicObject2 = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
			if (!Convert.ToBoolean(dynamicObject2["IsSNManage"]))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("物料未启用序列号管理，不能选择序列号！", "004023000012238", 5, new object[0]), "", 0);
				return false;
			}
			paras.Add(new KeyValuePair<string, string>("InInvBillTypeSet", "ALL"));
			paras.Add(new KeyValuePair<string, string>("OutInvBillTypeSet", "ALL"));
			paras.Add(new KeyValuePair<string, string>("SNEntityKey", "FEntrySerial"));
			paras.Add(new KeyValuePair<string, string>("SNParentEntityKey", this._parEntryEntity.Key));
			paras.Add(new KeyValuePair<string, string>("BillDetailId", "0"));
			paras.Add(new KeyValuePair<string, string>("BillFormId", "STK_STKCountInput"));
			paras.Add(new KeyValuePair<string, string>("BillId", "0"));
			paras.Add(new KeyValuePair<string, string>("MaterialMasterId", dynamicObject[FormConst.MASTER_ID].ToString()));
			paras.Add(new KeyValuePair<string, string>("MaterialNumber", dynamicObject["Number"].ToString()));
			paras.Add(new KeyValuePair<string, string>("MaterialName", dynamicObject["Name"].ToString()));
			field = base.View.BusinessInfo.GetField("FStockOrgId");
			dynamicObject = (field.DynamicProperty.GetValue((DynamicObject)entityDataObject.Parent) as DynamicObject);
			paras.Add(new KeyValuePair<string, string>("OrgId", dynamicObject["Id"].ToString()));
			paras.Add(new KeyValuePair<string, string>("StockOrgId", dynamicObject["Id"].ToString()));
			paras.Add(new KeyValuePair<string, string>("UseableStates", " ,1,3"));
			paras.AddRange(this.GetInvFilter(entityDataObject));
			return true;
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x000577F4 File Offset: 0x000559F4
		private IEnumerable<KeyValuePair<string, string>> GetInvFilter(DynamicObject parData)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			StringBuilder stringBuilder = new StringBuilder();
			list.AddRange(this.GetOwnerKeepFilter(parData, stringBuilder));
			list.AddRange(this.GetStockFilter(parData, stringBuilder));
			list.AddRange(this.GetLotFilter(parData, stringBuilder));
			list.AddRange(this.GetAuxPropFilter(parData, stringBuilder));
			list.AddRange(this.GetProductFilter(parData, stringBuilder));
			if (stringBuilder.Length > 0)
			{
				string value = string.Format(" ( NOT ( {0} ) ) ", stringBuilder.ToString());
				list.Add(new KeyValuePair<string, string>("InvFilter", value));
			}
			return list;
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x00057880 File Offset: 0x00055A80
		private List<KeyValuePair<string, string>> GetAuxPropFilter(DynamicObject parData, StringBuilder invFilter)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			DynamicObject dynamicObject = parData["AuxPropId"] as DynamicObject;
			if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
			{
				invFilter.AppendFormat(" {0} TI.FAUXPROPID = @AuxPropId ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@AuxPropId", dynamicObject["Id"].ToString()));
			}
			return list;
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x00057900 File Offset: 0x00055B00
		private List<KeyValuePair<string, string>> GetProductFilter(DynamicObject parData, StringBuilder invFilter)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			DynamicObject obj = parData["BomId"] as DynamicObject;
			long dynamicValue = this.GetDynamicValue(obj);
			if (dynamicValue > 0L)
			{
				invFilter.AppendFormat(" {0} TI.FBOMID = @BOMId ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@BOMId", dynamicValue.ToString()));
			}
			string value = parData["MtoNo"] as string;
			if (!string.IsNullOrWhiteSpace(value))
			{
				invFilter.AppendFormat(" {0} TI.FMTONO = @MtoNo ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@MtoNo", value));
			}
			value = (parData["ProjectNo"] as string);
			if (!string.IsNullOrWhiteSpace(value))
			{
				invFilter.AppendFormat(" {0} TI.FPROJECTNO = @ProjectNo ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@ProjectNo", value));
			}
			return list;
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x00057A00 File Offset: 0x00055C00
		private List<KeyValuePair<string, string>> GetStockFilter(DynamicObject parData, StringBuilder invFilter)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			DynamicObject dynamicObject = parData["StockId"] as DynamicObject;
			long dynamicValue = this.GetDynamicValue(dynamicObject);
			if (dynamicValue > 0L)
			{
				invFilter.AppendFormat(" {0} TI.FSTOCKID = @StockId ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@StockId", dynamicValue.ToString()));
			}
			dynamicObject = (parData["StockLocId"] as DynamicObject);
			dynamicValue = this.GetDynamicValue(dynamicObject);
			if (dynamicValue > 0L)
			{
				invFilter.AppendFormat(" {0} TI.FSTOCKLOCID = @StockPlaceId ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@StockPlaceId", dynamicObject["Id"].ToString()));
			}
			dynamicObject = (parData["FStockStatusId"] as DynamicObject);
			dynamicValue = this.GetDynamicValue(dynamicObject);
			if (dynamicValue > 0L)
			{
				invFilter.AppendFormat(" {0} TI.FSTOCKSTATUSID = @StockStatusId ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@StockStatusId", dynamicObject["Id"].ToString()));
			}
			return list;
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x00057B28 File Offset: 0x00055D28
		private List<KeyValuePair<string, string>> GetOwnerKeepFilter(DynamicObject parData, StringBuilder invFilter)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			string value = parData["OwnerTypeId"] as string;
			if (!string.IsNullOrWhiteSpace(value))
			{
				invFilter.AppendFormat(" TI.FOWNERTYPEID = @OwnerType ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@OwnerType", value));
			}
			DynamicObject dynamicObject = parData["OwnerId"] as DynamicObject;
			if (dynamicObject != null)
			{
				long dynamicValue = this.GetDynamicValue(dynamicObject);
				if (dynamicValue > 0L)
				{
					invFilter.AppendFormat(" {0} TI.FOWNERID = @OwnerId ", (invFilter.Length < 1) ? "" : " AND ");
					list.Add(new KeyValuePair<string, string>("@@OwnerId", dynamicValue.ToString()));
				}
			}
			value = (parData["KeeperTypeId"] as string);
			if (!string.IsNullOrWhiteSpace(value))
			{
				invFilter.AppendFormat(" {0} TI.FKEEPERTYPEID = @KeeperType ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@KeeperType", value));
			}
			dynamicObject = (parData["KeeperId"] as DynamicObject);
			if (dynamicObject != null)
			{
				long dynamicValue2 = this.GetDynamicValue(dynamicObject);
				if (dynamicValue2 > 0L)
				{
					invFilter.AppendFormat(" {0} TI.FKEEPERID = @KeeperId ", (invFilter.Length < 1) ? "" : " AND ");
					list.Add(new KeyValuePair<string, string>("@@KeeperId", dynamicValue2.ToString()));
				}
			}
			return list;
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x00057C88 File Offset: 0x00055E88
		private List<KeyValuePair<string, string>> GetLotFilter(DynamicObject parData, StringBuilder invFilter)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			LotField lotField = base.View.BusinessInfo.GetField("FLot") as LotField;
			string value = lotField.TextDynamicProperty.GetValue<string>(parData);
			if (!string.IsNullOrWhiteSpace(value))
			{
				invFilter.AppendFormat(" {0} TL.FNUMBER = @LotNumber ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@LotNumber", value));
			}
			Field field = base.View.BusinessInfo.GetField("FProduceDate");
			object value2 = field.DynamicProperty.GetValue(parData);
			DateTime minValue = DateTime.MinValue;
			if (value2 != null && DateTime.TryParse(value2.ToString(), out minValue) && minValue > DateTime.MinValue)
			{
				invFilter.AppendFormat(" {0} (CASE WHEN TMS.FISEXPPARTOFLOT = '1' THEN TL.FPRODUCEDATE ELSE TI.FPRODUCEDATE END = @ProduceDate ) ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@ProduceDate", minValue.ToShortDateString()));
			}
			field = base.View.BusinessInfo.GetField("FExpiryDate");
			value2 = field.DynamicProperty.GetValue(parData);
			minValue = DateTime.MinValue;
			if (value2 != null && DateTime.TryParse(value2.ToString(), out minValue) && minValue > DateTime.MinValue)
			{
				invFilter.AppendFormat(" {0} (CASE WHEN TMS.FISEXPPARTOFLOT = '1' THEN TL.FEXPIRYDATE ELSE TI.FEXPIRYDATE END = @ExpiryDate ) ", (invFilter.Length < 1) ? "" : " AND ");
				list.Add(new KeyValuePair<string, string>("@@ExpiryDate", minValue.ToShortDateString()));
			}
			return list;
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x00057E14 File Offset: 0x00056014
		private void ShowSerialSelectForm(List<KeyValuePair<string, string>> paras)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "STK_SelectSerial";
			dynamicFormShowParameter.Height = 700;
			dynamicFormShowParameter.Width = 980;
			foreach (KeyValuePair<string, string> keyValuePair in paras)
			{
				dynamicFormShowParameter.CustomParams.Add(keyValuePair.Key, keyValuePair.Value);
			}
			base.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.ApplySelSNReturnData));
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x00057ED8 File Offset: 0x000560D8
		private void ApplySelSNReturnData(FormResult ret)
		{
			base.View.Session.Remove("FormInputParam");
			if (ret == null || ret.ReturnData == null)
			{
				return;
			}
			List<SimpleSerialSnap> list = null;
			if (ret.ReturnData is List<KeyValuePair<SimpleSerialSnap, string>>)
			{
				List<KeyValuePair<SimpleSerialSnap, string>> list2 = ret.ReturnData as List<KeyValuePair<SimpleSerialSnap, string>>;
				if (list2 != null && list2.Count > 0)
				{
					list = (from p in list2
					select p.Key).ToList<SimpleSerialSnap>();
				}
			}
			else if (ret.ReturnData is List<SimpleSerialSnap>)
			{
				list = (ret.ReturnData as List<SimpleSerialSnap>);
			}
			if (list == null || list.Count < 1)
			{
				return;
			}
			this.WriteBackSelSerialData(list);
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x00057FA8 File Offset: 0x000561A8
		private void WriteBackSelSerialData(List<SimpleSerialSnap> rets)
		{
			List<SimpleSerialSnap> list = new List<SimpleSerialSnap>();
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(this._parEntryEntity.Key);
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(this._parEntryEntity, entryCurrentRowIndex);
			DynamicObjectCollection dynamicObjectCollection = this._snEntity.DynamicProperty.GetValue(entityDataObject) as DynamicObjectCollection;
			DynamicObject obj = entityDataObject["MaterialId"] as DynamicObject;
			long dynamicValue = this.GetDynamicValue(obj);
			if (dynamicValue < 1L)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请先录入物料再录入序列号！", "004023000012239", 5, new object[0]), "", 0);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			using (List<SimpleSerialSnap>.Enumerator enumerator = rets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SimpleSerialSnap sn = enumerator.Current;
					if (list.Any((SimpleSerialSnap p) => p.Number.Equals(sn.Number, StringComparison.OrdinalIgnoreCase)))
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("您录入的序列号【{0}】在集合内部重复！", "004023000012240", 5, new object[0]), sn.Number));
					}
					else if (dynamicValue != sn.MaterialMasterId)
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("您录入的序列号【{0}】所属物料与当前行不一致！", "004023000012241", 5, new object[0]), sn.Number));
					}
					else
					{
						list.Add(sn);
					}
				}
			}
			List<int> list2 = new List<int>();
			int num = 0;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject == null || dynamicObject["SerialNo"] == null || string.IsNullOrWhiteSpace(dynamicObject["SerialNo"].ToString()))
				{
					list2.Add(num);
				}
				num++;
			}
			int count = dynamicObjectCollection.Count;
			int num2 = list.Count - list2.Count;
			if (num2 > 0)
			{
				this.Model.BatchCreateNewEntryRow(this._snEntity.Key, num2);
				for (int i = 0; i < num2; i++)
				{
					list2.Add(count + i);
				}
			}
			num = 0;
			foreach (SimpleSerialSnap simpleSerialSnap in list)
			{
				int num3 = -1;
				if (this.ValidateInputSerial(entityDataObject, simpleSerialSnap, ref num3, true, stringBuilder, false))
				{
					dynamicObjectCollection[list2[num]]["SerialNo"] = simpleSerialSnap.Number;
					dynamicObjectCollection[list2[num]]["IsAccount"] = false;
					dynamicObjectCollection[list2[num]]["Status"] = true;
					dynamicObjectCollection[list2[num]]["IsGain"] = true;
					dynamicObjectCollection[list2[num]]["IsLoss"] = false;
					dynamicObjectCollection[list2[num]]["IsScaning"] = false;
					num++;
				}
			}
			if (stringBuilder.Length > 0)
			{
				base.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
			}
			int num4 = Convert.ToInt32(entityDataObject["GainSNCount"]) + num;
			this.Model.SetValue("FGainSNCount", num4, entryCurrentRowIndex);
			base.View.InvokeFieldUpdateService("FGainSNCount", entryCurrentRowIndex);
			base.View.UpdateView(this._snEntity.Key);
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x00058394 File Offset: 0x00056594
		private bool ValidateInputSerial(int snRowIndex, string serialNo, ref int existIndex, out SimpleSerialSnap serial)
		{
			serial = null;
			existIndex = -1;
			if (serialNo == null || string.IsNullOrWhiteSpace(serialNo))
			{
				return true;
			}
			DynamicObject dynamicObject = this.Model.GetEntityDataObject(this._snEntity, snRowIndex).Parent as DynamicObject;
			DynamicObjectCollection dynamicObjectCollection = this._snEntity.DynamicProperty.GetValue(dynamicObject) as DynamicObjectCollection;
			int num = 1;
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				if (dynamicObject2["SerialNo"] != null && serialNo.Equals(dynamicObject2["SerialNo"].ToString(), StringComparison.OrdinalIgnoreCase))
				{
					base.View.ShowMessage(string.Format(ResManager.LoadKDString("您输入的序列号【{0}】与第{1}行重复", "004023000012242", 5, new object[0]), serialNo, num), 0);
					return false;
				}
				num++;
			}
			return this.GetSerialByNumber(dynamicObject, serialNo, out serial) && (serial == null || this.ValidateInputSerial(dynamicObject, serial, ref existIndex, false, null, true));
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x000584A8 File Offset: 0x000566A8
		private bool ValidateInputSerial(DynamicObject parData, SimpleSerialSnap serial, ref int existIndex, bool validDuplicate = false, StringBuilder errBuilder = null, bool showErr = true)
		{
			existIndex = -1;
			string text = "";
			if (validDuplicate)
			{
				DynamicObjectCollection dynamicObjectCollection = this._snEntity.DynamicProperty.GetValue(parData) as DynamicObjectCollection;
				int num = 0;
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (dynamicObject["SerialNo"] != null && serial.Number.Equals(dynamicObject["SerialNo"].ToString(), StringComparison.OrdinalIgnoreCase))
					{
						existIndex = num;
						if (!this._isScanning)
						{
							text = string.Format(ResManager.LoadKDString("您输入的序列号【{0}】与第{1}行重复", "004023000012242", 5, new object[0]), serial.Number, num);
							if (errBuilder != null)
							{
								errBuilder.AppendLine(text);
							}
							if (showErr)
							{
								base.View.ShowMessage(text, 0);
							}
						}
						return false;
					}
					num++;
				}
			}
			if (serial.ForBidStatus == "B")
			{
				text = string.Format(ResManager.LoadKDString("您输入的序列号已经被禁用", "004023000012243", 5, new object[0]), new object[0]);
				if (errBuilder != null)
				{
					errBuilder.AppendLine(text);
				}
				if (showErr)
				{
					base.View.ShowMessage(text, 0);
				}
				return false;
			}
			bool result = true;
			text = "";
			string a;
			if ((a = serial.State.Trim()) != null)
			{
				if (a == "" || a == "3")
				{
					goto IL_1FB;
				}
				if (a == "0" || a == "2")
				{
					text = string.Format(ResManager.LoadKDString("您录入的序列号【{0}】状态未确认，不允许使用！", "004023000012244", 5, new object[0]), serial.Number);
					result = false;
					goto IL_1FB;
				}
				if (a == "1")
				{
					if (!this.ValidateSerialInvFieldFilter(parData, serial, out text))
					{
						result = false;
						goto IL_1FB;
					}
					goto IL_1FB;
				}
			}
			text = string.Format(ResManager.LoadKDString("您录入的序列号【{0}】状态不明，不允许使用！", "004023000012245", 5, new object[0]), serial.Number);
			result = false;
			IL_1FB:
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (errBuilder != null)
				{
					errBuilder.AppendLine(text);
				}
				if (showErr)
				{
					base.View.ShowErrMessage(text, "", 0);
				}
			}
			return result;
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x000586F0 File Offset: 0x000568F0
		private bool GetSerialByNumber(DynamicObject parData, string serialNumber, out SimpleSerialSnap serial)
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)parData["StkCountInputSerial"];
			Field field = base.View.BusinessInfo.GetField("FMaterialId");
			DynamicObject value = field.DynamicProperty.GetValue<DynamicObject>(parData);
			long num = 0L;
			if (value != null)
			{
				num = Convert.ToInt64(value[FormConst.MASTER_ID]);
			}
			field = base.View.BusinessInfo.GetField("FStockOrgId");
			value = field.DynamicProperty.GetValue<DynamicObject>((DynamicObject)parData.Parent);
			long num2 = 0L;
			if (value != null)
			{
				num2 = Convert.ToInt64(value["Id"]);
			}
			if (num <= 0L || num2 <= 0L)
			{
				serial = null;
				return false;
			}
			List<SimpleSerialSnap> serials = SerialServiceHelper.GetSerialByNumber(base.View.Context, this._baseDataOrgCtl, serialNumber, num2, num2, num, false, false).ToList<SimpleSerialSnap>();
			return this.FindMatchSingleSerial(serials, num2, num, serialNumber, out serial);
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x00058838 File Offset: 0x00056A38
		private bool FindMatchSingleSerial(List<SimpleSerialSnap> serials, long stockOrgId, long materialId, string serialNumber, out SimpleSerialSnap serial)
		{
			serial = null;
			if (serials == null || serials.Count<SimpleSerialSnap>() < 1)
			{
				return true;
			}
			if (this._snManageLevel == "A" && serials.Count((SimpleSerialSnap p) => p.StockOrgId != stockOrgId || p.MaterialMasterId != materialId) > 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("你输入的序列号已经被使用", "004023000012246", 5, new object[0]), 0);
				return false;
			}
			if (this._snManageLevel == "O" && serials.Count((SimpleSerialSnap p) => p.StockOrgId == stockOrgId && p.MaterialMasterId != materialId) > 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("你输入的序列号已经被使用", "004023000012246", 5, new object[0]), 0);
				return false;
			}
			serial = serials.FirstOrDefault((SimpleSerialSnap p) => p.StockOrgId == stockOrgId && p.MaterialMasterId == materialId);
			return true;
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x0005892C File Offset: 0x00056B2C
		private bool ValidateSerialInvFieldFilter(DynamicObject parData, SimpleSerialSnap serial, out string strErrMsg)
		{
			strErrMsg = "";
			DynamicObject dynamicObject = parData["MaterialId"] as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) == 0L)
			{
				strErrMsg = ResManager.LoadKDString("请先录入物料！", "004023000012247", 5, new object[0]);
				return false;
			}
			if (serial == null)
			{
				return true;
			}
			if (!string.IsNullOrWhiteSpace(serial.State) && !serial.State.Equals("1"))
			{
				return true;
			}
			if (serial.AuxPropid > 0L)
			{
				DynamicObject dynamicObject2 = parData["AuxPropId"] as DynamicObject;
				if (dynamicObject2 == null)
				{
					return true;
				}
				long num = 0L;
				if (dynamicObject2 != null)
				{
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialAuxPty"] as DynamicObjectCollection;
					if (dynamicObjectCollection != null)
					{
						if (dynamicObjectCollection.Count((DynamicObject p) => Convert.ToBoolean(p["IsEnable1"])) > 0)
						{
							string key = Common.FlexValToString("BD_FLEXSITEMDETAILV", dynamicObject2);
							if (!this._auxProps.TryGetValue(key, out num))
							{
								num = FlexServiceHelper.GetFlexDataId(base.Context, dynamicObject2, "BD_FLEXSITEMDETAILV");
								this._auxProps[key] = num;
							}
							if (num == 0L)
							{
								return true;
							}
							if (num != serial.AuxPropid)
							{
								return true;
							}
						}
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(serial.KeeperType))
			{
				object obj = parData["KeeperTypeId"];
				string text = (obj == null) ? "" : obj.ToString();
				if (!serial.KeeperType.Equals(text))
				{
					return true;
				}
			}
			if (serial.KeeperMasterId > 0L)
			{
				DynamicObject dynamicObject2 = parData["KeeperId"] as DynamicObject;
				long dynamicValue = this.GetDynamicValue(dynamicObject2);
				if (dynamicValue != serial.KeeperMasterId)
				{
					return true;
				}
			}
			if (!string.IsNullOrWhiteSpace(serial.OwnerType))
			{
				object obj = parData["OwnerTypeId"];
				string text = (obj == null) ? "" : obj.ToString();
				if (!serial.OwnerType.Equals(text))
				{
					return true;
				}
			}
			if (serial.OwnerMasterId > 0L)
			{
				DynamicObject dynamicObject2 = parData["OwnerId"] as DynamicObject;
				long dynamicValue = this.GetDynamicValue(dynamicObject2);
				if (dynamicValue != serial.OwnerMasterId)
				{
					return true;
				}
			}
			if (serial.StockId > 0L)
			{
				DynamicObject dynamicObject2 = parData["StockId"] as DynamicObject;
				long dynamicValue = this.GetDynamicValue(dynamicObject2);
				if (dynamicValue != serial.StockId)
				{
					return true;
				}
			}
			if (serial.StockLocId > 0L)
			{
				DynamicObject dynamicObject2 = parData["StockLocId"] as DynamicObject;
				long dynamicValue = this.GetDynamicValue(dynamicObject2);
				if (dynamicValue != serial.StockLocId)
				{
					return true;
				}
			}
			if (serial.StockStatusId > 0L)
			{
				DynamicObject dynamicObject2 = parData["FStockStatusId"] as DynamicObject;
				long dynamicValue = this.GetDynamicValue(dynamicObject2);
				if (dynamicValue != serial.StockStatusId)
				{
					return true;
				}
			}
			if (!string.IsNullOrWhiteSpace(serial.LotNumber))
			{
				LotField lotField = base.View.BusinessInfo.GetField("FLot") as LotField;
				object obj = lotField.TextDynamicProperty.GetValue(parData);
				if (obj == null)
				{
					return true;
				}
				string text = obj.ToString();
				if (!text.Equals(serial.LotNumber))
				{
					return true;
				}
			}
			if (serial.ProduceDate != null)
			{
				object obj = parData["ProduceDate"];
				string text = (obj == null) ? "" : obj.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					return true;
				}
				if (DateTime.Parse(text) != serial.ProduceDate)
				{
					return true;
				}
			}
			if (serial.ExpiryDate != null)
			{
				object obj = parData["ExpiryDate"];
				string text = (obj == null) ? "" : obj.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					return true;
				}
				if (DateTime.Parse(text) != serial.ProduceDate)
				{
					return true;
				}
			}
			if (serial.BomId > 0L)
			{
				DynamicObject dynamicObject2 = parData["BomId"] as DynamicObject;
				long dynamicValue = this.GetDynamicValue(dynamicObject2);
				if (dynamicValue != serial.BomId)
				{
					return true;
				}
			}
			if (!string.IsNullOrWhiteSpace(serial.MtoNo))
			{
				object obj = parData["MtoNo"];
				string text = (obj == null) ? "" : obj.ToString();
				if (!serial.KeeperType.Equals(text))
				{
					return true;
				}
			}
			if (!string.IsNullOrWhiteSpace(serial.ProjectNo))
			{
				object obj = parData["ProjectNo"];
				string text = (obj == null) ? "" : obj.ToString();
				if (!serial.KeeperType.Equals(text))
				{
					return true;
				}
			}
			strErrMsg = string.Format(ResManager.LoadKDString("您输入的序列号【{0}】在库，且维度与当前分录完全一致，不允许盘盈！", "004023000012248", 5, new object[0]), serial.Number);
			return false;
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x00058DCC File Offset: 0x00056FCC
		private bool CheckSNRowDelete()
		{
			bool result = true;
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntrySerial");
			if (BillUtils.GetValue<bool>(this.Model, "FIsAccount", entryCurrentRowIndex, false, null))
			{
				base.View.ShowMessage(ResManager.LoadKDString("不能删除账存序列号！", "004023000012249", 5, new object[0]), 0);
				result = false;
			}
			return result;
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x00058E28 File Offset: 0x00057028
		private void RefreshSnCount()
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FBillEntry");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(this._parEntryEntity, entryCurrentRowIndex);
			DynamicObjectCollection dynamicObjectCollection = entityDataObject["StkCountInputSerial"] as DynamicObjectCollection;
			if (entityDataObject == null || dynamicObjectCollection == null || dynamicObjectCollection.Count < 1)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			decimal num3 = 0m;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject["SerialNo"] != null && !string.IsNullOrWhiteSpace(dynamicObject["SerialNo"].ToString()))
				{
					if (Convert.ToBoolean(dynamicObject["IsGain"]))
					{
						num++;
					}
					if (Convert.ToBoolean(dynamicObject["IsLoss"]))
					{
						num2++;
					}
					if (Convert.ToBoolean(dynamicObject["Status"]))
					{
						num3 = ++num3;
					}
				}
			}
			this.Model.SetValue("FGainSNCount", num, entryCurrentRowIndex);
			base.View.InvokeFieldUpdateService("FGainSNCount", entryCurrentRowIndex);
			this.Model.SetValue("FLossSNCount", num2, entryCurrentRowIndex);
			base.View.InvokeFieldUpdateService("FLossSNCount", entryCurrentRowIndex);
			decimal num4 = 0m;
			if (num3 > 0m)
			{
				GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs();
				Field field = base.View.BusinessInfo.GetField("FMaterialId");
				DynamicObject value = field.DynamicProperty.GetValue<DynamicObject>(entityDataObject);
				if (value == null)
				{
					return;
				}
				getUnitConvertRateArgs.MaterialId = Convert.ToInt64(value["Id"]);
				field = base.View.BusinessInfo.GetField("FUnitID");
				value = field.DynamicProperty.GetValue<DynamicObject>(entityDataObject);
				if (value == null)
				{
					return;
				}
				getUnitConvertRateArgs.DestUnitId = Convert.ToInt64(value["Id"]);
				field = base.View.BusinessInfo.GetField("FSNUnitID");
				value = field.DynamicProperty.GetValue<DynamicObject>(entityDataObject);
				if (value == null)
				{
					return;
				}
				getUnitConvertRateArgs.SourceUnitId = Convert.ToInt64(value["Id"]);
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, getUnitConvertRateArgs);
				num4 = unitConvertRate.ConvertQty(num3, "");
			}
			this.Model.SetValue("FCountQty", num4, entryCurrentRowIndex);
			base.View.InvokeFieldUpdateService("FCountQty", entryCurrentRowIndex);
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x000590BC File Offset: 0x000572BC
		private void SetSnLock()
		{
			bool flag = false;
			bool flag2 = true;
			if (base.View.OpenParameter.Status == 1)
			{
				flag2 = false;
			}
			string value = BillUtils.GetValue<string>(this.Model, "FDocumentStatus", -1, "C", null);
			if (flag2 && this._useSerialPara && !ObjectUtils.IsNullOrEmpty(value) && value != "B" && value != "C")
			{
				int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(this._parEntryEntity.Key);
				DynamicObject value2 = BillUtils.GetValue<DynamicObject>(this.Model, "FMaterialId", entryCurrentRowIndex, null, null);
				if (value2 != null && Convert.ToInt64(value2["Id"]) > 0L)
				{
					DynamicObject dynamicObject = ((DynamicObjectCollection)value2["MaterialStock"])[0];
					if (dynamicObject != null)
					{
						flag = Convert.ToBoolean(dynamicObject["IsSNManage"]);
					}
				}
			}
			base.View.LockField("FSerialNo", flag);
			base.View.LockField("FSerialId", flag);
			base.View.LockField("FStatus", flag);
			base.View.LockField("FIsGain", false);
			base.View.LockField("FIsLoss", false);
			base.View.LockField("FIsAccount", false);
			base.View.LockField("FIsScaning", flag);
			base.View.GetControl<EntryGrid>(this._snEntity.Key);
			BarDataManager menu = base.View.LayoutInfo.GetEntryEntityAppearance(this._snEntity.Key).Menu;
			if (menu == null || menu.BarItems == null || menu.BarItems.Count < 1)
			{
				return;
			}
			foreach (BarItem barItem in menu.BarItems)
			{
				BarItemControl barItem2 = base.View.GetBarItem(this._snEntity.Key, barItem.Name);
				if (barItem.Name.ToUpperInvariant().Equals("TBSNMASTER"))
				{
					barItem2.Enabled = this._useSerialPara;
				}
				else
				{
					barItem2.Enabled = flag;
				}
			}
			bool enabled = false;
			if (flag)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this._snEntity);
				foreach (DynamicObject dynamicObject2 in entityDataObject)
				{
					if (dynamicObject2["SerialNo"] != null && !string.IsNullOrWhiteSpace(dynamicObject2["SerialNo"].ToString()) && dynamicObject2["IsScaning"] != null && !string.IsNullOrWhiteSpace(dynamicObject2["IsScaning"].ToString()) && Convert.ToBoolean(dynamicObject2["IsScaning"]))
					{
						enabled = true;
						break;
					}
				}
			}
			base.View.GetBarItem("FEntrySerial", "tbFinishScan").Enabled = enabled;
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x000593D8 File Offset: 0x000575D8
		private void ViewInputSerial()
		{
			List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>();
			if (!this.GetParaFromBillView(paras))
			{
				return;
			}
			this.ShowViewSerialForm(paras);
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x000593FC File Offset: 0x000575FC
		private bool GetParaFromBillView(List<KeyValuePair<string, string>> paras)
		{
			DynamicObject dataObject = base.View.Model.DataObject;
			if (!dataObject.DataEntityState.FromDatabase)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请先保存单据再执行序列号查询功能！", "004023000012250", 5, new object[0]), 0);
				return false;
			}
			if (!SerialServiceHelper.BillHaveSerial(base.View.Context, base.View.BillBusinessInfo, Convert.ToInt64(dataObject["Id"])))
			{
				base.View.ShowMessage(ResManager.LoadKDString("单据未录入序列号！", "004023000012251", 5, new object[0]), 0);
				return false;
			}
			string value = "1";
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, "STK_StkCntInputUserSet", true) as FormMetadata;
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.View.Context, formMetadata.BusinessInfo, base.View.Context.UserId, "STK_StockCountInput", "UserParameter");
			if (dynamicObject != null && !Convert.ToBoolean(dynamicObject["OnlyShowEffectSN"]))
			{
				value = "0";
			}
			paras.Add(new KeyValuePair<string, string>("BillFormId", base.View.BillBusinessInfo.GetForm().Id));
			paras.Add(new KeyValuePair<string, string>("BillId", dataObject["Id"].ToString()));
			paras.Add(new KeyValuePair<string, string>("OnlyShowEffectSN", value));
			return true;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x00059564 File Offset: 0x00057764
		private void ShowViewSerialForm(List<KeyValuePair<string, string>> paras)
		{
			SysReportShowParameter sysReportShowParameter = new SysReportShowParameter();
			sysReportShowParameter.ParentPageId = base.View.PageId;
			sysReportShowParameter.MultiSelect = false;
			sysReportShowParameter.FormId = "STK_BillSerialRpt";
			sysReportShowParameter.Height = 700;
			sysReportShowParameter.Width = 950;
			sysReportShowParameter.IsShowFilter = false;
			foreach (KeyValuePair<string, string> keyValuePair in paras)
			{
				sysReportShowParameter.CustomParams.Add(keyValuePair.Key, keyValuePair.Value);
			}
			base.View.ShowForm(sysReportShowParameter);
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x00059618 File Offset: 0x00057818
		private void ShowDiffReport()
		{
			DynamicObject dataObject = base.View.Model.DataObject;
			if (!dataObject.DataEntityState.FromDatabase)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请先保存单据再执行存盘点差异报告查询功能！", "004023000014247", 5, new object[0]), 0);
				return;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = "STK_StkCountDiffRpt"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有库存盘点差异报告的查看权限！", "004023000014248", 5, new object[0]), "", 0);
				return;
			}
			MoveReportShowParameter moveReportShowParameter = new MoveReportShowParameter();
			moveReportShowParameter.ParentPageId = base.View.PageId;
			moveReportShowParameter.MultiSelect = false;
			moveReportShowParameter.FormId = "STK_StkCountDiffRpt";
			moveReportShowParameter.Height = 700;
			moveReportShowParameter.Width = 950;
			moveReportShowParameter.IsShowFilter = false;
			moveReportShowParameter.CustomParams.Add("FromBill", "1");
			moveReportShowParameter.CustomParams.Add("InputBillIds", this.Model.DataObject["Id"].ToString());
			base.View.ShowForm(moveReportShowParameter);
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x00059750 File Offset: 0x00057950
		private void DoGetbackUpQty()
		{
			bool flag;
			if (this.strSource == CountSource.CYCLECOUNTPLAN)
			{
				flag = StockServiceHelper.GetBackUpQty(base.View.Context, Convert.ToInt64(this.Model.DataObject["Id"]), Convert.ToInt64(this.Model.DataObject["StockOrgId_Id"]));
			}
			else
			{
				flag = StockServiceHelper.GetDateBackInputUpQty(base.View.Context, Convert.ToInt64(this.Model.DataObject["Id"]), Convert.ToInt64(this.Model.DataObject["StockOrgId_Id"]));
			}
			if (flag)
			{
				base.View.Refresh();
			}
			else
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("获取账存数失败！", "004023030002290", 5, new object[0]), "", 0);
			}
			this.isGetBackUpQty = false;
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x0005983C File Offset: 0x00057A3C
		private bool CheckRowDelete()
		{
			bool result = true;
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FBillEntry");
			decimal d = Convert.ToDecimal(this.Model.GetValue("FAcctQty", entryCurrentRowIndex));
			if (d > 0m)
			{
				base.View.ShowMessage(ResManager.LoadKDString("不能删除有账存余额的行！", "004023030002293", 5, new object[0]), 0);
				result = false;
			}
			return result;
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x000598A8 File Offset: 0x00057AA8
		private bool GetOwnerFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FOWNERID")
			{
				object value = base.View.Model.GetValue("FOwnerTypeId", row);
				string a2 = "";
				if (value != null)
				{
					a2 = Convert.ToString(value);
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				if (a2 == "BD_OwnerOrg")
				{
					List<SelectorItemInfo> list = new List<SelectorItemInfo>();
					list.Add(new SelectorItemInfo("FOrgId"));
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "ORG_BizRelation",
						FilterClauseWihtKey = string.Format("FRelationOrgID={0} and FBRTypeId=112", num),
						SelectItems = list
					};
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
					if (dynamicObjectCollection.Count > 0)
					{
						filter = string.Format(" FORGID in (SELECT {0} UNION (select t0.FORGID from T_ORG_BIZRELATIONENTRY t0\r\n                                                    left join T_ORG_BIZRELATION t1 on t0.FBIZRELATIONID=t1.FBIZRELATIONID\r\n                                                    where t1.FBRTYPEID=112 and t0.FRELATIONORGID={1}))", num, num);
					}
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x000599E0 File Offset: 0x00057BE0
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
					if (a == "FEXTAUXUNITID")
					{
						filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
					}
				}
				else
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
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x00059B14 File Offset: 0x00057D14
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockId", row) as DynamicObject;
			if (dynamicObject != null)
			{
				string text = Convert.ToString(dynamicObject["FStockStatusType"]);
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = "'" + text.Replace(",", "','") + "'";
					filter = string.Format(" FType IN ({0})", text);
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x00059BA4 File Offset: 0x00057DA4
		private void MarkMinusAcctRow()
		{
			EntryGrid entryGrid = base.View.GetControl("FBillEntry") as EntryGrid;
			DynamicObjectCollection dynamicObjectCollection = this._parEntryEntity.DynamicProperty.GetValue(this.Model.DataObject) as DynamicObjectCollection;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				if (Convert.ToDecimal(this.Model.GetValue("FBaseAcctQty", i)) < 0m)
				{
					list.Add(new KeyValuePair<int, string>(i, "#FFEC6E"));
				}
			}
			if (list.Count > 0)
			{
				entryGrid.SetRowBackcolor(list);
			}
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x00059C44 File Offset: 0x00057E44
		private void QueryTargetleBill(string formID, string entiryTB)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = formID
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有对应单据的查看权限！", "004023030002107", 5, new object[0]), "", 0);
				return;
			}
			long num = Convert.ToInt64(this.Model.DataObject["Id"]);
			if (num < 1L)
			{
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FID"));
			string filterClauseWihtKey = string.Format(" EXISTS (select 1 from {1} E INNER JOIN {1}_LK  L ON E.FENTRYID=L.FENTRYID\r\n                                        WHERE  E.FID= FID AND L.FSBILLID ={0}) ", num, entiryTB);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = formID,
				FilterClauseWihtKey = filterClauseWihtKey,
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				return;
			}
			long[] array = new long[dynamicObjectCollection.Count];
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				array[i] = Convert.ToInt64(dynamicObjectCollection[i]["FID"]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formID;
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", array));
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x00059DFC File Offset: 0x00057FFC
		private void QuerySrcleBill(string formID, string source)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = formID
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有对应单据的查看权限！", "004023030002107", 5, new object[0]), "", 0);
				return;
			}
			long num = Convert.ToInt64(this.Model.DataObject["Id"]);
			if (num < 1L)
			{
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FSchemeId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_StockCountInput",
				FilterClauseWihtKey = string.Format(" FSource ={0} AND FID = {1}  ", source, num),
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				return;
			}
			long[] array = new long[dynamicObjectCollection.Count];
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				array[i] = Convert.ToInt64(dynamicObjectCollection[i]["FSchemeId"]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formID;
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", array));
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x00059FB4 File Offset: 0x000581B4
		private bool IsChangeSysEntryInvField(string fieldKey, int rowIndex, object oldValue, object newValue)
		{
			if (!Convert.ToBoolean(this.Model.GetValue("FIsSystem", rowIndex)))
			{
				return false;
			}
			bool flag = false;
			long num = 0L;
			long num2 = 0L;
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FSTOCKID":
			case "FOWNERID":
			case "FKEEPERID":
			case "FBOMID":
			case "FSTOCKSTATUSID":
			case "FMATERIALID":
				if (oldValue is DynamicObject && oldValue != null)
				{
					num = Convert.ToInt64(((DynamicObject)oldValue)["Id"]);
				}
				if (newValue is DynamicObject && newValue != null)
				{
					num2 = Convert.ToInt64(((DynamicObject)newValue)["Id"]);
				}
				flag = (num != num2);
				break;
			case "FSTOCKLOCID":
			case "FAUXPROPID":
				flag = !this.IsSameDynamicObject(oldValue as DynamicObject, newValue as DynamicObject);
				break;
			case "FOWNERTYPEID":
			case "FKEEPERTYPEID":
			case "FPROJECTNO":
			case "FMTONO":
			{
				string a = "";
				string b = "";
				if (oldValue != null && !string.IsNullOrWhiteSpace(oldValue.ToString()))
				{
					a = oldValue.ToString().Trim();
				}
				if (newValue != null && !string.IsNullOrWhiteSpace(newValue.ToString()))
				{
					b = newValue.ToString().Trim();
				}
				flag = (a != b);
				break;
			}
			case "FLOT":
			{
				LotField lotField = this.Model.BusinessInfo.GetField(fieldKey) as LotField;
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(this._parEntryEntity, rowIndex);
				oldValue = lotField.TextDynamicProperty.GetValue(entityDataObject);
				string a = "";
				string b = "";
				object obj = newValue;
				if (newValue is DynamicObject)
				{
					obj = ((DynamicObject)newValue)["Number"];
				}
				if (oldValue != null && !string.IsNullOrWhiteSpace(oldValue.ToString()))
				{
					a = oldValue.ToString().Trim();
				}
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					b = obj.ToString().Trim();
				}
				flag = (a != b);
				break;
			}
			case "FPRODUCEDATE":
			case "FEXPIRYDATE":
			{
				DateTime d = DateTime.MinValue;
				DateTime d2 = DateTime.MinValue;
				if (oldValue is DateTime)
				{
					d = Convert.ToDateTime(oldValue);
				}
				if (newValue is DateTime)
				{
					d2 = Convert.ToDateTime(newValue);
				}
				else if (newValue != null && !string.IsNullOrWhiteSpace(newValue.ToString()))
				{
					DateTime.TryParse(newValue.ToString(), out d2);
				}
				flag = (d != d2);
				break;
			}
			}
			if (flag)
			{
				Field field = this.Model.BusinessInfo.GetField(fieldKey);
				throw new KDException("SysEntryFieldChange", string.Format(ResManager.LoadKDString("盘点作业生成的分录不允许修改【{0}】字段的值！", "004023030002296", 5, new object[0]), field.Name));
			}
			return flag;
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x0005A338 File Offset: 0x00058538
		private bool IsSameDynamicObject(DynamicObject dynamicObjectL, DynamicObject dynamicObjectR)
		{
			if (dynamicObjectL == null ^ dynamicObjectR == null)
			{
				return false;
			}
			if (dynamicObjectL == null && dynamicObjectR == null)
			{
				return true;
			}
			DynamicObjectType dynamicObjectType = dynamicObjectL.DynamicObjectType;
			if (dynamicObjectType != dynamicObjectR.DynamicObjectType)
			{
				return false;
			}
			bool result = true;
			foreach (DynamicProperty dynamicProperty in dynamicObjectType.Properties)
			{
				if (!(dynamicProperty.Name == "Id"))
				{
					object value = dynamicProperty.GetValue(dynamicObjectL);
					object value2 = dynamicProperty.GetValue(dynamicObjectR);
					if (dynamicProperty.PropertyType == typeof(string))
					{
						string a = (value == null) ? "" : value.ToString().Trim();
						string b = (value2 == null) ? "" : value2.ToString().Trim();
						if (a != b)
						{
							result = false;
							break;
						}
					}
					else if (dynamicProperty.PropertyType == typeof(DynamicObject))
					{
						string a = (value == null) ? " " : ((DynamicObject)value)["Id"].ToString().Trim();
						string b = (value2 == null) ? " " : ((DynamicObject)value2)["Id"].ToString().Trim();
						if (a != b)
						{
							result = false;
							break;
						}
					}
					else if (value != value2)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x0005A4D4 File Offset: 0x000586D4
		private bool VaildatePermission(string billFormId, string strPermItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = billFormId,
				SubSystemId = base.View.Model.SubSytemId
			}, strPermItemId);
			return permissionAuthResult.Passed;
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x0005A520 File Offset: 0x00058720
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

		// Token: 0x04000281 RID: 641
		private bool isGetBackUpQty;

		// Token: 0x04000282 RID: 642
		private string strSource;

		// Token: 0x04000283 RID: 643
		private bool dataChanged;

		// Token: 0x04000284 RID: 644
		private DynamicObject oldStockLoc;

		// Token: 0x04000285 RID: 645
		private DynamicObject oldAuxProp;

		// Token: 0x04000286 RID: 646
		private int stockLocRowIndex = -1;

		// Token: 0x04000287 RID: 647
		private int auxPropRowIndex = -1;

		// Token: 0x04000288 RID: 648
		private bool hasAsked;

		// Token: 0x04000289 RID: 649
		private bool _useSerialPara;

		// Token: 0x0400028A RID: 650
		private string _snManageLevel = "O";

		// Token: 0x0400028B RID: 651
		private EntryEntity _snEntity;

		// Token: 0x0400028C RID: 652
		private EntryEntity _parEntryEntity;

		// Token: 0x0400028D RID: 653
		private Dictionary<string, bool> _baseDataOrgCtl = new Dictionary<string, bool>();

		// Token: 0x0400028E RID: 654
		private Dictionary<string, long> _auxProps = new Dictionary<string, long>();

		// Token: 0x0400028F RID: 655
		private bool _isScanning;

		// Token: 0x04000290 RID: 656
		private long lastAuxpropId;
	}
}
