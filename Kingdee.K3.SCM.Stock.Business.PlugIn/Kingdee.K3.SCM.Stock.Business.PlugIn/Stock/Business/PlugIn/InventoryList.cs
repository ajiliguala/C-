using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Objects.BillUserParameter;
using Kingdee.BOS.Core.Objects.Permission.Objects;
using Kingdee.BOS.Core.Objects.SqlBuilder;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Permission.Objects;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Model.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000097 RID: 151
	public class InventoryList : AbstractListPlugIn
	{
		// Token: 0x06000832 RID: 2098 RVA: 0x00069DDC File Offset: 0x00067FDC
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.prdDateApp = ((IListView)this.View).BillLayoutInfo.GetFieldAppearance("FProduceDate");
			this.expDateApp = ((IListView)this.View).BillLayoutInfo.GetFieldAppearance("FExpiryDate");
			this.usePLNReserve = CommonServiceHelper.IsUsePLNReserve(this.View.Context);
			object systemProfile = CommonServiceHelper.GetSystemProfile(this.View.Context, 0L, "STK_StockParameter", "ControlSerialNo", "");
			if (systemProfile != null)
			{
				this._useSN = Convert.ToBoolean(systemProfile);
			}
			this.InitializaMaterialTreeInfo();
			object customParameter = this.View.OpenParameter.GetCustomParameter("QueryPage");
			if (customParameter != null)
			{
				this.sQueryPage = customParameter.ToString();
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("NeedReturnData");
			if (customParameter != null)
			{
				this.needReturnData = customParameter.ToString();
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("QueryMode");
			if (customParameter != null)
			{
				this.queryMode = Convert.ToInt32(customParameter);
			}
			customParameter = this.ListView.OpenParameter.GetCustomParameter("IsFromQuery");
			if (customParameter != null)
			{
				this.isFromQuery = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
			customParameter = this.ListView.OpenParameter.GetCustomParameter("IsFromDetailQuery");
			if (customParameter != null)
			{
				this.isFromDetailQuery = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
			this._ignoreSchemeFilter = false;
			this._useIgnoreSchemeFilterPara = this.isFromDetailQuery;
			customParameter = this.ListView.OpenParameter.GetCustomParameter("UseIgnoreSchemeFilterOption");
			if (customParameter != null)
			{
				this._useIgnoreSchemeFilterPara = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
			customParameter = this.ListView.OpenParameter.GetCustomParameter("IsFromInvToGy");
			if (customParameter != null)
			{
				this.isInvSynGyQuery = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x00069FD4 File Offset: 0x000681D4
		public override void AfterBindData(EventArgs e)
		{
			this.View.GetMainBarItem("tbReturnData").Visible = this.needReturnData.Equals("1");
			this.View.GetMainBarItem("tbReturnData").Visible = this.needReturnData.Equals("1");
			this.View.GetMainBarItem("tbSplitButton_AddToDataCollection").Visible = this.needReturnData.Equals("1");
			if (this.isFromQuery)
			{
				if (!this.isFromDetailQuery && !this.isShowExit)
				{
					this.View.GetMainBarItem("tbClose").Visible = false;
				}
				if (this.isFromDetailQuery && this._isInitial)
				{
					QuickFilter control = this.View.GetControl<QuickFilter>("FQkFilterPanel");
					if (control != null)
					{
						control.SetValue("");
						control.SetFilterRows(new JSONArray());
					}
				}
				this._isInitial = false;
			}
			this.View.GetMainBarItem("tbReserveLinkQuery").Visible = this.usePLNReserve;
			this.View.GetMainBarItem("tbreservrLinkTraceBack").Visible = false;
			this.View.GetMainBarItem("tbViewSN").Visible = this._useSN;
			this.View.GetControl<EntryGrid>("FList").SetAllColHeaderAsText();
			this._detailQueryType = InventoryQuery.GetStringInvUserSet<string>(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "QueryType", "1");
			this.SetToolBarAndQFilterVisible(this._detailQueryType);
			this.SetMaterialGroupControl();
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x0006A168 File Offset: 0x00068368
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			List<string> list;
			if (!this.CheckPermission(e, out list))
			{
				e.Cancel = true;
				this.View.ShowWarnningMessage(ResManager.LoadKDString("没有该操作权限!", "004023030002158", 5, new object[0]), ResManager.LoadKDString("权限错误", "004023030002161", 5, new object[0]), 0, null, 1);
				return;
			}
			string key;
			switch (key = e.BarItemKey.ToUpperInvariant())
			{
			case "TBRETURNDATA":
				if (this.isFromQuery && this.queryMode == 1 && this.needReturnData.Equals("1") && this.ListView.SelectedRowsInfo != null && this.ListView.CurrentSelectedRowInfo != null)
				{
					((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "ReturnDetailData", "");
				}
				e.Cancel = true;
				return;
			case "TBCLOSE":
				if (this.isFromQuery && !this.isFromDetailQuery && !this.isShowExit)
				{
					((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "CloseWindowByDetail", "");
					e.Cancel = true;
					return;
				}
				break;
			case "TBLOCK":
			{
				string operateName = ResManager.LoadKDString("锁库", "004023030009245", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				e.Cancel = !this.LockUnLockInventoty(true, list);
				return;
			}
			case "TBUNLOCK":
			{
				string operateName = ResManager.LoadKDString("解锁", "004023030009246", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				e.Cancel = !this.LockUnLockInventoty(false, list);
				return;
			}
			case "TBREFRESH":
				this._isForceRefresh = true;
				this.View.RefreshByFilter();
				this._isForceRefresh = false;
				e.Cancel = true;
				return;
			case "TBVIEWSN":
				this.ViewInvSerial();
				return;
			case "BTNSEARCH":
				this._callBySearchButton = true;
				return;
			case "TBFILTER":
				this._callByFilterButton = true;
				return;
			case "TBLOCKLIST":
				e.Cancel = this.ShowLockStockList(list);
				break;

				return;
			}
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x0006A44C File Offset: 0x0006864C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (a == "CLOSEDATACOLLECTIONANDRETURN")
				{
					if (this.isFromQuery && this.queryMode == 1 && this.needReturnData.Equals("1"))
					{
						((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "CLOSEDATACOLLECTIONANDRETURN", "");
					}
					e.Cancel = true;
					return;
				}
				if (a == "RETURNANDCLEARDATACOLLECTION")
				{
					if (this.isFromQuery && this.queryMode == 1 && this.needReturnData.Equals("1") && this.View.Session != null && this.View.Session.ContainsKey("Data_Collection"))
					{
						Dictionary<string, ListSelectedRow> dictionary = this.View.Session["Data_Collection"] as Dictionary<string, ListSelectedRow>;
						if (dictionary != null && dictionary.Count > 0)
						{
							((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "RETURNANDCLEARDATACOLLECTION", "");
						}
					}
					e.Cancel = true;
					return;
				}
				if (a == "RETURNDATA")
				{
					if (this.isFromQuery && this.queryMode == 1 && this.needReturnData.Equals("1"))
					{
						((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "RETURNDATAFROMDATACOLLECTION", "");
					}
					e.Cancel = true;
					return;
				}
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x0006A5EC File Offset: 0x000687EC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "ADDTODATACOLLECTION"))
				{
					return;
				}
				if (ParameterUtils.GetOpClearSelectedRow(this.ListView))
				{
					new List<int>();
					if (this.ListView.SelectedRowsInfo != null && this.ListView.SelectedRowsInfo.Count<ListSelectedRow>() > 0)
					{
						EntryGrid control = this.ListView.GetControl<EntryGrid>("FLIST");
						int[] array = new int[1];
						control.SelectRows(array);
						this.View.ParentFormView.SendDynamicFormAction(this.View);
					}
				}
			}
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x0006A688 File Offset: 0x00068888
		public override void CustomEvents(CustomEventsArgs e)
		{
			string a;
			if ((a = e.EventName.ToUpperInvariant()) != null)
			{
				if (a == "LOCKINVENTORY")
				{
					this.LockUnLockInventoty(true, null);
					this.View.ParentFormView.SendDynamicFormAction(this.View);
					return;
				}
				if (a == "UNLOCKINVENTORY")
				{
					this.LockUnLockInventoty(false, null);
					this.View.ParentFormView.SendDynamicFormAction(this.View);
					return;
				}
				if (a == "VIEWSN")
				{
					this.ViewInvSerial();
					this.View.ParentFormView.SendDynamicFormAction(this.View);
					return;
				}
				if (a == "REFRESHDATA")
				{
					this._inputOrgIds = "";
					this._inputMoreFilter = false;
					if (!string.IsNullOrWhiteSpace(e.EventArgs))
					{
						string[] array = e.EventArgs.Split(new string[]
						{
							"];^]"
						}, StringSplitOptions.None);
						if (array != null)
						{
							this._queryFilter = array[0];
							this._inputOrgIds = array[1];
							this._inputMoreFilter = (array[2] != null && array[2] == "1");
						}
					}
					this._isForceRefresh = false;
					if (this.View.Session.ContainsKey("IsForceRefresh"))
					{
						this._isForceRefresh = Convert.ToBoolean(this.View.Session["IsForceRefresh"]);
						this.View.Session.Remove("IsForceRefresh");
					}
					this.View.RefreshByFilter();
					this.View.ParentFormView.SendDynamicFormAction(this.View);
					this._isForceRefresh = false;
					return;
				}
				if (a == "DOOPERATE")
				{
					this.View.ParentFormView.SendDynamicFormAction(this.View);
					return;
				}
				if (!(a == "COMMITFORM"))
				{
					return;
				}
				if (this.isFromQuery)
				{
					if (this.queryMode == 1 && this.needReturnData.Equals("1") && this.ListView.SelectedRowsInfo != null && this.ListView.CurrentSelectedRowInfo != null)
					{
						((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "ReturnDetailData", "");
						return;
					}
					((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "CloseWindowByDetail", "");
				}
			}
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x0006A8F4 File Offset: 0x00068AF4
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			object customParameter = this.View.OpenParameter.GetCustomParameter("QueryOrgId");
			if (customParameter != null && !string.IsNullOrWhiteSpace(customParameter.ToString()))
			{
				this.qOrgId = Convert.ToInt64(customParameter);
			}
			if (this.qOrgId < 1L)
			{
				this.qOrgId = Convert.ToInt64(base.Context.CurrentOrganizationInfo.ID);
			}
			customParameter = this.ListView.OpenParameter.GetCustomParameter("IsShowExit");
			if (customParameter != null)
			{
				this.isShowExit = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
			customParameter = this.ListView.OpenParameter.GetCustomParameter("IsFilterSchemeChanged");
			if (customParameter != null)
			{
				StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
			}
			string queryStockOrgIds = "";
			customParameter = this.ListView.OpenParameter.GetCustomParameter("StockOrgIds");
			if (customParameter != null)
			{
				queryStockOrgIds = customParameter.ToString();
			}
			this._ignoreSchemeFilter = false;
			if (this._useIgnoreSchemeFilterPara)
			{
				this._ignoreSchemeFilter = InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FUseSchemeFilter");
			}
			bool flag;
			if (this.isInvSynGyQuery)
			{
				object customParameter2 = this.View.OpenParameter.GetCustomParameter("QueryInvFilter");
				if (customParameter2 != null && !string.IsNullOrWhiteSpace(customParameter2.ToString()) && Convert.ToString(customParameter2.ToString()).Length >= 36)
				{
					e.ExtJoinTables.Add(new ExtJoinTableDescription
					{
						TableName = " TABLE(fn_StrSplit(@FID, ',', 2)) ",
						TableNameAs = "TINV",
						FieldName = "FID",
						ScourceKey = "FID",
						JoinFirst = true,
						JoinOption = 1
					});
					e.SqlParams.Add(new SqlParam("@FID", 162, customParameter2.ToString().Split(new char[]
					{
						','
					}).Distinct<string>().ToArray<string>()));
				}
				else
				{
					e.FilterString = " 1 <> 1 ";
				}
				flag = true;
			}
			else if (this.isFromQuery)
			{
				this._callByFilterButton = false;
				this._callBySearchButton = false;
				bool flag2 = false;
				customParameter = this.ListView.OpenParameter.GetCustomParameter("InitNoData", true);
				if (customParameter != null)
				{
					flag2 = StringUtils.EqualsIgnoreCase(customParameter.ToString(), "True");
				}
				if (flag2)
				{
					this._treeNodeClick = false;
					e.FilterString = " 1 <> 1 ";
					return;
				}
				StringBuilder stringBuilder = new StringBuilder();
				object customParameter3 = this.View.OpenParameter.GetCustomParameter("QueryFefreshFilter");
				if (customParameter3 != null && !string.IsNullOrWhiteSpace(customParameter3.ToString()))
				{
					this.AppendFilter(stringBuilder, customParameter3.ToString());
				}
				if (this._isInitial)
				{
					customParameter3 = this.View.OpenParameter.GetCustomParameter("QueryFilter");
					if (customParameter3 != null && !string.IsNullOrWhiteSpace(customParameter3.ToString()))
					{
						this._queryFilter = customParameter3.ToString();
					}
				}
				if (!string.IsNullOrWhiteSpace(this._queryFilter))
				{
					this.AppendFilter(stringBuilder, this._queryFilter);
				}
				object customParameter4 = this.ListView.OpenParameter.GetCustomParameter("QueryBillFormId");
				if (StringUtils.EqualsIgnoreCase(Convert.ToString(customParameter4), "QM_STKAPPInspect"))
				{
					this.AppendFilter(stringBuilder, " EXISTS(SELECT 1 FROM T_BD_MATERIALQUALITY TMQ \r\nINNER JOIN T_BD_MATERIAL TM ON TMQ.FMATERIALID=TM.FMATERIALID \r\nWHERE TM.FMASTERID = FMATERIALID AND TM.FUSEORGID = FSTOCKORGID AND (TMQ.FCHECKSTOCK='1' OR TMQ.FCHECKDELIVERY = '1')) ");
				}
				flag = (stringBuilder.Length > 0);
				string orgFilter = this.GetOrgFilter(e, queryStockOrgIds);
				if (!string.IsNullOrWhiteSpace(orgFilter))
				{
					this.AppendFilter(stringBuilder, orgFilter);
				}
				if (string.IsNullOrEmpty(e.FilterString) && stringBuilder.Length == 0)
				{
					if (!this._isInitial)
					{
						this.View.ShowNotificationMessage(ResManager.LoadKDString("当前没有任何过滤条件，将不会显示即时库存数据，请设置合适的过滤条件！", "004023000012232", 5, new object[0]), "", 0);
					}
					this.AppendFilter(stringBuilder, " 1 <> 1");
				}
				else if (!InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FShowZeroInv"))
				{
					this.AppendFilter(stringBuilder, " (FBASEQTY <> 0 OR FSECQTY <> 0 ) ");
				}
				if (this.isFromDetailQuery)
				{
					if (this._isInitial)
					{
						customParameter = this.ListView.OpenParameter.GetCustomParameter("QuerySortString", true);
						if (customParameter != null)
						{
							this._joinQuerySort = customParameter.ToString();
						}
						customParameter = this.ListView.OpenParameter.GetCustomParameter("HaveMoreFilter", true);
						flag = (customParameter != null && customParameter.ToString() == "1");
						this._inputMoreFilter = flag;
					}
					else
					{
						flag = this._inputMoreFilter;
					}
					if (string.IsNullOrWhiteSpace(e.SortString) && !string.IsNullOrWhiteSpace(this._joinQuerySort))
					{
						e.SortString = this._joinQuerySort;
					}
					if (this._materialGropField != null && Convert.ToBoolean(this._materialGropField.SupportGroup))
					{
						if (!this._treeNodeClick)
						{
							((ITreeListModel)this.Model).CurrSelectIds = "-1";
							((ITreeListModel)this.Model).CurrGroupParentId = "-1";
							((ITreeListModel)this.Model).TreeViewFilterParameter.SelectedGroupIds.Clear();
							((ITreeListModel)this.Model).TreeViewFilterParameter.FilterString = " ";
							((IListModel)this.Model).FilterParameter.SelectedGroupIds.Clear();
						}
						this._treeNodeClick = false;
					}
				}
				string text = "";
				if (stringBuilder.Length > 0)
				{
					text = stringBuilder.ToString(4, stringBuilder.Length - 4);
				}
				if (!this._ignoreSchemeFilter)
				{
					e.AppendQueryFilter(text);
				}
				else
				{
					e.FilterString = text;
				}
				if (this.isFromDetailQuery && this._isInitial && !string.IsNullOrWhiteSpace(e.QuickFilterString))
				{
					e.QuickFilterString = "";
				}
			}
			else
			{
				flag = false;
				if (!string.IsNullOrWhiteSpace(e.FilterString) || !string.IsNullOrWhiteSpace(this.ListModel.FilterParameter.QuickFilterString))
				{
					flag = true;
				}
				string orgFilter2 = this.GetOrgFilter(e, queryStockOrgIds);
				if (!string.IsNullOrWhiteSpace(orgFilter2))
				{
					e.AppendQueryFilter(orgFilter2);
				}
				if (flag)
				{
					if (!string.IsNullOrWhiteSpace(this.ListModel.FilterParameter.QuickFilterString))
					{
						e.AppendQueryFilter(this.ListModel.FilterParameter.QuickFilterString);
					}
					if (!InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FShowZeroInv"))
					{
						e.AppendQueryFilter(" (FBASEQTY <> 0 OR FSECQTY <> 0 ) ");
					}
				}
			}
			e.AppendQueryFilter(" FISEFFECTIVED = '1' ");
			if (!flag)
			{
				if (InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FBlankNoMoreFilter"))
				{
					e.FilterString = " 1 <> 1 ";
					return;
				}
				if (!InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FShowZeroInv"))
				{
					e.AppendQueryFilter(" (FBASEQTY <> 0 OR FSECQTY <> 0 ) ");
				}
			}
			this.AddMustSelFields();
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			this._visibleFieldKeys = (from p in columnInfo
			where p.Visible
			select p.Key).ToList<string>();
			e.SQLType = 0;
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0006B054 File Offset: 0x00069254
		public override void BeforeGetDataForTempTableAccess(BeforeGetDataForTempTableAccessArgs e)
		{
			base.BeforeGetDataForTempTableAccess(e);
			if (!string.IsNullOrWhiteSpace(e.TableName) && this._needRegexFields != null && this._needRegexFields.Count > 0)
			{
				StockServiceHelper.RegexInvDetailQueryTempData(this.View.Context, e.TableName, this._needRegexFields);
			}
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x0006B0A8 File Offset: 0x000692A8
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			string key;
			switch (key = e.FieldKey.ToUpperInvariant())
			{
			case "FSTOCKID.FNUMBER":
			case "FSTOCKNAME":
				this.ShowBaseDataForm(e.Row, "FStockId", "BD_STOCK");
				e.Cancel = true;
				return;
			case "FMATERIALID.FNUMBER":
			case "FMATERIALNAME":
				this.ShowBaseDataForm(e.Row, "FMaterialId", "BD_MATERIAL");
				e.Cancel = true;
				return;
			case "FLOT.FNAME":
			case "FLOT.FNUMBER":
				this.ShowBaseDataForm(e.Row, "FLot", "BD_BatchMainFile");
				e.Cancel = true;
				return;
			case "FBOMID.FNUMBER":
				this.ShowBaseDataForm(e.Row, "FBomId", "ENG_BOM");
				e.Cancel = true;
				return;
			case "FBASEQTY":
			case "FQTY":
			case "FSECQTY":
				this.ShowInOutDetailRpt(e.Row);
				e.Cancel = true;
				return;
			case "FSTOCKSTATUSID.FNAME":
				this.ShowBaseDataForm(e.Row, "FSTOCKSTATUSID", "BD_StockStatus");
				e.Cancel = true;
				break;

				return;
			}
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x0006B25E File Offset: 0x0006945E
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (StringUtils.EqualsIgnoreCase(e.Key, "FList"))
			{
				this.SummaryFieldCollect();
			}
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0006B280 File Offset: 0x00069480
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			if (this.isFromQuery && this.queryMode == 1 && this.needReturnData.Equals("1") && this.ListView.SelectedRowsInfo != null && this.ListView.CurrentSelectedRowInfo != null)
			{
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "ReturnDetailData", "");
			}
			e.Cancel = true;
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x0006B2F8 File Offset: 0x000694F8
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			base.BeforeFilterGridF7Select(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FLOT.FNUMBER"))
				{
					return;
				}
				List<long> list = new List<long>(this.ListModel.FilterParameter.IsolationOrgList);
				if (list != null && list.Count > 0)
				{
					string text = string.Format(" FUSEORGID IN ({0}) AND FBIZTYPE = '1' AND FLotStatus = '1' ", string.Join<long>(",", list));
					if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = text;
						return;
					}
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
					return;
				}
				else
				{
					e.ListFilterParameter.Filter = " 1 <> 1";
				}
			}
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x0006B400 File Offset: 0x00069600
		public override List<TreeNode> GetTreeViewData(TreeNodeArgs treeNodeArgs)
		{
			if (!string.IsNullOrEmpty(treeNodeArgs.NodeId) && this._materialGropField != null && ((ITreeListModel)this.Model).CurrGroupId.Equals(this._materialGropField.Id))
			{
				List<TreeNode> result = new List<TreeNode>();
				GroupField groupField = (from p in this._materialMeta.BusinessInfo.GetForm().FormGroups
				where p.GroupFieldKey.Equals("FMaterialGroup")
				select p).FirstOrDefault<FormGroup>().GroupField as GroupField;
				GroupTreeParameter groupTreeParameter = new GroupTreeParameter(this._materialMeta.BusinessInfo, groupField)
				{
					IsLookUp = true,
					IsFilterGroupID = true,
					IsFilterGroupDataRule = false,
					ParentGroupId = Convert.ToInt64(treeNodeArgs.NodeId)
				};
				List<GroupTreeNodeInfo> groupTreeChildNodes = PermissionServiceHelper.GetGroupTreeChildNodes(base.Context, groupTreeParameter);
				if (groupTreeChildNodes == null)
				{
					return result;
				}
				BaseDataField baseDataField = (from p in this.View.BillBusinessInfo.GetForm().FormGroups
				where p.GroupFieldKey.Equals("FMaterialGroup")
				select p).FirstOrDefault<FormGroup>().GroupField as BaseDataField;
				GroupField.GroupFieldDisplayType displayType = (baseDataField == null) ? groupField.DisplayType : baseDataField.GroupDisplayType;
				GroupField.GroupFieldSortType sortType = (baseDataField == null) ? groupField.SortType : baseDataField.GroupSortType;
				return InventoryList.GetTreeFromGroupList(groupTreeChildNodes, displayType, sortType);
			}
			else
			{
				if (string.IsNullOrEmpty(treeNodeArgs.NodeId) || this._stockGropField == null || !((ITreeListModel)this.Model).CurrGroupId.Equals(this._stockGropField.Id))
				{
					return null;
				}
				List<TreeNode> result2 = new List<TreeNode>();
				GroupField groupField2 = (from p in this._stockMeta.BusinessInfo.GetForm().FormGroups
				where p.GroupFieldKey.Equals("FGroup")
				select p).FirstOrDefault<FormGroup>().GroupField as GroupField;
				GroupTreeParameter groupTreeParameter2 = new GroupTreeParameter(this._stockMeta.BusinessInfo, groupField2)
				{
					IsLookUp = true,
					IsFilterGroupID = true,
					IsFilterGroupDataRule = false,
					ParentGroupId = Convert.ToInt64(treeNodeArgs.NodeId)
				};
				List<GroupTreeNodeInfo> groupTreeChildNodes2 = PermissionServiceHelper.GetGroupTreeChildNodes(base.Context, groupTreeParameter2);
				if (groupTreeChildNodes2 == null)
				{
					return result2;
				}
				BaseDataField baseDataField2 = (from p in this.View.BillBusinessInfo.GetForm().FormGroups
				where p.GroupFieldKey.Equals("FStockGroup")
				select p).FirstOrDefault<FormGroup>().GroupField as BaseDataField;
				GroupField.GroupFieldDisplayType displayType2 = (baseDataField2 == null) ? groupField2.DisplayType : baseDataField2.GroupDisplayType;
				GroupField.GroupFieldSortType sortType2 = (baseDataField2 == null) ? groupField2.SortType : baseDataField2.GroupSortType;
				return InventoryList.GetTreeFromGroupList(groupTreeChildNodes2, displayType2, sortType2);
			}
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0006B6D7 File Offset: 0x000698D7
		public override void TreeLoadData(TreeLoadDataArgs e)
		{
			base.TreeLoadData(e);
			this.SetGroupToolBarVisible();
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0006B6E8 File Offset: 0x000698E8
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			if (this._materialGropField != null && ((ITreeListModel)this.Model).CurrGroupId.Equals(this._materialGropField.Id))
			{
				TreeViewFilterParameter treeViewFilterParameter = (TreeViewFilterParameter)((ITreeListModel)this.Model).TreeViewFilterParameter;
				treeViewFilterParameter.IgnoreSelectedGroupIds = true;
				treeViewFilterParameter.FilterString = this.GetFilterSqlForMaterialGroupTree(e.NodeId);
				this._treeNodeClick = true;
				return;
			}
			if (this._stockGropField != null && ((ITreeListModel)this.Model).CurrGroupId.Equals(this._stockGropField.Id))
			{
				TreeViewFilterParameter treeViewFilterParameter2 = (TreeViewFilterParameter)((ITreeListModel)this.Model).TreeViewFilterParameter;
				treeViewFilterParameter2.IgnoreSelectedGroupIds = true;
				treeViewFilterParameter2.FilterString = this.GetFilterSqlForStockGroupTree(e.NodeId);
				this._treeNodeClick = true;
			}
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0006B7D8 File Offset: 0x000699D8
		private static List<TreeNode> GetTreeFromGroupList(List<GroupTreeNodeInfo> groupTreeNodes, GroupField.GroupFieldDisplayType displayType, GroupField.GroupFieldSortType sortType)
		{
			List<TreeNode> list = new List<TreeNode>();
			if (groupTreeNodes == null)
			{
				return list;
			}
			List<GroupTreeNodeInfo> list2;
			switch (sortType)
			{
			case 1:
				list2 = (from p in groupTreeNodes
				orderby p.Id
				select p).ToList<GroupTreeNodeInfo>();
				break;
			case 2:
				list2 = (from p in groupTreeNodes
				orderby p.Number
				select p).ToList<GroupTreeNodeInfo>();
				break;
			case 3:
				list2 = (from p in groupTreeNodes
				orderby p.Name
				select p).ToList<GroupTreeNodeInfo>();
				break;
			default:
				list2 = groupTreeNodes;
				break;
			}
			foreach (GroupTreeNodeInfo groupTreeNodeInfo in list2)
			{
				string text;
				switch (displayType)
				{
				case 0:
					text = string.Format("{0}({1})", groupTreeNodeInfo.Number, groupTreeNodeInfo.Name);
					break;
				case 1:
					text = groupTreeNodeInfo.Name;
					break;
				case 2:
					text = groupTreeNodeInfo.Number;
					break;
				default:
					text = string.Format("{0}({1})", groupTreeNodeInfo.Number, groupTreeNodeInfo.Name);
					break;
				}
				list.Add(new TreeNode
				{
					children = null,
					id = groupTreeNodeInfo.Id.ToString(),
					parentid = groupTreeNodeInfo.ParentId.ToString(),
					text = text,
					Style = (groupTreeNodeInfo.IsRuleValid ? string.Empty : "invalid"),
					cls = "parentnode",
					Number = groupTreeNodeInfo.Number,
					Name = groupTreeNodeInfo.Name
				});
			}
			return list;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0006B9DC File Offset: 0x00069BDC
		private string GetFilterSqlForMaterialGroupTree(string currentNodeID)
		{
			string text = "";
			if (Convert.ToInt64(currentNodeID) > 0L)
			{
				bool flag = this.IsDisplayChildData();
				string arg = string.Format("TGRP01.FID={0}", currentNodeID);
				string text2 = " TMAT01.FMASTERID=t0.FMATERIALID AND TMAT01.FUSEORGID=t0.FSTOCKORGID ";
				if (!flag)
				{
					text = string.Format(" EXISTS ( SELECT 1 FROM T_BD_MATERIAL TMAT01 INNER JOIN T_BD_MATERIALGROUP TGRP01 ON TMAT01.FMATERIALGROUP = TGRP01.FID WHERE {0} AND {1} )", arg, text2);
				}
				else
				{
					FormGroup formGroup = (from p in this._materialMeta.BusinessInfo.GetForm().FormGroups
					where p.GroupFieldKey.Equals("FMaterialGroup")
					select p).FirstOrDefault<FormGroup>();
					if (formGroup == null)
					{
						return text;
					}
					GroupNodeInfo groupNodeInfo = ListDataServiceHelper.GetGroupNodeInfo(this.View.Context, formGroup, currentNodeID.ToString().Split(new char[]
					{
						','
					})).FirstOrDefault<GroupNodeInfo>();
					if (groupNodeInfo == null)
					{
						return text;
					}
					string text3 = string.IsNullOrWhiteSpace(groupNodeInfo.FullParentId) ? "" : groupNodeInfo.FullParentId.Trim();
					text3 = text3 + "." + groupNodeInfo.Id.ToString();
					text = string.Format("TGRP01.FFULLPARENTID LIKE  '{0}.%' OR TGRP01.FFULLPARENTID='{0}'", text3);
					text = string.Format(" EXISTS ( SELECT 1 FROM T_BD_MATERIAL TMAT01 INNER JOIN T_BD_MATERIALGROUP TGRP01 ON TMAT01.FMATERIALGROUP = TGRP01.FID WHERE ({0} OR ({1})) AND {2} )", arg, text, text2);
				}
			}
			return text;
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x0006BB14 File Offset: 0x00069D14
		private string GetFilterSqlForStockGroupTree(string currentNodeID)
		{
			string text = "";
			if (Convert.ToInt64(currentNodeID) > 0L)
			{
				bool flag = this.IsDisplayChildData();
				string arg = string.Format("TGRP01.FID={0}", currentNodeID);
				string text2 = " TSTO01.FSTOCKID=t0.FSTOCKID ";
				if (!flag)
				{
					text = string.Format(" EXISTS ( SELECT 1 FROM T_BD_STOCK TSTO01 INNER JOIN T_BD_STOCKGROUP TGRP01 ON TSTO01.FGROUP = TGRP01.FID WHERE {0} AND {1} )", arg, text2);
				}
				else
				{
					FormGroup formGroup = (from p in this._stockMeta.BusinessInfo.GetForm().FormGroups
					where p.GroupFieldKey.Equals("FGroup")
					select p).FirstOrDefault<FormGroup>();
					if (formGroup == null)
					{
						return text;
					}
					GroupNodeInfo groupNodeInfo = ListDataServiceHelper.GetGroupNodeInfo(this.View.Context, formGroup, currentNodeID.ToString().Split(new char[]
					{
						','
					})).FirstOrDefault<GroupNodeInfo>();
					if (groupNodeInfo == null)
					{
						return text;
					}
					string text3 = string.IsNullOrWhiteSpace(groupNodeInfo.FullParentId) ? "" : groupNodeInfo.FullParentId.Trim();
					text3 = text3 + "." + groupNodeInfo.Id.ToString();
					text = string.Format("TGRP01.FFULLPARENTID LIKE  '{0}.%' OR TGRP01.FFULLPARENTID='{0}'", text3);
					text = string.Format(" EXISTS ( SELECT 1 FROM T_BD_STOCK TSTO01 INNER JOIN T_BD_STOCKGROUP TGRP01 ON TSTO01.FGROUP = TGRP01.FID WHERE ({0} OR ({1})) AND {2} )", arg, text, text2);
				}
			}
			return text;
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0006BC3C File Offset: 0x00069E3C
		private bool IsDisplayChildData()
		{
			bool result = false;
			if (this.View.Model.ParameterData != null && this.View.Model.ParameterData.DynamicObjectType.Properties.ContainsKey("DisplayChildData"))
			{
				result = !Convert.ToBoolean(this.View.Model.ParameterData["DisplayChildData"]);
			}
			return result;
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0006BCA8 File Offset: 0x00069EA8
		private void SetMaterialGroupControl()
		{
			if (Convert.ToBoolean(this._materialGropField.SupportGroup) || Convert.ToBoolean(this._stockGropField.SupportGroup))
			{
				string id = this.View.ParentFormView.BillBusinessInfo.GetForm().Id;
				if (!id.StartsWith("BOS_"))
				{
					SplitContainer control = this.View.GetControl<SplitContainer>("FListSpliter");
					if (control != null)
					{
						control.HideFirstPanel(true);
						if (!this.isFromDetailQuery && !this.isInvSynGyQuery)
						{
							control.SetSplitButtonVisible(false);
						}
					}
				}
				this.SetGroupToolBarVisible();
			}
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x0006BD3C File Offset: 0x00069F3C
		private void SetGroupToolBarVisible()
		{
			if (Convert.ToBoolean(this._materialGropField.AllowEditGroup) || Convert.ToBoolean(this._stockGropField.AllowEditGroup))
			{
				this.View.GetBarItem("FGroupToolbar", "tbAddNewGroup").Visible = false;
				this.View.GetBarItem("FGroupToolbar", "tbModify").Visible = false;
				this.View.GetBarItem("FGroupToolbar", "tbDelete").Visible = false;
				this.View.GetBarItem("FGroupToolbar", "tbRefreshTree").Visible = false;
			}
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x0006BDDC File Offset: 0x00069FDC
		private void InitializaMaterialTreeInfo()
		{
			this._treeNodeClick = true;
			this._materialGropField = (this.View.BillBusinessInfo.GetField("FMaterialGroup") as BaseDataField);
			if (Convert.ToBoolean(this._materialGropField.SupportGroup))
			{
				this._materialMeta = (MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata);
			}
			this._stockGropField = (this.View.BillBusinessInfo.GetField("FStockGroup") as BaseDataField);
			if (Convert.ToBoolean(this._stockGropField.SupportGroup))
			{
				this._stockMeta = (MetaDataServiceHelper.Load(base.Context, "BD_STOCK", true) as FormMetadata);
			}
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x0006BE8C File Offset: 0x0006A08C
		private void AppendFilter(StringBuilder retBuilder, string filter)
		{
			filter = filter.TrimStart(new char[0]);
			if (StringUtils.EqualsIgnoreCase("AND", filter.Substring(0, 3)))
			{
				retBuilder.AppendFormat(" {0} ", filter);
				return;
			}
			retBuilder.AppendFormat(" AND ({0}) ", filter);
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x0006BED4 File Offset: 0x0006A0D4
		private bool LockUnLockInventoty(bool isLock, List<string> invIds = null)
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (invIds == null && selectedRowsInfo.Count == 0)
			{
				string text = isLock ? ResManager.LoadKDString("锁库 操作需选择一个物料!", "004023030000262", 5, new object[0]) : ResManager.LoadKDString("解锁 操作需选择一个物料!", "004023030000265", 5, new object[0]);
				this.View.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			List<string> list;
			if (invIds == null)
			{
				list = (from p in selectedRowsInfo
				select p.PrimaryKeyValue).ToList<string>();
			}
			else
			{
				list = invIds;
			}
			string text2 = string.Join("','", list);
			text2 = "'" + text2 + "'";
			if (isLock && !StockServiceHelper.HaveLockableInvData(this.View.Context, text2, this.qOrgId))
			{
				string text = ResManager.LoadKDString("选择的即时库存记录可锁库数量为0或【仓库】（或者【库存状态】）不允许锁库，请重新选择", "004023030002164", 5, new object[0]);
				this.ListView.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			if (!isLock && !StockServiceHelper.HaveLockInfo(this.View.Context, text2, "Inv"))
			{
				string text = ResManager.LoadKDString("即时库存未锁库，不能解锁。", "004023030002167", 5, new object[0]);
				this.ListView.ShowWarnningMessage(text, "", 0, null, 1);
				return false;
			}
			string formID = isLock ? "STK_InventoryLock" : "STK_UnLockStockOperate";
			this.ShowLockForm(formID, list);
			return true;
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x0006C044 File Offset: 0x0006A244
		private void SetToolBarAndQFilterVisible(string detailQueryType)
		{
			bool flag = (this.isFromDetailQuery && detailQueryType == "2") || !this.isFromDetailQuery;
			this.View.GetControl<Panel>("FQuickFilterPanel").Visible = !flag;
			this.View.GetControl<QuickFilter>("FQkFilterPanel").Visible = !flag;
			this.View.GetControl<Panel>("FQuickFilterPanel").Visible = flag;
			this.View.GetControl<QuickFilter>("FQkFilterPanel").Visible = flag;
			if (this.isFromDetailQuery)
			{
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "SWITCHQUERYTYPE", detailQueryType);
				BarDataManager listMenu = this.ListView.BillLayoutInfo.GetFormAppearance().ListMenu;
				foreach (BarItem barItem in listMenu.BarItems)
				{
					this.View.GetMainBarItem(barItem.Name).Visible = false;
				}
			}
			if (this.isInvSynGyQuery)
			{
				BarDataManager listMenu2 = this.ListView.BillLayoutInfo.GetFormAppearance().ListMenu;
				foreach (BarItem barItem2 in listMenu2.BarItems)
				{
					this.View.GetMainBarItem(barItem2.Name).Visible = false;
				}
				this.View.GetMainBarItem("tbFilter").Visible = true;
				this.View.GetMainBarItem("tbSplitButton").Visible = true;
				this.View.GetMainBarItem("tbExport").Visible = true;
				this.View.GetMainBarItem("tbExportSetting").Visible = true;
				this.View.GetMainBarItem("tbRefresh").Visible = true;
				this.View.GetMainBarItem("tbOption").Visible = true;
			}
		}

		// Token: 0x0600084B RID: 2123 RVA: 0x0006C26C File Offset: 0x0006A46C
		private string GetOrgFilter(FilterArgs e, string queryStockOrgIds)
		{
			if (this._filterObject == null)
			{
				DataRuleFilterParamenter dataRuleFilterParamenter = new DataRuleFilterParamenter("STK_Inventory", 1)
				{
					PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
					BusinessInfo = this.View.BillBusinessInfo,
					IsLookUp = false
				};
				this._filterObject = PermissionServiceHelper.LoadDataRuleFilter(base.Context, dataRuleFilterParamenter);
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<long> list;
			if (this._ignoreSchemeFilter && !string.IsNullOrWhiteSpace(queryStockOrgIds))
			{
				list = new List<long>(from p in queryStockOrgIds.Split(new char[]
				{
					','
				}, StringSplitOptions.RemoveEmptyEntries)
				select Convert.ToInt64(p));
				this.ListModel.FilterParameter.IsolationOrgList = list;
				((ListModel)this.ListModel).IsolationOrgList = list;
			}
			else
			{
				list = new List<long>(this.ListModel.FilterParameter.IsolationOrgList);
			}
			if (this.View.Context.IsStandardEdition() && !list.Contains(this.View.Context.CurrentOrganizationInfo.ID))
			{
				list.Add(this.View.Context.CurrentOrganizationInfo.ID);
			}
			if (!string.IsNullOrWhiteSpace(this._inputOrgIds))
			{
				string[] array = this._inputOrgIds.Split(new char[]
				{
					','
				}, StringSplitOptions.RemoveEmptyEntries);
				for (int i = list.Count - 1; i >= 0; i--)
				{
					long num = list[i];
					bool flag = false;
					foreach (string value in array)
					{
						if (Convert.ToInt64(value) == num)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list.RemoveAt(i);
					}
				}
			}
			if (list.Count < 1)
			{
				return "";
			}
			if (list.Count < 50)
			{
				stringBuilder.AppendFormat(" (FSTOCKORGID IN ({0}) ", string.Join<long>(",", list));
			}
			else
			{
				stringBuilder.AppendLine().AppendFormat(" (FSTOCKORGID IN (SELECT /*+ cardinality(TOGS {0})*/ TOGS.FID FROM table(fn_StrSplit(@FSelOrgID, ',', 1)) TOGS ) ", list.Count);
				if (e.SqlParams == null)
				{
					e.SqlParams = new List<SqlParam>();
				}
				e.SqlParams.Add(new SqlParam("@FSelOrgID", 161, list.ToArray()));
			}
			if (!string.IsNullOrWhiteSpace(this._filterObject.FilterString))
			{
				stringBuilder.AppendFormat("{0}", " AND " + this._filterObject.FilterString);
				if (this._filterObject.SQLFilterParams.Count > 0)
				{
					if (e.SqlParams == null)
					{
						e.SqlParams = new List<SqlParam>();
					}
					e.SqlParams.AddRange(this._filterObject.SQLFilterParams);
				}
			}
			stringBuilder.Append(" ) ");
			return stringBuilder.ToString();
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x0006C570 File Offset: 0x0006A770
		private void AddMustSelFields()
		{
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
			this._needRegexFields = new List<string>();
			bool flag = columnInfo.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FProduceDate"));
			if (flag)
			{
				this.addSpecField(columnInfo, fieldList, "FLot", "FLot.FNumber", 56, false);
				this._needRegexFields.Add("FProduceDate");
			}
			bool flag2 = columnInfo.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FExpiryDate"));
			if (flag2)
			{
				this.addSpecField(columnInfo, fieldList, "FLot", "FLot.FNumber", 56, false);
				this._needRegexFields.Add("FExpiryDate");
			}
			this.ShowQty(columnInfo, fieldList);
			bool flag3 = columnInfo.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FBaseLockQty"));
			if (flag3 && !this._needRegexFields.Contains("FBaseLockQty"))
			{
				this._needRegexFields.Add("FBaseLockQty");
			}
			this.ShowLockQty(columnInfo, fieldList);
			this.ShowBaseAvbQty(columnInfo, fieldList);
			this.ShowAvbQty(columnInfo, fieldList);
			flag3 = columnInfo.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FSecLockQty"));
			if (flag3 && !this._needRegexFields.Contains("FSecLockQty"))
			{
				this._needRegexFields.Add("FSecLockQty");
			}
			this.ShowSecAvbQty(columnInfo, fieldList);
			this.ShowMaterialAndUnit(columnInfo, fieldList);
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x0006C708 File Offset: 0x0006A908
		private void ShowMaterialAndUnit(List<ColumnField> selCols, List<Field> fields)
		{
			if (this._needRegexFields.Count <= 0)
			{
				return;
			}
			if (this._needRegexFields.Contains("FQty") || this._needRegexFields.Contains("FAVBQty") || this._needRegexFields.Contains("FLockQty"))
			{
				this.addSpecField(selCols, fields, "FStockUnitId", "FStockUnitId.FName", 56, false);
				this.addSpecField(selCols, fields, "FMaterialId", "FMaterialId.FNumber", 56, false);
			}
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x0006C798 File Offset: 0x0006A998
		private void ShowSecAvbQty(List<ColumnField> selCols, List<Field> fields)
		{
			bool flag = selCols.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FSecAVBQty"));
			if (flag && !this._needRegexFields.Contains("FSecAVBQty"))
			{
				this._needRegexFields.Add("FSecAVBQty");
			}
			if (flag)
			{
				this.addSpecField(selCols, fields, "FSecQty", "FSecQty", 106, false);
				this.addSpecField(selCols, fields, "FSecLockQty", "FSecLockQty", 106, true);
			}
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x0006C830 File Offset: 0x0006AA30
		private void ShowAvbQty(List<ColumnField> selCols, List<Field> fields)
		{
			bool flag = selCols.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FAVBQty"));
			if (flag && !this._needRegexFields.Contains("FAVBQty"))
			{
				this._needRegexFields.Add("FAVBQty");
			}
			if (flag)
			{
				this.addSpecField(selCols, fields, "FBaseQty", "FBaseQty", 106, false);
				this.addSpecField(selCols, fields, "FBaseLockQty", "FBaseLockQty", 106, true);
				this.addSpecField(selCols, fields, "FBaseAVBQty", "FBaseAVBQty", 106, true);
			}
		}

		// Token: 0x06000850 RID: 2128 RVA: 0x0006C8DC File Offset: 0x0006AADC
		private void ShowBaseAvbQty(List<ColumnField> selCols, List<Field> fields)
		{
			bool flag = selCols.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FBaseAVBQty"));
			if (flag && !this._needRegexFields.Contains("FBaseAVBQty"))
			{
				this._needRegexFields.Add("FBaseAVBQty");
			}
			if (flag)
			{
				this.addSpecField(selCols, fields, "FBaseQty", "FBaseQty", 106, false);
				this.addSpecField(selCols, fields, "FBaseLockQty", "FBaseLockQty", 106, true);
			}
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x0006C974 File Offset: 0x0006AB74
		private void ShowLockQty(List<ColumnField> selCols, List<Field> fields)
		{
			bool flag = selCols.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FLockQty"));
			if (flag && !this._needRegexFields.Contains("FLockQty"))
			{
				this._needRegexFields.Add("FLockQty");
			}
			if (flag)
			{
				this.addSpecField(selCols, fields, "FBaseLockQty", "FBaseLockQty", 106, true);
			}
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x0006C9F4 File Offset: 0x0006ABF4
		private void ShowQty(List<ColumnField> selCols, List<Field> fields)
		{
			bool flag = selCols.Exists((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FQty"));
			if (flag && !this._needRegexFields.Contains("FQty"))
			{
				this._needRegexFields.Add("FQty");
			}
			if (flag)
			{
				this.addSpecField(selCols, fields, "FBaseQty", "FBaseQty", 106, false);
			}
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x0006CA90 File Offset: 0x0006AC90
		private void addSpecField(List<ColumnField> selCols, List<Field> fields, string fieldKey, string selKey, SqlStorageType colType, bool needCal)
		{
			Field field = fields.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, fieldKey));
			if (selCols.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, selKey)) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = selKey,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = colType,
					ColWidth = 0,
					CoreField = false,
					DefaultColWidth = 0,
					DefaultVisible = false,
					EntityCaption = field.Entity.Name,
					EntityKey = field.EntityKey,
					FieldName = field.FieldName,
					IsHyperlink = false,
					Visible = false
				};
				this.ListModel.FilterParameter.ColumnInfo.Add(item);
				if (needCal && !this._needRegexFields.Contains(fieldKey))
				{
					this._needRegexFields.Add(fieldKey);
				}
			}
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x0006CBB4 File Offset: 0x0006ADB4
		private Dictionary<string, long> GetSelectedRowIdOrgs()
		{
			Dictionary<string, long> result = new Dictionary<string, long>();
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count == 0)
			{
				return result;
			}
			List<string> list = (from p in selectedRowsInfo
			select p.PrimaryKeyValue).ToList<string>();
			return StockServiceHelper.GetInventoryOrgInfo(base.Context, list);
		}

		// Token: 0x06000855 RID: 2133 RVA: 0x0006CC14 File Offset: 0x0006AE14
		private void ShowLockForm(string formID, List<string> selIds)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = formID;
			dynamicFormShowParameter.CustomParams.Add("OpType", "Inv");
			string text = string.Join("','", selIds);
			text = "'" + text + "'";
			if (!string.IsNullOrEmpty(text))
			{
				dynamicFormShowParameter.CustomParams.Add("Parameters", string.Join(",", new string[]
				{
					text
				}));
				dynamicFormShowParameter.CustomParams.Add("OrgId", this.qOrgId.ToString());
			}
			this.lockselIds = selIds;
			this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.AfterShowLock));
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x0006CCE0 File Offset: 0x0006AEE0
		protected void AfterShowLock(FormResult result)
		{
			if (result.ReturnData != null)
			{
				this.View.RefreshByFilter();
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "RefreshSumFormPage", "");
			}
			this.lockselIds = new List<string>();
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x0006CD78 File Offset: 0x0006AF78
		private bool CheckPermission(BarItemClickEventArgs e, out List<string> permittedInvIds)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string text = string.Empty;
			string id = "STK_Inventory";
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (a == "TBLOCK")
				{
					text = "6ff9853429aa4384892b4f5d0f86dd1b";
					id = "STK_LockStock";
					goto IL_D6;
				}
				if (a == "TBUNLOCK")
				{
					text = "f483e76da3ba4fdb96052059b5d71c6c";
					id = "STK_LockStock";
					goto IL_D6;
				}
				if (a == "TBLOCKLIST")
				{
					text = "6e44119a58cb4a8e86f6c385e14a17ad";
					id = "STK_LockStock";
					goto IL_D6;
				}
			}
			text = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			IL_D6:
			permittedInvIds = new List<string>();
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}
			Dictionary<string, long> selectedRowIdOrgs = this.GetSelectedRowIdOrgs();
			if (selectedRowIdOrgs.Keys.Count < 1)
			{
				return true;
			}
			List<BusinessObject> list = new List<BusinessObject>();
			foreach (long num in selectedRowIdOrgs.Values.Distinct<long>())
			{
				list.Add(new BusinessObject(num)
				{
					Id = id,
					pkId = num.ToString()
				});
			}
			List<PermissionAuthResult> list2 = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, list, text);
			if (list2 == null || list2.Count < 1)
			{
				return false;
			}
			permittedInvIds = new List<string>();
			foreach (string text2 in selectedRowIdOrgs.Keys)
			{
				string orgId = selectedRowIdOrgs[text2].ToString();
				PermissionAuthResult permissionAuthResult = list2.FirstOrDefault((PermissionAuthResult p) => p.Id.Equals(orgId) && p.Passed);
				if (permissionAuthResult != null)
				{
					permittedInvIds.Add(text2);
				}
			}
			return permittedInvIds.Count > 0;
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x0006D004 File Offset: 0x0006B204
		private bool CheckViewPermission(int rowindex, string formID)
		{
			string text = "6e44119a58cb4a8e86f6c385e14a17ad";
			ListSelectedRow listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == rowindex);
			List<long> list = new List<long>();
			if (listSelectedRow == null)
			{
				return false;
			}
			if (listSelectedRow.MainOrgId > 0L)
			{
				list.Add(listSelectedRow.MainOrgId);
			}
			List<BusinessObject> list2 = new List<BusinessObject>();
			foreach (long num in list)
			{
				list2.Add(new BusinessObject(num)
				{
					Id = formID,
					pkId = num.ToString()
				});
			}
			List<PermissionAuthResult> list3 = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, list2, text);
			if (list3 == null || list3.Count < 1)
			{
				return false;
			}
			bool result = false;
			using (List<long>.Enumerator enumerator2 = list.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					long orgId = enumerator2.Current;
					PermissionAuthResult permissionAuthResult = list3.FirstOrDefault(delegate(PermissionAuthResult p)
					{
						string id = p.Id;
						long orgId = orgId;
						return id.Equals(orgId.ToString()) && p.Passed;
					});
					if (permissionAuthResult != null)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x06000859 RID: 2137 RVA: 0x0006D188 File Offset: 0x0006B388
		private void ShowBaseDataForm(int selRow, string idFieldKey, string formId)
		{
			if (selRow <= 0)
			{
				return;
			}
			if (!this.CheckViewPermission(selRow, formId))
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("您没有查看权限！", "004023000022180", 5, new object[0]), "", 0, null, 1);
				return;
			}
			ListSelectedRow listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == selRow);
			if (listSelectedRow == null || string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
			{
				return;
			}
			IEnumerable<InvQueryRetRecord> inventoryDatas = StockServiceHelper.GetInventoryDatas(this.ListView.Context, listSelectedRow.PrimaryKeyValue, 0L);
			if (inventoryDatas == null || inventoryDatas.Count<InvQueryRetRecord>() <= 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("找不到对应的基础资料数据，可能即时库存数据已经被删除，请刷新数据后重试！", "004023030004741", 5, new object[0]), 0);
				return;
			}
			InvQueryRetRecord invQueryRetRecord = inventoryDatas.ToArray<InvQueryRetRecord>()[0];
			string text = "";
			if (StringUtils.EqualsIgnoreCase(idFieldKey, "FStockId"))
			{
				text = invQueryRetRecord.StockID.ToString();
			}
			else if (StringUtils.EqualsIgnoreCase(idFieldKey, "FMaterialId"))
			{
				text = invQueryRetRecord.MaterialID.ToString();
			}
			else if (StringUtils.EqualsIgnoreCase(idFieldKey, "FLot"))
			{
				text = invQueryRetRecord.Lot.ToString();
			}
			else if (StringUtils.EqualsIgnoreCase(idFieldKey, "FBomId"))
			{
				text = invQueryRetRecord.BOMID.ToString();
			}
			else if (StringUtils.EqualsIgnoreCase(idFieldKey, "FStockStatusID"))
			{
				text = invQueryRetRecord.StockStatusID.ToString();
			}
			if (string.IsNullOrWhiteSpace(text) || text == "0")
			{
				this.View.ShowMessage(ResManager.LoadKDString("找不到对应的基础资料数据！", "004023030004759", 5, new object[0]), 0);
				return;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, formId, true) as FormMetadata;
			FilterObjectByDataRuleParamenter filterObjectByDataRuleParamenter = new FilterObjectByDataRuleParamenter(formMetadata.BusinessInfo, new List<string>
			{
				text
			});
			List<string> list = PermissionServiceHelper.FilterObjectByDataRule(base.Context, filterObjectByDataRuleParamenter);
			if (!list.Contains(text))
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("您没有查看权限！", "004023000022180", 5, new object[0]), "", 0, null, 1);
				return;
			}
			BillShowParameter billShowParameter = new BillShowParameter
			{
				FormId = formId,
				PKey = text,
				Status = 1,
				AllowNavigation = false
			};
			this.ListView.ShowForm(billShowParameter);
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x0006D408 File Offset: 0x0006B608
		private void ShowInOutDetailRpt(int selRow)
		{
			if (selRow <= 0)
			{
				return;
			}
			ListSelectedRow listSelectedRow = this.ListView.CurrentPageRowsInfo.FirstOrDefault((ListSelectedRow o) => o.RowKey == selRow);
			if (listSelectedRow == null || string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
			{
				return;
			}
			if (!this.CheckViewPermission(selRow, "STK_StockDetailRpt"))
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("您在该库存组织下没有物料收发明细报表的查看权限!", "004023030009357", 5, new object[0]), "", 0, null, 1);
				return;
			}
			IEnumerable<InvQueryRetRecord> inventoryDatas = StockServiceHelper.GetInventoryDatas(this.ListView.Context, listSelectedRow.PrimaryKeyValue, 0L);
			if (inventoryDatas == null || inventoryDatas.Count<InvQueryRetRecord>() <= 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("找不到对应的基础资料数据，可能即时库存数据已经被删除，请刷新数据后重试！", "004023030004741", 5, new object[0]), 0);
				return;
			}
			InvQueryRetRecord invQueryRetRecord = inventoryDatas.ToArray<InvQueryRetRecord>()[0];
			if (invQueryRetRecord.StockOrgID <= 0L)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择库存组织有值的数据执行链接查询！", "004023030004744", 5, new object[0]), 0);
				return;
			}
			DateTime d = DateTime.MinValue;
			DataTable stockOrgAcctLastCloseDate = CommonServiceHelper.GetStockOrgAcctLastCloseDate(base.Context, invQueryRetRecord.StockOrgID.ToString());
			if (stockOrgAcctLastCloseDate.Rows.Count == 1 && !(stockOrgAcctLastCloseDate.Rows[0]["FCLOSEDATE"] is DBNull) && !string.IsNullOrWhiteSpace(stockOrgAcctLastCloseDate.Rows[0]["FCLOSEDATE"].ToString()) && DateTime.Parse(stockOrgAcctLastCloseDate.Rows[0]["FCLOSEDATE"].ToString()) != DateTime.MinValue)
			{
				d = DateTime.Parse(stockOrgAcctLastCloseDate.Rows[0]["FCLOSEDATE"].ToString());
			}
			if (d == DateTime.MinValue)
			{
				if (!CommonServiceHelper.HaveStockInitCloseRecord(base.Context, invQueryRetRecord.StockOrgID))
				{
					this.View.ShowMessage(ResManager.LoadKDString("库存组织未结束初始化，请先在库存系统结束初始化！", "004023030004747", 5, new object[0]), 0);
					return;
				}
				object updateStockDate = StockServiceHelper.GetUpdateStockDate(base.Context, invQueryRetRecord.StockOrgID);
				if (updateStockDate == null || string.IsNullOrWhiteSpace(updateStockDate.ToString()))
				{
					this.View.ShowMessage(ResManager.LoadKDString("库存组织启用日期获取失败！", "004023030004750", 5, new object[0]), 0);
					return;
				}
				d = DateTime.Parse(updateStockDate.ToString());
			}
			MoveReportShowParameter moveReportShowParameter = new MoveReportShowParameter();
			moveReportShowParameter.ParentPageId = this.View.PageId;
			moveReportShowParameter.MultiSelect = false;
			moveReportShowParameter.FormId = "STK_StockDetailRpt";
			moveReportShowParameter.Height = 700;
			moveReportShowParameter.Width = 950;
			moveReportShowParameter.IsShowFilter = false;
			moveReportShowParameter.CustomParams.Add("SourceBillFormId", "STK_Inventory");
			moveReportShowParameter.CustomParams.Add("SourceBillId", listSelectedRow.PrimaryKeyValue);
			moveReportShowParameter.CustomParams.Add("SourceOrgId", invQueryRetRecord.StockOrgID.ToString());
			moveReportShowParameter.CustomParams.Add("SourceBeginDate", d.ToString());
			moveReportShowParameter.CustomParams.Add("SourceEndDate", DateTimeFormatUtils.BeginDateTimeOfDay(DateTime.MaxValue).ToString());
			this.View.ShowForm(moveReportShowParameter);
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x0006D774 File Offset: 0x0006B974
		private void ViewInvSerial()
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count == 0)
			{
				this.View.ShowWarnningMessage(ResManager.LoadKDString("请先选择一条库存记录！", "004023030004753", 5, new object[0]), "", 0, null, 1);
				return;
			}
			List<string> list = (from p in selectedRowsInfo
			select p.PrimaryKeyValue).ToList<string>();
			if (!StockServiceHelper.HaveInStockSerialOnInv(base.Context, list))
			{
				return;
			}
			SysReportShowParameter sysReportShowParameter = new SysReportShowParameter();
			sysReportShowParameter.ParentPageId = this.View.PageId;
			sysReportShowParameter.MultiSelect = false;
			sysReportShowParameter.FormId = "STK_InvSerialRpt";
			sysReportShowParameter.Height = 700;
			sysReportShowParameter.Width = 950;
			sysReportShowParameter.IsShowFilter = false;
			sysReportShowParameter.CustomParams.Add("BillFormId", "STK_Inventory");
			sysReportShowParameter.CustomComplexParams.Add("BillIds", list);
			this.View.ShowForm(sysReportShowParameter);
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x0006D870 File Offset: 0x0006BA70
		private bool ShowLockStockList(List<string> permtedIds)
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count == 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "004023000014244", 5, new object[0]), 0);
				return true;
			}
			string text = string.Join("','", permtedIds);
			text = "'" + text + "'";
			if (!StockServiceHelper.HaveLockInfo(this.View.Context, text, "Inv"))
			{
				this.View.ShowMessage(ResManager.LoadKDString("无锁库信息！", "004023030008997", 5, new object[0]), 0);
				return true;
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.FormId = "STK_LockStock";
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.IsIsolationOrg = false;
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsLookUp = false;
			listShowParameter.CustomParams.Add("InvDetailIds", string.Join("','", permtedIds));
			this.View.ShowForm(listShowParameter);
			return false;
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x0006D97C File Offset: 0x0006BB7C
		private void SummaryFieldCollect()
		{
			if (!this.isFromDetailQuery)
			{
				return;
			}
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (selectedRowsInfo.Count == 0)
			{
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "UPDATESUMINFO", "");
				return;
			}
			DynamicObject parameterData = this.Model.ParameterData;
			BillUserParameterView billUserParameterView = parameterData;
			List<SummaryFieldEntry> summaryFieldEntry = billUserParameterView.SummaryFieldEntry;
			StringBuilder stringBuilder = new StringBuilder();
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			List<decimal> list2 = new List<decimal>();
			foreach (SummaryFieldEntry summaryFieldEntry2 in summaryFieldEntry)
			{
				if (summaryFieldEntry2.ListSummaryCheckBox && this._visibleFieldKeys.Contains(summaryFieldEntry2.FieldKey))
				{
					string text = summaryFieldEntry2.FieldText;
					int num = text.IndexOf('.');
					if (num > 0)
					{
						text = text.Substring(num + 1);
					}
					list.Add(new KeyValuePair<string, string>(summaryFieldEntry2.FieldKey, text));
					list2.Add(0m);
				}
			}
			if (list.Count < 1)
			{
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "UPDATESUMINFO", "");
				return;
			}
			foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
			{
				for (int i = 0; i < list.Count; i++)
				{
					List<decimal> list3;
					int index;
					(list3 = list2)[index = i] = list3[index] + Convert.ToDecimal(listSelectedRow.DataRow[list[i].Key]);
				}
			}
			stringBuilder.Append("：");
			for (int j = 0; j < list.Count; j++)
			{
				string text2 = list2[j].ToString().TrimEnd(new char[]
				{
					'0'
				});
				if (text2.EndsWith("."))
				{
					text2 = text2.Substring(0, text2.Length - 1);
				}
				if (text2 == "")
				{
					text2 = "0";
				}
				stringBuilder.AppendFormat("{0}：{1}；", list[j].Value, text2);
			}
			string text3 = ResManager.LoadKDString("当前选中行合计", "004023030009620", 5, new object[0]) + stringBuilder.ToString(0, stringBuilder.Length - 1);
			((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "UPDATESUMINFO", text3);
		}

		// Token: 0x04000315 RID: 789
		private const string PRODUCEDATEFIELDKEY = "FProduceDate";

		// Token: 0x04000316 RID: 790
		private const string EXPDATEFIELDKEY = "FExpiryDate";

		// Token: 0x04000317 RID: 791
		private const string LOTPRODUCEDATEFIELDKEY = "FLotProduceDate";

		// Token: 0x04000318 RID: 792
		private const string LOTEXPDATEFIELDKEY = "FLotExpiryDate";

		// Token: 0x04000319 RID: 793
		private bool isFromQuery;

		// Token: 0x0400031A RID: 794
		private bool _isInitial = true;

		// Token: 0x0400031B RID: 795
		private string sQueryPage = "";

		// Token: 0x0400031C RID: 796
		private string needReturnData = "";

		// Token: 0x0400031D RID: 797
		private long qOrgId;

		// Token: 0x0400031E RID: 798
		private int queryMode;

		// Token: 0x0400031F RID: 799
		private List<string> lockselIds = new List<string>();

		// Token: 0x04000320 RID: 800
		private bool usePLNReserve;

		// Token: 0x04000321 RID: 801
		private bool _useSN;

		// Token: 0x04000322 RID: 802
		private bool isFromDetailQuery;

		// Token: 0x04000323 RID: 803
		private string _queryFilter = "";

		// Token: 0x04000324 RID: 804
		private string _inputOrgIds = "";

		// Token: 0x04000325 RID: 805
		private bool _inputMoreFilter;

		// Token: 0x04000326 RID: 806
		private DataRuleFilterObject _filterObject;

		// Token: 0x04000327 RID: 807
		private string _joinQuerySort = "";

		// Token: 0x04000328 RID: 808
		private bool isShowExit;

		// Token: 0x04000329 RID: 809
		private List<string> _needRegexFields = new List<string>();

		// Token: 0x0400032A RID: 810
		private List<string> _visibleFieldKeys = new List<string>();

		// Token: 0x0400032B RID: 811
		private bool _isForceRefresh;

		// Token: 0x0400032C RID: 812
		private bool isInvSynGyQuery;

		// Token: 0x0400032D RID: 813
		private string _detailQueryType = "";

		// Token: 0x0400032E RID: 814
		private BaseDataField _materialGropField;

		// Token: 0x0400032F RID: 815
		private FormMetadata _materialMeta;

		// Token: 0x04000330 RID: 816
		private bool _treeNodeClick;

		// Token: 0x04000331 RID: 817
		private BaseDataField _stockGropField;

		// Token: 0x04000332 RID: 818
		private FormMetadata _stockMeta;

		// Token: 0x04000333 RID: 819
		private bool _ignoreSchemeFilter;

		// Token: 0x04000334 RID: 820
		private bool _useIgnoreSchemeFilterPara;

		// Token: 0x04000335 RID: 821
		private bool _callByFilterButton;

		// Token: 0x04000336 RID: 822
		private bool _callBySearchButton;

		// Token: 0x04000337 RID: 823
		private FieldAppearance prdDateApp;

		// Token: 0x04000338 RID: 824
		private FieldAppearance expDateApp;
	}
}
