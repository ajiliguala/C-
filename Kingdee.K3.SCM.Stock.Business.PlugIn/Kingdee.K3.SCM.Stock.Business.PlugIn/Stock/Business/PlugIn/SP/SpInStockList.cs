using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper.SP;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x02000081 RID: 129
	[Description("简单生产入库单-列表插件")]
	public class SpInStockList : AbstractListPlugIn
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060005D5 RID: 1493 RVA: 0x0004747B File Offset: 0x0004567B
		// (set) Token: 0x060005D6 RID: 1494 RVA: 0x00047483 File Offset: 0x00045683
		private SelInStockBillParam HostFormFilter { get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060005D7 RID: 1495 RVA: 0x0004748C File Offset: 0x0004568C
		private bool IsSelBillMode
		{
			get
			{
				return this.ListView.OpenParameter.ListType == 3;
			}
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x000474A4 File Offset: 0x000456A4
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (e.Operation.FormOperation.OperationId != FormOperation.Operation_Push)
			{
				if (this.IsSelBillMode && (e.Operation.FormOperation.OperationId == FormOperation.Operation_ReturnData || e.Operation.FormOperation.OperationId == 82005L))
				{
					e.Cancel = !this.SetConvertVariableValue();
				}
				return;
			}
			e.Option.SetVariableValue("IsExpandVirtualMtrl", true);
			e.Option.SetVariableValue("IsExpandPurchaseMtrl", false);
			e.Option.SetVariableValue("FilterString", this.ListView.Model.FilterParameter.FilterString);
			if (!this.isPushValidated)
			{
				e.Cancel = !this.CanDoPushPickBill();
				return;
			}
			this.isPushValidated = false;
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x000475C4 File Offset: 0x000457C4
		private bool CanDoPushPickBill()
		{
			List<string> list = this.ValidatePickedEntrys();
			if (list.Count > 0)
			{
				List<FieldAppearance> list2 = new List<FieldAppearance>();
				FieldAppearance fieldAppearance = K3DisplayerUtil.CreateDisplayerField<TextFieldAppearance, TextField>(this.View.Context, "FErrInfo", ResManager.LoadKDString("异常信息", "004023000017458", 5, new object[0]), "", null);
				fieldAppearance.Width = new LocaleValue("500", this.View.Context.UserLocale.LCID);
				list2.Add(fieldAppearance);
				K3DisplayerModel k3DisplayerModel = K3DisplayerModel.Create(this.View.Context, list2.ToArray(), null);
				foreach (string text in list)
				{
					k3DisplayerModel.AddMessage(text);
				}
				k3DisplayerModel.CancelButton.Visible = true;
				k3DisplayerModel.CancelButton.Caption = new LocaleValue(ResManager.LoadKDString("否", "004023000013912", 5, new object[0]));
				k3DisplayerModel.OKButton.Caption = new LocaleValue(ResManager.LoadKDString("是", "004023030005539", 5, new object[0]));
				k3DisplayerModel.OKButton.Visible = true;
				k3DisplayerModel.SummaryMessage = ResManager.LoadKDString("以下简单生产入库单已存在关联领料单，是否继续？", "004023000019013", 5, new object[0]);
				this.View.ShowK3Displayer(k3DisplayerModel, delegate(FormResult o)
				{
					if (o != null && o.ReturnData is K3DisplayerModel && (o.ReturnData as K3DisplayerModel).IsOK)
					{
						this.isPushValidated = true;
						this.View.InvokeFormOperation("Push");
					}
				}, "BOS_K3Displayer");
				return false;
			}
			return true;
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00047750 File Offset: 0x00045950
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			base.ListRowDoubleClick(e);
			if (this.IsSelBillMode)
			{
				e.Cancel = !this.SetConvertVariableValue();
			}
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x00047770 File Offset: 0x00045970
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			if (e.ConvertOperation == 12 && e.Bills is List<ConvertBillElement>)
			{
				bool flag = false;
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				ConvertBillElement convertBillElement = new ConvertBillElement();
				convertBillElement.FormID = "SP_PickMtrl";
				convertBillElement.ConvertBillType = 0;
				convertBillElement.Name = new LocaleValue(ResManager.LoadKDString("简单生产领料单", "004023030004282", 5, new object[0]), base.Context.UserLocale.LCID);
				if (this.ListView.SelectedRowsInfo.Count > 0)
				{
					List<long> list2 = new List<long>();
					foreach (ListSelectedRow listSelectedRow in this.ListView.SelectedRowsInfo)
					{
						long item = Convert.ToInt64(listSelectedRow.PrimaryKeyValue);
						if (!list2.Contains(item))
						{
							list2.Add(item);
						}
					}
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "SP_InStock",
						SelectItems = new List<SelectorItemInfo>
						{
							new SelectorItemInfo("FPrdOrgId")
						},
						FilterClauseWihtKey = string.Format(" FID IN ({0}) ", string.Join<long>(",", list2)),
						RequiresDataPermission = false
					};
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
					if (dynamicObjectCollection.Count > 0)
					{
						long num = Convert.ToInt64(dynamicObjectCollection[0]["FPrdOrgId"]);
						foreach (DynamicObject dynamicObject in dynamicObjectCollection)
						{
							if (Convert.ToInt64(dynamicObject["FPrdOrgId"]) != num)
							{
								flag = true;
								this.View.ShowMessage(ResManager.LoadKDString("如果要下推为简单生产领料单，只能选择生产组织相同的入库单执行下推", "004023000021873", 5, new object[0]), 0, ResManager.LoadKDString("选择的列表中包含不同的生产组织，下推只能下推为简单生产退库单", "004023000021874", 5, new object[0]), 0);
								break;
							}
						}
					}
					if (!flag)
					{
						list.Add(convertBillElement);
						e.Bills = list;
						e.AddReplaceRelation("ENG_BomExpandBill", "SP_PickMtrl");
						return;
					}
				}
			}
			else
			{
				FormOperationEnum convertOperation = e.ConvertOperation;
			}
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x000479C4 File Offset: 0x00045BC4
		public override void OnShowTrackResult(ShowTrackResultEventArgs e)
		{
			base.OnShowTrackResult(e);
			FormOperationEnum trackOperation = e.TrackOperation;
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x00047A2C File Offset: 0x00045C2C
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

		// Token: 0x060005DE RID: 1502 RVA: 0x00047CE4 File Offset: 0x00045EE4
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

		// Token: 0x060005DF RID: 1503 RVA: 0x00047DBC File Offset: 0x00045FBC
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (this.IsSelBillMode && this.View.ParentFormView != null && this.View.ParentFormView.BusinessInfo.GetForm().Id == "SP_PickMtrl")
			{
				this.HostFormFilter = (this.View.ParentFormView.Session["SelInStockBillParam"] as SelInStockBillParam);
				if (this.HostFormFilter != null && !string.IsNullOrWhiteSpace(this.HostFormFilter.FilterString))
				{
					e.AppendQueryFilter(this.HostFormFilter.FilterString);
				}
			}
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FStockOrgID",
				PurchaseParameterKey = "GroupDataIsolation",
				PurchaseParameterObject = "STK_StockParameter",
				BusinessGroupKey = "FSTOCKERGROUPID",
				OperatorType = "WHY"
			});
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.AppendQueryFilter(text);
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.SortString))
			{
				return;
			}
			Field field = this.View.BillBusinessInfo.GetFieldList().FirstOrDefault((Field p) => p is CreateDateField);
			Field field2 = this.View.BillBusinessInfo.GetFieldList().FirstOrDefault((Field p) => p is BillNoField);
			List<string> list = new List<string>();
			if (field != null)
			{
				list.Add(string.Format(" {0} DESC ", field.Key));
			}
			if (field2 != null)
			{
				list.Add(string.Format(" {0} DESC ", field2.Key));
			}
			foreach (FilterEntity filterEntity in e.SelectedEntities)
			{
				if ((filterEntity.Selected && filterEntity.EntityType == 2) || (filterEntity.Selected && filterEntity.EntityType == 3))
				{
					EntryEntity entryEntity = this.View.BillBusinessInfo.GetEntryEntity(filterEntity.Key);
					if (entryEntity != null)
					{
						if (string.IsNullOrWhiteSpace(entryEntity.SeqFieldKey))
						{
							list.Add(string.Format(" {0}.{1} ASC ", entryEntity.TableAlias, entryEntity.EntryPkFieldName));
						}
						else
						{
							list.Add(string.Format(" {0}.{1} ASC ", entryEntity.TableAlias, entryEntity.SeqFieldKey));
						}
					}
				}
			}
			e.SortString = string.Join(",", list);
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x0004806C File Offset: 0x0004626C
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (this.IsSelBillMode && this.View.ParentFormView != null && this.View.ParentFormView.BusinessInfo.GetForm().Id == "SP_PickMtrl")
			{
				this.View.ParentFormView.Session["SelInStockBillParam"] = null;
			}
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x000480D6 File Offset: 0x000462D6
		public override void OnLoad(EventArgs e)
		{
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x000480D8 File Offset: 0x000462D8
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.View.GetControl<Panel>("FExpandParaPanel").Visible = this.IsSelBillMode;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x000480FC File Offset: 0x000462FC
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (BillUtils.GetValue<bool>(this.Model, "FQkIsExpandPurchaseMtrl", -1, false, null))
			{
				this.View.LockField("FQkIsExpandVirtualMtrl", false);
				return;
			}
			this.View.LockField("FQkIsExpandVirtualMtrl", true);
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00048148 File Offset: 0x00046348
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FQkIsExpandPurchaseMtrl"))
				{
					return;
				}
				if (BillUtils.GetValue<bool>(this.Model, "FQkIsExpandPurchaseMtrl", -1, false, null))
				{
					this.Model.SetValue("FQkIsExpandVirtualMtrl", true);
					this.View.LockField("FQkIsExpandVirtualMtrl", false);
					return;
				}
				this.View.LockField("FQkIsExpandVirtualMtrl", true);
			}
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x000481C8 File Offset: 0x000463C8
		private bool SetConvertVariableValue()
		{
			object obj = null;
			if (this.View.ParentFormView != null && this.View.ParentFormView.Session.TryGetValue("_DrawOperationOption_", out obj))
			{
				string text = this.ListView.Model.FilterParameter.FilterString;
				if (this.HostFormFilter != null && this.HostFormFilter.WorkShopId == 0L)
				{
					long selWorkShopId = this.GetSelWorkShopId();
					if (selWorkShopId < 0L)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("选中的简单生产入库单的生产车间必须相同！", "004023000015611", 5, new object[0]), "", 0);
						return false;
					}
					if (selWorkShopId == 0L)
					{
						return false;
					}
					this.HostFormFilter.WorkShopId = selWorkShopId;
					if (string.IsNullOrWhiteSpace(text))
					{
						text = string.Format(" FWorkShopId1 = {0} ", selWorkShopId);
					}
					else
					{
						text += string.Format(" AND FWorkShopId1 = {0} ", selWorkShopId);
					}
				}
				OperateOption operateOption = obj as OperateOption;
				if (operateOption != null)
				{
					operateOption.SetVariableValue("IsExpandVirtualMtrl", BillUtils.GetValue<bool>(this.View.Model, "FQkIsExpandVirtualMtrl", -1, false, null));
					operateOption.SetVariableValue("IsExpandPurchaseMtrl", BillUtils.GetValue<bool>(this.View.Model, "FQkIsExpandPurchaseMtrl", -1, false, null));
					operateOption.SetVariableValue("FilterString", text);
					operateOption.SetVariableValue("SelInStockBillParam", this.HostFormFilter);
				}
			}
			return true;
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x0004832C File Offset: 0x0004652C
		private long GetSelWorkShopId()
		{
			bool flag = false;
			foreach (FilterEntity filterEntity in this.ListModel.FilterParameter.SelectedEntities)
			{
				if (filterEntity.Selected && StringUtils.EqualsIgnoreCase(filterEntity.Key, "FEntity"))
				{
					flag = true;
					break;
				}
			}
			List<long> list = new List<long>();
			if (flag)
			{
				using (IEnumerator<ListSelectedRow> enumerator2 = this.ListView.SelectedRowsInfo.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ListSelectedRow listSelectedRow = enumerator2.Current;
						if (listSelectedRow.Selected && listSelectedRow.EntryPrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.EntryPrimaryKeyValue))
						{
							list.Add(Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue));
						}
					}
					goto IL_12A;
				}
			}
			foreach (ListSelectedRow listSelectedRow2 in this.ListView.SelectedRowsInfo)
			{
				if (listSelectedRow2.Selected && listSelectedRow2.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow2.PrimaryKeyValue))
				{
					list.Add(Convert.ToInt64(listSelectedRow2.PrimaryKeyValue));
				}
			}
			IL_12A:
			if (list.Count < 1)
			{
				return 0L;
			}
			string text;
			if (flag)
			{
				text = string.Format("SELECT DISTINCT FWORKSHOPID FROM T_SP_INSTOCKENTRY WHERE FENTRYID IN ({0})", string.Join<long>(",", list.Distinct<long>()));
			}
			else
			{
				text = string.Format("SELECT DISTINCT FWORKSHOPID FROM T_SP_INSTOCKENTRY WHERE FID IN ({0})", string.Join<long>(",", list.Distinct<long>()));
			}
			DataTable dataTable = DBServiceHelper.ExecuteDataSet(this.View.Context, text).Tables[0];
			if (dataTable.Rows.Count != 1)
			{
				return -1L;
			}
			return Convert.ToInt64(dataTable.Rows[0][0]);
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x00048524 File Offset: 0x00046724
		private List<string> ValidatePickedEntrys()
		{
			List<string> list = new List<string>();
			bool flag = false;
			foreach (FilterEntity filterEntity in this.ListModel.FilterParameter.SelectedEntities)
			{
				if (filterEntity.Selected && StringUtils.EqualsIgnoreCase(filterEntity.Key, "FEntity"))
				{
					flag = true;
					break;
				}
			}
			List<long> list2 = new List<long>();
			if (flag)
			{
				using (IEnumerator<ListSelectedRow> enumerator2 = this.ListView.SelectedRowsInfo.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ListSelectedRow listSelectedRow = enumerator2.Current;
						if (listSelectedRow.Selected && listSelectedRow.EntryPrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.EntryPrimaryKeyValue))
						{
							list2.Add(Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue));
						}
					}
					goto IL_135;
				}
			}
			foreach (ListSelectedRow listSelectedRow2 in this.ListView.SelectedRowsInfo)
			{
				if (listSelectedRow2.Selected && listSelectedRow2.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow2.PrimaryKeyValue))
				{
					list2.Add(Convert.ToInt64(listSelectedRow2.PrimaryKeyValue));
				}
			}
			IL_135:
			if (list2.Count < 1)
			{
				return list;
			}
			List<SPInstockPickInfo> pickedInstockEntrys = SpInStockServiceHelper.GetPickedInstockEntrys(this.View.Context, list2, flag);
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

		// Token: 0x060005E8 RID: 1512 RVA: 0x00048730 File Offset: 0x00046930
		private BillNode GetSPPickMtrlTrackResult(BillNode trackResult)
		{
			bool flag = true;
			List<long> selectedIds = this.GetSelectedIds(ref flag);
			if (selectedIds == null || selectedIds.Count < 1)
			{
				return trackResult;
			}
			List<string> pickRelateInstockEntryIds = SpPickMtrlServiceHelper.GetPickRelateInstockEntryIds(this.View.Context, selectedIds, flag, false);
			trackResult = BillNode.Create("SP_PickMtrl", "", null);
			trackResult.LinkEntry = "FEntity";
			trackResult.TrackUpDownLinkEntry = "FEntity";
			trackResult.AddLinkCopyData(pickRelateInstockEntryIds);
			return trackResult;
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x0004879C File Offset: 0x0004699C
		private List<long> GetSelectedIds(ref bool isEntryId)
		{
			new List<string>();
			isEntryId = false;
			foreach (FilterEntity filterEntity in this.ListModel.FilterParameter.SelectedEntities)
			{
				if (filterEntity.Selected && StringUtils.EqualsIgnoreCase(filterEntity.Key, "FEntity"))
				{
					isEntryId = true;
					break;
				}
			}
			List<long> list = new List<long>();
			if (isEntryId)
			{
				using (IEnumerator<ListSelectedRow> enumerator2 = this.ListView.SelectedRowsInfo.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ListSelectedRow listSelectedRow = enumerator2.Current;
						if (listSelectedRow.Selected && listSelectedRow.EntryPrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.EntryPrimaryKeyValue))
						{
							list.Add(Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue));
						}
					}
					return list;
				}
			}
			foreach (ListSelectedRow listSelectedRow2 in this.ListView.SelectedRowsInfo)
			{
				if (listSelectedRow2.Selected && listSelectedRow2.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow2.PrimaryKeyValue))
				{
					list.Add(Convert.ToInt64(listSelectedRow2.PrimaryKeyValue));
				}
			}
			return list;
		}

		// Token: 0x04000237 RID: 567
		private const string SPPickMtrlUserParameter = "SP_PickMtrlUserParameter";

		// Token: 0x04000238 RID: 568
		private bool isPushValidated;
	}
}
