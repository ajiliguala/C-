using System;
using System.Collections.Generic;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper.SP;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x02000058 RID: 88
	public class SpPickMtrlList : AbstractListPlugIn
	{
		// Token: 0x060003EF RID: 1007 RVA: 0x0002FA68 File Offset: 0x0002DC68
		public override void PrepareFilterParameter(FilterArgs e)
		{
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
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x0002FAE6 File Offset: 0x0002DCE6
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			FormOperationEnum convertOperation = e.ConvertOperation;
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0002FAF9 File Offset: 0x0002DCF9
		public override void OnShowTrackResult(ShowTrackResultEventArgs e)
		{
			base.OnShowTrackResult(e);
			FormOperationEnum trackOperation = e.TrackOperation;
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x0002FB0C File Offset: 0x0002DD0C
		private BillNode GetSPInstockTrackResult(BillNode trackResult)
		{
			bool flag = true;
			List<long> selectedIds = this.GetSelectedIds(ref flag);
			if (selectedIds == null || selectedIds.Count < 1)
			{
				return trackResult;
			}
			List<string> pickRelateInstockEntryIds = SpPickMtrlServiceHelper.GetPickRelateInstockEntryIds(this.View.Context, selectedIds, flag, true);
			trackResult = BillNode.Create("SP_InStock", "", null);
			trackResult.LinkEntry = "FEntity";
			trackResult.TrackUpDownLinkEntry = "FEntity";
			trackResult.AddLinkCopyData(pickRelateInstockEntryIds);
			return trackResult;
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0002FB78 File Offset: 0x0002DD78
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
	}
}
