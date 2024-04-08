using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Model.ListFilter;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000073 RID: 115
	public class LotRuleCodeSetEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600052E RID: 1326 RVA: 0x0003FA75 File Offset: 0x0003DC75
		public override void BeforeBindData(EventArgs e)
		{
			this.FillOrgList();
			if (!this.View.Context.IsMultiOrg)
			{
				this.View.LockField("FUSEORGID", false);
			}
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x0003FAA0 File Offset: 0x0003DCA0
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.InitialFilterMetaData();
			this.InitialFilterGrid();
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x0003FAC0 File Offset: 0x0003DCC0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBUPDATE"))
				{
					if (!(a == "TBQUIT"))
					{
						return;
					}
					if (this.isUpdated)
					{
						this.View.Close();
						return;
					}
					this.View.ShowMessage(ResManager.LoadKDString("批号规则设置没有更新，是否继续", "004023030002188", 5, new object[0]), 4, delegate(MessageBoxResult result)
					{
						if (result == 6)
						{
							this.View.Close();
						}
					}, "", 0);
				}
				else
				{
					string operateName = ResManager.LoadKDString("更新", "004023030009261", 5, new object[0]);
					string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
					if (!string.IsNullOrWhiteSpace(onlyViewMsg))
					{
						this.View.ShowErrMessage(onlyViewMsg, "", 0);
						e.Cancel = true;
						return;
					}
					this.DoAction();
					return;
				}
			}
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x0003FB98 File Offset: 0x0003DD98
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			e.IsShowUsed = true;
			e.IsShowApproved = true;
			e.CommonFilterModel = this._listFilterModel;
			if (e.FieldKey.ToUpperInvariant().Equals("FCATEGORYID.FNAME"))
			{
				object value = this.View.Model.GetValue("FUSEORGID");
				if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
				{
					ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
					listShowParameter.MutilListUseOrgId = value.ToString();
					return;
				}
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("请先选使用组织！", "004023030002191", 5, new object[0]), 0);
			}
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x0003FC3E File Offset: 0x0003DE3E
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (e.Key.ToUpperInvariant().Equals("FFILTERGRID"))
			{
				this._filterGridStr = e.Value.ToString();
			}
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0003FC68 File Offset: 0x0003DE68
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x0003FC71 File Offset: 0x0003DE71
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x0003FC7C File Offset: 0x0003DE7C
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

		// Token: 0x06000536 RID: 1334 RVA: 0x0003FCD8 File Offset: 0x0003DED8
		private void InitialFilterGrid()
		{
			FilterGrid control = this.View.GetControl<FilterGrid>("FFilterGrid");
			if (control != null)
			{
				control.SetCompareTypes(this._compareTypes);
				control.SetLogicData(this._logicData);
				this._listFilterModel = new ListFilterModel();
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "STK_LotRuleSetFilterFileds");
				this._listFilterModel.FilterObject.FilterMetaData = this._filterMetaData;
				this._listFilterModel.SetContext(base.Context, formMetaData.BusinessInfo, formMetaData.BusinessInfo.GetForm().GetFormServiceProvider(false));
				this._listFilterModel.InitFieldList(formMetaData, null);
				this._listFilterModel.FilterObject.SetSelectEntity(",FBILLHEAD,");
				control.SetFilterFields(this._listFilterModel.FilterObject.GetAllFilterFieldList());
			}
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x0003FDA8 File Offset: 0x0003DFA8
		private void DoAction()
		{
			DynamicObject dynamicObject = (DynamicObject)this.View.Model.GetValue("FLotCodeRule");
			if (dynamicObject == null)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("更新前，请录入批号规则！", "004023030002194", 5, new object[0]), "", 0);
				return;
			}
			long num = Convert.ToInt64(dynamicObject["Id"]);
			object value = this.View.Model.GetValue("FUSEORGID");
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("更新前，请录入适用组织！", "004023030002197", 5, new object[0]), "", 0);
				return;
			}
			string text = this.View.Model.GetValue("FUpdateType").ToString();
			string text2 = string.Format(" M.FUSEORGID IN ({0}) ", value.ToString());
			if (text.ToUpperInvariant().Equals("FUPDATEBLANK"))
			{
				text2 += string.Format(" AND (T.FBATCHRULEID IS NULL OR T.FBATCHRULEID=0) ", new object[0]);
			}
			if (!string.IsNullOrWhiteSpace(this._filterGridStr))
			{
				this._listFilterModel.FilterObject.Setting = this._filterGridStr;
				List<FilterRow> filterRows = this._listFilterModel.FilterObject.FilterRows;
				foreach (FilterRow filterRow in filterRows)
				{
					if (filterRow.FilterField.FieldName.ToUpperInvariant().Equals("FERPCLSID") && string.IsNullOrWhiteSpace(filterRow.Value))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("物料属性不能为空！", "004023030002200", 5, new object[0]), "", 0);
						return;
					}
				}
				string filterSQLString = this._listFilterModel.FilterObject.GetFilterSQLString(base.Context, this.GetUserNow(this._listFilterModel.FilterObject));
				text2 += (string.IsNullOrWhiteSpace(filterSQLString) ? "" : (" AND " + filterSQLString.Replace("FCategoryID.FName", "CL.FName").Replace("FMaterialGroup.FName", "GL.FName").Replace("FCategoryID.", "C.").Replace("FMaterialGroup.", "G.")));
			}
			int num2 = StockServiceHelper.UpdateLotCodeRule(base.Context, num, text2);
			this.View.ShowMessage(string.Format(ResManager.LoadKDString("已更新{0}个物料的批号规则，请到物料列表中查看。", "004023030002203", 5, new object[0]), num2), 0);
			this.isUpdated = true;
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x00040050 File Offset: 0x0003E250
		private void FillOrgList()
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = this.View.BusinessInfo.GetForm().Id,
				PermissionControl = this.View.BusinessInfo.GetForm().SupportPermissionControl,
				SubSystemId = this.View.Model.SubSytemId
			};
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "1d8037a05b774e678d65df98afe9afdd");
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FUSEORGID", 0);
			fieldEditor.SetComboItems(this.GetOrganization(this.View.Context, permissionOrg));
			if (base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L) || base.Context.CurrentOrganizationInfo.FunctionIds.Contains(102L) || base.Context.CurrentOrganizationInfo.FunctionIds.Contains(101L) || base.Context.CurrentOrganizationInfo.FunctionIds.Contains(104L))
			{
				this.View.Model.SetValue("FUSEORGID", base.Context.CurrentOrganizationInfo.ID);
			}
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x00040180 File Offset: 0x0003E380
		private List<EnumItem> GetOrganization(Context context, List<long> orgList)
		{
			List<EnumItem> list = new List<EnumItem>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FORGID"));
			list2.Add(new SelectorItemInfo("FNUMBER"));
			list2.Add(new SelectorItemInfo("FNAME"));
			string text = this.GetInFilter("FORGID", orgList);
			text += string.Format(" AND (FORGFUNCTIONS LIKE '%{0}%' ", 103L.ToString());
			text += string.Format(" OR FORGFUNCTIONS LIKE '%{0}%' ", 102L.ToString());
			text += string.Format(" OR FORGFUNCTIONS LIKE '%{0}%' ", 101L.ToString());
			text += string.Format(" OR FORGFUNCTIONS LIKE '%{0}%' ) ", 104L.ToString());
			text += string.Format(" AND (FDocumentStatus = 'C' AND FForbidStatus='A' ) ", new object[0]);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list2,
				FilterClauseWihtKey = text
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				enumItem.EnumId = dynamicObject["FORGID"].ToString();
				enumItem.Value = dynamicObject["FORGID"].ToString();
				string text2 = (dynamicObject["FName"] == null) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(text2, base.Context.UserLocale.LCID);
				list.Add(enumItem);
			}
			if (list.Count<EnumItem>() == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("无符合权限的组织机构！", "004023030002206", 5, new object[0]), 0, "", 0);
			}
			return list;
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00040398 File Offset: 0x0003E598
		protected string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count<long>() == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x000403C8 File Offset: 0x0003E5C8
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

		// Token: 0x040001F3 RID: 499
		private const string Permission = "1d8037a05b774e678d65df98afe9afdd";

		// Token: 0x040001F4 RID: 500
		private bool isUpdated;

		// Token: 0x040001F5 RID: 501
		private string _filterGridStr;

		// Token: 0x040001F6 RID: 502
		private FilterMetaData _filterMetaData;

		// Token: 0x040001F7 RID: 503
		private object _compareTypes;

		// Token: 0x040001F8 RID: 504
		private object _logicData;

		// Token: 0x040001F9 RID: 505
		private ListFilterModel _listFilterModel;
	}
}
