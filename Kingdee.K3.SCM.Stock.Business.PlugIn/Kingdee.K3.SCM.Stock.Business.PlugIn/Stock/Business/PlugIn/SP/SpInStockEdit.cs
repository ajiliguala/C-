using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper.SP;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x02000080 RID: 128
	[Description("简单生产入库单-表单插件")]
	public class SpInStockEdit : AbstractBillPlugIn
	{
		// Token: 0x060005BC RID: 1468 RVA: 0x0004642C File Offset: 0x0004462C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_Push)
			{
				e.Option.SetVariableValue("IsExpandVirtualMtrl", true);
				e.Option.SetVariableValue("IsExpandPurchaseMtrl", false);
				e.Option.SetVariableValue("FilterString", string.Empty);
				if (!this.isPushValidated)
				{
					e.Cancel = !this.CanDoPushPickBill();
					return;
				}
				this.isPushValidated = false;
			}
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x000464B8 File Offset: 0x000446B8
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 12 && e.Bills is List<ConvertBillElement>)
			{
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				list.Add(new ConvertBillElement
				{
					FormID = "SP_PickMtrl",
					ConvertBillType = 0,
					Name = new LocaleValue(ResManager.LoadKDString("简单生产领料单", "004023030004282", 5, new object[0]), base.Context.UserLocale.LCID)
				});
				e.Bills = list;
				DynamicObject dynamicObject = this.Model.GetValue("FPrdOrgId") as DynamicObject;
				if (dynamicObject != null)
				{
					e.AddOptionalOrgIds("SP_PickMtrl", new List<long>
					{
						Convert.ToInt64(dynamicObject["Id"])
					});
				}
				e.AddReplaceRelation("ENG_BomExpandBill", "SP_PickMtrl");
			}
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x000465F4 File Offset: 0x000447F4
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 12 && e.TargetFormId == "SP_PickMtrl")
			{
				ConvertRuleElement convertRuleElement = ConvertServiceHelper.GetConvertRules(base.Context, "ENG_BomExpandBill", e.TargetFormId).FirstOrDefault<ConvertRuleElement>();
				if (convertRuleElement != null)
				{
					string value = this.GetConLossRate("SP_PickMtrl") ? "FBaseQty" : "FBaseActualQty";
					ConvertFilterPolicyElement convertFilterPolicyElement = (from p in convertRuleElement.Policies
					where p is ConvertFilterPolicyElement
					select p as ConvertFilterPolicyElement).FirstOrDefault<ConvertFilterPolicyElement>();
					convertFilterPolicyElement.AlertMessage = new LocaleValue(ResManager.LoadKDString("1.用户需有入库物料BOM上发料组织下简单生产领料单的新增权限。\r\n2.简单生产入库单领料标识应该为“否”。", "004023030006275", 5, new object[0]), base.Context.LogLocale.LCID);
					List<DefaultConvertPolicyElement> source = (from w in convertRuleElement.Policies
					where w is DefaultConvertPolicyElement
					select w as DefaultConvertPolicyElement).ToList<DefaultConvertPolicyElement>();
					DefaultConvertPolicyElement defaultConvertPolicyElement = source.FirstOrDefault<DefaultConvertPolicyElement>();
					if (defaultConvertPolicyElement != null)
					{
						KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("FStockOrgId", "FSupplyOrgId"),
							new KeyValuePair<string, string>("FPrdOrgId", "fprdorgid_reg"),
							new KeyValuePair<string, string>("FWorkShopId", "fworkshopid_reg"),
							new KeyValuePair<string, string>("FOwnerTypeId0", "fownertypeid_reg"),
							new KeyValuePair<string, string>("FOwnerId0", "fownerid_reg"),
							new KeyValuePair<string, string>("FKeeperId", "FSupplyOrgId"),
							new KeyValuePair<string, string>("FBaseAppQty", value),
							new KeyValuePair<string, string>("FBaseActualQty", value)
						};
						KeyValuePair<string, string>[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							KeyValuePair<string, string> keyValuePair = array2[i];
							FieldMapElement fieldMapElement = defaultConvertPolicyElement.FieldMaps.FirstOrDefault(delegate(FieldMapElement w)
							{
								string targetFieldKey = w.TargetFieldKey;
								KeyValuePair<string, string> keyValuePair2 = keyValuePair;
								return targetFieldKey == keyValuePair2.Key;
							});
							if (fieldMapElement != null)
							{
								FieldMapElement fieldMapElement2 = fieldMapElement;
								KeyValuePair<string, string> keyValuePair3 = keyValuePair;
								fieldMapElement2.SourceFieldKey = keyValuePair3.Value;
							}
						}
					}
					e.Rule = convertRuleElement;
				}
			}
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x000468AC File Offset: 0x00044AAC
		private bool GetConLossRate(string sformid)
		{
			bool result = false;
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, sformid, true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			string text = "SP_PickMtrlUserParameter";
			if (!string.IsNullOrWhiteSpace(formMetadata.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = formMetadata.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata2 = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			if (formMetadata2 == null)
			{
				return result;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata2.BusinessInfo, base.Context.UserId, sformid, "UserParameter");
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
			{
				result = Convert.ToBoolean(dynamicObject["ConLossRate"]);
			}
			return result;
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x0004696C File Offset: 0x00044B6C
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			if (!e.Context.IsMultiOrg && StockServiceHelper.GetUpdateStockDate(e.Context, e.Context.CurrentOrganizationInfo.ID) == null)
			{
				e.CancelMessage = ResManager.LoadKDString("请先在【启用库存管理】中设置库存启用日期,再进行库存业务处理.", "004023030002269", 5, new object[0]);
				e.Cancel = true;
			}
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x000469C8 File Offset: 0x00044BC8
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x000469D1 File Offset: 0x00044BD1
		public override void AfterBindData(EventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
			{
				SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
			}
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x00046A0C File Offset: 0x00044C0C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FOwnerId0"))
				{
					if (key == "FMaterialId")
					{
						long num = 0L;
						DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
						if (dynamicObject != null)
						{
							num = Convert.ToInt64(dynamicObject["Id"]);
						}
						DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
						base.View.Model.SetValue("FBomId", SCMCommon.GetBomDefaultValueByMaterial(base.Context, dynamicObject2, 0L, false, num, false), e.Row);
						return;
					}
					if (key == "FStockerId")
					{
						Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
						return;
					}
					if (!(key == "FAuxpropId"))
					{
						return;
					}
					DynamicObject newAuxpropData = e.OldValue as DynamicObject;
					this.AuxpropDataChanged(newAuxpropData, e.Row);
				}
				else if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
				{
					SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
					return;
				}
			}
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00046B64 File Offset: 0x00044D64
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

		// Token: 0x060005C5 RID: 1477 RVA: 0x00046BBC File Offset: 0x00044DBC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string filter = e.ListFilterParameter.Filter;
			bool f7AndSetNumberEvent = this.GetF7AndSetNumberEvent(e.FieldKey, e.Row, out filter);
			if (f7AndSetNumberEvent)
			{
				e.Cancel = true;
				return;
			}
			if (!string.IsNullOrWhiteSpace(filter))
			{
				e.ListFilterParameter.Filter = Common.SqlAppendAnd(e.ListFilterParameter.Filter, filter);
			}
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00046C20 File Offset: 0x00044E20
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string filter = e.Filter;
			this.GetF7AndSetNumberEvent(e.BaseDataFieldKey, e.Row, out filter);
			if (!string.IsNullOrWhiteSpace(filter))
			{
				e.Filter = Common.SqlAppendAnd(e.Filter, filter);
			}
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00046C6C File Offset: 0x00044E6C
		private bool GetF7AndSetNumberEvent(string fieldKey, int eRow, out string filter)
		{
			bool flag = false;
			filter = null;
			switch (fieldKey)
			{
			case "FMaterialId":
				filter = " FIsProduce = '1' ";
				break;
			case "FPrdOrgId":
			{
				long value = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null);
				if (value <= 0L)
				{
					flag = true;
					base.View.ShowMessage(ResManager.LoadKDString("请先录入入库组织", "004023030004285", 5, new object[0]), 0);
				}
				break;
			}
			case "FStockStatusId":
				flag = Common.GetStockStatusFilterStr(this, eRow, "FStockId", out filter);
				break;
			case "FStockerId":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FStockerGroupId") as DynamicObject;
				filter += " FIsUse='1' ";
				long num2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				if (num2 != 0L)
				{
					filter = filter + "And FOPERATORGROUPID = " + num2.ToString();
				}
				break;
			}
			case "FStockerGroupId":
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockerId") as DynamicObject;
				filter += " FIsUse='1' ";
				if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L)
				{
					filter += string.Format("And FENTRYID IN (SELECT tod.FOPERATORGROUPID FROM T_BD_OPERATORENTRY toe\r\n                                                INNER JOIN T_BD_OPERATORDETAILS tod ON tod.FENTRYID = toe.FENTRYID\r\n                                                WHERE toe.FENTRYID = {0})", Convert.ToInt64(dynamicObject2["Id"]));
				}
				break;
			}
			case "FExtAuxUnitId":
				filter = SCMCommon.GetAuxUnitFilter(this, "FMaterialId", "FBaseUnitId", "FSecUnitId", eRow);
				break;
			case "FStockOrgId":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			case "FOwnerId0":
				filter = SCMCommon.GetOwnerFilterByRelation(this, new GetOwnerFilterArgs("FOwnerTypeId0", eRow, "FPrdOrgId", eRow, "109", "2"));
				break;
			}
			if (flag)
			{
				filter = " 1 = 0 ";
			}
			return flag;
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00046EC8 File Offset: 0x000450C8
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x00046F2C File Offset: 0x0004512C
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00046FA0 File Offset: 0x000451A0
		private bool CanDoPushPickBill()
		{
			List<string> list = this.ValidatePickedEntrys();
			if (list.Count > 0)
			{
				List<FieldAppearance> list2 = new List<FieldAppearance>();
				FieldAppearance fieldAppearance = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(base.View.Context, "FErrInfo", ResManager.LoadKDString("异常信息", "004023000017458", 5, new object[0]), "", null);
				fieldAppearance.Width = new LocaleValue("500", base.View.Context.UserLocale.LCID);
				list2.Add(fieldAppearance);
				K3DisplayerModel k3DisplayerModel = K3DisplayerModel.Create(base.View.Context, list2.ToArray(), null);
				foreach (string text in list)
				{
					k3DisplayerModel.AddMessage(text);
				}
				k3DisplayerModel.CancelButton.Visible = true;
				k3DisplayerModel.CancelButton.Caption = new LocaleValue(ResManager.LoadKDString("否", "004023000013912", 5, new object[0]));
				k3DisplayerModel.OKButton.Caption = new LocaleValue(ResManager.LoadKDString("是", "004023030005539", 5, new object[0]));
				k3DisplayerModel.OKButton.Visible = true;
				k3DisplayerModel.SummaryMessage = ResManager.LoadKDString("以下简单生产入库单已存在关联领料单，是否继续？", "004023000019013", 5, new object[0]);
				base.View.ShowK3Displayer(k3DisplayerModel, delegate(FormResult o)
				{
					if (o != null && o.ReturnData is K3DisplayerModel && (o.ReturnData as K3DisplayerModel).IsOK)
					{
						this.isPushValidated = true;
						base.View.InvokeFormOperation("Push");
					}
				}, "BOS_K3Displayer");
				return false;
			}
			return true;
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x0004712C File Offset: 0x0004532C
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

		// Token: 0x060005CC RID: 1484 RVA: 0x00047220 File Offset: 0x00045420
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

		// Token: 0x060005CD RID: 1485 RVA: 0x0004730C File Offset: 0x0004550C
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

		// Token: 0x060005CE RID: 1486 RVA: 0x0004739C File Offset: 0x0004559C
		private List<string> ValidatePickedEntrys()
		{
			List<string> list = new List<string>();
			List<long> list2 = new List<long>();
			list2.Add(Convert.ToInt64(this.Model.DataObject["Id"]));
			List<SPInstockPickInfo> pickedInstockEntrys = SpInStockServiceHelper.GetPickedInstockEntrys(base.View.Context, list2, false);
			if (pickedInstockEntrys != null && pickedInstockEntrys.Count > 0)
			{
				string format = ResManager.LoadKDString("{0} 第{1}行 已存在关联领料单！", "004001030006057", 5, new object[0]);
				foreach (SPInstockPickInfo spinstockPickInfo in pickedInstockEntrys)
				{
					list.Add(string.Format(format, spinstockPickInfo.BillNo, spinstockPickInfo.EntrySeq));
				}
			}
			return list;
		}

		// Token: 0x0400022F RID: 559
		private const string SPPickMtrlUserParameter = "SP_PickMtrlUserParameter";

		// Token: 0x04000230 RID: 560
		private string PushEnityKey = "FEntity";

		// Token: 0x04000231 RID: 561
		private bool isPushValidated;

		// Token: 0x04000232 RID: 562
		private long lastAuxpropId;
	}
}
