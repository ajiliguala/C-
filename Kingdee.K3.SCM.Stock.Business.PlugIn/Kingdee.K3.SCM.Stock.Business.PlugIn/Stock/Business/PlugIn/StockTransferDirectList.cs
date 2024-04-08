using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Contracts;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000090 RID: 144
	public class StockTransferDirectList : AbstractListPlugIn
	{
		// Token: 0x060007A7 RID: 1959 RVA: 0x00062C98 File Offset: 0x00060E98
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation == 13)
			{
				List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
				if (list != null && list.Count > 0 && base.Context.IsMultiOrg)
				{
					e.Bills = (from c in list
					where !c.FormID.Equals("DRP_NeedApplication", StringComparison.OrdinalIgnoreCase)
					select c).ToList<ConvertBillElement>();
				}
				return;
			}
			if (convertOperation != 26)
			{
				return;
			}
			List<ConvertBillElement> list2 = null;
			if (e.Bills != null)
			{
				list2 = (e.Bills as List<ConvertBillElement>);
			}
			if (e.Bills == null)
			{
				list2 = new List<ConvertBillElement>();
			}
			ConvertBillElement convertBillElement = new ConvertBillElement();
			convertBillElement.FormID = "SUB_SUBREQORDER";
			ConvertBillElement convertBillElement2 = new ConvertBillElement();
			convertBillElement2.FormID = "PRD_MO";
			list2.Add(convertBillElement);
			list2.Add(convertBillElement2);
			e.AddReplaceRelation("SUB_PPBOM", convertBillElement.FormID);
			e.AddReplaceRelation("PRD_PPBOM", convertBillElement2.FormID);
			e.Bills = list2;
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x00062D90 File Offset: 0x00060F90
		public override void OnShowTrackResult(ShowTrackResultEventArgs e)
		{
			base.OnShowTrackResult(e);
			FormOperationEnum trackOperation = e.TrackOperation;
			if (trackOperation != 26)
			{
				return;
			}
			if (e.TrackResult != null)
			{
				BillNode billNode = e.TrackResult as BillNode;
				if (!billNode.FormKey.Equals(e.TargetFormKey, StringComparison.OrdinalIgnoreCase))
				{
					e.TrackResult = this.GetReplaceTrackResult(billNode, e.TargetFormKey);
				}
			}
		}

		// Token: 0x060007A9 RID: 1961 RVA: 0x00062E20 File Offset: 0x00061020
		protected BillNode GetReplaceTrackResult(BillNode trackResult, string targetFormKey)
		{
			if (trackResult.FormKey.Equals("SUB_PPBOM", StringComparison.OrdinalIgnoreCase) && targetFormKey.Equals("SUB_SUBREQORDER", StringComparison.OrdinalIgnoreCase))
			{
				DynamicObjectCollection subEntityIdsByPPBomEntityId = TrackUpDownHelper.GetSubEntityIdsByPPBomEntityId(this.View.Context, (from o in trackResult.LinkIds
				select Convert.ToInt64(o)).ToList<long>());
				trackResult = BillNode.Create("SUB_SUBREQORDER", "", null);
				trackResult.AddLinkCopyData((from o in subEntityIdsByPPBomEntityId
				select o["FSUBREQENTRYID"].ToString()).ToList<string>());
				return trackResult;
			}
			if (trackResult.FormKey.Equals("PRD_PPBOM", StringComparison.OrdinalIgnoreCase) && targetFormKey.Equals("PRD_MO", StringComparison.OrdinalIgnoreCase))
			{
				DynamicObjectCollection moEntityIdsByPPBomEntityId = TrackUpDownHelper.GetMoEntityIdsByPPBomEntityId(this.View.Context, (from o in trackResult.LinkIds
				select Convert.ToInt64(o)).ToList<long>());
				trackResult = BillNode.Create("PRD_MO", "", null);
				trackResult.AddLinkCopyData((from o in moEntityIdsByPPBomEntityId
				select o["FMOENTRYID"].ToString()).ToList<string>());
				return trackResult;
			}
			return trackResult;
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x00062F7C File Offset: 0x0006117C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (!this.View.Context.IsMultiOrg)
			{
				this.View.GetMainBarItem("tbGetUnits").Visible = false;
			}
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x00062FB0 File Offset: 0x000611B0
		public override void PrepareFilterParameter(FilterArgs e)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "STK_TransDirectUserParameter", true);
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
				OrgIdKey = "FStockOutOrgId",
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

		// Token: 0x060007AC RID: 1964 RVA: 0x0006318C File Offset: 0x0006138C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			this.ConsignTransferReturnWriteOffFeature(e);
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
							dynamicObject = availableQtyParamByOrgId.FirstOrDefault((DynamicObject p) => StringUtils.EqualsIgnoreCase(Convert.ToString(p["FBILLFORMID"]), "STK_TransferDirect"));
						}
						list.Add(listSelectedRow.MainOrgId);
					}
				}
				if (dynamicObject != null && list.Count > 1)
				{
					this.ListView.ShowMessage(ResManager.LoadKDString("启用了可发量参数，只支持批量处理同一主业务组织的单据！", "005130000019525", 5, new object[0]), 0);
					e.Cancel = true;
					return;
				}
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x000632D4 File Offset: 0x000614D4
		private void ConsignTransferReturnWriteOffFeature(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE") && !(a == "SUBMIT") && !(a == "AUDIT"))
				{
					return;
				}
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				if (selectedRowsInfo != null && selectedRowsInfo.Count > 0)
				{
					List<string> values = (from x in selectedRowsInfo
					select x.PrimaryKeyValue).Distinct<string>().ToList<string>();
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
					{
						FormId = "STK_TransferDirect",
						SelectItems = SelectorItemInfo.CreateItems(new string[]
						{
							"FId",
							"FBillTypeId",
							"FBizType",
							"FTransferDirect",
							"FWriteOffConsign"
						}),
						FilterClauseWihtKey = string.Format(" FID IN ({0})", string.Join(",", values))
					};
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
					if (dynamicObjectCollection != null)
					{
						foreach (DynamicObject dynamicObject in dynamicObjectCollection)
						{
							string a2 = Convert.ToString(dynamicObject["FBILLTYPEID"]);
							string a3 = Convert.ToString(dynamicObject["FBIZTYPE"]);
							string a4 = Convert.ToString(dynamicObject["FTRANSFERDIRECT"]);
							bool flag = Convert.ToBoolean(dynamicObject["FWriteOffConsign"]);
							bool flag2 = a2 == "0bcc8f3ce0a64171b1a901344d1ac239" || a3 == "CONSIGNMENT";
							flag2 = (flag2 && flag && a4 == "RETURN");
							if (flag2)
							{
								string text = ResManager.LoadKDString("寄售调拨退回自动冲销(直接调拨单列表埋点)", "004023000020137", 5, new object[0]);
								SCMCommon.EventTrackingWithoutView(base.Context, 174, "ConsignTransferReturnAutoWriteOff", text, "ValueChange", e.Operation.FormOperation.Operation, text, this.View.PageId);
							}
						}
					}
				}
			}
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x00063528 File Offset: 0x00061728
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBGETUNITS"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("获取往来单位", "004023030009278", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				if (!this.CheckPermission(e))
				{
					e.Cancel = true;
					this.View.ShowMessage(ResManager.LoadKDString("您没有“直接调拨单”的“获取往来单位”权限！", "004023000019380", 5, new object[0]), 0);
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
				List<string> list2 = StockServiceHelper.UpdateBillsUnits(base.Context, list, "STK_TRANSFERDIRECT");
				if (list.Count == 1)
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (list2 != null && list2.Count > 0)
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("单据编号为{0}的直接调拨单，获取往来单位成功。", "004023000018975", 5, new object[0]), list[0]));
					}
					else
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("单据编号为{0}的直接调拨单，获取往来单位失败。", "004023000018976", 5, new object[0]), list[0]));
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
								Name = string.Format(ResManager.LoadKDString("单据【{0}】", "004023000018977", 5, new object[0]), text),
								Message = string.Format(ResManager.LoadKDString("单据编号为{0}的直接调拨单，获取往来单位成功。", "004023000018975", 5, new object[0]), text),
								SuccessStatus = true
							});
						}
						else
						{
							operateResultCollection.Add(new OperateResult
							{
								Name = string.Format(ResManager.LoadKDString("单据【{0}】", "004023000018977", 5, new object[0]), text),
								Message = string.Format(ResManager.LoadKDString("单据编号为{0}的直接调拨单，获取往来单位失败。", "004023000018976", 5, new object[0]), text),
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

		// Token: 0x060007AF RID: 1967 RVA: 0x0006382C File Offset: 0x00061A2C
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string text = string.Empty;
			string id = "STK_TransferDirect";
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
