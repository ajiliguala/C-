using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core.Business;
using Kingdee.K3.SCM.Core.Business.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x02000038 RID: 56
	public class SafeStockWarnPlugIn : AbstractWarnPlugIn
	{
		// Token: 0x06000234 RID: 564 RVA: 0x0001B590 File Offset: 0x00019790
		public override WarnResult GetData(Context ctx, WarnSchemeArgs args)
		{
			base.GetData(ctx, args);
			long safeStockWarnCount = StockWarnServiceHelper.GetSafeStockWarnCount(ctx, args);
			WarnResult warnResult = new WarnResult();
			warnResult.LightType = "0";
			if (safeStockWarnCount > args.RedLine)
			{
				warnResult.LightType = "1";
			}
			else if (safeStockWarnCount > args.YellowLine)
			{
				warnResult.LightType = "2";
			}
			else if (safeStockWarnCount >= 0L)
			{
				warnResult.LightType = "3";
			}
			if (warnResult.LightType == "0")
			{
				warnResult.DataTime = "";
			}
			else
			{
				warnResult.DataTime = TimeServiceHelper.GetSystemDateTime(ctx).ToString("MM.dd HH:mm");
			}
			return warnResult;
		}

		// Token: 0x06000235 RID: 565 RVA: 0x0001B648 File Offset: 0x00019848
		public override void DoShowForm(Context ctx, IDynamicFormView view, WarnSchemeArgs args)
		{
			base.DoShowForm(ctx, view, args);
			if (args == null || args.QFilterData == null)
			{
				return;
			}
			List<long> allowOrgIds = StockConsoleCommon.GetAllowOrgIds(ctx, args, SafeStockWarnPlugIn.FormId, true);
			if (allowOrgIds.Count < 1)
			{
				return;
			}
			view.ShowForm(new SysReportShowParameter
			{
				FormId = SafeStockWarnPlugIn.FormId,
				ParentPageId = view.PageId,
				OpenStyle = 
				{
					ShowType = 7
				},
				ParentPageId = view.PageId,
				PageId = SequentialGuid.NewGuid().ToString(),
				IsShowFilter = false,
				MultiSelect = false,
				CustomParams = 
				{
					{
						"filterschemeid",
						SafeStockWarnPlugIn.SchemeId
					},
					{
						"DefaultSelectSchemeID",
						SafeStockWarnPlugIn.SchemeId
					},
					{
						"SourceBillFormId",
						"SafeStockWarnConsole"
					},
					{
						"SourceWarnId",
						args.WarnSchemeId
					},
					{
						"SourceOrgId",
						string.Join<long>(",", allowOrgIds)
					},
					{
						"SourceFilter",
						args.FilterString
					},
					{
						"SourceQFilter",
						(args.QFilterData == null) ? " " : args.QFilterData.ToJSONString()
					}
				}
			});
		}

		// Token: 0x040000C1 RID: 193
		private static readonly string FormId = "STK_WarnSafeStockRpt";

		// Token: 0x040000C2 RID: 194
		private static readonly string SchemeId = "54ddc39bce75b7";
	}
}
