using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockTranser
{
	// Token: 0x0200005D RID: 93
	[Description("调拨申请单插件-表单插件")]
	public class StockTransferApplyEdit : AbstractBillPlugIn
	{
		// Token: 0x0600040C RID: 1036 RVA: 0x00030B14 File Offset: 0x0002ED14
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00030B1D File Offset: 0x0002ED1D
		public override void AfterBindData(EventArgs e)
		{
			this.SetComValue();
			base.AfterBindData(e);
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x00030B2C File Offset: 0x0002ED2C
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetBusinessTypeByBillType();
				this.SetDefaultValue();
			}
			base.AfterCreateModelData(e);
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x00030B66 File Offset: 0x0002ED66
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetDefOwnerValue(e.Row);
			}
			base.AfterCreateNewEntryRow(e);
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00030B94 File Offset: 0x0002ED94
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			switch (key = e.Field.Key.ToUpperInvariant())
			{
			case "FOWNERTYPEIDHEAD":
			case "FOWNERTYPEINIDHEAD":
				this.SynOwnerType("FOwnerTypeIdHead", "FOwnerTypeId");
				this.SynOwnerType("FOwnerTypeInIdHead", "FOwnerTypeInId");
				return;
			case "FAPPORGID":
			case "FMATERIALID":
			case "FOWNERID":
				break;
			case "FBUSINESSTYPE":
				this.SetComValue();
				break;

				return;
			}
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00030C70 File Offset: 0x0002EE70
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string key;
			switch (key = e.BaseDataFieldKey.ToUpperInvariant())
			{
			case "FSTOCKORGINID":
			case "FSTOCKORGID":
			case "FMATERIALID":
			case "FBOMID":
			case "FSTOCKID":
			case "FSTOCKINID":
			case "FSTOCKSTATUSID":
			case "FSTOCKSTATUSINID":
			{
				string text;
				if (this.GetFieldFilter(e.BaseDataFieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.Filter))
					{
						e.Filter = text;
						return;
					}
					e.Filter = e.Filter + " AND " + text;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00030D80 File Offset: 0x0002EF80
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSTOCKORGINID":
			case "FSTOCKORGID":
			case "FMATERIALID":
			case "FBOMID":
			case "FSTOCKID":
			case "FSTOCKINID":
			case "FSTOCKSTATUSID":
			case "FSTOCKSTATUSINID":
			{
				string text;
				if (this.GetFieldFilter(e.FieldKey, out text, e.Row))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
				}
				break;
			}

				return;
			}
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00030EA0 File Offset: 0x0002F0A0
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 13 && e.Bills is List<ConvertBillElement>)
			{
				long value = BillUtils.GetValue<long>(base.View.Model, "FAPPORGID", -1, 0L, null);
				if (value > 0L && Common.HaveBOMViewPermission(base.Context, value))
				{
					Common.SetBomExpandBillToConvertForm(base.Context, (List<ConvertBillElement>)e.Bills);
				}
			}
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00030F10 File Offset: 0x0002F110
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13 && e.SourceFormId == "ENG_PRODUCTSTRUCTURE")
			{
				List<string> list = new List<string>();
				SelBomBillParam bomExpandBillFieldValue = Common.GetBomExpandBillFieldValue(base.View, "FAPPORGID", "FOwnerTypeIdHead", "");
				if (Common.ValidateBomExpandBillFieldValue(base.View, bomExpandBillFieldValue, list))
				{
					base.View.Session["SelInStockBillParam"] = bomExpandBillFieldValue;
					Common.SetBomExpandConvertRuleinfo(base.Context, base.View, e);
					return;
				}
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("【{0}】 字段为选单必录项！", "004023030004312", 5, new object[0]), string.Join(ResManager.LoadKDString("】,【", "004023030004315", 5, new object[0]), list)), "", 0);
			}
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x000310C8 File Offset: 0x0002F2C8
		public override void OnChangeConvertRuleEnumList(ChangeConvertRuleEnumListEventArgs e)
		{
			base.OnChangeConvertRuleEnumList(e);
			ConvertRuleElement convertRuleElement = e.Convertrules.FirstOrDefault<ConvertRuleElement>();
			if (convertRuleElement != null && convertRuleElement.SourceFormId.Equals("STK_TRANSFERAPPLY") && (convertRuleElement.TargetFormId.Equals("STK_TransferDirect") || convertRuleElement.TargetFormId.Equals("STK_TRANSFEROUT")))
			{
				string text = Convert.ToString(base.View.Model.GetValue("FTransferDirect"));
				string a;
				if ((a = text) != null)
				{
					if (!(a == "GENERAL"))
					{
						if (!(a == "RETURN"))
						{
							return;
						}
						if (convertRuleElement.TargetFormId.Equals("STK_TransferDirect"))
						{
							e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("StkTransferApply-StkTransferDirect") || p.EnumId.Equals("StkTransferApply-StkTransferDirect_R"));
							e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("StkTransferApply-StkTransferDirect") || p.Id.Equals("StkTransferApply-StkTransferDirect_R"));
							return;
						}
						e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT") || p.EnumId.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT_R"));
						e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT") || p.Id.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT_R"));
					}
					else
					{
						if (convertRuleElement.TargetFormId.Equals("STK_TransferDirect"))
						{
							e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("StkTransferApp_R-StkTransferDirect_R"));
							e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("StkTransferApp_R-StkTransferDirect_R"));
							return;
						}
						e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("STK_TRANSFERAPP_R-STK_TRANSFEROUT_R"));
						e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("STK_TRANSFERAPP_R-STK_TRANSFEROUT_R"));
						return;
					}
				}
			}
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x000312DC File Offset: 0x0002F4DC
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				Common.DoBomExpandDraw(base.View, Common.GetBomExpandBillFieldValue(base.View, "FAPPORGID", "", ""));
				base.View.UpdateView("FEntity");
				base.View.Model.DataChanged = true;
			}
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0003137C File Offset: 0x0002F57C
		private void SynOwnerType(string sourfield, string destfield)
		{
			string text = base.View.Model.GetValue(sourfield) as string;
			Field field = base.View.BusinessInfo.GetField(destfield);
			int entryRowCount = base.View.Model.GetEntryRowCount(field.EntityKey);
			for (int i = 0; i < entryRowCount; i++)
			{
				base.View.Model.SetValue(destfield, text, i);
			}
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x000313E8 File Offset: 0x0002F5E8
		private void SetDefOwnerValue(int row)
		{
			DynamicObject dynamicObject = this.Model.GetValue("FAPPORGID") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			base.View.Model.SetValue("FStockOrgId", num, row);
			base.View.Model.SetValue("FStockOrgInId", num, row);
			string text = Convert.ToString(this.Model.GetValue("FOwnerTypeIdHead"));
			string text2 = Convert.ToString(this.Model.GetValue("FOwnerTypeInIdHead"));
			base.View.Model.SetValue("FOwnerTypeId", text, row);
			base.View.Model.SetValue("FOwnerTypeInId", text2, row);
			DynamicObject dynamicObject2 = this.Model.GetValue("FStockOrgId", row) as DynamicObject;
			if (Convert.ToString(this.Model.GetValue("FOwnerTypeId", row)).Equals("BD_OwnerOrg") && dynamicObject2 != null)
			{
				this.Model.SetValue("FOwnerId", Convert.ToInt64(dynamicObject2["Id"]), row);
			}
			DynamicObject dynamicObject3 = this.Model.GetValue("FStockOrgInId", row) as DynamicObject;
			if (Convert.ToString(this.Model.GetValue("FOwnerTypeInId", row)).Equals("BD_OwnerOrg") && dynamicObject3 != null)
			{
				this.Model.SetValue("FOwnerInId", Convert.ToInt64(dynamicObject3["Id"]), row);
			}
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x00031580 File Offset: 0x0002F780
		private void SetDefaultValue()
		{
			DynamicObject dynamicObject = this.Model.GetValue("FAPPORGID") as DynamicObject;
			string text = Convert.ToString(this.Model.GetValue("FOwnerTypeIdHead"));
			string text2 = Convert.ToString(this.Model.GetValue("FOwnerTypeInIdHead"));
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId", i) as DynamicObject;
				if (dynamicObject2 == null || Convert.ToInt64(dynamicObject2["Id"]) == 0L)
				{
					base.View.Model.SetValue("FStockOrgId", num, i);
				}
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FStockOrgInId", i) as DynamicObject;
				if (dynamicObject3 == null || Convert.ToInt64(dynamicObject3["Id"]) == 0L)
				{
					base.View.Model.SetValue("FStockOrgInId", num, i);
				}
				base.View.Model.SetValue("FOwnerTypeId", text, i);
				base.View.Model.SetValue("FOwnerTypeInId", text2, i);
			}
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x000317B4 File Offset: 0x0002F9B4
		private void SetComValue()
		{
			ComboFieldEditor comboFieldEditor = base.View.GetFieldEditor("FTRANSTYPE", 0) as ComboFieldEditor;
			if (comboFieldEditor == null)
			{
				return;
			}
			ComboField comboField = this.Model.BusinessInfo.GetElement("FTRANSTYPE") as ComboField;
			List<EnumItem> list = new List<EnumItem>();
			list = CommonServiceHelper.GetEnumItem(base.Context, (comboField == null) ? "" : comboField.EnumType.ToString());
			(from p in list
			where p.Value == "InnerOrgTransfer"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item = (from p in list
			where p.Value == "OverOrgTransfer"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item2 = (from p in list
			where p.Value == "OverOrgSale"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item3 = (from p in list
			where p.Value == "OverOrgPurchase"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item4 = (from p in list
			where p.Value == "OverOrgSubPick"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item5 = (from p in list
			where p.Value == "OverOrgMisDelivery"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item6 = (from p in list
			where p.Value == "OverOrgPick"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item7 = (from p in list
			where p.Value == "OverOrgPrdIn"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item8 = (from p in list
			where p.Value == "OverOrgPurVMI"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item9 = (from p in list
			where p.Value == "OverOrgPrdOut"
			select p).FirstOrDefault<EnumItem>();
			EnumItem item10 = (from p in list
			where p.Value == "OverOrgSubExConsume"
			select p).FirstOrDefault<EnumItem>();
			if (!base.Context.IsMultiOrg)
			{
				string value = this.Model.GetValue("FBUSINESSTYPE", 0) as string;
				if (!"VMI".Equals(value))
				{
					list.Remove(item);
				}
				list.Remove(item2);
				list.Remove(item3);
				list.Remove(item5);
				list.Remove(item4);
				list.Remove(item6);
				list.Remove(item7);
				list.Remove(item8);
				list.Remove(item9);
				list.Remove(item10);
				comboFieldEditor.SetComboItems(list);
				return;
			}
			list.Remove(item2);
			list.Remove(item3);
			list.Remove(item5);
			list.Remove(item4);
			list.Remove(item6);
			list.Remove(item7);
			list.Remove(item8);
			list.Remove(item9);
			list.Remove(item10);
			comboFieldEditor.SetComboItems(list);
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x00031ADC File Offset: 0x0002FCDC
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillTypeID");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "STK_TransApplyBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBusinessType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00031B2C File Offset: 0x0002FD2C
		private bool GetFieldFilter(string fieldKey, out string filter, int row)
		{
			filter = "";
			string text = string.Empty;
			if (string.IsNullOrWhiteSpace(fieldKey))
			{
				return false;
			}
			string key;
			switch (key = fieldKey.ToUpperInvariant())
			{
			case "FSTOCKORGINID":
			case "FSTOCKORGID":
				filter = " EXISTS (SELECT 1 FROM T_BAS_SYSTEMPROFILE T2 WHERE T2.FORGID = FORGID AND T2.FCATEGORY='STK' AND T2.FKEY='STARTSTOCKDATE' )";
				break;
			case "FMATERIALID":
				filter = " FIsInventory = '1'";
				text = (base.View.Model.GetValue("FBusinessType") as string);
				if (StringUtils.EqualsIgnoreCase(text, "VMI"))
				{
					filter += " and FIsVmiBusiness = '1' ";
				}
				break;
			case "FBOMID":
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID", row) as DynamicObject;
				if (dynamicObject != null)
				{
					int num2 = Convert.ToInt32(((DynamicObjectCollection)dynamicObject["MaterialBase"])[0]["ErpClsID"]);
					if (num2 == 2 || num2 == 3)
					{
						filter = " FBOMCATEGORY = '1' AND FBOMUSE IN ('99','2') ";
					}
					else
					{
						filter = " FBOMCATEGORY = '1' AND FBOMUSE = '99' ";
					}
				}
				break;
			}
			case "FSTOCKSTATUSID":
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockId", row) as DynamicObject;
				if (dynamicObject2 != null)
				{
					List<SelectorItemInfo> list = new List<SelectorItemInfo>();
					list.Add(new SelectorItemInfo("FStockStatusType"));
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "BD_STOCK",
						FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject2["Id"]),
						SelectItems = list
					};
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
					string text2 = "";
					if (dynamicObjectCollection != null)
					{
						DynamicObject dynamicObject3 = dynamicObjectCollection[0];
						text2 = Convert.ToString(dynamicObject3["FStockStatusType"]);
					}
					if (!string.IsNullOrWhiteSpace(text2))
					{
						text2 = "'" + text2.Replace(",", "','") + "'";
						filter = string.Format(" FType IN ({0})", text2);
					}
				}
				break;
			}
			case "FSTOCKSTATUSINID":
			{
				DynamicObject dynamicObject4 = base.View.Model.GetValue("FStockInId", row) as DynamicObject;
				if (dynamicObject4 != null)
				{
					List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
					list2.Add(new SelectorItemInfo("FStockStatusType"));
					QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
					{
						FormId = "BD_STOCK",
						FilterClauseWihtKey = string.Format("FStockId={0}", dynamicObject4["Id"]),
						SelectItems = list2
					};
					DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter2, null);
					string text3 = "";
					if (dynamicObjectCollection2 != null)
					{
						DynamicObject dynamicObject5 = dynamicObjectCollection2[0];
						text3 = Convert.ToString(dynamicObject5["FStockStatusType"]);
					}
					if (!string.IsNullOrWhiteSpace(text3))
					{
						text3 = "'" + text3.Replace(",", "','") + "'";
						filter = string.Format(" FType IN ({0})", text3);
					}
				}
				break;
			}
			}
			return !string.IsNullOrWhiteSpace(filter);
		}
	}
}
