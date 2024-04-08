using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Common.BusinessEntity.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000018 RID: 24
	[Description("受托加工收料单 表单插件")]
	public class OEMReceiveEdit : AbstractBillPlugIn
	{
		// Token: 0x060000B5 RID: 181 RVA: 0x00009B64 File Offset: 0x00007D64
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.View.RuleContainer.AddPluginRule<OEMReceive>("FBillHead", 1, new Action<OEMReceive>(this.SetStockerGroup), new string[]
			{
				"FReceiverId"
			});
			this._baseDataOrgCtl = Common.GetSalBaseDataCtrolType(base.View.Context);
			base.OnInitialize(e);
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00009BC4 File Offset: 0x00007DC4
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBBOMEXPAND")
				{
					OEMReceiveEdit.BomExpand(this);
					base.View.UpdateView("FBillEntry");
					return;
				}
				if (!(a == "TBEND") && !(a == "TBUNEND"))
				{
					return;
				}
				base.View.UpdateView("FBillEntry");
			}
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00009C30 File Offset: 0x00007E30
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			if (e.EntityKey.ToUpperInvariant() == "FSUBHEADENTITY")
			{
				this.DeleteOemReceiveEntryRow(e.Row);
				return;
			}
			if (e.EntityKey.ToUpperInvariant() == "FBILLENTRY")
			{
				this.DeleteHeadEntryRow(e.Row);
			}
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00009C84 File Offset: 0x00007E84
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

		// Token: 0x060000B9 RID: 185 RVA: 0x00009CE0 File Offset: 0x00007EE0
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

		// Token: 0x060000BA RID: 186 RVA: 0x00009D80 File Offset: 0x00007F80
		private void DeleteOemReceiveEntryRow(int row)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["OEMReceiveEntry"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = base.View.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			if (dynamicObjectCollection == null || !dynamicObjectCollection.Any<DynamicObject>())
			{
				return;
			}
			long lRow = Convert.ToInt64(base.View.Model.GetValue("FSubSrcEntryId", row));
			if (lRow == 0L)
			{
				return;
			}
			if (dynamicObjectCollection2 != null)
			{
				int srcSeqNo = Convert.ToInt32(dynamicObjectCollection2[row]["Seq"]);
				List<DynamicObject> list = (from p in dynamicObjectCollection
				where Convert.ToInt64(p["SrcEntryId"]) == lRow && Convert.ToInt32(p["SrcSeqNo"]) == srcSeqNo
				select p).ToList<DynamicObject>();
				if (!list.Any<DynamicObject>())
				{
					return;
				}
				foreach (DynamicObject dynamicObject in list)
				{
					this.Model.DeleteEntryRow("FBillEntry", Convert.ToInt32(dynamicObject["Seq"]) - 1);
				}
			}
			base.View.UpdateView("FBillEntry");
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00009ED0 File Offset: 0x000080D0
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null)
			{
				if (base.View.OpenParameter.CreateFrom != 1)
				{
					long baseDataLongValue = SCMCommon.GetBaseDataLongValue(this, "FStockOrgId", -1);
					if (baseDataLongValue > 0L)
					{
						SCMCommon.SetOpertorIdByUserId(this, "FReceiverId", "WHY", baseDataLongValue);
						Common.SetGroupValue(this, "FReceiverId", "FStockerGroupId", "WHY");
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
							SCMCommon.SetOpertorIdByUserId(this, "FReceiverId", "WHY", baseDataLongValue2);
							Common.SetGroupValue(this, "FReceiverId", "FStockerGroupId", "WHY");
						}
					}
				}
				this.GetUseCustMatMappingParamater();
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00009FB8 File Offset: 0x000081B8
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.AfterUpdateViewState(e);
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if (StringUtils.EqualsIgnoreCase(text, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM"))
			{
				this.LockMaterialField();
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0000A080 File Offset: 0x00008280
		public override void AfterBindData(EventArgs e)
		{
			string sSubSrcType = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if ((base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom == 1) || base.View.OpenParameter.CreateFrom == 2)
			{
				if (StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_SaleOrder"))
				{
					this.SetDefaultBom();
					OEMReceiveEdit.BomExpand(this);
					base.View.UpdateView("FBillEntry");
				}
				else if (StringUtils.EqualsIgnoreCase(sSubSrcType, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_OEMBOM"))
				{
					base.View.GetMainBarItem("tbBomExpand").Visible = false;
					this.SetDefaultNeedCheck();
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
			if (StringUtils.EqualsIgnoreCase(sSubSrcType, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(sSubSrcType, "SAL_OEMBOM"))
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

		// Token: 0x060000BE RID: 190 RVA: 0x0000A548 File Offset: 0x00008748
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if (StringUtils.EqualsIgnoreCase(text, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM"))
			{
				OEMReceiveEdit.<>c__DisplayClass16 CS$<>8__locals1 = new OEMReceiveEdit.<>c__DisplayClass16();
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

		// Token: 0x060000BF RID: 191 RVA: 0x0000A6E4 File Offset: 0x000088E4
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
					if (text.Equals("STK_OEMInStockRETURN") && e.SourceFormId.Equals("SAL_OEMBOM"))
					{
						throw new Exception(ResManager.LoadKDString("已关联受托加工材料退料单,选单时不允许选受托加工材料清单。", "004023000019115", 5, new object[0]));
					}
				}
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0000A84C File Offset: 0x00008A4C
		private void LockMaterialField()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				this.LockMaterialField(i);
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000A884 File Offset: 0x00008A84
		private void LockMaterialField(int row)
		{
			if (Convert.ToInt64(base.View.Model.GetValue("FSrcEntryId", row)) > 0L)
			{
				base.View.GetFieldEditor("FMaterialId", row).Enabled = false;
				base.View.GetFieldEditor("FCustMatId", row).Enabled = false;
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000A8E0 File Offset: 0x00008AE0
		private void SetDefaultNeedCheck()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
				if (dynamicObject != null)
				{
					bool flag = Convert.ToBoolean(((DynamicObjectCollection)dynamicObject["MaterialQM"])[0]["CheckEntrusted"]);
					if (flag)
					{
						base.View.Model.SetValue("FNeedCheck", "True", i);
					}
				}
			}
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000A974 File Offset: 0x00008B74
		private void SetDefaultBom()
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
						int num = Convert.ToInt32(((DynamicObjectCollection)dynamicObject["MaterialBase"])[0]["ErpClsID"]);
						Enums.Enu_BOMUse enu_BOMUse = num;
						long defaultBomKey = MFGServiceHelperForSCM.GetDefaultBomKey(base.Context, Convert.ToInt64(dynamicObject["msterID"]), Convert.ToInt64(dynamicObject2["Id"]), 0L, enu_BOMUse);
						base.View.Model.SetValue("FSubBomId", defaultBomKey, i);
					}
				}
			}
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000AAA0 File Offset: 0x00008CA0
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
			case "FRECEIVERID":
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

		// Token: 0x060000C5 RID: 197 RVA: 0x0000ACA0 File Offset: 0x00008EA0
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
			case "FRECEIVERID":
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

		// Token: 0x060000C6 RID: 198 RVA: 0x0000AE4C File Offset: 0x0000904C
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				string msg;
				if (a == "FACTRECEIVEQTY")
				{
					msg = ResManager.LoadKDString("实收数量不能大于实到数量！", "004023000024605", 5, new object[0]);
					this.ValidateQtyRange("FActlandQty", "FACTRECEIVEQTY", e, 2, msg);
					return;
				}
				if (a == "FREJECTQTY")
				{
					msg = ResManager.LoadKDString("拒收数量不能大于实到数量！", "004023000024606", 5, new object[0]);
					this.ValidateQtyRange("FActlandQty", "FREJECTQTY", e, 0, msg);
					return;
				}
				if (a == "FEXTAUXUNITQTY")
				{
					msg = ResManager.LoadKDString("实收数量(辅单位)不能大于实到数量(辅单位)！", "004023000024607", 5, new object[0]);
					this.ValidateQtyRange("FACTLANDSECQTY", "FEXTAUXUNITQTY", e, 1, msg);
					return;
				}
				if (!(a == "FREJECTSECQTY"))
				{
					return;
				}
				msg = ResManager.LoadKDString("拒收数量(辅单位)不能大于实到数量(辅单位)！", "004023000024608", 5, new object[0]);
				this.ValidateQtyRange("FACTLANDSECQTY", "FREJECTSECQTY", e, 0, msg);
			}
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x0000AF54 File Offset: 0x00009154
		protected long SetUnitConvertValue(BeforeUpdateValueEventArgs e, int convertDirection, long maxQty, long inputValue)
		{
			if (convertDirection < 1)
			{
				return 0L;
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
			long materialId = 0L;
			if (dynamicObject != null)
			{
				materialId = Convert.ToInt64(dynamicObject["Id"]);
			}
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FUnitID", e.Row) as DynamicObject;
			long num = 0L;
			if (dynamicObject2 != null)
			{
				num = Convert.ToInt64(dynamicObject2["Id"]);
			}
			DynamicObject dynamicObject3 = base.View.Model.GetValue("FExtAuxUnitId", e.Row) as DynamicObject;
			long num2 = 0L;
			if (dynamicObject3 != null)
			{
				num2 = Convert.ToInt64(dynamicObject3["Id"]);
			}
			if (convertDirection == 1)
			{
				decimal baseqty = Convert.ToDecimal(base.View.Model.GetValue("FACTRECEIVEQTY", e.Row));
				decimal destUnitQty = this.GetDestUnitQty(materialId, num, num2, baseqty);
				if (destUnitQty <= maxQty)
				{
					return 0L;
				}
				base.View.Model.SetValue("FEXTAUXUNITQTY", maxQty, e.Row);
				base.View.UpdateView("FBillEntry");
				return maxQty;
			}
			else
			{
				if (convertDirection != 2)
				{
					return 0L;
				}
				long value = Convert.ToInt64(base.View.Model.GetValue("FEXTAUXUNITQTY", e.Row));
				decimal destUnitQty = this.GetDestUnitQty(materialId, num2, num, value);
				if (destUnitQty <= maxQty)
				{
					return 0L;
				}
				base.View.Model.SetValue("FACTRECEIVEQTY", maxQty, e.Row);
				base.View.UpdateView("FBillEntry");
				return maxQty;
			}
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000B118 File Offset: 0x00009318
		public void ValidateQtyRange(string maxValueKey, string validateKey, BeforeUpdateValueEventArgs e, int convertDirection, string msg)
		{
			long num = Convert.ToInt64(base.View.Model.GetValue(maxValueKey, e.Row));
			long num2 = Convert.ToInt64(e.Value);
			if (num2 > num)
			{
				e.Cancel = true;
				base.View.Model.SetValue(validateKey, num, e.Row);
				num2 = num;
			}
			if (num2 <= num)
			{
				return;
			}
			base.View.ShowErrMessage("", msg, 0);
			e.Cancel = true;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x0000B198 File Offset: 0x00009398
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FRECEIVERID":
				Common.SetGroupValue(this, "FReceiverId", "FStockerGroupId", "WHY");
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
			case "FAUXUNITQTY":
				this.SynBackQty(e, Convert.ToDecimal(e.NewValue) - Convert.ToDecimal(e.OldValue));
				break;
			case "FSRCENTRYID":
				if (Convert.ToInt64(e.NewValue) > 0L)
				{
					string text2 = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
					if ((StringUtils.EqualsIgnoreCase(text2, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(text2, "SAL_OEMBOM")) && base.View.Model.GetValue("FMATERIALID", e.Row) != null)
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

		// Token: 0x060000CA RID: 202 RVA: 0x0000B670 File Offset: 0x00009870
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this._lastAuxPropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060000CB RID: 203 RVA: 0x0000B6D4 File Offset: 0x000098D4
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060000CC RID: 204 RVA: 0x0000B740 File Offset: 0x00009940
		private void SynBackQty(DataChangedEventArgs e, decimal value)
		{
			long srcEntryId = Convert.ToInt64(base.View.Model.GetValue("FSrcEntryId", e.Row));
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if ((StringUtils.EqualsIgnoreCase(text, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM")) && srcEntryId > 0L)
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
					string text2 = StringUtils.EqualsIgnoreCase(e.Field.Key, "FBASEQTY") ? "FSubBaseQTY" : "FSubAUXQTY";
					object value2 = base.View.Model.GetValue(text2, num - 1);
					base.View.Model.SetValue(text2, Convert.ToDecimal(value2) + value, num - 1);
					if (text2.Equals("FSubBaseQTY"))
					{
						base.View.InvokeFieldUpdateService(text2, num - 1);
					}
				}
			}
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000B8D0 File Offset: 0x00009AD0
		private decimal GetDestUnitQty(long materialId, long sourceUnitId, long destUnitId, decimal baseqty)
		{
			GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs
			{
				MaterialId = materialId,
				DestUnitId = destUnitId,
				SourceUnitId = sourceUnitId
			};
			UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, getUnitConvertRateArgs);
			return unitConvertRate.ConvertQty(baseqty, "");
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000B964 File Offset: 0x00009B64
		private void DeleteHeadEntryRow(int row)
		{
			long srcEntryId = Convert.ToInt64(base.View.Model.GetValue("FSrcEntryId", row));
			string text = Convert.ToString(base.View.Model.GetValue("FSubSRCTYPE", 0));
			if ((StringUtils.EqualsIgnoreCase(text, "STK_OEMInStockRETURN") || StringUtils.EqualsIgnoreCase(text, "SAL_OEMBOM")) && srcEntryId > 0L)
			{
				Entity entity = this.Model.BusinessInfo.GetEntity("FSubHeadEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
				DynamicObject dynamicObject = (from p in entityDataObject
				where Convert.ToInt64(p["SubSrcEntryId"]) == srcEntryId
				select p).FirstOrDefault<DynamicObject>();
				int num = (dynamicObject == null) ? 0 : Convert.ToInt32(dynamicObject["Seq"]);
				Entity entity2 = this.Model.BusinessInfo.GetEntity("FBillEntry");
				DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entity2);
				List<int> list = (from p in entityDataObject2
				where Convert.ToInt64(p["SrcEntryId"]) == srcEntryId
				select Convert.ToInt32(p["Seq"]) - 1).ToList<int>();
				if (list != null && list.Count<int>() > 0 && list.Contains(row))
				{
					list.Remove(row);
				}
				if (list == null || list.Count<int>() <= 0)
				{
					this.Model.DeleteEntryRow("FSubHeadEntity", num - 1);
				}
				else
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", row) as DynamicObject;
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FBaseUnitId", row) as DynamicObject;
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FAuxUnitId", row) as DynamicObject;
					bool flag = true;
					string text2 = "FSubBaseQty";
					string text3 = "FSubAuxQty";
					decimal num2 = Convert.ToDecimal(base.View.Model.GetValue("FBaseQty", row));
					decimal num3 = Convert.ToDecimal(base.View.Model.GetValue("FAuxUnitQty", row));
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
									base.View.Model.SetValue(text2, Convert.ToDecimal(base.View.Model.GetValue(text2, num - 1)) - num2, num - 1);
									base.View.InvokeFieldUpdateService(text2, num - 1);
								}
								if (num3 != 0m)
								{
									base.View.Model.SetValue(text3, Convert.ToDecimal(base.View.Model.GetValue(text3, num - 1)) - num3, num - 1);
								}
							}
							else
							{
								if (num3 != 0m)
								{
									base.View.Model.SetValue(text3, Convert.ToDecimal(base.View.Model.GetValue(text3, num - 1)) - num3, num - 1);
								}
								if (num2 != 0m)
								{
									base.View.Model.SetValue(text2, Convert.ToDecimal(base.View.Model.GetValue(text2, num - 1)) - num2, num - 1);
									base.View.InvokeFieldUpdateService(text2, num - 1);
								}
							}
						}
					}
					if (flag && num2 != 0m)
					{
						base.View.Model.SetValue(text2, Convert.ToDecimal(base.View.Model.GetValue(text2, num - 1)) - num2, num - 1);
						base.View.InvokeFieldUpdateService(text2, num - 1);
					}
				}
				base.View.UpdateView("FSubHeadEntity");
			}
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0000BF40 File Offset: 0x0000A140
		private void ShowSrcOrderList(int row)
		{
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			if (dynamicObjectCollection != null)
			{
				List<DynamicObject> source = (from p in dynamicObjectCollection
				where Convert.ToInt64(p["SubMaterialId_Id"]) != 0L
				select p).ToList<DynamicObject>();
				if (!source.Any<DynamicObject>())
				{
					return;
				}
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "STK_SelectOrder",
				SyncCallBackAction = true,
				ParentPageId = base.View.PageId,
				PageId = SequentialGuid.NewGuid().ToString()
			};
			string key = "SessionOrderKey";
			string text = "SessionOrderValue";
			dynamicFormShowParameter.CustomParams.Add(key, text);
			base.View.Session[text] = dynamicObjectCollection;
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData == null)
				{
					return;
				}
				DynamicObject dynamicObject = (DynamicObject)result.ReturnData;
				if (dynamicObject == null)
				{
					return;
				}
				this.View.Model.SetValue("FSrcBillNo", dynamicObject["OrderNo"], row);
				this.View.Model.SetValue("FSrcSeq", dynamicObject["OrderSeq"], row);
				this.View.Model.SetValue("FSrcEntryId", dynamicObject["SrcEntryId"], row);
			});
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0000C048 File Offset: 0x0000A248
		private void AuxpropDataChanged(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			if (num == this._lastAuxPropId)
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
			this._lastAuxPropId = num;
			base.View.UpdateView("FBillEntry", row);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0000C134 File Offset: 0x0000A334
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

		// Token: 0x060000D2 RID: 210 RVA: 0x0000C1C3 File Offset: 0x0000A3C3
		private void SetStockerGroup(OEMReceive oemReceive)
		{
			Common.SetGroupValue(this, "FReceiverId", "FStockerGroupId", "WHY");
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0000C1DC File Offset: 0x0000A3DC
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
						filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FAuxUnitId", row);
					}
				}
				else
				{
					DynamicObject dynamicObject4 = base.View.Model.GetValue("FSTOCKSTATUSID", row) as DynamicObject;
					string arg = (dynamicObject4 == null) ? "" : Convert.ToString(dynamicObject4["Number"]);
					List<SelectorItemInfo> selectItems = new List<SelectorItemInfo>
					{
						new SelectorItemInfo("FType")
					};
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "BD_StockStatus",
						FilterClauseWihtKey = string.Format("FNumber='{0}'", arg),
						SelectItems = selectItems
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

		// Token: 0x060000D4 RID: 212 RVA: 0x0000C3E4 File Offset: 0x0000A5E4
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

		// Token: 0x060000D5 RID: 213 RVA: 0x0000C474 File Offset: 0x0000A674
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
					if (!(a == "FRECEIVERID"))
					{
						if (!(a == "FSTOCKERGROUPID"))
						{
							if (a == "FSUBBOMID")
							{
								DynamicObject dynamicObject = base.View.Model.GetValue("FSubMaterialId", row) as DynamicObject;
								if (dynamicObject != null)
								{
									DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialBase"] as DynamicObjectCollection;
									if (dynamicObjectCollection != null)
									{
										DynamicObject dynamicObject2 = dynamicObjectCollection[0];
										int num = Convert.ToInt32(dynamicObject2["ErpClsID"]);
										long num2 = Convert.ToInt64(dynamicObject["Id"]);
										filter += string.Format(" FMATERIALID = {0} AND (FBOMUSE='{1}' OR FBOMUSE='99') ", num2, num);
									}
								}
							}
						}
						else
						{
							DynamicObject dynamicObject3 = base.View.Model.GetValue("FReceiverId") as DynamicObject;
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
				else
				{
					DynamicObject dynamicObject5 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
					filter = "FUseOrgId = " + ((dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"])).ToString();
				}
			}
			return true;
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x0000C694 File Offset: 0x0000A894
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

		// Token: 0x060000D7 RID: 215 RVA: 0x0000C7DC File Offset: 0x0000A9DC
		public static void BomExpand(AbstractBillPlugIn billplugin)
		{
			MemBomExpandOption memBomExpandOption = new MemBomExpandOption();
			memBomExpandOption.ExpandLevelTo = OEMReceiveEdit.GetExpandLevel(billplugin);
			memBomExpandOption.ValidDate = new DateTime?(TimeServiceHelper.GetSystemDateTime(billplugin.Context).Date);
			memBomExpandOption.IsConvertUnitQty = true;
			memBomExpandOption.BomExpandId = Guid.NewGuid().ToString();
			memBomExpandOption.CsdSubstitution = OEMReceiveEdit.IsBOMExpendCarryCsdSub;
			List<DynamicObject> bomSourceData = OEMReceiveEdit.GetBomSourceData(memBomExpandOption, billplugin);
			if (bomSourceData == null || bomSourceData.Count<DynamicObject>() < 1)
			{
				return;
			}
			DynamicObject dynamicObject = MFGServiceHelperForSCM.ExpandBomForward(billplugin.Context, bomSourceData, memBomExpandOption);
			DynamicObject dynamicObject2 = billplugin.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			long ownerId = 0L;
			if (dynamicObject2 != null)
			{
				ownerId = Convert.ToInt64(dynamicObject2["Id"]);
			}
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
								DynamicObject dynamicObject3 = dyCurRow["MATERIALID"] as DynamicObject;
								if ((dynamicObject3 == null || ((!OEMReceiveEdit.IsBOMExpendCarryMat || !Convert.ToString(dyCurRow["ISSUETYPE"]).Equals("7")) && Convert.ToBoolean(((DynamicObjectCollection)dynamicObject3["MaterialBase"])[0]["IsInventory"]))) && !Convert.ToString(dyCurRow["MaterialType"]).Equals("2"))
								{
									if (Convert.ToString(dyCurRow["MaterialType"]).Equals("3"))
									{
										dyCurRow["ACCUDISASSMBLERATE"] = 0;
										dyCurRow["BASEQTY"] = 0;
										dyCurRow["BaseActualQty"] = 0;
									}
									DynamicObject dynamicObject4 = list.FirstOrDefault((DynamicObject p) => Convert.ToInt64(p["MATERIALID_Id"]) == Convert.ToInt64(dyCurRow["MATERIALID_Id"]) && Convert.ToInt32(p["SRCSEQNO"]) == Convert.ToInt32(dyCurRow["SRCSEQNO"]) && Convert.ToString(p["OWNERTYPEID"]) == Convert.ToString(dyCurRow["OWNERTYPEID"]) && Convert.ToInt64(p["OWNERID_Id"]) == Convert.ToInt64(dyCurRow["OWNERID_Id"]));
									if (dynamicObject4 == null)
									{
										dyCurRow["DISASSMBLERATE"] = dyCurRow["ACCUDISASSMBLERATE"];
										list.Add(dyCurRow);
									}
									else
									{
										if (Convert.ToInt32(dynamicObject4["BOMLEVEL"]) < Convert.ToInt32(dyCurRow["BOMLEVEL"]))
										{
											dynamicObject4["BOMLEVEL"] = dyCurRow["BOMLEVEL"];
										}
										dynamicObject4["BASEQTY"] = Convert.ToDecimal(dynamicObject4["BASEQTY"]) + Convert.ToDecimal(dyCurRow["BASEQTY"]);
										dynamicObject4["QTY"] = Convert.ToDecimal(dynamicObject4["QTY"]) + Convert.ToDecimal(dyCurRow["QTY"]);
										dynamicObject4["BaseActualQty"] = Convert.ToDecimal(dynamicObject4["BaseActualQty"]) + Convert.ToDecimal(dyCurRow["BaseActualQty"]);
										dynamicObject4["DISASSMBLERATE"] = Convert.ToDecimal(dynamicObject4["DISASSMBLERATE"]) + Convert.ToDecimal(dyCurRow["ACCUDISASSMBLERATE"]);
									}
								}
							}
						}
					}
				}
				OEMReceiveEdit.UpdateSubEntity(OEMReceiveEdit.UpdateUnitId(billplugin.Context, list), billplugin, ownerId);
			}
			MFGServiceHelperForSCM.ClearBomExpandResult(billplugin.Context, memBomExpandOption);
			int entryRowCount = billplugin.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = entryRowCount - 1; i >= 0; i--)
			{
				if (!(billplugin.View.Model.GetValue("FMaterialId", i) is DynamicObject))
				{
					billplugin.View.Model.DeleteEntryRow("FBillEntry", i);
				}
			}
			DynamicObjectCollection dynamicObjectCollection2 = billplugin.View.Model.DataObject["OEMReceiveEntry"] as DynamicObjectCollection;
			for (int j = 0; j < dynamicObjectCollection2.Count<DynamicObject>(); j++)
			{
				dynamicObjectCollection2[j]["Seq"] = j + 1;
			}
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0000CCEC File Offset: 0x0000AEEC
		private static int GetExpandLevel(AbstractBillPlugIn billplugin)
		{
			int result = 1;
			string id = billplugin.View.BusinessInfo.GetForm().Id;
			string text = string.Empty;
			if (!string.IsNullOrWhiteSpace(billplugin.View.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = billplugin.View.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(billplugin.Context, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(billplugin.Context, formMetadata.BusinessInfo, billplugin.Context.UserId, id, "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
			{
				OEMReceiveEdit.IsConLossRate = Convert.ToBoolean(dynamicObject["ConLossRate"]);
			}
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("BOMExpendCarryMat"))
			{
				OEMReceiveEdit.IsBOMExpendCarryMat = Convert.ToBoolean(dynamicObject["BOMExpendCarryMat"]);
			}
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("BOMExpendCarryCsdSub"))
			{
				OEMReceiveEdit.IsBOMExpendCarryCsdSub = Convert.ToBoolean(dynamicObject["BOMExpendCarryCsdSub"]);
			}
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("OrderCarryMto"))
			{
				OEMReceiveEdit.IsOrderCarryMto = Convert.ToBoolean(dynamicObject["OrderCarryMto"]);
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

		// Token: 0x060000D9 RID: 217 RVA: 0x0000CF04 File Offset: 0x0000B104
		private static List<DynamicObject> GetBomSourceData(MemBomExpandOption bomQueryOption, AbstractBillPlugIn billplugin)
		{
			DynamicObjectCollection dynamicObjectCollection = billplugin.View.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			List<DynamicObject> list = new List<DynamicObject>();
			int num = (from p in dynamicObjectCollection
			where p["SubBomId"] == null
			select p).Count<DynamicObject>();
			if (num == dynamicObjectCollection.Count<DynamicObject>())
			{
				billplugin.View.ShowMessage(ResManager.LoadKDString("请选择BOM版本", "004023000012229", 5, new object[0]), 0);
			}
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject["SubBomId"] != null && dynamicObject["SubMaterialId"] != null && Convert.ToBoolean(dynamicObject["Select"]))
				{
					int num2 = Convert.ToInt32(dynamicObject["Seq"]);
					long materialId_Id = Convert.ToInt64((dynamicObject["SubMaterialId"] as DynamicObject)["Id"]);
					long bomId_Id = Convert.ToInt64((dynamicObject["SubBomId"] as DynamicObject)["Id"]);
					decimal needQty = Convert.ToDecimal(dynamicObject["SubBaseQty"]);
					long srcEntryId = Convert.ToInt64(dynamicObject["Id"]);
					BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
					bomForwardSourceDynamicRow.MaterialId_Id = materialId_Id;
					bomForwardSourceDynamicRow.BomId_Id = bomId_Id;
					bomForwardSourceDynamicRow.NeedQty = needQty;
					bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
					bomForwardSourceDynamicRow.UnitId_Id = Convert.ToInt64((dynamicObject["SubUnitID"] as DynamicObject)["Id"]);
					bomForwardSourceDynamicRow.SrcInterId = Convert.ToInt64(billplugin.View.Model.GetPKValue());
					bomForwardSourceDynamicRow.SrcEntryId = srcEntryId;
					bomForwardSourceDynamicRow.SrcSeqNo = (long)num2;
					list.Add(bomForwardSourceDynamicRow.DataEntity);
				}
			}
			return list;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0000D134 File Offset: 0x0000B334
		public static List<DynamicObject> UpdateUnitId(Context ctx, List<DynamicObject> result)
		{
			Dictionary<long, long> unitIdByMaterilId = OEMReceiveEdit.GetUnitIdByMaterilId(ctx, (from p in result
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
				if (!OEMReceiveEdit.IsConLossRate)
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
						dynamicObject["QTY"] = unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseActualQty"]), "");
					}
				}
				else if (unitConvertRate.ConvertType == 1 && unitIdByMaterilId.Keys.Contains(Convert.ToInt64(dynamicObject["MATERIALID_Id"])) && Convert.ToInt64(dynamicObject["UNITID_Id"]) != unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])])
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
						dynamicObject["QTY"] = ((!OEMReceiveEdit.IsConLossRate) ? unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseActualQty"]), "") : unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseQty"]), ""));
					}
				}
			}
			return result;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000D43C File Offset: 0x0000B63C
		public static Dictionary<long, long> GetUnitIdByMaterilId(Context ctx, List<long> materialids)
		{
			if (materialids == null || materialids.Count<long>() < 1)
			{
				return null;
			}
			return BDCommonServiceHelper.GetUnitIdByMaterilId(ctx, materialids);
		}

		// Token: 0x060000DC RID: 220 RVA: 0x0000D54C File Offset: 0x0000B74C
		private static void UpdateSubEntity(List<DynamicObject> result, AbstractBillPlugIn billplugin, long OwnerId)
		{
			DynamicObjectCollection dynamicObjectCollection = billplugin.View.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			DynamicObjectType dynamicObjectType = billplugin.View.BusinessInfo.GetEntity("FBillEntry").DynamicObjectType;
			billplugin.Model.BusinessInfo.GetEntity("FBillEntry");
			billplugin.View.Model.GetValue("FOwnerIdHead");
			int num = 0;
			using (IEnumerator<DynamicObject> enumerator = dynamicObjectCollection.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					OEMReceiveEdit.<>c__DisplayClass49 CS$<>8__locals2 = new OEMReceiveEdit.<>c__DisplayClass49();
					CS$<>8__locals2.entryRow = enumerator.Current;
					int seq = Convert.ToInt32(CS$<>8__locals2.entryRow["Seq"]);
					List<DynamicObject> list = (from p in result
					where Convert.ToInt32(p["SRCSEQNO"]) == seq && Convert.ToString(p["OWNERTYPEID"]) == "BD_Customer" && (Convert.ToInt64(p["OWNERID_Id"]) == OwnerId || Convert.ToInt64(p["OWNERID_Id"]) == 0L) && Convert.ToInt32(p["BOMLEVEL"]) != 0
					select p).ToList<DynamicObject>();
					int count = list.Count;
					DynamicObjectCollection dynamicObjectCollection2 = billplugin.View.Model.DataObject["OEMReceiveEntry"] as DynamicObjectCollection;
					List<DynamicObject> list2 = (from p in dynamicObjectCollection2
					where Convert.ToInt64(p["SrcEntryId"]) == Convert.ToInt64(CS$<>8__locals2.entryRow["SubSrcEntryId"]) && Convert.ToInt32(p["SrcSeqNo"]) == seq
					select p).ToList<DynamicObject>();
					if (list2 != null)
					{
						foreach (DynamicObject item in list2)
						{
							dynamicObjectCollection2.Remove(item);
						}
					}
					if (count >= 1)
					{
						billplugin.Model.BatchCreateNewEntryRow("FBillEntry", count);
						foreach (DynamicObject item2 in list)
						{
							if (OEMReceiveEdit.UpdateSubRowData(item2, billplugin, num, CS$<>8__locals2.entryRow))
							{
								num++;
							}
						}
						if (list2 != null)
						{
							for (int i = count - 1; i > num; i--)
							{
								list2.RemoveAt(i);
							}
						}
					}
				}
			}
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000D7CC File Offset: 0x0000B9CC
		private static bool UpdateSubRowData(DynamicObject item, AbstractBillPlugIn billplugin, int row, DynamicObject dyRow)
		{
			billplugin.Model.SetValue("FMaterialID", item["MATERIALID_Id"], row);
			DynamicObject dynamicObject = billplugin.Model.GetValue("FMaterialID", row) as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
			{
				if (item["MATERIALID"] == null || !DynamicObjectUtils.Contains((DynamicObject)item["MATERIALID"], "Number"))
				{
					return false;
				}
				billplugin.Model.SetItemValueByNumber("FMaterialID", Convert.ToString(((DynamicObject)item["MATERIALID"])["Number"]), row);
				dynamicObject = (billplugin.Model.GetValue("FMaterialID", row) as DynamicObject);
				if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
				{
					return false;
				}
			}
			billplugin.Model.SetValue("FSrcSeqNo", Convert.ToString(dyRow["Seq"]), row);
			billplugin.Model.SetValue("FSrcSeq", Convert.ToString(dyRow["SubSrcSeq"]), row);
			billplugin.Model.SetValue("FSrcBillNo", Convert.ToString(dyRow["SubSRCBILLNO"]), row);
			billplugin.Model.SetValue("FSrcEntryId", Convert.ToString(dyRow["SubSrcEntryId"]), row);
			billplugin.Model.SetValue("FReSrcSeq", Convert.ToString(dyRow["SubSrcSeq"]), row);
			billplugin.Model.SetValue("FReSrcBillNo", Convert.ToString(dyRow["SubSRCBILLNO"]), row);
			billplugin.View.InvokeFieldUpdateService("FMaterialID", row);
			billplugin.Model.SetValue("FUnitID", item["UNITID_Id"], row);
			billplugin.Model.SetValue("FBaseUnitID", item["BASEUNITID_Id"], row);
			billplugin.Model.SetValue("FActlandQty", item["QTY"], row);
			billplugin.View.InvokeFieldUpdateService("FActlandQty", row);
			DynamicObject dynamicObject2 = billplugin.Model.GetValue("FStockOrgId", row) as DynamicObject;
			long num = 0L;
			if (dynamicObject2 != null)
			{
				num = Convert.ToInt64(dynamicObject2["Id"]);
			}
			billplugin.Model.SetValue("FAuxPropId", item["AuxPropId_Id"], row);
			DynamicObjectCollection source = dynamicObject["MaterialInvPty"] as DynamicObjectCollection;
			DynamicObject dynamicObject3 = source.SingleOrDefault((DynamicObject p) => Convert.ToBoolean(p["IsEnable"]) && Convert.ToInt64(p["InvPtyId_Id"]) == 10003L);
			if (dynamicObject3 != null)
			{
				long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(billplugin.Context, dynamicObject, Convert.ToInt64(item["AuxPropId_Id"]), false, num, false);
				billplugin.Model.SetValue("FBomID", bomDefaultValueByMaterial, row);
			}
			if (OEMReceiveEdit.IsOrderCarryMto)
			{
				string text = Convert.ToString(((DynamicObjectCollection)dynamicObject["MaterialPlan"])[0]["PlanMode"]);
				if (text.Equals("1") || text.Equals("2"))
				{
					billplugin.Model.SetValue("FMtoNo", Convert.ToString(dyRow["SubMtoNo"]), row);
				}
			}
			billplugin.Model.SetValue("FKeeperID", dynamicObject2, row);
			return true;
		}

		// Token: 0x04000048 RID: 72
		private long _lastAuxPropId;

		// Token: 0x04000049 RID: 73
		private static bool IsConLossRate;

		// Token: 0x0400004A RID: 74
		private static bool IsBOMExpendCarryMat;

		// Token: 0x0400004B RID: 75
		private static bool IsBOMExpendCarryCsdSub;

		// Token: 0x0400004C RID: 76
		private static bool IsOrderCarryMto;

		// Token: 0x0400004D RID: 77
		private bool para_UseCustMatMapping;

		// Token: 0x0400004E RID: 78
		private bool associatedCopyEntryRow;

		// Token: 0x0400004F RID: 79
		private bool copyEntryRow;

		// Token: 0x04000050 RID: 80
		private Dictionary<string, bool> _baseDataOrgCtl = new Dictionary<string, bool>();
	}
}
