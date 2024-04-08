using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200008B RID: 139
	public class StockCountEffectList : AbstractListPlugIn
	{
		// Token: 0x060006D7 RID: 1751 RVA: 0x00055054 File Offset: 0x00053254
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBQUERYCOUNTINPUT"))
				{
					return;
				}
				this.QueryCycleCountInput();
			}
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x0005508C File Offset: 0x0005328C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Operation.FormOperation.Operation, "UnAudit"))
			{
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				List<object> list = (from p in selectedRowsInfo
				select p.PrimaryKeyValue).ToList<object>();
				DynamicObject[] dynamicObjectCollection = this.GetDynamicObjectCollection(list.ToArray());
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (!StringUtils.EqualsIgnoreCase(dynamicObject["DocumentStatus"].ToString(), "C") && !StringUtils.EqualsIgnoreCase(dynamicObject["DocumentStatus"].ToString(), "B"))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("单据在提交后才可以执行反审核操作!", "004046030002275", 5, new object[0]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					if (dynamicObject["StkCountSchemeId"] != null && Convert.ToInt64(dynamicObject["StkCountSchemeId"]) > 0L)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("物料盘点作业审核生成的单据,不允许反审核!", "004046030002281", 5, new object[0]), ResManager.LoadKDString("反审核失败", "004046030002278", 5, new object[0]), 0);
						e.Cancel = true;
						break;
					}
				}
			}
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x000551F8 File Offset: 0x000533F8
		private DynamicObject[] GetDynamicObjectCollection(object[] fid)
		{
			if (this._Metadata == null)
			{
				this._Metadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, true);
			}
			return BusinessDataServiceHelper.Load(base.Context, fid, this._Metadata.BusinessInfo.GetDynamicObjectType());
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x0005525C File Offset: 0x0005345C
		public override void PrepareFilterParameter(FilterArgs e)
		{
			int num = -1;
			object customParameter = this.View.OpenParameter.GetCustomParameter("CountType");
			if (customParameter != null)
			{
				num = Convert.ToInt32(customParameter);
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("SourceType");
			if (customParameter != null)
			{
				if (!string.IsNullOrWhiteSpace(customParameter.ToString()))
				{
					num = Convert.ToInt32(customParameter);
				}
				num--;
			}
			if (num >= 0)
			{
				switch (num)
				{
				case 0:
					e.AppendQueryFilter(" FSourceType <> 2 ");
					this.sourceType = "1";
					break;
				case 1:
					e.AppendQueryFilter(" FSourceType <> 1 ");
					this.sourceType = "2";
					break;
				}
			}
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FStockOrgId",
				PurchaseParameterKey = "GroupDataIsolation",
				PurchaseParameterObject = "STK_StockParameter",
				BusinessGroupKey = "FStockerGroupId",
				OperatorType = "WHY"
			});
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.AppendQueryFilter(text);
			}
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x0005537C File Offset: 0x0005357C
		private void QueryCycleCountInput()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "STK_StockCountInput"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有物料盘点作业的查看权限！", "004023030002104", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FBillNo"));
			list.Add(new SelectorItemInfo("FStockOrgId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = this.View.OpenParameter.FormId,
				FilterClauseWihtKey = string.Format(" FSourceType <> '0' AND FID IN ({0})  ", string.Join<long>(",", fidsFun)),
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("手工录入单据，没有相关的物料盘点作业单据！", "004046030002284", 5, new object[0]), "", 0);
				return;
			}
			List<long> list2 = new List<long>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				long item = Convert.ToInt64(dynamicObject["FStockOrgId"]);
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
			}
			string arg = this.View.OpenParameter.FormId.ToUpperInvariant().Equals("STK_StockCountGain".ToUpperInvariant()) ? "T_STK_STKCOUNTGAINENTRY" : "T_STK_STKCOUNTLOSSENTRY";
			string filter = string.Format(" EXISTS (select 1 from {1} E INNER JOIN {1}_LK  L ON E.FENTRYID=L.FENTRYID\r\n                                        WHERE  E.FID IN ({0}) AND L.FSBILLID=FID) ", string.Join<long>(",", fidsFun), arg);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_StockCountInput";
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", list2);
			if (list2.Count == 1)
			{
				listShowParameter.UseOrgId = list2[0];
			}
			listShowParameter.CustomParams.Add("source", this.sourceType);
			Common.SetFormOpenStyle(this.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = filter;
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x000555DC File Offset: 0x000537DC
		private long[] GetFIDsFun()
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			List<object> list = (from p in selectedRowsInfo
			select p.PrimaryKeyValue).ToList<object>();
			long[] array = new long[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = Convert.ToInt64(list[i]);
			}
			return array;
		}

		// Token: 0x0400027D RID: 637
		private FormMetadata _Metadata;

		// Token: 0x0400027E RID: 638
		private string sourceType;
	}
}
