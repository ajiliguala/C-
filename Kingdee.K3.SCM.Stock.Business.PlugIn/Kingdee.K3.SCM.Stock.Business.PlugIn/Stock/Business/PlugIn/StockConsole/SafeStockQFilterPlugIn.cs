using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x02000037 RID: 55
	[Description("库存工作台安全库存预警快捷过滤插件")]
	public class SafeStockQFilterPlugIn : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600022B RID: 555 RVA: 0x0001B2DB File Offset: 0x000194DB
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.FillOrgList(true);
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0001B2EB File Offset: 0x000194EB
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.FillOrgList(false);
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0001B304 File Offset: 0x00019504
		private void FillOrgList(bool isNew)
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FORGID"));
			list.Add(new SelectorItemInfo("FNUMBER"));
			list.Add(new SelectorItemInfo("FNAME"));
			string filterClauseWihtKey = string.Format(" FORGFUNCTIONS LIKE '%{0}%' ", 103L.ToString());
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list,
				FilterClauseWihtKey = filterClauseWihtKey
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			List<EnumItem> list2 = new List<EnumItem>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				enumItem.EnumId = dynamicObject["FORGID"].ToString();
				enumItem.Value = dynamicObject["FORGID"].ToString();
				long num = (long)dynamicObject["FORGID"];
				string text = (dynamicObject["FName"] == null) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(text, base.Context.UserLocale.LCID);
				list2.Add(enumItem);
			}
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FStockOrgId", 0);
			fieldEditor.SetComboItems(list2);
			if (isNew)
			{
				string text2 = string.Join(",", from p in list2
				select p.Value);
				this.Model.SetValue("FStockOrgId", text2);
			}
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0001B4DC File Offset: 0x000196DC
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

		// Token: 0x0600022F RID: 559 RVA: 0x0001B546 File Offset: 0x00019746
		protected string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count<long>() == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x06000230 RID: 560 RVA: 0x0001B575 File Offset: 0x00019775
		protected virtual string GetOtherFilter()
		{
			return " AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\n                      WHERE BSP.FCATEGORY = 'STK' AND BSP.FACCOUNTBOOKID = 0 AND BSP.FORGID = FORGID \r\n                      AND BSP.FKEY = 'IsInvEndInitial' AND BSP.FVALUE = '1') ";
		}

		// Token: 0x040000BF RID: 191
		private static readonly string formId = "STK_WarnSafeStockRpt";
	}
}
