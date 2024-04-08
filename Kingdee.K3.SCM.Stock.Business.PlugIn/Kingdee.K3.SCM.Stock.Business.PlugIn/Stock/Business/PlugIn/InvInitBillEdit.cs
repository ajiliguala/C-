using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000099 RID: 153
	public class InvInitBillEdit : AbstractBillPlugIn
	{
		// Token: 0x0600088D RID: 2189 RVA: 0x0006F303 File Offset: 0x0006D503
		public override void AfterCreateNewData(EventArgs e)
		{
			this.SyncOwnerAndKeeper();
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x0006F30B File Offset: 0x0006D50B
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FInvInitDetail"))
			{
				this.SetDefOwnerTypeAndOwnerValue(e.Row, "NewRow");
			}
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x0006F338 File Offset: 0x0006D538
		public override void AfterLoadData(EventArgs e)
		{
			object value = this.Model.GetValue("FStockOrgID");
			if (value is DynamicObject)
			{
				this.stockOrgID = Convert.ToInt64(((DynamicObject)value)["Id"]);
			}
			this.GetCurrencyAndStockInDate();
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x0006F380 File Offset: 0x0006D580
		private void SyncOwnerAndKeeper()
		{
			object value = this.Model.GetValue("FStockOrgID");
			if (value is DynamicObject)
			{
				this.stockOrgID = Convert.ToInt64(((DynamicObject)value)["Id"]);
			}
			this.GetCurrencyAndStockInDate();
			value = this.Model.GetValue("FOwnerTypeIdHead");
			if (value != null)
			{
				string text = value.ToString();
				if (!string.IsNullOrWhiteSpace(text) && text.Equals("BD_OwnerOrg", StringComparison.OrdinalIgnoreCase))
				{
					if (this.stockOrgID == 0L)
					{
						this.Model.SetItemValueByNumber("FOwnerIDHead", "", 0);
					}
					else
					{
						this.Model.SetValue("FOwnerIDHead", this.stockOrgID.ToString(), 0);
					}
				}
			}
			value = this.Model.GetValue("FKeeperTypeID");
			if (value != null)
			{
				string text = value.ToString();
				if (!string.IsNullOrWhiteSpace(text) && text.Equals("BD_KeeperOrg", StringComparison.OrdinalIgnoreCase))
				{
					if (this.stockOrgID == 0L)
					{
						this.Model.SetItemValueByNumber("FKeeperID", "", 0);
						return;
					}
					this.Model.SetValue("FKeeperID", this.stockOrgID.ToString(), 0);
				}
			}
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x0006F4A4 File Offset: 0x0006D6A4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FUNITID":
			case "FEXTAUXUNITID":
			{
				string text;
				if (this.GetFieldFilter(e.FieldKey, e.Row, out text))
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
			case "FOWNERIDHEAD":
			case "FOWNERID":
			{
				string text;
				if (this.GetOwnerFieldFilter(e.FieldKey, out text, e.Row))
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
			case "FSTOCKID":
			{
				string text;
				if (this.GetStockFieldFilter(e.FieldKey, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
						listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + text;
					}
				}
				break;
			}
			case "FSTOCKORGID":
			{
				string text;
				if (this.GetFieldFilter(e.FieldKey, e.Row, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter4 = e.ListFilterParameter;
						listFilterParameter4.Filter = listFilterParameter4.Filter + " AND " + text;
					}
				}
				break;
			}
			case "FPDSTOCKSTATUS":
			{
				string text;
				if (this.GetStockStatusFieldFilter(e.FieldKey, out text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
					}
					else
					{
						IRegularFilterParameter listFilterParameter5 = e.ListFilterParameter;
						listFilterParameter5.Filter = listFilterParameter5.Filter + " AND " + text;
					}
				}
				break;
			}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x0006F734 File Offset: 0x0006D934
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FUNITID":
			case "FEXTAUXUNITID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, e.Row, out text))
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
			case "FOWNERIDHEAD":
			case "FOWNERID":
			{
				string text;
				if (this.GetOwnerFieldFilter(e.BaseDataFieldKey, out text, e.Row))
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
			case "FSTOCKID":
			{
				string text;
				if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text))
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
			case "FSTOCKORGID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, e.Row, out text))
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
			case "FPDSTOCKSTATUS":
			{
				string text;
				if (this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text))
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

		// Token: 0x06000893 RID: 2195 RVA: 0x0006F978 File Offset: 0x0006DB78
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x0006F9DC File Offset: 0x0006DBDC
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x0006FA14 File Offset: 0x0006DC14
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSTOCKORGID":
				this.stockOrgID = Convert.ToInt64(e.NewValue);
				this.SyncOwnerAndKeeper();
				return;
			case "FMATERIALID":
				if (Convert.ToInt64(e.NewValue) > 0L)
				{
					this.Model.SetItemValueByID("FCurrencyID", this.currencyID, e.Row);
					if (this.stockInDate == DateTime.MinValue)
					{
						this.Model.SetValue("FStockInDate", "", e.Row);
					}
					else
					{
						this.Model.SetValue("FStockInDate", this.stockInDate, e.Row);
					}
					DynamicObject dynamicObject = this.Model.GetValue("FStockId") as DynamicObject;
					if (dynamicObject != null)
					{
						this.Model.SetValue("FStockStatusID", dynamicObject["DefStockStatusId"], e.Row);
					}
					else
					{
						this.Model.SetValue("FStockStatusID", null, e.Row);
					}
					long num2 = 0L;
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject2 != null)
					{
						num2 = Convert.ToInt64(dynamicObject2["Id"]);
					}
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
					base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject3, 0L, false, num2, false), e.Row);
					this.SetDefOwnerTypeAndOwnerValue(e.Row, "DataChange");
					return;
				}
				return;
			case "FOWNERIDHEAD":
			{
				string keeperTypeAndKeeperByOwner = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeperByOwner(keeperTypeAndKeeperByOwner);
				this.SetCurrencyByOwner();
				return;
			}
			case "FOWNERTYPEIDHEAD":
				Common.SynOwnerType(this, "FOwnerTypeIdHead", "FOwnerTypeId");
				return;
			case "FSTOCKINDATE":
				this.CheckStartDate(e.Row);
				return;
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				return;
			}
			}
			base.DataChanged(e);
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x0006FCB4 File Offset: 0x0006DEB4
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

		// Token: 0x06000897 RID: 2199 RVA: 0x0006FDA8 File Offset: 0x0006DFA8
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

		// Token: 0x06000898 RID: 2200 RVA: 0x0006FE94 File Offset: 0x0006E094
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

		// Token: 0x06000899 RID: 2201 RVA: 0x0006FF24 File Offset: 0x0006E124
		private bool GetFieldFilter(string fieldKey, int row, out string filter)
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
					if (!(a == "FSTOCKORGID"))
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
							filter = " FIsInventory = '1'";
						}
					}
					else
					{
						filter = string.Format("NOT EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = t0.FORGID AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1')", new object[0]);
					}
				}
				else
				{
					object value = this.Model.GetValue("FKeeperTypeID", row);
					if (value != null)
					{
						string text = value.ToString();
						if (text.Equals("BD_Supplier", StringComparison.OrdinalIgnoreCase))
						{
							DynamicObject dynamicObject = this.Model.GetValue("FKeeperID") as DynamicObject;
							if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0L)
							{
								filter = string.Format(" FSTOCKPROPERTY = 3 AND FSupplierId = {0}", dynamicObject["Id"]);
							}
							else
							{
								filter = " FSTOCKPROPERTY = 3 ";
							}
						}
					}
				}
			}
			return true;
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x00070050 File Offset: 0x0006E250
		private void SetKeeperTypeAndKeeperByOwner(string newOwerValue)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead"));
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgID") as DynamicObject;
			long value = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			string text2 = Convert.ToString(value);
			if (newOwerValue == "")
			{
				base.View.GetFieldEditor("FKeeperTypeID", 0).Enabled = false;
			}
			else if (!StringUtils.EqualsIgnoreCase(newOwerValue, text2) && StringUtils.EqualsIgnoreCase(text, "BD_OwnerOrg"))
			{
				base.View.GetFieldEditor("FKeeperTypeID", 0).Enabled = false;
			}
			else if (!StringUtils.EqualsIgnoreCase(text, "BD_OwnerOrg"))
			{
				base.View.GetFieldEditor("FKeeperTypeID", 0).Enabled = false;
			}
			else
			{
				base.View.GetFieldEditor("FKeeperTypeID", 0).Enabled = true;
			}
			int entryRowCount = base.View.Model.GetEntryRowCount("FInvInitDetail");
			if (newOwerValue == text2 && text == "BD_OwnerOrg")
			{
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					base.View.Model.SetValue("FOwnerTypeId", text, i);
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, i);
					}
				}
				return;
			}
			for (int j = 0; j < entryRowCount; j++)
			{
				base.View.Model.SetValue("FOwnerTypeId", text, j);
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", j) as DynamicObject;
				if (text == "BD_OwnerOrg")
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
			}
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x00070330 File Offset: 0x0006E530
		private void SetDefOwnerTypeAndOwnerValue(int row, string sType)
		{
			object value = base.View.Model.GetValue("FOWNERTYPEIDHEAD");
			if (value != null)
			{
				Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FOWNERIDHEAD") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject2 != null)
			{
				Convert.ToInt64(dynamicObject2["Id"]);
			}
			base.View.Model.SetValue("FOWNERTYPEID", value, row);
			if (!StringUtils.EqualsIgnoreCase(sType, "NewRow"))
			{
				base.View.Model.SetValue("FOwnerId", num, row);
			}
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x00070404 File Offset: 0x0006E604
		private bool GetOwnerFieldFilter(string fieldKey, out string filter, int index)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && (a == "FOWNERIDHEAD" || a == "FOWNERID"))
			{
				string a2 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeIdHead"));
				string a3 = Convert.ToString(base.View.Model.GetValue("FOwnerTypeId", index));
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgID") as DynamicObject;
				long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				if ((fieldKey.ToUpperInvariant().Equals("FOWNERIDHEAD") && a2 == "BD_OwnerOrg") || (fieldKey.ToUpperInvariant().Equals("FOWNERID") && a3 == "BD_OwnerOrg"))
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
					filter = string.Format(" NOT EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP WHERE BSP.FCATEGORY = 'HS' AND BSP.FORGID = t0.FORGID AND BSP.FKEY = 'IsBizOrgEndInitial' AND BSP.FVALUE = '1') ", new object[0]);
					if (dynamicObjectCollection.Count > 0)
					{
						filter += string.Format(" AND FORGID in (SELECT {0} UNION (select t0.FORGID from T_ORG_BIZRELATIONENTRY t0\r\n                                                    left join T_ORG_BIZRELATION t1 on t0.FBIZRELATIONID=t1.FBIZRELATIONID\r\n                                                    where t1.FBRTYPEID=112 and t0.FRELATIONORGID={1}))", num, num);
					}
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x000705A4 File Offset: 0x0006E7A4
		private bool GetStockFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null && a == "FSTOCKID")
			{
				string arg = string.Empty;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockStatusID", 0) as DynamicObject;
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
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x000706AC File Offset: 0x0006E8AC
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockID") as DynamicObject;
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

		// Token: 0x0600089F RID: 2207 RVA: 0x000707CC File Offset: 0x0006E9CC
		private void CheckStartDate(int row)
		{
			long stockOrgId = 0L;
			DateTime t = KDTimeZone.MinSystemDateTime;
			object value = base.View.Model.GetValue("FStockInDate", row);
			if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
			{
				t = Convert.ToDateTime(value.ToString());
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId", row) as DynamicObject;
			if (dynamicObject != null && dynamicObject != null)
			{
				stockOrgId = Convert.ToInt64(dynamicObject["Id"]);
			}
			List<long> list = new List<long>();
			if (!list.Contains(stockOrgId))
			{
				list.Add(stockOrgId);
			}
			DateTime dateTime = KDTimeZone.MaxSystemDateTime;
			IEnumerable<SystemProfileRecord> source = CommonServiceHelper.GeSTKInitCloseInfoByOrgID(base.Context, list);
			SystemProfileRecord systemProfileRecord = source.SingleOrDefault((SystemProfileRecord p) => p.FOrgId == stockOrgId);
			if (systemProfileRecord != null && !string.IsNullOrWhiteSpace(systemProfileRecord.FValue))
			{
				dateTime = Convert.ToDateTime(systemProfileRecord.FValue).AddDays(-1.0);
			}
			if (t > dateTime)
			{
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("分录{0}入库日期必须小于库存组织的启用日期{1}", "004023030000964", 5, new object[0]), row + 1, dateTime), ResManager.LoadKDString("日期校验不通过", "004023030000967", 5, new object[0]), 0);
			}
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x00070934 File Offset: 0x0006EB34
		private void GetCurrencyAndStockInDate()
		{
			this.GetCurrencyId();
			this.stockInDate = DateTime.MinValue;
			if (this.stockOrgID > 0L)
			{
				object updateStockDate = StockServiceHelper.GetUpdateStockDate(base.Context, this.stockOrgID);
				if (updateStockDate != null)
				{
					this.stockInDate = Convert.ToDateTime(updateStockDate).AddDays(-1.0);
				}
			}
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x00070990 File Offset: 0x0006EB90
		private void GetCurrencyId()
		{
			this.currencyID = 0L;
			object value = this.Model.GetValue("FOwnerTypeIdHead");
			if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
			{
				string text = value.ToString();
				if (text.Equals("BD_OwnerOrg", StringComparison.OrdinalIgnoreCase))
				{
					DynamicObject dynamicObject = this.Model.GetValue("FOwnerIDHead") as DynamicObject;
					if (dynamicObject != null)
					{
						this.currencyID = CommonServiceHelper.GetDefaultCurrencyByBizOrgID(base.Context, Convert.ToInt64(dynamicObject["Id"]));
					}
				}
			}
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x00070A18 File Offset: 0x0006EC18
		private void SetCurrencyByOwner()
		{
			this.GetCurrencyId();
			this.Model.SetItemValueByID("FHeadCurrencyId", this.currencyID, 0);
			int entryRowCount = this.Model.GetEntryRowCount("FInvInitDetail");
			for (int i = 0; i < entryRowCount; i++)
			{
				object value = this.Model.GetValue("FMaterialID", i);
				if (value != null)
				{
					this.Model.SetItemValueByID("FCurrencyID", this.currencyID, i);
				}
			}
		}

		// Token: 0x0400035C RID: 860
		private long currencyID;

		// Token: 0x0400035D RID: 861
		private long stockOrgID;

		// Token: 0x0400035E RID: 862
		private DateTime stockInDate = DateTime.MinValue;

		// Token: 0x0400035F RID: 863
		private bool hasAsked;

		// Token: 0x04000360 RID: 864
		private long lastAuxpropId;
	}
}
