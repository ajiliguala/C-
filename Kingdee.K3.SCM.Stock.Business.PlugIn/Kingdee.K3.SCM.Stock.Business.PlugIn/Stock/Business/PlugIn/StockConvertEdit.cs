using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A1 RID: 161
	public class StockConvertEdit : AbstractBillPlugIn
	{
		// Token: 0x06000991 RID: 2449 RVA: 0x00080C91 File Offset: 0x0007EE91
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._onlySumBFields = this.GetOnlySumbFields();
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00080CA6 File Offset: 0x0007EEA6
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.newEntryRow = e.Row;
			}
			base.AfterCreateNewEntryRow(e);
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00080CD4 File Offset: 0x0007EED4
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
			this.SetBizTypeByBillType();
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x00080D30 File Offset: 0x0007EF30
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (base.Context.ServiceType != 1)
			{
				string text = Convert.ToString(base.View.Model.GetValue("FBizType"));
				string a = base.View.Model.GetValue("FConvertType", e.Row) as string;
				string key;
				if ((key = e.Key) != null && key == "FStockStatus" && !text.Equals("1"))
				{
					bool flag = true;
					long num;
					if (e.Value is DynamicObject)
					{
						DynamicObject dynamicObject = e.Value as DynamicObject;
						num = Convert.ToInt64(dynamicObject["Id"]);
					}
					else
					{
						num = Convert.ToInt64(e.Value);
					}
					if (a == "A" && !string.IsNullOrEmpty(Convert.ToString(e.Value)))
					{
						flag = this.CheckChangedHaveDuplicateValue("FStockStatus", num, e.Row);
						if (!flag && !base.View.Session.ContainsKey("ReturnSerialNumber"))
						{
							base.View.ShowMessage(ResManager.LoadKDString("库存状态与转换后记录库存状态相同", "004023030000376", 5, new object[0]), 0);
						}
					}
					else if (!string.IsNullOrEmpty(Convert.ToString(e.Value)))
					{
						int entryLastestAfterChangeRow = this.GetEntryLastestAfterChangeRow(e.Row);
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockStatus", entryLastestAfterChangeRow) as DynamicObject;
						if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) == num)
						{
							flag = false;
							if (!base.View.Session.ContainsKey("ReturnSerialNumber"))
							{
								base.View.ShowMessage(ResManager.LoadKDString("库存状态与转换前记录库存状态相同", "004023030000379", 5, new object[0]), 0);
							}
						}
					}
					if (!flag)
					{
						e.Cancel = true;
						base.View.UpdateView("FStockStatus", e.Row);
					}
				}
			}
			base.BeforeUpdateValue(e);
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00080F2C File Offset: 0x0007F12C
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
				object value = base.View.Model.GetValue("FConvertReason");
				if (value != null && value.ToString() == "2")
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("库存请检单请检冻结的库存状态转换单不允许反审核。", "004023000019777", 5, new object[0]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x0008107C File Offset: 0x0007F27C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string text = Convert.ToString(base.View.Model.GetValue("FBizType"));
			string a = base.View.Model.GetValue("FConvertType", e.Row) as string;
			string key;
			switch (key = e.Field.Key)
			{
			case "FStockStatus":
				if (!text.Equals("1"))
				{
					bool flag = true;
					long num2;
					if (e.NewValue is DynamicObject)
					{
						DynamicObject dynamicObject = e.NewValue as DynamicObject;
						num2 = Convert.ToInt64(dynamicObject["Id"]);
					}
					else
					{
						num2 = Convert.ToInt64(e.NewValue);
					}
					if (a == "A" && !string.IsNullOrEmpty(Convert.ToString(e.NewValue)))
					{
						flag = this.CheckChangedHaveDuplicateValue("FStockStatus", num2, e.Row);
						if (!flag && !base.View.Session.ContainsKey("ReturnSerialNumber"))
						{
							base.View.ShowMessage(ResManager.LoadKDString("库存状态与转换后记录库存状态相同", "004023030000376", 5, new object[0]), 0);
						}
					}
					else if (!string.IsNullOrEmpty(Convert.ToString(e.NewValue)))
					{
						int entryLastestAfterChangeRow = this.GetEntryLastestAfterChangeRow(e.Row);
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockStatus", entryLastestAfterChangeRow) as DynamicObject;
						if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) == num2 && !base.View.Session.ContainsKey("ReturnSerialNumber"))
						{
							flag = false;
							base.View.ShowMessage(ResManager.LoadKDString("库存状态与转换前记录库存状态相同", "004023030000379", 5, new object[0]), 0);
						}
					}
					if (!flag)
					{
						this.Model.BeginIniti();
						base.View.Model.SetValue("FStockStatus", e.OldValue, e.Row);
						this.Model.EndIniti();
						base.View.UpdateView("FStockStatus", e.Row);
					}
				}
				break;
			case "FBOMId":
			case "FKeeperId":
				if (a == "A")
				{
					this.UpdateChangedValue(e.Field.Key, e.NewValue, e.Row);
				}
				break;
			case "FMTONo":
				if (a == "A")
				{
					this.SynDataFromAToB(e.Field.Key, e.NewValue, e.Row);
				}
				break;
			case "FMaterialId":
				if (a == "A")
				{
					long num3 = 0L;
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject3 != null)
					{
						num3 = Convert.ToInt64(dynamicObject3["Id"]);
					}
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
					base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject4, 0L, false, num3, false), e.Row);
				}
				if (base.Context.ServiceType == 1)
				{
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
					if (dynamicObject5 != null)
					{
						DynamicObjectCollection source = (DynamicObjectCollection)dynamicObject5["MaterialInvPty"];
						if (source.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["InvPtyId_Id"]) == 10003L && Convert.ToBoolean(p["IsEnable"])) == null)
						{
							base.View.Model.SetValue("FBOMID", null, e.Row);
						}
					}
				}
				break;
			case "FOwnerIdHead":
			{
				string newOwerValue = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeper(newOwerValue, e.Row);
				break;
			}
			case "FKeeperTypeId":
				if (a == "A")
				{
					this.SynDataFromAToB(e.Field.Key, e.NewValue, e.Row);
				}
				break;
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

		// Token: 0x06000997 RID: 2455 RVA: 0x000815A0 File Offset: 0x0007F7A0
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

		// Token: 0x06000998 RID: 2456 RVA: 0x00081694 File Offset: 0x0007F894
		private bool IsFlexField(string key)
		{
			return key.IndexOf("__") > -1;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x000816A4 File Offset: 0x0007F8A4
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
					this.GetLastestRowValueSetToNewRow("FBOMId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FAuxUnitId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FLot", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FProduceDate", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FExpiryDate", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FMTONo", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FProjectNo", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FKeeperTypeId", entryLastestAfterChangeRow, this.newEntryRow);
					this.GetLastestRowValueSetToNewRow("FKeeperId", entryLastestAfterChangeRow, this.newEntryRow);
				}
				this.newEntryRow = -1;
			}
			base.AfterDoOperation(e);
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x000817D0 File Offset: 0x0007F9D0
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
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
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					}
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

		// Token: 0x0600099B RID: 2459 RVA: 0x00081934 File Offset: 0x0007FB34
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
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
					}
					else
					{
						e.Filter = e.Filter + " AND " + text;
					}
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

		// Token: 0x0600099C RID: 2460 RVA: 0x00081A90 File Offset: 0x0007FC90
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

		// Token: 0x0600099D RID: 2461 RVA: 0x00081B20 File Offset: 0x0007FD20
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

		// Token: 0x0600099E RID: 2462 RVA: 0x00081B84 File Offset: 0x0007FD84
		private void SetBizTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_StockConBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BizType"]);
			}
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x00081BD4 File Offset: 0x0007FDD4
		private void GetLastestRowValueSetToNewRow(string key, int lastestRow, int newRow)
		{
			bool flag = this.SyncDataWhenCreateNewRow(key, "B", lastestRow, newRow);
			if (flag)
			{
				object value = base.View.Model.GetValue(key, lastestRow);
				base.View.Model.SetValue(key, value, newRow);
				base.View.InvokeFieldUpdateService(key, newRow);
			}
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x00081C28 File Offset: 0x0007FE28
		private bool CheckChangedHaveDuplicateValue(string key, long value, int row)
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = row + 1; i < entryRowCount; i++)
			{
				string a = base.View.Model.GetValue("FConvertType", i) as string;
				if (a == "A")
				{
					break;
				}
				DynamicObject dynamicObject = base.View.Model.GetValue(key, i) as DynamicObject;
				if (dynamicObject != null && value == Convert.ToInt64(dynamicObject["Id"]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x00081CB4 File Offset: 0x0007FEB4
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

		// Token: 0x060009A2 RID: 2466 RVA: 0x00081D28 File Offset: 0x0007FF28
		private void UpdateChangedValue(string key, object value, int row)
		{
			Field field = base.View.Model.BusinessInfo.GetField(key);
			if (field != null)
			{
				Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
				base.View.Model.GetEntityDataObject(entity);
				DynamicObject dynamicObject = base.View.Model.GetValue(key, row) as DynamicObject;
				if (dynamicObject == null)
				{
					return;
				}
				long num = Convert.ToInt64(dynamicObject["Id"]);
				int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
				int i = row + 1;
				while (i < entryRowCount)
				{
					string a = base.View.Model.GetValue("FConvertType", i) as string;
					if (a == "A")
					{
						return;
					}
					if (!(key == "FBOMId") || base.Context.ServiceType != 1)
					{
						goto IL_142;
					}
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					if (dynamicObject2 != null)
					{
						DynamicObjectCollection source = (DynamicObjectCollection)dynamicObject2["MaterialInvPty"];
						DynamicObject dynamicObject3 = source.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["InvPtyId_Id"]) == 10003L && Convert.ToBoolean(p["IsEnable"]));
						if (dynamicObject3 != null)
						{
							goto IL_142;
						}
					}
					IL_17D:
					i++;
					continue;
					IL_142:
					base.View.Model.SetValue(key, num, i);
					base.View.Model.SetValue(key, dynamicObject, i);
					base.View.UpdateView(key, i);
					goto IL_17D;
				}
			}
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x00081EC4 File Offset: 0x000800C4
		private void SynDataFromAToB(string key, object value, int row)
		{
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
			base.View.Model.GetEntityDataObject(entity);
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = row + 1; i < entryRowCount; i++)
			{
				string a = base.View.Model.GetValue("FConvertType", i) as string;
				if (a == "A")
				{
					return;
				}
				if (key == "FMTONo" && base.Context.ServiceType == 1)
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					if (dynamicObject != null)
					{
						DynamicObject dynamicObject2 = ((DynamicObjectCollection)dynamicObject["MaterialPlan"])[0];
						if (dynamicObject2 != null && Convert.ToString(dynamicObject2["PlanMode"]) != "1" && Convert.ToString(dynamicObject2["PlanMode"]) != "2")
						{
							return;
						}
					}
				}
				base.View.Model.SetValue(key, value, i);
			}
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00081FFC File Offset: 0x000801FC
		private Field GetEntityField(string key)
		{
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
			foreach (Field field in entity.Fields)
			{
				if (field.Key == key)
				{
					return field;
				}
			}
			return null;
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x00082078 File Offset: 0x00080278
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

		// Token: 0x060009A6 RID: 2470 RVA: 0x000821D4 File Offset: 0x000803D4
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

		// Token: 0x060009A7 RID: 2471 RVA: 0x0008232C File Offset: 0x0008052C
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

		// Token: 0x060009A8 RID: 2472 RVA: 0x0008257C File Offset: 0x0008077C
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

		// Token: 0x060009A9 RID: 2473 RVA: 0x00082684 File Offset: 0x00080884
		private void SetDefOwnerIDValue()
		{
			object value = base.View.Model.GetValue("FOwnerTypeIdHead");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			Convert.ToString(num);
			if (a == "BD_OwnerOrg")
			{
				base.View.Model.SetValue("FOwnerIdHead", num);
			}
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x0008271C File Offset: 0x0008091C
		public virtual bool SyncDataWhenCreateNewRow(string fieldKey, string convertType, int indexA, int indexB)
		{
			return true;
		}

		// Token: 0x040003E0 RID: 992
		private int newEntryRow = -1;

		// Token: 0x040003E1 RID: 993
		private string _formId = "";

		// Token: 0x040003E2 RID: 994
		private string _changeType = "";

		// Token: 0x040003E3 RID: 995
		private string _flexFieldKey = "";

		// Token: 0x040003E4 RID: 996
		private int _row;

		// Token: 0x040003E5 RID: 997
		private long lastAuxpropId;

		// Token: 0x040003E6 RID: 998
		private List<string> _onlySumBFields;
	}
}
