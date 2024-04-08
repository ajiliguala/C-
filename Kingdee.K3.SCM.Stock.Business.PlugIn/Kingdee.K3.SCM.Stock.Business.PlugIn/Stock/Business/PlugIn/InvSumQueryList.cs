using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.BusinessFlow.ReserveLogic;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Metadata.QueryElement;
using Kingdee.BOS.Core.Objects.Permission.Objects;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Permission.Objects;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.Model.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.Core.SCM;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200009C RID: 156
	public class InvSumQueryList : AbstractListPlugIn
	{
		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060008F1 RID: 2289 RVA: 0x00076C4F File Offset: 0x00074E4F
		public string TransactionID
		{
			get
			{
				return this.transID;
			}
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x00076C58 File Offset: 0x00074E58
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.usePLNReserve = CommonServiceHelper.IsUsePLNReserve(this.View.Context);
			object systemProfile = CommonServiceHelper.GetSystemProfile(this.View.Context, 0L, "STK_StockParameter", "ControlSerialNo", "");
			if (systemProfile != null)
			{
				this._useSN = Convert.ToBoolean(systemProfile);
			}
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x00076CB4 File Offset: 0x00074EB4
		public override void AfterBindData(EventArgs e)
		{
			this.View.GetMainBarItem("tbReturnData").Visible = this.needReturnData.Equals("1");
			this.View.GetMainBarItem("tbReserveLinkQuery").Visible = this.usePLNReserve;
			this.View.GetMainBarItem("tbreservrLinkTraceBack").Visible = false;
			this.View.GetMainBarItem("tbViewSN").Visible = this._useSN;
			this.View.GetControl<EntryGrid>("FList").SetAllColHeaderAsText();
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x00076D48 File Offset: 0x00074F48
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			bool flag = false;
			string text = this.GetOpenParameter(ref flag);
			if (this._isFirst)
			{
				this._isFirst = false;
				bool boolInvUserSet = InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FListFirstInNoQuery");
				if (boolInvUserSet)
				{
					return;
				}
			}
			this._sumQuerySqlParas = new List<SqlParam>();
			if (!string.IsNullOrWhiteSpace(e.FilterString))
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = string.Format(" {0} AND ( {1} )", text, e.FilterString);
				}
				else
				{
					text = e.FilterString;
				}
				flag = true;
			}
			string quickFilterString = this.ListModel.FilterParameter.QuickFilterString;
			if (!string.IsNullOrWhiteSpace(quickFilterString))
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = string.Format(" {0} AND ( {1} )", text, quickFilterString);
				}
				else
				{
					text = quickFilterString;
				}
				flag = true;
			}
			if (!flag && InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FBlankNoMoreFilter"))
			{
				e.FilterString = " 1 <> 1 ";
				return;
			}
			if (this.detailInfo == null)
			{
				this.detailInfo = ((FormMetadata)MetaDataServiceHelper.Load(base.Context, "STK_Inventory", true)).BusinessInfo;
			}
			string orgFilter = this.GetOrgFilter(e);
			string ownerFilter = this.GetOwnerFilter();
			if (!string.IsNullOrWhiteSpace(ownerFilter))
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = string.Format(" {0} AND ( {1} )", text, ownerFilter);
				}
				else
				{
					text = ownerFilter;
				}
			}
			string text2 = " FISEFFECTIVED = '1' ";
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = string.Format(" {0} AND ( {1} )", text, text2);
			}
			else
			{
				text = text2;
			}
			bool boolInvUserSet2 = InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FShowZeroInv");
			if (!boolInvUserSet2)
			{
				string text3 = " (FBASEQTY <> 0 OR FSECQTY <> 0 ) ";
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = string.Format(" {0} AND ( {1} )", text, text3);
				}
				else
				{
					text = text3;
				}
			}
			this._sumQueryFilter = " 1 <> 1";
			int maxRowCount = 2000;
			if (e.CustomFilter.DynamicObjectType.Properties.Contains("MaxRowCount"))
			{
				maxRowCount = Convert.ToInt32(e.CustomFilter["MaxRowCount"]);
			}
			if (this.BuildSumVewData(text, orgFilter, maxRowCount))
			{
				this._sumQueryFilter = text;
				if (!string.IsNullOrWhiteSpace(orgFilter))
				{
					if (!string.IsNullOrWhiteSpace(this._sumQueryFilter))
					{
						this._sumQueryFilter = string.Format(" {0} AND ( {1} )", orgFilter, this._sumQueryFilter);
					}
					else
					{
						this._sumQueryFilter = orgFilter;
					}
				}
				e.FilterString = string.Format(" FTRANSID='{0}' {1} ", this.transID, boolInvUserSet2 ? "" : " AND (FBASEQTY <> 0 OR FSECQTY <> 0 OR FSUMLEVEL > 1) ");
				e.QuickFilterString = "";
				e.SortString = "";
			}
			e.HeadSummaryFilterString = "FSUMLEVEL = 1 ";
			this.sumPrecisions = new Dictionary<int, int>();
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x00077006 File Offset: 0x00075206
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (!string.IsNullOrWhiteSpace(this.transID))
			{
				StockServiceHelper.ClearInvSumQueryData(base.Context, this.transID);
				this.transID = "";
			}
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x00077038 File Offset: 0x00075238
		public override void Dispose()
		{
			base.Dispose();
			if (!string.IsNullOrWhiteSpace(this.transID))
			{
				StockServiceHelper.ClearInvSumQueryData(base.Context, this.transID);
				this.transID = "";
			}
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x0007706C File Offset: 0x0007526C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (!this.CheckPermission(e))
			{
				e.Cancel = true;
				this.View.ShowWarnningMessage(ResManager.LoadKDString("没有该操作权限!", "004023030002158", 5, new object[0]), ResManager.LoadKDString("权限错误", "004023030002161", 5, new object[0]), 0, null, 1);
				return;
			}
			string key;
			switch (key = e.BarItemKey.ToUpperInvariant())
			{
			case "TBDETAIL":
				this.View.ParentFormView.Session["SumQuerySqlParas"] = this._sumQuerySqlParas;
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "ShowDetailInv", this._sumQueryFilter);
				e.Cancel = true;
				return;
			case "TBRETURNDATA":
				if (this.queryMode == 1 && this.needReturnData.Equals("1") && this.ListView.SelectedRowsInfo != null && this.ListView.CurrentSelectedRowInfo != null)
				{
					this.View.ParentFormView.Session["SumQuerySqlParas"] = this._sumQuerySqlParas;
					((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "ReturnDataFromSum", this._sumQueryFilter);
				}
				e.Cancel = true;
				return;
			case "TBCLOSE":
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "CloseWindowBySum", "");
				e.Cancel = true;
				return;
			case "TBRESERVELINKQUERY":
				this.ReserveLinkQuery();
				return;
			case "TBRESERVRLINKTRACEBACK":
				this.ReserveTrackQuery();
				return;
			case "TBREFRESH":
				this.View.RefreshByFilter();
				e.Cancel = true;
				return;
			case "TBVIEWSN":
				this.ViewInvSerial();
				return;
			case "TBCLEAREXFIELD":
			{
				string operateName = ResManager.LoadKDString("清除", "004023030009252", 5, new object[0]);
				string onlyViewMsg = Common.GetOnlyViewMsg(base.Context, operateName);
				if (!string.IsNullOrWhiteSpace(onlyViewMsg))
				{
					e.Cancel = true;
					this.View.ShowErrMessage(onlyViewMsg, "", 0);
					return;
				}
				this.ClearExField();
				break;
			}

				return;
			}
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x00077300 File Offset: 0x00075500
		public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
		{
			if (this.ListView.CurrentSelectedRowInfo != null)
			{
				this.View.ParentFormView.Session["SumQuerySqlParas"] = this._sumQuerySqlParas;
				((IDynamicFormViewService)this.View.ParentFormView).CustomEvents(this.sQueryPage, "ShowDetailInv", this._sumQueryFilter);
			}
			e.Cancel = true;
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x00077368 File Offset: 0x00075568
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

		// Token: 0x060008FA RID: 2298 RVA: 0x0007751E File Offset: 0x0007571E
		public override void FormatCellValue(FormatCellValueArgs args)
		{
			this.FormatSumLevel(args);
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x00077528 File Offset: 0x00075728
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			base.BeforeFilterGridF7Select(e);
			e.PermissionFormId = "STK_Inventory";
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

		// Token: 0x060008FC RID: 2300 RVA: 0x000775F4 File Offset: 0x000757F4
		private string GetOpenParameter(ref bool haveMoreFilter)
		{
			string text = "";
			object customParameter = this.View.OpenParameter.GetCustomParameter("QueryFilter");
			if (customParameter != null)
			{
				text = customParameter.ToString();
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("QueryPage");
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
			customParameter = this.View.OpenParameter.GetCustomParameter("QueryOrgId");
			if (customParameter != null)
			{
				this.queryOrgId = Convert.ToInt64(customParameter);
			}
			customParameter = this.View.OpenParameter.GetCustomParameter("HaveMoreFilter");
			if (customParameter != null)
			{
				haveMoreFilter = (customParameter.ToString() == "1");
			}
			else
			{
				haveMoreFilter = !string.IsNullOrWhiteSpace(text);
			}
			return text;
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x000776F4 File Offset: 0x000758F4
		private string GetOrgFilter(FilterArgs e)
		{
			if (this._filterObject == null)
			{
				DataRuleFilterParamenter dataRuleFilterParamenter = new DataRuleFilterParamenter("STK_Inventory", 1)
				{
					PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
					BusinessInfo = this.detailInfo,
					IsLookUp = false
				};
				this._filterObject = PermissionServiceHelper.LoadDataRuleFilter(base.Context, dataRuleFilterParamenter);
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<long> isolationOrgList = this.ListModel.FilterParameter.IsolationOrgList;
			if (this.View.Context.IsStandardEdition() && !isolationOrgList.Contains(this.View.Context.CurrentOrganizationInfo.ID))
			{
				isolationOrgList.Add(this.View.Context.CurrentOrganizationInfo.ID);
			}
			object obj = this.ListModel.FilterParameter.CustomFilter["QueryType"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
			{
				obj.ToString().Equals("1");
			}
			if (isolationOrgList.Count <= 0)
			{
				stringBuilder.AppendLine(" (1<>1) ");
			}
			else if (isolationOrgList.Count < 50)
			{
				stringBuilder.AppendFormat(" (FSTOCKORGID = 0 OR ((FSTOCKORGID IN ({0}) ", string.Join<long>(",", isolationOrgList));
			}
			else
			{
				stringBuilder.AppendLine().AppendFormat(" (FSTOCKORGID = 0 OR ((FSTOCKORGID IN (SELECT /*+ cardinality(TOGS {0})*/ TOGS.FID FROM table(fn_StrSplit(@FSelOrgID, ',', 1)) TOGS ) ", isolationOrgList.Count);
				if (e.SqlParams == null)
				{
					e.SqlParams = new List<SqlParam>();
				}
				this._sumQuerySqlParas.Add(new SqlParam("@FSelOrgID", 161, isolationOrgList.Distinct<long>().ToArray<long>()));
			}
			if (this._filterObject != null)
			{
				if (!string.IsNullOrWhiteSpace(this._filterObject.FilterString))
				{
					stringBuilder.AppendFormat(" AND ({0}) ", this._filterObject.FilterString);
					if (this._filterObject.SQLFilterParams.Count > 0)
					{
						this._sumQuerySqlParas.AddRange(this._filterObject.SQLFilterParams);
					}
				}
				if (this._filterObject.BaseDataRuleFilterObject != null && !string.IsNullOrWhiteSpace(this._filterObject.BaseDataRuleFilterObject.Filter))
				{
					stringBuilder.AppendFormat(" AND ({0}) ", this._filterObject.BaseDataRuleFilterObject.Filter);
					if (this._filterObject.BaseDataRuleFilterObject.SqlParams != null && this._filterObject.BaseDataRuleFilterObject.SqlParams.Count > 0)
					{
						this._sumQuerySqlParas.AddRange(this._filterObject.BaseDataRuleFilterObject.SqlParams);
					}
				}
			}
			stringBuilder.Append(" ))) ");
			return stringBuilder.ToString();
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x0007796C File Offset: 0x00075B6C
		private string GetOwnerFilter()
		{
			string text = "";
			string text2 = "";
			object obj = this.ListModel.FilterParameter.CustomFilter["QueryType"];
			if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()) && obj.ToString().Equals("1"))
			{
				obj = this.ListModel.FilterParameter.CustomFilter["OwnerTypeIdHead"];
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					text2 = obj.ToString();
					string a;
					if ((a = text2.ToUpperInvariant()) != null)
					{
						if (!(a == "BD_OWNERORG"))
						{
							if (!(a == "BD_CUSTOMER"))
							{
								if (a == "BD_SUPPLIER")
								{
									text = "OwnerSupplyId";
								}
							}
							else
							{
								text = "OwnerCustId";
							}
						}
						else
						{
							text = "OwnerOrgId";
						}
					}
				}
			}
			string text3 = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				text3 = string.Format(" (FOWNERTYPEID = '{0}' ", text2);
				DynamicObject dynamicObject = this.ListModel.FilterParameter.CustomFilter[text] as DynamicObject;
				if (dynamicObject != null)
				{
					text3 += string.Format("  AND FOWNERID = {0} ", this.GetDynamicValue(dynamicObject));
				}
				text3 += " ) ";
			}
			return text3;
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x00077AB0 File Offset: 0x00075CB0
		private bool BuildSumVewData(string queryFilter, string orgFilter, int maxRowCount)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			List<string> value = new List<string>();
			new List<SqlParam>();
			List<string> selFields = this.GetSelFields();
			List<string> list = new List<string>();
			Dictionary<string, string> value2 = new Dictionary<string, string>();
			string text = "";
			this._periodFormatFields = "";
			this.GetSumFields(out list, out value2, out text);
			this.AddMustSelFields();
			ListSqlBuilder listSqlBuilder = this.CreateSqlBuilder(queryFilter, orgFilter, text, selFields, list);
			listSqlBuilder.SQLType = 0;
			if (listSqlBuilder != null)
			{
				listSqlBuilder.MaxRowCount = maxRowCount;
				value = listSqlBuilder.BuildSqlForList(ref this.tableName);
			}
			dictionary["OldTransID"] = this.transID;
			dictionary["SelFields"] = selFields;
			dictionary["ListSql"] = value;
			dictionary["ListTmpTable"] = this.tableName;
			dictionary["SumFields"] = list;
			dictionary["SumFieldControl"] = value2;
			dictionary["UseSumSort"] = (string.IsNullOrWhiteSpace(this.ListView.Model.FilterParameter.SortString) && string.IsNullOrWhiteSpace(text));
			this.transID = SequentialGuid.NewGuid().ToString().Replace("-", "");
			dictionary["TransID"] = this.transID;
			dictionary["SqlParas"] = this._sumQuerySqlParas;
			dictionary["PeriodFormatFields"] = this._periodFormatFields;
			StockServiceHelper.FillInvSumQueryData(base.Context, dictionary);
			return true;
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x00077C74 File Offset: 0x00075E74
		private void AddMustSelFields()
		{
			List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
			List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
			Field field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FStockOrgId"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FStockOrgId")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 56,
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
			}
			field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FSumLevel"));
			if (columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FSumLevel")) == null && field != null)
			{
				ColumnField item = new ColumnField
				{
					Key = field.Key,
					Caption = field.Name,
					ColIndex = field.ListTabIndex,
					ColType = 56,
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
			}
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x00077EA4 File Offset: 0x000760A4
		private void FormatSumLevel(FormatCellValueArgs args)
		{
			string fieldName = args.Header.FieldName;
			string text = "";
			if (args.DataRow.ColumnContains("FSUMLEVEL"))
			{
				text = "FSUMLEVEL";
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				base.FormatCellValue(args);
				return;
			}
			string text2 = "";
			if (args.DataRow.ColumnContains("FStockUnitId_FPrecision"))
			{
				text2 = "FStockUnitId_FPrecision";
			}
			int num = Convert.ToInt32(args.DataRow[text]);
			if (num > 1)
			{
				string text3 = this.sumFormatInfo[num];
				if (StringUtils.EqualsIgnoreCase(fieldName, text3))
				{
					args.FormateValue = string.Format(ResManager.LoadKDString("{0}({1}级小计)", "004023030002170", 5, new object[0]), args.FormateValue, num - 1);
					return;
				}
				if (StringUtils.EqualsIgnoreCase(fieldName, "FQty") || StringUtils.EqualsIgnoreCase(fieldName, "FLockQty") || StringUtils.EqualsIgnoreCase(fieldName, "FAVBQty"))
				{
					if (string.IsNullOrWhiteSpace(text2))
					{
						base.FormatCellValue(args);
						return;
					}
					int num2 = 0;
					int num3 = 0;
					this.sumPrecisions.TryGetValue(num - 1, out num2);
					this.sumPrecisions.TryGetValue(num, out num3);
					if (num2 > num3)
					{
						this.sumPrecisions[num] = num2;
					}
					if (!string.IsNullOrWhiteSpace(args.FormateValue))
					{
						args.FormateValue = this.GetFieldNumericData(args.Value.ToString(), num2);
						return;
					}
					base.FormatCellValue(args);
					return;
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(text2) && (StringUtils.EqualsIgnoreCase(fieldName, "FQty") || StringUtils.EqualsIgnoreCase(fieldName, "FLockQty") || StringUtils.EqualsIgnoreCase(fieldName, "FAVBQty")))
				{
					this.sumPrecisions[1] = Convert.ToInt32(args.DataRow[text2]);
				}
				base.FormatCellValue(args);
			}
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x00078128 File Offset: 0x00076328
		private List<string> GetSelFields()
		{
			List<string> list = new List<string>();
			List<Field> fieldList = this.detailInfo.GetFieldList();
			IEnumerable<ColumnField> enumerable = from p in this.ListView.Model.FilterParameter.ColumnInfo
			where p.Visible
			select p;
			Field fld;
			foreach (ColumnField columnField in enumerable)
			{
				string fieldName = columnField.FieldName.Contains("_") ? StringUtils.GetString(columnField.FieldName, 1, "_") : columnField.FieldName;
				Element element = fieldList.SingleOrDefault((Element p) => !string.IsNullOrWhiteSpace(p.Key) && StringUtils.EqualsIgnoreCase(p.Key, fieldName));
				if (element != null && !(element is BaseQtyField) && !(element is QtyField))
				{
					if (element is BasePropertyField)
					{
						fieldName = ((BasePropertyField)element).ControlFieldKey;
						fld = fieldList.SingleOrDefault((Field p) => !string.IsNullOrWhiteSpace(p.FieldName) && StringUtils.EqualsIgnoreCase(p.FieldName, fieldName));
					}
					else
					{
						fld = fieldList.SingleOrDefault((Field p) => !string.IsNullOrWhiteSpace(p.FieldName) && StringUtils.EqualsIgnoreCase(p.FieldName, fieldName));
					}
					if (fld != null && InvSumQueryList.InvStockQueryFileds.Contains(fld.FieldName.ToUpperInvariant()))
					{
						if (!list.Exists((string p) => p.Equals(fld.FieldName)))
						{
							list.Add(fld.FieldName);
						}
					}
				}
			}
			fld = fieldList.SingleOrDefault((Field p) => !string.IsNullOrWhiteSpace(p.FieldName) && StringUtils.EqualsIgnoreCase(p.FieldName, "FStockOrgId"));
			if (fld != null && !list.Exists((string p) => p.Equals(fld.FieldName)))
			{
				list.Add(fld.FieldName);
			}
			return list;
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x00078458 File Offset: 0x00076658
		private void GetSumFields(out List<string> sumFields, out Dictionary<string, string> sumFieldControl, out string sortString)
		{
			this.sumFormatInfo = new Dictionary<int, string>();
			sumFields = new List<string>();
			sumFieldControl = new Dictionary<string, string>();
			sortString = "";
			if (this.ListView.Model.FilterParameter.CustomFilter != null)
			{
				DynamicObject customFilter = this.ListView.Model.FilterParameter.CustomFilter;
				if (!customFilter.GetDataEntityType().Properties.ContainsKey("SumFields"))
				{
					return;
				}
				IEnumerable<DynamicObject> enumerable;
				if (base.Context.IsMultiOrg)
				{
					enumerable = (from p in (DynamicObjectCollection)customFilter["SumFields"]
					where p["FieldKey"] != null
					select p).Distinct<DynamicObject>();
				}
				else
				{
					enumerable = (from p in (DynamicObjectCollection)customFilter["SumFields"]
					where p["FieldKey"] != null && Convert.ToString(p["FieldKey"]) != "FStockOrgId"
					select p).Distinct<DynamicObject>();
				}
				if (enumerable != null && enumerable.Count<DynamicObject>() > 0)
				{
					IEnumerable<ColumnField> source = from p in this.ListView.Model.FilterParameter.ColumnInfo
					where p.Visible
					select p;
					List<Field> fieldList = this.View.BillBusinessInfo.GetFieldList();
					int num = enumerable.Count<DynamicObject>() + 1;
					int num2 = 0;
					StringBuilder stringBuilder = new StringBuilder();
					foreach (DynamicObject dynamicObject in enumerable)
					{
						string fieldName = dynamicObject["FieldKey"].ToString();
						ColumnField col = source.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.FieldName, fieldName));
						if (col == null)
						{
							col = source.FirstOrDefault((ColumnField p) => p.FieldName.StartsWith(fieldName + "_"));
						}
						if (col == null)
						{
							sumFields.Clear();
							this.sumFormatInfo.Clear();
							sumFieldControl.Clear();
							this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("设置的汇总字段[{0}]在即时库存表中不存在，请检查模板设置", "004023030002173", 5, new object[0]), dynamicObject["FieldText"]).ToString(), "", 0);
							return;
						}
						this.sumFormatInfo[num - num2] = col.FieldName;
						Element element = fieldList.SingleOrDefault((Element p) => !string.IsNullOrWhiteSpace(p.Key) && StringUtils.EqualsIgnoreCase(p.Key, fieldName));
						Field field;
						if (element != null && element is BasePropertyField)
						{
							fieldName = ((BasePropertyField)element).ControlFieldKey;
							field = fieldList.SingleOrDefault((Field p) => !string.IsNullOrWhiteSpace(p.FieldName) && StringUtils.EqualsIgnoreCase(p.FieldName, fieldName));
						}
						else
						{
							field = fieldList.SingleOrDefault((Field p) => !string.IsNullOrWhiteSpace(p.FieldName) && StringUtils.EqualsIgnoreCase(p.FieldName, fieldName));
						}
						if (field != null)
						{
							if (!sumFields.Exists((string p) => p.Equals(col.FieldName)))
							{
								sumFields.Add(col.FieldName);
								sumFieldControl[col.FieldName] = field.FieldName;
								if (field is RelatedFlexGroupField)
								{
									stringBuilder.Append(Common.GetRelatedGroupFieldSortString(field as RelatedFlexGroupField, "ASC"));
								}
								else
								{
									stringBuilder.AppendFormat(" {0} ASC , ", col.Key);
								}
							}
						}
						num2++;
					}
					if (stringBuilder.Length > 0)
					{
						sortString = stringBuilder.ToString(0, stringBuilder.Length - 2);
					}
				}
			}
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0007881C File Offset: 0x00076A1C
		private ListSqlBuilder CreateSqlBuilder(string queryFilterString, string dataRuleFilter, string sortString, List<string> selFields, List<string> sumFields)
		{
			if (this.ListModel.FilterParameter == null)
			{
				return null;
			}
			QueryInfo queryInfo = new QueryInfo(this.detailInfo);
			SqlBuilderParameter sqlBuilderParameter = this.CreateSqlBuilderParameter(queryInfo, queryFilterString, dataRuleFilter, selFields, sortString, sumFields);
			this.tableName = DBServiceHelper.CreateTemporaryTableName(base.Context);
			return new ListSqlBuilder(base.Context, queryInfo, sqlBuilderParameter, this.tableName);
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x00078974 File Offset: 0x00076B74
		private SqlBuilderParameter CreateSqlBuilderParameter(QueryInfo queryInfo, string queryFilterString, string dataRuleFilter, List<string> selFields, string sortString, List<string> sumFields)
		{
			List<EntityTable> list = new List<EntityTable>(from p in queryInfo.EntityTables
			where !StringUtils.EqualsIgnoreCase(p.Key, "FInventorySerial")
			select p);
			List<ListHeader> listheader = (from p in this.ListModel.FilterParameter.ColumnInfo
			orderby p.ColIndex
			where p.Visible
			select new ListHeader
			{
				Caption = p.Caption,
				ColType = p.ColType,
				Width = p.ColWidth,
				FieldName = p.FieldName,
				Visible = p.Visible,
				ColIndex = p.ColIndex,
				Key = p.Key
			}).ToList<ListHeader>();
			List<ListHeader> list2 = new List<ListHeader>();
			this.AddExtListHeader(list2, listheader, list, queryInfo);
			SqlBuilderParameter sqlbuilderparameter = new SqlBuilderParameter
			{
				SelectedFieldKeys = new List<string>(),
				SelectEntityTables = list,
				OrderByClauseWihtKey = (string.IsNullOrWhiteSpace(this.ListModel.FilterParameter.SortString) ? sortString : this.ListModel.FilterParameter.SortString),
				FilterClauseWihtKey = queryFilterString,
				DataRuleFilterString = dataRuleFilter,
				IsolationOrgId = ((ListModel)this.ListModel).IsolationOrgId,
				IsolationOrgList = ((ListModel)this.ListModel).IsolationOrgList
			};
			this.MergeMustInputField(sqlbuilderparameter.SelectedFieldKeys, selFields, sumFields);
			this.AddBDMustLoadPropertyFields(sqlbuilderparameter, queryInfo);
			sqlbuilderparameter.SelectedFieldKeys.AddRange((from p in list2
			where ListUtils.IsEmpty<string>(from x in sqlbuilderparameter.SelectedFieldKeys
			where StringUtils.EqualsIgnoreCase(x, p.Key)
			select x)
			select p.Key).ToList<string>());
			if (sqlbuilderparameter.SelectedFieldKeys.Contains("FPRODUCEDATE"))
			{
				this._periodFormatFields = "FPRODUCEDATE,";
				if (!sqlbuilderparameter.SelectedFieldKeys.Contains("FLOT"))
				{
					sqlbuilderparameter.SelectedFieldKeys.Add("FLOT");
				}
			}
			if (sqlbuilderparameter.SelectedFieldKeys.Contains("FEXPIRYDATE"))
			{
				this._periodFormatFields += "FEXPIRYDATE,";
				if (!sqlbuilderparameter.SelectedFieldKeys.Contains("FLOT"))
				{
					sqlbuilderparameter.SelectedFieldKeys.Add("FLOT");
				}
			}
			return sqlbuilderparameter;
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x00078BF8 File Offset: 0x00076DF8
		private void MergeMustInputField(List<string> list, List<string> selFields, List<string> sumFields)
		{
			IEnumerable<string> source = from p in list
			select p.ToUpperInvariant();
			if (!source.Contains("FQTY"))
			{
				list.Add("FQTY");
			}
			if (!source.Contains("FSTOCKUNITID"))
			{
				list.Add("FSTOCKUNITID");
			}
			if (!source.Contains("FBASEUNITID"))
			{
				list.Add("FBASEUNITID");
			}
			if (!source.Contains("FBASEQTY"))
			{
				list.Add("FBASEQTY");
			}
			if (!source.Contains("FSECQTY"))
			{
				list.Add("FSECQTY");
			}
			if (!source.Contains("FSECUNITID"))
			{
				list.Add("FSECUNITID");
			}
			if (!source.Contains("FLOCKQTY"))
			{
				list.Add("FLOCKQTY");
			}
			if (!source.Contains("FBASELOCKQTY"))
			{
				list.Add("FBASELOCKQTY");
			}
			if (!source.Contains("FSECLOCKQTY"))
			{
				list.Add("FSECLOCKQTY");
			}
			if (!source.Contains("FAVBQTY"))
			{
				list.Add("FAVBQTY");
			}
			if (!source.Contains("FBASEAVBQTY"))
			{
				list.Add("FBASEAVBQTY");
			}
			if (!source.Contains("FSECAVBQTY"))
			{
				list.Add("FSECAVBQTY");
			}
			foreach (string text in selFields)
			{
				if (!source.Contains(text))
				{
					list.Add(text);
				}
			}
			foreach (string text2 in sumFields)
			{
				if (!source.Contains(text2))
				{
					list.Add(text2);
				}
			}
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x00078DE8 File Offset: 0x00076FE8
		private void AddFlexPropertyFields(SqlBuilderParameter sqlbuilderparameter, QueryInfo queryInfo)
		{
			IEnumerable<SelectField> enumerable = from p in queryInfo.SelectFields
			where p is SelectFlexField
			select p;
			if (enumerable == null)
			{
				return;
			}
			foreach (ColumnField columnField in this.ListModel.FilterParameter.ColumnInfo)
			{
				string @string = StringUtils.GetString(columnField.Key, 1, ".");
				string string2 = StringUtils.GetString(columnField.Key, 2, ".");
				if (string.IsNullOrWhiteSpace(string2))
				{
					Field field = queryInfo.Businessinfo.GetField(@string);
					if (field != null && field is RelatedFlexGroupField)
					{
						foreach (SelectField selectField in enumerable)
						{
							SelectFlexField selectFlexField = (SelectFlexField)selectField;
							if (selectFlexField.IsDisplayProperty && selectFlexField.Key.IndexOf(string.Format("{0}.", @string)) == 0)
							{
								sqlbuilderparameter.SelectedFieldKeys.Add(selectFlexField.Key);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x00078F60 File Offset: 0x00077160
		private void AddExtListHeader(List<ListHeader> extListheader, List<ListHeader> listheader, List<EntityTable> selectedEntityTables, QueryInfo queryInfo)
		{
			MustSelectedField mustSelectedField = queryInfo.MustSelectedField;
			if (mustSelectedField != null)
			{
				string formIdFieldName = mustSelectedField.FormIdFieldName;
				if (mustSelectedField.DisplayFormID)
				{
					extListheader.Add(new ListHeader(formIdFieldName, new LocaleValue(), false)
					{
						Key = formIdFieldName
					});
				}
				if (mustSelectedField.DisplayName)
				{
					if (!listheader.Exists((ListHeader o) => StringUtils.EqualsIgnoreCase(o.Key, mustSelectedField.NameFieldKey)))
					{
						extListheader.Add(new ListHeader(mustSelectedField.NameFieldKey, new LocaleValue(), false)
						{
							Key = mustSelectedField.NameFieldKey
						});
					}
				}
				if (mustSelectedField.DisplayNumber)
				{
					if (!listheader.Exists((ListHeader o) => StringUtils.EqualsIgnoreCase(o.Key, mustSelectedField.NumberFieldKey)))
					{
						extListheader.Add(new ListHeader(mustSelectedField.NumberFieldKey, new LocaleValue(), false)
						{
							Key = mustSelectedField.NumberFieldKey
						});
					}
				}
			}
			string fieldName = "";
			ListHeader listHeader;
			foreach (EntityTable entityTable in selectedEntityTables)
			{
				if (entityTable.ElementType == 34 || queryInfo.Businessinfo.GetEntity(entityTable.Key).EntityType != 0)
				{
					listHeader = new ListHeader();
					if (entityTable.ElementType == 34)
					{
						listHeader.FieldName = entityTable.PkFieldName.ToUpperInvariant();
					}
					else
					{
						string text = string.Format("{0}_{1}", entityTable.Key, entityTable.EntryPkFieldName.ToUpperInvariant());
						listHeader.FieldName = text;
						SelectField selectField = queryInfo.GetSelectField(text);
						fieldName = selectField.FieldName;
					}
					bool flag = false;
					foreach (ListHeader listHeader2 in listheader)
					{
						if (StringUtils.EqualsIgnoreCase(listHeader2.FieldName, listHeader.FieldName))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						listHeader.Caption = new LocaleValue();
						listHeader.Width = 0;
						listHeader.Visible = false;
						listHeader.Key = listHeader.FieldName;
						extListheader.Add(listHeader);
					}
				}
			}
			listHeader = new ListHeader();
			listHeader.FieldName = fieldName;
			listHeader.Caption = new LocaleValue();
			listHeader.Width = 0;
			listHeader.Visible = false;
			listHeader.Key = listHeader.FieldName;
			extListheader.Add(listHeader);
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x00079284 File Offset: 0x00077484
		private void AddBDMustLoadPropertyFields(SqlBuilderParameter sqlbuilderparameter, QueryInfo queryInfo)
		{
			Dictionary<string, SelectField> dictionary = new Dictionary<string, SelectField>();
			Dictionary<string, List<SelectField>> dictionary2 = new Dictionary<string, List<SelectField>>();
			foreach (SelectField selectField in queryInfo.SelectFields)
			{
				string @string = StringUtils.GetString(selectField.Key, 1, ".");
				if (selectField.ControlField)
				{
					List<SelectField> list = null;
					dictionary2.TryGetValue(@string, out list);
					if (list == null)
					{
						list = new List<SelectField>();
						dictionary2.Add(@string, list);
					}
					list.Add(selectField);
				}
				else if (selectField.Visible == 4)
				{
					dictionary.Add(@string, selectField);
				}
			}
			foreach (ColumnField columnField in this.ListModel.FilterParameter.ColumnInfo)
			{
				if (columnField.Visible)
				{
					string string2 = StringUtils.GetString(columnField.Key, 1, ".");
					Field field = queryInfo.Businessinfo.GetField(string2);
					if (field != null && !string.IsNullOrWhiteSpace(field.ControlFieldKey))
					{
						Field controlField = queryInfo.Businessinfo.GetField(field.ControlFieldKey);
						if (controlField != null)
						{
							List<SelectField> list2 = null;
							dictionary2.TryGetValue(controlField.Key, out list2);
							if (list2 != null && list2.Count != 0)
							{
								if (sqlbuilderparameter.SelectEntityTables.FirstOrDefault((EntityTable entityTable) => StringUtils.EqualsIgnoreCase(entityTable.Key, controlField.EntityKey)) == null)
								{
									if (!(controlField.Entity is SubHeadEntity))
									{
										continue;
									}
									sqlbuilderparameter.SelectEntityTables.Add(queryInfo.GetEntityTable(controlField.EntityKey));
								}
								SelectField basedataField = null;
								dictionary.TryGetValue(controlField.Key, out basedataField);
								if (basedataField != null && this.ListModel.FilterParameter.ColumnInfo.FirstOrDefault((ColumnField fld) => StringUtils.EqualsIgnoreCase(fld.Key, basedataField.Key)) == null)
								{
									sqlbuilderparameter.SelectedFieldKeys.Add(basedataField.Key);
								}
								sqlbuilderparameter.SelectedFieldKeys.AddRange((from p in list2
								select p.Key).ToList<string>());
							}
						}
					}
				}
			}
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x00079538 File Offset: 0x00077738
		protected string GetFieldNumericData(string value, int iDecimal)
		{
			string text = FieldFormatterUtil.GetDecimalFormatString(base.Context, decimal.Parse(value), iDecimal);
			bool boolInvUserSet = InventoryQuery.GetBoolInvUserSet(this.View.Context, this.View.BillBusinessInfo.GetForm().Id, "FZeroNotDisp");
			if (boolInvUserSet)
			{
				text = FieldFormatterUtil.GetNoZeroString(text);
			}
			return text;
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x000795B0 File Offset: 0x000777B0
		private bool CheckPermission(BarItemClickEventArgs e)
		{
			List<BarItem> barItems = ((IListView)this.View).BillLayoutInfo.GetFormAppearance().ListMenu.BarItems;
			string id = "STK_Inventory";
			string permissionItemIdByMenuBar = FormOperation.GetPermissionItemIdByMenuBar(this.View, (from p in barItems
			where StringUtils.EqualsIgnoreCase(p.Key, e.BarItemKey)
			select p).SingleOrDefault<BarItem>());
			if (string.IsNullOrWhiteSpace(permissionItemIdByMenuBar))
			{
				return true;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = id
			}, permissionItemIdByMenuBar);
			return permissionAuthResult.Passed;
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x0007969C File Offset: 0x0007789C
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

		// Token: 0x0600090D RID: 2317 RVA: 0x00079820 File Offset: 0x00077A20
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
			IEnumerable<InvQueryRetRecord> sumInventoryDatas = StockServiceHelper.GetSumInventoryDatas(this.ListView.Context, listSelectedRow.PrimaryKeyValue, 0L);
			if (sumInventoryDatas == null || sumInventoryDatas.Count<InvQueryRetRecord>() <= 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("找不到对应的基础资料数据，请选择库存组织有值的数据执行链接查询", "004023030004756", 5, new object[0]), 0);
				return;
			}
			InvQueryRetRecord invQueryRetRecord = sumInventoryDatas.ToArray<InvQueryRetRecord>()[0];
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

		// Token: 0x0600090E RID: 2318 RVA: 0x00079AA0 File Offset: 0x00077CA0
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
			IEnumerable<InvQueryRetRecord> sumInventoryDatas = StockServiceHelper.GetSumInventoryDatas(this.ListView.Context, listSelectedRow.PrimaryKeyValue, 0L);
			if (sumInventoryDatas == null || sumInventoryDatas.Count<InvQueryRetRecord>() <= 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("找不到对应的基础资料数据，请选择库存组织有值的数据执行链接查询！", "004023030004762", 5, new object[0]), 0);
				return;
			}
			InvQueryRetRecord invQueryRetRecord = sumInventoryDatas.ToArray<InvQueryRetRecord>()[0];
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
			moveReportShowParameter.CustomParams.Add("SourceBillFormId", "STK_InvSumQuery");
			moveReportShowParameter.CustomParams.Add("SourceBillId", listSelectedRow.PrimaryKeyValue);
			moveReportShowParameter.CustomParams.Add("SourceOrgId", invQueryRetRecord.StockOrgID.ToString());
			moveReportShowParameter.CustomParams.Add("SourceBeginDate", d.ToString());
			moveReportShowParameter.CustomParams.Add("ShowFields", this.GetShowFields());
			moveReportShowParameter.CustomParams.Add("SourceEndDate", DateTimeFormatUtils.BeginDateTimeOfDay(DateTime.MaxValue).ToString());
			this.View.ShowForm(moveReportShowParameter);
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x00079E24 File Offset: 0x00078024
		private string GetShowFields()
		{
			List<string> list = new List<string>();
			List<ColumnField> list2 = (from p in this.ListView.Model.FilterParameter.ColumnInfo
			where p.Visible
			select p).ToList<ColumnField>();
			foreach (ColumnField columnField in list2)
			{
				if (columnField.Key == "FMtoNo")
				{
					list.Add(columnField.Key.ToUpperInvariant());
					break;
				}
			}
			return string.Join(",", list);
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x00079EE0 File Offset: 0x000780E0
		private void ViewInvSerial()
		{
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "STK_Inventory");
			List<string> list = new List<string>();
			foreach (ListSelectedRow sumRow in this.ListView.SelectedRowsInfo)
			{
				string detailFilter = InventoryQuery.GetDetailFilter(base.Context, this.ListView, this._sumQueryFilter, sumRow);
				if (!string.IsNullOrWhiteSpace(detailFilter))
				{
					IEnumerable<string> inventoryDetailIdsByFilter = InventoryQuery.GetInventoryDetailIdsByFilter(base.Context, formMetaData.BusinessInfo, detailFilter, this._sumQuerySqlParas);
					if (inventoryDetailIdsByFilter != null && inventoryDetailIdsByFilter.Count<string>() > 0)
					{
						list.AddRange(inventoryDetailIdsByFilter);
					}
				}
			}
			if (list.Count < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择有效即时库存分录进行查询", "004023030004765", 5, new object[0]), 0);
				return;
			}
			if (!StockServiceHelper.HaveInStockSerialOnInv(base.Context, list))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有对应的序列号数据", "004023030006355", 5, new object[0]), 0);
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

		// Token: 0x06000911 RID: 2321 RVA: 0x0007A0B4 File Offset: 0x000782B4
		private void ReserveTrackQuery()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "PLN_ReserveTraceBack"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowWarnningMessage(string.Format(ResManager.LoadKDString("当前用户没有预留综合查询的查看权限！", "004023030004768", 5, new object[0]), new object[0]), "", 0, null, 1);
				return;
			}
			ReserveQueryOption option = this.GetReserveQueryOption();
			if (option == null)
			{
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = "PLN_ReserveTraceBack";
			if (option != null)
			{
				this.View.Session["FormInputParam"] = option;
			}
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult results)
			{
				if (option != null)
				{
					this.View.Session.Remove("FormInputParam");
				}
			});
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0007A1D4 File Offset: 0x000783D4
		private void ReserveLinkQuery()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "PLN_RESERVELINKQUERY"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				this.View.ShowWarnningMessage(string.Format(ResManager.LoadKDString("当前用户没有预留综合查询的查看权限！", "004023030004768", 5, new object[0]), new object[0]), "", 0, null, 1);
				return;
			}
			ReserveQueryOption option = this.GetReserveQueryOption();
			if (option == null)
			{
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = "PLN_RESERVELINKQUERY";
			if (option != null)
			{
				this.View.Session["FormInputParam"] = option;
			}
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult results)
			{
				if (option != null)
				{
					this.View.Session.Remove("FormInputParam");
				}
			});
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0007A2C8 File Offset: 0x000784C8
		private ReserveQueryOption GetReserveQueryOption()
		{
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "STK_Inventory");
			List<string> list = new List<string>();
			foreach (ListSelectedRow sumRow in this.ListView.SelectedRowsInfo)
			{
				string detailFilter = InventoryQuery.GetDetailFilter(base.Context, this.ListView, this._sumQueryFilter, sumRow);
				if (!string.IsNullOrWhiteSpace(detailFilter))
				{
					IEnumerable<string> inventoryDetailIdsByFilter = InventoryQuery.GetInventoryDetailIdsByFilter(base.Context, formMetaData.BusinessInfo, detailFilter, this._sumQuerySqlParas);
					if (inventoryDetailIdsByFilter != null && inventoryDetailIdsByFilter.Count<string>() > 0)
					{
						list.AddRange(inventoryDetailIdsByFilter);
					}
				}
			}
			if (list.Count < 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择有效即时库存分录进行查询", "004023030004765", 5, new object[0]), 0);
				return null;
			}
			ReserveQueryOption reserveQueryOption = new ReserveQueryOption();
			List<ReserveQueryBillInfo> list2 = new List<ReserveQueryBillInfo>();
			foreach (string interID in list)
			{
				ReserveQueryBillInfo item = new ReserveQueryBillInfo
				{
					BillInfo = new OriBillInfo
					{
						FormID = "STK_Inventory",
						InterID = interID,
						EntryID = ""
					},
					IsDemand = false
				};
				list2.Add(item);
			}
			reserveQueryOption.BillInfos = list2;
			return reserveQueryOption;
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x0007A458 File Offset: 0x00078658
		private long GetDynamicValue(DynamicObject obj)
		{
			if (obj == null)
			{
				return 0L;
			}
			if (obj.DynamicObjectType.Properties.ContainsKey(FormConst.MASTER_ID))
			{
				return Convert.ToInt64(obj[FormConst.MASTER_ID]);
			}
			if (obj.DynamicObjectType.Properties.ContainsKey("Id"))
			{
				return Convert.ToInt64(obj["Id"]);
			}
			return 0L;
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x0007A4F4 File Offset: 0x000786F4
		private void ClearExField()
		{
			new StringBuilder();
			List<string> tableNoNullableCols = CommonServiceHelper.GetTableNoNullableCols(base.Context, this.InvQueryTableName);
			if (tableNoNullableCols != null && tableNoNullableCols.Count<string>() > 0)
			{
				List<string> list = (from p in tableNoNullableCols
				where !InvSumQueryList.InvStockQueryFileds.Split(new char[]
				{
					','
				}).ToList<string>().Contains(p)
				select p).ToList<string>();
				try
				{
					if (list != null && list.Count<string>() > 0)
					{
						CommonServiceHelper.RemoveTableNoNullColsCons(base.Context, this.InvQueryTableName, list);
					}
					this.View.ShowMessage(ResManager.LoadKDString("清理字段非空属性成功", "004023000023049", 5, new object[0]), 0);
				}
				catch (Exception ex)
				{
					this.View.ShowErrMessage(ex.Message, "", 0);
				}
			}
		}

		// Token: 0x0400037B RID: 891
		private string transID = "";

		// Token: 0x0400037C RID: 892
		private BusinessInfo detailInfo;

		// Token: 0x0400037D RID: 893
		private string sQueryPage = "";

		// Token: 0x0400037E RID: 894
		private string needReturnData = "";

		// Token: 0x0400037F RID: 895
		private long queryOrgId;

		// Token: 0x04000380 RID: 896
		private Dictionary<int, string> sumFormatInfo;

		// Token: 0x04000381 RID: 897
		private Dictionary<int, int> sumPrecisions;

		// Token: 0x04000382 RID: 898
		private int queryMode;

		// Token: 0x04000383 RID: 899
		private string _sumQueryFilter = "";

		// Token: 0x04000384 RID: 900
		private DataRuleFilterObject _filterObject;

		// Token: 0x04000385 RID: 901
		private bool usePLNReserve;

		// Token: 0x04000386 RID: 902
		private bool _useSN;

		// Token: 0x04000387 RID: 903
		private string _periodFormatFields = "";

		// Token: 0x04000388 RID: 904
		private List<SqlParam> _sumQuerySqlParas = new List<SqlParam>();

		// Token: 0x04000389 RID: 905
		public static readonly string InvStockQueryFileds = "FID,FTRANSID,FSUMLEVEL,FSTOCKORGID,FOWNERTYPEID,FOWNERID,FKEEPERTYPEID,FKEEPERID,FSTOCKID,FSTOCKLOCID,FMATERIALID,FAUXPROPID,FSTOCKSTATUSID,FLOT,FBOMID,FMTONO,FPROJECTNO,FPRODUCEDATE,FEXPIRYDATE,FBASEUNITID,FBASEQTY,FSECUNITID,FSECQTY,FQTY,FSTOCKUNITID,FQUERYTIME,FBASELOCKQTY,FLOCKQTY,FSECLOCKQTY,FBASEAVBQTY,FAVBQTY,FSECAVBQTY";

		// Token: 0x0400038A RID: 906
		private readonly string InvQueryTableName = "T_STK_INVSUMQUERY";

		// Token: 0x0400038B RID: 907
		private bool _isFirst = true;

		// Token: 0x0400038C RID: 908
		private string tableName = string.Empty;
	}
}
