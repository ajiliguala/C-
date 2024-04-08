using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Model.ListFilter;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200001C RID: 28
	[Description("库存调整询界面插件")]
	[DisplayName("库存调整")]
	public class InventoryAdjust : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000117 RID: 279 RVA: 0x00010342 File Offset: 0x0000E542
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.InitialFilterMetaData();
			this.InitialFilterGrid();
			this.ShowViewArea(false);
			this.AddOnInvListForm();
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00010364 File Offset: 0x0000E564
		public override void BeforeBindData(EventArgs e)
		{
			this.SetStockOrg();
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0001036C File Offset: 0x0000E56C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBACTION"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("调整", "004023030009253", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.CheckPermission(e))
				{
					e.Cancel = true;
					this.View.ShowWarnningMessage(ResManager.LoadKDString("您没有该操作权限!", "004023000018974", 5, new object[0]), "", 0, null, 1);
					return;
				}
				object value = this.Model.GetValue("FStockOrgId");
				if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(ResManager.LoadKDString("调整前，请录入库存组织！", "004023030002197", 5, new object[0]), "", 0);
					return;
				}
				if (!this.CheckAdjustParam())
				{
					e.Cancel = true;
					this.View.ShowWarnningMessage(ResManager.LoadKDString("请选择调整范围!", "004023000039710", 5, new object[0]), "", 0, null, 1);
				}
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x000104A8 File Offset: 0x0000E6A8
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			e.IsShowUsed = true;
			e.IsShowApproved = true;
			e.CommonFilterModel = this._listFilterModel;
			string text = string.Empty;
			ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
			if (listShowParameter != null)
			{
				listShowParameter.IsIsolationOrg = false;
			}
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FMATERIALID.FNUMBER") && !(a == "FMATERIALNAME"))
				{
					if (!(a == "FMATERIALTYPENAME.FNUMBER"))
					{
						if (a == "FSTOCKID.FNUMBER" || a == "FSTOCKNAME")
						{
							if (this.selOrgId.Trim().Length <= 0)
							{
								e.Cancel = true;
								this.View.ShowMessage(ResManager.LoadKDString("请先选库存组织", "004024030002386", 5, new object[0]), 0);
							}
							if (!BDServiceHelper.IsBaseDataShare(base.Context, "BD_STOCK"))
							{
								text = string.Format("  FUseOrgId IN ({0})", this.selOrgId);
							}
						}
					}
					else
					{
						if (this.selOrgId.Trim().Length <= 0)
						{
							e.Cancel = true;
							this.View.ShowMessage(ResManager.LoadKDString("请先选库存组织", "004024030002386", 5, new object[0]), 0);
						}
						if (!BDServiceHelper.IsBaseDataShare(base.Context, "BD_MATERIALCATEGORY"))
						{
							text = string.Format("  FUseOrgId IN ({0})", this.selOrgId);
						}
					}
				}
				else
				{
					if (this.selOrgId.Trim().Length <= 0)
					{
						e.Cancel = true;
						this.View.ShowMessage(ResManager.LoadKDString("请先选库存组织", "004024030002386", 5, new object[0]), 0);
					}
					if (!BDServiceHelper.IsBaseDataShare(base.Context, "BD_MATERIAL"))
					{
						text = string.Format("  FUseOrgId IN ({0})", this.selOrgId);
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
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

		// Token: 0x0600011B RID: 283 RVA: 0x000106B6 File Offset: 0x0000E8B6
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			e.IsShowUsed = true;
			e.IsShowApproved = true;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x000106C8 File Offset: 0x0000E8C8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (e.Key.ToUpperInvariant().Equals("FFILTERGRID"))
			{
				this._filterGridStr = e.Value.ToString();
			}
			if (e.Key.ToUpperInvariant().Equals("FSTOCKORGID"))
			{
				this.selOrgId = e.Value.ToString();
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00010728 File Offset: 0x0000E928
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "DONOTHING"))
				{
					return;
				}
				e.Option.SetVariableValue("IsInvokeAdjust", this._bIsAdjust ? "1" : "0");
				e.Option.SetVariableValue("FilterStr", this.GetFilterStr());
				e.Option.SetVariableValue("AdjustType", this.GetAdjustType());
				if (this._bIsAdjust)
				{
					this._bIsAdjust = false;
				}
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x000107F0 File Offset: 0x0000E9F0
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "DONOTHING"))
				{
					return;
				}
				JSONObject jsonobject = e.OperationResult.FuncResult as JSONObject;
				if (jsonobject == null)
				{
					return;
				}
				if (jsonobject.ContainsKey("IsPassVerif") && !Convert.ToBoolean(jsonobject["IsPassVerif"]))
				{
					if (jsonobject.ContainsKey("ErrLevel") && Convert.ToInt32(jsonobject["ErrLevel"]).Equals(0))
					{
						this.View.ShowMessage(Convert.ToString(jsonobject["Errmessage"]), 4, delegate(MessageBoxResult result)
						{
							if (result == 6)
							{
								this._bIsAdjust = true;
								this.View.InvokeFormOperation("DoNothing");
								return;
							}
							this._bIsAdjust = false;
						}, "", 0);
						return;
					}
					this.View.ShowErrMessage(Convert.ToString(jsonobject["Errmessage"]), "", 4);
					return;
				}
				else
				{
					if (jsonobject.ContainsKey("Errmessage") && !string.IsNullOrEmpty(Convert.ToString(jsonobject["Errmessage"])))
					{
						this.View.ShowMessage(jsonobject["Errmessage"].ToString(), 0);
					}
					this._returnCurDataIds = jsonobject["Ids"];
					this.ShowResult(this._returnCurDataIds);
				}
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00010938 File Offset: 0x0000EB38
		private string GetFilterStr()
		{
			object value = this.Model.GetValue("FStockOrgId");
			this.Model.GetValue("FFilterGrid");
			string filterStr = string.Format(" T.FStockOrgId IN ({0}) ", value.ToString());
			return this.GetFormatFilterStr(filterStr);
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00010980 File Offset: 0x0000EB80
		private int GetAdjustType()
		{
			int num = 0;
			if (Convert.ToBoolean(this.Model.GetValue("FAdjustStock")))
			{
				num++;
			}
			if (Convert.ToBoolean(this.Model.GetValue("FAdjustStockSec")))
			{
				num += 10;
			}
			if (Convert.ToBoolean(this.Model.GetValue("FAdjustStockBase")))
			{
				num += 100;
			}
			if (Convert.ToBoolean(this.Model.GetValue("FAdjustSec")))
			{
				num += 1000;
			}
			return num;
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00010A04 File Offset: 0x0000EC04
		private void InitialFilterMetaData()
		{
			if (this._filterMetaData == null)
			{
				this._filterMetaData = CommonFilterServiceHelper.GetFilterMetaData(base.Context, "");
				JSONObject jsonobject = this._filterMetaData.ToJSONObject();
				jsonobject.TryGetValue(CommonFilterConst.JSONKey_CompareTypes, out this._compareTypes);
				jsonobject.TryGetValue(CommonFilterConst.JSONKey_Logics, out this._logicData);
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00010A60 File Offset: 0x0000EC60
		private void InitialFilterGrid()
		{
			FilterGrid control = this.View.GetControl<FilterGrid>("FFilterGrid");
			if (control != null)
			{
				control.SetCompareTypes(this._compareTypes);
				control.SetLogicData(this._logicData);
				this._listFilterModel = new ListFilterModel();
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "STK_StockAdjustFilterFileds");
				this._listFilterModel.FilterObject.FilterMetaData = this._filterMetaData;
				this._listFilterModel.SetContext(base.Context, formMetaData.BusinessInfo, formMetaData.BusinessInfo.GetForm().GetFormServiceProvider(false));
				this._listFilterModel.InitFieldList(formMetaData, null);
				this._listFilterModel.FilterObject.SetSelectEntity(",FBILLHEAD,");
				control.SetFilterFields(this._listFilterModel.FilterObject.GetAllFilterFieldList());
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00010B30 File Offset: 0x0000ED30
		private string GetFilterSql()
		{
			object value = this.Model.GetValue("FStockOrgId");
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("调整前，请录入库存组织！", "004023030002197", 5, new object[0]), "", 0);
				return " ";
			}
			string filterStr = string.Format(" T.FStockOrgId IN ({0}) ", value.ToString());
			return this.GetFormatFilterStr(filterStr);
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00010BA4 File Offset: 0x0000EDA4
		private string GetFormatFilterStr(string filterStr)
		{
			if (!string.IsNullOrWhiteSpace(this._filterGridStr))
			{
				this._listFilterModel.FilterObject.Setting = this._filterGridStr;
				string filterSQLString = this._listFilterModel.FilterObject.GetFilterSQLString(base.Context, this.GetUserNow(this._listFilterModel.FilterObject));
				filterStr += (string.IsNullOrWhiteSpace(filterSQLString) ? "" : (" AND " + filterSQLString.Replace("FMaterialId.FNumber", "BM.FNumber").Replace("FMaterialName", "BM_L.FName").Replace("FSpecification", "BM_L.FSpecification").Replace("FMaterialGroup.FName", "BMG_L.FName").Replace("FErpClsID", "BMB.FErpClsID").Replace("FMaterialTypeName.FNumber", "BMC.FNumber").Replace("FStockId.FNumber", "BS.FNumber").Replace("FStockName", "BS_L.FName")));
			}
			return filterStr;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00010CA0 File Offset: 0x0000EEA0
		private DateTime? GetUserNow(FilterObject filter)
		{
			bool flag = false;
			foreach (FilterRow filterRow in filter.FilterRows)
			{
				flag = (filterRow.FilterField.FieldType == 58 || filterRow.FilterField.FieldType == 189 || filterRow.FilterField.FieldType == 61);
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				return new DateTime?(TimeServiceHelper.GetUserDateTime(base.Context));
			}
			return null;
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00010D44 File Offset: 0x0000EF44
		private void ShowResult(object sFids)
		{
			string text = " (1=2) ";
			IListView listView = this.View.GetView(this.detailFormPageId) as IListView;
			if (listView != null)
			{
				if (sFids != null && !string.IsNullOrEmpty(sFids.ToString()))
				{
					text = string.Format(" (FID IN ({0}))  ", sFids.ToString());
					this.ShowViewArea(true);
				}
				else
				{
					this.ShowViewArea(false);
				}
				listView.OpenParameter.SetCustomParameter("QueryAdjustFilter", text);
				listView.RefreshByFilter();
				this.View.SendDynamicFormAction(listView);
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00010DC5 File Offset: 0x0000EFC5
		private void SetStockOrg()
		{
			if (!base.Context.IsMultiOrg)
			{
				this.View.StyleManager.SetEnabled("FStockOrgId", null, false);
			}
			this.InitStkOrgId();
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00010E10 File Offset: 0x0000F010
		protected void InitStkOrgId()
		{
			if (this.View.ParentFormView != null)
			{
				this.lstStkOrg = this.GetPermissionOrg(this.View.ParentFormView.BillBusinessInfo.GetForm().Id);
			}
			List<EnumItem> organization = this.GetOrganization(this.View.Context);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FStockOrgId", 0);
			fieldEditor.SetComboItems(organization);
			this.lstStkOrg = new List<long>();
			foreach (EnumItem enumItem in organization)
			{
				this.lstStkOrg.Add(Convert.ToInt64(enumItem.Value));
			}
			object value = this.Model.GetValue("FStockOrgId");
			if (ObjectUtils.IsNullOrEmpty(value) && organization.Count((EnumItem p) => Convert.ToInt64(p.Value) == base.Context.CurrentOrganizationInfo.ID) > 0 && base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L))
			{
				this.View.Model.SetValue("FStockOrgId", base.Context.CurrentOrganizationInfo.ID);
				this.selOrgId = base.Context.CurrentOrganizationInfo.ID.ToString();
				if (!this.dctSelOrg.ContainsKey(base.Context.CurrentOrganizationInfo.ID))
				{
					this.dctSelOrg.Add(base.Context.CurrentOrganizationInfo.ID, base.Context.CurrentOrganizationInfo.Name);
				}
			}
			if (!ObjectUtils.IsNullOrEmpty(value) && !string.Equals(this.selOrgId, value.ToString()))
			{
				this.selOrgId = value.ToString();
				this.SetDctSelOrgId();
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00010FE8 File Offset: 0x0000F1E8
		protected void SetDctSelOrgId()
		{
			this.dctSelOrg.Clear();
			if (this.selOrgId.Length == 0)
			{
				return;
			}
			List<string> list = this.selOrgId.Split(new char[]
			{
				','
			}).ToList<string>();
			foreach (string value in list)
			{
				if (!this.dctSelOrg.ContainsKey(Convert.ToInt64(value)) && this.dctAllOrg.ContainsKey(Convert.ToInt64(value)))
				{
					this.dctSelOrg.Add(Convert.ToInt64(value), this.dctAllOrg[Convert.ToInt64(value)]);
				}
			}
		}

		// Token: 0x0600012A RID: 298 RVA: 0x000110B0 File Offset: 0x0000F2B0
		protected List<EnumItem> GetOrganization(Context ctx)
		{
			List<EnumItem> list = new List<EnumItem>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FORGID"));
			list2.Add(new SelectorItemInfo("FNUMBER"));
			list2.Add(new SelectorItemInfo("FNAME"));
			string text = this.GetInFilter("FORGID", this.lstStkOrg);
			text += string.Format(" AND FORGFUNCTIONS LIKE '%{0}%' ", 103L.ToString());
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list2,
				FilterClauseWihtKey = text,
				OrderByClauseWihtKey = "FNUMBER"
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				enumItem.EnumId = dynamicObject["FORGID"].ToString();
				enumItem.Value = dynamicObject["FORGID"].ToString();
				long key = (long)dynamicObject["FORGID"];
				string text2 = (dynamicObject["FName"] == null) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(string.Format("{0} {1}", (dynamicObject["FNumber"] ?? "").ToString(), text2), base.Context.UserLocale.LCID);
				list.Add(enumItem);
				if (!this.dctAllOrg.ContainsKey(key))
				{
					this.dctAllOrg.Add(key, text2);
				}
			}
			if (list.Count<EnumItem>() == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("库存组织未结束初始化，请先结束初始化！", "004024030002389", 5, new object[0]), 0, "", 0);
			}
			return list;
		}

		// Token: 0x0600012B RID: 299 RVA: 0x000112CC File Offset: 0x0000F4CC
		protected virtual string GetOtherFilter()
		{
			return " AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\n                        WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\n                        AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ";
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000112D3 File Offset: 0x0000F4D3
		protected string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count<long>() == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00011304 File Offset: 0x0000F504
		protected List<long> GetPermissionOrg(string formId)
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = formId,
				PermissionControl = this.View.ParentFormView.BillBusinessInfo.GetForm().SupportPermissionControl,
				SubSystemId = this.View.ParentFormView.Model.SubSytemId
			};
			return PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00011370 File Offset: 0x0000F570
		private void AddOnInvListForm()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.IsIsolationOrg = false;
			listShowParameter.FormId = "STK_MisDelivery";
			listShowParameter.OpenStyle.TagetKey = "FPanelResult";
			listShowParameter.OpenStyle.ShowType = 3;
			this.detailFormPageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.PageId = this.detailFormPageId;
			listShowParameter.CustomParams.Add("IsFromStockAdjust", "True");
			listShowParameter.CustomParams.Add("HideExitBarItem", "True");
			listShowParameter.CustomParams.Add("UseDefaultScheme", "True");
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00011421 File Offset: 0x0000F621
		private void ShowViewArea(bool bShow)
		{
			this.View.GetControl<SplitContainer>("FSplitContainer").HideSecondPanel(!bShow);
		}

		// Token: 0x06000130 RID: 304 RVA: 0x0001145C File Offset: 0x0000F65C
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = this.View.LayoutInfo.GetFormAppearance().Menu.BarItems;
			string text = string.Empty;
			string id = "STK_InventoryAdjust";
			text = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = id
			}, text);
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x000114F8 File Offset: 0x0000F6F8
		private bool CheckAdjustParam()
		{
			bool result = false;
			if (Convert.ToBoolean(this.Model.GetValue("FAdjustStock")) || Convert.ToBoolean(this.Model.GetValue("FAdjustStockSec")) || Convert.ToBoolean(this.Model.GetValue("FAdjustStockBase")) || Convert.ToBoolean(this.Model.GetValue("FAdjustSec")))
			{
				result = true;
			}
			return result;
		}

		// Token: 0x04000067 RID: 103
		protected string detailFormPageId = "";

		// Token: 0x04000068 RID: 104
		protected object _returnCurDataIds;

		// Token: 0x04000069 RID: 105
		protected bool _bIsAdjust;

		// Token: 0x0400006A RID: 106
		private ListFilterModel _listFilterModel;

		// Token: 0x0400006B RID: 107
		private FilterMetaData _filterMetaData;

		// Token: 0x0400006C RID: 108
		private object _compareTypes;

		// Token: 0x0400006D RID: 109
		private object _logicData;

		// Token: 0x0400006E RID: 110
		private string _filterGridStr;

		// Token: 0x0400006F RID: 111
		protected List<long> lstStkOrg = new List<long>();

		// Token: 0x04000070 RID: 112
		protected string selOrgId = string.Empty;

		// Token: 0x04000071 RID: 113
		protected Dictionary<long, string> dctSelOrg = new Dictionary<long, string>();

		// Token: 0x04000072 RID: 114
		protected Dictionary<long, string> dctAllOrg = new Dictionary<long, string>();
	}
}
