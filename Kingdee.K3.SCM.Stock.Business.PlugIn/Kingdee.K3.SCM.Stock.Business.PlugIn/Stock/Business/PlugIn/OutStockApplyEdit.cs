using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
	// Token: 0x02000014 RID: 20
	[Description("出库申请单 表单插件")]
	public class OutStockApplyEdit : AbstractBillPlugIn
	{
		// Token: 0x0600007F RID: 127 RVA: 0x000072C1 File Offset: 0x000054C1
		public override void BeforeBindData(EventArgs e)
		{
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000072C4 File Offset: 0x000054C4
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "2"))
			{
				this._defaultStock = Common.GetDefaultVMIStock(this, "FOWNERID", "FStockId", "0,6", false);
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x0000731C File Offset: 0x0000551C
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetBusinessTypeByBillType();
				Convert.ToString(base.View.Model.GetValue("FBizType"));
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
				bool flag = false;
				if (dynamicObject != null)
				{
					object obj = dynamicObject["OrgFunctions"];
					flag = obj.ToString().Split(new char[]
					{
						','
					}).Contains("103");
				}
				if (baseDataLongValue <= 0L || !flag)
				{
					return;
				}
				int num = base.View.Model.GetEntryRowCount("FEntity") - 1;
				for (int i = 0; i <= num; i++)
				{
					object value = this.Model.GetValue("FOwnerTypeId", i);
					if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
					{
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgIdEntry", i) as DynamicObject;
						if (dynamicObject2 == null || Convert.ToInt64(dynamicObject2["Id"]) == 0L)
						{
							base.View.Model.SetValue("FStockOrgIdEntry", baseDataLongValue, i);
						}
					}
				}
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00007482 File Offset: 0x00005682
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetDefOwnerValue(e.Row);
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x000074A8 File Offset: 0x000056A8
		private void SetDefOwnerValue(int row)
		{
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			base.View.Model.SetValue("FStockOrgIdEntry", num, row);
			DynamicObject dynamicObject2 = this.Model.GetValue("FStockOrgIdEntry", row) as DynamicObject;
			if (Convert.ToString(this.Model.GetValue("FOwnerTypeId", row)).Equals("BD_OwnerOrg") && dynamicObject2 != null)
			{
				this.Model.SetValue("FOwnerId", Convert.ToInt64(dynamicObject2["Id"]), row);
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00007564 File Offset: 0x00005764
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpper())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FOWNERID":
			case "FEXTAUXUNITID":
			case "FSTOCKORGIDENTRY":
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
			case "FLOT":
			{
				string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
				{
					MaterialFieldKey = "FMaterialId",
					StockOrgFieldKey = "FStockOrgId",
					OwnerTypeFieldKey = "FOwnerTypeId",
					OwnerFieldKey = "FOwnerId",
					AuxpropFieldKey = "FAuxPropId",
					BomFieldKey = "FBomId",
					StockFieldKey = "FStockId",
					StockLocFieldKey = "FStockLocId",
					StockStatusFieldKey = "FStockStatusId",
					MtoFieldKey = "FMtoNo"
				}, e.Row);
				if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
						return;
					}
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + lotF8InvFilter;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000077A4 File Offset: 0x000059A4
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpper())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FOWNERID":
			case "FEXTAUXUNITID":
			case "FSTOCKORGIDENTRY":
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

		// Token: 0x06000086 RID: 134 RVA: 0x000078EC File Offset: 0x00005AEC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FBizType"));
			string a;
			if ((a = e.Field.Key.ToUpper()) != null && !(a == "FSTOCKORGID"))
			{
				if (!(a == "FMATERIALID"))
				{
					if (!(a == "FSTOCKERID"))
					{
						if (!(a == "FAUXPROPID"))
						{
							return;
						}
						object oldValue = e.OldValue;
					}
				}
				else
				{
					this.SetDefKeeperTypeAndKeeperValue(e.Row);
					DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject != null)
					{
						Convert.ToInt64(dynamicObject["Id"]);
					}
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L && StringUtils.EqualsIgnoreCase(text, "2"))
					{
						DynamicObjectCollection dynamicObjectCollection = dynamicObject2["MaterialStock"] as DynamicObjectCollection;
						if (dynamicObjectCollection != null)
						{
							long num = Convert.ToInt64(dynamicObjectCollection[0]["StockId_Id"]);
							base.View.Model.SetValue("FStockId", num, e.Row);
						}
						DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockId", e.Row) as DynamicObject;
						if (dynamicObject3 == null || Convert.ToInt64(dynamicObject3["Id"]) == 0L || this._defaultStock != 0L)
						{
							base.View.Model.SetValue("FStockId", this._defaultStock, e.Row);
							return;
						}
						base.View.Model.SetValue("FStockLocID", dynamicObjectCollection[0]["StockPlaceId_Id"], e.Row);
						return;
					}
				}
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00007AE8 File Offset: 0x00005CE8
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this._lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00007B4C File Offset: 0x00005D4C
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1)
			{
				StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId");
			}
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00007B8C File Offset: 0x00005D8C
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
				if (value > 0L && !text.Equals("1") && !text.Equals("3") && Common.HaveBOMViewPermission(base.Context, value))
				{
					Common.SetBomExpandBillToConvertForm(base.Context, (List<ConvertBillElement>)e.Bills);
				}
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00007C8C File Offset: 0x00005E8C
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13 && e.SourceFormId == "ENG_PRODUCTSTRUCTURE")
			{
				List<string> list = new List<string>();
				SelBomBillParam bomExpandBillFieldValue = Common.GetBomExpandBillFieldValue(base.View, "FStockOrgId", "FOwnerTypeIdHead", "");
				if (Common.ValidateBomExpandBillFieldValue(base.View, bomExpandBillFieldValue, list))
				{
					base.View.Session["SelInStockBillParam"] = bomExpandBillFieldValue;
					Common.SetBomExpandConvertRuleinfo(base.Context, base.View, e);
					return;
				}
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("【{0}】 字段为选单必录项！", "004023030004312", 5, new object[0]), string.Join(ResManager.LoadKDString("】,【", "004023030004315", 5, new object[0]), list)), "", 0);
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00007D64 File Offset: 0x00005F64
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				Common.DoBomExpandDraw(base.View, Common.GetBomExpandBillFieldValue(base.View, "FStockOrgId", "", ""));
				base.View.UpdateView("FEntity");
				base.View.Model.DataChanged = true;
			}
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00007E04 File Offset: 0x00006004
		private bool GetBomFilter(int row, out string filter)
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
			filter = ((!ListUtils.IsEmpty<long>(approvedBomIdByOrgId)) ? string.Format(" FID IN ({0}) ", string.Join<long>(",", approvedBomIdByOrgId)) : string.Format(" FID={0}", 0));
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00007EF8 File Offset: 0x000060F8
		private void AuxpropDataChanged(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num == this._lastAuxpropId)
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
			this._lastAuxpropId = num;
			base.View.UpdateView("FEntity", row);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00007FE4 File Offset: 0x000061E4
		private void AuxpropDataChanged(DynamicObject newAuxpropData, int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMId", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject, newAuxpropData, false, value2, false);
			if (bomDefaultValueByMaterial != value)
			{
				base.View.Model.SetValue("FBOMId", bomDefaultValueByMaterial, row);
			}
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00008074 File Offset: 0x00006274
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
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00008174 File Offset: 0x00006374
		private void SetDefKeeperTypeAndKeeperValue(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				Convert.ToInt64(dynamicObject["Id"]);
			}
		}

		// Token: 0x06000091 RID: 145 RVA: 0x000081B0 File Offset: 0x000063B0
		private void SetKeeperTypeAndKeeper(string newOwerValue)
		{
		}

		// Token: 0x06000092 RID: 146 RVA: 0x000081B4 File Offset: 0x000063B4
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
			case "FSTOCKORGIDENTRY":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			case "FOWNERID":
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FBizType")), "2") && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId", row)), "BD_Supplier"))
				{
					filter = Common.getVMIOwnerFilter();
				}
				break;
			case "FSTOCKID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockStatusId", row) as DynamicObject;
				string arg = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Number"]);
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
				if (StringUtils.EqualsIgnoreCase(text, "1"))
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
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00008534 File Offset: 0x00006734
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

		// Token: 0x06000094 RID: 148 RVA: 0x00008688 File Offset: 0x00006888
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

		// Token: 0x06000095 RID: 149 RVA: 0x000087D4 File Offset: 0x000069D4
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_OSABillTypeParaSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BizType"]);
			}
		}

		// Token: 0x0400002E RID: 46
		private const string BizTypeMtr = "0";

		// Token: 0x0400002F RID: 47
		private const string BizTypeVmi = "2";

		// Token: 0x04000030 RID: 48
		private const string BizTypeAsset = "1";

		// Token: 0x04000031 RID: 49
		private const string BizTypeFee = "3";

		// Token: 0x04000032 RID: 50
		private long _lastAuxpropId;

		// Token: 0x04000033 RID: 51
		private long _defaultStock;
	}
}
