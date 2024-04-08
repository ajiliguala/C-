using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000069 RID: 105
	[Description("物料周期盘点表单据插件")]
	public class CycleCountTableEdit : AbstractBillPlugIn
	{
		// Token: 0x06000489 RID: 1161 RVA: 0x000363DC File Offset: 0x000345DC
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			BillOpenParameter paramter = e.Paramter;
			this.billID = Convert.ToInt64(paramter.PkValue);
			this.fieldList = (from p in this.Model.BusinessInfo.Entrys[1].Fields
			where !string.IsNullOrEmpty(p.FieldName.Trim())
			select p).ToList<Field>();
			this.tmpTabelName = CycleCountTablePageService.CreatePgTmpTable(base.Context, this.billID, this.fieldList);
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00036468 File Offset: 0x00034668
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBQUERYCOUNTINPUT")
				{
					this.QueryCycleCountInput();
					return;
				}
				if (a == "TBQUERYCOUNTPLAN")
				{
					this.QuerySrcleBill("STK_CycleCountPlan");
					return;
				}
			}
			base.BarItemClick(e);
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x000364BC File Offset: 0x000346BC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SUBMIT"))
				{
					return;
				}
				e.Cancel = this.VaildateData();
			}
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x00036503 File Offset: 0x00034703
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			if (e.Key.ToUpperInvariant().Equals("FBTSEARCH"))
			{
				this.SeachFun();
			}
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x00036524 File Offset: 0x00034724
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBPAGEUP")
				{
					this.GetDateByPage(this.currentPage - 1);
					return;
				}
				if (a == "TBPAGEDOWN")
				{
					this.GetDateByPage(this.currentPage + 1);
					return;
				}
				if (a == "TBDELETEENTRY")
				{
					this.DeleteRow();
					return;
				}
				if (!(a == "TBSEARCH"))
				{
					return;
				}
				this.SetPanelVisible(!this.visble);
			}
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x000365DC File Offset: 0x000347DC
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToLowerInvariant()) != null && a == "fsepbillitemid")
			{
				DynamicObjectCollection source = base.View.Model.DataObject["STK_CYCLECOUNTTABLESEPB"] as DynamicObjectCollection;
				DynamicObject dynamicObject = (from d in source
				where d["SepBillItemId"].ToString() == e.Value.ToString()
				select d).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					base.View.ShowMessage(ResManager.LoadKDString("不允许添加重复的规则选项！", "004023030002095", 5, new object[0]), 0);
					e.Cancel = true;
					base.View.UpdateView("FSepBillItemId", e.Row);
				}
			}
			foreach (Field field in this.fieldList)
			{
				if (field.Key.ToUpperInvariant().Equals(e.Key.ToUpperInvariant()))
				{
					this.EidtRow(e.Row);
					break;
				}
			}
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x00036758 File Offset: 0x00034958
		public override void DataChanged(DataChangedEventArgs e)
		{
			if (e.Field.Key.ToUpperInvariant() == "FSEPBILLITEMID")
			{
				object newValue = e.NewValue;
				DynamicObject dynamicObject = (from d in this.dycSepBillItem
				where d["Value"].ToString() == e.NewValue.ToString()
				select d).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					base.View.Model.SetValue("FSrcFieldName", dynamicObject["FSRCFIELDNAME"], e.Row);
					base.View.Model.SetValue("FTableName", dynamicObject["FTableName"], e.Row);
					base.View.Model.SetValue("FTableRelation", dynamicObject["FTABLERELATION"], e.Row);
				}
			}
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x00036850 File Offset: 0x00034A50
		public override void AfterBindData(EventArgs e)
		{
			if (this.dycSepBillItem == null)
			{
				this.dycSepBillItem = StockServiceHelper.GetCountSepBillItem(base.Context, "Cycle");
			}
			this.InitiaSepBillItem(this.dycSepBillItem);
			this.GetDateByPage(this.currentPage);
			this.SetPanelVisible(this.visble);
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x000368A0 File Offset: 0x00034AA0
		public override void BeforeSubmit(BeforeSubmitEventArgs e)
		{
			e.Cancel = this.VaildateData();
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x000368AE File Offset: 0x00034AAE
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			e.Cancel = this.VaildateData();
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x000368BC File Offset: 0x00034ABC
		private bool VaildateData()
		{
			this.GetEntityData();
			if (!this.GetDateByPage(this.currentPage))
			{
				return true;
			}
			OperateResultCollection operateResultCollection = CycleCountTablePageService.ValidateEntity(base.Context, this.tmpTabelName);
			if (operateResultCollection.Count > 0)
			{
				base.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
				return true;
			}
			return false;
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x0003690F File Offset: 0x00034B0F
		public override void AfterSave(AfterSaveEventArgs e)
		{
			if (e.OperationResult.IsSuccess)
			{
				CycleCountTablePageService.ConvertDataFormTmpTable(base.Context, this.tmpTabelName, this.billID);
			}
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00036938 File Offset: 0x00034B38
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (e.Entity.Key.Equals("FAssignBillEntity"))
			{
				if (this.dycSepBillItem == null)
				{
					this.dycSepBillItem = StockServiceHelper.GetCountSepBillItem(base.Context, "Cycle");
				}
				this.InitiaSepBillItem(this.dycSepBillItem);
				return;
			}
			if (e.Entity.Key.Equals("FEntity"))
			{
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(e.Entity, e.Row - 1);
				if (entityDataObject != null)
				{
					int num = Convert.ToInt32(entityDataObject["Seq"]);
					entityDataObject = this.Model.GetEntityDataObject(e.Entity, e.Row);
					entityDataObject["Seq"] = num + 1;
				}
			}
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x000369FC File Offset: 0x00034BFC
		public override void Dispose()
		{
			if (!this._disposed)
			{
				if (!string.IsNullOrWhiteSpace(this.tmpTabelName))
				{
					DBServiceHelper.DeleteTemporaryTableName(base.Context, new string[]
					{
						this.tmpTabelName
					});
				}
				this._disposed = true;
			}
			base.Dispose();
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x00036A48 File Offset: 0x00034C48
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
					if (Convert.ToString(dyc[i]["FTableName"]) == "T_BD_FLEXSITEMDETAILV")
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

		// Token: 0x06000498 RID: 1176 RVA: 0x00036BD4 File Offset: 0x00034DD4
		private void QueryCycleCountInput()
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
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.CustomParams.Add("source", "2");
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			listShowParameter.CustomParams.Add("CountTableIds", this.Model.DataObject["Id"].ToString());
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00036CE4 File Offset: 0x00034EE4
		private void QuerySrcleBill(string formID)
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
			list.Add(new SelectorItemInfo("FPlanId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_CycleCountTable",
				FilterClauseWihtKey = string.Format("FID = {0} ", num),
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
				array[i] = Convert.ToInt64(dynamicObjectCollection[i]["FPlanId"]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formID;
			listShowParameter.UseOrgId = Convert.ToInt64(this.Model.DataObject["StockOrgId_ID"]);
			listShowParameter.MutilListUseOrgId = this.Model.DataObject["StockOrgId_ID"].ToString();
			Common.SetFormOpenStyle(base.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", array));
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x00036E9C File Offset: 0x0003509C
		private bool GetDateByPage(int currentPage)
		{
			DynamicObjectCollection entityData = this.GetEntityData();
			if (!this.SaveCurrentPgData(entityData))
			{
				return false;
			}
			entityData.Clear();
			DynamicObjectCollection dgetMaterialByPage = CycleCountTablePageService.GetDGetMaterialByPage(base.Context, this.tmpTabelName, this.billID, this.pageSize, ref currentPage, ref this.pageCount);
			this.currentPage = currentPage;
			this.FillEnityObject(entityData, dgetMaterialByPage);
			return true;
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00036EF8 File Offset: 0x000350F8
		private void FillEnityObject(DynamicObjectCollection subObjList, DynamicObjectCollection pgData)
		{
			subObjList.Clear();
			DynamicObjectType dynamicCollectionItemPropertyType = subObjList.DynamicCollectionItemPropertyType;
			if (pgData != null)
			{
				foreach (DynamicObject dynamicObject in pgData)
				{
					DynamicObject dynamicObject2 = new DynamicObject(dynamicCollectionItemPropertyType);
					subObjList.Add(dynamicObject2);
					dynamicObject2["Flag"] = dynamicObject["FFlag"];
					dynamicObject2["TempID"] = dynamicObject["FTempID"];
					dynamicObject2["ID"] = dynamicObject["FEntryId"];
					dynamicObject2["Seq"] = dynamicObject["FRowNum"];
					foreach (Field field in this.fieldList)
					{
						if (field is BaseDataField)
						{
							dynamicObject2[field.PropertyName + "_Id"] = dynamicObject[field.Key];
							dynamicObject2[field.PropertyName] = MaterialABCGroupService.LoadReferenceData(base.Context, (field as BaseDataField).RefFormDynamicObjectType, dynamicObject[field.Key]);
						}
						else if (field is RelatedFlexGroupField)
						{
							dynamicObject2[field.PropertyName + "_Id"] = dynamicObject[field.Key];
							dynamicObject2[field.PropertyName] = MaterialABCGroupService.LoadReferenceData(base.Context, (field as RelatedFlexGroupField).RefFormDynamicObjectType, dynamicObject[field.Key]);
						}
						else
						{
							dynamicObject2[field.PropertyName] = dynamicObject[field.Key];
						}
					}
					dynamicObject2.DataEntityState.SetDirty(false);
				}
			}
			base.View.UpdateView(this.entityKey);
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00037100 File Offset: 0x00035300
		private void EidtRow(int rowIndex)
		{
			Entity entryEntity = base.View.BusinessInfo.GetEntryEntity(this.entityKey);
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, rowIndex);
			string text = entityDataObject["Flag"].ToString();
			if (string.IsNullOrWhiteSpace(text) || text.Equals("0"))
			{
				base.View.Model.SetValue("FFlag", 3, rowIndex);
			}
			entityDataObject.DataEntityState.SetDirty(true);
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00037180 File Offset: 0x00035380
		private void DeleteRow()
		{
			DynamicObject dynamicObject;
			int num;
			if (!this.Model.TryGetEntryCurrentRow(this.entityKey, ref dynamicObject, ref num))
			{
				return;
			}
			if (dynamicObject["TempID"] != null && Convert.ToInt64(dynamicObject["TempID"]) > 0L)
			{
				base.View.Model.SetValue("FFlag", 2, num);
				dynamicObject.DataEntityState.SetDirty(true);
				List<DynamicObject> list = new List<DynamicObject>();
				list.Add(dynamicObject);
				CycleCountTablePageService.SavePageData(base.Context, this.tmpTabelName, this.billID, list);
			}
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00037260 File Offset: 0x00035460
		private bool SaveCurrentPgData(DynamicObjectCollection subObjList)
		{
			List<string> list = new List<string>();
			foreach (DynamicObject dynamicObject in subObjList)
			{
				if (dynamicObject["MaterialId"] != null)
				{
					DateTime minValue = DateTime.MinValue;
					object obj = dynamicObject["PlanDate"];
					if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
					{
						DateTime.TryParse(obj.ToString(), out minValue);
						if (minValue != DateTime.MinValue)
						{
							continue;
						}
					}
					if (list.Count >= 20)
					{
						list.Add("...");
						break;
					}
					list.Add(dynamicObject["Seq"].ToString());
				}
			}
			if (list.Count > 0)
			{
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("计划盘点日期不允许为空，分录[{0}]计划盘点日期未录入。", "004023000013416", 5, new object[0]), string.Join(",", list)), "", 0);
				return false;
			}
			List<DynamicObject> list2 = (from p in subObjList
			where p["MaterialId"] != null && p["Flag"] != null && !string.IsNullOrWhiteSpace(p["Flag"].ToString()) && p.DataEntityState.DataEntityDirty
			orderby p["Seq"]
			select p).ToList<DynamicObject>();
			if (list2.Count > 0)
			{
				CycleCountTablePageService.SavePageData(base.Context, this.tmpTabelName, this.billID, list2);
			}
			return true;
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x000373E0 File Offset: 0x000355E0
		private DynamicObjectCollection GetEntityData()
		{
			Entity entryEntity = base.View.BusinessInfo.GetEntryEntity(this.entityKey);
			return base.View.Model.GetEntityDataObject(entryEntity);
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x00037417 File Offset: 0x00035617
		private void SetPanelVisible(bool visble)
		{
			base.View.GetControl<Panel>("FSearchPanel").Visible = visble;
			this.visble = visble;
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00037438 File Offset: 0x00035638
		private void SeachFun()
		{
			string text = "";
			string text2 = "";
			object value = this.Model.GetValue("FSreachType");
			if (value != null)
			{
				text = value.ToString();
			}
			object value2 = this.Model.GetValue("FSreachTxt");
			if (value2 != null)
			{
				text2 = value2.ToString();
			}
			DynamicObjectCollection pgData = CycleCountTablePageService.SreachFun(base.Context, this.tmpTabelName, text, text2);
			this.currentPage = 1;
			DynamicObjectCollection entityData = this.GetEntityData();
			if (!this.SaveCurrentPgData(entityData))
			{
				return;
			}
			entityData.Clear();
			this.FillEnityObject(entityData, pgData);
		}

		// Token: 0x040001AA RID: 426
		private DynamicObjectCollection dycSepBillItem;

		// Token: 0x040001AB RID: 427
		private bool _disposed;

		// Token: 0x040001AC RID: 428
		private string entityKey = "FEntity";

		// Token: 0x040001AD RID: 429
		private string tmpTabelName = string.Empty;

		// Token: 0x040001AE RID: 430
		private long billID;

		// Token: 0x040001AF RID: 431
		private List<Field> fieldList = new List<Field>();

		// Token: 0x040001B0 RID: 432
		private bool visble;

		// Token: 0x040001B1 RID: 433
		private int pageSize = 500;

		// Token: 0x040001B2 RID: 434
		private int pageCount;

		// Token: 0x040001B3 RID: 435
		private int currentPage = 1;

		// Token: 0x0200006A RID: 106
		private enum RowFlag_ENUM
		{
			// Token: 0x040001B8 RID: 440
			NEWROW = 1,
			// Token: 0x040001B9 RID: 441
			DELROW,
			// Token: 0x040001BA RID: 442
			EDITROW
		}
	}
}
