using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x0200002D RID: 45
	[Description("简单生产退库单-表单插件")]
	public class SpOutStockEdit : AbstractBillPlugIn
	{
		// Token: 0x060001BD RID: 445 RVA: 0x00015E98 File Offset: 0x00014098
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			if (!e.Context.IsMultiOrg && StockServiceHelper.GetUpdateStockDate(e.Context, e.Context.CurrentOrganizationInfo.ID) == null)
			{
				e.CancelMessage = ResManager.LoadKDString("请先在【启用库存管理】中设置库存启用日期,再进行库存业务处理.", "004023030002269", 5, new object[0]);
				e.Cancel = true;
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00015EF4 File Offset: 0x000140F4
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00015EFD File Offset: 0x000140FD
		public override void AfterBindData(EventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
			{
				SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
			}
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00015F38 File Offset: 0x00014138
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FOwnerId0"))
				{
					if (key == "FMaterialId")
					{
						long num = 0L;
						DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
						if (dynamicObject != null)
						{
							num = Convert.ToInt64(dynamicObject["Id"]);
						}
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
						base.View.Model.SetValue("FBomId", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num, false), e.Row);
						return;
					}
					if (key == "FStockerId")
					{
						Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
						return;
					}
					if (!(key == "FAuxpropId"))
					{
						return;
					}
					DynamicObject newAuxpropData = e.OldValue as DynamicObject;
					this.AuxpropDataChanged(newAuxpropData, e.Row);
				}
				else if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
				{
					SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
					return;
				}
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00016090 File Offset: 0x00014290
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				if (baseDataLongValue > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
				}
			}
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x000160E8 File Offset: 0x000142E8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string text = e.ListFilterParameter.Filter;
			bool f7AndSetNumberEvent = this.GetF7AndSetNumberEvent(e.FieldKey, e.Row, out text);
			if (f7AndSetNumberEvent)
			{
				e.Cancel = true;
			}
			else if (!string.IsNullOrWhiteSpace(text))
			{
				e.ListFilterParameter.Filter = Common.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
			if (e.FieldKey.Equals("FLot"))
			{
				text = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
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
					MtoFieldKey = "FMtoNo"
				}, e.Row);
				if (!string.IsNullOrWhiteSpace(text))
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
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00016240 File Offset: 0x00014440
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string filter = e.Filter;
			this.GetF7AndSetNumberEvent(e.BaseDataFieldKey, e.Row, out filter);
			if (!string.IsNullOrWhiteSpace(filter))
			{
				e.Filter = Common.SqlAppendAnd(e.Filter, filter);
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0001628C File Offset: 0x0001448C
		private bool GetF7AndSetNumberEvent(string fieldKey, int eRow, out string filter)
		{
			bool flag = false;
			filter = null;
			switch (fieldKey)
			{
			case "FMaterialId":
				filter = " FIsProduce = '1' ";
				break;
			case "FPrdOrgId":
			{
				long value = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null);
				if (value <= 0L)
				{
					flag = true;
					base.View.ShowMessage(ResManager.LoadKDString("请先录入退库组织", "004023000021834", 5, new object[0]), 0);
				}
				break;
			}
			case "FStockStatusId":
				flag = Common.GetStockStatusFilterStr(this, eRow, "FStockId", out filter);
				break;
			case "FStockerId":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				long num2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				if (num2 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num2.ToString();
				}
				break;
			}
			case "FStockerGroupId":
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockerId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject2["Id"]));
				}
				break;
			}
			case "FExtAuxUnitId":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", eRow);
				break;
			case "FStockOrgId":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			case "FOwnerId0":
				filter = SCMCommon.GetOwnerFilterByRelation(this, new GetOwnerFilterArgs("FOwnerTypeId0", eRow, "FPrdOrgId", eRow, "109", "2"));
				break;
			}
			if (flag)
			{
				filter = " 1 = 0 ";
			}
			return flag;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x000164E8 File Offset: 0x000146E8
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0001654C File Offset: 0x0001474C
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00016584 File Offset: 0x00014784
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

		// Token: 0x060001C8 RID: 456 RVA: 0x00016678 File Offset: 0x00014878
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

		// Token: 0x060001C9 RID: 457 RVA: 0x00016764 File Offset: 0x00014964
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

		// Token: 0x040000A8 RID: 168
		private long lastAuxpropId;
	}
}
