using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000045 RID: 69
	[Description("批号主档列表插件")]
	public class BatchMainFileList : AbstractListPlugIn
	{
		// Token: 0x060002B3 RID: 691 RVA: 0x000216B4 File Offset: 0x0001F8B4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBADJUSTDIFF"))
				{
					return;
				}
				string operateName = ResManager.LoadKDString("批号调整", "004023030009276", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.DoLotDiffAdjust();
			}
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x00021731 File Offset: 0x0001F931
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_AttachmentMgr)
			{
				e.Option.SetVariableValue("ForceEnableAttachOperate", true);
			}
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0002177C File Offset: 0x0001F97C
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			e.AppendQueryFilter("  FLotStatus = '1' AND FBizType = '1' ");
			ListOpenParameter listOpenParameter = this.View.OpenParameter as ListOpenParameter;
			if (listOpenParameter != null && listOpenParameter.IsLookUp && this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo != null)
			{
				Form form = this.View.ParentFormView.BillBusinessInfo.GetForm();
				if (form != null && !string.IsNullOrWhiteSpace(form.Id))
				{
					string text = "BOS_BillUserParameter";
					if (!string.IsNullOrWhiteSpace(form.ParameterObjectId))
					{
						text = form.ParameterObjectId;
					}
					FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, text, true) as FormMetadata;
					if (formMetadata != null)
					{
						DynamicObject dynamicObject = UserParamterServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo, this.View.Context.UserId, form.Id, "UserParameter");
						bool flag = false;
						if (dynamicObject.DynamicObjectType.Properties.ContainsKey("HideZeroInvLot") && dynamicObject["HideZeroInvLot"] != null && !string.IsNullOrWhiteSpace(dynamicObject["HideZeroInvLot"].ToString()))
						{
							flag = Convert.ToBoolean(dynamicObject["HideZeroInvLot"]);
						}
						if (flag)
						{
							string a = "1";
							if (dynamicObject.DynamicObjectType.Properties.ContainsKey("InvMatchType") && dynamicObject["InvMatchType"] != null && !string.IsNullOrWhiteSpace(dynamicObject["InvMatchType"].ToString()))
							{
								a = dynamicObject["InvMatchType"].ToString();
							}
							if (a == "1" || !e.FilterString.Contains(" AND (TI.FBASEQTY <> 0 OR TI.FSECQTY <> 0)"))
							{
								e.AppendQueryFilter("  EXISTS (SELECT 1 FROM T_STK_INVENTORY TI WHERE FLOTID = TI.FLOT AND (TI.FBASEQTY <> 0 OR TI.FSECQTY <> 0) ) ");
							}
						}
					}
				}
			}
			bool flag2 = e.SelectedEntities.Exists((FilterEntity p) => p.Key.Equals("FEntityTrace"));
			if (flag2)
			{
				if (string.IsNullOrWhiteSpace(e.SortString))
				{
					e.SortString = "FNumber,FLotId ";
					return;
				}
				string text2 = e.SortString.ToUpper();
				if (text2.StartsWith("FNUMBER ASC"))
				{
					text2 = "FNUMBER ASC,FLOTID ASC" + text2.Substring(11, text2.Length - 11);
				}
				else if (text2.StartsWith("FNUMBER DESC"))
				{
					text2 = "FNUMBER DESC,FLOTID DESC" + text2.Substring(12, text2.Length - 12);
				}
				else
				{
					text2 = text2.Replace(",FNUMBER ASC", ",FNUMBER ASC,FLOTID ASC");
					text2 = text2.Replace(",FNUMBER DESC", ",FNUMBER DESC,FLOTID DESC");
				}
				e.SortString = text2;
			}
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x00021A3C File Offset: 0x0001FC3C
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) == null || !(a == "FBILLNO"))
			{
				base.EntryButtonCellClick(e);
				return;
			}
			if (e.Row <= 0)
			{
				return;
			}
			this.ShowBill(e.Row);
			e.Cancel = true;
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x00021AA4 File Offset: 0x0001FCA4
		private void ShowBill(int rowRow)
		{
			if (rowRow <= 0)
			{
				return;
			}
			ListSelectedRow listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == rowRow);
			if (listSelectedRow == null || string.IsNullOrWhiteSpace(listSelectedRow.EntryPrimaryKeyValue))
			{
				return;
			}
			string text = string.Format("SELECT T0.FBILLFORMID,T0.FBILLID,T1.FUSEORGID FROM T_BD_LOTMASTERBILLTRACE T0 INNER JOIN T_BD_LOTMASTER T1 ON T0.FLOTID = T1.FLOTID WHERE T0.FBILLTRACEID = @TraceId", new object[0]);
			List<SqlParam> list = new List<SqlParam>();
			list.Add(new SqlParam("@TraceId", 12, Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue)));
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(this.View.Context, text, null, null, CommandType.Text, list.ToArray());
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count < 1)
			{
				return;
			}
			string text2 = Convert.ToString(dynamicObjectCollection[0]["FBILLFORMID"]);
			long num = Convert.ToInt64(dynamicObjectCollection[0]["FBILLID"]);
			long num2 = Convert.ToInt64(dynamicObjectCollection[0]["FUSEORGID"]);
			if (StringUtils.EqualsIgnoreCase(text2, "STK_TRANSFEROUT") || StringUtils.EqualsIgnoreCase(text2, "STK_TRANSFERIN"))
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, text2, true) as FormMetadata;
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FStockOrgID"));
				OQLFilter oqlfilter = new OQLFilter();
				oqlfilter.Add(new OQLFilterHeadEntityItem
				{
					EntityKey = "FBillHead",
					FilterString = string.Format(" FID = {0} ", num)
				});
				DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, formMetadata.BusinessInfo, list2, oqlfilter);
				if (array != null && array.Length > 0)
				{
					num2 = Convert.ToInt64(array[0]["StockOrgID_Id"]);
				}
			}
			else if (StringUtils.EqualsIgnoreCase(text2, "STK_TransferDirect"))
			{
				FormMetadata formMetadata2 = MetaDataServiceHelper.Load(this.View.Context, text2, true) as FormMetadata;
				List<SelectorItemInfo> list3 = new List<SelectorItemInfo>();
				list3.Add(new SelectorItemInfo("FStockOutOrgId"));
				OQLFilter oqlfilter2 = new OQLFilter();
				oqlfilter2.Add(new OQLFilterHeadEntityItem
				{
					EntityKey = "FBillHead",
					FilterString = string.Format(" FID = {0} ", num)
				});
				DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, formMetadata2.BusinessInfo, list3, oqlfilter2);
				if (array2 != null && array2.Length > 0)
				{
					num2 = Convert.ToInt64(array2[0]["StockOutOrgId_Id"]);
				}
			}
			if (num2 < 1L)
			{
				return;
			}
			SCMCommon.ShowBizBillForm(this, text2, num, num2, 0L);
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x00021D40 File Offset: 0x0001FF40
		private void DoLotDiffAdjust()
		{
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(this.View.Context, new BusinessObject
			{
				Id = this.View.BillBusinessInfo.GetForm().Id,
				PermissionControl = 1,
				SubSystemId = this.View.Model.SubSytemId
			}, "5d0784ee55d888");
			if (permissionOrg == null || permissionOrg.Count<long>() < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("对不起，您没有批号调整的权限！", "004023030009590", 5, new object[0]), 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "STK_AdjustLotNo";
			dynamicFormShowParameter.SyncCallBackAction = true;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.PageId = SequentialGuid.NewGuid().ToString();
			this.View.ShowForm(dynamicFormShowParameter);
		}
	}
}
