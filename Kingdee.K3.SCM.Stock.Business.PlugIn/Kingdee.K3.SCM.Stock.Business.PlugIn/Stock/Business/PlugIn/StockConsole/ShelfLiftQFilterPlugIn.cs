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
	// Token: 0x02000039 RID: 57
	[Description("库存工作台保质期预警快捷过滤插件")]
	public class ShelfLiftQFilterPlugIn : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000238 RID: 568 RVA: 0x0001B7B3 File Offset: 0x000199B3
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.FillOrgList(true);
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0001B7C3 File Offset: 0x000199C3
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.FillOrgList(false);
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0001B7DC File Offset: 0x000199DC
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

		// Token: 0x0600023B RID: 571 RVA: 0x0001B9B4 File Offset: 0x00019BB4
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

		// Token: 0x0600023C RID: 572 RVA: 0x0001BA1E File Offset: 0x00019C1E
		protected string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count<long>() == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0001BA4D File Offset: 0x00019C4D
		protected virtual string GetOtherFilter()
		{
			return " AND EXISTS(SELECT 1 FROM T_BAS_SYSTEMPROFILE BSP \r\n                      WHERE BSP.FCATEGORY = 'STK'  AND BSP.FORGID = FORGID \r\n                      AND BSP.FKEY = 'STARTSTOCKDATE' ) ";
		}

		// Token: 0x040000C3 RID: 195
		private static readonly string formId = "STK_ShelfLiftAlarmRpt";
	}
}
