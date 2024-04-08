using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.FIN.HS;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000041 RID: 65
	public class AssembledAppEdit : AbstractBillPlugIn
	{
		// Token: 0x0600026E RID: 622 RVA: 0x0001D364 File Offset: 0x0001B564
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			base.OnBillInitialize(e);
			TextField textField = base.View.BusinessInfo.GetField("FDescriptionSETY") as TextField;
			this._subMemLen = textField.Editlen;
			this.GetSubPickInvService();
			if (base.View.OpenParameter.Status == null)
			{
				this.hasPara = false;
				return;
			}
			this.hasPara = this.GetBillOpenParameter(out this.hsorgId, out this.hsacctsysid, out this.hsacctPolicyId);
			if (this.hasPara)
			{
				base.View.OpenParameter.Status = 1;
			}
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0001D3F6 File Offset: 0x0001B5F6
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.UpdateBillCostInfo();
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0001D408 File Offset: 0x0001B608
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (e.Operation.OperationId == FormOperation.Operation_Draw)
			{
				for (int i = base.View.Model.GetEntryRowCount("FEntity") - 1; i >= 0; i--)
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
					if (num == 0L)
					{
						base.View.Model.DeleteEntryRow("FEntity", i);
					}
				}
			}
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0001D4A0 File Offset: 0x0001B6A0
		public override void AfterCreateModelData(EventArgs e)
		{
			this.hasPara = false;
			this.SetDefLocalCurrency();
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
				if (baseDataLongValue > 0L)
				{
					SCMCommon.SetOpertorIdByUserId(this, "FSTOCKERID", "WHY", baseDataLongValue);
				}
			}
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0001D502 File Offset: 0x0001B702
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			if (!this.ClearZeroRow())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0001D51C File Offset: 0x0001B71C
		private bool CheckSubMaterial(DynamicObject data, DynamicObject material, string affAirType)
		{
			string arg = Convert.ToString(material["Number"]);
			string arg2 = Convert.ToString(material["Name"]);
			bool allowSubSameAsPSetting = this.GetAllowSubSameAsPSetting();
			DynamicObjectCollection dynamicObjectCollection = data["STK_ASSEMBLYSUBITEM"] as DynamicObjectCollection;
			if (dynamicObjectCollection.Count<DynamicObject>() < 1)
			{
				string value = string.Format(ResManager.LoadKDString("第{0}行物料{1}-{2}不存在子项行记录，保存不成功!", "004023030000178", 5, new object[0]), data["Seq"], arg, arg2);
				this._ErrMsg.Add(value);
				return true;
			}
			long num = Convert.ToInt64(material["Id"]);
			decimal num2 = 0m;
			bool result = false;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject["MaterialIDSETY"] == null)
				{
					result = true;
					string value2 = string.Format(ResManager.LoadKDString("第{0}行物料{1}-{2}子项物料中存在物料为空的记录，保存不成功!", "004023030000181", 5, new object[0]), data["Seq"], arg, arg2);
					this._ErrMsg.Add(value2);
					break;
				}
				DynamicObject dynamicObject2 = dynamicObject["MaterialIDSETY"] as DynamicObject;
				if (Convert.ToInt64(dynamicObject2["Id"]) == num && !allowSubSameAsPSetting)
				{
					result = true;
					string value3 = string.Format(ResManager.LoadKDString("第{0}行物料{1}-{2}在子项存在相同物料，保存不成功!", "004023030000187", 5, new object[0]), data["Seq"], arg, arg2);
					this._ErrMsg.Add(value3);
					break;
				}
				num2 += Convert.ToDecimal(dynamicObject["FCostProportion"]);
			}
			int num3 = (int)num2;
			if (affAirType.Equals("DASSEMBLY") && num3 != 0 && num3 != 100)
			{
				result = true;
				string value4 = string.Format(ResManager.LoadKDString("在拆卸事务类型下成品项第{0}行物料{1}-{2}物料所对应的所有子件的拆分百分比合计必须为0或者100。", "004023030000190", 5, new object[0]), data["Seq"], arg, arg2);
				this._ErrMsg.Add(value4);
			}
			return result;
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0001D730 File Offset: 0x0001B930
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBBOMEXPAND")
				{
					this.BomExpand();
					base.View.UpdateView("FEntity");
					base.View.UpdateView("FSubEntity");
					return;
				}
				if (!(a == "TBDELETEALLROW"))
				{
					return;
				}
				if (e.ParentKey.ToUpperInvariant() == "FENTITY")
				{
					Entity entity = this.Model.BusinessInfo.GetEntity("FEntity");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
					entityDataObject.Clear();
					base.View.UpdateView("FEntity");
					return;
				}
				this.Model.DeleteEntryData("FSerialEntity");
				this.Model.DeleteEntryData(e.ParentKey);
			}
		}

		// Token: 0x06000275 RID: 629 RVA: 0x0001D804 File Offset: 0x0001BA04
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			this.Model.BusinessInfo.GetEntity("FSubEntity");
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FMATERIALIDSETY"))
				{
					return;
				}
				if (!this.CheckProductInfo())
				{
					base.View.ShowMessage(this.BuildErrInfo(), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0001D8A8 File Offset: 0x0001BAA8
		public override void DataChanged(DataChangedEventArgs e)
		{
			long num = 0L;
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FMATERIALID":
			{
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(this.Model.GetValue("FKeeperTypeID", e.Row)), "BD_KeeperOrg"))
				{
					this.Model.SetValue("FKeeperID", dynamicObject, e.Row);
				}
				Entity entity = base.View.BusinessInfo.GetEntity("FEntity");
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, e.Row);
				DynamicObjectCollection subEntry = entityDataObject["STK_ASSEMBLYSUBITEM"] as DynamicObjectCollection;
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, true, num, true);
				if (dynamicObject2 == null || Convert.ToInt64(dynamicObject2["id"].ToString()) <= 0L)
				{
					base.View.Model.SetValue("FRefBomID", null, e.Row);
					base.View.Model.SetItemValueByID("FBOMID", null, e.Row);
					subEntry.Clear();
					base.View.UpdateView("FSubEntity");
					return;
				}
				if (SCMCommon.CheckMaterialIsEnableBom(dynamicObject2))
				{
					base.View.Model.SetItemValueByID("FBOMID", bomDefaultValueByMaterial, e.Row);
				}
				else
				{
					base.View.Model.SetItemValueByID("FBOMID", null, e.Row);
				}
				int num3 = 0;
				if (subEntry != null)
				{
					num3 = subEntry.Count((DynamicObject p) => p["MaterialIDSETY"] != null);
				}
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FRefBomID", e.Row) as DynamicObject;
				if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"].ToString()) > 0L && num3 > 0)
				{
					base.View.ShowMessage(ResManager.LoadKDString("是否清除子项物料！", "004023030000193", 5, new object[0]), 4, delegate(MessageBoxResult messageBoxResult)
					{
						if (messageBoxResult == 6)
						{
							subEntry.Clear();
							this.View.UpdateView("FSubEntity");
						}
					}, "", 0);
				}
				base.View.Model.SetItemValueByID("FRefBomID", bomDefaultValueByMaterial, e.Row);
				return;
			}
			case "FMATERIALIDSETY":
			{
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(this.Model.GetValue("FKeeperTypeIDSETY", e.Row)), "BD_KeeperOrg"))
				{
					this.Model.SetValue("FKeeperIDSETY", dynamicObject, e.Row);
				}
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FMATERIALIDSETY", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMIDSETY", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject4, 0L, true, num, false), e.Row);
				return;
			}
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
				return;
			case "FOWNERIDHEAD":
				this.SetDefLocalCurrency();
				return;
			case "FSTOCKID":
			{
				DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockID") as DynamicObject;
				SCMCommon.TakeDefaultStockStatusOther(this, "FStockStatusID", dynamicObject5, e.Row, "'0','7','8'");
				return;
			}
			case "FSTOCKIDSETY":
			{
				DynamicObject dynamicObject6 = base.View.Model.GetValue("FStockIDSETY") as DynamicObject;
				SCMCommon.TakeDefaultStockStatusOther(this, "FStockStatusIDSETY", dynamicObject6, e.Row, "'0','7','8'");
				return;
			}
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				return;
			}
			case "FAUXPROPIDSETY":
			{
				DynamicObject newAuxpropData2 = e.OldValue as DynamicObject;
				this.AuxpropIDSETYDataChanged(newAuxpropData2, e.Row);
				break;
			}

				return;
			}
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0001DD64 File Offset: 0x0001BF64
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSTOCKLOCID":
				this.StockControlLocSelect(e);
				return;
			case "FMATERIALID":
			case "FMATERIALIDSETY":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FEXTAUXUNITID":
			case "FEXTAUXUNITIDSETY":
			case "FSALEUNITID":
			{
				string lotF8InvFilter;
				if (!this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row, "FStockStatusID"))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = lotF8InvFilter;
					return;
				}
				IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
				listFilterParameter.Filter = listFilterParameter.Filter + " AND " + lotF8InvFilter;
				return;
			}
			case "FSTOCKIDSETY":
			{
				string lotF8InvFilter;
				if (!this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row, "FStockStatusIDSETY"))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = lotF8InvFilter;
					return;
				}
				IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
				listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + lotF8InvFilter;
				return;
			}
			case "FSTOCKSTATUSID":
			{
				string lotF8InvFilter;
				if (!this.GetStockStatusFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = lotF8InvFilter;
					return;
				}
				IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
				listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + lotF8InvFilter;
				return;
			}
			case "FSTOCKSTATUSIDSETY":
			{
				string lotF8InvFilter;
				if (!this.GetStockStatusFieldFilterSETY(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = lotF8InvFilter;
					return;
				}
				IRegularFilterParameter listFilterParameter4 = e.ListFilterParameter;
				listFilterParameter4.Filter = listFilterParameter4.Filter + " AND " + lotF8InvFilter;
				return;
			}
			case "FLOT":
			{
				object value = this.Model.GetValue("FAffairType");
				if (value != null)
				{
					if (Convert.ToString(value) != "Dassembly")
					{
						return;
					}
					string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
					{
						MaterialFieldKey = "FMaterialID",
						StockOrgFieldKey = "FStockOrgId",
						OwnerTypeFieldKey = "FOwnerTypeID",
						OwnerFieldKey = "FOwnerID",
						KeeperTypeFieldKey = "FKeeperTypeID",
						KeeperFieldKey = "FKeeperID",
						AuxpropFieldKey = "FAuxPropId",
						BomFieldKey = "FBomID",
						StockFieldKey = "FStockID",
						StockLocFieldKey = "FStockLocId",
						StockStatusFieldKey = "FStockStatusID",
						MtoFieldKey = "FMTONO",
						ProjectFieldKey = "FProjectNo"
					}, e.Row);
					if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = lotF8InvFilter;
							return;
						}
						IRegularFilterParameter listFilterParameter5 = e.ListFilterParameter;
						listFilterParameter5.Filter = listFilterParameter5.Filter + " AND " + lotF8InvFilter;
						return;
					}
				}
				break;
			}
			case "FLOTSETY":
			{
				object value = this.Model.GetValue("FAffairType");
				if (value != null)
				{
					if (Convert.ToString(value) != "Assembly")
					{
						return;
					}
					string lotF8InvFilter = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
					{
						MaterialFieldKey = "FMaterialIDSETY",
						StockOrgFieldKey = "FStockOrgId",
						OwnerTypeFieldKey = "FOwnerTypeIDSETY",
						OwnerFieldKey = "FOwnerIDSETY",
						KeeperTypeFieldKey = "FKeeperTypeIDSETY",
						KeeperFieldKey = "FKeeperIDSETY",
						AuxpropFieldKey = "FAuxPropIdSETY",
						BomFieldKey = "FBomIDSETY",
						StockFieldKey = "FStockIDSETY",
						StockLocFieldKey = "FStockLocIdSETY",
						StockStatusFieldKey = "FStockStatusIDSETY",
						MtoFieldKey = "FMTONOSETY",
						ProjectFieldKey = "FProjectNoSETY"
					}, e.Row);
					if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
					{
						if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = lotF8InvFilter;
							return;
						}
						IRegularFilterParameter listFilterParameter6 = e.ListFilterParameter;
						listFilterParameter6.Filter = listFilterParameter6.Filter + " AND " + lotF8InvFilter;
					}
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0001E220 File Offset: 0x0001C420
		private void StockControlLocSelect(BeforeF7SelectEventArgs e)
		{
			bool flag = false;
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntity");
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockID", entryCurrentRowIndex) as DynamicObject;
			if (dynamicObject != null)
			{
				flag = Convert.ToBoolean(dynamicObject["IsOpenLocation"]);
				if (!flag)
				{
					base.View.ShowMessage(ResManager.LoadKDString("仓库未启用仓位管理", "004023030000196", 5, new object[0]), 0);
				}
			}
			else
			{
				base.View.ShowMessage(ResManager.LoadKDString("选仓位前必须先选仓库", "004023030000199", 5, new object[0]), 0);
			}
			if (flag)
			{
				e.ListFilterParameter.Filter = " FSTOCKID = " + dynamicObject["ID"];
				return;
			}
			e.Cancel = true;
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0001E2E4 File Offset: 0x0001C4E4
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FMATERIALIDSETY":
			case "FSTOCKID":
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			case "FEXTAUXUNITID":
			case "FEXTAUXUNITIDSETY":
			case "FSALEUNITID":
			{
				string text;
				if (!this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row, "FStockStatusID"))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.Filter))
				{
					e.Filter = text;
					return;
				}
				e.Filter = e.Filter + " AND " + text;
				return;
			}
			case "FSTOCKIDSETY":
			{
				string text;
				if (!this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row, "FStockStatusIDSETY"))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.Filter))
				{
					e.Filter = text;
					return;
				}
				e.Filter = e.Filter + " AND " + text;
				return;
			}
			case "FSTOCKSTATUSID":
			{
				string text;
				if (!this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.Filter))
				{
					e.Filter = text;
					return;
				}
				e.Filter = e.Filter + " AND " + text;
				return;
			}
			case "FSTOCKSTATUSIDSETY":
			{
				string text;
				if (!this.GetStockStatusFieldFilterSETY(e.BaseDataFieldKey, out text, e.Row))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.Filter))
				{
					e.Filter = text;
					return;
				}
				e.Filter = e.Filter + " AND " + text;
				break;
			}

				return;
			}
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0001E504 File Offset: 0x0001C704
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
				return;
			}
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropIdSETY"))
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FAuxPropIdSETY", e.Row) as DynamicObject;
				this.lastAuxpropIdSety = ((dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]));
			}
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0001E5BC File Offset: 0x0001C7BC
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropIdSety"))
			{
				this.AuxpropDataSetyChanged(e.Row);
			}
		}

		// Token: 0x0600027C RID: 636 RVA: 0x0001E628 File Offset: 0x0001C828
		private bool ClearZeroRow()
		{
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && Convert.ToBoolean(parameterData["IsClearZeroRow"]))
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["ProductEntity"] as DynamicObjectCollection;
				int num = dynamicObjectCollection.Count - 1;
				for (int i = num; i >= 0; i--)
				{
					if (dynamicObjectCollection[i] != null && dynamicObjectCollection[i].DynamicObjectType.Properties.ContainsKey("STK_ASSEMBLYSUBITEM"))
					{
						DynamicObjectCollection dynamicObjectCollection2 = dynamicObjectCollection[i]["STK_ASSEMBLYSUBITEM"] as DynamicObjectCollection;
						int count = dynamicObjectCollection2.Count;
						for (int j = dynamicObjectCollection2.Count - 1; j >= 0; j--)
						{
							if (dynamicObjectCollection2[j]["MaterialIDSETY"] != null && Convert.ToDecimal(dynamicObjectCollection2[j]["FQtySETY"]) == 0m)
							{
								dynamicObjectCollection2.RemoveAt(j);
							}
						}
					}
				}
				base.View.UpdateView("FSubEntity");
			}
			return true;
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0001E748 File Offset: 0x0001C948
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

		// Token: 0x0600027E RID: 638 RVA: 0x0001E83C File Offset: 0x0001CA3C
		private void AuxpropDataChanged(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num == this.lastAuxpropId)
			{
				return;
			}
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FRefBomID", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, num, true, value2, true);
			if (bomDefaultValueByMaterial != value)
			{
				base.View.Model.SetValue("FRefBomID", bomDefaultValueByMaterial, row);
				if (SCMCommon.CheckMaterialIsEnableBom(dynamicObject2))
				{
					base.View.Model.SetItemValueByID("FBOMID", bomDefaultValueByMaterial, row);
				}
				else
				{
					base.View.Model.SetItemValueByID("FBOMID", null, row);
				}
			}
			this.lastAuxpropId = num;
			base.View.UpdateView("FEntity", row);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0001E964 File Offset: 0x0001CB64
		private void AuxpropDataSetyChanged(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropIdSety", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num == this.lastAuxpropIdSety)
			{
				return;
			}
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialIdSety", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBOMIdSety", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, num, true, value2, false);
			if (bomDefaultValueByMaterial != value)
			{
				base.View.Model.SetValue("FBOMIdSety", bomDefaultValueByMaterial, row);
			}
			this.lastAuxpropIdSety = num;
			base.View.UpdateView("FSubEntity", row);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0001EA50 File Offset: 0x0001CC50
		private void AuxpropDataChanged(DynamicObject newAuxpropData, int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FRefBomID", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterialExceptApi = SCMCommon.GetBomDefaultValueByMaterialExceptApi(base.View, dynamicObject, newAuxpropData, true, value2, value, true);
			if (bomDefaultValueByMaterialExceptApi != value)
			{
				base.View.Model.SetValue("FRefBomID", bomDefaultValueByMaterialExceptApi, row);
				if (SCMCommon.CheckMaterialIsEnableBom(dynamicObject))
				{
					base.View.Model.SetItemValueByID("FBOMID", bomDefaultValueByMaterialExceptApi, row);
					return;
				}
				base.View.Model.SetItemValueByID("FBOMID", null, row);
			}
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0001EB1C File Offset: 0x0001CD1C
		private void AuxpropIDSETYDataChanged(DynamicObject newAuxpropData, int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialIDSETY", row) as DynamicObject;
			long value = BillUtils.GetValue<long>(base.View.Model, "FBomIDSETY", row, 0L, null);
			long value2 = BillUtils.GetValue<long>(base.View.Model, "FStockOrgId", -1, 0L, null);
			long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject, newAuxpropData, true, value2, false);
			if (bomDefaultValueByMaterial != value)
			{
				base.View.Model.SetValue("FBomIDSETY", bomDefaultValueByMaterial, row);
			}
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0001EBAC File Offset: 0x0001CDAC
		private bool GetAllowSubSameAsPSetting()
		{
			string text = "BOS_BillUserParameter";
			if (!string.IsNullOrWhiteSpace(base.View.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = base.View.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return false;
			}
			bool result = false;
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, base.View.BusinessInfo.GetForm().Id, "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("AllowSubSameAsP") && dynamicObject["AllowSubSameAsP"] != null && !string.IsNullOrWhiteSpace(dynamicObject["AllowSubSameAsP"].ToString()))
			{
				result = Convert.ToBoolean(dynamicObject["AllowSubSameAsP"]);
			}
			return result;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0001EC94 File Offset: 0x0001CE94
		private bool CheckProductInfo()
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FEntity");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, this.Model.GetEntryCurrentRowIndex("FEntity"));
			if (entityDataObject["MATERIALID"] == null)
			{
				this._ErrMsg.Add(ResManager.LoadKDString("请确认成品行物料信息", "004023030000202", 5, new object[0]));
				return false;
			}
			return true;
		}

		// Token: 0x06000284 RID: 644 RVA: 0x0001ED08 File Offset: 0x0001CF08
		private void BomExpandByRow(int row)
		{
			this.BomExpand(new List<int>
			{
				row
			});
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0001EDDC File Offset: 0x0001CFDC
		protected void BomExpand(List<int> rows)
		{
			MemBomExpandOption memBomExpandOption = new MemBomExpandOption();
			memBomExpandOption.ExpandLevelTo = this.GetExpandLevel();
			memBomExpandOption.ValidDate = new DateTime?(TimeServiceHelper.GetSystemDateTime(base.View.Context).Date);
			memBomExpandOption.BomExpandId = SequentialGuid.NewGuid().ToString();
			memBomExpandOption.CsdSubstitution = AssembledAppEdit.IsBOMExpendCarryCsdSub;
			List<DynamicObject> bomSourceData = this.GetBomSourceData(rows, memBomExpandOption);
			if (bomSourceData == null || bomSourceData.Count<DynamicObject>() < 1)
			{
				return;
			}
			DynamicObject dynamicObject = MFGServiceHelperForSCM.ExpandBomForward(base.Context, bomSourceData, memBomExpandOption);
			if (dynamicObject != null)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject["BomExpandResult"] as DynamicObjectCollection;
				List<DynamicObject> list = new List<DynamicObject>();
				using (IEnumerator<DynamicObject> enumerator = dynamicObjectCollection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject dyCurRow = enumerator.Current;
						if (!string.IsNullOrWhiteSpace(dyCurRow["ParentEntryId"].ToString()))
						{
							string rowId = dyCurRow["RowId"].ToString();
							if (dynamicObjectCollection.FirstOrDefault((DynamicObject p) => rowId.Equals(p["ParentEntryId"].ToString())) == null)
							{
								DynamicObject dynamicObject2 = dyCurRow["MATERIALID"] as DynamicObject;
								if ((dynamicObject2 == null || ((!AssembledAppEdit.IsBOMExpendCarryMat || !Convert.ToString(dyCurRow["ISSUETYPE"]).Equals("7")) && Convert.ToBoolean(((DynamicObjectCollection)dynamicObject2["MaterialBase"])[0]["IsInventory"]))) && !Convert.ToString(dyCurRow["MaterialType"]).Equals("2"))
								{
									if (Convert.ToString(dyCurRow["MaterialType"]).Equals("3"))
									{
										dyCurRow["ACCUDISASSMBLERATE"] = 0;
										dyCurRow["BASEQTY"] = 0;
										dyCurRow["BaseActualQty"] = 0;
									}
									DynamicObject dynamicObject3 = list.FirstOrDefault((DynamicObject p) => Convert.ToInt64(p["MATERIALID_Id"]) == Convert.ToInt64(dyCurRow["MATERIALID_Id"]) && Convert.ToInt32(p["SRCSEQNO"]) == Convert.ToInt32(dyCurRow["SRCSEQNO"]) && Convert.ToInt64(p["AuxPropId_Id"]) == Convert.ToInt64(dyCurRow["AuxPropId_Id"]));
									if (dynamicObject3 == null)
									{
										dyCurRow["DISASSMBLERATE"] = dyCurRow["ACCUDISASSMBLERATE"];
										list.Add(dyCurRow);
									}
									else
									{
										if (Convert.ToInt32(dynamicObject3["BOMLEVEL"]) < Convert.ToInt32(dyCurRow["BOMLEVEL"]))
										{
											dynamicObject3["BOMLEVEL"] = dyCurRow["BOMLEVEL"];
										}
										dynamicObject3["BASEQTY"] = Convert.ToDecimal(dynamicObject3["BASEQTY"]) + Convert.ToDecimal(dyCurRow["BASEQTY"]);
										dynamicObject3["BaseActualQty"] = Convert.ToDecimal(dynamicObject3["BaseActualQty"]) + Convert.ToDecimal(dyCurRow["BaseActualQty"]);
										dynamicObject3["DISASSMBLERATE"] = Convert.ToDecimal(dynamicObject3["DISASSMBLERATE"]) + Convert.ToDecimal(dyCurRow["ACCUDISASSMBLERATE"]);
										string text = Convert.ToString(dyCurRow["Memo"]);
										if (!string.IsNullOrWhiteSpace(text))
										{
											string text2 = Convert.ToString(dynamicObject3["Memo"]);
											if (string.IsNullOrWhiteSpace(text2))
											{
												dynamicObject3["Memo"] = text;
											}
											else
											{
												dynamicObject3["Memo"] = text2 + "\r\n" + text;
											}
										}
									}
									this.MergeBOMChildData(dynamicObject3, dyCurRow);
								}
							}
						}
					}
				}
				this.UpdateSubEntity(AssembledAppEdit.UpdateUnitId(base.Context, list), rows);
			}
			MFGServiceHelperForSCM.ClearBomExpandResult(base.Context, memBomExpandOption);
		}

		// Token: 0x06000286 RID: 646 RVA: 0x0001F244 File Offset: 0x0001D444
		private List<DynamicObject> GetBomSourceData(List<int> rows, MemBomExpandOption bomQueryOption)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["ProductEntity"] as DynamicObjectCollection;
			List<DynamicObject> list = new List<DynamicObject>();
			int num = (from p in dynamicObjectCollection
			where p["RefBomID"] == null
			select p).Count<DynamicObject>();
			if (num == dynamicObjectCollection.Count<DynamicObject>())
			{
				base.View.ShowMessage(ResManager.LoadKDString("请先录入参考bom单号", "004023030000205", 5, new object[0]), 0);
				return null;
			}
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject["RefBomID"] != null && dynamicObject["MaterialID"] != null)
				{
					int num2 = Convert.ToInt32(dynamicObject["Seq"]);
					if (rows == null || rows.Count<int>() <= 0 || rows.Contains(num2 - 1))
					{
						long materialId_Id = Convert.ToInt64((dynamicObject["MaterialID"] as DynamicObject)["Id"]);
						long bomId_Id = Convert.ToInt64((dynamicObject["RefBomID"] as DynamicObject)["Id"]);
						decimal needQty = Convert.ToDecimal(dynamicObject["FBaseQty"]);
						long srcEntryId = Convert.ToInt64(dynamicObject["Id"]);
						BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
						bomForwardSourceDynamicRow.MaterialId_Id = materialId_Id;
						bomForwardSourceDynamicRow.BomId_Id = bomId_Id;
						bomForwardSourceDynamicRow.NeedQty = needQty;
						bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
						bomForwardSourceDynamicRow.UnitId_Id = BillUtils.GetDynamicObjectItemValue<long>(dynamicObject, "FUnitID_Id", 0L);
						bomForwardSourceDynamicRow.SrcInterId = Convert.ToInt64(base.View.Model.GetPKValue());
						bomForwardSourceDynamicRow.SrcEntryId = srcEntryId;
						bomForwardSourceDynamicRow.SrcSeqNo = (long)num2;
						list.Add(bomForwardSourceDynamicRow.DataEntity);
					}
				}
			}
			return list;
		}

		// Token: 0x06000287 RID: 647 RVA: 0x0001F454 File Offset: 0x0001D654
		private void BuildResultViewModel(DynamicObjectCollection result)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0001F45B File Offset: 0x0001D65B
		private void BomExpand()
		{
			this.BomExpand(null);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0001F464 File Offset: 0x0001D664
		private int GetExpandLevel()
		{
			int result = 1;
			string text = "AssembledAppUserParam";
			if (!string.IsNullOrWhiteSpace(base.View.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = base.View.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, "STK_AssembledApp", "UserParameter");
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
			{
				AssembledAppEdit.IsConLossRate = Convert.ToBoolean(dynamicObject["ConLossRate"]);
			}
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("ExpandPickInv"))
			{
				this._needExpandPickInv = Convert.ToBoolean(dynamicObject["ExpandPickInv"]);
			}
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("BOMExpendCarryMat"))
			{
				AssembledAppEdit.IsBOMExpendCarryMat = Convert.ToBoolean(dynamicObject["BOMExpendCarryMat"]);
			}
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("BOMExpendCarryCsdSub"))
			{
				AssembledAppEdit.IsBOMExpendCarryCsdSub = Convert.ToBoolean(dynamicObject["BOMExpendCarryCsdSub"]);
			}
			if (dynamicObject == null || !dynamicObject.DynamicObjectType.Properties.ContainsKey("ExpandType"))
			{
				return result;
			}
			string text2 = Convert.ToString(dynamicObject["ExpandType"]);
			string a;
			if ((a = text2) != null)
			{
				if (a == "1")
				{
					return 1;
				}
				if (a == "2")
				{
					return 30;
				}
				if (a == "3")
				{
					return (Convert.ToInt32(Convert.ToString(dynamicObject["ExpandLevel"])) == 0) ? 1 : Convert.ToInt32(Convert.ToString(dynamicObject["ExpandLevel"]));
				}
			}
			return 1;
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0001F658 File Offset: 0x0001D858
		public static List<DynamicObject> UpdateUnitId(Context ctx, List<DynamicObject> result)
		{
			Dictionary<long, long> unitIdByMaterilId = AssembledAppEdit.GetUnitIdByMaterilId(ctx, (from p in result
			select Convert.ToInt64(p["MATERIALID_ID"])).ToList<long>());
			foreach (DynamicObject dynamicObject in result)
			{
				GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs
				{
					MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
					DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
					SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
				};
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
				if (unitConvertRate.ConvertType == 1 && unitIdByMaterilId.Keys.Contains(Convert.ToInt64(dynamicObject["MATERIALID_Id"])) && Convert.ToInt64(dynamicObject["UNITID_Id"]) != unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])])
				{
					dynamicObject["UNITID_Id"] = unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])];
					getUnitConvertRateArgs = new GetUnitConvertRateArgs
					{
						MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
						DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
						SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
					};
					unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
					if (unitConvertRate != null && unitConvertRate.ConvertNumerator / unitConvertRate.ConvertDenominator > 0m)
					{
						dynamicObject["QTY"] = ((!AssembledAppEdit.IsConLossRate) ? unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseActualQty"]), "") : unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseQty"]), ""));
					}
				}
			}
			return result;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0001F884 File Offset: 0x0001DA84
		public static Dictionary<long, long> GetUnitIdByMaterilId(Context ctx, List<long> materialids)
		{
			if (materialids == null || materialids.Count<long>() < 1)
			{
				return null;
			}
			return BDCommonServiceHelper.GetUnitIdByMaterilId(ctx, materialids);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0001F89B File Offset: 0x0001DA9B
		public virtual void MergeBOMChildData(DynamicObject dyPreRow, DynamicObject dyCurRow)
		{
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0001F8D8 File Offset: 0x0001DAD8
		public virtual void UpdateSubEntity(List<DynamicObject> result, List<int> rows)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["ProductEntity"] as DynamicObjectCollection;
			DynamicObjectType dynamicObjectType = base.View.BusinessInfo.GetEntity("FSubEntity").DynamicObjectType;
			this.Model.BusinessInfo.GetEntity("FSubEntity");
			RelatedFlexGroupField auxPropFld = base.View.BusinessInfo.GetField("FAuxPropIDSETY") as RelatedFlexGroupField;
			RelatedFlexGroupField stkLocFld = base.View.BusinessInfo.GetField("FStockLocIdSETY") as RelatedFlexGroupField;
			int num = -1;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				bool fillRate = true;
				int seq = Convert.ToInt32(dynamicObject["Seq"]);
				if (rows == null || rows.Count<int>() <= 0 || rows.Contains(seq - 1))
				{
					List<DynamicObject> list = (from p in result
					where Convert.ToInt32(p["SRCSEQNO"]) == seq && Convert.ToInt32(p["BOMLEVEL"]) != 0
					select p).ToList<DynamicObject>();
					int count = list.Count;
					this.Model.SetEntryCurrentRowIndex("FEntity", seq - 1);
					DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["STK_ASSEMBLYSUBITEM"] as DynamicObjectCollection;
					dynamicObjectCollection2.Clear();
					if (count >= 1)
					{
						this.Model.BatchCreateNewEntryRow("FSubEntity", count);
						int num2 = 0;
						foreach (DynamicObject item in list)
						{
							if (this.UpdateSubRowData(item, num2, fillRate, auxPropFld, stkLocFld))
							{
								num2++;
							}
						}
						for (int i = count - 1; i > num2; i--)
						{
							dynamicObjectCollection2.RemoveAt(i);
						}
						if (num == -1)
						{
							num = seq - 1;
						}
					}
				}
			}
			if (num >= 0)
			{
				this.InvokePickInv((DynamicObjectCollection)dynamicObjectCollection[num]["STK_ASSEMBLYSUBITEM"]);
			}
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0001FB48 File Offset: 0x0001DD48
		public virtual bool UpdateSubRowData(DynamicObject item, int row, bool fillRate, RelatedFlexGroupField auxPropFld, RelatedFlexGroupField stkLocFld)
		{
			this.Model.SetValue("FSubServiceContext", "BomExpand", row);
			this.Model.SetValue("FMaterialIDSETY", item["MATERIALID_Id"], row);
			DynamicObject dynamicObject = this.Model.GetValue("FMaterialIDSETY", row) as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
			{
				if (item["MATERIALID"] == null || !DynamicObjectUtils.Contains((DynamicObject)item["MATERIALID"], "Number"))
				{
					return false;
				}
				this.Model.SetItemValueByNumber("FMaterialIDSETY", Convert.ToString(((DynamicObject)item["MATERIALID"])["Number"]), row);
				dynamicObject = (this.Model.GetValue("FMaterialIDSETY", row) as DynamicObject);
				if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
				{
					return false;
				}
			}
			base.View.InvokeFieldUpdateService("FMaterialIDSETY", row);
			this.Model.SetValue("FUnitIDSETY", item["UNITID_Id"], row);
			this.Model.SetValue("FBaseQtySETY", (!AssembledAppEdit.IsConLossRate) ? item["BaseActualQty"] : item["BaseQty"], row);
			base.View.InvokeFieldUpdateService("FBaseQtySETY", row);
			DynamicObjectCollection source = dynamicObject["MaterialInvPty"] as DynamicObjectCollection;
			DynamicObject dynamicObject2 = source.SingleOrDefault((DynamicObject p) => Convert.ToBoolean(p["IsEnable"]) && Convert.ToInt64(p["InvPtyId_Id"]) == 10003L);
			if (dynamicObject2 != null)
			{
				this.Model.SetValue("FBomIDSETY", item["BOMID_Id"], row);
			}
			DynamicObject dynamicObject3 = base.View.Model.GetEntityDataObject(auxPropFld.Entity)[row];
			auxPropFld.RefIDDynamicProperty.SetValue(dynamicObject3, (item["AuxPropId"] == null) ? 0 : ((DynamicObject)item["AuxPropId"])["Id"]);
			this.Model.SetValue("FAuxPropIDSETY", item["AuxPropId"], row);
			if (Convert.ToInt64(item["STOCKID_Id"]) > 0L)
			{
				this.Model.SetValue("FStockIDSETY", item["STOCKID_Id"], row);
				stkLocFld.RefIDDynamicProperty.SetValue(dynamicObject3, (item["STOCKLOCID"] == null) ? 0 : ((DynamicObject)item["STOCKLOCID"])["Id"]);
				this.Model.SetValue("FStockLocIdSETY", item["STOCKLOCID"], row);
			}
			else
			{
				DynamicObject dynamicObject4 = ((DynamicObjectCollection)dynamicObject["MaterialStock"])[0];
				if (dynamicObject4 != null)
				{
					this.Model.SetValue("FStockIDSETY", dynamicObject4["StockId_Id"], row);
					stkLocFld.RefIDDynamicProperty.SetValue(dynamicObject3, (dynamicObject4["StockPlaceId"] == null) ? 0 : ((DynamicObject)dynamicObject4["StockPlaceId"])["Id"]);
					this.Model.SetValue("FStockLocIdSETY", dynamicObject4["StockPlaceId"], row);
				}
			}
			if (fillRate)
			{
				this.Model.SetValue("FCostProportion", item["DISASSMBLERATE"], row);
			}
			object value = this.Model.GetValue("FStockOrgId");
			this.Model.SetValue("FKeeperIDSETY", value, row);
			object value2 = this.Model.GetValue("FOwnerTypeIdHead");
			this.Model.SetValue("FOwnerTypeIDSETY", value2, row);
			value2 = this.Model.GetValue("FOwnerIdHead");
			this.Model.SetValue("FOwnerIDSETY", value2, row);
			string text = Convert.ToString(item["Memo"]);
			if (text.Length > this._subMemLen)
			{
				text = text.Substring(0, this._subMemLen);
			}
			this.Model.SetValue("FDescriptionSETY", text, row);
			this.Model.SetValue("FSubServiceContext", "", row);
			return true;
		}

		// Token: 0x0600028F RID: 655 RVA: 0x0001FF88 File Offset: 0x0001E188
		public virtual void InvokePickInv(DynamicObjectCollection subEntrys)
		{
			if (!this._needExpandPickInv)
			{
				return;
			}
			if (subEntrys.Count < 1)
			{
				return;
			}
			if (this._pickInvService != null && this._pickInvService.IsEnabled && !this._pickInvService.IsForbidden)
			{
				string value = Convert.ToString(this.Model.GetValue("FDocumentStatus"));
				if ("B".Equals(value) || "C".Equals(value))
				{
					return;
				}
				value = Convert.ToString(this.Model.GetValue("FCANCELSTATUS"));
				if ("B".Equals(value))
				{
					return;
				}
				FormBusinessServiceUtil.InvokeService(base.View, this._barItem, this._pickInvService, "FSubEntity", subEntrys[0], 0);
			}
		}

		// Token: 0x06000290 RID: 656 RVA: 0x0002005C File Offset: 0x0001E25C
		public virtual void GetSubPickInvService()
		{
			this._pickInvService = null;
			this._barItem = null;
			foreach (Appearance appearance in base.View.LayoutInfo.Appearances)
			{
				if ("FSubEntity".Equals(appearance.Key))
				{
					BarDataManager menu = ((EntryEntityAppearance)appearance).Menu;
					foreach (BarItem barItem in menu.BarItems)
					{
						if (!ListUtils.IsEmpty<FormBusinessService>(barItem.ClickActions))
						{
							FormBusinessService formBusinessService = barItem.ClickActions.FirstOrDefault((FormBusinessService p) => p.ActionId == 133L);
							if (formBusinessService != null)
							{
								this._pickInvService = (formBusinessService.Clone() as FormBusinessService);
								((LotPickingBusinessServiceMeta)this._pickInvService).OnlyCurrentRow = false;
								this._pickInvService.ClassName = "Kingdee.K3.SCM.Business.DynamicForm.BusinessService.PickInventory, Kingdee.K3.SCM.Business.DynamicForm";
								this._barItem = barItem;
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0002019C File Offset: 0x0001E39C
		private bool StringEqualsOrdinal(object str1, object str2)
		{
			return str1 != null && str2 != null && string.Equals(str1.ToString(), str2.ToString(), StringComparison.Ordinal);
		}

		// Token: 0x06000292 RID: 658 RVA: 0x000201B8 File Offset: 0x0001E3B8
		private string BuildErrInfo()
		{
			if (this._ErrMsg.Count > 10)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string value in this._ErrMsg)
				{
					stringBuilder.AppendLine(value);
				}
				this._ErrMsg.Clear();
				return stringBuilder.ToString();
			}
			string text = null;
			foreach (string str in this._ErrMsg)
			{
				text = text + str + Environment.NewLine;
			}
			this._ErrMsg.Clear();
			return text;
		}

		// Token: 0x06000293 RID: 659 RVA: 0x0002029C File Offset: 0x0001E49C
		public void SetDefLocalCurrency()
		{
			GetLocalCurrencyArgs getLocalCurrencyArgs = new GetLocalCurrencyArgs("2", "FStockOrgId", "", "FBASECURRID", "", "FOwnerTypeIdHead", "FOwnerIdHead");
			SCMCommon.SetDefCurrencyAndExchangeType(this, getLocalCurrencyArgs);
		}

		// Token: 0x06000294 RID: 660 RVA: 0x000202DC File Offset: 0x0001E4DC
		private bool GetStockFieldFilter(string fieldKey, out string filter, int row, string stockStatusId)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FSTOCKID":
			case "FSTOCKIDSETY":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue(stockStatusId, row) as DynamicObject;
				if (dynamicObject != null)
				{
					filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject["Type"]);
				}
				break;
			}
			case "FMATERIALID":
			case "FMATERIALIDSETY":
				filter = " FIsInventory = '1'";
				break;
			case "FSTOCKERID":
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FSTOCKERGROUPID") as DynamicObject;
				filter += " FIsUse='1' ";
				long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				if (num2 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num2.ToString();
				}
				break;
			}
			case "FSTOCKERGROUPID":
			{
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FSTOCKERID") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject3["Id"]));
				}
				break;
			}
			case "FEXTAUXUNITID":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
				break;
			case "FEXTAUXUNITIDSETY":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialIDSETY", "FBaseUnitIDSETY", "FSecUnitIDSETY", row);
				break;
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000295 RID: 661 RVA: 0x00020510 File Offset: 0x0001E710
		private bool GetStockStatusFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockID", row) as DynamicObject;
			if (dynamicObject != null)
			{
				string text = Convert.ToString(dynamicObject["StockStatusType"]);
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = "'" + text.Replace(",", "','") + "'";
					filter = string.Format(" FType IN ({0})", text);
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000296 RID: 662 RVA: 0x000205A4 File Offset: 0x0001E7A4
		private bool GetStockStatusFieldFilterSETY(string fieldKey, out string filter, int row)
		{
			filter = "";
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockIDSETY", row) as DynamicObject;
			if (dynamicObject != null)
			{
				string arg = Convert.ToString(dynamicObject["StockStatusType"]);
				filter = string.Format(" FType IN ({0})", arg);
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000297 RID: 663 RVA: 0x00020610 File Offset: 0x0001E810
		private void UpdateBillCostInfo()
		{
			if (!this.hasPara || Convert.ToInt64(base.View.Model.DataObject["Id"]) <= 0L)
			{
				return;
			}
			AcctgResultRefreshBill billCostInfo = this.GetBillCostInfo(this.hsorgId, this.hsacctsysid, this.hsacctPolicyId);
			if (billCostInfo != null)
			{
				this.ApplyCostData(billCostInfo);
			}
			base.View.GetMainBarItem("tbSave").Enabled = false;
			base.View.GetMainBarItem("tbSplitSave").Enabled = false;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x00020724 File Offset: 0x0001E924
		private void ApplyCostData(AcctgResultRefreshBill costInfo)
		{
			AssembledAppEdit.<>c__DisplayClass1d CS$<>8__locals1 = new AssembledAppEdit.<>c__DisplayClass1d();
			if (costInfo == null || costInfo.EntryDatas == null || costInfo.EntryDatas.Count < 1)
			{
				return;
			}
			this.Model.SetItemValueByID("FBASECURRID", costInfo.LocalCurrId, 0);
			Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			CS$<>8__locals1.entryData = entryEntity.DynamicProperty.GetValue<DynamicObjectCollection>(this.Model.DataObject);
			EntryEntity entryEntity2 = base.View.BusinessInfo.GetEntryEntity("FSubEntity");
			CS$<>8__locals1.subEntryData = null;
			DynamicObjectCollection dynamicObjectCollection = null;
			costInfo.EntryDatas.TryGetValue("FEntity", out dynamicObjectCollection);
			DynamicObjectCollection dynamicObjectCollection2 = null;
			costInfo.EntryDatas.TryGetValue("FSubEntity", out dynamicObjectCollection2);
			int count = CS$<>8__locals1.entryData.Count;
			Field field = base.View.BusinessInfo.GetField("FAmountSETY");
			Field field2 = base.View.BusinessInfo.GetField("FPriceSETY");
			Field field3 = base.View.BusinessInfo.GetField("FBaseQtySETY");
			if (dynamicObjectCollection == null || count < 1)
			{
				return;
			}
			int i;
			for (i = 0; i < count; i++)
			{
				decimal num = 0m;
				decimal num2 = 0m;
				DynamicObject dynamicObject = dynamicObjectCollection.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["FEntryId"]) == Convert.ToInt64(CS$<>8__locals1.entryData[i]["Id"]));
				if (dynamicObject != null)
				{
					num = Convert.ToDecimal(dynamicObject["FAmount_LC"]);
					this.Model.SetValue("FAmount", num, i);
					base.View.InvokeFieldUpdateService("FAmount", i);
					decimal value = BillUtils.GetValue<decimal>(this.Model, "FBaseQty", i, 0m, null);
					if (value != 0m)
					{
						this.Model.SetValue("FPrice", num / value, i);
					}
					else
					{
						this.Model.SetValue("FPrice", 0, i);
					}
				}
				CS$<>8__locals1.subEntryData = entryEntity2.DynamicProperty.GetValue<DynamicObjectCollection>(CS$<>8__locals1.entryData[i]);
				int count2 = CS$<>8__locals1.subEntryData.Count;
				if (dynamicObjectCollection2 == null || count2 < 1)
				{
					this.Model.SetValue("Fee_ETY", num, i);
					base.View.InvokeFieldUpdateService("Fee_ETY", i);
				}
				else
				{
					this.Model.SetEntryCurrentRowIndex("FEntity", i);
					int j;
					for (j = 0; j < count2; j++)
					{
						DynamicObject dynamicObject2 = dynamicObjectCollection2.SingleOrDefault((DynamicObject p) => Convert.ToInt64(p["FEntryId"]) == Convert.ToInt64(CS$<>8__locals1.subEntryData[j]["Id"]));
						if (dynamicObject2 != null)
						{
							decimal num3 = Convert.ToDecimal(dynamicObject2["FAmount_LC"]);
							field.DynamicProperty.SetValue(CS$<>8__locals1.subEntryData[j], num3);
							base.View.InvokeFieldUpdateService("FAmountSETY", j);
							decimal value = field3.DynamicProperty.GetValue<decimal>(CS$<>8__locals1.subEntryData[j]);
							if (value != 0m)
							{
								field2.DynamicProperty.SetValue(CS$<>8__locals1.subEntryData[j], num3 / value);
							}
							else
							{
								field2.DynamicProperty.SetValue(CS$<>8__locals1.subEntryData[j], 0);
								this.Model.SetValue("FPriceSETY", 0, j);
							}
							num2 += num3;
						}
					}
					this.Model.SetValue("Fee_ETY", Math.Abs(num - num2), i);
					base.View.InvokeFieldUpdateService("Fee_ETY", i);
				}
			}
			base.View.UpdateView("FSubEntity");
		}

		// Token: 0x06000299 RID: 665 RVA: 0x00020BE0 File Offset: 0x0001EDE0
		private bool GetBillOpenParameter(out long orgId, out long acctsysid, out long acctPolicyId)
		{
			bool flag = false;
			orgId = 0L;
			acctsysid = 0L;
			acctPolicyId = 0L;
			DynamicFormOpenParameter openParameter = base.View.OpenParameter;
			if (openParameter == null && base.View.ParentFormView != null && base.View.ParentFormView is IListView)
			{
				openParameter = base.View.ParentFormView.OpenParameter;
			}
			if (openParameter == null)
			{
				return flag;
			}
			object customParameter = openParameter.GetCustomParameter("FACCTSYSTEMID");
			if (customParameter != null)
			{
				long.TryParse(customParameter.ToString(), out acctsysid);
			}
			if (acctsysid == 0L)
			{
				if (base.View.ParentFormView == null || !(base.View.ParentFormView is IListView))
				{
					return flag;
				}
				openParameter = base.View.ParentFormView.OpenParameter;
				if (openParameter == null)
				{
					return flag;
				}
				customParameter = openParameter.GetCustomParameter("FACCTSYSTEMID");
				if (customParameter != null)
				{
					long.TryParse(customParameter.ToString(), out acctsysid);
				}
				if (acctsysid == 0L)
				{
					return flag;
				}
			}
			customParameter = base.View.OpenParameter.GetCustomParameter("FACCTORGID");
			if (customParameter != null)
			{
				long.TryParse(customParameter.ToString(), out orgId);
			}
			if (orgId == 0L)
			{
				return flag;
			}
			customParameter = base.View.OpenParameter.GetCustomParameter("FACCTPOLICYID");
			if (customParameter != null)
			{
				long.TryParse(customParameter.ToString(), out acctPolicyId);
			}
			return acctPolicyId != 0L || flag;
		}

		// Token: 0x0600029A RID: 666 RVA: 0x00020D20 File Offset: 0x0001EF20
		private AcctgResultRefreshBill GetBillCostInfo(long orgId, long acctsysid, long acctPolicyId)
		{
			AcctgResultRefreshBill acctgResultRefreshBill = new AcctgResultRefreshBill();
			acctgResultRefreshBill.AcctgSysId = acctsysid;
			acctgResultRefreshBill.OrgId = orgId;
			acctgResultRefreshBill.AcctPolicyId = acctPolicyId;
			acctgResultRefreshBill.BillFromId = base.View.BillBusinessInfo.GetForm().Id;
			acctgResultRefreshBill.BillId = Convert.ToInt64(this.Model.DataObject["Id"]);
			return CommonServiceHelper.GetBillHSCostInfo(base.View.Context, acctgResultRefreshBill);
		}

		// Token: 0x040000E3 RID: 227
		private StringCollection _ErrMsg = new StringCollection();

		// Token: 0x040000E4 RID: 228
		private bool hasPara;

		// Token: 0x040000E5 RID: 229
		private long hsacctsysid;

		// Token: 0x040000E6 RID: 230
		private long hsorgId;

		// Token: 0x040000E7 RID: 231
		private long hsacctPolicyId;

		// Token: 0x040000E8 RID: 232
		private bool hasAsked;

		// Token: 0x040000E9 RID: 233
		private long lastAuxpropId;

		// Token: 0x040000EA RID: 234
		private long lastAuxpropIdSety;

		// Token: 0x040000EB RID: 235
		private static bool IsConLossRate;

		// Token: 0x040000EC RID: 236
		private static bool IsBOMExpendCarryMat;

		// Token: 0x040000ED RID: 237
		private static bool IsBOMExpendCarryCsdSub;

		// Token: 0x040000EE RID: 238
		private int _subMemLen;

		// Token: 0x040000EF RID: 239
		protected FormBusinessService _pickInvService;

		// Token: 0x040000F0 RID: 240
		protected BarItem _barItem;

		// Token: 0x040000F1 RID: 241
		protected bool _needExpandPickInv;
	}
}
