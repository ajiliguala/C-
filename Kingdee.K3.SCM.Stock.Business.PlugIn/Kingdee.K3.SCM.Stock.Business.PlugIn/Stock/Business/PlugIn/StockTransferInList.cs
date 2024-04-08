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
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000091 RID: 145
	public class StockTransferInList : AbstractListPlugIn
	{
		// Token: 0x060007B9 RID: 1977 RVA: 0x000638A3 File Offset: 0x00061AA3
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (!this.View.Context.IsMultiOrg)
			{
				this.View.GetMainBarItem("tbGetUnits").Visible = false;
			}
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x000638D4 File Offset: 0x00061AD4
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

		// Token: 0x060007BB RID: 1979 RVA: 0x000639D8 File Offset: 0x00061BD8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBBUSINESSPUSH"))
				{
					if (a == "TBGETUNITS")
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
								this.View.ShowMessage(ResManager.LoadKDString("您没有“分布式调入单”的“获取往来单位”权限！", "004023000019381", 5, new object[0]), 0);
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
							List<string> list2 = StockServiceHelper.UpdateBillsUnits(base.Context, list, "STK_TRANSFERIN");
							if (list.Count == 1)
							{
								StringBuilder stringBuilder = new StringBuilder();
								if (list2 != null && list2.Count > 0)
								{
									stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调入单，获取往来单位成功。", "004023000018978", 5, new object[0]), list[0]));
								}
								else
								{
									stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调入单，获取往来单位失败。", "004023000018979", 5, new object[0]), list[0]));
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
											Message = string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调入单，获取往来单位成功。", "004023000018978", 5, new object[0]), text),
											SuccessStatus = true
										});
									}
									else
									{
										operateResultCollection.Add(new OperateResult
										{
											Name = string.Format(ResManager.LoadKDString("单据【{0}】", "004023000018977", 5, new object[0]), text),
											Message = string.Format(ResManager.LoadKDString("单据编号为{0}的分布式调入单，获取往来单位失败。", "004023000018979", 5, new object[0]), text),
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
				}
				else
				{
					ListSelectedRowCollection selectedRowsInfo2 = this.ListView.SelectedRowsInfo;
					List<long> list3 = (from p in selectedRowsInfo2
					select Convert.ToInt64(p.PrimaryKeyValue)).ToList<long>();
					bool flag = CommonServiceHelper.IsContainsIOSBill(base.Context, list3, "T_STK_STKTRANSFERIN");
					if (flag)
					{
						e.Cancel = true;
						this.View.ShowErrMessage(ResManager.LoadKDString("选择下推的单据包含有内部交易单据，不允许下推！", "004023000010996", 5, new object[0]), "", 0);
						return;
					}
				}
			}
			base.BarItemClick(e);
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x00063D80 File Offset: 0x00061F80
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

		// Token: 0x060007BD RID: 1981 RVA: 0x00063F3E File Offset: 0x0006213E
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x00063F40 File Offset: 0x00062140
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string text = string.Empty;
			string id = "STK_TRANSFERIN";
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
	}
}
