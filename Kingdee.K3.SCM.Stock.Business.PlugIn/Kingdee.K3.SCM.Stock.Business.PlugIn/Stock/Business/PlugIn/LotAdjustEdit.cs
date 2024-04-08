using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Contracts;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200006F RID: 111
	[Description("批号调整单单据插件")]
	public class LotAdjustEdit : AbstractBillPlugIn
	{
		// Token: 0x060004E2 RID: 1250 RVA: 0x0003AEA0 File Offset: 0x000390A0
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			base.View.GetControl<EntryGrid>("FSTK_LOTADJUSTENTRY").SimplizationAllColumns();
			this._onlySumBFields = this.GetOnlySumbFields();
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0003AEC4 File Offset: 0x000390C4
		public override void AfterBindData(EventArgs e)
		{
			this._allowAdjustStockStatus = this.GetBillTypeAdjustStockStatus();
			this._allowAdjustAuxProp = this.GetBillTypeAdjustAuxProp();
			this._cancelLotValidate = this.CancelLotValidateSettingChecked();
			if (base.View.OpenParameter.Status == 2)
			{
				this.LockFieldInSameGroup();
			}
			else if (base.View.OpenParameter.Status == null && !this._allowAdjustStockStatus)
			{
				this.LockStockStatusFieldInSameGroup();
			}
			else if (base.View.OpenParameter.Status == null && !this._allowAdjustAuxProp)
			{
				this.LockAuxPropFieldInSameGroup();
			}
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && parameterData.DynamicObjectType.Properties.ContainsKey("InvQueryReRuleGroup") && parameterData["InvQueryReRuleGroup"] != null && !string.IsNullOrWhiteSpace(parameterData["InvQueryReRuleGroup"].ToString()))
			{
				this._invQueryReRuleForLotAdjust = Convert.ToString(parameterData["InvQueryReRuleGroup"]);
			}
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0003AFB2 File Offset: 0x000391B2
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			this.isCopyBill = false;
			base.AfterCopyData(e);
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x0003AFC2 File Offset: 0x000391C2
		public override void CopyData(CopyDataEventArgs e)
		{
			this.isCopyBill = true;
			base.CopyData(e);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0003AFD4 File Offset: 0x000391D4
		public override void AfterCreateNewData(EventArgs e)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null && !this.isCopyBill)
			{
				base.View.Model.SetValue("FOwnerIdHead", dynamicObject["Id"]);
			}
			this.SetDefCurrency();
			if (!this.isCopyBill && base.View.OpenParameter.Status == null)
			{
				base.View.Model.SetValue("FGroup", 1, 0);
			}
			if (!this.isCopyBill)
			{
				this.SetDefKeeperTypeAndKeeperValue(0);
			}
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0003B074 File Offset: 0x00039274
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

		// Token: 0x060004E8 RID: 1256 RVA: 0x0003B0C9 File Offset: 0x000392C9
		public override void BeforeCreateNewEntryRow(BeforeCreateNewEntryEventArgs e)
		{
			base.BeforeCreateNewEntryRow(e);
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x0003B0F0 File Offset: 0x000392F0
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string operation;
			if ((operation = e.Operation.FormOperation.Operation) != null)
			{
				if (!(operation == "NewGroupEntry"))
				{
					if (!(operation == "NewAfterConvertEntry"))
					{
						if (!(operation == "InsertBeforeConvertEntry"))
						{
							if (operation == "QueryStock")
							{
								int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FSTK_LOTADJUSTENTRY");
								int num = Convert.ToInt32(base.View.Model.GetValue("FGroup", entryCurrentRowIndex));
								DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(base.View.BillBusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY"));
								if (entityDataObject != null && entityDataObject.Count<DynamicObject>() > 0 && num > 0)
								{
									List<DynamicObject> list = (from p in entityDataObject
									where Convert.ToString(p["ConvertType"]).Equals("A")
									select p).ToList<DynamicObject>();
									if (list != null && list.Count<DynamicObject>() > 0)
									{
										foreach (DynamicObject dynamicObject in list)
										{
											int num2 = Convert.ToInt32(dynamicObject["Seq"]) - 1;
											if (!base.View.GetFieldEditor("FMATERIALID", num2).Enabled)
											{
												this._queryStockLockRows.Add(num2);
											}
											else
											{
												this._queryStockUnLockRows.Add(num2);
											}
										}
									}
								}
							}
						}
						else
						{
							this.m_EntryBarItemClickMenuId = "tbInsertBeforeConvert";
						}
					}
					else
					{
						this.m_EntryBarItemClickMenuId = "tbNewAfterConvert";
					}
				}
				else
				{
					this.m_EntryBarItemClickMenuId = "tbAddGroup";
				}
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0003B2BC File Offset: 0x000394BC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string operation;
			if ((operation = e.Operation.Operation) != null)
			{
				if (!(operation == "InsertBeforeConvertEntry"))
				{
					return;
				}
				this.m_EntryBarItemClickMenuId = "";
			}
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0003B324 File Offset: 0x00039524
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			string eventName;
			if ((eventName = e.EventName) != null)
			{
				if (!(eventName == "QueryStockFinished"))
				{
					return;
				}
				object obj = null;
				if (base.View.Session.ContainsKey("QueryStockFinished"))
				{
					obj = base.View.Session["QueryStockFinished"];
				}
				if (obj != null && obj.ToString().Equals("1"))
				{
					if (this._queryStockInsertRows.Count<int>() > 0)
					{
						if (this._queryStockLockRows.Count<int>() > 0)
						{
							using (List<int>.Enumerator enumerator = this._queryStockLockRows.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									int row = enumerator.Current;
									this.LockField(row + (from p in this._queryStockInsertRows
									where p <= row
									select p).ToList<int>().Count<int>());
								}
							}
						}
						if (this._queryStockUnLockRows.Count<int>() > 0)
						{
							using (List<int>.Enumerator enumerator2 = this._queryStockUnLockRows.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									int row = enumerator2.Current;
									this.UnLockField(row + (from p in this._queryStockInsertRows
									where p <= row
									select p).ToList<int>().Count<int>());
								}
							}
						}
						foreach (int row2 in this._queryStockInsertRows)
						{
							this.LockField(row2);
						}
						this._queryStockInsertRows.Clear();
					}
					this._queryStockLockRows.Clear();
					this._queryStockUnLockRows.Clear();
				}
			}
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x0003B534 File Offset: 0x00039734
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			this.SetDefKeeperTypeAndKeeperValue(e.Row);
			string entryBarItemClickMenuId;
			switch (entryBarItemClickMenuId = this.m_EntryBarItemClickMenuId)
			{
			case "tbNewBeforeConvert":
			case "tbInsertBeforeConvert":
			case "tbSplitInsertBeforeConvert":
				this.SetDefValueAfterAddEntry("A", e.Row);
				break;
			case "tbNewAfterConvert":
			case "tbInsertAfterConvert":
				this.SetDefValueAfterAddEntry("B", e.Row);
				this.SetDefSupplierLotValue(e.Row);
				break;
			case "tbCopyRow":
				if (!this.finishCopy)
				{
					this.finishCopy = true;
					if (this.oldIndex > -1)
					{
						this.LockField(e.Row);
					}
					else
					{
						this.SetDefValueAfterAddEntry(base.View.Model.GetValue("FConvertType", e.Row).ToString(), e.Row);
					}
				}
				break;
			case "tbAddGroup":
			case "tbSplitAddGroup":
			{
				int num2 = 1;
				if (e.Row >= 1)
				{
					num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", e.Row - 1)) + 1;
				}
				base.View.Model.SetValue("FGroup", num2, e.Row);
				break;
			}
			}
			if (this.m_EntryBarItemClickMenuId.Equals("tbInsertBeforeConvert") && base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id))
			{
				this._queryStockInsertRows.Add(e.Row);
			}
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0003B778 File Offset: 0x00039978
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			this.m_EntryBarItemClickMenuId = e.BarItemKey;
			string key;
			switch (key = e.BarItemKey.ToUpperInvariant())
			{
			case "TBDELETEGROUP":
			{
				int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FSTK_LOTADJUSTENTRY");
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", entryCurrentRowIndex));
				if (num2 <= 0)
				{
					return;
				}
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(base.View.BillBusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY"));
				for (int i = entityDataObject.Count - 1; i >= 0; i--)
				{
					DynamicObject dynamicObject = entityDataObject[i];
					if (num2 == Convert.ToInt32(dynamicObject["FGroup"]))
					{
						base.View.Model.DeleteEntryRow("FSTK_LOTADJUSTENTRY", i);
					}
				}
				return;
			}
			case "TBCOPYROW":
			case "TBNEWBEFORECONVERT":
			case "TBNEWAFTERCONVERT":
			{
				bool flag = false;
				this.oldIndex = -1;
				int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FSTK_LOTADJUSTENTRY");
				if (e.BarItemKey.ToUpperInvariant().Equals("TBCOPYROW"))
				{
					flag = true;
					this.finishCopy = false;
					this.copyIndex = entryCurrentRowIndex;
				}
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", entryCurrentRowIndex));
				if (e.BarItemKey.ToUpperInvariant().Equals("TBNEWBEFORECONVERT") || e.BarItemKey.ToUpperInvariant().Equals("TBNEWAFTERCONVERT"))
				{
					this._beforeSetConvertType = true;
				}
				if (num2 <= 0)
				{
					return;
				}
				int num3 = 0;
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(base.View.BillBusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY"));
				for (int j = entryCurrentRowIndex; j < entityDataObject.Count; j++)
				{
					DynamicObject dynamicObject = entityDataObject[j];
					if (num2 != Convert.ToInt32(dynamicObject["FGroup"]))
					{
						num3 = j;
						break;
					}
				}
				if (num3 > 0)
				{
					this.oldIndex = entryCurrentRowIndex;
					if (flag)
					{
						base.View.Model.CopyEntryRow("FSTK_LOTADJUSTENTRY", entryCurrentRowIndex, num3, false);
					}
					else
					{
						this.Model.InsertEntryRow("FSTK_LOTADJUSTENTRY", num3);
					}
					e.Cancel = true;
					return;
				}
				break;
			}
			case "TBINSERTBEFORECONVERT":
			case "TBINSERTAFTERCONVERT":
				this._beforeSetConvertType = true;
				break;

				return;
			}
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x0003BA38 File Offset: 0x00039C38
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FLot"))
				{
					return;
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject != null && dynamicObject2 != null)
				{
					object obj = e.Value;
					if (e.Value is DynamicObject)
					{
						obj = ((DynamicObject)e.Value)["Number"];
					}
					if (obj != null && !string.IsNullOrEmpty(Convert.ToString(obj)))
					{
						List<SelectorItemInfo> list = new List<SelectorItemInfo>();
						list.Add(new SelectorItemInfo("FSupplyLot"));
						QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
						{
							FormId = "BD_BatchMainFile",
							FilterClauseWihtKey = string.Format(" FNUMBER = N'{1}' AND FBIZTYPE='1' AND FMATERIALID = {0} AND FUSEORGID = {2} AND FLOTSTATUS <> '2' ", Convert.ToInt64(dynamicObject2["msterId"]), Convert.ToString(obj), Convert.ToInt64(dynamicObject["Id"])),
							SelectItems = list
						};
						DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
						if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
						{
							base.View.Model.SetValue("FSupplierLot", dynamicObjectCollection.FirstOrDefault<DynamicObject>()["FSupplyLot"], e.Row);
							return;
						}
						if (Convert.ToString(base.View.Model.GetValue("FConvertType", e.Row)) == "A")
						{
							base.View.Model.SetValue("FSupplierLot", null, e.Row);
							return;
						}
					}
					else if (Convert.ToString(base.View.Model.GetValue("FConvertType", e.Row)) == "A")
					{
						base.View.Model.SetValue("FSupplierLot", null, e.Row);
					}
				}
			}
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0003BC94 File Offset: 0x00039E94
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key)
			{
			case "FStockerId":
				Common.SetGroupValue(this, "FStockerId", "FSTOCKGROUPID", "WHY");
				return;
			case "FMATERIALID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				if (this.m_DataChanged)
				{
					this.SetDefKeeperTypeAndKeeperValue(e.Row);
					if (base.Context.ServiceType != 1)
					{
						this.SetDyValueInSameGroup(e.Field.Key, e.NewValue, e.Row);
					}
					long num2 = 0L;
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject2 != null)
					{
						num2 = Convert.ToInt64(dynamicObject2["Id"]);
					}
					base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject, 0L, false, num2, false), e.Row);
				}
				if (dynamicObject != null)
				{
					bool flag = Convert.ToBoolean(((DynamicObjectCollection)dynamicObject["MaterialStock"])[0]["IsBatchManage"]);
					base.View.GetFieldEditor("FLot", e.Row).Enabled = (this._cancelLotValidate || flag);
				}
				Entity entity = base.View.BillBusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY");
				DynamicObject dynamicObject3 = base.View.Model.GetEntityDataObject(entity)[e.Row];
				RelatedFlexGroupField relatedFlexGroupField = base.View.BusinessInfo.GetField("FAUXPROPID") as RelatedFlexGroupField;
				RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = (RelatedFlexGroupFieldAppearance)base.View.LayoutInfo.GetFieldAppearance("FAUXPROPID");
				string relatedBaseDataFlexGroupField = relatedFlexGroupField.RelatedBaseDataFlexGroupField;
				BaseDataField baseDataField = (BaseDataField)base.View.BusinessInfo.GetField(relatedBaseDataFlexGroupField);
				DynamicObject dynamicObject4 = baseDataField.DynamicProperty.GetValue(dynamicObject3) as DynamicObject;
				if (dynamicObject4 == null)
				{
					return;
				}
				List<string> flexEnableList = baseDataField.Controller.GetFlexEnableList(dynamicObject4);
				string a = Convert.ToString(dynamicObject3["ConvertType"]);
				if (!this._allowAdjustAuxProp && a == "B")
				{
					base.View.GetFieldEditor("FAUXPROPID", e.Row).Enabled = false;
					return;
				}
				this.SetFlexFixedColEnable(dynamicObject3, relatedFlexGroupField, flexEnableList);
				return;
			}
			case "FOwnerTypeIdHead":
				if (e.NewValue != null && e.NewValue.ToString() == "BD_OwnerOrg")
				{
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					if (dynamicObject5 != null)
					{
						base.View.Model.SetValue("FOwnerIdHead", dynamicObject5["Id"]);
						return;
					}
				}
				break;
			case "FOwnerIdHead":
			{
				string newOwerValue = Convert.ToString(e.NewValue);
				this.SetKeeperTypeAndKeeper(newOwerValue, e.Row);
				return;
			}
			case "FGroup":
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
				if (entryRowCount == e.Row + 1)
				{
					base.View.GetFieldEditor("FMATERIALID", e.Row).Enabled = true;
					base.View.GetFieldEditor("FUnitID", e.Row).Enabled = true;
					base.View.GetFieldEditor("FBOMID", e.Row).Enabled = true;
					base.View.GetFieldEditor("FStockStatusId", e.Row).Enabled = true;
					base.View.GetFieldEditor("FMtono", e.Row).Enabled = true;
					return;
				}
				break;
			}
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				this.SyncAuxPropData(e.Field, e.Row);
				return;
			}
			case "FStockStatusId":
			case "FSTOCKID":
			{
				string a2 = base.View.Model.GetValue("FConvertType", e.Row) as string;
				if (!this._allowAdjustStockStatus && !this._beforeSetConvertType && a2 == "A" && this.m_DataChanged && base.Context.ServiceType != 1)
				{
					this.SetDyValueInSameGroup(e.Field.Key, e.NewValue, e.Row);
					return;
				}
				break;
			}
			case "FUnitID":
			case "FBOMID":
			case "FKeeperId":
			{
				string a2 = base.View.Model.GetValue("FConvertType", e.Row) as string;
				if (!this._beforeSetConvertType && a2 == "A" && this.m_DataChanged && base.Context.ServiceType != 1)
				{
					this.SetDyValueInSameGroup(e.Field.Key, e.NewValue, e.Row);
					return;
				}
				break;
			}
			case "FMtono":
			case "FProjectNo":
			case "FKeeperTypeId":
			{
				string a2 = base.View.Model.GetValue("FConvertType", e.Row) as string;
				string value = (e.NewValue == null) ? "" : e.NewValue.ToString();
				if (!this._beforeSetConvertType && a2 == "A" && this.m_DataChanged)
				{
					this.SetStrValueInSameGroup(e.Field.Key, value, e.Row);
					return;
				}
				break;
			}
			case "FConvertType":
				if (Convert.ToString(base.View.Model.GetValue("FConvertType", e.Row)) == "B")
				{
					DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate"));
					base.View.Model.SetValue("FBusinessDate", dateTime, e.Row);
					return;
				}
				base.View.Model.SetValue("FBusinessDate", "", e.Row);
				return;
			case "FSupplierLot":
			{
				string text = Convert.ToString(base.View.Model.GetValue("FSupplierLot", e.Row));
				if (!string.IsNullOrEmpty(text) && Convert.ToString(base.View.Model.GetValue("FConvertType", e.Row)) == "A")
				{
					int groupid = Convert.ToInt32(base.View.Model.GetValue("FGroup", e.Row));
					Entity entity2 = base.View.Model.BusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY");
					List<DynamicObject> list = (from p in this.Model.GetEntityDataObject(entity2)
					where Convert.ToInt32(p["seq"]) > e.Row + 1 && Convert.ToInt32(p["FGroup"]) == groupid
					select p).ToList<DynamicObject>();
					if (list != null && list.Count<DynamicObject>() > 0)
					{
						foreach (DynamicObject dynamicObject6 in list)
						{
							Convert.ToString(base.View.Model.GetValue("FConvertType", Convert.ToInt32(dynamicObject6["seq"]) - 1));
							if (Convert.ToString(base.View.Model.GetValue("FConvertType", Convert.ToInt32(dynamicObject6["seq"]) - 1)).Equals("A"))
							{
								break;
							}
							if (string.IsNullOrEmpty(Convert.ToString(base.View.Model.GetValue("FSupplierLot", Convert.ToInt32(dynamicObject6["seq"]) - 1))))
							{
								base.View.Model.SetValue("FSupplierLot", text, Convert.ToInt32(dynamicObject6["seq"]) - 1);
							}
						}
					}
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0003C69C File Offset: 0x0003A89C
		private void SyncAuxPropData(Field field, int row)
		{
			if (this._allowAdjustAuxProp)
			{
				return;
			}
			string a = base.View.Model.GetValue("FConvertType", row) as string;
			if (a != "A")
			{
				return;
			}
			if (this.m_EntryBarItemClickMenuId == "tbInsertBeforeConvert")
			{
				return;
			}
			RelatedFlexGroupField relatedFlexGroupField = (RelatedFlexGroupField)field;
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY");
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entity, row);
			long valueId = Convert.ToInt64(relatedFlexGroupField.RefIDDynamicProperty.GetValue<long>(entityDataObject));
			int num = Convert.ToInt32(base.View.Model.GetValue("FGroup", row));
			int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
			int i = row + 1;
			DynamicObject value = (DynamicObject)base.View.Model.GetValue(field, row);
			while (i < entryRowCount)
			{
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", i));
				if (num != num2)
				{
					return;
				}
				if (base.View.Model.GetValue("FConvertType", i) == null)
				{
					return;
				}
				this.SetFlexValue(relatedFlexGroupField, entity, value, valueId, i);
				i++;
			}
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0003C7E8 File Offset: 0x0003A9E8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FOWNERIDHEAD":
			{
				object value = base.View.Model.GetValue("FOwnerTypeIdHead");
				string lotF8InvFilter;
				if (value != null && value.ToString() == "BD_OwnerOrg" && this.GetFieldFilter(e.FieldKey.ToUpperInvariant(), out lotF8InvFilter, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrEmpty(e.ListFilterParameter.Filter) ? lotF8InvFilter : (e.ListFilterParameter.Filter + "AND" + lotF8InvFilter));
					return;
				}
				break;
			}
			case "FLOT":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) < 0L)
				{
					e.Cancel = true;
					base.View.ShowNotificationMessage(ResManager.LoadKDString("请先输入物料!", "004023030002182", 5, new object[0]), "", 0);
					return;
				}
				string value2 = BillUtils.GetValue<string>(this.Model, "FConvertType", e.Row, null, null);
				if (!string.IsNullOrWhiteSpace(value2) && value2.Equals("A"))
				{
					string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
					{
						MaterialFieldKey = "FMATERIALID",
						StockOrgFieldKey = "FStockOrgId",
						OwnerTypeFieldKey = "FOwnerTypeId",
						OwnerFieldKey = "FOwnerId",
						KeeperTypeFieldKey = "FKeeperTypeId",
						KeeperFieldKey = "FKeeperId",
						AuxpropFieldKey = "FAUXPROPID",
						BomFieldKey = "FBOMID",
						StockFieldKey = "FSTOCKID",
						StockLocFieldKey = "FStockLocId",
						StockStatusFieldKey = "FStockStatusId",
						MtoFieldKey = "FMtono",
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
						return;
					}
				}
				break;
			}
			case "FBOMID":
			case "FPFBOMID":
			{
				string lotF8InvFilter;
				if (this.GetFieldFilter(e.FieldKey.ToUpperInvariant(), out lotF8InvFilter, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrEmpty(e.ListFilterParameter.Filter) ? lotF8InvFilter : (e.ListFilterParameter.Filter + "AND" + lotF8InvFilter));
					return;
				}
				e.Cancel = true;
				base.View.ShowNotificationMessage(ResManager.LoadKDString("请先输入物料!", "004023030002182", 5, new object[0]), "", 0);
				return;
			}
			case "FMATERIALID":
			case "FSTOCKID":
			case "FEXTAUXUNITID":
			case "FSTOCKGROUPID":
			case "FSTOCKERID":
			{
				string lotF8InvFilter;
				if (this.GetFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
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
				if (this.GetFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrEmpty(e.ListFilterParameter.Filter) ? lotF8InvFilter : (e.ListFilterParameter.Filter + " AND " + lotF8InvFilter));
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x0003CC1C File Offset: 0x0003AE1C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FOWNERIDHEAD":
			{
				object value = base.View.Model.GetValue("FOwnerTypeIdHead");
				string text;
				if (value != null && value.ToString() == "BD_OwnerOrg" && this.GetFieldFilter(e.BaseDataFieldKey.ToUpperInvariant(), out text, e.Row))
				{
					e.Filter = (string.IsNullOrEmpty(e.Filter) ? text : (e.Filter + "AND" + text));
					return;
				}
				break;
			}
			case "FLOT":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) < 0L)
				{
					base.View.ShowNotificationMessage(ResManager.LoadKDString("请先输入物料!", "004023030002182", 5, new object[0]), "", 0);
					return;
				}
				break;
			}
			case "FBOMID":
			case "FPFBOMID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					e.Filter = (string.IsNullOrEmpty(e.Filter) ? text : (e.Filter + " AND " + text));
					return;
				}
				base.View.ShowNotificationMessage(ResManager.LoadKDString("请先输入物料", "004023030002185", 5, new object[0]), "", 0);
				return;
			}
			case "FMATERIALID":
			case "FSTOCKID":
			case "FEXTAUXUNITID":
			case "FSTOCKSTATUSID":
			case "FSTOCKGROUPID":
			case "FSTOCKERID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					e.Filter = (string.IsNullOrWhiteSpace(e.Filter) ? text : (e.Filter + " AND " + text));
				}
				break;
			}

				return;
			}
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0003CE90 File Offset: 0x0003B090
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0003CEF4 File Offset: 0x0003B0F4
		public override void ShowFlexFormLoad(ShowFlexFormLoadEventArgs e)
		{
			base.ShowFlexFormLoad(e);
			if (StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				e.CancelFlexItemInit = true;
			}
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0003CF1B File Offset: 0x0003B11B
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
				this.SyncAuxPropData(e.FlexField, e.Row);
			}
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0003CF70 File Offset: 0x0003B170
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

		// Token: 0x060004F7 RID: 1271 RVA: 0x0003D000 File Offset: 0x0003B200
		public virtual List<string> GetOnlySumbFields()
		{
			return new List<string>
			{
				"FQty",
				"FBaseQty",
				"FSNQty",
				"FSecQty",
				"FExtAuxUnitQty",
				"FInvQty",
				"FAmount"
			};
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0003D064 File Offset: 0x0003B264
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

		// Token: 0x060004F9 RID: 1273 RVA: 0x0003D158 File Offset: 0x0003B358
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

		// Token: 0x060004FA RID: 1274 RVA: 0x0003D244 File Offset: 0x0003B444
		private void AuxpropDataChanged(DynamicObject newAuxpropData, int row)
		{
			string a = Convert.ToString(base.View.Model.GetValue("FConvertType", row));
			if (a != "A")
			{
				return;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMId", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterialExceptApi = SCMCommon.GetBomDefaultValueByMaterialExceptApi(base.View, dynamicObject, newAuxpropData, false, value2, value, false);
			if (bomDefaultValueByMaterialExceptApi != value)
			{
				base.View.Model.SetValue("FBOMId", bomDefaultValueByMaterialExceptApi, row);
			}
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x0003D300 File Offset: 0x0003B500
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

		// Token: 0x060004FC RID: 1276 RVA: 0x0003D3A0 File Offset: 0x0003B5A0
		private bool GetFieldFilter(string fieldKey, out string filter, int row = -1)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FOWNERIDHEAD":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					List<SelectorItemInfo> list = new List<SelectorItemInfo>();
					list.Add(new SelectorItemInfo("FOrgId"));
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "ORG_BizRelation",
						FilterClauseWihtKey = string.Format("FRelationOrgID={0} and FBRTypeId=112", dynamicObject["Id"]),
						SelectItems = list
					};
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
					if (dynamicObjectCollection.Count > 0)
					{
						filter = string.Format("(FORGID in (select t0.FORGID from t_org_bizrelationEntry t0\r\n                            left join t_org_bizrelation t1 on t0.FBIZRELATIONID=t1.FBIZRELATIONID\r\n                            where  t1.FBRTYPEID = 112 and t0.FRELATIONORGID={0}) \r\n                            or FORGID in ({0}))", dynamicObject["Id"]);
					}
				}
				break;
			}
			case "FBOMID":
			case "FPFBOMID":
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", row) as DynamicObject;
				if (dynamicObject2 != null)
				{
					filter = string.Format("FMATERIALID={0}", dynamicObject2["Id"]);
				}
				break;
			}
			case "FSTOCKID":
			{
				if (base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null)
				{
					return false;
				}
				string arg = string.Empty;
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockStatusId", row) as DynamicObject;
				arg = ((dynamicObject3 == null) ? "" : Convert.ToString(dynamicObject3["Number"]));
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FType"));
				QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
				{
					FormId = "BD_StockStatus",
					FilterClauseWihtKey = string.Format("FNumber='{0}'", arg),
					SelectItems = list2
				};
				DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter2, null);
				if (dynamicObjectCollection2.Count > 0)
				{
					DynamicObject dynamicObject4 = dynamicObjectCollection2[0];
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject4["FType"]);
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				if (base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null)
				{
					return false;
				}
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FSTOCKID", row) as DynamicObject;
				if (dynamicObject5 != null)
				{
					string text = dynamicObject5["StockStatusType"].ToString();
					if (!string.IsNullOrWhiteSpace(text))
					{
						text = "'" + text.Replace(",", "','") + "'";
						filter = string.Format(" FTYPE in ({0})", text);
					}
				}
				break;
			}
			case "FMATERIALID":
				filter = (this._cancelLotValidate ? " FIsInventory = '1'" : "(FISBATCHMANAGE = '1' OR FIsKFPeriod='1') AND FIsInventory = '1'");
				break;
			case "FEXTAUXUNITID":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
				break;
			case "FSTOCKGROUPID":
			{
				DynamicObject dynamicObject6 = base.View.Model.GetValue("FStockerId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject6 != null && Convert.ToInt64(dynamicObject6["Id"]) > 0L)
				{
					filter += string.Format("AND FENTRYID IN (SELECT TOD.FOPERATORGROUPID FROM T_BD_OPERATORENTRY TOE\r\n                                                INNER JOIN T_BD_OPERATORDETAILS TOD ON TOD.FENTRYID = TOE.FENTRYID\r\n                                                WHERE TOE.FENTRYID = {0})", Convert.ToInt64(dynamicObject6["Id"]));
				}
				break;
			}
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject7 = base.View.Model.GetValue("FStockGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject7 != null && Convert.ToInt64(dynamicObject7["Id"]) > 0L)
				{
					filter += string.Format(" AND FOPERATORGROUPID = {0}", Convert.ToInt64(dynamicObject7["Id"]));
				}
				break;
			}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0003D848 File Offset: 0x0003BA48
		private bool CancelLotValidateSettingChecked()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FBillTypeID") as DynamicObject;
			string text = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Id"]);
			ICommonService commonService = ServiceFactory.GetCommonService(base.Context);
			return Convert.ToInt32(commonService.GetBillTypeParamProfile(base.Context, text, "STK_LotAdjustBillTypeParmSetting", "CancelLotValidate", 0)) == 1;
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x0003D8BC File Offset: 0x0003BABC
		private void SetDefCurrency()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBaseCurrId", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x0003D8FC File Offset: 0x0003BAFC
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
			base.View.Model.SetValue("FOwnerTypeId", value, row);
			base.View.Model.SetValue("FOwnerId", num, row);
			string text = base.View.Model.GetValue("FKeeperTypeId", row) as string;
			if (!string.IsNullOrWhiteSpace(text) && text.Equals("BD_KeeperOrg"))
			{
				base.View.Model.SetValue("FKeeperId", num2, row);
			}
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0003DA1C File Offset: 0x0003BC1C
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
			int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
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

		// Token: 0x06000501 RID: 1281 RVA: 0x0003DB74 File Offset: 0x0003BD74
		private void SetDefValueAfterAddEntry(string convertType, int row)
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
			if (entryRowCount > 1)
			{
				int num;
				if (row == 0)
				{
					num = row + 1;
				}
				else
				{
					num = row - 1;
				}
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", num));
				base.View.Model.BeginIniti();
				base.View.Model.SetValue("FGroup", num2, row);
				base.View.Model.EndIniti();
				base.View.Model.SetValue("FConvertType", convertType, row);
				this._beforeSetConvertType = false;
				bool flag = this.SyncDataWhenCreateNewRow("FMATERIALID", convertType, num, row);
				if (flag)
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", num) as DynamicObject;
					if (dynamicObject != null)
					{
						base.View.Model.SetValue("FMATERIALID", dynamicObject["Id"], row);
						base.View.InvokeFieldUpdateService("FMATERIALID", row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FAUXPROPID", convertType, num, row);
				if (flag)
				{
					object value = base.View.Model.GetValue("FAUXPROPID", num);
					if (!this._allowAdjustAuxProp && value != null)
					{
						((DynamicObjectCollection)this.Model.DataObject["STK_LOTADJUSTENTRY"])[row]["AUXPROPID_Id"] = Convert.ToInt64(((DynamicObject)value)["Id"]);
						base.View.Model.SetValue("FAUXPROPID", value, row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FBOMID", convertType, num, row);
				if (flag)
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FBOMID", num) as DynamicObject;
					if (dynamicObject != null)
					{
						base.View.Model.SetValue("FBOMID", dynamicObject["Id"], row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FStockStatusId", convertType, num, row);
				if (flag)
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FStockStatusId", num) as DynamicObject;
					if (!this._allowAdjustStockStatus && dynamicObject != null)
					{
						base.View.Model.SetValue("FStockStatusId", dynamicObject["Id"], row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FKeeperTypeId", convertType, num, row);
				if (flag)
				{
					object value2 = base.View.Model.GetValue("FKeeperTypeId", num);
					if (value2 != null)
					{
						base.View.Model.SetValue("FKeeperTypeId", value2, row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FKeeperId", convertType, num, row);
				if (flag)
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FKeeperId", num) as DynamicObject;
					if (dynamicObject != null)
					{
						base.View.Model.SetValue("FKeeperId", dynamicObject["Id"], row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FProduceDate", convertType, num, row);
				if (flag)
				{
					object value3 = base.View.Model.GetValue("FProduceDate", num);
					if (value3 != null && convertType.Equals('A'))
					{
						base.View.Model.SetValue("FProduceDate", value3, row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FExpiryDate", convertType, num, row);
				if (flag)
				{
					object value4 = base.View.Model.GetValue("FExpiryDate", num);
					if (value4 != null && convertType.Equals('A'))
					{
						base.View.Model.SetValue("FExpiryDate", value4, row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FMtono", convertType, num, row);
				if (flag)
				{
					object value5 = base.View.Model.GetValue("FMtono", num);
					if (value5 != null)
					{
						base.View.Model.SetValue("FMtono", value5, row);
					}
				}
				flag = this.SyncDataWhenCreateNewRow("FProjectNo", convertType, num, row);
				if (flag)
				{
					object value6 = base.View.Model.GetValue("FProjectNo", num);
					if (value6 != null)
					{
						base.View.Model.SetValue("FProjectNo", value6, row);
					}
				}
				this.LockField(row);
				return;
			}
			if (entryRowCount == 1 && row == 0)
			{
				base.View.Model.SetValue("FGroup", 1, row);
			}
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x0003DFFC File Offset: 0x0003C1FC
		private void SetDyValueInSameGroup(string key, object value, int row)
		{
			int iGroup = Convert.ToInt32(base.View.Model.GetValue("FGroup", row));
			DynamicObjectCollection source = base.View.Model.DataObject["STK_LOTADJUSTENTRY"] as DynamicObjectCollection;
			IEnumerator<DynamicObject> enumerator = (from p in source
			where Convert.ToInt32(p["FGroup"]) == iGroup
			select p).GetEnumerator();
			string propertyName = base.View.BusinessInfo.GetField(key).PropertyName;
			long num = Convert.ToInt64(value);
			while (enumerator.MoveNext())
			{
				DynamicObject dynamicObject = enumerator.Current;
				DynamicObject dynamicObject2 = dynamicObject[propertyName] as DynamicObject;
				long num2 = 0L;
				if (dynamicObject2 != null)
				{
					num2 = Convert.ToInt64(dynamicObject2["Id"]);
				}
				int num3 = Convert.ToInt32(enumerator.Current["Seq"]) - 1;
				if (num3 != row && num2 != num)
				{
					this.m_DataChanged = false;
					base.View.Model.SetValue(key, value, num3);
					base.View.InvokeFieldUpdateService(key, num3);
				}
			}
			this.m_DataChanged = true;
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0003E13C File Offset: 0x0003C33C
		private void SetStrValueInSameGroup(string key, string value, int row)
		{
			int iGroup = Convert.ToInt32(base.View.Model.GetValue("FGroup", row));
			DynamicObjectCollection source = base.View.Model.DataObject["STK_LOTADJUSTENTRY"] as DynamicObjectCollection;
			IEnumerator<DynamicObject> enumerator = (from p in source
			where Convert.ToInt32(p["FGroup"]) == iGroup
			select p).GetEnumerator();
			string propertyName = base.View.BusinessInfo.GetField(key).PropertyName;
			while (enumerator.MoveNext())
			{
				string text = (enumerator.Current[propertyName] == null) ? "" : enumerator.Current[propertyName].ToString();
				int num = Convert.ToInt32(enumerator.Current["Seq"]) - 1;
				if (num != row && !text.Equals(value))
				{
					this.m_DataChanged = false;
					base.View.Model.SetValue(key, value, num);
					base.View.InvokeFieldUpdateService(key, num);
				}
			}
			this.m_DataChanged = true;
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0003E24C File Offset: 0x0003C44C
		private void LockStockStatusFieldInSameGroup()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
			for (int i = 1; i < entryRowCount; i++)
			{
				int num = Convert.ToInt32(base.View.Model.GetValue("FGroup", i));
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", i - 1));
				if (num == num2)
				{
					if (!this._allowAdjustStockStatus)
					{
						base.View.GetFieldEditor("FStockStatusId", i).Enabled = false;
					}
					if (!this._allowAdjustAuxProp)
					{
						base.View.GetFieldEditor("FAUXPROPID", i).Enabled = false;
					}
				}
			}
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0003E300 File Offset: 0x0003C500
		private void LockAuxPropFieldInSameGroup()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
			for (int i = 1; i < entryRowCount; i++)
			{
				int num = Convert.ToInt32(base.View.Model.GetValue("FGroup", i));
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", i - 1));
				if (num == num2 && !this._allowAdjustAuxProp)
				{
					base.View.GetFieldEditor("FAUXPROPID", i).Enabled = false;
				}
			}
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0003E38C File Offset: 0x0003C58C
		private void LockFieldInSameGroup()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FSTK_LOTADJUSTENTRY");
			for (int i = 1; i < entryRowCount; i++)
			{
				int num = Convert.ToInt32(base.View.Model.GetValue("FGroup", i));
				int num2 = Convert.ToInt32(base.View.Model.GetValue("FGroup", i - 1));
				if (num == num2)
				{
					this.LockField(i);
				}
			}
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0003E400 File Offset: 0x0003C600
		private void LockField(int row)
		{
			base.View.GetFieldEditor("FMATERIALID", row).Enabled = false;
			base.View.GetFieldEditor("FUnitID", row).Enabled = false;
			base.View.GetFieldEditor("FBOMID", row).Enabled = false;
			base.View.GetFieldEditor("FMtono", row).Enabled = false;
			if (!this._allowAdjustStockStatus)
			{
				base.View.GetFieldEditor("FStockStatusId", row).Enabled = false;
			}
			if (!this._allowAdjustAuxProp)
			{
				base.View.GetFieldEditor("FAUXPROPID", row).Enabled = false;
			}
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0003E4A8 File Offset: 0x0003C6A8
		private void UnLockField(int row)
		{
			base.View.GetFieldEditor("FMATERIALID", row).Enabled = true;
			base.View.GetFieldEditor("FUnitID", row).Enabled = true;
			base.View.GetFieldEditor("FBOMID", row).Enabled = true;
			base.View.GetFieldEditor("FMtono", row).Enabled = true;
			if (!this._allowAdjustStockStatus)
			{
				base.View.GetFieldEditor("FStockStatusId", row).Enabled = true;
			}
			if (!this._allowAdjustAuxProp)
			{
				base.View.GetFieldEditor("FAUXPROPID", row).Enabled = true;
			}
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x0003E550 File Offset: 0x0003C750
		private bool GetBillTypeAdjustStockStatus()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FBillTypeID") as DynamicObject;
			string text = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Id"]);
			ICommonService commonService = ServiceFactory.GetCommonService(base.Context);
			return Convert.ToInt32(commonService.GetBillTypeParamProfile(base.Context, text, "STK_LotAdjustBillTypeParmSetting", "AdjustStockStatus", 0)) == 1;
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0003E5C4 File Offset: 0x0003C7C4
		private bool GetBillTypeAdjustAuxProp()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FBillTypeID") as DynamicObject;
			string text = (dynamicObject == null) ? "" : Convert.ToString(dynamicObject["Id"]);
			ICommonService commonService = ServiceFactory.GetCommonService(base.Context);
			return Convert.ToInt32(commonService.GetBillTypeParamProfile(base.Context, text, "STK_LotAdjustBillTypeParmSetting", "AdjustAuxProp", 0)) == 1;
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0003E688 File Offset: 0x0003C888
		private void SetDefSupplierLotValue(int row)
		{
			int iGroup = Convert.ToInt32(base.View.Model.GetValue("FGroup", row));
			Entity entity = base.View.Model.BusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY");
			List<DynamicObject> list = (from p in this.Model.GetEntityDataObject(entity)
			where Convert.ToInt32(p["seq"]) < row + 1 && Convert.ToInt32(p["FGroup"]) == iGroup
			orderby Convert.ToInt32(p["seq"]) descending
			select p).ToList<DynamicObject>();
			if (list != null && list.Count<DynamicObject>() > 0)
			{
				string text = string.Empty;
				int indexA = -1;
				foreach (DynamicObject dynamicObject in list)
				{
					if (Convert.ToString(base.View.Model.GetValue("FConvertType", Convert.ToInt32(dynamicObject["seq"]) - 1)).Equals("A"))
					{
						text = Convert.ToString(base.View.Model.GetValue("FSupplierLot", Convert.ToInt32(dynamicObject["seq"]) - 1));
						indexA = Convert.ToInt32(dynamicObject["seq"]) - 1;
						break;
					}
				}
				if (this.SyncDataWhenCreateNewRow("FSupplierLot", "B", indexA, row) && !string.IsNullOrEmpty(text))
				{
					base.View.Model.SetValue("FSupplierLot", text, row);
				}
			}
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x0003E848 File Offset: 0x0003CA48
		private void SetSupplierLotEnable(int row, bool enable)
		{
			object value = base.View.Model.GetValue("FConvertType", row);
			if (value.Equals("A"))
			{
				base.View.GetFieldEditor("FSupplierLot", row).Enabled = enable;
			}
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0003E8AC File Offset: 0x0003CAAC
		private void LockSupplierLot()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			List<DynamicObject> list = (from p in base.View.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FSTK_LOTADJUSTENTRY"))
			where Convert.ToString(p["ConvertType"]).Equals("A")
			select p).ToList<DynamicObject>();
			if (list != null)
			{
				LotField lotField = this.Model.BusinessInfo.GetField("Flot") as LotField;
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FSupplyLot"));
				foreach (DynamicObject dynamicObject2 in list)
				{
					DynamicObject dynamicObject3 = dynamicObject2["MaterialId"] as DynamicObject;
					DynamicObject dynamicObject4 = dynamicObject2["Lot"] as DynamicObject;
					object value = lotField.TextDynamicProperty.GetValue(dynamicObject2);
					if (dynamicObject3 == null || (dynamicObject3 != null && dynamicObject4 != null && Convert.ToInt64(dynamicObject4["Id"]) > 0L))
					{
						this.SetSupplierLotEnable(Convert.ToInt32(dynamicObject2["Seq"]) - 1, false);
					}
					else if (dynamicObject3 != null && dynamicObject4 == null && !string.IsNullOrEmpty(Convert.ToString(value)))
					{
						QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
						{
							FormId = "BD_BatchMainFile",
							FilterClauseWihtKey = string.Format(" FNUMBER = N'{1}' AND FBIZTYPE='1' AND FMATERIALID = {0} AND FUSEORGID = {2} ", Convert.ToInt64(dynamicObject3["msterId"]), value, (dynamicObject != null) ? Convert.ToInt64(dynamicObject["Id"]) : 0L),
							SelectItems = list2
						};
						DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
						if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
						{
							this.SetSupplierLotEnable(Convert.ToInt32(dynamicObject2["Seq"]) - 1, false);
						}
					}
				}
			}
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0003EAD8 File Offset: 0x0003CCD8
		private void SetFlexFixedColEnable(DynamicObject row, RelatedFlexGroupField field, List<string> listFlexId)
		{
			RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = (RelatedFlexGroupFieldAppearance)base.View.LayoutInfo.GetFieldAppearance(field.Key);
			foreach (FieldAppearance fieldAppearance in relatedFlexGroupFieldAppearance.RelateFlexLayoutInfo.GetFieldAppearances())
			{
				string item = fieldAppearance.Key.TrimStart(new char[]
				{
					'F'
				});
				string text = string.Format("$${0}__{1}", field.Key.ToUpperInvariant(), fieldAppearance.Field.Key.ToUpperInvariant());
				if (listFlexId != null && listFlexId.Contains(item))
				{
					base.View.StyleManager.SetEnabled(text, row, "", true);
				}
				else
				{
					base.View.StyleManager.SetEnabled(text, row, "", false);
				}
			}
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x0003EBCC File Offset: 0x0003CDCC
		public virtual bool SyncDataWhenCreateNewRow(string fieldKey, string convertType, int indexA, int indexB)
		{
			return true;
		}

		// Token: 0x040001D4 RID: 468
		private string m_EntryBarItemClickMenuId = string.Empty;

		// Token: 0x040001D5 RID: 469
		private int oldIndex = -1;

		// Token: 0x040001D6 RID: 470
		private bool finishCopy;

		// Token: 0x040001D7 RID: 471
		private int copyIndex = -1;

		// Token: 0x040001D8 RID: 472
		private bool isCopyBill;

		// Token: 0x040001D9 RID: 473
		private bool m_DataChanged = true;

		// Token: 0x040001DA RID: 474
		private bool _beforeSetConvertType;

		// Token: 0x040001DB RID: 475
		private long lastAuxpropId;

		// Token: 0x040001DC RID: 476
		private List<string> _onlySumBFields;

		// Token: 0x040001DD RID: 477
		private bool _allowAdjustStockStatus;

		// Token: 0x040001DE RID: 478
		private bool _allowAdjustAuxProp;

		// Token: 0x040001DF RID: 479
		private string _invQueryReRuleForLotAdjust = "1";

		// Token: 0x040001E0 RID: 480
		private bool _cancelLotValidate;

		// Token: 0x040001E1 RID: 481
		private List<int> _queryStockInsertRows = new List<int>();

		// Token: 0x040001E2 RID: 482
		private List<int> _queryStockLockRows = new List<int>();

		// Token: 0x040001E3 RID: 483
		private List<int> _queryStockUnLockRows = new List<int>();
	}
}
