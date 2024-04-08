using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200008D RID: 141
	[Description("物料盘点作业列表插件")]
	public class StockCountInputList : AbstractListPlugIn
	{
		// Token: 0x06000721 RID: 1825 RVA: 0x0005A5BC File Offset: 0x000587BC
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object systemProfile = CommonServiceHelper.GetSystemProfile(this.View.Context, 0L, "STK_StockParameter", "ControlSerialNo", "");
			if (systemProfile != null)
			{
				this._useSN = Convert.ToBoolean(systemProfile);
			}
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x0005A604 File Offset: 0x00058804
		public override void AfterBindData(EventArgs e)
		{
			if (this.source.Equals("1"))
			{
				this.ListView.GetMainBarItem("TBQUERYCOUNTTABLE").Visible = false;
			}
			else
			{
				this.ListView.GetMainBarItem("TBQUERYCOUNTSCHEME").Visible = false;
			}
			this.View.GetMainBarItem("tbViewSN").Visible = this._useSN;
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x0005A66C File Offset: 0x0005886C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string key;
			switch (key = e.BarItemKey.ToUpper())
			{
			case "TBQUERYCOUNTGAIN":
				this.QueryTargetleBill("STK_StockCountGain", "T_STK_STKCOUNTGAINENTRY");
				return;
			case "TBQUERYCOUNTLOSS":
				this.QueryTargetleBill("STK_StockCountLoss", "T_STK_STKCOUNTLOSSENTRY");
				return;
			case "TBQUERYCOUNTSCHEME":
				this.QuerySrcleBill("STK_StockCountScheme", this.source);
				return;
			case "TBQUERYCOUNTTABLE":
				this.QuerySrcleBill("STK_CycleCountTable", this.source);
				return;
			case "TBVIEWSN":
				this.ViewInputSerial();
				return;
			case "TBDIFRPT":
				this.ShowDiffReport();
				break;

				return;
			}
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x0005A76C File Offset: 0x0005896C
		public override void PrepareFilterParameter(FilterArgs e)
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("source");
			if (customParameter != null)
			{
				this.source = customParameter.ToString().Trim().Replace("'", "");
				if (!string.IsNullOrWhiteSpace(this.source))
				{
					e.AppendQueryFilter(string.Format(" FSOURCE = '{0}'", this.source));
				}
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("CountTableIds");
			string text = "";
			if (customParameter != null)
			{
				text = customParameter.ToString().Trim().Replace("'", "");
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				e.AppendQueryFilter(string.Format(" FSchemeId IN ({0})", text));
			}
			if (string.IsNullOrWhiteSpace(e.SortString))
			{
				e.SortString = " FCreateDate DESC ";
			}
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x0005A840 File Offset: 0x00058A40
		private void QueryTargetleBill(string formID, string entiryTB)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = formID
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有对应单据的查看权限！", "004023030002107", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FID"));
			list.Add(new SelectorItemInfo("FStockOrgId"));
			string filterClauseWihtKey = string.Format(" EXISTS (select 1 from {1} E INNER JOIN {1}_LK  L ON E.FENTRYID=L.FENTRYID\r\n                                        WHERE  E.FID= FID AND L.FSBILLID IN ({0})) ", string.Join<long>(",", fidsFun), entiryTB);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = formID,
				FilterClauseWihtKey = filterClauseWihtKey,
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				return;
			}
			long[] array = new long[dynamicObjectCollection.Count];
			List<long> list2 = new List<long>();
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				long item = Convert.ToInt64(dynamicObjectCollection[i]["FStockOrgId"]);
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
				array[i] = Convert.ToInt64(dynamicObjectCollection[i]["FID"]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formID;
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", list2);
			if (list2.Count == 1)
			{
				listShowParameter.UseOrgId = list2[0];
			}
			Common.SetFormOpenStyle(this.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", array));
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x0005AA1C File Offset: 0x00058C1C
		private void QuerySrcleBill(string formID, string source)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = formID
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有对应单据的查看权限！", "004023030002107", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FSchemeId"));
			list.Add(new SelectorItemInfo("FStockOrgId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "STK_StockCountInput",
				FilterClauseWihtKey = string.Format(" FSource ={0} AND FID IN ({1})  ", source, string.Join<long>(",", fidsFun)),
				SelectItems = list
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (dynamicObjectCollection.Count == 0)
			{
				return;
			}
			long[] array = new long[dynamicObjectCollection.Count];
			List<long> list2 = new List<long>();
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				long item = Convert.ToInt64(dynamicObjectCollection[i]["FStockOrgId"]);
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
				array[i] = Convert.ToInt64(dynamicObjectCollection[i]["FSchemeId"]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formID;
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", list2);
			if (list2.Count == 1)
			{
				listShowParameter.UseOrgId = list2[0];
			}
			Common.SetFormOpenStyle(this.View, listShowParameter);
			listShowParameter.ListFilterParameter.Filter = string.Format(" FID IN ({0}) ", string.Join<long>(",", array));
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x0005AC00 File Offset: 0x00058E00
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

		// Token: 0x06000728 RID: 1832 RVA: 0x0005AC70 File Offset: 0x00058E70
		private void ViewInputSerial()
		{
			List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>();
			if (!this.GetParaFromListView(paras))
			{
				return;
			}
			this.ShowViewSerialForm(paras);
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x0005AC94 File Offset: 0x00058E94
		private bool GetParaFromListView(List<KeyValuePair<string, string>> paras)
		{
			ListSelectedRowCollection selectedRowsInfo = ((IListView)this.View).SelectedRowsInfo;
			if (selectedRowsInfo.Count < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先选择要执行查询的单据！", "004023000012252", 5, new object[0]), 0);
				return false;
			}
			if (!SerialServiceHelper.BillHaveSerial(this.View.Context, this.View.BillBusinessInfo, Convert.ToInt64(selectedRowsInfo[0].PrimaryKeyValue)))
			{
				this.View.ShowMessage(ResManager.LoadKDString("单据未录入序列号！", "004023000012251", 5, new object[0]), 0);
				return false;
			}
			string value = "1";
			FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "STK_StkCntInputUserSet", true) as FormMetadata;
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo, this.View.Context.UserId, "STK_StockCountInput", "UserParameter");
			if (dynamicObject != null && !Convert.ToBoolean(dynamicObject["OnlyShowEffectSN"]))
			{
				value = "0";
			}
			paras.Add(new KeyValuePair<string, string>("BillFormId", this.View.BillBusinessInfo.GetForm().Id));
			paras.Add(new KeyValuePair<string, string>("BillId", selectedRowsInfo[0].PrimaryKeyValue));
			paras.Add(new KeyValuePair<string, string>("OnlyShowEffectSN", value));
			return true;
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x0005ADF4 File Offset: 0x00058FF4
		private void ShowViewSerialForm(List<KeyValuePair<string, string>> paras)
		{
			SysReportShowParameter sysReportShowParameter = new SysReportShowParameter();
			sysReportShowParameter.ParentPageId = this.View.PageId;
			sysReportShowParameter.MultiSelect = false;
			sysReportShowParameter.FormId = "STK_BillSerialRpt";
			sysReportShowParameter.Height = 700;
			sysReportShowParameter.Width = 950;
			sysReportShowParameter.IsShowFilter = false;
			foreach (KeyValuePair<string, string> keyValuePair in paras)
			{
				sysReportShowParameter.CustomParams.Add(keyValuePair.Key, keyValuePair.Value);
			}
			this.View.ShowForm(sysReportShowParameter);
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x0005AEA8 File Offset: 0x000590A8
		private void ShowDiffReport()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "STK_StkCountDiffRpt"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("对不起您没有库存盘点差异报告的查看权限！", "004023000014248", 5, new object[0]), "", 0);
				return;
			}
			long[] fidsFun = this.GetFIDsFun();
			if (fidsFun.Length < 1)
			{
				return;
			}
			MoveReportShowParameter moveReportShowParameter = new MoveReportShowParameter();
			moveReportShowParameter.ParentPageId = this.View.PageId;
			moveReportShowParameter.MultiSelect = false;
			moveReportShowParameter.FormId = "STK_StkCountDiffRpt";
			moveReportShowParameter.Height = 700;
			moveReportShowParameter.Width = 950;
			moveReportShowParameter.IsShowFilter = false;
			moveReportShowParameter.CustomParams.Add("FromBill", "1");
			moveReportShowParameter.CustomParams.Add("InputBillIds", string.Join<long>(",", fidsFun));
			this.View.ShowForm(moveReportShowParameter);
		}

		// Token: 0x04000294 RID: 660
		private string source = "";

		// Token: 0x04000295 RID: 661
		private bool _useSN;
	}
}
