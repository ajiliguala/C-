using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Contracts;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.K3.SCM.Stock.Business.PlugIn;

namespace Kingdee.K3.SCM.Purchase.Business.PlugIn
{
	// Token: 0x02000092 RID: 146
	public class StockTransferOutList : AbstractListPlugIn
	{
		// Token: 0x060007C2 RID: 1986 RVA: 0x00063FD4 File Offset: 0x000621D4
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null && (a == "SUBMIT" || a == "AUDIT"))
			{
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				ISaleService saleService = ServiceFactory.GetSaleService(base.Context);
				List<long> list = new List<long>();
				DynamicObject dynamicObject = null;
				foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
				{
					if (!list.Contains(listSelectedRow.MainOrgId))
					{
						if (dynamicObject == null)
						{
							DynamicObjectCollection availableQtyParamByOrgId = saleService.GetAvailableQtyParamByOrgId(base.Context, listSelectedRow.MainOrgId);
							dynamicObject = availableQtyParamByOrgId.FirstOrDefault((DynamicObject p) => StringUtils.EqualsIgnoreCase(Convert.ToString(p["FBILLFORMID"]), "STK_TRANSFEROUT"));
						}
						list.Add(listSelectedRow.MainOrgId);
					}
				}
				if (dynamicObject != null && list.Count > 1)
				{
					this.ListView.ShowMessage(ResManager.LoadKDString("启用了可发量参数，只支持批量处理同一主业务组织的单据！", "004023000037982", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x00064110 File Offset: 0x00062310
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (!this.View.Context.IsMultiOrg)
			{
				this.View.GetMainBarItem("tbGetUnits").Visible = false;
			}
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x00064141 File Offset: 0x00062341
		public override void AfterShowForm(AfterShowFormEventArgs e)
		{
			base.AfterShowForm(e);
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x00064158 File Offset: 0x00062358
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null && a == "TBGETUNITS")
			{
				string operateName = ResManager.LoadKDString("获取往来单位", "004023030009278", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
				}
				else
				{
					if (!this.CheckPermission(e))
					{
						e.Cancel = true;
						this.View.ShowMessage(ResManager.LoadKDString("您没有“分布式调出单”的“获取往来单位”权限！", "004023000019382", 5, new object[0]), 0);
						return;
					}
					ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
					if (selectedRowsInfo.Count<ListSelectedRow>() < 1)
					{
						this.View.ShowWarnningMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), "", 0, null, 1);
						return;
					}
					List<string> list = (from p in selectedRowsInfo
					select p.BillNo.ToString()).ToList<string>();
					List<string> list2 = StockServiceHelper.UpdateBillsUnits(base.Context, list, "STK_TRANSFEROUT");
					if (list.Count == 1)
					{
						StringBuilder stringBuilder = new StringBuilder();
						if (list2 != null && list2.Count > 0)
						{
							stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调出单，获取往来单位成功。", "004023000018980", 5, new object[0]), list[0]));
						}
						else
						{
							stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调出单，获取往来单位失败。", "004023000018981", 5, new object[0]), list[0]));
						}
						if (stringBuilder != null)
						{
							this.View.ShowMessage(stringBuilder.ToString(), 0);
						}
					}
					else
					{
						OperateResultCollection operateResultCollection = new OperateResultCollection();
						foreach (string text in list)
						{
							if (list2.Contains(text))
							{
								operateResultCollection.Add(new OperateResult
								{
									Name = string.Format(ResManager.LoadKDString("单据【{0}】", "004023000018977", 5, new object[0]), "004001030006269", text),
									Message = string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调出单，获取往来单位成功。", "004023000018980", 5, new object[0]), text),
									SuccessStatus = true
								});
							}
							else
							{
								operateResultCollection.Add(new OperateResult
								{
									Name = string.Format(ResManager.LoadKDString("单据【{0}】", "004023000018977", 5, new object[0]), "004001030006269", text),
									Message = string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调出单，获取往来单位失败。", "004023000018981", 5, new object[0]), text),
									SuccessStatus = false
								});
							}
						}
						if (operateResultCollection != null && operateResultCollection.Count > 0)
						{
							this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
						}
					}
					this.View.Refresh();
				}
			}
			base.BarItemClick(e);
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x0006446C File Offset: 0x0006266C
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string text = string.Empty;
			string id = "STK_TRANSFEROUT";
			text = "22acc52260854ff39e1fd31b39e8c33d";
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = id
			}, text);
			return permissionAuthResult.Passed;
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x000644DC File Offset: 0x000626DC
		private void SetButtonDisabled()
		{
			BarDataManager listMenu = this.ListView.BillLayoutInfo.GetFormAppearance().ListMenu;
			foreach (BarItem barItem in listMenu.BarItems)
			{
				this.View.GetMainBarItem(barItem.Name).Enabled = false;
			}
			this.View.GetMainBarItem("tbView").Enabled = true;
			this.View.GetMainBarItem("tbModify").Enabled = true;
			this.View.GetMainBarItem("tbFilter").Enabled = true;
			this.View.GetMainBarItem("tbRefresh").Enabled = true;
			this.View.GetMainBarItem("tbClose").Enabled = true;
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x000645C4 File Offset: 0x000627C4
		public override void PrepareFilterParameter(FilterArgs e)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "STK_BillUserParameter", true);
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, this.View.BillBusinessInfo.GetForm().Id, "UserParameter");
			bool flag = false;
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("ShowIosBill"))
			{
				flag = (dynamicObject["ShowIosBill"] != null && Convert.ToBoolean(dynamicObject["ShowIosBill"]));
			}
			object customParameter = this.View.OpenParameter.GetCustomParameter("OverOrgView");
			if (this.View.OpenParameter.GetCustomParameter("OpenSource") == null && customParameter == null && !flag)
			{
				if (string.IsNullOrWhiteSpace(e.FilterString))
				{
					e.FilterString = " FISGENFORIOS<>'1' ";
				}
				else
				{
					e.FilterString += " And FISGENFORIOS<>'1' ";
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
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.FilterString) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.FilterString = text;
				return;
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.FilterString) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				e.FilterString = e.FilterString + " AND (" + text + ")";
			}
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x00064798 File Offset: 0x00062998
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			if (!this.View.Context.IsMultiOrg && e.Bills != null && e.Bills is List<ConvertBillElement>)
			{
				e.Bills = (from p in (List<ConvertBillElement>)e.Bills
				where !p.FormID.Equals("PLN_REQUIREMENTORDER")
				select p).ToList<ConvertBillElement>();
			}
		}

		// Token: 0x060007CA RID: 1994 RVA: 0x00064820 File Offset: 0x00062A20
		public List<long> GetSelectedBillPrimaryKey(bool canMultSelect = false)
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			List<long> list = new List<long>();
			if (selectedRowsInfo.Count == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), 0);
				return list;
			}
			List<string> list2 = new List<string>();
			foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
			{
				if (!list2.Contains(listSelectedRow.BillNo))
				{
					list2.Add(listSelectedRow.BillNo);
				}
			}
			if (canMultSelect)
			{
				return (from row in selectedRowsInfo
				select Convert.ToInt64(row.PrimaryKeyValue)).ToList<long>();
			}
			if (list2.Count > 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("为了确保同步数据正确性，请一次仅选择一张单据！", "004023000022289", 5, new object[0]), 0);
				return list;
			}
			List<long> list3 = (from row in selectedRowsInfo
			select Convert.ToInt64(row.PrimaryKeyValue)).ToList<long>();
			list.Add(list3[0]);
			return list;
		}
	}
}
