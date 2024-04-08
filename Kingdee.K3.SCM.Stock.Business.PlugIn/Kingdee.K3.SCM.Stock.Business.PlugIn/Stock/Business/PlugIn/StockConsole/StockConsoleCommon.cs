using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.SCM.Core.Business.Args;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x0200003B RID: 59
	public class StockConsoleCommon
	{
		// Token: 0x06000245 RID: 581 RVA: 0x0001BCA8 File Offset: 0x00019EA8
		public static List<long> GetAllowOrgIds(Context ctx, WarnSchemeArgs args, string sFormId, bool filterStockOrg = false)
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = sFormId,
				PermissionControl = 1,
				SubSystemId = "STK"
			};
			List<long> list = PermissionServiceHelper.GetPermissionOrg(ctx, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (list.Count < 1)
			{
				return list;
			}
			if (filterStockOrg)
			{
				list = StockConsoleCommon.GetOrganization(ctx, list);
			}
			string text = "";
			if (args.QFilterData["FStockOrgId"] != null)
			{
				text = args.QFilterData["FStockOrgId"].ToString();
			}
			if (!Convert.ToBoolean(args.QFilterData["FIsAllOrg"]))
			{
				List<long> list2 = new List<long>();
				if (!string.IsNullOrWhiteSpace(text))
				{
					string[] source = text.Split(new char[]
					{
						','
					}, StringSplitOptions.RemoveEmptyEntries);
					long[] source2 = (from p in source
					select Convert.ToInt64(p)).ToArray<long>();
					using (List<long>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							long orgId = enumerator.Current;
							if (source2.Any((long p) => p == orgId))
							{
								list2.Add(orgId);
							}
						}
					}
				}
				return list2;
			}
			return list;
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0001BE1C File Offset: 0x0001A01C
		protected static List<long> GetOrganization(Context ctx, List<long> orgList)
		{
			List<long> list = new List<long>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FORGID"));
			list2.Add(new SelectorItemInfo("FNUMBER"));
			list2.Add(new SelectorItemInfo("FNAME"));
			string text = StockConsoleCommon.GetInFilter("FORGID", orgList);
			text += string.Format(" AND FORGFUNCTIONS LIKE '%{0}%' ", 103L.ToString());
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list2,
				FilterClauseWihtKey = text
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryBuilderParemeter, null);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				long item = (long)dynamicObject["FORGID"];
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x06000247 RID: 583 RVA: 0x0001BF20 File Offset: 0x0001A120
		protected static string GetInFilter(string key, List<long> valList)
		{
			if (valList == null || valList.Count<long>() == 0)
			{
				return string.Format("{0} = -1 ", key);
			}
			return string.Format("{0} IN ({1})", key, string.Join<long>(",", valList));
		}
	}
}
