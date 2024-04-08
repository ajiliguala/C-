using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200003F RID: 63
	public class AbcGroupMaterialEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600025A RID: 602 RVA: 0x0001C84C File Offset: 0x0001AA4C
		public override void OnInitialize(InitializeEventArgs e)
		{
			if (e.Paramter.GetCustomParameter("currentGroupNo") != null)
			{
				this.currentGroupNo = e.Paramter.GetCustomParameter("currentGroupNo").ToString();
			}
			else
			{
				this.View.Close();
			}
			if (e.Paramter.GetCustomParameter("tmpTabelName") != null)
			{
				this.tmpTabelName = e.Paramter.GetCustomParameter("tmpTabelName").ToString();
			}
			if (e.Paramter.GetCustomParameter("billID") != null)
			{
				this.billID = Convert.ToInt64(e.Paramter.GetCustomParameter("billID"));
			}
			if (e.Paramter.GetCustomParameter("isEidt") != null)
			{
				this.isEidt = Convert.ToBoolean(e.Paramter.GetCustomParameter("isEidt"));
			}
			if (e.Paramter.GetCustomParameter("UseOrgId") != null)
			{
				this.useOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("UseOrgId"));
			}
			this.document = this.View.ParentFormView.Model.GetValue("FDocumentStatus");
			this.forbid = this.View.ParentFormView.Model.GetValue("FForbidStatus");
		}

		// Token: 0x0600025B RID: 603 RVA: 0x0001C987 File Offset: 0x0001AB87
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.Model.SetItemValueByID("FUseOrgId", this.useOrgId, 0);
		}

		// Token: 0x0600025C RID: 604 RVA: 0x0001C9AC File Offset: 0x0001ABAC
		public override void AfterBindData(EventArgs e)
		{
			if (!this.isEidt || (this.document != null && this.document.ToString().Equals("C")) || (this.forbid != null && this.forbid.ToString().Equals("B")))
			{
				this.View.GetMainBarItem("tbAddSUBRow").Visible = false;
				this.View.GetMainBarItem("tbDeleteSUBRow").Visible = false;
				this.View.GetMainBarItem("tbInsertRow").Visible = false;
				this.View.GetMainBarItem("tbAccessoryEntry").Visible = false;
				this.View.GetControl("FMaterialABCGroupDetail").Enabled = false;
				this.isEidt = false;
			}
			this.SetPanelVisible(this.visble);
			this.GetPageData(1);
		}

		// Token: 0x0600025D RID: 605 RVA: 0x0001CA8C File Offset: 0x0001AC8C
		public override void AfterButtonClick(AfterButtonClickEventArgs e)
		{
			if (e.Key.ToUpperInvariant().Equals("FTBSEARCH"))
			{
				DynamicObjectCollection entityData = this.GetEntityData();
				this.SaveCurrentPgData(this.currentGroupNo, entityData);
				string text = Convert.ToString(this.Model.GetValue("FMID"));
				string text2 = Convert.ToString(this.Model.GetValue("FMName"));
				string text3 = Convert.ToString(this.Model.GetValue("FMMode"));
				DynamicObjectCollection pgData = MaterialABCGroupService.SearchMaterial(base.Context, this.tmpTabelName, this.currentGroupNo, text, text2, text3);
				this.FillEnityObject(entityData, pgData);
			}
		}

		// Token: 0x0600025E RID: 606 RVA: 0x0001CB2C File Offset: 0x0001AD2C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBPAGEUP")
				{
					this.GetPageData(this.currentPage - 1);
					return;
				}
				if (a == "TBPAGEDOWN")
				{
					this.GetPageData(this.currentPage + 1);
					return;
				}
				if (a == "TBDELETESUBROW")
				{
					this.DelRow();
					return;
				}
				if (!(a == "TBSHOWPANLE"))
				{
					return;
				}
				this.SetPanelVisible(!this.visble);
			}
		}

		// Token: 0x0600025F RID: 607 RVA: 0x0001CBB4 File Offset: 0x0001ADB4
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (a == "FMATERIALID")
				{
					this.CheckMaterialId(e);
					return;
				}
				if (!(a == "FMUSTSELECT") && !(a == "FISCYCLECOUNT") && !(a == "FMATERIALNOTE"))
				{
					return;
				}
				this.EidtRow(e.Row);
			}
		}

		// Token: 0x06000260 RID: 608 RVA: 0x0001CC1C File Offset: 0x0001AE1C
		public override void DataUpdateEnd()
		{
			base.DataUpdateEnd();
			if (this.lDelIndex != null && this.lDelIndex.Count > 0)
			{
				this.lDelIndex.Sort();
				int num = 0;
				foreach (int num2 in this.lDelIndex)
				{
					this.Model.DeleteEntryRow("FMaterialABCGroupDetail", num2 - num);
					num++;
				}
				this.lDelIndex.Clear();
			}
		}

		// Token: 0x06000261 RID: 609 RVA: 0x0001CCB4 File Offset: 0x0001AEB4
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			DynamicObjectCollection entityData = this.GetEntityData();
			this.SaveCurrentPgData(this.currentGroupNo, entityData);
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0001CCD8 File Offset: 0x0001AED8
		private DynamicObjectCollection GetEntityData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FMaterialABCGroupDetail");
			return this.View.Model.GetEntityDataObject(entryEntity);
		}

		// Token: 0x06000263 RID: 611 RVA: 0x0001CD10 File Offset: 0x0001AF10
		private void GetPageData(int currentPage)
		{
			if (string.IsNullOrWhiteSpace(this.currentGroupNo))
			{
				return;
			}
			DynamicObjectCollection entityData = this.GetEntityData();
			this.SaveCurrentPgData(this.currentGroupNo, entityData);
			DynamicObjectCollection materialByPage = MaterialABCGroupService.GetMaterialByPage(base.Context, this.tmpTabelName, this.billID, this.currentGroupNo, this.pageSize, ref currentPage, ref this.pageCount);
			this.currentPage = currentPage;
			this.FillEnityObject(entityData, materialByPage);
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0001CD7C File Offset: 0x0001AF7C
		private void FillEnityObject(DynamicObjectCollection subObjList, DynamicObjectCollection pgData)
		{
			DynamicObjectType dynamicCollectionItemPropertyType = subObjList.DynamicCollectionItemPropertyType;
			BaseDataField baseDataField = (BaseDataField)this.Model.BusinessInfo.GetField("FMaterialId");
			subObjList.Clear();
			if (pgData != null)
			{
				foreach (DynamicObject dynamicObject in pgData)
				{
					DynamicObject dynamicObject2 = new DynamicObject(dynamicCollectionItemPropertyType);
					subObjList.Add(dynamicObject2);
					dynamicObject2["MaterialId_Id"] = dynamicObject["MaterialId"];
					dynamicObject2["MaterialId"] = MaterialABCGroupService.LoadReferenceData(base.Context, baseDataField.RefFormDynamicObjectType, dynamicObject["MaterialId"]);
					dynamicObject2["DETAILID"] = dynamicObject["DETAILID"];
					dynamicObject2["Seq"] = dynamicObject["Seq"];
					dynamicObject2["MustSelect"] = (Convert.ToInt32(dynamicObject["MustSelect"]) == 1);
					dynamicObject2["IsCycleCount"] = (Convert.ToInt32(dynamicObject["IsCycleCount"]) == 1);
					dynamicObject2["MaterialNote"] = dynamicObject["MaterialNote"];
					dynamicObject2["Flag"] = dynamicObject["Flag"];
					dynamicObject2["TempID"] = dynamicObject["TmpID"];
					dynamicObject2.DataEntityState.SetDirty(false);
				}
			}
			if (subObjList.Count == 0)
			{
				DynamicObject dynamicObject3 = new DynamicObject(dynamicCollectionItemPropertyType);
				dynamicObject3["Flag"] = 1;
				dynamicObject3["Seq"] = 1;
				dynamicObject3.DataEntityState.SetDirty(false);
				subObjList.Add(dynamicObject3);
			}
			this.View.UpdateView("FMaterialABCGroupDetail");
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0001CFC8 File Offset: 0x0001B1C8
		private void SaveCurrentPgData(string groupNo, DynamicObjectCollection subObjList)
		{
			List<DynamicObject> list = (from p in subObjList
			where p["MaterialId"] != null && p["Flag"] != null && !string.IsNullOrWhiteSpace(p["Flag"].ToString()) && p.DataEntityState.DataEntityDirty
			orderby p["Seq"]
			select p).ToList<DynamicObject>();
			if (list.Count > 0 && this.isEidt)
			{
				MaterialABCGroupService.SavePageData(base.Context, this.tmpTabelName, groupNo, list);
			}
			if ((from p in subObjList
			where p["MaterialId"] != null
			select p).Count<DynamicObject>() > 0)
			{
				this.View.ReturnToParentWindow(true);
				return;
			}
			this.View.ReturnToParentWindow(false);
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0001D0D8 File Offset: 0x0001B2D8
		private void CheckMaterialId(BeforeUpdateValueEventArgs e)
		{
			DynamicObjectCollection entityData = this.GetEntityData();
			long materialId = (e.Value is DynamicObject) ? Convert.ToInt64(((DynamicObject)e.Value)["Id"]) : Convert.ToInt64(e.Value);
			bool flag = true;
			if ((from p in entityData
			where p["MaterialId"] != null && Convert.ToInt64(p["MaterialId_Id"]).Equals(materialId)
			select p).Count<DynamicObject>() > 0)
			{
				flag = false;
				this.View.ShowWarnningMessage(ResManager.LoadKDString("物料编码重复，请重新录入。", "004023030002047", 5, new object[0]), "", 0, null, 1);
			}
			if (MaterialABCGroupService.IsExistMaterialInTb(base.Context, this.tmpTabelName, this.billID, materialId))
			{
				flag = false;
				this.View.ShowWarnningMessage(ResManager.LoadKDString("物料编码重复，请重新录入。", "004023030002047", 5, new object[0]), "", 0, null, 1);
			}
			if (flag)
			{
				this.EidtRow(e.Row);
				return;
			}
			this.View.UpdateView(e.Key, e.Row);
			this.lDelIndex.Add(e.Row);
			e.Cancel = true;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0001D1FC File Offset: 0x0001B3FC
		private void EidtRow(int rowIndex)
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FMaterialABCGroupDetail");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, rowIndex);
			string text = entityDataObject["Flag"].ToString();
			if (string.IsNullOrWhiteSpace(text) || text.Equals("0"))
			{
				this.View.Model.SetValue("FFlag", 3, rowIndex);
			}
			entityDataObject.DataEntityState.SetDirty(true);
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0001D27C File Offset: 0x0001B47C
		private void DelRow()
		{
			DynamicObject dynamicObject;
			int num;
			if (!this.Model.TryGetEntryCurrentRow("FMaterialABCGroupDetail", ref dynamicObject, ref num))
			{
				return;
			}
			if (dynamicObject["TempID"] != null && Convert.ToInt64(dynamicObject["TempID"]) > 0L)
			{
				this.View.Model.SetValue("FFlag", 2, num);
				dynamicObject.DataEntityState.SetDirty(true);
				List<DynamicObject> list = new List<DynamicObject>();
				list.Add(dynamicObject);
				MaterialABCGroupService.SavePageData(base.Context, this.tmpTabelName, this.currentGroupNo, list);
			}
			this.Model.DeleteEntryRow("FMaterialABCGroupDetail", num);
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0001D31F File Offset: 0x0001B51F
		private void SetPanelVisible(bool visble)
		{
			this.View.GetControl<Panel>("FSPanel").Visible = visble;
			this.visble = visble;
		}

		// Token: 0x040000CF RID: 207
		private const string KEY_MaterialEntity = "FMaterialABCGroupDetail";

		// Token: 0x040000D0 RID: 208
		private string tmpTabelName;

		// Token: 0x040000D1 RID: 209
		private string currentGroupNo;

		// Token: 0x040000D2 RID: 210
		private long billID;

		// Token: 0x040000D3 RID: 211
		private long useOrgId;

		// Token: 0x040000D4 RID: 212
		private int pageSize = 200;

		// Token: 0x040000D5 RID: 213
		private int pageCount;

		// Token: 0x040000D6 RID: 214
		private int currentPage = 1;

		// Token: 0x040000D7 RID: 215
		private bool visble;

		// Token: 0x040000D8 RID: 216
		private object document;

		// Token: 0x040000D9 RID: 217
		private object forbid;

		// Token: 0x040000DA RID: 218
		private bool isEidt;

		// Token: 0x040000DB RID: 219
		private List<int> lDelIndex = new List<int>();

		// Token: 0x02000040 RID: 64
		private enum RowFlag_ENUM
		{
			// Token: 0x040000E0 RID: 224
			NEWROW = 1,
			// Token: 0x040000E1 RID: 225
			DELROW,
			// Token: 0x040000E2 RID: 226
			EDITROW
		}
	}
}
