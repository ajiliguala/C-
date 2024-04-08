using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.Common.Business.PlugIn;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper.SP;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x02000082 RID: 130
	[Description("简单生产领料单-表单插件")]
	public class SpPickMtrlEdit : AbstractBillPlugIn
	{
		// Token: 0x060005F2 RID: 1522 RVA: 0x00048908 File Offset: 0x00046B08
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			if (!e.Context.IsMultiOrg && StockServiceHelper.GetUpdateStockDate(e.Context, e.Context.CurrentOrganizationInfo.ID) == null)
			{
				e.CancelMessage = ResManager.LoadKDString("请先在【启用库存管理】中设置库存启用日期,再进行库存业务处理.", "004023030002269", 5, new object[0]);
				e.Cancel = true;
			}
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00048964 File Offset: 0x00046B64
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_Draw)
			{
				SelInStockBillParam selInStockBillParam = new SelInStockBillParam();
				selInStockBillParam.StockOrgId = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null);
				selInStockBillParam.PrdOrgId = BillUtils.GetValue<long>(this.Model, "FPrdOrgId", -1, 0L, null);
				selInStockBillParam.OwnerType = BillUtils.GetValue<string>(this.Model, "FOwnerTypeId0", -1, "", null);
				selInStockBillParam.OwnerId = BillUtils.GetValue<long>(this.Model, "FOwnerId0", -1, 0L, null);
				selInStockBillParam.WorkShopId = BillUtils.GetValue<long>(this.Model, "FWorkShopId", -1, 0L, null);
				if (this.ValidatePush(selInStockBillParam))
				{
					selInStockBillParam.FilterString = string.Format(" FPrdOrgId = {0} AND FIsPick = 'N' AND  FDocumentStatus = 'C' AND FCancelStatus = 'A' ", selInStockBillParam.PrdOrgId);
					if (selInStockBillParam.WorkShopId > 0L)
					{
						SelInStockBillParam selInStockBillParam2 = selInStockBillParam;
						selInStockBillParam2.FilterString += string.Format(" AND FWorkShopId1 = {0} ", selInStockBillParam.WorkShopId);
					}
					e.Option.SetVariableValue("FilterString", selInStockBillParam.FilterString);
					e.Option.SetVariableValue("SelInStockBillParam", selInStockBillParam);
					base.View.Session["SelInStockBillParam"] = selInStockBillParam;
					return;
				}
				e.Cancel = true;
			}
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00048ABC File Offset: 0x00046CBC
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

		// Token: 0x060005F5 RID: 1525 RVA: 0x00048B54 File Offset: 0x00046D54
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 13 && e.Bills is List<ConvertBillElement>)
			{
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				list.Add(new ConvertBillElement
				{
					FormID = "SP_InStock",
					ConvertBillType = 0,
					Name = new LocaleValue(ResManager.LoadKDString("简单生产入库单", "004023030004288", 5, new object[0]), base.Context.UserLocale.LCID)
				});
				list.Add(new ConvertBillElement
				{
					FormID = "ENG_PRODUCTSTRUCTURE",
					ConvertBillType = 0,
					Name = new LocaleValue(ResManager.LoadKDString("物料清单", "004023030004291", 5, new object[0]), base.Context.UserLocale.LCID)
				});
				e.Bills = list;
				return;
			}
			FormOperationEnum convertOperation = e.ConvertOperation;
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00048C43 File Offset: 0x00046E43
		public override void OnShowTrackResult(ShowTrackResultEventArgs e)
		{
			base.OnShowTrackResult(e);
			FormOperationEnum trackOperation = e.TrackOperation;
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00048C68 File Offset: 0x00046E68
		private BillNode GetSPInstockTrackResult(BillNode trackResult)
		{
			if (this.Model.DataObject == null)
			{
				return trackResult;
			}
			DynamicObjectCollection source = this.Model.DataObject["Entity"] as DynamicObjectCollection;
			List<long> list = (from p in source
			select Convert.ToInt64(p["Id"])).ToList<long>();
			List<string> pickRelateInstockEntryIds = SpPickMtrlServiceHelper.GetPickRelateInstockEntryIds(base.View.Context, list, true, true);
			trackResult = BillNode.Create("SP_InStock", "", null);
			trackResult.LinkEntry = "FEntity";
			trackResult.TrackUpDownLinkEntry = "FEntity";
			trackResult.AddLinkCopyData(pickRelateInstockEntryIds);
			return trackResult;
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00048D0C File Offset: 0x00046F0C
		public override void OnGetConvertRule(GetConvertRuleEventArgs e)
		{
			base.OnGetConvertRule(e);
			if (e.ConvertOperation == 13 && (e.SourceFormId == "ENG_PRODUCTSTRUCTURE" || e.SourceFormId == "SP_InStock"))
			{
				ConvertRuleElement convertRuleElement = ConvertServiceHelper.GetConvertRules(base.Context, "ENG_BomExpandBill", e.TargetFormId).FirstOrDefault<ConvertRuleElement>();
				if (convertRuleElement != null && e.SourceFormId == "ENG_PRODUCTSTRUCTURE")
				{
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.FormId = e.SourceFormId;
					dynamicFormShowParameter.OpenStyle.ShowType = 5;
					dynamicFormShowParameter.CustomParams["ShowMode"] = 3.ToString();
					dynamicFormShowParameter.ParentPageId = base.View.PageId;
					e.DynamicFormShowParameter = dynamicFormShowParameter;
				}
				this.SetParentOwnerFieldMap(convertRuleElement, e.SourceFormId);
				e.Rule = convertRuleElement;
			}
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00048DE8 File Offset: 0x00046FE8
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			this._entityIdsInStockBeforeSave = SpPickMtrlServiceHelper.GetSpPickMtrlEntryIds(base.View.Context, BillUtils.GetDynamicObjectItemValue<long>(this.Model.DataObject, "Id", 0L));
			if (!this.ClearZeroRow())
			{
				e.Cancel = true;
			}
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x00048E90 File Offset: 0x00047090
		public override void AfterSave(AfterSaveEventArgs e)
		{
			if (this._entityIdsInStockBeforeSave != null && this._entityIdsInStockBeforeSave.Count<long>() > 0)
			{
				List<long> entityIdsInStockAfterSave = (from w in BillUtils.GetDynamicObjectItemValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null)
				where BillUtils.GetDynamicObjectItemValue<string>(w, "SrcBillType", null) == "SP_InStock" && BillUtils.GetDynamicObjectItemValue<long>(w, "SrcEnteryId", 0L) > 0L
				select BillUtils.GetDynamicObjectItemValue<long>(w, "SrcEnteryId", 0L)).Distinct<long>().ToList<long>();
				List<long> list = (from w in this._entityIdsInStockBeforeSave
				where !entityIdsInStockAfterSave.Contains(w)
				select w).ToList<long>();
				if (list.Count<long>() > 0)
				{
					SpInStockServiceHelper.ResetIsPickInInStcokEntry(base.View.Context, list);
				}
			}
			base.AfterSave(e);
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x00048F6C File Offset: 0x0004716C
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetDefKeeperTypeAndKeeperValue(e.Row);
			}
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x00048F94 File Offset: 0x00047194
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

		// Token: 0x060005FD RID: 1533 RVA: 0x0004915C File Offset: 0x0004735C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			IDynamicFormView view = base.View.GetView(e.Key);
			if (view != null && view.BusinessInfo.GetForm().Id == "ENG_PRODUCTSTRUCTURE" && e.EventName == "CustomSelBill")
			{
				SelBill0ption formSession = Common.GetFormSession<SelBill0ption>(base.View, "returnData", true);
				this.DoSelBills(formSession);
			}
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x000491CC File Offset: 0x000473CC
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.LockOrgAndOwner();
			this.LockKPTypeAndKPAfterSave();
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x000491E1 File Offset: 0x000473E1
		public override void AfterCreateModelData(EventArgs e)
		{
			if (base.View.OpenParameter.Status == null && base.View.OpenParameter.CreateFrom != 1)
			{
				this.SetBusinessTypeByBillType();
			}
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x00049210 File Offset: 0x00047410
		public override void AfterBindData(EventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(Convert.ToString(base.View.Model.GetValue("FOwnerTypeId0", 0)), "BD_OwnerOrg"))
			{
				SCMCommon.SetDefLocalCurrency(this, "FOwnerId0", "FCurrId");
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SP_PickMtrl", true) as FormMetadata;
			if (formMetadata == null)
			{
				return;
			}
			string text = "SP_PickMtrlUserParameter";
			if (!string.IsNullOrWhiteSpace(formMetadata.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = formMetadata.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata2 = MetaDataServiceHelper.Load(base.Context, text, true) as FormMetadata;
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata2.BusinessInfo, base.Context.UserId, "SP_PickMtrl", "UserParameter");
			bool dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<bool>(dynamicObject, "IsSetPicker", false);
			if (dynamicObjectItemValue)
			{
				this.SetPickerId();
				return;
			}
			this.SetStockerId();
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x000492F8 File Offset: 0x000474F8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string text = e.ListFilterParameter.Filter;
			bool f7AndSetNumberEvent = this.GetF7AndSetNumberEvent(e.FieldKey, e.Row, out text);
			if (f7AndSetNumberEvent)
			{
				e.Cancel = true;
			}
			else if (!string.IsNullOrWhiteSpace(text))
			{
				e.ListFilterParameter.Filter = Common.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FLot"))
			{
				text = Common.GetLotF8InvFilter(this, new LotF8InvFilterArgBD
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
					MtoFieldKey = "FMtoNo"
				}, e.Row);
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
				}
			}
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00049450 File Offset: 0x00047650
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			base.AuthPermissionBeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FBomId"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x06000603 RID: 1539 RVA: 0x00049484 File Offset: 0x00047684
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

		// Token: 0x06000604 RID: 1540 RVA: 0x000494D0 File Offset: 0x000476D0
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

		// Token: 0x06000605 RID: 1541 RVA: 0x00049568 File Offset: 0x00047768
		private void SetPickerId()
		{
			if (base.View.OpenParameter.Status == null)
			{
				List<DynamicObject> userLinkInfo = SpPickMtrlServiceHelper.GetUserLinkInfo(base.Context, base.Context.UserId);
				if (!ListUtils.IsEmpty<DynamicObject>(userLinkInfo))
				{
					long dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<long>(userLinkInfo.FirstOrDefault<DynamicObject>(), "Fid", 0L);
					this.Model.SetValue("FPickerId", dynamicObjectItemValue);
				}
			}
		}

		// Token: 0x06000606 RID: 1542 RVA: 0x000495D0 File Offset: 0x000477D0
		private bool GetF7AndSetNumberEvent(string fieldKey, int eRow, out string filter)
		{
			bool flag = false;
			filter = "";
			switch (fieldKey)
			{
			case "FPrdOrgId":
			{
				long value = BillUtils.GetValue<long>(this.Model, "FStockOrgId", -1, 0L, null);
				if (value <= 0L)
				{
					flag = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("请先录入发料组织的内容！", "004023030004294", 5, new object[0]), "", 0);
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

		// Token: 0x06000607 RID: 1543 RVA: 0x00049964 File Offset: 0x00047B64
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObject dynamicObject = base.View.Model.GetValue("FAuxPropId", e.Row) as DynamicObject;
				this.lastAuxpropId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			}
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x000499C8 File Offset: 0x00047BC8
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x00049A00 File Offset: 0x00047C00
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbStockSplit"))
				{
					return;
				}
				this.SplitPickMtrl("StockId_Id");
			}
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x00049A74 File Offset: 0x00047C74
		private void SplitPickMtrl(string splitKey)
		{
			if (base.View.Model.DataChanged)
			{
				base.View.ShowMessage(ResManager.LoadKDString("界面有变动,请先保存单据", "004023000023424", 5, new object[0]), 0);
				return;
			}
			DynamicObject dataObject = base.View.Model.DataObject;
			string[] splitKeys = splitKey.Split(new string[]
			{
				"|"
			}, StringSplitOptions.RemoveEmptyEntries);
			DynamicObjectCollection dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = new Dictionary<long, IGrouping<long, DynamicObject>>();
			List<DynamicObject> list = new List<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			if (splitKeys.Length <= 1)
			{
				dictionary = (from w in dynamicObjectItemValue
				where BillUtils.GetDynamicObjectItemValue<long>(w, splitKeys[0], 0L) != 0L
				select w into g
				group g by BillUtils.GetDynamicObjectItemValue<long>(g, splitKeys[0], 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			}
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectItemValue))
			{
				base.View.ShowMessage(ResManager.LoadKDString("简单生产领料单明细表体为空或仓库全为空，无需分仓领料", "004023000019677", 5, new object[0]), 0);
				return;
			}
			if (dictionary.Keys.Count<long>() == 1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("简单生产领料单明细表体不存在仓库不一致的分录，无需分仓领料", "004023000019678", 5, new object[0]), 0);
				return;
			}
			int num = 0;
			foreach (KeyValuePair<long, IGrouping<long, DynamicObject>> keyValuePair in dictionary)
			{
				num++;
				if (num == 1)
				{
					int num2 = 1;
					using (IEnumerator<DynamicObject> enumerator2 = keyValuePair.Value.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							DynamicObject dynamicObject = enumerator2.Current;
							BillUtils.SetDynamicObjectItemValue(dynamicObject, "Seq", num2++);
						}
						continue;
					}
				}
				DynamicObject dynamicObject2 = OrmUtils.Clone(dataObject, false, true) as DynamicObject;
				DynamicObjectCollection dynamicObjectItemValue2 = BillUtils.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject2, "Entity", null);
				dynamicObjectItemValue2.Clear();
				int num3 = 1;
				foreach (DynamicObject dynamicObject3 in keyValuePair.Value)
				{
					DynamicObject dynamicObject4 = OrmUtils.Clone(dynamicObject3, false, true) as DynamicObject;
					long dynamicObjectItemValue3 = BillUtils.GetDynamicObjectItemValue<long>(dynamicObject3, "Id", 0L);
					BillUtils.SetDynamicObjectItemValue(dynamicObject4, "Seq", num3);
					BillUtils.SetDynamicObjectItemValue(dynamicObject4, "SrcEnteryId", dynamicObjectItemValue3);
					dynamicObjectItemValue2.Add(dynamicObject4);
					list2.Add(dynamicObject3);
					num3++;
				}
				BillUtils.SetDynamicObjectItemValue(dynamicObject2, "BillNo", string.Empty);
				BillUtils.SetDynamicObjectItemValue(dynamicObject2, "DocumentStatus", "Z");
				list.Add(dynamicObject2);
			}
			OperateOption operateOption = OperateOption.Create();
			operateOption.SetVariableValue("IsOverPickValidator", false);
			IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, base.View.BusinessInfo, list.ToArray(), operateOption, "");
			if (!ListUtils.IsEmpty<DynamicObject>(operationResult.SuccessDataEnity))
			{
				this.ShowResult(operationResult.SuccessDataEnity.ToList<DynamicObject>(), "SP_PickMtrl", dynamicObjectItemValue, list2);
			}
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x00049E24 File Offset: 0x00048024
		private void ShowResult(List<DynamicObject> objs, string targetFormId, DynamicObjectCollection pickEntryDatas, List<DynamicObject> removeEntryDatas)
		{
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.ParentPageId = base.View.PageId;
			if (objs.Count == 1)
			{
				billShowParameter.Status = 2;
				string key = "_ConvertSessionKey";
				string text = "ConverOneResult";
				billShowParameter.CustomParams.Add(key, text);
				base.View.Session[text] = objs[0];
				billShowParameter.FormId = targetFormId;
				billShowParameter.PKey = Convert.ToString(BillUtils.GetDynamicObjectItemValue<long>(objs[0], "Id", 0L));
			}
			else
			{
				if (objs.Count <= 1)
				{
					return;
				}
				billShowParameter.FormId = "BOS_ConvertResultForm";
				string key2 = "ConvertResults";
				base.View.Session[key2] = objs.ToArray();
				billShowParameter.CustomParams.Add("_ConvertResultFormId", targetFormId);
			}
			if (base.View.Context.UserToken.ToLowerInvariant().Equals("bosidetest"))
			{
				billShowParameter.OpenStyle.ShowType = 0;
			}
			else
			{
				billShowParameter.OpenStyle.ShowType = 7;
			}
			base.View.ShowForm(billShowParameter);
			IEnumerable<DynamicObject> source = from data in objs
			from entrydata in BillUtils.GetDynamicObjectItemValue<DynamicObjectCollection>(data, "Entity", null)
			select entrydata;
			List<long> list = (from s in source
			select BillUtils.GetDynamicObjectItemValue<long>(s, "SrcEnteryId", 0L)).ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return;
			}
			new List<long>();
			foreach (DynamicObject dynamicObject in removeEntryDatas)
			{
				long dynamicObjectItemValue = BillUtils.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L);
				if (list.Contains(dynamicObjectItemValue))
				{
					pickEntryDatas.Remove(dynamicObject);
				}
			}
			OperateOption operateOption = OperateOption.Create();
			operateOption.SetVariableValue("IsOverPickValidator", false);
			BusinessDataServiceHelper.Save(base.Context, base.View.Model.BusinessInfo, base.View.Model.DataObject, operateOption, "");
			base.View.UpdateView();
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x0004A074 File Offset: 0x00048274
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

		// Token: 0x0600060D RID: 1549 RVA: 0x0004A168 File Offset: 0x00048368
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

		// Token: 0x0600060E RID: 1550 RVA: 0x0004A254 File Offset: 0x00048454
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

		// Token: 0x0600060F RID: 1551 RVA: 0x0004A300 File Offset: 0x00048500
		private bool ClearZeroRow()
		{
			DynamicObject parameterData = this.Model.ParameterData;
			if (parameterData != null && Convert.ToBoolean(parameterData["IsClearZeroRow"]))
			{
				Entity entity = this.Model.BillBusinessInfo.GetEntity("FEntity");
				List<DynamicObject> list = (from x in this.Model.GetEntityDataObject(entity)
				where Convert.ToDecimal(x["ActualQty"]) == 0m
				select x).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject in list)
				{
					this.Model.DeleteEntryRow("FEntity", Convert.ToInt32(dynamicObject["Seq"]) - 1);
				}
				if (this.Model.GetEntryRowCount("FEntity") == 0)
				{
					base.View.ShowErrMessage("", ResManager.LoadKDString("分录“明细”是必填项。", "004023000021872", 5, new object[0]), 0);
					return false;
				}
				base.View.UpdateView("FEntity");
			}
			return true;
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x0004A480 File Offset: 0x00048680
		private void DoSelBills(SelBill0ption selBillOPtion)
		{
			IEnumerable<DynamicObject> listSelRows = selBillOPtion.ListSelRows;
			if (listSelRows == null || listSelRows.Count<DynamicObject>() <= 0)
			{
				return;
			}
			int i = 0;
			ListSelectedRow[] source = (from p in listSelRows
			select new ListSelectedRow("0", BillUtils.GetDynamicObjectItemValue<long>(p, "Id", 0L).ToString(), i++, "ENG_BomExpandBill")
			{
				EntryEntityKey = "FBomResult"
			}).ToArray<ListSelectedRow>();
			ConvertRuleElement convertRuleElement = Common.SPGetConvertRule(base.View, "ENG_BomExpandBill", "SP_PickMtrl");
			if (selBillOPtion.FormId == "SP_InStock")
			{
				this.SetParentOwnerFieldMap(convertRuleElement, selBillOPtion.FormId);
			}
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

		// Token: 0x06000611 RID: 1553 RVA: 0x0004A648 File Offset: 0x00048848
		private void SetParentOwnerFieldMap(ConvertRuleElement bom2PickRule, string sFormid)
		{
			if (bom2PickRule != null)
			{
				string sourceFieldKey = this.GetConLossRate() ? "FBaseQty" : "FBaseActualQty";
				ConvertFilterPolicyElement convertFilterPolicyElement = (from p in bom2PickRule.Policies
				where p is ConvertFilterPolicyElement
				select p as ConvertFilterPolicyElement).FirstOrDefault<ConvertFilterPolicyElement>();
				convertFilterPolicyElement.AlertMessage = new LocaleValue(ResManager.LoadKDString("1.用户需有入库物料BOM上发料组织下简单生产领料单的新增权限。\r\n2.简单生产入库单领料标识应该为“否”。", "004023030006275", 5, new object[0]), base.Context.LogLocale.LCID);
				List<DefaultConvertPolicyElement> source = (from w in bom2PickRule.Policies
				where w is DefaultConvertPolicyElement
				select w as DefaultConvertPolicyElement).ToList<DefaultConvertPolicyElement>();
				if (source.Count<DefaultConvertPolicyElement>() >= 1)
				{
					DefaultConvertPolicyElement defaultConvertPolicyElement = source.First<DefaultConvertPolicyElement>();
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					foreach (FieldMapElement fieldMapElement in defaultConvertPolicyElement.FieldMaps)
					{
						if (fieldMapElement.TargetFieldKey == "FBaseActualQty")
						{
							flag = true;
							fieldMapElement.SourceFieldKey = sourceFieldKey;
						}
						else if (fieldMapElement.TargetFieldKey == "FBaseAppQty")
						{
							flag2 = true;
							fieldMapElement.SourceFieldKey = sourceFieldKey;
						}
						else if (fieldMapElement.TargetFieldKey == "FWorkShopId" && sFormid == "SP_InStock")
						{
							flag3 = true;
							fieldMapElement.SourceFieldKey = "fworkshopid_reg";
						}
						if (((flag && flag2) & !sFormid.Equals("SP_InStock")) || (sFormid.Equals("SP_InStock") && flag && flag2 && flag3))
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x0004A844 File Offset: 0x00048A44
		private bool GetConLossRate()
		{
			bool result = false;
			string text = "SP_PickMtrlUserParameter";
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

		// Token: 0x06000613 RID: 1555 RVA: 0x0004A908 File Offset: 0x00048B08
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

		// Token: 0x06000614 RID: 1556 RVA: 0x0004AA50 File Offset: 0x00048C50
		private void LockOrgAndOwner()
		{
			bool flag = true;
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["Entity"];
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				if ((from w in dynamicObjectCollection
				where BillUtils.GetDynamicObjectItemValue<int>(w, "SrcEnteryId", 0) > 0
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

		// Token: 0x06000615 RID: 1557 RVA: 0x0004AB60 File Offset: 0x00048D60
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

		// Token: 0x06000616 RID: 1558 RVA: 0x0004ACAC File Offset: 0x00048EAC
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

		// Token: 0x06000617 RID: 1559 RVA: 0x0004AEA4 File Offset: 0x000490A4
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

		// Token: 0x06000618 RID: 1560 RVA: 0x0004B008 File Offset: 0x00049208
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

		// Token: 0x06000619 RID: 1561 RVA: 0x0004B2FC File Offset: 0x000494FC
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

		// Token: 0x0600061A RID: 1562 RVA: 0x0004B3E4 File Offset: 0x000495E4
		private void SetBusinessTypeByBillType()
		{
			string baseDataStringValue = SCMCommon.GetBaseDataStringValue(this, "FBillType");
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "SP_PickMtrlBillTypeParmSetting", baseDataStringValue, true);
			if (dynamicObject != null)
			{
				base.View.Model.SetValue("FBizType", dynamicObject["BusinessType"]);
			}
		}

		// Token: 0x04000240 RID: 576
		private const string SPPickMtrlUserParameter = "SP_PickMtrlUserParameter";

		// Token: 0x04000241 RID: 577
		private List<long> _entityIdsInStockBeforeSave;

		// Token: 0x04000242 RID: 578
		private long lastAuxpropId;

		// Token: 0x04000243 RID: 579
		private string PushEnityKey = "FEntity";
	}
}
