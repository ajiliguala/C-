using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Base.PlugIn;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000075 RID: 117
	public class MaterialABCGroupEdit : AbstractBasePlugIn
	{
		// Token: 0x06000545 RID: 1349 RVA: 0x000407C8 File Offset: 0x0003E9C8
		public override void OnInitialize(InitializeEventArgs e)
		{
			BillOpenParameter billOpenParameter = (BillOpenParameter)e.Paramter;
			this.status = billOpenParameter.Status;
			this.billID = ((billOpenParameter.PkValue == null) ? 0L : Convert.ToInt64(billOpenParameter.PkValue));
			this.tmpTabelName = MaterialABCGroupService.CreatePgTmpTable(base.Context);
			base.View.GetControl<EntryGrid>("FMATERIALABCGROUPENTRY").SetFireDoubleClickEvent(true);
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x00040834 File Offset: 0x0003EA34
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			DynamicObject dataObject = this.Model.DataObject;
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dataObject["MaterialABCGroupEntry"];
			dynamicObjectCollection.Clear();
			Entity entity = base.View.BusinessInfo.GetEntity("FMaterialABCGroupEntry");
			DynamicObject dynamicObject = new DynamicObject(entity.DynamicObjectType);
			dynamicObject["Seq"] = 1;
			dynamicObject["GroupNo"] = "01";
			dynamicObject["GroupName"] = new LocaleValue(ResManager.LoadKDString("A类", "004023030006495", 5, new object[0]), base.View.Context.UserLocale.LCID);
			dynamicObjectCollection.Add(dynamicObject);
			dynamicObject = new DynamicObject(entity.DynamicObjectType);
			dynamicObject["Seq"] = 2;
			dynamicObject["GroupNo"] = "02";
			dynamicObject["GroupName"] = new LocaleValue(ResManager.LoadKDString("B类", "004023030006496", 5, new object[0]), base.View.Context.UserLocale.LCID);
			dynamicObjectCollection.Add(dynamicObject);
			dynamicObject = new DynamicObject(entity.DynamicObjectType);
			dynamicObject["Seq"] = 3;
			dynamicObject["GroupNo"] = "03";
			dynamicObject["GroupName"] = new LocaleValue(ResManager.LoadKDString("C类", "004023030006497", 5, new object[0]), base.View.Context.UserLocale.LCID);
			dynamicObjectCollection.Add(dynamicObject);
			dynamicObject = new DynamicObject(entity.DynamicObjectType);
			dynamicObject["Seq"] = 4;
			dynamicObjectCollection.Add(dynamicObject);
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x000409F2 File Offset: 0x0003EBF2
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			if (!e.Key.ToUpperInvariant().Equals("FMATERIALABCGROUPENTRY"))
			{
				return;
			}
			this.ShowMaterial(false);
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x00040A14 File Offset: 0x0003EC14
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBLOOKMATERIAL")
				{
					this.ShowMaterial(false);
					return;
				}
				if (a == "TBEIDTMATERIAL")
				{
					this.ShowMaterial(true);
					return;
				}
				if (a == "TBDELETEROW")
				{
					this.DeleteEntityRow(e);
					return;
				}
				if (!(a == "TBSELABCRULE"))
				{
					return;
				}
				this.SelABCGroupRule();
			}
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x00040AB8 File Offset: 0x0003ECB8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (base.View.BusinessInfo.GetField(e.Key) == null)
			{
				return;
			}
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FGROUPNO"))
				{
					return;
				}
				Entity entity = base.View.BusinessInfo.GetField(e.Key).Entity;
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
				if ((from p in entityDataObject
				where p["GROUPNO"] != null && p["GROUPNO"].Equals(e.Value)
				select p).Count<DynamicObject>() > 0)
				{
					base.View.UpdateView(e.Key, e.Row);
					base.View.ShowWarnningMessage(ResManager.LoadKDString("分组编码重复，请重新录入。", "004023030002209", 5, new object[0]), "", 0, null, 1);
					e.Cancel = true;
				}
				this.currentGroupNo = e.Value.ToString();
			}
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x00040BD8 File Offset: 0x0003EDD8
		public override void AfterSave(AfterSaveEventArgs e)
		{
			if (e.OperationResult.IsSuccess)
			{
				this.billID = Convert.ToInt64(this.Model.DataObject["Id"]);
				MaterialABCGroupService.ConvertDataFormTmpTable(base.Context, this.tmpTabelName, this.billID);
			}
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x00040C2C File Offset: 0x0003EE2C
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "DRAFT"))
				{
					if (a == "PREVIOUS" || a == "NEXT")
					{
						this.billID = Convert.ToInt64(base.View.OpenParameter.PkValue);
						this.tmpTabelName = MaterialABCGroupService.CreatePgTmpTable(base.Context);
						return;
					}
					if (!(a == "SUBMIT"))
					{
						return;
					}
					if (e.OperationResult.IsSuccess)
					{
						this.billID = Convert.ToInt64(this.Model.DataObject["Id"]);
						MaterialABCGroupService.ConvertDataFormTmpTable(base.Context, this.tmpTabelName, this.billID);
					}
				}
				else if (e.OperationResult.IsSuccess)
				{
					this.billID = Convert.ToInt64(this.Model.DataObject["Id"]);
					MaterialABCGroupService.ConvertDataFormTmpTable(base.Context, this.tmpTabelName, this.billID);
					return;
				}
			}
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x00040D40 File Offset: 0x0003EF40
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBSPLITNEW") && !(a == "TBNEW"))
				{
					return;
				}
				DBServiceHelper.DeleteTemporaryTableName(base.Context, new string[]
				{
					this.tmpTabelName
				});
				this.tmpTabelName = MaterialABCGroupService.CreatePgTmpTable(base.Context);
				this.billID = 0L;
			}
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x00040DB0 File Offset: 0x0003EFB0
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

		// Token: 0x0600054E RID: 1358 RVA: 0x00040DFC File Offset: 0x0003EFFC
		private void DeleteEntityRow(BarItemClickEventArgs e)
		{
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex(e.ParentKey);
			Entity entryEntity = base.View.BusinessInfo.GetEntryEntity(e.ParentKey);
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
			if (entityDataObject != null && entityDataObject["GroupNo"] != null && !string.IsNullOrWhiteSpace(entityDataObject["GroupNo"].ToString()))
			{
				MaterialABCGroupService.DeleteGroupByNo(base.Context, this.tmpTabelName, entityDataObject["GroupNo"].ToString());
			}
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x00040E88 File Offset: 0x0003F088
		private void SetCurrentGroupNo(int row)
		{
			object value = this.Model.GetValue("FGROUPNO", row);
			this.groupRow = row;
			this.currentGroupNo = ((value == null) ? "" : value.ToString());
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x00040EC4 File Offset: 0x0003F0C4
		protected void SelABCGroupRule()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FCreateOrgId") as DynamicObject;
			string value = (dynamicObject != null) ? Convert.ToString(dynamicObject["Id"]) : "";
			string value2 = (dynamicObject != null) ? Convert.ToString(dynamicObject["OrgFunctions"]) : "";
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "STK_ABCGroupingFilter";
			dynamicFormShowParameter.CustomParams.Add("CreateOrgId", value);
			dynamicFormShowParameter.CustomParams.Add("OrgFunctions", value2);
			base.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.GetABCGroupRule));
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x00040F80 File Offset: 0x0003F180
		protected void GetABCGroupRule(FormResult result)
		{
			FilterParameter filterParameter = (FilterParameter)result.ReturnData;
			if (filterParameter != null)
			{
				List<string> materialABCGroup = StockServiceHelper.GetMaterialABCGroup(base.Context, filterParameter, this.tmpTabelName);
				base.View.Model.DeleteEntryData("FMATERIALABCGROUPENTRY");
				DynamicObjectCollection source = filterParameter.CustomFilter["ABCEntityFilter"] as DynamicObjectCollection;
				List<DynamicObject> list = (from p in source
				where p["GroupNumber"] != null
				select p).ToList<DynamicObject>();
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					int num = Convert.ToInt32(list[i]["Seq"]);
					string text = Convert.ToString(list[i]["GroupNumber"]);
					string text2 = Convert.ToString(list[i]["GroupName"]);
					base.View.Model.CreateNewEntryRow("FMATERIALABCGROUPENTRY");
					base.View.Model.SetValue("FSeq", num, i);
					base.View.Model.SetValue("FGroupNo", text, i);
					base.View.Model.SetValue("FGroupName", text2, i);
					base.View.Model.SetValue("FExistsMetaril", materialABCGroup.Contains(list[i]["GroupNumber"].ToString()), i);
				}
				base.View.UpdateView("FMATERIALABCGROUPENTRY");
			}
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x00041120 File Offset: 0x0003F320
		private void ShowMaterial(bool isEidt)
		{
			DynamicObject dynamicObject = this.Model.GetValue("FCreateOrgId") as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) < 1L)
			{
				base.View.ShowWarnningMessage(ResManager.LoadKDString("使用还未录入!", "004023030005533", 5, new object[0]), "", 0, null, 1);
				return;
			}
			this.SetCurrentGroupNo(base.View.Model.GetEntryCurrentRowIndex("FMATERIALABCGROUPENTRY"));
			if (string.IsNullOrWhiteSpace(this.currentGroupNo))
			{
				base.View.ShowWarnningMessage(ResManager.LoadKDString("分组编码还未录入!", "004023030002212", 5, new object[0]), "", 0, null, 1);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.FormId = "STK_ABCGroupMaterial";
			dynamicFormShowParameter.CustomParams.Add("currentGroupNo", this.currentGroupNo);
			dynamicFormShowParameter.CustomParams.Add("tmpTabelName", this.tmpTabelName);
			dynamicFormShowParameter.CustomParams.Add("billID", this.billID.ToString());
			dynamicFormShowParameter.CustomParams.Add("isEidt", isEidt.ToString());
			dynamicFormShowParameter.CustomParams.Add("UseOrgId", dynamicObject["Id"].ToString());
			base.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.CloseMaterial));
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x0004129C File Offset: 0x0003F49C
		protected void CloseMaterial(FormResult result)
		{
			if (base.View == null || base.View.Model == null)
			{
				return;
			}
			if (result != null && result.ReturnData != null && result.ReturnData.ToString() != null)
			{
				bool flag = false;
				bool.TryParse(result.ReturnData.ToString(), out flag);
				int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FMATERIALABCGROUPENTRY");
				base.View.Model.SetValue("FExistsMetaril", flag, entryCurrentRowIndex);
				return;
			}
			int entryCurrentRowIndex2 = this.Model.GetEntryCurrentRowIndex("FMATERIALABCGROUPENTRY");
			base.View.Model.SetValue("FExistsMetaril", false, entryCurrentRowIndex2);
		}

		// Token: 0x040001FE RID: 510
		private const string KEY_GroupEntity = "FMATERIALABCGROUPENTRY";

		// Token: 0x040001FF RID: 511
		private bool _disposed;

		// Token: 0x04000200 RID: 512
		private int groupRow;

		// Token: 0x04000201 RID: 513
		private string currentGroupNo;

		// Token: 0x04000202 RID: 514
		private long billID;

		// Token: 0x04000203 RID: 515
		private OperationStatus status;

		// Token: 0x04000204 RID: 516
		private string tmpTabelName = string.Empty;
	}
}
