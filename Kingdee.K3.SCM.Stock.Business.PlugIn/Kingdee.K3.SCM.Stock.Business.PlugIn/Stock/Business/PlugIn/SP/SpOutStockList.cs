using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x0200002E RID: 46
	[Description("简单生产退库单-列表插件")]
	public class SpOutStockList : AbstractListPlugIn
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060001CB RID: 459 RVA: 0x000167FB File Offset: 0x000149FB
		// (set) Token: 0x060001CC RID: 460 RVA: 0x00016803 File Offset: 0x00014A03
		private SelInStockBillParam HostFormFilter { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060001CD RID: 461 RVA: 0x0001680C File Offset: 0x00014A0C
		private bool IsSelBillMode
		{
			get
			{
				return this.ListView.OpenParameter.ListType == 3;
			}
		}

		// Token: 0x060001CE RID: 462 RVA: 0x00016821 File Offset: 0x00014A21
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			base.ListRowDoubleClick(e);
			if (this.IsSelBillMode)
			{
				e.Cancel = !this.SetConvertVariableValue();
			}
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00016844 File Offset: 0x00014A44
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
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00016958 File Offset: 0x00014B58
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (this.IsSelBillMode && this.View.ParentFormView != null && this.View.ParentFormView.BusinessInfo.GetForm().Id == "SP_PickMtrl")
			{
				this.View.ParentFormView.Session["SelInStockBillParam"] = null;
			}
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x000169C2 File Offset: 0x00014BC2
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x000169CC File Offset: 0x00014BCC
		private bool SetConvertVariableValue()
		{
			object obj = null;
			if (this.View.ParentFormView != null && this.View.ParentFormView.Session.TryGetValue("_DrawOperationOption_", out obj))
			{
				string text = this.ListView.Model.FilterParameter.FilterString;
				if (this.HostFormFilter.WorkShopId == 0L)
				{
					long selWorkShopId = this.GetSelWorkShopId();
					if (selWorkShopId < 0L)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("选中的简单生产退库单的生产车间必须相同！", "004023000021835", 5, new object[0]), "", 0);
						return false;
					}
					if (selWorkShopId == 0L)
					{
						return false;
					}
					this.HostFormFilter.WorkShopId = selWorkShopId;
					text += string.Format(" AND FWorkShopId1 = {0} ", selWorkShopId);
				}
				OperateOption operateOption = obj as OperateOption;
				if (operateOption != null)
				{
					operateOption.SetVariableValue("FilterString", text);
					operateOption.SetVariableValue("SelOutStockBillParam", this.HostFormFilter);
				}
			}
			return true;
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00016AB8 File Offset: 0x00014CB8
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
				text = string.Format("SELECT DISTINCT FWORKSHOPID FROM T_SP_OUTSTOCKENTRY WHERE FENTRYID IN ({0})", string.Join<long>(",", list.Distinct<long>()));
			}
			else
			{
				text = string.Format("SELECT DISTINCT FWORKSHOPID FROM T_SP_OUTSTOCKENTRY WHERE FID IN ({0})", string.Join<long>(",", list.Distinct<long>()));
			}
			DataTable dataTable = DBServiceHelper.ExecuteDataSet(this.View.Context, text).Tables[0];
			if (dataTable.Rows.Count != 1)
			{
				return -1L;
			}
			return Convert.ToInt64(dataTable.Rows[0][0]);
		}
	}
}
