using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Validation;
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
	// Token: 0x02000083 RID: 131
	public class SpReturnMtrlEdit : AbstractBillPlugIn
	{
		// Token: 0x0600062A RID: 1578 RVA: 0x0004B448 File Offset: 0x00049648
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			if (!e.Context.IsMultiOrg && StockServiceHelper.GetUpdateStockDate(e.Context, e.Context.CurrentOrganizationInfo.ID) == null)
			{
				e.CancelMessage = ResManager.LoadKDString("请先在【启用库存管理】中设置库存启用日期,再进行库存业务处理.", "004023030002269", 5, new object[0]);
				e.Cancel = true;
			}
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x0004B4A4 File Offset: 0x000496A4
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetDefKeeperTypeAndKeeperValue(e.Row);
			}
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x0004B4DC File Offset: 0x000496DC
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			List<ConvertBillElement> list;
			if (e.Bills != null)
			{
				list = (e.Bills as List<ConvertBillElement>);
			}
			else
			{
				list = new List<ConvertBillElement>();
			}
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation != 13)
			{
				if (convertOperation == 26 && list.Count > 0)
				{
					ConvertBillElement convertBillElement = list.FirstOrDefault((ConvertBillElement o) => StringUtils.EqualsIgnoreCase(o.FormID, "ENG_BomExpandBill"));
					if (convertBillElement != null)
					{
						list.Remove(convertBillElement);
					}
				}
			}
			else
			{
				list.Add(new ConvertBillElement
				{
					FormID = "ENG_PRODUCTSTRUCTURE",
					ConvertBillType = 0,
					Name = new LocaleValue(ResManager.LoadKDString("物料清单", "004023030004291", 5, new object[0]), base.Context.UserLocale.LCID)
				});
			}
			e.Bills = list;
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0004B5B8 File Offset: 0x000497B8
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13 && (e.SourceFormId == "ENG_PRODUCTSTRUCTURE" || e.SourceFormId == "SP_PickMtrl"))
			{
				ConvertRuleElement convertRuleElement = ConvertServiceHelper.GetConvertRules(base.Context, (e.SourceFormId == "ENG_PRODUCTSTRUCTURE") ? "ENG_BomExpandBill" : e.SourceFormId, e.TargetFormId).FirstOrDefault<ConvertRuleElement>();
				SelInStockBillParam value = new SelInStockBillParam();
				value = this.PrepareSelInStockBillParam();
				if (convertRuleElement != null && e.SourceFormId == "ENG_PRODUCTSTRUCTURE")
				{
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.FormId = e.SourceFormId;
					dynamicFormShowParameter.OpenStyle.ShowType = 5;
					dynamicFormShowParameter.CustomParams["ShowMode"] = 3.ToString();
					dynamicFormShowParameter.ParentPageId = base.View.PageId;
					e.DynamicFormShowParameter = dynamicFormShowParameter;
					base.View.Session["SelInStockBillParam"] = value;
					this.SetParentOwnerFieldMap(convertRuleElement);
				}
				e.Rule = convertRuleElement;
			}
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x0004B6E0 File Offset: 0x000498E0
		private void SetParentOwnerFieldMap(ConvertRuleElement bom2RetRule)
		{
			if (bom2RetRule != null)
			{
				string sourceFieldKey = this.GetConLossRate() ? "FBaseQty" : "FBaseActualQty";
				List<DefaultConvertPolicyElement> source = (from w in bom2RetRule.Policies
				where w is DefaultConvertPolicyElement
				select w as DefaultConvertPolicyElement).ToList<DefaultConvertPolicyElement>();
				if (source.Count<DefaultConvertPolicyElement>() >= 1)
				{
					DefaultConvertPolicyElement defaultConvertPolicyElement = source.First<DefaultConvertPolicyElement>();
					bool flag = false;
					bool flag2 = false;
					foreach (FieldMapElement fieldMapElement in defaultConvertPolicyElement.FieldMaps)
					{
						if (StringUtils.EqualsIgnoreCase(fieldMapElement.TargetFieldKey, "FBaseQty"))
						{
							flag = true;
							fieldMapElement.SourceFieldKey = sourceFieldKey;
						}
						else if (StringUtils.EqualsIgnoreCase(fieldMapElement.TargetFieldKey, "FBaseAppQty"))
						{
							flag2 = true;
							fieldMapElement.SourceFieldKey = sourceFieldKey;
						}
						if (flag && flag2)
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0004B7FC File Offset: 0x000499FC
		private bool GetConLossRate()
		{
			bool result = false;
			string text = "SP_ReturnMtrlUserParameter";
			if (!string.IsNullOrWhiteSpace(base.View.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = base.View.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, base.View.BusinessInfo.GetForm().Id, "UserParameter");
			if (dynamicObject != null || dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
			{
				result = Convert.ToBoolean(dynamicObject["ConLossRate"]);
			}
			return result;
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0004B8C0 File Offset: 0x00049AC0
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FOwnerId0")
				{
					if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
					{
						SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
					}
					string keeperTypeAndKeeper = Convert.ToString(e.NewValue);
					this.SetKeeperTypeAndKeeper(keeperTypeAndKeeper);
					return;
				}
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
					this.SetDefOwnerAndKeeperValue(e.Row);
					return;
				}
				if (key == "FStockerId")
				{
					Common.SetGroupValue(this, "FStockerId", "FStockerGroupId", "WHY");
					return;
				}
				if (key == "FOwnerTypeId0")
				{
					Common.SynOwnerType(this, "FOwnerTypeId0", "FOwnerTypeId");
					return;
				}
				if (key == "FKeeperTypeId")
				{
					string newKeeperTypeId = Convert.ToString(e.NewValue);
					this.SetKeeperValue(newKeeperTypeId, e.Row);
					return;
				}
				if (!(key == "FAuxPropId"))
				{
					return;
				}
				DynamicObject newAuxpropData = e.OldValue as DynamicObject;
				this.AuxpropDataChanged(newAuxpropData, e.Row);
			}
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0004BA85 File Offset: 0x00049C85
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetBusinessTypeByBillType();
			}
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0004BAB4 File Offset: 0x00049CB4
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			base.AfterCopyRow(e);
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["Entity"] as DynamicObjectCollection;
			DynamicObject dynamicObject = dynamicObjectCollection[e.NewRow];
			string text = BillUtils.GetDynamicObjectItemValue<string>(dynamicObject, "SrcBillType", "").ToString();
			if (text.Equals("ENG_BOM") || string.IsNullOrEmpty(text))
			{
				FieldEditor fieldEditor = base.View.GetFieldEditor("FMaterialId", -1);
				FieldAppearance fieldAppearance = (FieldAppearance)fieldEditor.ControlAppearance;
				base.View.StyleManager.SetEnabled(fieldAppearance, dynamicObject, "", true);
				FieldEditor fieldEditor2 = base.View.GetFieldEditor("FUnitID", -1);
				FieldAppearance fieldAppearance2 = (FieldAppearance)fieldEditor2.ControlAppearance;
				base.View.StyleManager.SetEnabled(fieldAppearance2, dynamicObject, "", true);
			}
			if (text.Equals("SP_PickMtrl"))
			{
				FieldEditor fieldEditor3 = base.View.GetFieldEditor("FUnitID", -1);
				FieldAppearance fieldAppearance3 = (FieldAppearance)fieldEditor3.ControlAppearance;
				base.View.StyleManager.SetEnabled(fieldAppearance3, dynamicObject, "", false);
				FieldEditor fieldEditor4 = base.View.GetFieldEditor("FMaterialId", -1);
				FieldAppearance fieldAppearance4 = (FieldAppearance)fieldEditor4.ControlAppearance;
				base.View.StyleManager.SetEnabled(fieldAppearance4, dynamicObject, "", false);
			}
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x0004BC0E File Offset: 0x00049E0E
		public override void BeforeBindData(EventArgs e)
		{
			this.LockOrgAndOwner();
			this.LockKPTypeAndKPAfterSave();
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x0004BC1C File Offset: 0x00049E1C
		public override void AfterBindData(EventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
			{
				SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SP_ReturnMtrl", true) as FormMetadata;
			if (formMetadata == null)
			{
				return;
			}
			string text = "SP_ReturnMtrlUserParameter";
			if (!string.IsNullOrWhiteSpace(formMetadata.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = formMetadata.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata2 = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata2.BusinessInfo, base.Context.UserId, "SP_ReturnMtrl", "UserParameter");
			bool dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<bool>(dynamicObject, "IsSetPicker", false);
			if (dynamicObjectItemValue)
			{
				this.SetPickerId();
				return;
			}
			this.SetStockerId();
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0004BD04 File Offset: 0x00049F04
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

		// Token: 0x06000636 RID: 1590 RVA: 0x0004BD68 File Offset: 0x00049F68
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			base.AuthPermissionBeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FBOMID"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0004BDA0 File Offset: 0x00049FA0
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

		// Token: 0x06000638 RID: 1592 RVA: 0x0004BDEC File Offset: 0x00049FEC
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

		// Token: 0x06000639 RID: 1593 RVA: 0x0004BE84 File Offset: 0x0004A084
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_Draw)
			{
				SelInStockBillParam billParam = new SelInStockBillParam();
				billParam = this.PrepareSelInStockBillParam();
				if (!this.ValidatePush(billParam))
				{
					e.Cancel = true;
				}
			}
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_Save)
			{
				Dictionary<string, DynamicObjectCollection> dictionary = new Dictionary<string, DynamicObjectCollection>();
				long dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<long>(this.Model.DataObject, "Id", 0L);
				if (dynamicObjectItemValue > 0L)
				{
					DynamicObjectCollection spReturnMtrlEntryData = SpReturnMtrlServiceHelper.GetSpReturnMtrlEntryData(base.View.Context, new List<long>
					{
						dynamicObjectItemValue
					});
					dictionary.Add("BeforeSave", spReturnMtrlEntryData);
				}
				DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
				if (dynamicObjectCollection.Count > 0)
				{
					dictionary.Add("AfterSave", dynamicObjectCollection);
				}
				if (dictionary.Count > 0)
				{
					e.Option.SetVariableValue("DataDirtyEntitysList", dictionary);
				}
			}
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0004BF84 File Offset: 0x0004A184
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				SelBill0ption formSession = Common.GetFormSession<SelBill0ption>(base.View, "returnData", true);
				this.DoSelBills(formSession, "Id", "ENG_BomExpandBill", "SP_ReturnMtrl");
			}
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x0004C014 File Offset: 0x0004A214
		public override void AfterSave(AfterSaveEventArgs e)
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["Entity"];
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				IEnumerable<DynamicObject> enumerable = from w in dynamicObjectCollection
				where BillUtils.GetDynamicObjectItemValue<int>(w, "MaterialId_Id", 0) > 0
				select w;
				if (enumerable.Count<DynamicObject>() > 0)
				{
					foreach (DynamicObject objectEntity in enumerable)
					{
						this.LockDynamicObjectFieldAppearance(objectEntity);
					}
				}
			}
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0004C0B0 File Offset: 0x0004A2B0
		private void SetStockerId()
		{
			if (base.View.OpenParameter.Status == null)
			{
				long num = BillUtils.GetDynamicObjectItemValue<long>(base.View.Model.DataObject, "StockerId_Id", 0L);
				if (num <= 0L)
				{
					long dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<long>(base.View.Model.DataObject, "StockOrgId_Id", 0L);
					num = StaffServiceHelper.GetUserOperatorId(base.Context, base.Context.UserId, dynamicObjectItemValue, "WHY");
					base.View.Model.SetValue("FStockerId", num);
				}
			}
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0004C148 File Offset: 0x0004A348
		private void SetPickerId()
		{
			if (base.View.OpenParameter.Status == null)
			{
				List<DynamicObject> userLinkInfo = SpPickMtrlServiceHelper.GetUserLinkInfo(base.Context, base.Context.UserId);
				if (!ListUtils.IsEmpty<DynamicObject>(userLinkInfo))
				{
					long dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<long>(userLinkInfo.FirstOrDefault<DynamicObject>(), "Fid", 0L);
					this.Model.SetValue("FReturnerId", dynamicObjectItemValue);
				}
			}
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0004C208 File Offset: 0x0004A408
		private void DoSelBills(SelBill0ption selBillOPtion, string EntryId, string UpstreamFormId, string DownstreamFormId)
		{
			IEnumerable<DynamicObject> listSelRows = selBillOPtion.ListSelRows;
			if (listSelRows == null || listSelRows.Count<DynamicObject>() <= 0)
			{
				return;
			}
			int i = 0;
			ListSelectedRow[] source = (from p in listSelRows
			select new ListSelectedRow("0", BillUtils.GetDynamicObjectItemValue<long>(p, "Id", 0L).ToString(), i++, UpstreamFormId)
			{
				EntryEntityKey = "FBomResult"
			}).ToArray<ListSelectedRow>();
			ConvertRuleElement convertRuleElement = Common.SPGetConvertRule(base.View, UpstreamFormId, DownstreamFormId);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("SelInStockBillParam", new SelInStockBillParam
			{
				StockOrgId = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null)
			});
			ConvertOperationResult convertOperationResult = ConvertServiceHelper.Draw(base.View.Context, new DrawArgs(convertRuleElement, base.View.Model.DataObject, source.ToArray<ListSelectedRow>())
			{
				CustomParams = dictionary
			}, null);
			if (!ListUtils.IsEmpty<ValidationErrorInfo>(convertOperationResult.ValidationErrors))
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ValidationErrorInfo validationErrorInfo in convertOperationResult.ValidationErrors)
				{
					if (!string.IsNullOrWhiteSpace(validationErrorInfo.Message))
					{
						stringBuilder.AppendLine(validationErrorInfo.Message);
					}
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder))
				{
					base.View.ShowNotificationMessage(stringBuilder.ToString(), string.Empty, 1);
				}
			}
			base.View.UpdateView("FEntity");
			this.Model.DataChanged = true;
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0004C3A4 File Offset: 0x0004A5A4
		private bool IsExistData()
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["Entity"];
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count<DynamicObject>() <= 0)
			{
				return false;
			}
			IEnumerable<DynamicObject> source = from w in dynamicObjectCollection
			where BillUtils.GetDynamicObjectItemValue<int>(w, "MaterialId_Id", 0) > 0
			select w;
			return source.Count<DynamicObject>() > 0;
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0004C41C File Offset: 0x0004A61C
		private void LockData(bool IsLock)
		{
			base.View.GetFieldEditor("FStockOrgId", -1).Enabled = IsLock;
			base.View.GetFieldEditor("FPrdOrgId", -1).Enabled = IsLock;
			base.View.GetFieldEditor("FOwnerTypeId0", -1).Enabled = IsLock;
			base.View.GetFieldEditor("FOwnerId0", -1).Enabled = IsLock;
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["Entity"];
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				IEnumerable<DynamicObject> enumerable = from w in dynamicObjectCollection
				where BillUtils.GetDynamicObjectItemValue<int>(w, "MaterialId_Id", 0) > 0
				select w;
				if (enumerable.Count<DynamicObject>() > 0)
				{
					foreach (DynamicObject objectEntity in enumerable)
					{
						this.LockDynamicObjectFieldAppearance(objectEntity);
					}
				}
			}
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0004C514 File Offset: 0x0004A714
		private void LockField(string StringField, DynamicObject ObjectEntity, string lockKey, bool islock)
		{
			FieldEditor fieldEditor = base.View.GetFieldEditor(StringField, -1);
			FieldAppearance fieldAppearance = (FieldAppearance)fieldEditor.ControlAppearance;
			base.View.StyleManager.SetEnabled(fieldAppearance, ObjectEntity, lockKey, islock);
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0004C550 File Offset: 0x0004A750
		private bool ValidatePush(SelBomBillParam billParam)
		{
			List<string> list = new List<string>();
			if (billParam.StockOrgId <= 0L)
			{
				list.Add(ResManager.LoadKDString("发料组织", "004023030004297", 5, new object[0]));
			}
			if (billParam.PrdOrgId <= 0L)
			{
				list.Add(ResManager.LoadKDString("生产组织", "004023030004300", 5, new object[0]));
			}
			if (string.IsNullOrWhiteSpace(billParam.OwnerType))
			{
				list.Add(ResManager.LoadKDString("货主类型", "004023030004306", 5, new object[0]));
			}
			if (!string.IsNullOrWhiteSpace(billParam.OwnerType) && StringUtils.EqualsIgnoreCase(billParam.OwnerType, "BD_OwnerOrg") && billParam.OwnerId <= 0L)
			{
				list.Add(ResManager.LoadKDString("货主", "004023030004309", 5, new object[0]));
			}
			if (list.Count > 0)
			{
				base.View.ShowMessage(string.Format(ResManager.LoadKDString("【{0}】 字段为选单必录项！", "004023030004312", 5, new object[0]), string.Join(ResManager.LoadKDString("】,【", "004023030004315", 5, new object[0]), list)), 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0004C66C File Offset: 0x0004A86C
		private SelInStockBillParam PrepareSelInStockBillParam()
		{
			return new SelInStockBillParam
			{
				StockOrgId = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null),
				PrdOrgId = BillUtils.GetValue<long>(this.Model, "FPrdOrgId", -1, 0L, null),
				OwnerType = BillUtils.GetValue<string>(this.Model, "FOwnerTypeId0", -1, "", null),
				OwnerId = BillUtils.GetValue<long>(this.Model, "FOwnerId0", -1, 0L, null),
				WorkShopId = BillUtils.GetValue<long>(this.Model, "FWorkShopId", -1, 0L, null)
			};
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0004C708 File Offset: 0x0004A908
		private bool GetF7AndSetNumberEvent(string fieldKey, int eRow, out string filter)
		{
			bool flag = false;
			filter = null;
			switch (fieldKey)
			{
			case "FKeeperId":
			{
				string value = BillUtils.GetValue<string>(this.Model, "FKeeperTypeId", eRow, string.Empty, null);
				if (string.IsNullOrEmpty(value))
				{
					flag = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("请先录入保管者类型的内容！", "004023030004318", 5, new object[0]), "", 0);
				}
				break;
			}
			case "FPrdOrgId":
			{
				long value2 = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null);
				if (value2 <= 0L)
				{
					flag = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("请先录入收料组织的内容！", "004023030004321", 5, new object[0]), "", 0);
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
			{
				string text = base.View.Model.GetValue("FBizType") as string;
				if (StringUtils.EqualsIgnoreCase(text, "VMI"))
				{
					filter = " FVmiBusiness = '1' ";
				}
				else
				{
					filter = SCMCommon.GetOwnerFilterByRelation(this, new GetOwnerFilterArgs("FOwnerTypeId0", eRow, "FPrdOrgId", eRow, "102", "1"));
				}
				break;
			}
			case "FOwnerId":
			{
				string text2 = base.View.Model.GetValue("FBizType") as string;
				if (StringUtils.EqualsIgnoreCase(text2, "VMI"))
				{
					filter = " FVmiBusiness = '1' ";
				}
				else
				{
					filter = SCMCommon.GetOwnerFilterByRelation(this, new GetOwnerFilterArgs("FOwnerTypeId", eRow, "FPrdOrgId", eRow, "102", "1"));
				}
				break;
			}
			case "FPRODUCTGROUPID":
			{
				DynamicObject dynamicObject3 = base.View.Model.GetValue("FPrdOrgId") as DynamicObject;
				int orgId = (dynamicObject3 == null) ? 0 : Convert.ToInt32(dynamicObject3["Id"]);
				Common.GetProductGroupFieldFilter(orgId, out filter);
				break;
			}
			case "FMaterialId":
			{
				string text3 = base.View.Model.GetValue("FBizType") as string;
				if (StringUtils.EqualsIgnoreCase(text3, "VMI"))
				{
					filter = " FIsVmiBusiness = '1' ";
				}
				break;
			}
			}
			if (flag)
			{
				filter = " 1 = 0 ";
			}
			return flag;
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0004CAFC File Offset: 0x0004ACFC
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0004CB60 File Offset: 0x0004AD60
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x0004CB98 File Offset: 0x0004AD98
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

		// Token: 0x06000648 RID: 1608 RVA: 0x0004CC8C File Offset: 0x0004AE8C
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

		// Token: 0x06000649 RID: 1609 RVA: 0x0004CD78 File Offset: 0x0004AF78
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

		// Token: 0x0600064A RID: 1610 RVA: 0x0004CE08 File Offset: 0x0004B008
		private void LockDynamicObjectFieldAppearance(DynamicObject ObjectEntity)
		{
			Convert.ToInt32(BillUtils.GetDynamicObjectItemValue<int>(ObjectEntity, "Seq", 0));
			string text = BillUtils.GetDynamicObjectItemValue<string>(ObjectEntity, "SrcBillType", "").ToString();
			if (text.Equals("SP_PickMtrl"))
			{
				base.View.GetFieldEditor("FWorkShopId", -1).Enabled = false;
				List<string> lockfieldlist = new List<string>
				{
					"FUnitID",
					"FBomId",
					"FMaterialId",
					"FAuxPropId",
					"FLot",
					"FProductNo"
				};
				this.LockStringList(lockfieldlist, ObjectEntity);
			}
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x0004CEB4 File Offset: 0x0004B0B4
		private void LockStringList(List<string> lockfieldlist, DynamicObject ObjectEntity)
		{
			foreach (string stringField in lockfieldlist)
			{
				this.LockField(stringField, ObjectEntity, "", false);
			}
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0004CF38 File Offset: 0x0004B138
		private void LockOrgAndOwner()
		{
			bool flag = true;
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["Entity"];
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				if ((from w in dynamicObjectCollection
				where BillUtils.GetDynamicObjectItemValue<int>(w, "SrcEntryId", 0) > 0
				select w).Count<DynamicObject>() > 0)
				{
					flag = false;
				}
			}
			BillUtils.LockField(base.View, "FOwnerTypeId0", flag, -1);
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(this.Model.GetValue("FOwnerTypeId0")), "BD_OwnerOrg"))
			{
				BillUtils.LockField(base.View, "FOwnerId0", flag, -1);
			}
			BillUtils.LockField(base.View, "FStockOrgId", flag, -1);
			BillUtils.LockField(base.View, "FPrdOrgId", flag, -1);
			if (!flag)
			{
				if ((from w in dynamicObjectCollection
				where BillUtils.GetDynamicObjectItemValue<string>(w, "SrcBillType", null) == "SP_InStock"
				select w).Count<DynamicObject>() <= 0)
				{
					flag = true;
				}
			}
			BillUtils.LockField(base.View, "FWorkShopId", flag, -1);
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x0004D048 File Offset: 0x0004B248
		private void LockKPTypeAndKPAfterSave()
		{
			object value = base.View.Model.GetValue("FOwnerTypeId0");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FOwnerId", i) as DynamicObject;
				long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
				if (num2 == num && a == "BD_OwnerOrg")
				{
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = true;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = true;
				}
				else
				{
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = false;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = false;
				}
			}
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x0004D194 File Offset: 0x0004B394
		private void SetDefOwnerAndKeeperValue(int row = -1)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FPrdOrgId") as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			int num3 = row;
			int num4 = row;
			if (row == -1)
			{
				num3 = 0;
				num4 = base.View.Model.GetEntryRowCount("FEntity") - 1;
			}
			for (int i = num3; i <= num4; i++)
			{
				object value = this.Model.GetValue("FOwnerTypeId", i);
				DynamicObject dynamicObject3 = this.Model.GetValue("FMaterialId", i) as DynamicObject;
				if (value != null)
				{
					string text = value.ToString();
					this.Model.SetItemValueByNumber("FOwnerId", "", i);
					if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_OwnerOrg") && num2 > 0L)
					{
						if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) != 0L)
						{
							this.Model.SetValue("FOwnerId", num2.ToString(), i);
						}
						base.View.GetFieldEditor("FOwnerId", row).Enabled = true;
					}
				}
				value = this.Model.GetValue("FKeeperTypeId", i);
				if (value != null)
				{
					string text = value.ToString();
					this.Model.SetItemValueByNumber("FKeeperId", "", i);
					if (!string.IsNullOrWhiteSpace(text) && StringUtils.EqualsIgnoreCase(text, "BD_KeeperOrg") && num > 0L)
					{
						this.Model.SetValue("FKeeperId", num.ToString(), i);
						base.View.GetFieldEditor("FKeeperId", row).Enabled = true;
					}
				}
			}
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x0004D38C File Offset: 0x0004B58C
		private void SetDefKeeperTypeAndKeeperValue(int row)
		{
			object value = base.View.Model.GetValue("FOwnerTypeId0");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerId0") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			base.View.Model.SetValue("FOwnerTypeId", value, row);
			if (num == num2 && a == "BD_OwnerOrg")
			{
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", row);
				base.View.Model.SetValue("FKeeperId", num2, row);
				base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = true;
				base.View.GetFieldEditor("FKeeperId", row).Enabled = true;
				return;
			}
			base.View.GetFieldEditor("FKeeperTypeId", row).Enabled = false;
			base.View.GetFieldEditor("FKeeperId", row).Enabled = false;
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0004D4F0 File Offset: 0x0004B6F0
		private void SetKeeperTypeAndKeeper(string newOwerValue)
		{
			object value = base.View.Model.GetValue("FOwnerTypeId0");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long value2 = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			string text = Convert.ToString(value2);
			int entryRowCount = base.View.Model.GetEntryRowCount("FEntity");
			if (newOwerValue == text && a == "BD_OwnerOrg")
			{
				for (int i = 0; i < entryRowCount; i++)
				{
					DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", i) as DynamicObject;
					base.View.Model.SetValue("FOwnerTypeId", value, i);
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, i);
					}
					base.View.GetFieldEditor("FKeeperTypeId", i).Enabled = true;
					base.View.GetFieldEditor("FKeeperId", i).Enabled = true;
				}
				return;
			}
			for (int j = 0; j < entryRowCount; j++)
			{
				base.View.Model.SetValue("FOwnerTypeId", value, j);
				DynamicObject dynamicObject2 = base.View.Model.GetValue("FMaterialId", j) as DynamicObject;
				if (a == "BD_OwnerOrg")
				{
					if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, j);
					}
				}
				else if (!string.IsNullOrEmpty(newOwerValue) && !newOwerValue.Equals("0"))
				{
					DynamicObject dynamicObject3 = base.View.Model.GetValue("FOwnerId", j) as DynamicObject;
					long num = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
					if (num == 0L && base.View.GetControl("FOwnerId").Enabled && dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) != 0L)
					{
						base.View.Model.SetValue("FOwnerId", newOwerValue, j);
					}
				}
				base.View.Model.SetValue("FKeeperTypeId", "BD_KeeperOrg", j);
				base.View.Model.SetValue("FKeeperId", text, j);
				base.View.GetFieldEditor("FKeeperTypeId", j).Enabled = false;
				base.View.GetFieldEditor("FKeeperId", j).Enabled = false;
			}
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0004D7E4 File Offset: 0x0004B9E4
		private void SetKeeperValue(string newKeeperTypeId, int row)
		{
			object value = base.View.Model.GetValue("FOwnerTypeId0");
			string a = "";
			if (value != null)
			{
				a = Convert.ToString(value);
			}
			DynamicObject dynamicObject = base.View.Model.GetValue("FOwnerId0") as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = base.View.Model.GetValue("FStockOrgId") as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			if (num == num2 && a == "BD_OwnerOrg")
			{
				base.View.GetFieldEditor("FKEEPERID", row).Enabled = true;
				return;
			}
			base.View.GetFieldEditor("FKEEPERID", row).Enabled = false;
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x0004D8CC File Offset: 0x0004BACC
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillType");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "SP_ReturnMtrlBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x04000252 RID: 594
		private const string SPReturnMtrlUserParameter = "SP_ReturnMtrlUserParameter";

		// Token: 0x04000253 RID: 595
		private long lastAuxpropId;
	}
}
