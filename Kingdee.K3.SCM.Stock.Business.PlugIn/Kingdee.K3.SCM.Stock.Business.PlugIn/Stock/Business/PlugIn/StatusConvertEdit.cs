using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000093 RID: 147
	[Description("形态转换单单据插件")]
	public class StatusConvertEdit : AbstractBillPlugIn
	{
		// Token: 0x060007D1 RID: 2001 RVA: 0x00064964 File Offset: 0x00062B64
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._onlySumBFields = this.GetOnlySumbFields();
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x00064979 File Offset: 0x00062B79
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.newEntryRow = e.Row;
			}
			base.AfterCreateNewEntryRow(e);
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x000649A8 File Offset: 0x00062BA8
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

		// Token: 0x060007D4 RID: 2004 RVA: 0x00064A00 File Offset: 0x00062C00
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a = base.View.Model.GetValue("FConvertType", e.Row) as string;
			string key;
			switch (key = e.Field.Key)
			{
			case "FStockStatus":
			case "FStockId":
			case "FKeeperId":
				if (a == "A")
				{
					this.UpdateChangedValue(e.Field.Key, e.NewValue, e.Row);
				}
				break;
			case "FMaterialId":
				if (a == "A")
				{
					long num2 = 0L;
					DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject != null)
					{
						num2 = Convert.ToInt64(dynamicObject["Id"]);
					}
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
					base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
				}
				break;
			case "FKeeperTypeId":
				if (a == "A")
				{
					this.UpdateChangedValue(e.Field.Key, e.NewValue, e.Row);
				}
				break;
			case "FOwnerIdHead":
			{
				string newOwerValue = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeper(newOwerValue, e.Row);
				break;
			}
			case "FConvertType":
				if (a == "B")
				{
					DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate"));
					base.View.Model.SetValue("FBusinessDate", dateTime, e.Row);
				}
				else
				{
					base.View.Model.SetValue("FBusinessDate", "", e.Row);
				}
				break;
			}
			base.DataChanged(e);
		}

		// Token: 0x060007D5 RID: 2005 RVA: 0x00064C78 File Offset: 0x00062E78
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string operation;
			if ((operation = e.Operation.Operation) != null && (operation == "AddChangeAfterEntry" || operation == "InsertChangeAfterEntry"))
			{
				if (this.newEntryRow >= 0)
				{
					int entryLastestAfterChangeRow = this.GetEntryLastestAfterChangeRow(this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FMaterialId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FUnitId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FStockStatus", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FLot", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FProduceDate", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FExpiryDate", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FMTONo", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FProjectNo", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FAuxPropId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FBOMId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FKeeperTypeId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FKeeperId", entryLastestAfterChangeRow, this.newEntryRow);
				}
				this.newEntryRow = -1;
			}
			base.AfterDoOperation(e);
		}

		// Token: 0x060007D6 RID: 2006 RVA: 0x00064DB4 File Offset: 0x00062FB4
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpper())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
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
			case "FSTOCKSTATUS":
			{
				string text;
				if (this.GetStockStatusFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + text;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00064F0C File Offset: 0x0006310C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpper())
			{
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
			case "FSTOCKSTATUS":
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

		// Token: 0x060007D8 RID: 2008 RVA: 0x00065058 File Offset: 0x00063258
		public override void OnEntrySum(EntrySumEventArgs e)
		{
			base.OnEntrySum(e);
			if (this._onlySumBFields != null && this._onlySumBFields.Contains(e.Field.Key) && e.SumType == 1)
			{
				List<DynamicObject> list = (from p in e.DetailData
				where "B".Equals(p["ConvertType"])
				select p).ToList<DynamicObject>();
				e.Value = Common.GetResultByGroupSumType(base.View.BillBusinessInfo, list, e.Field, e.SumType);
			}
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x000650E8 File Offset: 0x000632E8
		public virtual List<string> GetOnlySumbFields()
		{
			return new List<string>
			{
				"FConvertQty",
				"FBaseQty",
				"FSNQty",
				"FSecQty",
				"FExtAuxUnitQty",
				"FInvQty",
				"FAmount"
			};
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x0006514C File Offset: 0x0006334C
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

		// Token: 0x060007DB RID: 2011 RVA: 0x00065240 File Offset: 0x00063440
		private void UpdateChangedValue(string key, object value, int row)
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = row + 1; i < entryRowCount; i++)
			{
				string a = base.View.Model.GetValue("FConvertType", i) as string;
				if (a == "A")
				{
					return;
				}
				base.View.Model.SetValue(key, value, i);
				base.View.InvokeFieldUpdateService(key, i);
			}
		}

		// Token: 0x060007DC RID: 2012 RVA: 0x000652BC File Offset: 0x000634BC
		private void GetLastestRowValueSetToNewRow(string key, int lastestRow, int newRow)
		{
			bool flag = this.SyncDataWhenCreateNewRow(key, "B", lastestRow, newRow);
			if (flag)
			{
				object value = base.View.Model.GetValue(key, lastestRow);
				if ("FAuxPropId".Equals(key))
				{
					if (value != null)
					{
						((DynamicObjectCollection)this.Model.DataObject["StatusConvertEntry"])[newRow]["AuxPropId_Id"] = Convert.ToInt64(((DynamicObject)value)["Id"]);
					}
					else
					{
						((DynamicObjectCollection)this.Model.DataObject["StatusConvertEntry"])[newRow]["AuxPropId_Id"] = 0;
					}
				}
				base.View.Model.SetValue(key, value, newRow);
				base.View.InvokeFieldUpdateService(key, newRow);
			}
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00065398 File Offset: 0x00063598
		private bool CheckChangedHaveDuplicateValue(string key, string value, int row)
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = row + 1; i < entryRowCount; i++)
			{
				string a = base.View.Model.GetValue("FConvertType", i) as string;
				if (a == "A")
				{
					break;
				}
				string text = Convert.ToString(base.View.Model.GetValue(key, i));
				if (!string.IsNullOrEmpty(text) && value == text)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x00065420 File Offset: 0x00063620
		private int GetEntryLastestAfterChangeRow(int selectRow)
		{
			for (int i = selectRow; i >= 0; i--)
			{
				string a = base.View.Model.GetValue("FConvertType", i) as string;
				if (a == "A")
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x00065468 File Offset: 0x00063668
		private void SetDefKeeperTypeAndKeeperValue(int row)
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			if (value != null)
			{
				Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			int num3 = row;
			int num4 = row;
			if (row < 0)
			{
				num3 = 0;
				num4 = this.Model.GetEntryRowCount("FEntity") - 1;
			}
			for (int i = num3; i <= num4; i++)
			{
				base.View.Model.SetValue("FOwnerTypeId", value, i);
				base.View.Model.SetValue("FOwnerId", num, i);
				string text = base.View.Model.GetValue("FKeeperTypeId", i) as string;
				if (!string.IsNullOrWhiteSpace(text) && text.Equals("BD_KeeperOrg"))
				{
					base.View.Model.SetValue("FKeeperId", num2, i);
				}
			}
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x000655C4 File Offset: 0x000637C4
		private void SetKeeperTypeAndKeeper(string newOwerValue, int row)
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
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			if (newOwerValue == text && a == "BD_OwnerOrg")
			{
				for (int i = 0; i < entryRowCount; i++)
				{
					base.View.Model.SetValue("FOwnerTypeId", value, i);
					base.View.Model.SetValue("FOwnerId", newOwerValue, i);
				}
				return;
			}
			for (int j = 0; j < entryRowCount; j++)
			{
				base.View.Model.SetValue("FOwnerTypeId", value, j);
				base.View.Model.SetValue("FOwnerId", newOwerValue, j);
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", j);
				base.View.Model.SetValue("FKeeperId", text, j);
			}
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x0006571C File Offset: 0x0006391C
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
					if (base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null)
					{
						return false;
					}
					string arg = string.Empty;
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockStatus", row) as DynamicObject;
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

		// Token: 0x060007E2 RID: 2018 RVA: 0x0006599C File Offset: 0x00063B9C
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			if (base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null)
			{
				return false;
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

		// Token: 0x060007E3 RID: 2019 RVA: 0x00065AD3 File Offset: 0x00063CD3
		public virtual bool SyncDataWhenCreateNewRow(string fieldKey, string convertType, int indexA, int indexB)
		{
			return true;
		}

		// Token: 0x040002DA RID: 730
		private long lastAuxpropId;

		// Token: 0x040002DB RID: 731
		private int newEntryRow = -1;

		// Token: 0x040002DC RID: 732
		private List<string> _onlySumBFields;
	}
}
