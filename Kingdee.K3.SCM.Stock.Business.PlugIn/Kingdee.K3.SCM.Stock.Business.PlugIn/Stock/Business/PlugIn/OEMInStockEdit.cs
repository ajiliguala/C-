using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Common.BusinessEntity.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000077 RID: 119
	public class OEMInStockEdit : AbstractBillPlugIn
	{
		// Token: 0x06000557 RID: 1367 RVA: 0x00041364 File Offset: 0x0003F564
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.View.RuleContainer.AddPluginRule<OEMInStock>("FBillHead", 1, new Action<OEMInStock>(this.SetStockerGroup), new string[]
			{
				"FStockerId"
			});
			this._baseDataOrgCtl = Common.GetSalBaseDataCtrolType(base.View.Context);
			base.OnInitialize(e);
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x000413C4 File Offset: 0x0003F5C4
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBBOMEXPAND"))
				{
					return;
				}
				Common.BomExpand(this);
				base.View.UpdateView("FBillEntry");
			}
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x00041404 File Offset: 0x0003F604
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			if (e.EntityKey.ToUpperInvariant() == "FSUBHEADENTITY")
			{
				this.DeleteOEMInstockEntryRow(e.Row);
				return;
			}
			if (e.EntityKey.ToUpperInvariant() == "FBILLENTRY")
			{
				this.DeleteHeadEntryRow(e.Row);
			}
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x00041494 File Offset: 0x0003F694
		private void DeleteOEMInstockEntryRow(int row)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["OEMInStockEntry"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = base.View.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count<DynamicObject>() < 1)
			{
				return;
			}
			long lRow = Convert.ToInt64(base.View.Model.GetValue("FSubSrcEntryId", row));
			if (lRow == 0L)
			{
				return;
			}
			int srcSeqNo = Convert.ToInt32(dynamicObjectCollection2[row]["Seq"]);
			List<DynamicObject> list = (from p in dynamicObjectCollection
			where Convert.ToInt64(p["SrcEntryId"]) == lRow && Convert.ToInt32(p["SrcSeqNo"]) == srcSeqNo
			select p).ToList<DynamicObject>();
			if (list == null || list.Count<DynamicObject>() < 1)
			{
				return;
			}
			foreach (DynamicObject dynamicObject in list)
			{
				this.Model.DeleteEntryRow("FBillEntry", Convert.ToInt32(dynamicObject["Seq"]) - 1);
			}
			base.View.UpdateView("FBillEntry");
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x000415D4 File Offset: 0x0003F7D4
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

		// Token: 0x0600055C RID: 1372 RVA: 0x00041630 File Offset: 0x0003F830
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

		// Token: 0x0600055D RID: 1373 RVA: 0x00041684 File Offset: 0x0003F884
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
				else
				{
					string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
					if (StringUtils.EqualsIgnoreCase(text, "SAL_SaleOrder") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM"))
					{
						long baseDataLongValue2 = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
						if (baseDataLongValue2 > 0L)
						{
							SCMCommon.SetOpertorIdByUserId(this, "FStockerId", "WHY", baseDataLongValue2);
							Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
						}
					}
				}
				this.GetUseCustMatMappingParamater();
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0004176A File Offset: 0x0003F96A
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x00041774 File Offset: 0x0003F974
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.AfterUpdateViewState(e);
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if (StringUtils.EqualsIgnoreCase(text, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM"))
			{
				this.LockMaterialField();
			}
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x000418A8 File Offset: 0x0003FAA8
		public override void AfterBindData(EventArgs e)
		{
			string sSubSrcType = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				if (StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_SaleOrder"))
				{
					this.SetDefaultBOM();
					Common.BomExpand(this);
					base.View.UpdateView("FBillEntry");
				}
				else if (StringUtils.EqualsIgnoreCase(sSubSrcType, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_OEMBOM"))
				{
					base.View.GetMainBarItem("tbBomExpand").Visible = false;
					if (base.View.OpenParameter.CreateFrom == 2)
					{
						Entity entity = base.View.BusinessInfo.GetEntity("FSubHeadEntity");
						DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
						Dictionary<long, IGrouping<long, DynamicObject>> dictionary = new Dictionary<long, IGrouping<long, DynamicObject>>();
						dictionary = (from w in entityDataObject
						where StringUtils.EqualsIgnoreCase(Convert.ToString(w["SubSRCTYPE"]), sSubSrcType)
						select w into g
						group g by Convert.ToInt64(g["SubSrcEntryId"])).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
						bool flag = false;
						foreach (KeyValuePair<long, IGrouping<long, DynamicObject>> keyValuePair in dictionary)
						{
							if (keyValuePair.Value.Count<DynamicObject>() > 1)
							{
								int num = 0;
								flag = true;
								foreach (DynamicObject dynamicObject in keyValuePair.Value)
								{
									if (num == 0)
									{
										dynamicObject["SubQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["SubQty"]));
										dynamicObject["SubBaseQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["SubBaseQty"]));
										dynamicObject["SubAuxQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["SubAuxQty"]));
										dynamicObject["ReceiveBaseQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["ReceiveBaseQty"]));
										dynamicObject["ReceiveAuxQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["ReceiveAuxQty"]));
										dynamicObject["CsnReceiveBaseQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["CsnReceiveBaseQty"]));
										dynamicObject["CsnReceiveAuxQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["CsnReceiveAuxQty"]));
										dynamicObject["RefuseBaseQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["RefuseBaseQty"]));
										dynamicObject["RefuseAuxQty"] = keyValuePair.Value.Sum((DynamicObject p) => Convert.ToDecimal(p["RefuseAuxQty"]));
									}
									else
									{
										entityDataObject.Remove(dynamicObject);
									}
									num++;
								}
							}
						}
						if (flag)
						{
							int num2 = 1;
							foreach (DynamicObject dynamicObject2 in entityDataObject)
							{
								dynamicObject2["Seq"] = num2;
								num2++;
							}
							base.View.UpdateView("FSubHeadEntity");
						}
					}
				}
			}
			if (StringUtils.EqualsIgnoreCase(sSubSrcType, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_OEMBOM"))
			{
				this.LockMaterialField();
			}
			if (StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_SaleOrder"))
			{
				if (base.Context.ClientType == 16)
				{
					IDynamicFormView view = base.View;
					string text = "expandBizPanel";
					JSONArray jsonarray = new JSONArray();
					jsonarray.Add("FTab_P");
					jsonarray.Add(true);
					view.AddAction(text, jsonarray);
				}
			}
			else if (base.Context.ClientType == 16)
			{
				IDynamicFormView view2 = base.View;
				string text2 = "expandBizPanel";
				JSONArray jsonarray2 = new JSONArray();
				jsonarray2.Add("FTab_P");
				jsonarray2.Add(false);
				view2.AddAction(text2, jsonarray2);
			}
			this.GetUseCustMatMappingParamater();
			base.View.GetControl("FCustMatID").Visible = this.para_UseCustMatMapping;
			base.View.GetControl("FCustMatName").Visible = this.para_UseCustMatMapping;
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00041EC8 File Offset: 0x000400C8
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if (StringUtils.EqualsIgnoreCase(text, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM"))
			{
				OEMInStockEdit.<>c__DisplayClass20 CS$<>8__locals1 = new OEMInStockEdit.<>c__DisplayClass20();
				CS$<>8__locals1.subEntrys = (this.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection);
				Entity entity = this.Model.BusinessInfo.GetEntity("FBillEntry");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
				int j = CS$<>8__locals1.subEntrys.Count - 1;
				int i;
				for (i = j; i >= 0; i--)
				{
					if (CS$<>8__locals1.subEntrys[i]["SubMaterialId"] != null && CS$<>8__locals1.subEntrys[i]["SubSRCTYPE"] == text)
					{
						if ((from p in entityDataObject
						where Convert.ToInt64(p["SrcEntryId"]) == Convert.ToInt64(CS$<>8__locals1.subEntrys[i]["SubSrcEntryId"])
						select p).FirstOrDefault<DynamicObject>() == null)
						{
							this.Model.DeleteEntryRow("FSubHeadEntity", i);
						}
					}
				}
				base.View.UpdateView("FSubHeadEntity");
			}
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x00042064 File Offset: 0x00040264
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13)
			{
				Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FSubHeadEntity");
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
				if (entityDataObject != null && entityDataObject.Count<DynamicObject>() > 0)
				{
					string text = (from p in entityDataObject
					where !string.IsNullOrEmpty(Convert.ToString(p["SubSRCTYPE"]))
					select Convert.ToString(p["SubSRCTYPE"])).FirstOrDefault<string>();
					if (text == null || ObjectUtils.IsNullOrEmpty(Convert.ToString(text)))
					{
						return;
					}
					if (text.Equals("SAL_OEMBOM") && !e.SourceFormId.Equals("SAL_OEMBOM"))
					{
						throw new Exception(ResManager.LoadKDString("已关联受托加工材料清单,选单时不允许选其他单据类型单据。", "004023000019113", 5, new object[0]));
					}
					if (text.Equals("SAL_SaleOrder") && e.SourceFormId.Equals("SAL_OEMBOM"))
					{
						throw new Exception(ResManager.LoadKDString("已关联销售订单,选单时不允许选受托加工材料清单。", "004023030039316", 5, new object[0]));
					}
					if (text.Equals("STK_OEMReceive") && e.SourceFormId.Equals("SAL_OEMBOM"))
					{
						throw new Exception(ResManager.LoadKDString("已关联受托加工材料收料单,选单时不允许选受托加工材料清单。", "00444711000019116", 5, new object[0]));
					}
				}
			}
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x000421CC File Offset: 0x000403CC
		private void LockStockField()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
				bool flag = Convert.ToBoolean(((DynamicObjectCollection)dynamicObject["MaterialStock"])[0]["IsBatchManage"]);
				bool flag2 = Convert.ToBoolean(((DynamicObjectCollection)dynamicObject["MaterialStock"])[0]["IsKFPeriod"]);
				string text = Convert.ToString(((DynamicObjectCollection)dynamicObject["MaterialPlan"])[0]["PlanMode"]);
				if (flag && (DynamicObject)base.View.Model.GetValue("FLot", i) != null)
				{
					base.View.GetFieldEditor("FLot", i).Enabled = false;
				}
				if (flag2)
				{
					base.View.GetFieldEditor("FProduceDate", i).Enabled = false;
					base.View.GetFieldEditor("FExpiryDate", i).Enabled = false;
				}
				if (SCMCommon.CheckMaterialIsEnableBom(dynamicObject) && (DynamicObject)base.View.Model.GetValue("FBomId", i) != null)
				{
					base.View.GetFieldEditor("FBomId", i).Enabled = false;
				}
				if (text.Equals("1") || (text.Equals("2") && base.View.Model.GetValue("FMtoNo", i) != null && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(base.View.Model.GetValue("FMtoNo", i).ToString())))
				{
					base.View.GetFieldEditor("FMtoNo", i).Enabled = false;
				}
			}
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x000423A4 File Offset: 0x000405A4
		private void LockMaterialField()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				this.LockMaterialField(i);
			}
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x000423DC File Offset: 0x000405DC
		private void LockMaterialField(int row)
		{
			if (Convert.ToInt64(base.View.Model.GetValue("FSrcEntryId", row)) > 0L)
			{
				base.View.GetFieldEditor("FMaterialId", row).Enabled = false;
				base.View.GetFieldEditor("FCustMatId", row).Enabled = false;
			}
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x00042438 File Offset: 0x00040638
		private void SetDefaultBOM()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FSubHeadEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				base.View.Model.SetValue("FSelect", "True", i);
				if (!(base.View.Model.GetValue("FSubBomId", i) is DynamicObject))
				{
					DynamicObject dynamicObject = base.View.Model.GetValue("FSubMaterialId", i) as DynamicObject;
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId", i) as DynamicObject;
					if (dynamicObject != null && dynamicObject2 != null)
					{
						BaseDataField baseDataField = (BaseDataField)base.View.BillBusinessInfo.GetField("FSubMaterialId");
						int num = Convert.ToInt32(((DynamicObjectCollection)dynamicObject["MaterialBase"])[0]["ErpClsID"]);
						Enums.Enu_BOMUse enu_BOMUse = num;
						long defaultBomKey = MFGServiceHelperForSCM.GetDefaultBomKey(base.Context, Convert.ToInt64(dynamicObject["msterID"]), Convert.ToInt64(dynamicObject2["Id"]), 0L, enu_BOMUse);
						base.View.Model.SetValue("FSubBomId", defaultBomKey, i);
					}
				}
			}
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x00042588 File Offset: 0x00040788
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSRCBILLNO":
				this.ShowSrcOrderList(e.Row);
				break;
			case "FMATERIALID":
			case "FSTOCKID":
			case "FEXTAUXUNITID":
			case "FCUSTMATID":
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
			case "FSTOCKSTATUSID":
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
			case "FSTOCKERID":
			case "FSUBBOMID":
			case "FSTOCKERGROUPID":
			{
				string text;
				if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
				{
					e.ListFilterParameter.Filter = (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter) ? text : (e.ListFilterParameter.Filter + "AND" + text));
				}
				break;
			}
			}
			base.BeforeF7Select(e);
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x00042788 File Offset: 0x00040988
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
			case "FSUBBOMID":
			case "FSTOCKERGROUPID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					e.Filter = (string.IsNullOrWhiteSpace(e.Filter) ? text : (e.Filter + "AND" + text));
				}
				break;
			}
			}
			base.BeforeSetItemValueByNumber(e);
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x00042934 File Offset: 0x00040B34
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FSTOCKERID":
				Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
				break;
			case "FMATERIALID":
			{
				long num2 = 0L;
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					num2 = Convert.ToInt64(dynamicObject["Id"]);
				}
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMATERIALID", e.Row) as DynamicObject;
				base.View.Model.SetValue("FBOMID", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num2, false), e.Row);
				long materialId = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
				string text = (dynamicObject3 != null) ? Convert.ToString(dynamicObject3["Id"]) : "";
				bool flag = base.View.Session.ContainsKey("StockQueryFormId") && base.View.Session["StockQueryFormId"] != null && StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Session["StockQueryFormId"]), base.View.BillBusinessInfo.GetForm().Id);
				if ((flag && ObjectUtils.IsNullOrEmptyOrWhiteSpace(text)) || (!flag && !this.associatedCopyEntryRow && !this.copyEntryRow))
				{
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FCustId") as DynamicObject;
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					long customerId = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
					long saleOrgId = (dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]);
					if (this.para_UseCustMatMapping)
					{
						Common.SetRelativeCodeByMaterialId(this, "FCustMatId", materialId, customerId, saleOrgId, e.Row);
					}
				}
				break;
			}
			case "FAUXPROPID":
			{
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
				break;
			}
			case "FBASEQTY":
			case "FSECQTY":
				this.SynBackQty(e, Convert.ToDecimal(e.NewValue) - Convert.ToDecimal(e.OldValue));
				break;
			case "FSRCENTRYID":
				if (Convert.ToInt64(e.NewValue) > 0L)
				{
					string text2 = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
					if ((StringUtils.EqualsIgnoreCase(text2, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(text2, "SAL_OEMBOM")) && base.View.Model.GetValue("FMATERIALID", e.Row) != null)
					{
						this.LockMaterialField(e.Row);
					}
				}
				break;
			case "FCUSTID":
				if (this.para_UseCustMatMapping)
				{
					Common.SetCustMatWhenCustChange(this, "FBillEntry", "FStockOrgId", "FCUSTID", "FCUSTMATID", "FCustMatName");
				}
				break;
			case "FCUSTMATID":
				if (!this.copyEntryRow && !this.associatedCopyEntryRow)
				{
					CustomerMaterialMappingArgs customerMaterialMappingArgs = new CustomerMaterialMappingArgs();
					DynamicObject dynamicObject6 = base.View.Model.GetValue("FCustMatId", e.Row) as DynamicObject;
					customerMaterialMappingArgs.CustMatId = ((dynamicObject6 == null) ? "" : dynamicObject6["Id"].ToString());
					DynamicObject dynamicObject7 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					customerMaterialMappingArgs.MainOrgId = ((dynamicObject7 == null) ? 0L : Convert.ToInt64(dynamicObject7["Id"]));
					customerMaterialMappingArgs.NeedOrgCtrl = this._baseDataOrgCtl["BD_MATERIAL"];
					customerMaterialMappingArgs.MaterialIdKey = "FMaterialId";
					customerMaterialMappingArgs.AuxpropIdKey = "FAuxpropId";
					customerMaterialMappingArgs.Row = e.Row;
					Common.SetMaterialIdAndAuxpropIdByCustMatId(this, customerMaterialMappingArgs);
				}
				break;
			}
			base.DataChanged(e);
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x00042E0C File Offset: 0x0004100C
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x00042E70 File Offset: 0x00041070
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x00042EDC File Offset: 0x000410DC
		private void SynBackQty(DataChangedEventArgs e, decimal value)
		{
			long srcEntryId = Convert.ToInt64(base.View.Model.GetValue("FSrcEntryId", e.Row));
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if ((StringUtils.EqualsIgnoreCase(text, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM")) && srcEntryId > 0L)
			{
				Entity entity = this.Model.BusinessInfo.GetEntity("FSubHeadEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
				DynamicObject dynamicObject = (from p in entityDataObject
				where Convert.ToInt64(p["SubSrcEntryId"]) == srcEntryId
				orderby Convert.ToDecimal(p["SubBaseQty"]) descending
				select p).FirstOrDefault<DynamicObject>();
				int num = (dynamicObject == null) ? 0 : Convert.ToInt32(dynamicObject["Seq"]);
				if (num > 0)
				{
					string text2 = Convert.ToString(base.View.Model.GetValue("FInStockType", e.Row));
					string text3 = StringUtils.EqualsIgnoreCase(e.Field.Key, "FSECQTY") ? "FSubAuxQty" : "FSubBaseQty";
					string a;
					if ((a = text2) != null)
					{
						if (!(a == "QuaInStock"))
						{
							if (!(a == "ConInStock"))
							{
								if (a == "UnQuaInStock")
								{
									text3 = (StringUtils.EqualsIgnoreCase(e.Field.Key, "FSECQTY") ? "FRefuseAuxQty" : "FRefuseBaseQty");
								}
							}
							else
							{
								text3 = (StringUtils.EqualsIgnoreCase(e.Field.Key, "FSECQTY") ? "FCsnReceiveAuxQty" : "FCsnReceiveBaseQty");
							}
						}
						else
						{
							text3 = (StringUtils.EqualsIgnoreCase(e.Field.Key, "FSECQTY") ? "FReceiveAuxQty" : "FReceiveBaseQty");
						}
					}
					object value2 = base.View.Model.GetValue(text3, num - 1);
					base.View.Model.SetValue(text3, Convert.ToDecimal(value2) + value, num - 1);
					base.View.InvokeFieldUpdateService(text3, num - 1);
				}
			}
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x00043178 File Offset: 0x00041378
		private void DeleteHeadEntryRow(int row)
		{
			long srcEntryId = Convert.ToInt64(base.View.Model.GetValue("FSrcEntryId", row));
			string text = Convert.ToString(base.View.Model.GetValue("FInStockType", row));
			string text2 = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if ((StringUtils.EqualsIgnoreCase(text2, "STK_OEMReceive") || StringUtils.EqualsIgnoreCase(text2, "SAL_OEMBOM")) && srcEntryId > 0L)
			{
				Entity entity = this.Model.BusinessInfo.GetEntity("FSubHeadEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
				Entity entity2 = this.Model.BusinessInfo.GetEntity("FBillEntry");
				DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entity2);
				List<int> list = (from p in entityDataObject2
				where Convert.ToInt64(p["SrcEntryId"]) == srcEntryId
				select Convert.ToInt32(p["Seq"]) - 1).ToList<int>();
				if (list != null && list.Count<int>() > 0 && list.Contains(row))
				{
					list.Remove(row);
				}
				DynamicObject dynamicObject = (from p in entityDataObject
				where Convert.ToInt64(p["SubSrcEntryId"]) == srcEntryId
				select p).FirstOrDefault<DynamicObject>();
				int num = (dynamicObject == null) ? 0 : Convert.ToInt32(dynamicObject["Seq"]);
				string text3 = "FSubBaseQty";
				string text4 = "FSubAuxQty";
				string a;
				if ((a = text) != null)
				{
					if (!(a == "QuaInStock"))
					{
						if (!(a == "ConInStock"))
						{
							if (a == "UnQuaInStock")
							{
								text3 = "FRefuseBaseQty";
								text4 = "FRefuseAuxQty";
							}
						}
						else
						{
							text3 = "FCsnReceiveBaseQty";
							text4 = "FCsnReceiveAuxQty";
						}
					}
					else
					{
						text3 = "FReceiveBaseQty";
						text4 = "FReceiveAuxQty";
					}
				}
				if (list == null || list.Count<int>() <= 0)
				{
					this.Model.DeleteEntryRow("FSubHeadEntity", num - 1);
				}
				else
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FBaseUnitId", row) as DynamicObject;
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FSecUnitId", row) as DynamicObject;
					bool flag = true;
					decimal num2 = Convert.ToDecimal(base.View.Model.GetValue("FBaseQty", row));
					decimal num3 = Convert.ToDecimal(base.View.Model.GetValue("FSecQty", row));
					if (dynamicObject2 != null && dynamicObject3 != null && dynamicObject4 != null && Convert.ToInt64(dynamicObject3["Id"]) != Convert.ToInt64(dynamicObject4["Id"]))
					{
						UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
						{
							MasterId = Convert.ToInt64(dynamicObject2[FormConst.MASTER_ID]),
							MaterialId = Convert.ToInt64(dynamicObject2["Id"]),
							SourceUnitId = Convert.ToInt64(dynamicObject3["Id"]),
							DestUnitId = Convert.ToInt64(dynamicObject4["Id"])
						});
						DynamicObject dynamicObject5 = ((DynamicObjectCollection)dynamicObject2["MaterialStock"])[0];
						if (unitConvertRate != null && dynamicObject5 != null)
						{
							flag = false;
							if (unitConvertRate.ConvertType == 1 && !Convert.ToString(dynamicObject5["UnitConvertDir"]).Equals("2"))
							{
								if (num2 != 0m)
								{
									base.View.Model.SetValue(text3, Convert.ToDecimal(base.View.Model.GetValue(text3, num - 1)) - num2, num - 1);
									base.View.InvokeFieldUpdateService(text3, num - 1);
								}
								if (num3 != 0m)
								{
									base.View.Model.SetValue(text4, Convert.ToDecimal(base.View.Model.GetValue(text4, num - 1)) - num3, num - 1);
								}
							}
							else
							{
								if (num3 != 0m)
								{
									base.View.Model.SetValue(text4, Convert.ToDecimal(base.View.Model.GetValue(text4, num - 1)) - num3, num - 1);
								}
								if (num2 != 0m)
								{
									base.View.Model.SetValue(text3, Convert.ToDecimal(base.View.Model.GetValue(text3, num - 1)) - num2, num - 1);
									base.View.InvokeFieldUpdateService(text3, num - 1);
								}
							}
						}
					}
					if (flag && num2 != 0m)
					{
						base.View.Model.SetValue(text3, Convert.ToDecimal(base.View.Model.GetValue(text3, num - 1)) - num2, num - 1);
						base.View.InvokeFieldUpdateService(text3, num - 1);
					}
				}
				base.View.UpdateView("FSubHeadEntity");
			}
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x000437F8 File Offset: 0x000419F8
		private void ShowSrcOrderList(int row)
		{
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			List<DynamicObject> list = (from p in dynamicObjectCollection
			where Convert.ToInt64(p["SubMaterialId_Id"]) != 0L
			select p).ToList<DynamicObject>();
			if (list == null || list.Count<DynamicObject>() < 1)
			{
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "STK_SelectOrder";
			dynamicFormShowParameter.SyncCallBackAction = true;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			string key = "SessionOrderKey";
			string text = "SessionOrderValue";
			dynamicFormShowParameter.CustomParams.Add(key, text);
			base.View.Session[text] = dynamicObjectCollection;
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData != null)
				{
					DynamicObject dynamicObject = (DynamicObject)result.ReturnData;
					Convert.ToDecimal(this.View.Model.GetValue("FRecAdvanceAmount", row));
					if (dynamicObject == null)
					{
						return;
					}
					this.View.Model.SetValue("FSrcBillNo", dynamicObject["OrderNo"], row);
					this.View.Model.SetValue("FSrcSeq", dynamicObject["OrderSeq"], row);
					this.View.Model.SetValue("FSrcEntryId", dynamicObject["SrcEntryId"], row);
				}
			});
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x000438F8 File Offset: 0x00041AF8
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

		// Token: 0x06000570 RID: 1392 RVA: 0x000439EC File Offset: 0x00041BEC
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

		// Token: 0x06000571 RID: 1393 RVA: 0x00043AD8 File Offset: 0x00041CD8
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

		// Token: 0x06000572 RID: 1394 RVA: 0x00043B67 File Offset: 0x00041D67
		private void SetStockerGroup(OEMInStock oemInStock)
		{
			Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x00043B80 File Offset: 0x00041D80
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

		// Token: 0x06000574 RID: 1396 RVA: 0x00043D88 File Offset: 0x00041F88
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

		// Token: 0x06000575 RID: 1397 RVA: 0x00043E18 File Offset: 0x00042018
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
				if (!(a == "FSTOCKERID"))
				{
					if (!(a == "FSTOCKERGROUPID"))
					{
						if (a == "FSUBBOMID")
						{
							DynamicObject dynamicObject = base.View.Model.GetValue("FSubMaterialId", row) as DynamicObject;
							if (dynamicObject != null)
							{
								DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialBase"] as DynamicObjectCollection;
								DynamicObject dynamicObject2 = dynamicObjectCollection[0];
								int num = Convert.ToInt32(dynamicObject2["ErpClsID"]);
								long num2 = Convert.ToInt64(dynamicObject["Id"]);
								filter += string.Format(" FMATERIALID = {0} AND (FBOMUSE='{1}' OR FBOMUSE='99') ", num2, num);
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
					long num3 = (dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["Id"]);
					if (0L != num3)
					{
						filter = filter + "And FOPERATORGROUPID = " + num3.ToString();
					}
				}
			}
			return true;
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x00043FD0 File Offset: 0x000421D0
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

		// Token: 0x04000206 RID: 518
		private bool associatedCopyEntryRow;

		// Token: 0x04000207 RID: 519
		private long lastAuxpropId;

		// Token: 0x04000208 RID: 520
		private bool para_UseCustMatMapping;

		// Token: 0x04000209 RID: 521
		private bool copyEntryRow;

		// Token: 0x0400020A RID: 522
		private Dictionary<string, bool> _baseDataOrgCtl = new Dictionary<string, bool>();
	}
}
