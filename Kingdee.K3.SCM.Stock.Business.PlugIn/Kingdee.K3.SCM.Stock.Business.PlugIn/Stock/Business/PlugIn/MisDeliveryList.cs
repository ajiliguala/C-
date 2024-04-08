using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.Contracts;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000066 RID: 102
	public class MisDeliveryList : AbstractListPlugIn
	{
		// Token: 0x0600045E RID: 1118 RVA: 0x000342D0 File Offset: 0x000324D0
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
							dynamicObject = availableQtyParamByOrgId.FirstOrDefault((DynamicObject p) => StringUtils.EqualsIgnoreCase(Convert.ToString(p["FBILLFORMID"]), "STK_MISDELIVERY"));
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

		// Token: 0x0600045F RID: 1119 RVA: 0x0003440C File Offset: 0x0003260C
		public override void ListInitialize(ListInitializeEventArgs e)
		{
			object customParameter = this.ListView.OpenParameter.GetCustomParameter("IsFromStockAdjust");
			if (customParameter != null)
			{
				this._isAdjustQuery = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
			if (this._isAdjustQuery && this.View.Model.ParameterData != null && this.View.Model.ParameterData.DynamicObjectType.Properties.ContainsKey("FIsShowFilter"))
			{
				this._isShowFilter = Convert.ToBoolean(this.View.Model.ParameterData["FIsShowFilter"]);
				this.View.Model.ParameterData["FIsShowFilter"] = false;
			}
			base.ListInitialize(e);
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x000344DC File Offset: 0x000326DC
		public override void PrepareFilterParameter(FilterArgs e)
		{
			if (this._isAdjustQuery)
			{
				if (this._isShowFilter)
				{
					this.View.Model.ParameterData["FIsShowFilter"] = this._isShowFilter;
					this._isShowFilter = false;
				}
				object customParameter = this.View.OpenParameter.GetCustomParameter("QueryAdjustFilter");
				if (customParameter != null && !string.IsNullOrWhiteSpace(customParameter.ToString()))
				{
					e.AppendQueryFilter(customParameter.ToString());
				}
				else
				{
					e.AppendQueryFilter(" (1<>1) ");
				}
				this.AddMustSelFields();
				return;
			}
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FStockOrgId",
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

		// Token: 0x06000461 RID: 1121 RVA: 0x000345EC File Offset: 0x000327EC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.ToUpperInvariant() == "BTN_SYNCREFAMOUNT")
			{
				ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
				if (selectedRowsInfo != null && selectedRowsInfo.Count > 0)
				{
					List<long> list = (from m in selectedRowsInfo
					select Convert.ToInt64(m.PrimaryKeyValue)).Distinct<long>().ToList<long>();
					if (!MisBillServiceHelper.UpdateMisDeliveryRefAmount(base.Context, list))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("参考总成本刷新失败", "004023030009409", 5, new object[0]), "", 0);
						return;
					}
					this.View.Refresh();
					this.View.ShowMessage(ResManager.LoadKDString("参考总成本刷新成功", "004023030009418", 5, new object[0]), 0);
					return;
				}
				else
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("请至少选择一条数据", "004023030009412", 5, new object[0]), "", 0);
				}
			}
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00034734 File Offset: 0x00032934
		private void AddMustSelFields()
		{
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
			Dictionary<string, string> dFields = new Dictionary<string, string>();
			dFields.Add("FStockOrgId", "FStockOrgId.FName");
			dFields.Add("FStockDirect", "FStockDirect");
			dFields.Add("FDocumentStatus", "FDocumentStatus");
			dFields.Add("FBizType", "FBizType");
			dFields.Add("FDate", "FDate");
			dFields.Add("FMaterialId", "FMaterialId.FNumber");
			dFields.Add("FMaterialName", "FMaterialName");
			dFields.Add("FBaseQty", "FBaseQty");
			dFields.Add("FBaseUnitId", "FBaseUnitId.FName");
			dFields.Add("FQty", "FQty");
			dFields.Add("FUnitID", "FUnitID.FName");
			dFields.Add("FStockId", "FStockId.FName");
			dFields.Add("FStockLocId", "FStockLocId");
			dFields.Add("FBillNo", "FBillNo");
			dFields.Add("FModel", "FModel");
			using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = dFields.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string sField = enumerator.Current;
					Field field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, sField));
					ColumnField columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, dFields[sField]));
					if (field != null && columnField == null)
					{
						columnField = new ColumnField
						{
							Key = dFields[sField],
							Caption = field.Name,
							ColIndex = field.ListTabIndex,
							ColType = 106,
							ColWidth = 100,
							CoreField = false,
							DefaultColWidth = 100,
							DefaultVisible = true,
							EntityCaption = field.Entity.Name,
							EntityKey = field.EntityKey,
							FieldName = dFields[sField].Replace(".", "_"),
							IsHyperlink = false,
							Visible = true
						};
						this.ListModel.FilterParameter.ColumnInfo.Add(columnField);
					}
				}
			}
		}

		// Token: 0x040001A2 RID: 418
		private bool _isAdjustQuery;

		// Token: 0x040001A3 RID: 419
		private bool _isShowFilter;
	}
}
