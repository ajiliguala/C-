using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000098 RID: 152
	public class InventoryQuery : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000877 RID: 2167 RVA: 0x0006DCD4 File Offset: 0x0006BED4
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object customParameter = this.View.OpenParameter.GetCustomParameter("QueryMode");
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("NeedReturnData");
			object customParameter3 = this.View.OpenParameter.GetCustomParameter("QueryFilter");
			object customParameter4 = this.View.OpenParameter.GetCustomParameter("QueryOrgId");
			object customParameter5 = this.View.OpenParameter.GetCustomParameter("StockOrgIds");
			object customParameter6 = this.View.OpenParameter.GetCustomParameter("showFilterRow");
			if (customParameter != null)
			{
				this.queryMode = Convert.ToInt32(customParameter);
			}
			if (customParameter2 != null)
			{
				this.returnDataMode = Convert.ToInt32(customParameter2);
			}
			if (customParameter4 != null && !string.IsNullOrWhiteSpace(customParameter4.ToString()))
			{
				this.qOrgId = Convert.ToInt64(customParameter4);
			}
			if (customParameter3 != null)
			{
				this.sFixFilter = customParameter3.ToString();
			}
			if (customParameter5 != null)
			{
				this.stockOrgIds = customParameter5.ToString();
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["QueryFilter"] = this.sFixFilter;
			dictionary["QueryPage"] = this.View.PageId;
			dictionary["NeedReturnData"] = this.returnDataMode.ToString();
			dictionary["QueryOrgId"] = this.qOrgId.ToString();
			dictionary["QueryMode"] = this.queryMode.ToString();
			if (customParameter6 != null)
			{
				dictionary["showFilterRow"] = customParameter6.ToString();
			}
			switch (this.queryMode)
			{
			case 1:
				goto IL_1A2;
			case 2:
				this.ShowViewArea(0, false);
				goto IL_1A2;
			}
			this.ShowViewArea(1, false);
			IL_1A2:
			this.AddOnInvListForm(dictionary);
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x0006DE8C File Offset: 0x0006C08C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			List<SqlParam> list = new List<SqlParam>();
			string key;
			switch (key = e.EventName.ToUpper())
			{
			case "RETURNDATAFROMSUM":
				if (this.returnDataMode > 0)
				{
					string empty = string.Empty;
					list = new List<SqlParam>();
					if (this.View.Session.ContainsKey("SumQuerySqlParas"))
					{
						list = (this.View.Session["SumQuerySqlParas"] as List<SqlParam>);
						this.View.Session.Remove("SumQuerySqlParas");
					}
					if (this.GetReturnDataFromSum(e.EventArgs, list, out empty))
					{
						this.View.ReturnToParentWindow(empty);
						this.View.Close();
					}
					((IListView)this.View.GetView(this.sumFormPageId)).SendDynamicFormAction(this.View);
					return;
				}
				this.View.Close();
				((IListView)this.View.GetView(this.sumFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "RETURNDETAILDATA":
				if (this.returnDataMode > 0)
				{
					string empty2 = string.Empty;
					if (this.GetReturnData(out empty2))
					{
						this.View.ReturnToParentWindow(empty2);
						this.View.Close();
					}
					((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
					return;
				}
				this.View.Close();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "SHOWDETAILINV":
				this.ShowViewArea(1, true);
				((IListView)this.View.GetView(this.sumFormPageId)).SendDynamicFormAction(this.View);
				list = new List<SqlParam>();
				if (this.View.Session.ContainsKey("SumQuerySqlParas"))
				{
					list = (this.View.Session["SumQuerySqlParas"] as List<SqlParam>);
					this.View.Session.Remove("SumQuerySqlParas");
				}
				this.ShowDetailInvData(e.EventArgs, list);
				this.ShowHideExitBarItem("4", this.View.GetView(this.sumFormPageId));
				return;
			case "CLOSEWINDOWBYSUM":
				this.View.Close();
				((IListView)this.View.GetView(this.sumFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "CLOSEWINDOWBYDETAIL":
				this.View.Close();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction(this.View);
				return;
			case "REFRESHSUMFORMPAGE":
				((IListView)this.View.GetView(this.sumFormPageId)).RefreshByFilter();
				((IListView)this.View.GetView(this.detailFormPageId)).SendDynamicFormAction((IListView)this.View.GetView(this.sumFormPageId));
				return;
			case "SPLITSTATECHANGED":
				this.ShowHideExitBarItem(e.EventArgs, this.View);
				break;

				return;
			}
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x0006E20C File Offset: 0x0006C40C
		private void AddOnInvListForm(Dictionary<string, string> dicParam = null)
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_InvSumQuery";
			listShowParameter.OpenStyle.TagetKey = "FPanelSum";
			listShowParameter.OpenStyle.ShowType = 3;
			this.sumFormPageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.PageId = this.sumFormPageId;
			if (!string.IsNullOrWhiteSpace(this.stockOrgIds))
			{
				listShowParameter.MutilListUseOrgId = this.stockOrgIds;
			}
			if (dicParam != null)
			{
				foreach (string key in dicParam.Keys)
				{
					listShowParameter.CustomParams.Add(key, dicParam[key]);
				}
			}
			listShowParameter.CustomParams.Add("EnableCustomDefaultScheme", "True");
			this.View.ShowForm(listShowParameter);
			listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "STK_Inventory";
			listShowParameter.OpenStyle.TagetKey = "FPanelDetail";
			listShowParameter.OpenStyle.ShowType = 3;
			this.detailFormPageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.PageId = this.detailFormPageId;
			listShowParameter.CustomParams.Add("IsFromQuery", "True");
			listShowParameter.CustomParams.Add("IsShowExit", "False");
			listShowParameter.CustomParams.Add("InitNoData", "True");
			listShowParameter.CustomParams.Add("UseDefaultScheme", "True");
			if (!string.IsNullOrWhiteSpace(this.stockOrgIds))
			{
				listShowParameter.MutilListUseOrgId = this.stockOrgIds;
			}
			if (dicParam != null)
			{
				foreach (string key2 in dicParam.Keys)
				{
					listShowParameter.CustomParams.Add(key2, dicParam[key2]);
				}
			}
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x0006E41C File Offset: 0x0006C61C
		private bool GetReturnData(out string retData)
		{
			retData = string.Empty;
			if (this.returnDataMode == 0 || this.qOrgId < 1L)
			{
				return true;
			}
			List<string> list = new List<string>();
			foreach (ListSelectedRow listSelectedRow in ((IListView)this.View.GetView(this.detailFormPageId)).SelectedRowsInfo)
			{
				if (listSelectedRow.Selected && listSelectedRow.PrimaryKeyValue != null && !string.IsNullOrWhiteSpace(listSelectedRow.PrimaryKeyValue))
				{
					list.Add(listSelectedRow.PrimaryKeyValue);
				}
			}
			if (list.Count < 1)
			{
				return true;
			}
			string text = string.Join("','", list);
			if (StockServiceHelper.IsSameInvData(base.Context, text, " T1.FOWNERTYPEID, T1.FOWNERID"))
			{
				retData = text;
				return true;
			}
			this.View.ShowMessage(ResManager.LoadKDString("只能返回相同货主", "004023030000268", 5, new object[0]), 0);
			return false;
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x0006E66C File Offset: 0x0006C86C
		private bool GetReturnDataFromSum(string sumQueryFilter, List<SqlParam> sumQuerySqlparas, out string retData)
		{
			retData = string.Empty;
			if (this.returnDataMode == 0 || this.qOrgId < 1L)
			{
				return true;
			}
			new List<string>();
			IListView listView = (IListView)this.View.GetView(this.sumFormPageId);
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (ListSelectedRow sumRow in listView.SelectedRowsInfo)
			{
				string detailFilter = InventoryQuery.GetDetailFilter(base.Context, listView, sumQueryFilter, sumRow);
				if (!string.IsNullOrWhiteSpace(detailFilter))
				{
					List<DynamicObject> inventorysByFilter = InventoryQuery.GetInventorysByFilter(base.Context, ((IListView)this.View.GetView(this.detailFormPageId)).BillBusinessInfo, detailFilter, sumQuerySqlparas);
					if (inventorysByFilter != null && inventorysByFilter.Count > 0)
					{
						list.AddRange(inventorysByFilter);
					}
				}
			}
			if (list == null || list.Count < 0)
			{
				return true;
			}
			if ((from x in list
			group x by new
			{
				type = x["FOwnerTypeId"].ToString(),
				owner = x["FOwnerId"].ToString()
			}).Count() > 1)
			{
				this.View.ShowMessage(ResManager.LoadKDString("只能返回相同货主", "004023030000268", 5, new object[0]), 0);
				return false;
			}
			retData = string.Join("','", from p in list
			select p["FId"].ToString());
			return true;
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x0006E7E8 File Offset: 0x0006C9E8
		private void ShowViewArea(int panelIndex, bool bShow)
		{
			if (panelIndex == 0)
			{
				this.View.GetControl<SplitContainer>("FSplitMain").HideFirstPanel(!bShow);
				return;
			}
			this.View.GetControl<SplitContainer>("FSplitMain").HideSecondPanel(!bShow);
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x0006E820 File Offset: 0x0006CA20
		protected void ShowDetailInvData(string paraString, List<SqlParam> paraSqlparas)
		{
			IListView listView = (IListView)this.View.GetView(this.sumFormPageId);
			string detailFilter = InventoryQuery.GetDetailFilter(base.Context, listView, "", null);
			IListView listView2 = this.View.GetView(this.detailFormPageId) as IListView;
			if (!string.IsNullOrWhiteSpace(detailFilter) && listView2 != null)
			{
				listView2.OpenParameter.SetCustomParameter("QueryFefreshFilter", detailFilter);
				listView2.Model.FilterParameter.IsolationOrgList = listView.Model.FilterParameter.IsolationOrgList;
				listView2.RefreshByFilter();
				((IListView)this.View.GetView(this.sumFormPageId)).SendDynamicFormAction(listView2);
			}
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x0006E8CC File Offset: 0x0006CACC
		private void ShowHideExitBarItem(string splitState, IDynamicFormView sourceView)
		{
			IListView listView = this.View.GetView(this.detailFormPageId) as IListView;
			if (listView != null)
			{
				listView.GetMainBarItem("tbClose").Visible = (splitState.Equals("0") || splitState.Equals("3"));
				sourceView.SendDynamicFormAction((IListView)this.View.GetView(this.detailFormPageId));
			}
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x0006E94C File Offset: 0x0006CB4C
		public static IEnumerable<string> GetInventoryDetailIdsByFilter(Context ctx, BusinessInfo info, string filter, List<SqlParam> sqlParas)
		{
			List<DynamicObject> inventorysByFilter = InventoryQuery.GetInventorysByFilter(ctx, info, filter, sqlParas);
			if (inventorysByFilter == null || inventorysByFilter.Count < 0)
			{
				return null;
			}
			return from p in inventorysByFilter
			select p["FId"].ToString();
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x0006E994 File Offset: 0x0006CB94
		public static List<DynamicObject> GetInventorysByFilter(Context ctx, BusinessInfo info, string filter, List<SqlParam> sqlParas)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FOwnerTypeId"));
			list2.Add(new SelectorItemInfo("FOwnerId"));
			list2.Add(new SelectorItemInfo("FId"));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = info;
			queryBuilderParemeter.FormId = "STK_Inventory";
			queryBuilderParemeter.FilterClauseWihtKey = filter;
			queryBuilderParemeter.SelectItems = list2;
			if (sqlParas != null && sqlParas.Count > 0)
			{
				queryBuilderParemeter.SqlParams.AddRange(sqlParas);
			}
			list.AddRange(QueryServiceHelper.GetDynamicObjectCollection(ctx, queryBuilderParemeter, null));
			return list;
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x0006EAC0 File Offset: 0x0006CCC0
		public static string GetDetailFilter(Context ctx, IListView sumInvListView, string preFilter, ListSelectedRow sumRow = null)
		{
			if (sumRow == null)
			{
				sumRow = sumInvListView.CurrentSelectedRowInfo;
			}
			long num = 0L;
			string text = "";
			if (!string.IsNullOrWhiteSpace(preFilter))
			{
				text = preFilter;
			}
			if (sumRow != null)
			{
				num = Convert.ToInt64(sumRow.PrimaryKeyValue);
			}
			if (num > 0L)
			{
				DynamicObject[] array = BusinessDataServiceHelper.Load(ctx, new object[]
				{
					num.ToString()
				}, sumInvListView.Model.BillBusinessInfo.GetDynamicObjectType());
				if (array.Count<DynamicObject>() > 0)
				{
					FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, "STK_Inventory", true);
					List<Field> fieldList = formMetadata.BusinessInfo.GetFieldList();
					IEnumerable<ColumnField> enumerable = from p in sumInvListView.Model.FilterParameter.ColumnInfo
					where p.Visible
					select p;
					StringBuilder stringBuilder = new StringBuilder();
					List<string> list = new List<string>();
					using (IEnumerator<ColumnField> enumerator = enumerable.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							InventoryQuery.<>c__DisplayClassd CS$<>8__locals1 = new InventoryQuery.<>c__DisplayClassd();
							CS$<>8__locals1.col = enumerator.Current;
							string fieldKey = StringUtils.GetString(CS$<>8__locals1.col.Key, 1, ".");
							Element element = (from p in fieldList
							where !string.IsNullOrWhiteSpace(p.Key) && p.Key.Equals(CS$<>8__locals1.col.Key)
							select p).SingleOrDefault<Element>();
							if (!(element is BaseQtyField) && !(element is QtyField))
							{
								Field fld;
								if (element != null && element is BasePropertyField)
								{
									fieldKey = ((BasePropertyField)element).ControlFieldKey;
									fld = (from p in fieldList
									where !string.IsNullOrWhiteSpace(p.Key) && p.Key.Equals(fieldKey)
									select p).SingleOrDefault<Field>();
								}
								else
								{
									fld = (from p in fieldList
									where !string.IsNullOrWhiteSpace(p.Key) && p.Key.Equals(fieldKey)
									select p).SingleOrDefault<Field>();
								}
								if (fld != null && !StringUtils.EqualsIgnoreCase(fld.Key, "FBASEQTY") && !StringUtils.EqualsIgnoreCase(fld.Key, "FQTY") && !StringUtils.EqualsIgnoreCase(fld.Key, "FSECQTY") && !list.Exists((string p) => StringUtils.EqualsIgnoreCase(p, fld.Key)))
								{
									if (fld is BaseDataField)
									{
										InventoryQuery.GetBaseDataFieldFilter(array, stringBuilder, fld);
									}
									else if (fld is RelatedFlexGroupField)
									{
										InventoryQuery.GetRelateFlexFieldFilter(array, stringBuilder, fld);
									}
									else if (InvSumQueryList.InvStockQueryFileds.Contains(fld.FieldName.ToUpper()))
									{
										InventoryQuery.GetGenFieldFilter(array, stringBuilder, fld);
									}
									list.Add(fld.FieldName);
								}
							}
						}
					}
					if (stringBuilder.Length > 0)
					{
						if (string.IsNullOrWhiteSpace(text))
						{
							text = string.Format(" ( {0} ) ", stringBuilder.ToString().Substring(4));
						}
						else
						{
							text = string.Format(" ( ({0}) {1} ) ", text, stringBuilder.ToString());
						}
					}
				}
				else
				{
					text = " (1 <> 1) ";
				}
			}
			else
			{
				text = " (1 <> 1) ";
			}
			return text;
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x0006EE18 File Offset: 0x0006D018
		private static void GetGenFieldFilter(DynamicObject[] objs, StringBuilder builder, Field fld)
		{
			if (objs[0][fld.PropertyName] == null)
			{
				builder.AppendFormat(" AND {0} IS NULL ", fld.FieldName);
				return;
			}
			if (fld is DateField || fld is DateTimeField)
			{
				if (StringUtils.EqualsIgnoreCase(fld.Key, "FProduceDate") || StringUtils.EqualsIgnoreCase(fld.Key, "FExpiryDate"))
				{
					DynamicObject dynamicObject = ((DynamicObjectCollection)((DynamicObject)objs[0]["MaterialId"])["MaterialStock"])[0];
					if (!Convert.ToBoolean(dynamicObject["IsKFPeriod"]) || (Convert.ToBoolean(dynamicObject["IsBatchManage"]) && Convert.ToBoolean(dynamicObject["IsExpParToFlot"])))
					{
						return;
					}
				}
				if (Convert.ToDateTime(objs[0][fld.PropertyName]) == Convert.ToDateTime("0001-01-01 00:00:00"))
				{
					builder.AppendFormat(" AND {0} IS NULL ", fld.FieldName);
					return;
				}
				builder.AppendFormat(" AND {0} = {1} ", fld.FieldName, "{ts'" + objs[0][fld.PropertyName] + "'}");
				return;
			}
			else
			{
				if (InventoryQuery.IsTextField(fld))
				{
					builder.AppendFormat(" AND {0} = '{1}' ", fld.FieldName, Convert.ToString(objs[0][fld.PropertyName]).Replace("'", "''"));
					return;
				}
				builder.AppendFormat(" AND {0} = {1} ", fld.FieldName, objs[0][fld.PropertyName]);
				return;
			}
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x0006EFA4 File Offset: 0x0006D1A4
		private static void GetRelateFlexFieldFilter(DynamicObject[] objs, StringBuilder builder, Field fld)
		{
			if (objs[0][fld.PropertyName] == null)
			{
				builder.AppendFormat(" AND ({0} IS NULL OR {0} = 0 ) ", fld.FieldName);
				return;
			}
			builder.AppendFormat(" AND {0} = {1} ", fld.FieldName, objs[0][fld.PropertyName + "_Id"]);
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x0006F000 File Offset: 0x0006D200
		private static void GetBaseDataFieldFilter(DynamicObject[] objs, StringBuilder builder, Field fld)
		{
			if (objs[0][fld.PropertyName] != null)
			{
				builder.AppendFormat(" AND {0} = {1} ", fld.FieldName, objs[0][fld.PropertyName + "_Id"]);
				return;
			}
			if (InventoryQuery.IsTextField(fld))
			{
				builder.AppendFormat(" AND ({0} IS NULL OR {0} = '' ) ", fld.FieldName);
				return;
			}
			builder.AppendFormat(" AND ({0} IS NULL OR {0} = 0 ) ", fld.FieldName);
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x0006F078 File Offset: 0x0006D278
		private static bool IsTextField(Field fld)
		{
			return fld.FieldType == Convert.ToInt32(231) || fld.FieldType == Convert.ToInt32(167) || fld.FieldType == Convert.ToInt32(239) || fld.FieldType == Convert.ToInt32(175) || fld.FieldType == Convert.ToInt32(36) || fld.FieldType == Convert.ToInt32(61) || fld.FieldType == Convert.ToInt32(58) || fld.FieldType == Convert.ToInt32(99) || fld.FieldType == Convert.ToInt32(35) || fld.FieldType == Convert.ToInt32(173) || fld.FieldType == Convert.ToInt32(34) || fld.FieldType == Convert.ToInt32(231) || fld.FieldType == Convert.ToInt32(189) || fld.FieldType == Convert.ToInt32(165);
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x0006F1D0 File Offset: 0x0006D3D0
		public static bool GetBoolInvUserSet(Context ctx, string formId, string settingPtyName)
		{
			bool result = false;
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "STK_InvUserSetting", true) as FormMetadata;
			if (formMetadata == null)
			{
				return false;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(ctx, formMetadata.BusinessInfo, ctx.UserId, formId, "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey(settingPtyName) && dynamicObject[settingPtyName] != null && !string.IsNullOrWhiteSpace(dynamicObject[settingPtyName].ToString()))
			{
				result = Convert.ToBoolean(dynamicObject[settingPtyName]);
			}
			return result;
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0006F250 File Offset: 0x0006D450
		public static T GetStringInvUserSet<T>(Context ctx, string formId, string settingPtyName, T defValue)
		{
			T result = defValue;
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "STK_InvUserSetting", true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(ctx, formMetadata.BusinessInfo, ctx.UserId, formId, "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey(settingPtyName) && dynamicObject[settingPtyName] != null && !string.IsNullOrWhiteSpace(dynamicObject[settingPtyName].ToString()))
			{
				result = (T)((object)dynamicObject[settingPtyName]);
			}
			return result;
		}

		// Token: 0x04000351 RID: 849
		protected int queryMode;

		// Token: 0x04000352 RID: 850
		protected int returnDataMode;

		// Token: 0x04000353 RID: 851
		protected long qOrgId;

		// Token: 0x04000354 RID: 852
		protected string stockOrgIds = "";

		// Token: 0x04000355 RID: 853
		protected string sFixFilter = "";

		// Token: 0x04000356 RID: 854
		protected string sumFormPageId = "";

		// Token: 0x04000357 RID: 855
		protected string detailFormPageId = "";
	}
}
