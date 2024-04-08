using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x02000036 RID: 54
	[Description("库存工作台负库存预警快捷过滤插件")]
	public class MinusInvWarnQuickFilterPlugIn : AbstractBillPlugIn
	{
		// Token: 0x06000226 RID: 550 RVA: 0x0001B0C6 File Offset: 0x000192C6
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.FillOrgList(true);
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0001B0D6 File Offset: 0x000192D6
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.FillOrgList(false);
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0001B0F0 File Offset: 0x000192F0
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
			ComboFieldEditor fieldEditor = base.View.GetFieldEditor<ComboFieldEditor>("FStockOrgId", 0);
			fieldEditor.SetComboItems(list2);
			if (isNew)
			{
				string text2 = string.Join(",", from p in list2
				select p.Value);
				this.Model.SetValue("FStockOrgId", text2);
			}
		}

		// Token: 0x040000BD RID: 189
		private List<long> lstStkOrg = new List<long>();
	}
}
