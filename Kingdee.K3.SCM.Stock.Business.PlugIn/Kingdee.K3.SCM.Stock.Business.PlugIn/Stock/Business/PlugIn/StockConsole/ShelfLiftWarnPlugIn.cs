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
	// Token: 0x0200003A RID: 58
	public class ShelfLiftWarnPlugIn : AbstractWarnPlugIn
	{
		// Token: 0x06000241 RID: 577 RVA: 0x0001BA68 File Offset: 0x00019C68
		public override WarnResult GetData(Context ctx, WarnSchemeArgs args)
		{
			base.GetData(ctx, args);
			long shelfLiftWarnCount = StockWarnServiceHelper.GetShelfLiftWarnCount(ctx, args);
			WarnResult warnResult = new WarnResult();
			warnResult.LightType = "0";
			if (shelfLiftWarnCount > args.RedLine)
			{
				warnResult.LightType = "1";
			}
			else if (shelfLiftWarnCount > args.YellowLine)
			{
				warnResult.LightType = "2";
			}
			else if (shelfLiftWarnCount >= 0L)
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

		// Token: 0x06000242 RID: 578 RVA: 0x0001BB20 File Offset: 0x00019D20
		public override void DoShowForm(Context ctx, IDynamicFormView view, WarnSchemeArgs args)
		{
			base.DoShowForm(ctx, view, args);
			if (args == null || args.QFilterData == null)
			{
				return;
			}
			List<long> allowOrgIds = StockConsoleCommon.GetAllowOrgIds(ctx, args, ShelfLiftWarnPlugIn.FormId, true);
			if (allowOrgIds.Count < 1)
			{
				return;
			}
			view.ShowForm(new MoveReportShowParameter
			{
				FormId = ShelfLiftWarnPlugIn.FormId,
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
						ShelfLiftWarnPlugIn.SchemeId
					},
					{
						"DefaultSelectSchemeID",
						ShelfLiftWarnPlugIn.SchemeId
					},
					{
						"SourceBillFormId",
						"ShelfLiftWarnConsole"
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

		// Token: 0x040000C5 RID: 197
		private static readonly string FormId = "STK_ShelfLiftAlarmRpt";

		// Token: 0x040000C6 RID: 198
		private static readonly string SchemeId = "0050569448a4bad211e30ef33ea3be60";
	}
}
