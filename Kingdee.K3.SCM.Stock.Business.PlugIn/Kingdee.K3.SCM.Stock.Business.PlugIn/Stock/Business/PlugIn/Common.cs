using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kingdee.BOS;
using Kingdee.BOS.Business.Bill.Service.Tax;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.SAL;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.SCM.Business;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200004B RID: 75
	public class Common
	{
		// Token: 0x06000305 RID: 773 RVA: 0x00024150 File Offset: 0x00022350
		public static long GetDefaultVMIStock(AbstractBillPlugIn billPlugIn, string ownerKey, string stockKey, string stockStatusFilter, bool needSetStock = false)
		{
			DynamicObject dynamicObject = billPlugIn.View.Model.GetValue(ownerKey) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FVmiStockId"));
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(billPlugIn.Context, new QueryBuilderParemeter
			{
				FormId = "BD_Supplier",
				SelectItems = list,
				FilterClauseWihtKey = string.Format(" FSupplierId = {0} ", num)
			}, null);
			long num2 = 0L;
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				num2 = Convert.ToInt64(dynamicObjectCollection[0]["FVmiStockId"]);
				list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FStockStatusType"));
				DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(billPlugIn.Context, new QueryBuilderParemeter
				{
					FormId = "BD_STOCK",
					SelectItems = list,
					FilterClauseWihtKey = string.Format(" FStockId = {0} ", num2)
				}, null);
				string pattern = ".*[" + stockStatusFilter.Replace(",", "") + "].*";
				if (dynamicObjectCollection2 == null || dynamicObjectCollection2.Count <= 0 || (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stockStatusFilter) && !Regex.IsMatch(Convert.ToString(dynamicObjectCollection2[0]["FStockStatusType"]), pattern)))
				{
					num2 = 0L;
				}
				if (num2 > 0L && needSetStock)
				{
					Common.SetDefaultVMIStock(billPlugIn, stockKey, num2);
				}
			}
			return num2;
		}

		// Token: 0x06000306 RID: 774 RVA: 0x000242DC File Offset: 0x000224DC
		public static void SetDefaultVMIStock(AbstractBillPlugIn billPlugIn, string stockKey, long defaultStock)
		{
			Field field = billPlugIn.View.BusinessInfo.GetField(stockKey);
			Entity entity = billPlugIn.View.BusinessInfo.GetEntity(field.EntityKey);
			billPlugIn.View.Model.GetEntityDataObject(entity);
			int entryRowCount = billPlugIn.View.Model.GetEntryRowCount(field.EntityKey);
			for (int i = 0; i < entryRowCount; i++)
			{
				billPlugIn.View.Model.SetValue(stockKey, defaultStock, i);
			}
		}

		// Token: 0x06000307 RID: 775 RVA: 0x00024360 File Offset: 0x00022560
		public static long GetDefaultVMIStockByRow(AbstractBillPlugIn billPlugIn, string ownerKey, string stockKey, string stockStatusFilter, int iRow)
		{
			DynamicObject dynamicObject = billPlugIn.View.Model.GetValue(ownerKey, iRow) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FVmiStockId"));
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(billPlugIn.Context, new QueryBuilderParemeter
			{
				FormId = "BD_Supplier",
				SelectItems = list,
				FilterClauseWihtKey = string.Format(" FSupplierId = {0} ", num)
			}, null);
			long num2 = 0L;
			if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() > 0)
			{
				num2 = Convert.ToInt64(dynamicObjectCollection[0]["FVmiStockId"]);
				list = new List<SelectorItemInfo>();
				list.Add(new SelectorItemInfo("FStockStatusType"));
				DynamicObjectCollection dynamicObjectCollection2 = QueryServiceHelper.GetDynamicObjectCollection(billPlugIn.Context, new QueryBuilderParemeter
				{
					FormId = "BD_STOCK",
					SelectItems = list,
					FilterClauseWihtKey = string.Format(" FStockId = {0} ", num2)
				}, null);
				string pattern = ".*[" + stockStatusFilter.Replace(",", "") + "].*";
				if (dynamicObjectCollection2 == null || dynamicObjectCollection2.Count <= 0 || (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stockStatusFilter) && !Regex.IsMatch(Convert.ToString(dynamicObjectCollection2[0]["FStockStatusType"]), pattern)))
				{
					num2 = 0L;
				}
				if (num2 > 0L)
				{
					Common.SetDefaultVMIStockByRow(billPlugIn, stockKey, num2, iRow);
				}
			}
			return num2;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x000244EC File Offset: 0x000226EC
		public static void SetDefaultVMIStockByRow(AbstractBillPlugIn billPlugIn, string stockKey, long defaultStock, int iRow)
		{
			Field field = billPlugIn.View.BusinessInfo.GetField(stockKey);
			Entity entity = billPlugIn.View.BusinessInfo.GetEntity(field.EntityKey);
			billPlugIn.View.Model.GetEntityDataObject(entity);
			billPlugIn.View.Model.GetEntryRowCount(field.EntityKey);
			billPlugIn.View.Model.SetValue(stockKey, defaultStock, iRow);
		}

		// Token: 0x06000309 RID: 777 RVA: 0x00024563 File Offset: 0x00022763
		public static string getVMIOwnerFilter()
		{
			return " exists (select 1 from t_bd_supplier tbs \r\n                                    inner join t_bd_supplierbusiness tbsb on tbs.fsupplierId = tbsb.fsupplierId\r\n                                    where tbsb.fvmibusiness = '1' and tbs.fsupplierid = fsupplierId) ";
		}

		// Token: 0x0600030A RID: 778 RVA: 0x00024574 File Offset: 0x00022774
		public static ConvertRuleElement SPGetConvertRule(IBillView view, string srcBillFormId, string targetBillFormId)
		{
			List<ConvertRuleElement> convertRules = ConvertServiceHelper.GetConvertRules(view.Context, srcBillFormId, targetBillFormId);
			ConvertRuleElement convertRuleElement = convertRules.FirstOrDefault((ConvertRuleElement t) => t.IsDefault);
			ListViewPlugInProxy listViewPlugInProxy = view.GetService<DynamicFormViewPlugInProxy>() as ListViewPlugInProxy;
			if (listViewPlugInProxy != null)
			{
				GetConvertRuleEventArgs getConvertRuleEventArgs = new GetConvertRuleEventArgs(srcBillFormId, targetBillFormId, 12, convertRules, convertRuleElement);
				listViewPlugInProxy.FireOnGetConvertRule(getConvertRuleEventArgs);
				convertRuleElement = (getConvertRuleEventArgs.Rule as ConvertRuleElement);
			}
			return convertRuleElement;
		}

		// Token: 0x0600030B RID: 779 RVA: 0x000245E4 File Offset: 0x000227E4
		public static T GetFormSession<T>(IDynamicFormView view, string sessionKey = "returnData", bool bRemoveAfterRead = true)
		{
			T result = default(T);
			object obj = null;
			try
			{
				if (view != null)
				{
					bool flag = view.Session.TryGetValue(sessionKey, out obj);
					if (flag && bRemoveAfterRead)
					{
						view.Session.Remove(sessionKey);
					}
					if (typeof(T).IsSubclassOf(typeof(ValueType)) && flag)
					{
						return (T)((object)Convert.ChangeType(obj, typeof(T)));
					}
					if (obj != null)
					{
						result = (T)((object)obj);
					}
				}
			}
			catch
			{
			}
			return result;
		}

		// Token: 0x0600030C RID: 780 RVA: 0x00024678 File Offset: 0x00022878
		public static bool GetStockStatusFilterStr(AbstractBillPlugIn bill, int eRow, string stockKey, out string filter)
		{
			filter = null;
			bool result = false;
			DynamicObject value = BillUtils.GetValue<DynamicObject>(bill.Model, stockKey, eRow, null, null);
			BaseDataField baseDataField = (BaseDataField)bill.View.BillBusinessInfo.GetField(stockKey);
			if (value != null)
			{
				string text = baseDataField.GetRefPropertyValue2(value, "StockStatusType").ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					text = "'" + text.Replace(",", "','") + "'";
					filter = string.Format("FType in ({0})", text);
				}
			}
			else
			{
				result = true;
				bill.View.ShowMessage(string.Format(ResManager.LoadKDString("请先录入{0}的内容", "004023030004270", 5, new object[0]), baseDataField.Name), 0);
			}
			return result;
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0002472D File Offset: 0x0002292D
		public static string SqlAppendAnd(string sql, string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				return sql;
			}
			return sql + (string.IsNullOrWhiteSpace(sql) ? "" : " AND ") + filter;
		}

		// Token: 0x0600030E RID: 782 RVA: 0x00024754 File Offset: 0x00022954
		public static void SetGroupValue(AbstractBillPlugIn billPlugin, string strOperator, string destOperatoGgroup, string type)
		{
			DynamicObject dynamicObject = billPlugin.View.Model.GetValue(strOperator) as DynamicObject;
			long num = 0L;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			if (num > 0L)
			{
				long operatorGroupIDByID = BDServiceHelper.GetOperatorGroupIDByID(billPlugin.Context, num, type);
				if (operatorGroupIDByID > 0L)
				{
					billPlugin.View.Model.SetValue(destOperatoGgroup, operatorGroupIDByID);
				}
			}
		}

		// Token: 0x0600030F RID: 783 RVA: 0x000247C0 File Offset: 0x000229C0
		public static List<long> GetPermissionViewOrg(Context ctx, string formId, string subSysId = "21")
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = formId,
				PermissionControl = 1,
				SubSystemId = subSysId
			};
			return PermissionServiceHelper.GetPermissionOrg(ctx, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x06000310 RID: 784 RVA: 0x000247F8 File Offset: 0x000229F8
		internal static string GetPriceListFilter(Context ctx, PurPriceFilterArgs arg)
		{
			long supMasterId = arg.SupMasterId;
			long providerId = arg.ProviderId;
			long currencyId = arg.CurrencyId;
			string priceType = arg.PriceType;
			DateTime billDate = arg.BillDate;
			string text = string.Format(" FPriceType = {0} and (FSupplierID=0 OR ( FSupplierID in (select a.fsupplierid from t_bd_supplier a\r\n                                        where a.fmasterid={1}) ) )\r\n                                        and FCurrencyID = {2} ", priceType, supMasterId.ToString(), currencyId.ToString());
			bool flag = SystemParameterServiceHelper.IsUseTaxCombination(ctx);
			if (flag)
			{
				text += " and FIsIncludedTax='0' ";
			}
			return text;
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0002485C File Offset: 0x00022A5C
		public static bool IsPriceListSuitable(AbstractBillPlugIn billPlugIn, PriceDiscTaxArgs args)
		{
			DynamicObject dynamicObject = billPlugIn.View.Model.GetValue(args.PriceListKey) as DynamicObject;
			DynamicObject dynamicObject2 = billPlugIn.View.Model.GetValue(args.SupplierOrCustomerKey) as DynamicObject;
			DynamicObject dynamicObject3 = billPlugIn.View.Model.GetValue(args.Provider) as DynamicObject;
			if (dynamicObject == null)
			{
				return true;
			}
			long num = Convert.ToInt64(dynamicObject["SupplierID_Id"]);
			long num2 = Convert.ToInt64(dynamicObject["SupplierLocId_Id"]);
			long num3 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			long num4 = (dynamicObject3 == null) ? 0L : Convert.ToInt64(dynamicObject3["Id"]);
			return (num == 0L && num2 == 0L) || (num == num3 && num2 == 0L) || (num == num3 && num2 == num4);
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0002493C File Offset: 0x00022B3C
		internal static string GetDiscountListFilter(Context ctx, PurPriceFilterArgs arg)
		{
			long supMasterId = arg.SupMasterId;
			long providerId = arg.ProviderId;
			long currencyId = arg.CurrencyId;
			long priceListId = arg.PriceListId;
			string priceType = arg.PriceType;
			DateTime billDate = arg.BillDate;
			string text = string.Format(" FPriceType = {0} and (FSupplierID=0 OR ( FSupplierID in (select a.fsupplierid from t_bd_supplier a\r\n                                        where a.fmasterid={1}) ) )\r\n                                        and FCurrencyID = {2} \r\n                                        ", priceType, supMasterId.ToString(), currencyId.ToString());
			if (priceListId != 0L && PurchaseServiceHelper.IsPriceListUse(ctx, priceListId))
			{
				text += string.Format(" and FPriceListId = {0}", priceListId.ToString());
			}
			return text;
		}

		// Token: 0x06000313 RID: 787 RVA: 0x000249B8 File Offset: 0x00022BB8
		internal static int GetBusinessTypeChangePriceListPriceType(string businessType)
		{
			switch (businessType)
			{
			case "JSCG":
				return 1;
			case "CG":
			case "ZCCG":
			case "FYCG":
			case "LSCG":
				return 2;
			case "WW":
				return 3;
			case "VMICG":
				return 4;
			}
			return 0;
		}

		// Token: 0x06000314 RID: 788 RVA: 0x00024A78 File Offset: 0x00022C78
		public static void ChangePriceDiscListRelated(AbstractBillPlugIn billPlugIn, PriceDiscTaxArgs args, BeforeUpdateValueEventArgs e, out string message, out bool isClearPriceList, out bool isClearDiscList)
		{
			isClearPriceList = false;
			isClearDiscList = false;
			message = string.Empty;
			DynamicObject dynamicObject = billPlugIn.View.Model.GetValue(args.SettleCurrKey) as DynamicObject;
			DynamicObject dynamicObject2 = billPlugIn.View.Model.GetValue(args.PriceListKey) as DynamicObject;
			DynamicObject dynamicObject3 = billPlugIn.View.Model.GetValue(args.DiscountListKey) as DynamicObject;
			DynamicObject dynamicObject4 = billPlugIn.View.Model.GetValue(args.SupplierOrCustomerKey) as DynamicObject;
			DynamicObject dynamicObject5 = billPlugIn.View.Model.GetValue(args.Provider) as DynamicObject;
			string a = billPlugIn.View.Model.GetValue(args.PricePoint).ToString();
			string businessType = billPlugIn.View.Model.GetValue(args.BusinessType).ToString();
			DateTime billDate = Convert.ToDateTime(billPlugIn.View.Model.GetValue(args.DateKey));
			string arg = string.Empty;
			PurPriceFilterArgs purPriceFilterArgs = new PurPriceFilterArgs();
			purPriceFilterArgs.CurrencyId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
			purPriceFilterArgs.IsIncludedTax = Convert.ToByte(billPlugIn.View.Model.GetValue(args.IsIncludeTaxKey)).ToString();
			purPriceFilterArgs.PriceType = Common.GetBusinessTypeChangePriceListPriceType(businessType).ToString();
			purPriceFilterArgs.SupMasterId = ((dynamicObject4 == null) ? 0L : Convert.ToInt64(dynamicObject4["msterId"]));
			purPriceFilterArgs.ProviderId = ((dynamicObject5 == null) ? 0L : Convert.ToInt64(dynamicObject5["Id"]));
			purPriceFilterArgs.PriceListId = ((dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]));
			if (e.Key.ToUpperInvariant() == args.DateKey.ToUpperInvariant())
			{
				billDate = Convert.ToDateTime(e.Value);
			}
			else if (e.Key.ToUpperInvariant() == args.PricePoint.ToUpperInvariant())
			{
				a = e.Value.ToString();
			}
			else if (e.Key.ToUpperInvariant() == args.PriceListKey.ToUpperInvariant())
			{
				if (e.Value is DynamicObject)
				{
					dynamicObject2 = (e.Value as DynamicObject);
					purPriceFilterArgs.PriceListId = ((dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]));
				}
				else
				{
					try
					{
						purPriceFilterArgs.PriceListId = Convert.ToInt64(e.Value);
					}
					catch
					{
						purPriceFilterArgs.PriceListId = 0L;
					}
				}
				arg = ResManager.LoadKDString("价目表", "004015030001480", 5, new object[0]);
			}
			else if (e.Key.ToUpperInvariant() == args.SettleCurrKey.ToUpperInvariant())
			{
				if (e.Value is DynamicObject)
				{
					dynamicObject = (e.Value as DynamicObject);
					purPriceFilterArgs.CurrencyId = ((dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]));
				}
				else
				{
					try
					{
						purPriceFilterArgs.CurrencyId = Convert.ToInt64(e.Value);
					}
					catch
					{
						purPriceFilterArgs.CurrencyId = 0L;
					}
				}
			}
			if (a == "2")
			{
				purPriceFilterArgs.BillDate = billDate;
			}
			else
			{
				purPriceFilterArgs.BillDate = TimeServiceHelper.GetSystemDateTime(billPlugIn.Context);
			}
			if (e.Key.ToUpperInvariant() != args.PriceListKey.ToUpperInvariant() && dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0L && Common.NeedClearPriceDiscList(billPlugIn, false, Convert.ToInt64(dynamicObject2["Id"]), args, purPriceFilterArgs))
			{
				isClearPriceList = true;
				message = ResManager.LoadKDString("价目表", "004015030001480", 5, new object[0]);
			}
			if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) > 0L && Common.NeedClearPriceDiscList(billPlugIn, true, Convert.ToInt64(dynamicObject3["Id"]), args, purPriceFilterArgs))
			{
				isClearDiscList = true;
				message = (isClearPriceList ? (message + ResManager.LoadKDString("、", "004015030001483", 5, new object[0]) + ResManager.LoadKDString("折扣表", "004015030001486", 5, new object[0])) : ResManager.LoadKDString("折扣表", "004015030001486", 5, new object[0]));
			}
			if ((e.Key.ToUpperInvariant() == args.DateKey.ToUpperInvariant() || e.Key.ToUpperInvariant() == args.PricePoint.ToUpperInvariant()) && (isClearDiscList || isClearPriceList))
			{
				message = string.Format(ResManager.LoadKDString("日期超出{0}有效期，将会清空{0}。{1}", "004023030002056", 5, new object[0]), message, Environment.NewLine);
				return;
			}
			if (e.Key.ToUpperInvariant() == args.SettleCurrKey.ToUpperInvariant() && (isClearDiscList || isClearPriceList))
			{
				message = string.Format(ResManager.LoadKDString("结算币别与{0}币别不一致，将会清空{0}。{1}", "004023030002059", 5, new object[0]), message, Environment.NewLine);
				return;
			}
			if (isClearDiscList || isClearPriceList)
			{
				message = string.Format(ResManager.LoadKDString("{0}不在{1}适用范围内，将会清空{1}。{2}", "004023030002062", 5, new object[0]), arg, message, Environment.NewLine);
			}
		}

		// Token: 0x06000315 RID: 789 RVA: 0x00024FCC File Offset: 0x000231CC
		private static bool NeedClearPriceDiscList(AbstractBillPlugIn billPlugIn, bool isChangeDiscList, long listId, PriceDiscTaxArgs relatedArgs, PurPriceFilterArgs args)
		{
			BaseDataField baseDataField;
			if (isChangeDiscList)
			{
				baseDataField = (billPlugIn.View.BillBusinessInfo.GetField(relatedArgs.DiscountListKey) as BaseDataField);
			}
			else
			{
				baseDataField = (billPlugIn.View.BillBusinessInfo.GetField(relatedArgs.PriceListKey) as BaseDataField);
			}
			string text = Common.JoinFilterString(baseDataField.Filter, string.Format(" FID = {0} AND FDocumentStatus='C' ", listId.ToString()));
			string text2;
			if (isChangeDiscList)
			{
				text2 = Common.GetDiscountListFilter(billPlugIn.Context, args);
			}
			else
			{
				text2 = Common.GetPriceListFilter(billPlugIn.Context, args);
			}
			if (!string.IsNullOrWhiteSpace(text2))
			{
				text = Common.JoinFilterString(text, text2);
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = baseDataField.LookUpObject.FormId;
			queryBuilderParemeter.FilterClauseWihtKey = text;
			queryBuilderParemeter.RequiresDataPermission = true;
			queryBuilderParemeter.SelectItems = new List<SelectorItemInfo>();
			queryBuilderParemeter.SelectItems.Add(new SelectorItemInfo(baseDataField.LookUpObject.PkFieldName));
			DynamicObject[] array = BusinessDataServiceHelper.Load(billPlugIn.Context, baseDataField.RefFormDynamicObjectType, queryBuilderParemeter);
			return array == null || array.Length == 0;
		}

		// Token: 0x06000316 RID: 790 RVA: 0x000250D6 File Offset: 0x000232D6
		private static string JoinFilterString(string filter1, string filter2)
		{
			if (StringUtils.IsEmpty(filter1))
			{
				return filter2;
			}
			if (StringUtils.IsEmpty(filter2))
			{
				return filter1;
			}
			return string.Format("({0}) AND ({1})", filter2, filter1);
		}

		// Token: 0x06000317 RID: 791 RVA: 0x000250F8 File Offset: 0x000232F8
		public static void GetRate(AbstractBillPlugIn billPlugIn, PriceDiscTaxArgs args, int row)
		{
			DateTime dateTime = Convert.ToDateTime(billPlugIn.View.Model.GetValue(args.DateKey));
			string text = string.Format("  FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' AND ({0}>=FEFFECTIVEDATE AND {0}<=FEXPIRYDATE) ", DateTimeFormatUtils.ToKSQlFormat(dateTime));
			TaxRuleConditionParam taxRuleConditionParam = Common.PrepareTaxRuleConditionParam(billPlugIn, 1, args, row);
			if (Common.svc == null)
			{
				Common.svc = new TaxService();
			}
			TaxRuleResult taxRuleResult = Common.svc.GetTaxRuleResult(taxRuleConditionParam, billPlugIn.View.Context);
			if (taxRuleResult.IsFind && taxRuleResult.TaxMixIds != null && taxRuleResult.TaxMixIds.Count > 0)
			{
				string arg = string.Join(",", taxRuleResult.TaxMixIds);
				text = text + " AND " + string.Format("FID in ({0})", arg);
			}
			SCMCommon.QueryTaxRuleListReturnTaxRate(billPlugIn, text, args.EntryTaxRateKey, row);
		}

		// Token: 0x06000318 RID: 792 RVA: 0x000251DC File Offset: 0x000233DC
		private static TaxRuleConditionParam PrepareTaxRuleConditionParam(AbstractBillPlugIn billPlugIn, TaxType taxType, PriceDiscTaxArgs args, int row)
		{
			TaxRuleConditionParam taxRuleConditionParam = new TaxRuleConditionParam();
			OrgField orgField = (from p in billPlugIn.View.BillBusinessInfo.GetFieldList()
			where p is OrgField && (p as OrgField).IsMainOrg == 1
			select p as OrgField).FirstOrDefault<OrgField>();
			if (orgField != null)
			{
				object value = billPlugIn.View.Model.GetValue(orgField.Key);
				if (value != null)
				{
					DynamicObject dynamicObject = value as DynamicObject;
					taxRuleConditionParam.MainBusinessOrg = ((dynamicObject == null) ? -1 : int.Parse(dynamicObject["Id"].ToString()));
				}
			}
			taxRuleConditionParam.TaxType = taxType;
			taxRuleConditionParam.Bill = billPlugIn.View.BillBusinessInfo.GetForm().Id;
			object value2 = billPlugIn.View.Model.GetValue(args.BillTypeKey);
			taxRuleConditionParam.BillType = ((value2 == null) ? null : value2.ToString());
			taxRuleConditionParam.PartnerType = 0;
			object value3 = billPlugIn.View.Model.GetValue(args.SupplierOrCustomerKey);
			if (value3 != null)
			{
				DynamicObject dynamicObject2 = (DynamicObject)value3;
				taxRuleConditionParam.SupplierOrCustomer = dynamicObject2["Id"].ToString();
				taxRuleConditionParam.TaxCategoryForSupplierOrCustomer = ((DynamicObjectCollection)dynamicObject2["SupplierFinance"])[0]["FTaxType_Id"].ToString();
			}
			DynamicObject dynamicObject3 = (DynamicObject)billPlugIn.View.Model.GetValue(args.MaterialKey, row);
			if (dynamicObject3 != null)
			{
				taxRuleConditionParam.Material = dynamicObject3["Id"].ToString();
				taxRuleConditionParam.TaxCategoryForMaterial = ((DynamicObjectCollection)dynamicObject3["MaterialBase"])[0]["TaxType_Id"].ToString();
			}
			return taxRuleConditionParam;
		}

		// Token: 0x06000319 RID: 793 RVA: 0x000253B4 File Offset: 0x000235B4
		public static void SetDefaultTaxRate(AbstractBillPlugIn billPlugIn, PriceDiscTaxArgs args, int row)
		{
			TaxRuleConditionParam taxRuleConditionParam = Common.PrepareTaxRuleConditionParam(billPlugIn, 1, args, row);
			if (Common.svc == null)
			{
				Common.svc = new TaxService();
			}
			TaxRuleResult taxRuleResult = Common.svc.GetTaxRuleResult(taxRuleConditionParam, billPlugIn.View.Context);
			DynamicObject defaultValue = taxRuleResult.DefaultValue;
			if (defaultValue != null)
			{
				billPlugIn.Model.SetValue(args.EntryTaxRateKey, Convert.ToDecimal(defaultValue["TaxRate"]), row);
				billPlugIn.View.InvokeFieldUpdateService(args.EntryTaxRateKey, row);
			}
		}

		// Token: 0x0600031A RID: 794 RVA: 0x00025438 File Offset: 0x00023638
		[Obsolete("The method  has been obsolete! Please use same name method in SCMCommon namespace!")]
		public static void UnitIdChangeReCalPrice(Context ctx, IBillView billView, string priceKey, int row, long materialId, long newUnitConvertRate, long oldUnitConvertRate)
		{
			UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, new GetUnitConvertRateArgs
			{
				SourceUnitId = newUnitConvertRate,
				DestUnitId = oldUnitConvertRate,
				MaterialId = materialId
			});
			if (unitConvertRate != null)
			{
				decimal num = Convert.ToDecimal(billView.Model.GetValue(priceKey, row));
				decimal num2 = unitConvertRate.ConvertQty(num, "");
				billView.Model.SetValue(priceKey, num2, row);
				billView.InvokeFieldUpdateService(priceKey, row);
			}
		}

		// Token: 0x0600031B RID: 795 RVA: 0x000254AC File Offset: 0x000236AC
		public static string FlexValToString(string formId, DynamicObject obj)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(formId);
			foreach (DynamicProperty dynamicProperty in obj.DynamicObjectType.Properties)
			{
				if (dynamicProperty.Name != "Id")
				{
					string value;
					if (dynamicProperty.GetValue(obj) is DynamicObject)
					{
						object primaryKeyValue = OrmUtils.GetPrimaryKeyValue((DynamicObject)dynamicProperty.GetValue(obj), true);
						value = ((primaryKeyValue == null) ? "" : Convert.ToString(primaryKeyValue));
					}
					else
					{
						value = ((obj[dynamicProperty.Name] == null) ? "" : Convert.ToString(obj[dynamicProperty.Name]));
					}
					stringBuilder.Append("||").Append(value);
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600031C RID: 796 RVA: 0x00025678 File Offset: 0x00023878
		public static void BomExpand(AbstractBillPlugIn billplugin)
		{
			MemBomExpandOption memBomExpandOption = new MemBomExpandOption();
			DynamicObject dynamicObject = null;
			memBomExpandOption.ExpandLevelTo = Common.GetExpandLevel(billplugin, ref dynamicObject);
			memBomExpandOption.ValidDate = new DateTime?(TimeServiceHelper.GetSystemDateTime(billplugin.Context).Date);
			memBomExpandOption.IsConvertUnitQty = true;
			memBomExpandOption.BomExpandId = Guid.NewGuid().ToString();
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("BOMExpendCarryCsdSub"))
			{
				memBomExpandOption.CsdSubstitution = Convert.ToBoolean(dynamicObject["BOMExpendCarryCsdSub"]);
			}
			List<DynamicObject> bomSourceData = Common.GetBomSourceData(memBomExpandOption, billplugin);
			if (bomSourceData == null || bomSourceData.Count<DynamicObject>() < 1)
			{
				return;
			}
			DynamicObject dynamicObject2 = MFGServiceHelperForSCM.ExpandBomForward(billplugin.Context, bomSourceData, memBomExpandOption);
			DynamicObject dynamicObject3 = billplugin.View.Model.GetValue("FOwnerIdHead") as DynamicObject;
			long ownerId = 0L;
			if (dynamicObject3 != null)
			{
				ownerId = Convert.ToInt64(dynamicObject3["Id"]);
			}
			if (dynamicObject2 != null)
			{
				bool isConLossRate = false;
				bool flag = false;
				bool isOrderCarryMto = false;
				if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
				{
					isConLossRate = Convert.ToBoolean(dynamicObject["ConLossRate"]);
				}
				if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("BOMExpendCarryMat"))
				{
					flag = Convert.ToBoolean(dynamicObject["BOMExpendCarryMat"]);
				}
				if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
				{
					isOrderCarryMto = Convert.ToBoolean(dynamicObject["OrderCarryMto"]);
				}
				DynamicObjectCollection dynamicObjectCollection = dynamicObject2["BomExpandResult"] as DynamicObjectCollection;
				List<DynamicObject> list = new List<DynamicObject>();
				using (IEnumerator<DynamicObject> enumerator = dynamicObjectCollection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject dyCurRow = enumerator.Current;
						if (!string.IsNullOrWhiteSpace(dyCurRow["ParentEntryId"].ToString()))
						{
							string rowId = dyCurRow["RowId"].ToString();
							if (dynamicObjectCollection.FirstOrDefault((DynamicObject p) => rowId.Equals(p["ParentEntryId"].ToString())) == null)
							{
								DynamicObject dynamicObject4 = dyCurRow["MATERIALID"] as DynamicObject;
								if ((dynamicObject4 == null || ((!flag || !Convert.ToString(dyCurRow["ISSUETYPE"]).Equals("7")) && Convert.ToBoolean(((DynamicObjectCollection)dynamicObject4["MaterialBase"])[0]["IsInventory"]))) && !Convert.ToString(dyCurRow["MaterialType"]).Equals("2"))
								{
									if (Convert.ToString(dyCurRow["MaterialType"]).Equals("3"))
									{
										dyCurRow["ACCUDISASSMBLERATE"] = 0;
										dyCurRow["BASEQTY"] = 0;
										dyCurRow["BaseActualQty"] = 0;
									}
									DynamicObject dynamicObject5 = list.FirstOrDefault((DynamicObject p) => Convert.ToInt64(p["MATERIALID_Id"]) == Convert.ToInt64(dyCurRow["MATERIALID_Id"]) && Convert.ToInt32(p["SRCSEQNO"]) == Convert.ToInt32(dyCurRow["SRCSEQNO"]) && Convert.ToString(p["OWNERTYPEID"]) == Convert.ToString(dyCurRow["OWNERTYPEID"]) && Convert.ToInt64(p["OWNERID_Id"]) == Convert.ToInt64(dyCurRow["OWNERID_Id"]));
									if (dynamicObject5 == null)
									{
										dyCurRow["DISASSMBLERATE"] = dyCurRow["ACCUDISASSMBLERATE"];
										list.Add(dyCurRow);
									}
									else
									{
										if (Convert.ToInt32(dynamicObject5["BOMLEVEL"]) < Convert.ToInt32(dyCurRow["BOMLEVEL"]))
										{
											dynamicObject5["BOMLEVEL"] = dyCurRow["BOMLEVEL"];
										}
										dynamicObject5["BASEQTY"] = Convert.ToDecimal(dynamicObject5["BASEQTY"]) + Convert.ToDecimal(dyCurRow["BASEQTY"]);
										dynamicObject5["QTY"] = Convert.ToDecimal(dynamicObject5["QTY"]) + Convert.ToDecimal(dyCurRow["QTY"]);
										dynamicObject5["BaseActualQty"] = Convert.ToDecimal(dynamicObject5["BaseActualQty"]) + Convert.ToDecimal(dyCurRow["BaseActualQty"]);
										dynamicObject5["DISASSMBLERATE"] = Convert.ToDecimal(dynamicObject5["DISASSMBLERATE"]) + Convert.ToDecimal(dyCurRow["ACCUDISASSMBLERATE"]);
									}
								}
							}
						}
					}
				}
				Common.UpdateSubEntity(Common.UpdateUnitId(billplugin.Context, list, isConLossRate), billplugin, ownerId, isOrderCarryMto);
			}
			MFGServiceHelperForSCM.ClearBomExpandResult(billplugin.Context, memBomExpandOption);
			int entryRowCount = billplugin.View.Model.GetEntryRowCount("FBillEntry");
			for (int i = entryRowCount - 1; i >= 0; i--)
			{
				if (!(billplugin.View.Model.GetValue("FMaterialId", i) is DynamicObject))
				{
					billplugin.View.Model.DeleteEntryRow("FBillEntry", i);
				}
			}
			DynamicObjectCollection dynamicObjectCollection2 = billplugin.View.Model.DataObject["OEMInStockEntry"] as DynamicObjectCollection;
			for (int j = 0; j < dynamicObjectCollection2.Count<DynamicObject>(); j++)
			{
				dynamicObjectCollection2[j]["Seq"] = j + 1;
			}
		}

		// Token: 0x0600031D RID: 797 RVA: 0x00025D38 File Offset: 0x00023F38
		private static void UpdateSubEntity(List<DynamicObject> result, AbstractBillPlugIn billplugin, long OwnerId, bool IsOrderCarryMto = false)
		{
			DynamicObjectCollection dynamicObjectCollection = billplugin.View.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			DynamicObjectType dynamicObjectType = billplugin.View.BusinessInfo.GetEntity("FBillEntry").DynamicObjectType;
			billplugin.Model.BusinessInfo.GetEntity("FBillEntry");
			billplugin.View.Model.GetValue("FOwnerIdHead");
			int num = 0;
			using (IEnumerator<DynamicObject> enumerator = dynamicObjectCollection.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Common.<>c__DisplayClass16 CS$<>8__locals2 = new Common.<>c__DisplayClass16();
					CS$<>8__locals2.entryRow = enumerator.Current;
					int seq = Convert.ToInt32(CS$<>8__locals2.entryRow["Seq"]);
					List<DynamicObject> list = (from p in result
					where Convert.ToInt32(p["SRCSEQNO"]) == seq && Convert.ToString(p["OWNERTYPEID"]) == "BD_Customer" && (Convert.ToInt64(p["OWNERID_Id"]) == OwnerId || Convert.ToInt64(p["OWNERID_Id"]) == 0L) && Convert.ToInt32(p["BOMLEVEL"]) != 0
					select p).ToList<DynamicObject>();
					int count = list.Count;
					DynamicObjectCollection dynamicObjectCollection2 = billplugin.View.Model.DataObject["OEMInStockEntry"] as DynamicObjectCollection;
					List<DynamicObject> list2 = (from p in dynamicObjectCollection2
					where Convert.ToInt64(p["SrcEntryId"]) == Convert.ToInt64(CS$<>8__locals2.entryRow["SubSrcEntryId"]) && Convert.ToInt32(p["SrcSeqNo"]) == seq
					select p).ToList<DynamicObject>();
					if (list2 != null)
					{
						foreach (DynamicObject item in list2)
						{
							dynamicObjectCollection2.Remove(item);
						}
					}
					if (count >= 1)
					{
						billplugin.Model.BatchCreateNewEntryRow("FBillEntry", count);
						foreach (DynamicObject item2 in list)
						{
							if (Common.UpdateSubRowData(item2, billplugin, num, CS$<>8__locals2.entryRow, IsOrderCarryMto))
							{
								num++;
							}
						}
						if (list2 != null)
						{
							for (int i = count - 1; i > num; i--)
							{
								list2.RemoveAt(i);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00025FBC File Offset: 0x000241BC
		private static bool UpdateSubRowData(DynamicObject item, AbstractBillPlugIn billplugin, int row, DynamicObject dyRow, bool IsOrderCarryMto)
		{
			billplugin.Model.SetValue("FMaterialID", item["MATERIALID_Id"], row);
			DynamicObject dynamicObject = billplugin.Model.GetValue("FMaterialID", row) as DynamicObject;
			if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
			{
				if (item["MATERIALID"] == null || !DynamicObjectUtils.Contains((DynamicObject)item["MATERIALID"], "Number"))
				{
					return false;
				}
				billplugin.Model.SetItemValueByNumber("FMaterialID", Convert.ToString(((DynamicObject)item["MATERIALID"])["Number"]), row);
				dynamicObject = (billplugin.Model.GetValue("FMaterialID", row) as DynamicObject);
				if (dynamicObject == null || Convert.ToInt64(dynamicObject["Id"]) <= 0L)
				{
					return false;
				}
			}
			billplugin.Model.SetValue("FSrcSeqNo", Convert.ToString(dyRow["Seq"]), row);
			billplugin.Model.SetValue("FReSrcSeq", Convert.ToString(dyRow["SubSrcSeq"]), row);
			billplugin.Model.SetValue("FReSrcBillNo", Convert.ToString(dyRow["SubSRCBILLNO"]), row);
			billplugin.Model.SetValue("FSrcEntryId", Convert.ToString(dyRow["SubSrcEntryId"]), row);
			billplugin.View.InvokeFieldUpdateService("FMaterialID", row);
			billplugin.Model.SetValue("FUnitID", item["UNITID_Id"], row);
			billplugin.Model.SetValue("FBaseUnitID", item["BASEUNITID_Id"], row);
			billplugin.Model.SetValue("FQty", item["QTY"], row);
			billplugin.View.InvokeFieldUpdateService("FQty", row);
			DynamicObject dynamicObject2 = billplugin.Model.GetValue("FStockOrgId", row) as DynamicObject;
			long num = 0L;
			if (dynamicObject2 != null)
			{
				num = Convert.ToInt64(dynamicObject2["Id"]);
			}
			billplugin.Model.SetValue("FAuxPropId", item["AuxPropId_Id"], row);
			DynamicObjectCollection source = dynamicObject["MaterialInvPty"] as DynamicObjectCollection;
			DynamicObject dynamicObject3 = source.SingleOrDefault((DynamicObject p) => Convert.ToBoolean(p["IsEnable"]) && Convert.ToInt64(p["InvPtyId_Id"]) == 10003L);
			if (dynamicObject3 != null)
			{
				long bomDefaultValueByMaterial = SCMCommon.GetBomDefaultValueByMaterial(billplugin.Context, dynamicObject, Convert.ToInt64(item["AuxPropId_Id"]), false, num, false);
				billplugin.Model.SetValue("FBomID", bomDefaultValueByMaterial, row);
			}
			if (IsOrderCarryMto)
			{
				string text = Convert.ToString(((DynamicObjectCollection)dynamicObject["MaterialPlan"])[0]["PlanMode"]);
				if (text.Equals("1") || text.Equals("2"))
				{
					billplugin.Model.SetValue("FMtoNo", Convert.ToString(dyRow["SubMtoNo"]), row);
				}
			}
			billplugin.Model.SetValue("FKeeperID", dynamicObject2, row);
			return true;
		}

		// Token: 0x0600031F RID: 799 RVA: 0x000262E4 File Offset: 0x000244E4
		private static List<DynamicObject> GetBomSourceData(MemBomExpandOption bomQueryOption, AbstractBillPlugIn billplugin)
		{
			DynamicObjectCollection dynamicObjectCollection = billplugin.View.Model.DataObject["SubHeadEntity"] as DynamicObjectCollection;
			List<DynamicObject> list = new List<DynamicObject>();
			int num = (from p in dynamicObjectCollection
			where p["SubBomId"] == null
			select p).Count<DynamicObject>();
			if (num == dynamicObjectCollection.Count<DynamicObject>())
			{
				billplugin.View.ShowMessage(ResManager.LoadKDString("请选择BOM版本", "004023000012229", 5, new object[0]), 0);
			}
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (dynamicObject["SubBomId"] != null && dynamicObject["SubMaterialId"] != null && Convert.ToBoolean(dynamicObject["Select"]))
				{
					int num2 = Convert.ToInt32(dynamicObject["Seq"]);
					long materialId_Id = Convert.ToInt64((dynamicObject["SubMaterialId"] as DynamicObject)["Id"]);
					long bomId_Id = Convert.ToInt64((dynamicObject["SubBomId"] as DynamicObject)["Id"]);
					decimal needQty = Convert.ToDecimal(dynamicObject["SubBaseQty"]);
					long srcEntryId = Convert.ToInt64(dynamicObject["Id"]);
					BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
					bomForwardSourceDynamicRow.MaterialId_Id = materialId_Id;
					bomForwardSourceDynamicRow.BomId_Id = bomId_Id;
					bomForwardSourceDynamicRow.NeedQty = needQty;
					bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
					bomForwardSourceDynamicRow.UnitId_Id = Convert.ToInt64((dynamicObject["SubUnitID"] as DynamicObject)["Id"]);
					bomForwardSourceDynamicRow.SrcInterId = Convert.ToInt64(billplugin.View.Model.GetPKValue());
					bomForwardSourceDynamicRow.SrcEntryId = srcEntryId;
					bomForwardSourceDynamicRow.SrcSeqNo = (long)num2;
					list.Add(bomForwardSourceDynamicRow.DataEntity);
				}
			}
			return list;
		}

		// Token: 0x06000320 RID: 800 RVA: 0x00026500 File Offset: 0x00024700
		private static int GetExpandLevel(AbstractBillPlugIn billplugin, ref DynamicObject setting)
		{
			int result = 1;
			string id = billplugin.View.BusinessInfo.GetForm().Id;
			string text = string.Empty;
			if (!string.IsNullOrWhiteSpace(billplugin.View.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = billplugin.View.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(billplugin.Context, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			setting = UserParamterServiceHelper.Load(billplugin.Context, formMetadata.BusinessInfo, billplugin.Context.UserId, id, "UserParameter");
			if (setting == null || !setting.DynamicObjectType.Properties.ContainsKey("ExpandType"))
			{
				return result;
			}
			string text2 = Convert.ToString(setting["ExpandType"]);
			string a;
			if ((a = text2) != null)
			{
				if (a == "1")
				{
					return 1;
				}
				if (a == "2")
				{
					return 30;
				}
				if (a == "3")
				{
					return (Convert.ToInt32(Convert.ToString(setting["ExpandLevel"])) == 0) ? 1 : Convert.ToInt32(Convert.ToString(setting["ExpandLevel"]));
				}
			}
			return 1;
		}

		// Token: 0x06000321 RID: 801 RVA: 0x00026640 File Offset: 0x00024840
		public static Dictionary<string, bool> GetInvBaseDataCtrolType(Context ctx)
		{
			Dictionary<string, int> baseDataControlType = BDCommonServiceHelper.GetBaseDataControlType(ctx, new List<string>
			{
				"BD_MATERIAL",
				"BD_STOCK",
				"ENG_BOM"
			});
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (string key in baseDataControlType.Keys)
			{
				dictionary[key] = (baseDataControlType[key] == 2 || baseDataControlType[key] == 3);
			}
			return dictionary;
		}

		// Token: 0x06000322 RID: 802 RVA: 0x000266F8 File Offset: 0x000248F8
		public static List<DynamicObject> UpdateUnitId(Context ctx, List<DynamicObject> result, bool IsConLossRate = false)
		{
			Dictionary<long, long> unitIdByMaterilId = Common.GetUnitIdByMaterilId(ctx, (from p in result
			select Convert.ToInt64(p["MATERIALID_ID"])).ToList<long>());
			foreach (DynamicObject dynamicObject in result)
			{
				GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs
				{
					MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
					DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
					SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
				};
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
				if (!IsConLossRate)
				{
					dynamicObject["UNITID_Id"] = unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])];
					getUnitConvertRateArgs = new GetUnitConvertRateArgs
					{
						MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
						DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
						SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
					};
					unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
					if (unitConvertRate != null && unitConvertRate.ConvertNumerator / unitConvertRate.ConvertDenominator > 0m)
					{
						dynamicObject["QTY"] = unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseActualQty"]), "");
					}
				}
				else if (unitConvertRate.ConvertType == 1 && unitIdByMaterilId.Keys.Contains(Convert.ToInt64(dynamicObject["MATERIALID_Id"])) && Convert.ToInt64(dynamicObject["UNITID_Id"]) != unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])])
				{
					dynamicObject["UNITID_Id"] = unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])];
					getUnitConvertRateArgs = new GetUnitConvertRateArgs
					{
						MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
						DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
						SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
					};
					unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
					if (unitConvertRate != null && unitConvertRate.ConvertNumerator / unitConvertRate.ConvertDenominator > 0m)
					{
						dynamicObject["QTY"] = ((!IsConLossRate) ? unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseActualQty"]), "") : unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseQty"]), ""));
					}
				}
			}
			return result;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x00026A0C File Offset: 0x00024C0C
		public static List<DynamicObject> UpdateUnitId(Context ctx, List<DynamicObject> result)
		{
			Dictionary<long, long> unitIdByMaterilId = Common.GetUnitIdByMaterilId(ctx, (from p in result
			select Convert.ToInt64(p["MATERIALID_ID"])).ToList<long>());
			foreach (DynamicObject dynamicObject in result)
			{
				GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs
				{
					MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
					DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
					SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
				};
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
				if (unitConvertRate.ConvertType == 1 && unitIdByMaterilId.Keys.Contains(Convert.ToInt64(dynamicObject["MATERIALID_Id"])) && Convert.ToInt64(dynamicObject["UNITID_Id"]) != unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])])
				{
					dynamicObject["UNITID_Id"] = unitIdByMaterilId[Convert.ToInt64(dynamicObject["MATERIALID_Id"])];
					getUnitConvertRateArgs = new GetUnitConvertRateArgs
					{
						MaterialId = Convert.ToInt64(dynamicObject["MATERIALID_Id"]),
						DestUnitId = Convert.ToInt64(dynamicObject["UNITID_Id"]),
						SourceUnitId = Convert.ToInt64(dynamicObject["BASEUNITID_Id"])
					};
					unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(ctx, getUnitConvertRateArgs);
					if (unitConvertRate != null && unitConvertRate.ConvertNumerator / unitConvertRate.ConvertDenominator > 0m)
					{
						dynamicObject["QTY"] = unitConvertRate.ConvertQty(Convert.ToDecimal(dynamicObject["BaseActualQty"]), "");
					}
				}
			}
			return result;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00026C14 File Offset: 0x00024E14
		public static Dictionary<long, long> GetUnitIdByMaterilId(Context ctx, List<long> materialids)
		{
			if (materialids == null || materialids.Count<long>() < 1)
			{
				return null;
			}
			return BDCommonServiceHelper.GetUnitIdByMaterilId(ctx, materialids);
		}

		// Token: 0x06000325 RID: 805 RVA: 0x00026C2B File Offset: 0x00024E2B
		public static bool GetProductGroupFieldFilter(int orgId, out string filter)
		{
			filter = string.Format("\r\n            FPRODUCTGROUPID IN (\r\n            SELECT DISTINCT T.FPRODUCTGROUPID FROM T_CB_PROACNTGROUP T \r\n            INNER JOIN T_ORG_ACCOUNTSYSTEM TA ON T.FACCTGSYSTEMID = TA.FACCTSYSTEMID\r\n            INNER JOIN T_ORG_ACCTSYSENTRY TE ON TE.FACCTSYSTEMID = TA.FACCTSYSTEMID \r\n\t\t\t\t\t\t\t\t            AND T.FACCTGORGID = TE.FMAINORGID\r\n            INNER JOIN T_ORG_ACCTSYSDETAIL TD ON TD.FENTRYID = TE.FENTRYID\r\n            WHERE T.FFORBIDSTATUS = 'A' AND T.FDOCUMENTSTATUS = 'C' \r\n\t            AND TA.FISDefault = '1' AND T.FGROUPBY = '2' AND TD.FSUBORGID = {0}\r\n            )", orgId);
			return !string.IsNullOrWhiteSpace(filter);
		}

		// Token: 0x06000326 RID: 806 RVA: 0x00026C4C File Offset: 0x00024E4C
		public static void SynHeadOwner(AbstractBillPlugIn billPlugin, string entitykey, string ownerheadkey, string ownerkey)
		{
			DynamicObject dynamicObject = billPlugin.View.Model.GetValue(ownerheadkey) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			int entryRowCount = billPlugin.View.Model.GetEntryRowCount(entitykey);
			for (int i = 0; i < entryRowCount; i++)
			{
				if (billPlugin.View.GetControl(ownerkey).Enabled)
				{
					billPlugin.View.Model.SetValue(ownerkey, num, i);
				}
			}
		}

		// Token: 0x06000327 RID: 807 RVA: 0x00026CD4 File Offset: 0x00024ED4
		public static void SynOwnerType(AbstractBillPlugIn billPlugin, string sourfield, string destfield)
		{
			string text = billPlugin.View.Model.GetValue(sourfield) as string;
			Field field = billPlugin.View.BusinessInfo.GetField(destfield);
			int entryRowCount = billPlugin.View.Model.GetEntryRowCount(field.EntityKey);
			for (int i = 0; i < entryRowCount; i++)
			{
				billPlugin.View.Model.SetValue(destfield, text, i);
			}
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00026D84 File Offset: 0x00024F84
		public static List<int> FillTransBaseMapData(IBillView billView, long srcOrgId, long destOrgId, string srcFieldKey, string destFieldKey, string baseDataNuKey, string baseDataNuName)
		{
			if (srcOrgId == destOrgId)
			{
				return null;
			}
			BaseDataField baseDataField = billView.BusinessInfo.GetField(srcFieldKey) as BaseDataField;
			BaseDataField baseDataField2 = billView.BusinessInfo.GetField(destFieldKey) as BaseDataField;
			if (baseDataField == null || baseDataField2 == null)
			{
				return null;
			}
			if (baseDataField2.LookUpObject.FormId != baseDataField2.LookUpObject.FormId)
			{
				return null;
			}
			if (CommonServiceHelper.GetBaseDataIsShare(billView.Context, baseDataField.LookUpObject.FormId))
			{
				return null;
			}
			List<string> list = new List<string>();
			Dictionary<int, string> dataIndexs = new Dictionary<int, string>();
			DynamicObjectCollection entityDataObject = billView.Model.GetEntityDataObject(baseDataField.Entity);
			int num = 0;
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				DynamicObject dynamicObject2 = dynamicObject[baseDataField.DynamicProperty.Name] as DynamicObject;
				if (dynamicObject2 == null)
				{
					num++;
				}
				else
				{
					string text = Convert.ToString(dynamicObject2[baseDataNuName]);
					long num2 = Convert.ToInt64(dynamicObject2["Id"]);
					if (string.IsNullOrWhiteSpace(text) || num2 == 0L)
					{
						num++;
					}
					else
					{
						DynamicObject dynamicObject3 = dynamicObject[baseDataField2.DynamicProperty.Name] as DynamicObject;
						if (dynamicObject3 != null)
						{
							long num3 = Convert.ToInt64(dynamicObject3["Id"]);
							string value = Convert.ToString(dynamicObject3[baseDataNuName]);
							if (num3 > 0L && num3 != num2 && text.Equals(value))
							{
								num++;
								continue;
							}
						}
						dataIndexs[num] = text;
						if (!list.Contains(text))
						{
							list.Add(text);
						}
						num++;
					}
				}
			}
			if (list.Count < 1)
			{
				return null;
			}
			string[] array = list.Distinct<string>().ToArray<string>();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = baseDataField2.LookUpObject.FormId;
			queryBuilderParemeter.IsShowApproved = true;
			queryBuilderParemeter.IsShowUsed = true;
			queryBuilderParemeter.FilterClauseWihtKey = string.Format(" {0} IN (SELECT /*+ cardinality(TD {1})*/ TD.FID FROM table(fn_StrSplit(@FNUMBERS, ',', 3)) TD) AND FUSEORGID = {2}", baseDataNuKey, array.Length, destOrgId);
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@FNUMBERS", 163, array));
			DynamicObject[] array2 = BusinessDataServiceHelper.LoadFromCache(billView.Context, baseDataField2.RefFormDynamicObjectType, queryBuilderParemeter);
			if (array2 == null || array2.Length < 1)
			{
				return null;
			}
			List<int> list2 = new List<int>();
			using (Dictionary<int, string>.KeyCollection.Enumerator enumerator2 = dataIndexs.Keys.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					int rowIndex = enumerator2.Current;
					DynamicObject dynamicObject4 = array2.FirstOrDefault((DynamicObject p) => Convert.ToString(p[baseDataNuName]).Equals(dataIndexs[rowIndex]));
					if (dynamicObject4 != null)
					{
						billView.Model.SetValue(baseDataField2.Key, dynamicObject4, rowIndex);
						list2.Add(rowIndex);
					}
				}
			}
			return list2;
		}

		// Token: 0x06000329 RID: 809 RVA: 0x00027350 File Offset: 0x00025550
		public static decimal GetResultByGroupSumType(BusinessInfo info, List<DynamicObject> list, Field field, int groupSumType)
		{
			DynamicProperty prop = field.DynamicProperty;
			decimal result = 0m;
			if (list.Count > 0)
			{
				if (groupSumType == 1)
				{
					decimal num;
					if (!(field is DecimalField))
					{
						num = list.Sum(delegate(DynamicObject o)
						{
							BasePropertyField basePropertyField = field as BasePropertyField;
							if (basePropertyField != null)
							{
								Field field2 = info.GetField(basePropertyField.ControlFieldKey);
								if (o != null && o.DynamicObjectType.Properties.Contains(field2.PropertyName))
								{
									object fieldValue = basePropertyField.GetFieldValue(o);
									if (fieldValue == null)
									{
										return 0m;
									}
									return Convert.ToDecimal(fieldValue);
								}
							}
							return 0m;
						});
					}
					else
					{
						num = list.Sum(delegate(DynamicObject o)
						{
							object valueFast = prop.GetValueFast(o);
							if (valueFast == null)
							{
								return 0m;
							}
							decimal num5;
							if (decimal.TryParse(valueFast.ToString(), out num5))
							{
								return Convert.ToDecimal(valueFast);
							}
							return 0m;
						});
					}
					result = num;
				}
				else if (groupSumType == 5)
				{
					decimal num2;
					if (!(field is DecimalField))
					{
						num2 = list.Average(delegate(DynamicObject o)
						{
							BasePropertyField basePropertyField = field as BasePropertyField;
							if (basePropertyField != null)
							{
								Field field2 = info.GetField(basePropertyField.ControlFieldKey);
								if (o != null && o.DynamicObjectType.Properties.Contains(field2.PropertyName))
								{
									object fieldValue = basePropertyField.GetFieldValue(o);
									if (fieldValue == null)
									{
										return 0m;
									}
									return Convert.ToDecimal(fieldValue);
								}
							}
							return 0m;
						});
					}
					else
					{
						num2 = list.Average(delegate(DynamicObject o)
						{
							object valueFast = prop.GetValueFast(o);
							if (valueFast == null)
							{
								return 0m;
							}
							return Convert.ToDecimal(valueFast);
						});
					}
					result = num2;
				}
				else if (groupSumType == 3)
				{
					decimal num3;
					if (!(field is DecimalField))
					{
						num3 = list.Max(delegate(DynamicObject o)
						{
							BasePropertyField basePropertyField = field as BasePropertyField;
							if (basePropertyField == null)
							{
								return 0m;
							}
							Field field2 = info.GetField(basePropertyField.ControlFieldKey);
							if (o == null || !o.DynamicObjectType.Properties.Contains(field2.PropertyName))
							{
								return 0m;
							}
							object fieldValue = basePropertyField.GetFieldValue(o);
							if (fieldValue == null)
							{
								return 0m;
							}
							return Convert.ToDecimal(fieldValue);
						});
					}
					else
					{
						num3 = list.Max(delegate(DynamicObject o)
						{
							object valueFast = prop.GetValueFast(o);
							if (valueFast == null)
							{
								return 0m;
							}
							return Convert.ToDecimal(valueFast);
						});
					}
					result = num3;
				}
				else if (groupSumType == 4)
				{
					decimal num4;
					if (!(field is DecimalField))
					{
						num4 = list.Min(delegate(DynamicObject o)
						{
							BasePropertyField basePropertyField = field as BasePropertyField;
							if (basePropertyField == null)
							{
								return 0m;
							}
							Field field2 = info.GetField(basePropertyField.ControlFieldKey);
							if (o == null || !o.DynamicObjectType.Properties.Contains(field2.PropertyName))
							{
								return 0m;
							}
							object fieldValue = basePropertyField.GetFieldValue(o);
							if (fieldValue == null)
							{
								return 0m;
							}
							return Convert.ToDecimal(fieldValue);
						});
					}
					else
					{
						num4 = list.Min(delegate(DynamicObject o)
						{
							object valueFast = prop.GetValueFast(o);
							if (valueFast == null)
							{
								return 0m;
							}
							return Convert.ToDecimal(valueFast);
						});
					}
					result = num4;
				}
				else if (groupSumType == 2)
				{
					result = list.Count;
				}
			}
			return result;
		}

		// Token: 0x0600032A RID: 810 RVA: 0x000274F4 File Offset: 0x000256F4
		public static void SetBomExpandBillToConvertForm(Context ctx, List<ConvertBillElement> sourceBills)
		{
			sourceBills.Add(new ConvertBillElement
			{
				FormID = "ENG_PRODUCTSTRUCTURE",
				ConvertBillType = 0,
				Name = new LocaleValue(ResManager.LoadKDString("物料清单", "004023030004291", 5, new object[0]), ctx.UserLocale.LCID)
			});
		}

		// Token: 0x0600032B RID: 811 RVA: 0x0002754C File Offset: 0x0002574C
		public static bool HaveBOMViewPermission(Context ctx, long stockOrgId)
		{
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(ctx, new BusinessObject
			{
				Id = "ENG_BOM"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			return permissionOrg != null && permissionOrg.Count<long>() > 0 && permissionOrg.Contains(stockOrgId);
		}

		// Token: 0x0600032C RID: 812 RVA: 0x00027590 File Offset: 0x00025790
		public static void SetBomExpandConvertRuleinfo(Context ctx, IBillView view, GetConvertRuleEventArgs e)
		{
			ConvertRuleElement convertRuleElement = ConvertServiceHelper.GetConvertRules(ctx, "ENG_BomExpandBill", e.TargetFormId).FirstOrDefault<ConvertRuleElement>();
			if (convertRuleElement != null)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = e.SourceFormId;
				dynamicFormShowParameter.OpenStyle.ShowType = 5;
				dynamicFormShowParameter.CustomParams["ShowMode"] = 3.ToString();
				dynamicFormShowParameter.ParentPageId = view.PageId;
				e.DynamicFormShowParameter = dynamicFormShowParameter;
				Common.SetBomExpandRuleFieldMap(ctx, view, convertRuleElement);
			}
			e.Rule = convertRuleElement;
		}

		// Token: 0x0600032D RID: 813 RVA: 0x00027610 File Offset: 0x00025810
		public static SelBomBillParam GetBomExpandBillFieldValue(IBillView view, string StockOrgKey, string OwnerTypeKey = "", string OwnerIdKey = "")
		{
			SelBomBillParam selBomBillParam = new SelBomBillParam();
			if (!string.IsNullOrEmpty(StockOrgKey))
			{
				selBomBillParam.StockOrgId = BillUtils.GetValue<long>(view.Model, StockOrgKey, -1, 0L, null);
				selBomBillParam.PrdOrgId = BillUtils.GetValue<long>(view.Model, StockOrgKey, -1, 0L, null);
			}
			if (!string.IsNullOrEmpty(OwnerTypeKey))
			{
				selBomBillParam.OwnerType = BillUtils.GetValue<string>(view.Model, OwnerTypeKey, -1, "BD_OwnerOrg", null);
			}
			if (!string.IsNullOrEmpty(OwnerIdKey))
			{
				selBomBillParam.OwnerId = BillUtils.GetValue<long>(view.Model, OwnerIdKey, -1, 0L, null);
			}
			return selBomBillParam;
		}

		// Token: 0x0600032E RID: 814 RVA: 0x00027698 File Offset: 0x00025898
		public static bool ValidateBomExpandBillFieldValue(IBillView view, SelBomBillParam selBillParam, List<string> lstField)
		{
			bool result = true;
			if (selBillParam.StockOrgId <= 0L)
			{
				lstField.Add(ResManager.LoadKDString("库存组织", "004023030009090", 5, new object[0]));
				result = false;
			}
			if (view.BillBusinessInfo.GetForm().Id.Equals("STK_MisDelivery") || view.BillBusinessInfo.GetForm().Id.Equals("STK_TransferDirect") || view.BillBusinessInfo.GetForm().Id.Equals("STK_TRANSFEROUT"))
			{
				if (string.IsNullOrWhiteSpace(selBillParam.OwnerType))
				{
					lstField.Add(ResManager.LoadKDString("货主类型", "004023030004306", 5, new object[0]));
					result = false;
				}
				if (!string.IsNullOrWhiteSpace(selBillParam.OwnerType) && StringUtils.EqualsIgnoreCase(selBillParam.OwnerType, "BD_OwnerOrg") && selBillParam.OwnerId <= 0L)
				{
					lstField.Add(ResManager.LoadKDString("货主", "004023030004309", 5, new object[0]));
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600032F RID: 815 RVA: 0x000277F0 File Offset: 0x000259F0
		public static void DoBomExpandDraw(IBillView view, SelBomBillParam selBomBillParam)
		{
			SelBill0ption formSession = Common.GetFormSession<SelBill0ption>(view, "returnData", true);
			IEnumerable<DynamicObject> listSelRows = formSession.ListSelRows;
			if (listSelRows == null || listSelRows.Count<DynamicObject>() <= 0)
			{
				return;
			}
			int i = 0;
			ListSelectedRow[] source = (from p in listSelRows
			select new ListSelectedRow("0", BillUtils.GetDynamicObjectItemValue<long>(p, "Id", 0L).ToString(), i++, "ENG_BomExpandBill")
			{
				EntryEntityKey = "FBomResult"
			}).ToArray<ListSelectedRow>();
			ConvertRuleElement convertRuleElement = Common.SPGetConvertRule(view, "ENG_BomExpandBill", view.BusinessInfo.GetForm().Id);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("SelBomBillParam", selBomBillParam);
			ConvertOperationResult convertOperationResult = ConvertServiceHelper.Draw(view.Context, new DrawArgs(convertRuleElement, view.Model.DataObject, source.ToArray<ListSelectedRow>())
			{
				CustomParams = dictionary
			}, null);
			if (!ListUtils.IsEmpty<ValidationErrorInfo>(convertOperationResult.ValidationErrors))
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ValidationErrorInfo validationErrorInfo in convertOperationResult.ValidationErrors)
				{
					if (!string.IsNullOrWhiteSpace(validationErrorInfo.Message))
					{
						stringBuilder.AppendLine(validationErrorInfo.Message);
					}
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder))
				{
					view.ShowNotificationMessage(stringBuilder.ToString(), string.Empty, 1);
				}
			}
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0002794C File Offset: 0x00025B4C
		private static void SetBomExpandRuleFieldMap(Context ctx, IBillView view, ConvertRuleElement bomExpandRule)
		{
			if (bomExpandRule != null)
			{
				bool conLossRate = Common.GetConLossRate(ctx, view);
				if (conLossRate)
				{
					List<DefaultConvertPolicyElement> source = (from w in bomExpandRule.Policies
					where w is DefaultConvertPolicyElement
					select w as DefaultConvertPolicyElement).ToList<DefaultConvertPolicyElement>();
					if (source.Count<DefaultConvertPolicyElement>() >= 1)
					{
						DefaultConvertPolicyElement defaultConvertPolicyElement = source.First<DefaultConvertPolicyElement>();
						foreach (FieldMapElement fieldMapElement in defaultConvertPolicyElement.FieldMaps)
						{
							if (StringUtils.EqualsIgnoreCase(fieldMapElement.TargetFieldKey, "FBaseQty"))
							{
								fieldMapElement.SourceFieldKey = "FBaseQty";
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000331 RID: 817 RVA: 0x00027A2C File Offset: 0x00025C2C
		private static bool GetConLossRate(Context ctx, IBillView view)
		{
			bool result = false;
			string text = "BOS_BillUserParameter";
			if (!string.IsNullOrWhiteSpace(view.BusinessInfo.GetForm().ParameterObjectId))
			{
				text = view.BusinessInfo.GetForm().ParameterObjectId;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, text, true) as FormMetadata;
			if (formMetadata == null)
			{
				return result;
			}
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(ctx, formMetadata.BusinessInfo, ctx.UserId, view.BusinessInfo.GetForm().Id, "UserParameter");
			if (dynamicObject != null && dynamicObject.DynamicObjectType.Properties.ContainsKey("ConLossRate"))
			{
				result = Convert.ToBoolean(dynamicObject["ConLossRate"]);
			}
			return result;
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00027AD0 File Offset: 0x00025CD0
		public static string GetOnlyViewMsg(Context ctx, string operateName)
		{
			string result = "";
			if (LicenseVerifier.IsViewOnlyUser(ctx))
			{
				result = string.Format(ResManager.LoadKDString("用户{0}为仅查询许可用户，不能执行“{1}”操作，请联系系统管理员！", "004023030009239", 5, new object[0]), ctx.UserName, operateName);
			}
			return result;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00027B4C File Offset: 0x00025D4C
		public static void SetCustMatWhenCustChange(AbstractBillPlugIn billPlugIn, string entryKey, string stockOrgKey, string custKey, string custMatKey, string custMatNameKey)
		{
			DynamicObject dynamicObject = billPlugIn.View.Model.GetValue(custKey) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : Convert.ToInt64(dynamicObject["Id"]);
			DynamicObject dynamicObject2 = billPlugIn.View.Model.GetValue(custKey) as DynamicObject;
			long num2 = (dynamicObject2 == null) ? 0L : Convert.ToInt64(dynamicObject2["Id"]);
			Entity entity = billPlugIn.View.BusinessInfo.GetEntity(entryKey);
			DynamicObjectCollection entityDataObject = billPlugIn.View.Model.GetEntityDataObject(entity);
			List<KeyValuePair<int, long>> list = (from p in entityDataObject
			where Convert.ToInt64(p["MaterialId_Id"]) > 0L
			select new KeyValuePair<int, long>(Convert.ToInt32(p["Seq"]), Convert.ToInt64(p["MaterialId_Id"]))).ToList<KeyValuePair<int, long>>();
			if (num2 == 0L || list == null || list.Count<KeyValuePair<int, long>>() == 0)
			{
				return;
			}
			Dictionary<int, CustomerMaterialResult> relativeCodeByCust = CommonServiceHelper.GetRelativeCodeByCust(billPlugIn.Context, list, num, num2);
			billPlugIn.View.Model.BeginIniti();
			for (int i = 0; i < entityDataObject.Count<DynamicObject>(); i++)
			{
				if (Convert.ToInt64(entityDataObject[i]["MaterialId_Id"]) > 0L)
				{
					int key = Convert.ToInt32(entityDataObject[i]["Seq"]);
					CustomerMaterialResult customerMaterialResult = relativeCodeByCust[key];
					DynamicObject dynamicObject3 = billPlugIn.View.Model.GetValue(custMatKey, i) as DynamicObject;
					if (dynamicObject3 != null && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject3["Id"]) && !dynamicObject3.DynamicObjectType.Properties.ContainsKey("CustomerId"))
					{
						IMetaDataService service = ServiceFactory.GetService<IMetaDataService>(billPlugIn.Context);
						IViewService viewService = ServiceFactory.GetViewService(billPlugIn.Context);
						FormMetadata formMetadata = (FormMetadata)service.Load(billPlugIn.Context, "Sal_CustMatMappingView", true);
						dynamicObject3 = viewService.Load(billPlugIn.Context, new object[]
						{
							dynamicObject3["Id"]
						}, formMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
					}
					if (dynamicObject3 == null || Convert.ToInt64(dynamicObject3["CustomerId_Id"]) != 0L || customerMaterialResult == null || customerMaterialResult.FCustId != 0L)
					{
						if (customerMaterialResult == null)
						{
							billPlugIn.View.Model.SetValue(custMatKey, null, i);
						}
						else
						{
							billPlugIn.View.Model.SetValue(custMatKey, customerMaterialResult.Fid, i);
						}
						billPlugIn.View.UpdateView(custMatKey, i);
						billPlugIn.View.UpdateView(custMatNameKey, i);
					}
				}
			}
			billPlugIn.View.Model.EndIniti();
		}

		// Token: 0x06000334 RID: 820 RVA: 0x00027E0C File Offset: 0x0002600C
		public static void SetMaterialIdAndAuxpropIdByCustMatId(AbstractBillPlugIn billPlugin, CustomerMaterialMappingArgs args)
		{
			CustomerMaterialResult customMaterialInfo = CommonServiceHelper.GetCustomMaterialInfo(billPlugin.Context, args);
			if (customMaterialInfo != null)
			{
				DynamicObject dynamicObject = billPlugin.View.Model.GetValue(args.MaterialIdKey, args.Row) as DynamicObject;
				long num = (dynamicObject != null) ? Convert.ToInt64(dynamicObject["id"]) : 0L;
				if (num != customMaterialInfo.FMaterialId)
				{
					billPlugin.View.Model.SetValue(args.MaterialIdKey, (customMaterialInfo.FMaterialId > 0L) ? customMaterialInfo.FMaterialId : 0L, args.Row);
					billPlugin.View.InvokeFieldUpdateService(args.MaterialIdKey, args.Row);
				}
				if (customMaterialInfo.FAuxpropId > 0L)
				{
					billPlugin.View.Model.SetValue(args.AuxpropIdKey, (customMaterialInfo.FAuxpropId > 0L) ? customMaterialInfo.FAuxpropId : 0L, args.Row);
				}
			}
		}

		// Token: 0x06000335 RID: 821 RVA: 0x00027EFC File Offset: 0x000260FC
		public static string GetMapIdFilter(long mainOrgId, long custId, long materialId, Dictionary<string, bool> baseDataNeedOrgCtrl)
		{
			bool flag = false;
			string text = " FEFFECTIVE='1' And ";
			baseDataNeedOrgCtrl.TryGetValue("Sal_CustMatMappingView", out flag);
			text += (flag ? string.Format(" FUseOrgId={0} And ", mainOrgId) : "");
			if (materialId > 0L)
			{
				baseDataNeedOrgCtrl.TryGetValue("BD_Customer", out flag);
				text += string.Format("FMATERIALID={0} AND (FCUSTOMERID=0 Or FCUSTOMERID=(SELECT FMasterId FROM t_bd_Customer WHERE FCustID={1}))", materialId, custId);
			}
			else
			{
				baseDataNeedOrgCtrl.TryGetValue("BD_MATERIAL", out flag);
				if (flag)
				{
					text += string.Format(" FMATERIALID IN ( SELECT TM.FMATERIALID FROM t_bd_Material TM INNER JOIN t_bd_Materialbase TMB ON TM.FMATERIALID = TMB.FMATERIALID WHERE TM.FUseOrgId = {0} AND TMB.FIsInventory='1' ) AND ", mainOrgId);
				}
				baseDataNeedOrgCtrl.TryGetValue("BD_Customer", out flag);
				text += string.Format(" (FCUSTOMERID=0 Or FCUSTOMERID=(SELECT FMasterId FROM t_bd_Customer WHERE FCustID={0}))", custId);
			}
			return text;
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00027FC0 File Offset: 0x000261C0
		public static void SetRelativeCodeByMaterialId(AbstractBillPlugIn billPlugin, string CustMatCodeKey, long materialId, long customerId, long saleOrgId, int row)
		{
			List<CustomerMaterialResult> relativeCodeByMaterial = CommonServiceHelper.GetRelativeCodeByMaterial(billPlugin.Context, materialId, customerId, saleOrgId);
			if (relativeCodeByMaterial.Count > 0)
			{
				string text = "";
				foreach (CustomerMaterialResult customerMaterialResult in relativeCodeByMaterial)
				{
					if (customerMaterialResult.FDefCarry)
					{
						text = customerMaterialResult.Fid;
						break;
					}
				}
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					foreach (CustomerMaterialResult customerMaterialResult2 in relativeCodeByMaterial)
					{
						if (customerMaterialResult2.FCustId > 0L)
						{
							text = customerMaterialResult2.Fid;
							break;
						}
					}
				}
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					text = relativeCodeByMaterial[0].Fid;
				}
				billPlugin.View.Model.SetValue(CustMatCodeKey, text, row);
				return;
			}
			billPlugin.View.Model.SetValue(CustMatCodeKey, null, row);
		}

		// Token: 0x06000337 RID: 823 RVA: 0x000280CC File Offset: 0x000262CC
		public static Dictionary<string, bool> GetSalBaseDataCtrolType(Context ctx)
		{
			Dictionary<string, int> baseDataControlType = BDCommonServiceHelper.GetBaseDataControlType(ctx, new List<string>
			{
				"BD_MATERIAL",
				"BD_Customer",
				"Sal_CustMatMappingView"
			});
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (string key in baseDataControlType.Keys)
			{
				dictionary[key] = (baseDataControlType[key] == 2 || baseDataControlType[key] == 3);
			}
			return dictionary;
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00028170 File Offset: 0x00026370
		public static List<string> GetIntersectList(Dictionary<int, List<string>> lList)
		{
			List<string> list = new List<string>();
			if (lList.Count > 0)
			{
				list = lList.Values.FirstOrDefault<List<string>>();
			}
			foreach (List<string> second in lList.Values)
			{
				list = list.Intersect(second).ToList<string>();
			}
			return list;
		}

		// Token: 0x06000339 RID: 825 RVA: 0x000281E8 File Offset: 0x000263E8
		public static string GetUsefulPurCatalogDocStatusFilter(Context ctx)
		{
			object systemProfile = CommonServiceHelper.GetSystemProfile(ctx, 0L, "PUR_SystemParameter", "FHYQDApproveYX", false);
			bool flag = systemProfile != null && Convert.ToBoolean(systemProfile);
			if (flag)
			{
				return "FDOCUMENTSTATUS='C' ";
			}
			return "FDOCUMENTSTATUS<>'Z' ";
		}

		// Token: 0x04000116 RID: 278
		private static TaxService svc;
	}
}
