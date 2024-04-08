using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Model.ListFilter;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000A2 RID: 162
	public class StockCountSchemeEdit : AbstractBillPlugIn
	{
		// Token: 0x060009AF RID: 2479 RVA: 0x0008274F File Offset: 0x0008094F
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			this.filterGrid = base.View.GetControl<FilterGrid>("FFilterGrid");
			this.InitialFilterMetaData();
			this.InitialFilterGrid();
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x00082774 File Offset: 0x00080974
		public override void AfterBindData(EventArgs e)
		{
			this.FillFilterGridData();
			Control control = base.View.GetControl("FPanelHide");
			control.Visible = false;
			if (this.dycSepBillItem == null)
			{
				this.dycSepBillItem = StockServiceHelper.GetCountSepBillItem(base.Context, "Table");
			}
			this.InitiaSepBillItem(this.dycSepBillItem);
			if (this.dycSortItem == null)
			{
				this.dycSortItem = StockServiceHelper.GetCountSortItem(base.Context, "Table");
			}
			this.InitiSortItem(this.dycSortItem);
			string a = Convert.ToString(base.View.Model.GetValue("FDocumentStatus"));
			this.filterGrid.Enabled = (a != "B" && a != "C");
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x00082834 File Offset: 0x00080A34
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.AfterUpdateViewState(e);
			this.filterGrid = base.View.GetControl<FilterGrid>("FFilterGrid");
			string text = Convert.ToString(base.View.Model.GetValue("FDocumentStatus"));
			text = ((text == null) ? "" : text.ToUpperInvariant());
			this.filterGrid.Enabled = (text != "B" && text != "C");
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x000828B0 File Offset: 0x00080AB0
		public override void AfterCreateNewData(EventArgs e)
		{
			if (base.View.OpenParameter.CreateFrom == null)
			{
				this.SetBackUpDate();
			}
			if (this.dycSepBillItem == null)
			{
				this.dycSepBillItem = StockServiceHelper.GetCountSepBillItem(base.Context, "Table");
			}
			this.InitiaSepBillItem(this.dycSepBillItem);
			if (this.dycSortItem == null)
			{
				this.dycSortItem = StockServiceHelper.GetCountSortItem(base.Context, "Table");
			}
			this.InitiSortItem(this.dycSortItem);
			this.InitMoveBills();
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x00082930 File Offset: 0x00080B30
		public override void ToolBarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBCLEARMATRANGE")
				{
					Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMatRange");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					entityDataObject.Clear();
					base.View.UpdateView("FEntityMatRange");
					return;
				}
				if (a == "TBCLEARMATGRPRANGE")
				{
					Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMatGroupRange");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					entityDataObject.Clear();
					base.View.UpdateView("FEntityMatGroupRange");
					return;
				}
			}
			base.ToolBarItemClick(e);
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x000829E8 File Offset: 0x00080BE8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null && a == "TBQUERYCOUNTINPUT")
			{
				this.QueryCountInput();
				return;
			}
			base.BarItemClick(e);
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00082A20 File Offset: 0x00080C20
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			e.IsShowUsed = true;
			e.IsShowApproved = true;
			e.CommonFilterModel = this._listFilterModel;
			IFilterLookUpField filterLookUpField = e.CommonFilterModel.GetFilterField(e.Key) as IFilterLookUpField;
			if (filterLookUpField == null)
			{
				return;
			}
			if (filterLookUpField.LookUpField.LookUpObject.FormId == "BOS_FLEXVALUE_SELECT")
			{
				string value = e.Key.Split(new char[]
				{
					'.'
				})[1].Replace("FF", "");
				if (e.ListFilterParameter.Filter.Trim().Length > 0)
				{
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter += " and ";
				}
				IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
				listFilterParameter2.Filter += string.Format(" FID={0} ", Convert.ToInt64(value));
			}
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x00082B0C File Offset: 0x00080D0C
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FRGMATERIALID") && !(a == "FRGMATERIALGROUPID"))
				{
					return;
				}
				e.IsShowUsed = false;
			}
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x00082B54 File Offset: 0x00080D54
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FRGMATERIALID") && !(a == "FRGMATERIALGROUPID"))
				{
					return;
				}
				e.IsShowUsed = false;
			}
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x00082B9C File Offset: 0x00080D9C
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			string text;
			if (dynamicObject == null)
			{
				text = ResManager.LoadKDString("请先录入库存组织！", "004023000017559", 5, new object[0]);
			}
			else
			{
				string strValue = Convert.ToString(this.Model.GetValue("FBackUpDate"));
				text = this.CheckBackupDate(strValue, Convert.ToInt64(dynamicObject["Id"]));
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				e.Cancel = true;
				base.View.ShowWarnningMessage(text, ResManager.LoadKDString("日期错误", "004023030002305", 5, new object[0]), 0, null, 1);
				return;
			}
			this.ClearBlankSortRow();
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x00082CA8 File Offset: 0x00080EA8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string text = (e.Value != null) ? e.Value.ToString() : "";
			string key;
			switch (key = e.Key.ToLowerInvariant())
			{
			case "ffiltergrid":
				if (!text.Equals(this.strFilterValue))
				{
					this.strFilterValue = text;
					this.AnalyzeFilterSetting(text);
					this.RebuildFilterItems();
				}
				break;
			case "fbackupdate":
			{
				DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
				string text2;
				if (dynamicObject == null)
				{
					text2 = ResManager.LoadKDString("请先录入库存组织！", "004023000017559", 5, new object[0]);
				}
				else
				{
					text2 = this.CheckBackupDate(text, Convert.ToInt64(dynamicObject["Id"]));
				}
				if (!string.IsNullOrWhiteSpace(text2))
				{
					e.Cancel = true;
					base.View.ShowWarnningMessage(text2, ResManager.LoadKDString("日期错误", "004023030002305", 5, new object[0]), 0, null, 1);
					base.View.UpdateView("FBackupDate");
				}
				break;
			}
			case "fsepbillitemid":
			{
				DynamicObjectCollection source = base.View.Model.DataObject["STK_STKCOUNTSCHEMESEPB"] as DynamicObjectCollection;
				DynamicObject dynamicObject2 = (from d in source
				where d["SepBillItemId"].ToString() == e.Value.ToString()
				select d).FirstOrDefault<DynamicObject>();
				if (dynamicObject2 != null)
				{
					base.View.ShowMessage(ResManager.LoadKDString("不允许添加重复的规则选项！", "004023030002095", 5, new object[0]), 0);
					e.Cancel = true;
					base.View.UpdateView("FSepBillItemId", e.Row);
				}
				break;
			}
			case "fsortitemid":
			{
				DynamicObjectCollection source = base.View.Model.DataObject["EntitySort"] as DynamicObjectCollection;
				DynamicObject dynamicObject2 = (from d in source
				where d["SortItemId"].ToString() == e.Value.ToString()
				select d).FirstOrDefault<DynamicObject>();
				if (dynamicObject2 != null)
				{
					base.View.ShowMessage(ResManager.LoadKDString("不允许添加重复的规则选项！", "004023030002095", 5, new object[0]), 0);
					e.Cancel = true;
					base.View.UpdateView("FSortItemId", e.Row);
				}
				break;
			}
			case "fmovebegdate":
			case "fmoveenddate":
			{
				if (text.Length > 0)
				{
					DateTime t = DateTime.Parse(text);
					if (t > new DateTime(9999, 12, 30))
					{
						string text2 = ResManager.LoadKDString("动盘开始日期不能大于9999-12-30。", "004023030009200", 5, new object[0]);
						if (e.Key.ToLowerInvariant() == "fmoveenddate")
						{
							text2 = ResManager.LoadKDString("动盘结束日期不能大于9999-12-30。", "004023030009201", 5, new object[0]);
						}
						base.View.ShowMessage(text2, 0);
						e.Cancel = true;
						break;
					}
				}
				string a = Convert.ToString(this.Model.GetValue("FCountType"));
				if (a == "M")
				{
					DateTime? dateTime = null;
					object value = this.Model.GetValue("FMoveBegDate");
					if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
					{
						dateTime = new DateTime?(Convert.ToDateTime(value));
					}
					DateTime? dateTime2 = null;
					value = this.Model.GetValue("FMoveEndDate");
					if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
					{
						dateTime2 = new DateTime?(Convert.ToDateTime(value));
					}
					if (e.Value != null && !string.IsNullOrWhiteSpace(e.Value.ToString()) && "fmovebegdate".Equals(e.Key.ToLowerInvariant()))
					{
						dateTime = new DateTime?(Convert.ToDateTime(e.Value));
					}
					else if (e.Value != null && !string.IsNullOrWhiteSpace(e.Value.ToString()) && "fmoveenddate".Equals(e.Key.ToLowerInvariant()))
					{
						dateTime2 = new DateTime?(Convert.ToDateTime(e.Value));
					}
					if (dateTime != null && dateTime2 != null && dateTime > dateTime2)
					{
						base.View.ShowMessage(ResManager.LoadKDString("动盘开始日期必须小于等于动盘结束日期。", "004023030009265", 5, new object[0]), 0);
						e.Cancel = true;
					}
				}
				break;
			}
			}
			base.BeforeUpdateValue(e);
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x00083208 File Offset: 0x00081408
		private string CheckBackupDate(string strValue, long orgId)
		{
			string result = "";
			DateTime stkCloseDateByOrgId = CommonServiceHelper.GetStkCloseDateByOrgId(base.Context, orgId);
			if (stkCloseDateByOrgId.Date == DateTime.MaxValue.Date)
			{
				object updateStockDate = StockServiceHelper.GetUpdateStockDate(base.Context, orgId);
				if (updateStockDate != null && strValue != "" && DateTime.Parse(strValue).CompareTo(DateTime.Parse(updateStockDate.ToString())) < 0)
				{
					result = string.Format(ResManager.LoadKDString("截止日期不能小于库存组织的库存启用日期（{0}）!", "004023030002299", 5, new object[0]), updateStockDate);
				}
			}
			else if (strValue != "" && DateTime.Parse(strValue).CompareTo(stkCloseDateByOrgId) < 0)
			{
				result = string.Format(ResManager.LoadKDString("截止日期不能小于库存组织的最后关账日期（{0}）!", "004023030002302", 5, new object[0]), stkCloseDateByOrgId.ToShortDateString());
			}
			return result;
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x0008333C File Offset: 0x0008153C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSEPBILLITEMID":
			{
				object obj = e.NewValue;
				DynamicObject dynamicObject = (from d in this.dycSepBillItem
				where d["Value"].ToString() == e.NewValue.ToString()
				select d).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					base.View.Model.SetValue("FSrcFieldName", dynamicObject["FSRCFIELDNAME"], e.Row);
					base.View.Model.SetValue("FTableName", dynamicObject["FTableName"], e.Row);
					base.View.Model.SetValue("FTableRelation", dynamicObject["FTABLERELATION"], e.Row);
					return;
				}
				break;
			}
			case "FSORTITEMID":
			{
				object obj = e.NewValue;
				DynamicObject dynamicObject = (from d in this.dycSortItem
				where d["Value"].ToString() == e.NewValue.ToString()
				select d).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					base.View.Model.SetValue("FSrcFieldNameSt", dynamicObject["FSRCFIELDNAME"], e.Row);
					base.View.Model.SetValue("FTableNameSt", dynamicObject["FTableName"], e.Row);
					base.View.Model.SetValue("FTableRelationSt", dynamicObject["FTABLERELATION"], e.Row);
					return;
				}
				break;
			}
			case "FCOUNTTYPE":
			case "FBACKUPTYPE":
			case "FBACKUPDATE":
			{
				string a = Convert.ToString(this.Model.GetValue("FCountType"));
				if (!(a == "M"))
				{
					this.Model.SetValue("FMoveBegDate", null);
					this.Model.SetValue("FMoveEndDate", null);
					return;
				}
				string a2 = Convert.ToString(this.Model.GetValue("FBackUpType"));
				object value = this.Model.GetValue("FMoveBegDate");
				if (!(a2 == "CloseDate"))
				{
					DateTime today = DateTime.Today;
					if (value == null || (value != null && !string.IsNullOrWhiteSpace(value.ToString()) && Convert.ToDateTime(value) > today))
					{
						this.Model.SetValue("FMoveBegDate", today);
					}
					this.Model.SetValue("FMoveEndDate", today);
					return;
				}
				object obj = this.Model.GetValue("FBackUpDATE");
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					if (value == null || (value != null && !string.IsNullOrWhiteSpace(value.ToString()) && Convert.ToDateTime(value) > Convert.ToDateTime(obj)))
					{
						this.Model.SetValue("FMoveBegDate", obj);
					}
					this.Model.SetValue("FMoveEndDate", obj);
					return;
				}
				this.Model.SetValue("FMoveBegDate", null);
				this.Model.SetValue("FMoveEndDate", null);
				return;
			}
			case "FRGMATERIALGROUPID":
			{
				DynamicObject dynamicObject2 = this.Model.GetValue("FRgMaterialGroupId", e.Row) as DynamicObject;
				if (dynamicObject2 != null)
				{
					this.Model.SetValue("FRgSMaterialGroupId", "." + dynamicObject2["Id"].ToString(), e.Row);
					return;
				}
				this.Model.SetValue("FRgSMaterialGroupId", "0", e.Row);
				break;
			}

				return;
			}
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x0008377C File Offset: 0x0008197C
		private void AnalyzeFilterSetting(string strFilterSetting)
		{
			if (StringUtils.IsEmpty(strFilterSetting))
			{
				return;
			}
			if (this._listFilterModel == null)
			{
				return;
			}
			if (this._listFilterModel.FilterObject.AllFilterFieldList == null)
			{
				return;
			}
			this._listFilterModel.FilterObject.Setting = strFilterSetting;
			string filterSQLString = this._listFilterModel.FilterObject.GetFilterSQLString(base.Context, this.GetUserNow(this._listFilterModel.FilterObject));
			this.Model.SetValue("FFilterSetting", this._listFilterModel.FilterObject.Setting);
			this.Model.SetValue("FFilterString", filterSQLString);
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x00083818 File Offset: 0x00081A18
		private void FillFilterGridData()
		{
			if (this._listFilterModel == null)
			{
				this._listFilterModel = new ListFilterModel();
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "STK_StkCountSchemeFilterFileds");
				this._listFilterModel.FilterObject.FilterMetaData = this._filterMetaData;
				this._listFilterModel.SetContext(base.Context, formMetaData.BusinessInfo, formMetaData.BusinessInfo.GetForm().GetFormServiceProvider(false));
				this._listFilterModel.InitFieldList(formMetaData, null);
				this._listFilterModel.FilterObject.SetSelectEntity(",FBILLHEAD,");
			}
			this.filterGrid.SetFilterFields(this._listFilterModel.FilterObject.GetAllFilterFieldList());
			EntitySelect control = base.View.GetControl<EntitySelect>(CommonFilterConst.ControlKey_EntitySelect);
			control.SetEntities(this._listFilterModel.EntityObject.GetAllEntities());
			control.SetSelectEntities(",FBILLHEAD,");
			DynamicObject dynamicObject = this.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = 0L;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			this._listFilterModel.IsolationOrgId = num;
			List<long> list = new List<long>();
			list.Add(num);
			this._listFilterModel.IsolationOrgList = list;
			object value = this.Model.GetValue("FFilterSetting");
			string setting = (value != null && !string.IsNullOrWhiteSpace(value.ToString())) ? value.ToString() : "[]";
			this._listFilterModel.FilterObject.Setting = setting;
			this.filterGrid.SetFilterRows(this._listFilterModel.FilterObject.GetFilterRows());
			this.strFilterValue = setting;
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x000839B4 File Offset: 0x00081BB4
		private DateTime? GetUserNow(FilterObject filter)
		{
			bool flag = false;
			foreach (FilterRow filterRow in filter.FilterRows)
			{
				flag = (filterRow.FilterField.FieldType == 58 || filterRow.FilterField.FieldType == 189 || filterRow.FilterField.FieldType == 61);
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				return new DateTime?(TimeServiceHelper.GetUserDateTime(base.Context));
			}
			return null;
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x00083A58 File Offset: 0x00081C58
		private void InitialFilterMetaData()
		{
			if (this._filterMetaData == null)
			{
				this._filterMetaData = CommonFilterServiceHelper.GetFilterMetaData(base.Context, "");
				JSONObject jsonobject = this._filterMetaData.ToJSONObject();
				jsonobject.TryGetValue(CommonFilterConst.JSONKey_CompareTypes, out this._compareTypes);
				jsonobject.TryGetValue(CommonFilterConst.JSONKey_Logics, out this._logicData);
			}
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x00083AB3 File Offset: 0x00081CB3
		private void InitialFilterGrid()
		{
			this.filterGrid.SetCompareTypes(this._compareTypes);
			this.filterGrid.SetLogicData(this._logicData);
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x00083AD8 File Offset: 0x00081CD8
		private void InitiaSepBillItem(DynamicObjectCollection dyc)
		{
			ComboFieldEditor comboFieldEditor = base.View.GetControl("FSepBillItemId") as ComboFieldEditor;
			List<EnumItem> list = new List<EnumItem>();
			string str = string.Empty;
			for (int i = 0; i < dyc.Count; i++)
			{
				if (dyc[i]["FKey"].ToString() == "T_STK_INVENTORY.FAUXPROPID")
				{
					str = dyc[i]["Caption"].ToString();
				}
				else
				{
					EnumItem enumItem = new EnumItem();
					enumItem.EnumId = dyc[i]["EnumId"].ToString();
					enumItem.Value = dyc[i]["Value"].ToString();
					if (dyc[i]["FTableName"].ToString() == "T_BD_FLEXSITEMDETAILV")
					{
						enumItem.Caption = new LocaleValue(str + "." + dyc[i]["Caption"].ToString(), base.Context.UserLocale.LCID);
					}
					else
					{
						enumItem.Caption = new LocaleValue(dyc[i]["Caption"].ToString(), base.Context.UserLocale.LCID);
					}
					enumItem.Seq = Convert.ToInt32(dyc[i]["Seq"]);
					list.Add(enumItem);
				}
			}
			comboFieldEditor.SetComboItems(list);
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x00083C64 File Offset: 0x00081E64
		private void InitiSortItem(DynamicObjectCollection dyc)
		{
			ComboFieldEditor comboFieldEditor = base.View.GetControl("FSortItemId") as ComboFieldEditor;
			List<EnumItem> list = new List<EnumItem>();
			string empty = string.Empty;
			for (int i = 0; i < dyc.Count; i++)
			{
				list.Add(new EnumItem
				{
					EnumId = dyc[i]["EnumId"].ToString(),
					Value = dyc[i]["Value"].ToString(),
					Caption = new LocaleValue(dyc[i]["Caption"].ToString(), base.Context.UserLocale.LCID),
					Seq = Convert.ToInt32(dyc[i]["Seq"])
				});
			}
			comboFieldEditor.SetComboItems(list);
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x00083D48 File Offset: 0x00081F48
		private void InitMoveBills()
		{
			if (this.DefaultMoveBills == null)
			{
				this.FillDefaultMoveBills();
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMoveBill");
			DynamicObjectType dynamicObjectType = entryEntity.DynamicObjectType;
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["EntityMoveBill"] as DynamicObjectCollection;
			if (dynamicObjectCollection == null)
			{
				dynamicObjectCollection = new DynamicObjectCollection(dynamicObjectType, null);
			}
			dynamicObjectCollection.Clear();
			int num = 1;
			foreach (string text in this.DefaultMoveBills)
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["Seq"] = num;
				dynamicObject["MBizFormID_Id"] = text;
				dynamicObjectCollection.Add(dynamicObject);
				num++;
			}
			if (dynamicObjectCollection.Count > 0)
			{
				DBServiceHelper.LoadReferenceObject(base.Context, dynamicObjectCollection.ToArray<DynamicObject>(), dynamicObjectType, false);
			}
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x00083E40 File Offset: 0x00082040
		private void FillDefaultMoveBills()
		{
			this.DefaultMoveBills = new List<string>
			{
				"STK_InStock",
				"PUR_MRB",
				"SAL_OUTSTOCK",
				"SAL_RETURNSTOCK",
				"STK_TransferDirect",
				"STK_TRANSFEROUT",
				"STK_TRANSFERIN",
				"STK_MISCELLANEOUS",
				"STK_MisDelivery",
				"STK_AssembledApp",
				"STK_OEMInStock",
				"STK_OEMInStockRETURN",
				"PRD_PickMtrl",
				"PRD_FeedMtrl",
				"PRD_ReturnMtrl",
				"PRD_INSTOCK",
				"PRD_RetStock",
				"SUB_PickMtrl",
				"SUB_FEEDMTRL",
				"SUB_EXCONSUME",
				"SUB_RETURNMTRL",
				"SP_PickMtrl",
				"SP_ReturnMtrl",
				"SP_InStock",
				"SP_OUTSTOCK"
			};
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x00083F70 File Offset: 0x00082170
		private void RebuildFilterItems()
		{
			Entity entity = base.View.BusinessInfo.GetField("FFilterField").Entity;
			DynamicObjectType dynamicObjectType = entity.DynamicObjectType;
			DynamicObjectCollection dynamicObjectCollection = entity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			if (dynamicObjectCollection == null)
			{
				dynamicObjectCollection = new DynamicObjectCollection(dynamicObjectType, null);
			}
			dynamicObjectCollection.Clear();
			if (this._listFilterModel == null || this._listFilterModel.FilterObject == null || this._listFilterModel.FilterObject.FilterRows == null || this._listFilterModel.FilterObject.FilterRows.Count < 1)
			{
				return;
			}
			foreach (FilterRow filterRow in this._listFilterModel.FilterObject.FilterRows)
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["Seq"] = filterRow.RowIndex;
				dynamicObject["FilterField"] = filterRow.FilterField.FieldName;
				dynamicObject["FilterKey"] = filterRow.FilterField.Key;
				dynamicObject["CompareType"] = filterRow.CompareType.Id;
				dynamicObject["LeftBracket"] = filterRow.LeftBracket;
				dynamicObject["Logic"] = filterRow.Logic;
				dynamicObject["RightBracket"] = filterRow.RightBracket;
				dynamicObject["Value"] = filterRow.Value;
				dynamicObjectCollection.Add(dynamicObject);
			}
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x00084110 File Offset: 0x00082310
		private void QueryCountInput()
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
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_StockCountInput";
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.CustomParams.Add("source", "1");
			listShowParameter.CustomParams.Add("CountTableIds", this.Model.DataObject["Id"].ToString());
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x00084220 File Offset: 0x00082420
		private void SetBackUpDate()
		{
			string text = "";
			object value = this.Model.GetValue("FBackUpType");
			if (value != null)
			{
				text = value.ToString();
			}
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.View.Context);
			this.Model.SetValue("FBackUpDate", StringUtils.EqualsIgnoreCase(text, "CloseDate") ? systemDateTime.Date.ToShortDateString() : "");
			base.View.GetFieldEditor("FBackUpDate", 0).SetEnabled("LockToDate", StringUtils.EqualsIgnoreCase(text, "CloseDate"));
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x000842B8 File Offset: 0x000824B8
		private void ClearBlankSortRow()
		{
			bool flag = false;
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["EntitySort"] as DynamicObjectCollection;
			int num = dynamicObjectCollection.Count - 1;
			for (int i = num; i >= 0; i--)
			{
				if (dynamicObjectCollection[i]["SortItemId"] == null || string.IsNullOrWhiteSpace((string)dynamicObjectCollection[i]["SortItemId"]) || Convert.ToInt64(dynamicObjectCollection[i]["SortItemId"]) < 1L)
				{
					this.Model.DeleteEntryRow("FEntitySort", i);
					flag = true;
				}
			}
			if (flag)
			{
				base.View.UpdateView("FEntitySort");
			}
		}

		// Token: 0x040003EA RID: 1002
		private FilterGrid filterGrid;

		// Token: 0x040003EB RID: 1003
		private ListFilterModel _listFilterModel;

		// Token: 0x040003EC RID: 1004
		private FilterMetaData _filterMetaData;

		// Token: 0x040003ED RID: 1005
		private object _compareTypes;

		// Token: 0x040003EE RID: 1006
		private object _logicData;

		// Token: 0x040003EF RID: 1007
		private DynamicObjectCollection dycSepBillItem;

		// Token: 0x040003F0 RID: 1008
		private DynamicObjectCollection dycSortItem;

		// Token: 0x040003F1 RID: 1009
		private string strFilterValue = "[]";

		// Token: 0x040003F2 RID: 1010
		protected List<string> DefaultMoveBills;
	}
}
