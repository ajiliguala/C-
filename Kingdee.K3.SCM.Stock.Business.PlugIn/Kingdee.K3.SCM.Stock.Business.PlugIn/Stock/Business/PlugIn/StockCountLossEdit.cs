using System;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200005A RID: 90
	public class StockCountLossEdit : AbstractBillPlugIn
	{
		// Token: 0x060003F7 RID: 1015 RVA: 0x0002FD1B File Offset: 0x0002DF1B
		public override void AfterCreateModelData(EventArgs e)
		{
			base.AfterCreateModelData(e);
			if (base.View.OpenParameter.Status == null)
			{
				this.SetBusinessTypeByBillType();
			}
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0002FD3C File Offset: 0x0002DF3C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI"))
			{
				this.defaultStock = Common.GetDefaultVMIStock(this, "FOwnerIdHead", "FStockId", "", false);
			}
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x0002FD94 File Offset: 0x0002DF94
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				string lotF8InvFilter;
				if (!(a == "FOWNERIDHEAD") && !(a == "FOWNERID") && !(a == "FMATERIALID") && !(a == "FEXTAUXUNITID"))
				{
					if (!(a == "FLOT"))
					{
						return;
					}
					lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
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
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + lotF8InvFilter;
					}
				}
				else if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
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
			}
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x0002FF30 File Offset: 0x0002E130
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpper()) != null)
			{
				if (!(a == "FOWNERIDHEAD") && !(a == "FOWNERID") && !(a == "FMATERIALID") && !(a == "FEXTAUXUNITID"))
				{
					return;
				}
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
			}
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x0002FFCC File Offset: 0x0002E1CC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpper()) != null)
			{
				if (!(a == "FMATERIALID"))
				{
					if (!(a == "FOWNERIDHEAD"))
					{
						return;
					}
					if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI"))
					{
						this.defaultStock = Common.GetDefaultVMIStock(this, "FOWNERIDHEAD", "FStockId", "", true);
					}
				}
				else
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
					string text = Convert.ToString(base.View.Model.GetValue("FBusinessType"));
					if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
					{
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockId", e.Row) as DynamicObject;
						if (dynamicObject2 == null || Convert.ToInt64(dynamicObject2["Id"]) == 0L || (StringUtils.EqualsIgnoreCase(text, "VMI") && this.defaultStock != 0L))
						{
							base.View.Model.SetValue("FStockId", this.defaultStock, e.Row);
							return;
						}
					}
				}
			}
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00030120 File Offset: 0x0002E320
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_CountLossParam", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBusinessType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00030170 File Offset: 0x0002E370
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
				if (!(a == "FOWNERIDHEAD"))
				{
					if (!(a == "FOWNERID"))
					{
						if (!(a == "FMATERIALID"))
						{
							if (a == "FEXTAUXUNITID")
							{
								filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
							}
						}
						else
						{
							string text = base.View.Model.GetValue("FBusinessType") as string;
							if (StringUtils.EqualsIgnoreCase(text, "VMI"))
							{
								filter = " FIsVmiBusiness = '1' ";
							}
						}
					}
					else if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId", row)), "BD_Supplier"))
					{
						filter = Common.getVMIOwnerFilter();
					}
				}
				else if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBusinessType")), "VMI") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead")), "BD_Supplier"))
				{
					filter = Common.getVMIOwnerFilter();
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x04000171 RID: 369
		private const string businessType_VMI = "VMI";

		// Token: 0x04000172 RID: 370
		private long defaultStock;
	}
}
