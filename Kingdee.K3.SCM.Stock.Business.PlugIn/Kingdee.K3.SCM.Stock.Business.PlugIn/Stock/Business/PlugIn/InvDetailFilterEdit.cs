using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.ListFilter;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000051 RID: 81
	[Description("即时库存明细列表过滤界面插件")]
	public class InvDetailFilterEdit : AbstractListFilterPlugIn
	{
		// Token: 0x06000391 RID: 913 RVA: 0x0002B07A File Offset: 0x0002927A
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.GetOrgList();
		}

		// Token: 0x06000392 RID: 914 RVA: 0x0002B08C File Offset: 0x0002928C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			object value = base.Model.GetValue(CommonFilterConst.IsolationOrgList);
			if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
			{
				this._oldOrgIds = value.ToString();
			}
			else if (base.Model.IsolationOrgList == null || base.Model.IsolationOrgList.Count < 1)
			{
				this._oldOrgIds = base.View.Context.CurrentOrganizationInfo.ID.ToString();
			}
			else
			{
				this._oldOrgIds = string.Join<long>(",", base.Model.IsolationOrgList);
			}
			ComboFieldEditor fieldEditor = base.View.GetFieldEditor<ComboFieldEditor>(CommonFilterConst.IsolationOrgList, 0);
			fieldEditor.SetComboItems(this.permtedOrgList);
			base.Model.SetValue(CommonFilterConst.IsolationOrgList, this._oldOrgIds);
			((ICommonFilterModelService)base.Model).SchemeEntity.IsolationOrgListSetting = this._oldOrgIds;
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0002B18B File Offset: 0x0002938B
		public override void SetFilterMulSelOrgListIds(OrgListEventArgs e)
		{
			base.SetFilterMulSelOrgListIds(e);
			e.OrgIds = (from p in this.permtedOrgList
			select Convert.ToInt64(p.Value)).ToList<long>();
		}

		// Token: 0x06000394 RID: 916 RVA: 0x0002B1D0 File Offset: 0x000293D0
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FORGLIST"))
				{
					return;
				}
				IEnumerable<string> values = from p in this.permtedOrgList
				select p.EnumId;
				string arg = string.Join(",", values);
				string text = string.Format(" FORGID IN ({0}) ", arg);
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = text;
					return;
				}
				IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
				listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x0002B288 File Offset: 0x00029488
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FSELECTALLORG"))
				{
					return;
				}
				if (Convert.ToBoolean(e.NewValue))
				{
					base.Model.MutilIsolationOrgIds = string.Join<long>(",", (from p in this.permtedOrgList
					select Convert.ToInt64(p.Value)).ToList<long>());
					return;
				}
				base.Model.MutilIsolationOrgIds = Convert.ToString(base.Model.GetValue(CommonFilterConst.IsolationOrgList));
			}
		}

		// Token: 0x06000396 RID: 918 RVA: 0x0002B330 File Offset: 0x00029530
		private void GetOrgList()
		{
			bool flag = false;
			object systemProfile = CommonServiceHelper.GetSystemProfile(base.Context, 0L, "BD_BasePlatform", "FIsEnabledForbidOrgQuery", false);
			if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
			{
				flag = Convert.ToBoolean(systemProfile);
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			list.Add(new SelectorItemInfo("FNUMBER"));
			list.Add(new SelectorItemInfo("FNAME"));
			string filterClauseWihtKey = " FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' AND FORGFUNCTIONS LIKE '%103%' ";
			if (flag)
			{
				filterClauseWihtKey = " FDOCUMENTSTATUS = 'C' AND FORGFUNCTIONS LIKE '%103%' ";
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = filterClauseWihtKey,
				RequiresDataPermission = false,
				OrderByClauseWihtKey = "FNUMBER"
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.View.Context, new BusinessObject
			{
				Id = "STK_Inventory"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			this.permtedOrgList = new List<EnumItem>();
			this.allStockOrgList = new List<EnumItem>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				enumItem.EnumId = dynamicObject["FORGID"].ToString();
				enumItem.Value = dynamicObject["FORGID"].ToString();
				string arg = (dynamicObject["FName"] == null || string.IsNullOrEmpty(Convert.ToString(dynamicObject["FName"]))) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(string.Format("{0} {1}", dynamicObject["FNumber"], arg), base.Context.UserLocale.LCID);
				this.allStockOrgList.Add(enumItem);
				if (permissionOrg.Contains(Convert.ToInt64(dynamicObject["FORGID"])))
				{
					this.permtedOrgList.Add(enumItem);
				}
			}
		}

		// Token: 0x06000397 RID: 919 RVA: 0x0002B57C File Offset: 0x0002977C
		private string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || ListUtils.IsEmpty<long>(valList))
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} in ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x04000136 RID: 310
		private List<EnumItem> permtedOrgList = new List<EnumItem>();

		// Token: 0x04000137 RID: 311
		private List<EnumItem> allStockOrgList = new List<EnumItem>();

		// Token: 0x04000138 RID: 312
		private string _oldOrgIds = "";
	}
}
