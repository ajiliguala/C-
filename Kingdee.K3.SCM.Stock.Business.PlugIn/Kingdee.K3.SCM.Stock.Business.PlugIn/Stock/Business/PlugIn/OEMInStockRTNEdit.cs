using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Common.BusinessEntity.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000056 RID: 86
	public class OEMInStockRTNEdit : AbstractBillPlugIn
	{
		// Token: 0x060003D3 RID: 979 RVA: 0x0002E304 File Offset: 0x0002C504
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.View.RuleContainer.AddPluginRule<OEMInStock>("FBillHead", 1, new Action<OEMInStock>(this.SetStockerGroup), new string[]
			{
				"FStockerId"
			});
			this._baseDataOrgCtl = Common.GetSalBaseDataCtrolType(base.View.Context);
			base.OnInitialize(e);
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x0002E364 File Offset: 0x0002C564
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				if (base.View.OpenParameter.CreateFrom != 1)
				{
					long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
					if (baseDataLongValue > 0L)
					{
						SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue);
						Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
					}
				}
				this.GetUseCustMatMappingParamater();
			}
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0002E3D4 File Offset: 0x0002C5D4
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x0002E3E0 File Offset: 0x0002C5E0
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.GetUseCustMatMappingParamater();
			base.View.GetControl("FCustMatID").Visible = this.para_UseCustMatMapping;
			base.View.GetControl("FCustMatName").Visible = this.para_UseCustMatMapping;
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0002E430 File Offset: 0x0002C630
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FEXTAUXUNITID":
			case "FCUSTMATID":
			{
				string lotF8InvFilter;
				if (this.GetStockFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter = listFilterParameter.Filter + " AND " + lotF8InvFilter;
					}
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				string lotF8InvFilter;
				if (this.GetStockStatusFieldFilter(e.FieldKey, out lotF8InvFilter, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
					}
					else
					{
						IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
						listFilterParameter2.Filter = listFilterParameter2.Filter + " AND " + lotF8InvFilter;
					}
				}
				break;
			}
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			{
				string lotF8InvFilter;
				if (this.GetFieldFilter(e.FieldKey, out lotF8InvFilter, -1))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter) ? lotF8InvFilter : (e.ListFilterParameter.Filter + "AND" + lotF8InvFilter));
				}
				break;
			}
			case "FLOT":
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
					BomFieldKey = "FBomId",
					StockFieldKey = "FStockId",
					StockLocFieldKey = "FStockLocId",
					StockStatusFieldKey = "FStockStatusId",
					MtoFieldKey = "FMtoNo",
					ProjectFieldKey = "FProjectNo"
				}, e.Row);
				if (!string.IsNullOrWhiteSpace(lotF8InvFilter))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = lotF8InvFilter;
					}
					else
					{
						IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
						listFilterParameter3.Filter = listFilterParameter3.Filter + " AND " + lotF8InvFilter;
					}
				}
				break;
			}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x0002E6FC File Offset: 0x0002C8FC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FMATERIALID":
			case "FSTOCKID":
			case "FEXTAUXUNITID":
			case "FCUSTMATID":
			{
				string text;
				if (this.GetStockFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrWhiteSpace(e.Filter))
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
			case "FSTOCKSTATUSID":
			{
				string text;
				if (this.GetStockStatusFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrWhiteSpace(e.Filter))
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
			case "FSTOCKERID":
			case "FSTOCKERGROUPID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, -1))
				{
					e.Filter = (string.IsNullOrWhiteSpace(e.Filter) ? text : (e.Filter + "AND" + text));
				}
				break;
			}
			}
			base.BeforeSetItemValueByNumber(e);
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x0002E88F File Offset: 0x0002CA8F
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if ("PickInvReturnBegin".Equals(e.EventName))
			{
				this.isPickReturn = true;
				return;
			}
			if ("PickInvReturnFinish".Equals(e.EventName))
			{
				this.isPickReturn = false;
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0002E8CC File Offset: 0x0002CACC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKERID"))
				{
					if (!(a == "FMATERIALID"))
					{
						if (!(a == "FAUXPROPID"))
						{
							if (!(a == "FCUSTID"))
							{
								if (a == "FCUSTMATID")
								{
									bool flag = base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id);
									if (!this.copyEntryRow && !this.associatedCopyEntryRow && !flag && !this.isPickReturn)
									{
										CustomerMaterialMappingArgs customerMaterialMappingArgs = new CustomerMaterialMappingArgs();
										DynamicObject dynamicObject = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
										customerMaterialMappingArgs.CustMatId = ((dynamicObject == null) ? "" : dynamicObject["Id"].ToString());
										DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
										customerMaterialMappingArgs.MainOrgId = ((dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]));
										customerMaterialMappingArgs.NeedOrgCtrl = this._baseDataOrgCtl["BD_MATERIAL"];
										customerMaterialMappingArgs.MaterialIdKey = "FMaterialId";
										customerMaterialMappingArgs.AuxpropIdKey = "FAuxpropId";
										customerMaterialMappingArgs.Row = e.Row;
										Common.SetMaterialIdAndAuxpropIdByCustMatId(this, customerMaterialMappingArgs);
									}
								}
							}
							else if (this.para_UseCustMatMapping)
							{
								Common.SetCustMatWhenCustChange(this, "FBillEntry", "FStockOrgId", "FCUSTID", "FCUSTMATID", "FCustMatName");
							}
						}
						else
						{
							DynamicObject newAuxpropData = e.OldValue as DynamicObject;
							this.AuxpropDataChanged(newAuxpropData, e.Row);
						}
					}
					else
					{
						long num = 0L;
						DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
						if (dynamicObject3 != null)
						{
							num = Convert.ToInt64(dynamicObject3["Id"]);
						}
						DynamicObject dynamicObject4 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
						base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject4, 0L, false, num, false), e.Row);
						long materialId = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
						DynamicObject dynamicObject5 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
						string text = (dynamicObject5 != null) ? Convert.ToString(dynamicObject5["Id"]) : "";
						bool flag = base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id);
						if (((flag || this.isPickReturn) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(text)) || (!flag && !this.associatedCopyEntryRow && !this.copyEntryRow && !this.isPickReturn))
						{
							DynamicObject dynamicObject6 = base.View.Model.GetValue("FCustId") as DynamicObject;
							DynamicObject dynamicObject7 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
							long customerId = (dynamicObject6 == null) ? 0L : Convert.ToInt64(dynamicObject6["Id"]);
							long saleOrgId = (dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["Id"]);
							if (this.para_UseCustMatMapping)
							{
								Common.SetRelativeCodeByMaterialId(this, "FCustMatId", materialId, customerId, saleOrgId, e.Row);
							}
						}
					}
				}
				else
				{
					Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
				}
			}
			base.DataChanged(e);
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0002ED24 File Offset: 0x0002CF24
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0002ED88 File Offset: 0x0002CF88
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0002EDBD File Offset: 0x0002CFBD
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			if (!this.ClearZeroRow())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0002EDD8 File Offset: 0x0002CFD8
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW"))
				{
					if (a == "COPYENTRYROW")
					{
						this.copyEntryRow = true;
					}
				}
				else
				{
					this.associatedCopyEntryRow = true;
				}
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x060003DF RID: 991 RVA: 0x0002EE34 File Offset: 0x0002D034
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ASSOCIATEDCOPYENTRYROW"))
				{
					if (a == "COPYENTRYROW")
					{
						this.copyEntryRow = false;
					}
				}
				else
				{
					this.associatedCopyEntryRow = false;
				}
			}
			base.AfterDoOperation(e);
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0002EE88 File Offset: 0x0002D088
		private bool ClearZeroRow()
		{
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && Convert.ToBoolean(parameterData["IsClearZeroRow"]))
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["OEMInStockRTNEntry"] as DynamicObjectCollection;
				int num = dynamicObjectCollection.Count - 1;
				for (int i = num; i >= 0; i--)
				{
					if (dynamicObjectCollection[i]["MaterialId"] != null && Convert.ToDecimal(dynamicObjectCollection[i]["Qty"]) == 0m)
					{
						this.Model.DeleteEntryRow("FBillEntry", i);
					}
				}
				if (this.Model.GetEntryRowCount("FBillEntry") == 0)
				{
					base.View.ShowErrMessage("", ResManager.LoadKDString("分录“明细”是必填项。", "004023000021872", 5, new object[0]), 0);
					return false;
				}
				base.View.UpdateView("FBillEntry");
			}
			return true;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0002EF7F File Offset: 0x0002D17F
		private void SetStockerGroup(OEMInStock oemInStock)
		{
			Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x0002EF98 File Offset: 0x0002D198
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

		// Token: 0x060003E3 RID: 995 RVA: 0x0002F08C File Offset: 0x0002D28C
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
			base.View.UpdateView("FBillEntry", row);
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x0002F178 File Offset: 0x0002D378
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

		// Token: 0x060003E5 RID: 997 RVA: 0x0002F208 File Offset: 0x0002D408
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
					if (!(a == "FEXTAUXUNITID"))
					{
						if (a == "FCUSTMATID")
						{
							DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
							long mainOrgId = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
							DynamicObject dynamicObject2 = base.View.Model.GetValue("FCustId") as DynamicObject;
							long custId = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
							DynamicObject dynamicObject3 = base.View.Model.GetValue("FMaterialID", row) as DynamicObject;
							long materialId = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
							filter = Common.GetMapIdFilter(mainOrgId, custId, materialId, this._baseDataOrgCtl);
						}
					}
					else
					{
						filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", row);
					}
				}
				else
				{
					string arg = string.Empty;
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FSTOCKSTATUSID", row) as DynamicObject;
					arg = ((dynamicObject4 == null) ? "" : Convert.ToString(dynamicObject4["Number"]));
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
						DynamicObject dynamicObject5 = dynamicObjectCollection[0];
						filter = string.Format(" FFORBIDSTATUS='A' AND FDOCUMENTSTATUS='C' AND FSTOCKSTATUSTYPE LIKE '%{0}%'", dynamicObject5["FType"]);
					}
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x0002F410 File Offset: 0x0002D610
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
				string text = Convert.ToString(dynamicObject["StockStatusType"]);
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = "'" + text.Replace(",", "','") + "'";
					filter = string.Format(" FType IN ({0})", text);
				}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x0002F4A0 File Offset: 0x0002D6A0
		private bool GetFieldFilter(string fieldKey, out string filter, int row = -1)
		{
			filter = string.Empty;
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string a;
			if ((a = fieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FOWNERIDHEAD"))
				{
					if (!(a == "FSTOCKERID"))
					{
						if (!(a == "FSTOCKERGROUPID"))
						{
							if (a == "FBOMID")
							{
								DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId") as DynamicObject;
								if (dynamicObject != null)
								{
									DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialBase"] as DynamicObjectCollection;
									DynamicObject dynamicObject2 = dynamicObjectCollection[0];
								}
							}
						}
						else
						{
							DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockerId") as DynamicObject;
							filter += " FIsUse='1' ";
							if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) > 0L)
							{
								filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject3["Id"]));
							}
						}
					}
					else
					{
						DynamicObject dynamicObject4 = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
						filter += " FIsUse='1' ";
						long num = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
						if (0L != num)
						{
							filter = filter + "And FOPERATORGROUPID = " + num.ToString();
						}
					}
				}
				else
				{
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					filter = "FUseOrgId = " + ((dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"])).ToString();
				}
			}
			return true;
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x0002F668 File Offset: 0x0002D868
		private void GetUseCustMatMappingParamater()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			if (dynamicObject != null)
			{
				long num = Convert.ToInt64(dynamicObject["Id"]);
				object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, num, "SAL_SystemParameter", "UseCustMatMapping", false);
				this.para_UseCustMatMapping = (systemProfile != null && Convert.ToBoolean(systemProfile));
			}
		}

		// Token: 0x0400016A RID: 362
		private long lastAuxpropId;

		// Token: 0x0400016B RID: 363
		private bool para_UseCustMatMapping;

		// Token: 0x0400016C RID: 364
		private bool associatedCopyEntryRow;

		// Token: 0x0400016D RID: 365
		private bool copyEntryRow;

		// Token: 0x0400016E RID: 366
		private bool isPickReturn;

		// Token: 0x0400016F RID: 367
		private Dictionary<string, bool> _baseDataOrgCtl = new Dictionary<string, bool>();
	}
}
