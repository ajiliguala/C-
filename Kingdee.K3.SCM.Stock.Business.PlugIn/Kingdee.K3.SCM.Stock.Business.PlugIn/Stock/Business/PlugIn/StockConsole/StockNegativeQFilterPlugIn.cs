using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x0200003C RID: 60
	[Description("库存工作台负结余预警快捷过滤插件")]
	public class StockNegativeQFilterPlugIn : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600024A RID: 586 RVA: 0x0001BF57 File Offset: 0x0001A157
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			this.FillOrgList(true);
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0001BF67 File Offset: 0x0001A167
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.FillOrgList(false);
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0001BF80 File Offset: 0x0001A180
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
			this.enumList.Clear();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				EnumItem enumItem = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				enumItem.EnumId = dynamicObject["FORGID"].ToString();
				enumItem.Value = dynamicObject["FORGID"].ToString();
				long num = (long)dynamicObject["FORGID"];
				string text = (dynamicObject["FName"] == null) ? "" : dynamicObject["FName"].ToString();
				enumItem.Caption = new LocaleValue(text, base.Context.UserLocale.LCID);
				this.enumList.Add(enumItem);
			}
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FStockOrgId", 0);
			fieldEditor.SetComboItems(this.enumList);
			if (isNew)
			{
				string text2 = string.Join(",", from p in this.enumList
				select p.Value);
				this.Model.SetValue("FStockOrgId", text2);
			}
		}

		// Token: 0x0600024D RID: 589 RVA: 0x0001C168 File Offset: 0x0001A368
		public override void DataChanged(DataChangedEventArgs e)
		{
		}

		// Token: 0x040000C8 RID: 200
		private static readonly string formId = "STK_StockNegativeRpt";

		// Token: 0x040000C9 RID: 201
		private List<EnumItem> enumList = new List<EnumItem>();
	}
}
