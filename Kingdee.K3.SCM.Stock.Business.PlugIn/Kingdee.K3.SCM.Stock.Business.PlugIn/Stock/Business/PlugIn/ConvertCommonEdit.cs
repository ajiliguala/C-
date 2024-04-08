using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200004C RID: 76
	public class ConvertCommonEdit : AbstractBillPlugIn
	{
		// Token: 0x06000347 RID: 839 RVA: 0x00028234 File Offset: 0x00026434
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			this.isAddAfter = false;
			string operation;
			if ((operation = e.Operation.FormOperation.Operation) != null)
			{
				if (!(operation == "AddChangeBeforeEntry"))
				{
					if (operation == "AddChangeAfterEntry" || operation == "InsertChangeAfterEntry")
					{
						int num;
						if (e.Operation.FormOperation.Operation == "InsertChangeAfterEntry")
						{
							num = base.View.Model.GetEntryCurrentRowIndex("FEntity") - 1;
							if (num < 0)
							{
								e.Cancel = true;
								base.View.ShowMessage(ResManager.LoadKDString("请不要在第一行插入转换后记录", "004023030000214", 5, new object[0]), 0);
								goto IL_2B0;
							}
						}
						else
						{
							num = base.View.Model.GetEntryRowCount("FEntity") - 1;
						}
						if (base.View.Model.GetEntryRowCount("FEntity") == 0)
						{
							e.Cancel = true;
							base.View.ShowMessage(ResManager.LoadKDString("请先增加转换前记录", "004023030000217", 5, new object[0]), 0);
						}
						else
						{
							Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
							DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
							DynamicObject entryLastestAfterChangeRow = this.GetEntryLastestAfterChangeRow(entityDataObject, num);
							if (entryLastestAfterChangeRow == null)
							{
								throw new Exception(ResManager.LoadKDString("错误添加转换后记录", "004023030000223", 5, new object[0]));
							}
							if (!(entryLastestAfterChangeRow["MaterialId"] is DynamicObject))
							{
								e.Cancel = true;
								base.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行的转换前记录尚未选择物料", "004023030000220", 5, new object[0]), entryLastestAfterChangeRow["Seq"].ToString()), 0);
							}
						}
						this.isAddAfter = true;
					}
				}
				else
				{
					DynamicObject entryLastestRow = this.GetEntryLastestRow();
					if (entryLastestRow != null && entryLastestRow["ConvertType"].ToString() == "A")
					{
						e.Cancel = true;
						DynamicObject dynamicObject = entryLastestRow["MaterialId"] as DynamicObject;
						if (dynamicObject == null)
						{
							base.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行不存在转换后记录", "004023030000208", 5, new object[0]), entryLastestRow["Seq"].ToString()), 0);
						}
						else
						{
							base.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}行物料{1}{2}不存在转换后记录", "004023030000211", 5, new object[0]), entryLastestRow["Seq"].ToString(), dynamicObject["Number"].ToString(), dynamicObject["Name"].ToString()), 0);
						}
					}
				}
			}
			IL_2B0:
			base.BeforeDoOperation(e);
		}

		// Token: 0x06000348 RID: 840 RVA: 0x000284F8 File Offset: 0x000266F8
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.newEntryRow = e.Row;
				if (!this.isAddAfter && base.Context.ServiceType != 1)
				{
					base.View.Model.SetValue("FConvertType", "A", e.Row);
				}
				this.SetDefOwnerAndKeeperValue(e.Row);
			}
			base.AfterCreateNewEntryRow(e);
		}

		// Token: 0x06000349 RID: 841 RVA: 0x00028574 File Offset: 0x00026774
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.EntityKey, "FEntity"))
			{
				string a = base.View.Model.GetValue("FConvertType", e.Row) as string;
				this.isDeleteChanged = (a == "A");
			}
			base.BeforeDeleteRow(e);
		}

		// Token: 0x0600034A RID: 842 RVA: 0x000285CC File Offset: 0x000267CC
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.EntityKey, "FEntity") && this.isDeleteChanged)
			{
				this.DeleteChangedRow(e.Row);
			}
			base.AfterDeleteRow(e);
		}

		// Token: 0x0600034B RID: 843 RVA: 0x000285FC File Offset: 0x000267FC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FLOT"))
			{
				string value = BillUtils.GetValue<string>(this.Model, "FConvertType", e.Row, null, null);
				if (!string.IsNullOrWhiteSpace(value) && value.Equals("A"))
				{
					string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
					{
						MaterialFieldKey = "FMaterialId",
						StockOrgFieldKey = "FStockOrgId",
						OwnerTypeFieldKey = "FOwnerTypeId",
						OwnerFieldKey = "FOwnerId",
						KeeperTypeFieldKey = "FKeeperTypeId",
						KeeperFieldKey = "FKeeperId",
						AuxpropFieldKey = "FAuxPropId",
						BomFieldKey = "FBOMId",
						StockFieldKey = "FStockId",
						StockLocFieldKey = "FStockLocId",
						StockStatusFieldKey = "FStockStatus",
						MtoFieldKey = "FMTONo",
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
			}
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00028740 File Offset: 0x00026940
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a = base.View.Model.GetValue("FConvertType", e.Row) as string;
			string key;
			switch (key = e.Field.Key)
			{
			case "FSTOCKORGID":
				this.SetDefOwnerAndKeeperValue(-1);
				break;
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FSTOCKERGROUPID", "WHY");
				break;
			case "FOwnerIdHead":
				this.SetDefLocalCurrencyAndExchangeType();
				break;
			case "FProduceDate":
			case "FExpiryDate":
			case "FUnitID":
			case "FLot":
				if (a == "A")
				{
					this.UpdateChangedValue(e.Field.Key, e.NewValue, e.Row);
				}
				break;
			case "FMaterialId":
				this.SetDefKeeperTypeAndKeeperValue(e.Row);
				if (a == "A")
				{
					long num2 = 0L;
					DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject != null)
					{
						num2 = Convert.ToInt64(dynamicObject["Id"]);
					}
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
					long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false);
					base.View.Model.SetValue("FBOMID", bomDefaultValueByMaterial, e.Row);
					if (e.OldValue == null)
					{
						this.UpdateChangedValue(e.Field.Key, e.NewValue, e.Row);
						this.UpdateChangedValue("FBOMID", bomDefaultValueByMaterial, e.Row);
					}
					else
					{
						this.DeleteChangedRow(e.Row + 1);
					}
				}
				break;
			case "FAuxPropId":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				this.SyncAuxPropData(e.Field, e.Row);
				break;
			}
			}
			base.DataChanged(e);
		}

		// Token: 0x0600034D RID: 845 RVA: 0x000289D4 File Offset: 0x00026BD4
		private void SyncAuxPropData(Field field, int row)
		{
			string a = base.View.Model.GetValue("FConvertType", row) as string;
			if (a != "A")
			{
				return;
			}
			if (!"FEntity".Equals(field.Entity.Key) || !(field is RelatedFlexGroupField) || field.Key.ToUpperInvariant().Equals("FSTOCKLOCID"))
			{
				return;
			}
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
			RelatedFlexGroupField relatedFlexGroupField = (RelatedFlexGroupField)field;
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entity, row);
			long valueId = Convert.ToInt64(relatedFlexGroupField.RefIDDynamicProperty.GetValue<long>(entityDataObject));
			int i = row + 1;
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			while (i < entryRowCount)
			{
				object value = base.View.Model.GetValue("FConvertType", i);
				if (value == null)
				{
					return;
				}
				if (!value.ToString().ToUpperInvariant().Equals("B"))
				{
					return;
				}
				DynamicObject value2 = (DynamicObject)base.View.Model.GetValue(field, row);
				this.SetFlexValue(relatedFlexGroupField, entity, value2, valueId, i);
				i++;
			}
		}

		// Token: 0x0600034E RID: 846 RVA: 0x00028B14 File Offset: 0x00026D14
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x0600034F RID: 847 RVA: 0x00028B78 File Offset: 0x00026D78
		public override void ShowFlexFormLoad(ShowFlexFormLoadEventArgs e)
		{
			base.ShowFlexFormLoad(e);
			if (StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				e.CancelFlexItemInit = true;
			}
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00028B9F File Offset: 0x00026D9F
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
				this.SyncAuxPropData(e.FlexField, e.Row);
			}
		}

		// Token: 0x06000351 RID: 849 RVA: 0x00028BE0 File Offset: 0x00026DE0
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string operation;
			if ((operation = e.Operation.Operation) != null)
			{
				if (!(operation == "AddChangeBeforeEntry"))
				{
					if (!(operation == "CopyEntryRow"))
					{
						if (!(operation == "DeleteEntry"))
						{
							if (operation == "AddChangeAfterEntry" || operation == "InsertChangeAfterEntry")
							{
								if (this.newEntryRow >= 0)
								{
									base.View.Model.SetValue("FConvertType", "B", this.newEntryRow);
									if (e.Operation.Operation == "InsertChangeAfterEntry")
									{
										this.RefreshAllRowControlService(this.newEntryRow);
									}
									this.SetFlexValueByConvertType(this.newEntryRow);
								}
								this.newEntryRow = -1;
							}
						}
						else
						{
							this.RefreshAllRowControlService(0);
						}
					}
					else
					{
						this.newEntryRow = -1;
					}
				}
				else
				{
					if (this.newEntryRow >= 0)
					{
						int num = this.newEntryRow;
						base.View.Model.SetValue("FConvertType", "A", this.newEntryRow);
						base.View.Model.CreateNewEntryRow("FEntity");
						base.View.Model.SetValue("FConvertType", "B", num + 1);
					}
					this.newEntryRow = -1;
				}
			}
			base.AfterDoOperation(e);
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00028D34 File Offset: 0x00026F34
		public override void LoadData(LoadDataEventArgs e)
		{
			this.SetSimplizationColumns();
			base.LoadData(e);
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00028D44 File Offset: 0x00026F44
		private void SetSimplizationColumns()
		{
			List<string> list = new List<string>();
			foreach (Field field in base.View.BusinessInfo.Entrys[1].Fields)
			{
				list.Add(field.Key);
			}
			list.Add("FSeq");
			EntryGrid control = base.View.GetControl<EntryGrid>("FEntity");
			control.SetSimplizationColumns(list);
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00028DDC File Offset: 0x00026FDC
		public override void AfterCreateNewData(EventArgs e)
		{
			this.SetSimplizationColumns();
			if (base.View.OpenParameter.Status == null && !this.isCopyBill && base.View.OpenParameter.CreateFrom != 1 && base.View.OpenParameter.CreateFrom != 2)
			{
				this.SetDefOwnerAndKeeperValue(-1);
			}
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00028E38 File Offset: 0x00027038
		public override void AfterBindData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1 && base.View.OpenParameter.CreateFrom != 2)
			{
				this.SetDefLocalCurrencyAndExchangeType();
				int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
				if (base.View.OpenParameter.Status == null && entryRowCount == 0)
				{
					base.View.InvokeFormOperation("AddChangeBeforeEntry");
					base.View.UpdateView("FEntity");
				}
			}
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00028ECF File Offset: 0x000270CF
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			this.isCopyBill = false;
			base.AfterCopyData(e);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00028EDF File Offset: 0x000270DF
		public override void CopyData(CopyDataEventArgs e)
		{
			this.isCopyBill = true;
			base.CopyData(e);
		}

		// Token: 0x06000358 RID: 856 RVA: 0x00028EF0 File Offset: 0x000270F0
		private void SetDefLocalCurrencyAndExchangeType()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrID", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00028F30 File Offset: 0x00027130
		private DynamicObject GetEntryLastestRow()
		{
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (entityDataObject.Count > 0)
			{
				return entityDataObject[entityDataObject.Count - 1];
			}
			return null;
		}

		// Token: 0x0600035A RID: 858 RVA: 0x00028F84 File Offset: 0x00027184
		private DynamicObject GetEntryLastestAfterChangeRow(DynamicObjectCollection entryObj, int select)
		{
			for (int i = select; i >= 0; i--)
			{
				if (entryObj[i]["ConvertType"].ToString() == "A")
				{
					return entryObj[i];
				}
			}
			return null;
		}

		// Token: 0x0600035B RID: 859 RVA: 0x00028FC8 File Offset: 0x000271C8
		private void RefreshAllRowControlService(int startRow)
		{
		}

		// Token: 0x0600035C RID: 860 RVA: 0x00028FCC File Offset: 0x000271CC
		private void DeleteChangedRow(int row)
		{
			while (row < base.View.Model.GetEntryRowCount("FEntity"))
			{
				string a = base.View.Model.GetValue("FConvertType", row) as string;
				if (a == "A")
				{
					return;
				}
				base.View.Model.DeleteEntryRow("FEntity", row);
			}
		}

		// Token: 0x0600035D RID: 861 RVA: 0x00029034 File Offset: 0x00027234
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

		// Token: 0x0600035E RID: 862 RVA: 0x000290B0 File Offset: 0x000272B0
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

		// Token: 0x0600035F RID: 863 RVA: 0x000291A4 File Offset: 0x000273A4
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

		// Token: 0x06000360 RID: 864 RVA: 0x00029290 File Offset: 0x00027490
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

		// Token: 0x06000361 RID: 865 RVA: 0x00029320 File Offset: 0x00027520
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
				object value = base.View.Model.GetValue("FOwnerTypeIdHead");
				string a = "";
				if (value != null)
				{
					a = Convert.ToString(value);
				}
				if (a == "BD_OwnerOrg")
				{
					base.View.Model.SetValue("FOwnerIdHead", num);
				}
			}
			for (int i = num2; i <= num3; i++)
			{
				object value2 = this.Model.GetValue("FOwnerTypeId", i);
				if (value2 != null)
				{
					string text = value2.ToString();
					this.Model.SetItemValueByNumber("FOwnerId", "", i);
					if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_OwnerOrg") && num > 0L)
					{
						this.Model.SetValue("FOwnerId", num.ToString(), i);
					}
				}
				value2 = this.Model.GetValue("FKeeperTypeId", i);
				if (value2 != null)
				{
					string text = value2.ToString();
					this.Model.SetItemValueByNumber("FKeeperId", "", i);
					if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_KeeperOrg") && num > 0L)
					{
						this.Model.SetValue("FKeeperId", num.ToString(), i);
					}
				}
			}
		}

		// Token: 0x06000362 RID: 866 RVA: 0x000294CC File Offset: 0x000276CC
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

		// Token: 0x06000363 RID: 867 RVA: 0x00029640 File Offset: 0x00027840
		private void SetFlexValueByConvertType(int irow)
		{
			object value = base.View.Model.GetValue("FConvertType", irow);
			if (value == null)
			{
				return;
			}
			if (value.ToString().ToUpperInvariant().Equals("B"))
			{
				int i = irow;
				Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
				while (i > -1)
				{
					i--;
					value = base.View.Model.GetValue("FConvertType", i);
					if (value == null)
					{
						return;
					}
					if (value.ToString().ToUpperInvariant().Equals("A"))
					{
						Field field = (from p in base.View.BusinessInfo.GetFieldList()
						where p.Key.ToUpperInvariant().Equals("FAUXPROPID")
						select p).FirstOrDefault<Field>();
						DynamicObject value2 = base.View.Model.GetValue(field.Key, i) as DynamicObject;
						DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, i);
						long valueId = Convert.ToInt64(((RelatedFlexGroupField)field).RefIDDynamicProperty.GetValue<long>(entityDataObject));
						this.SetFlexValue((RelatedFlexGroupField)field, entity, value2, valueId, irow);
						return;
					}
				}
			}
		}

		// Token: 0x06000364 RID: 868 RVA: 0x00029778 File Offset: 0x00027978
		private void SetFlexValue(RelatedFlexGroupField flexField, Entity entity, DynamicObject value, long valueId, int row)
		{
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, row);
			if (value == null)
			{
				this.Model.SetValue(flexField.Key, null, row);
				flexField.RefIDDynamicProperty.SetValue(entityDataObject, 0);
				return;
			}
			DynamicObject dynamicObject = (DynamicObject)ObjectUtils.CreateCopy(value);
			dynamicObject["Id"] = valueId;
			this.Model.SetValue(flexField.Key, dynamicObject, row);
			flexField.RefIDDynamicProperty.SetValue(entityDataObject, valueId);
			base.View.UpdateView(flexField.Key, row);
		}

		// Token: 0x04000122 RID: 290
		private bool isAddAfter;

		// Token: 0x04000123 RID: 291
		private long lastAuxpropId;

		// Token: 0x04000124 RID: 292
		private bool isCopyBill;

		// Token: 0x04000125 RID: 293
		private int newEntryRow = -1;

		// Token: 0x04000126 RID: 294
		private bool isDeleteChanged;
	}
}
