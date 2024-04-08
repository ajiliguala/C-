using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockTranser
{
	// Token: 0x0200005E RID: 94
	[Description("调拨申请单插件-列表插件")]
	public class StockTransferApplyList : AbstractListPlugIn
	{
		// Token: 0x06000431 RID: 1073 RVA: 0x00031EB9 File Offset: 0x000300B9
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00031FCC File Offset: 0x000301CC
		public override void OnChangeConvertRuleEnumList(ChangeConvertRuleEnumListEventArgs e)
		{
			base.OnChangeConvertRuleEnumList(e);
			if (this.ListView.SelectedRowsInfo == null || this.ListView.SelectedRowsInfo.Count<ListSelectedRow>() <= 0)
			{
				return;
			}
			ConvertRuleElement convertRuleElement = e.Convertrules.FirstOrDefault<ConvertRuleElement>();
			if (convertRuleElement != null && convertRuleElement.SourceFormId.Equals("STK_TRANSFERAPPLY") && (convertRuleElement.TargetFormId.Equals("STK_TransferDirect") || convertRuleElement.TargetFormId.Equals("STK_TRANSFEROUT")))
			{
				List<string> list = new List<string>();
				if (this.ListView.SelectedRowsInfo.FirstOrDefault<ListSelectedRow>().DataRow.ColumnContains("FTransferDirect"))
				{
					list = (from i in this.ListView.SelectedRowsInfo
					select Convert.ToString(i.DataRow["FTransferDirect"])).Distinct<string>().ToList<string>();
				}
				else
				{
					string[] primaryKeyValues = this.ListView.SelectedRowsInfo.GetPrimaryKeyValues();
					if (primaryKeyValues != null && primaryKeyValues.Count<string>() > 0)
					{
						QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
						{
							FormId = "STK_TRANSFERAPPLY",
							SelectItems = SelectorItemInfo.CreateItems("FTransferDirect "),
							FilterClauseWihtKey = string.Format("FID IN ({0}) ", string.Join(",", primaryKeyValues))
						};
						DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
						if (dynamicObjectCollection == null || dynamicObjectCollection.Count == 0)
						{
							return;
						}
						list = (from i in dynamicObjectCollection
						select Convert.ToString(i["FTransferDirect"])).Distinct<string>().ToList<string>();
					}
				}
				string a;
				if (list != null && list.Count<string>() == 1 && (a = list[0]) != null)
				{
					if (!(a == "GENERAL"))
					{
						if (!(a == "RETURN"))
						{
							return;
						}
						if (convertRuleElement.TargetFormId.Equals("STK_TransferDirect"))
						{
							e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("StkTransferApply-StkTransferDirect") || p.EnumId.Equals("StkTransferApply-StkTransferDirect_R"));
							e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("StkTransferApply-StkTransferDirect") || p.Id.Equals("StkTransferApply-StkTransferDirect_R"));
							return;
						}
						e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT") || p.EnumId.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT_R"));
						e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT") || p.Id.Equals("STK_TRANSFERAPPLY-STK_TRANSFEROUT_R"));
					}
					else
					{
						if (convertRuleElement.TargetFormId.Equals("STK_TransferDirect"))
						{
							e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("StkTransferApp_R-StkTransferDirect_R"));
							e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("StkTransferApp_R-StkTransferDirect_R"));
							return;
						}
						e.ConvertRuleEnumList.RemoveAll((EnumItem p) => p.EnumId.Equals("STK_TRANSFERAPP_R-STK_TRANSFEROUT_R"));
						e.Convertrules.RemoveAll((ConvertRuleElement p) => p.Id.Equals("STK_TRANSFERAPP_R-STK_TRANSFEROUT_R"));
						return;
					}
				}
			}
		}
	}
}
