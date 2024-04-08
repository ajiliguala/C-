using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core.Business;
using Kingdee.K3.SCM.Core.Business.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x0200003D RID: 61
	public class StockNegativeWarnPlugIn : AbstractWarnPlugIn
	{
		// Token: 0x06000251 RID: 593 RVA: 0x0001C18C File Offset: 0x0001A38C
		public override WarnResult GetData(Context ctx, WarnSchemeArgs args)
		{
			base.GetData(ctx, args);
			string dataTime = "";
			long negativeWarnCount = StockWarnServiceHelper.GetNegativeWarnCount(ctx, args, ref dataTime);
			WarnResult warnResult = new WarnResult();
			warnResult.LightType = "0";
			if (negativeWarnCount > args.RedLine)
			{
				warnResult.LightType = "1";
			}
			else if (negativeWarnCount > args.YellowLine)
			{
				warnResult.LightType = "2";
			}
			else if (negativeWarnCount >= 0L)
			{
				warnResult.LightType = "3";
			}
			if (negativeWarnCount == -2L)
			{
				dataTime = ResManager.LoadKDString("服务未启动", "004023000038830", 5, new object[0]);
			}
			warnResult.DataTime = dataTime;
			return warnResult;
		}

		// Token: 0x06000252 RID: 594 RVA: 0x0001C238 File Offset: 0x0001A438
		public override void DoShowForm(Context ctx, IDynamicFormView view, WarnSchemeArgs args)
		{
			base.DoShowForm(ctx, view, args);
			MoveReportShowParameter moveReportShowParameter = new MoveReportShowParameter
			{
				PageId = SequentialGuid.NewGuid().ToString(),
				ParentPageId = "StockNegativeWarmBGService",
				FormId = "STK_StockNegativeRpt",
				MultiSelect = false,
				IsShowFilter = false
			};
			moveReportShowParameter.OpenStyle.ShowType = 7;
			long num;
			long.TryParse(args.WarnSchemeId, out num);
			if (num <= 0L)
			{
				throw new Exception(ResManager.LoadKDString("未找到当前负结余卡片对应的预警方案.", "004023000038311", 5, new object[0]));
			}
			JSONObject qfilterData = args.QFilterData;
			if (qfilterData == null)
			{
				throw new Exception(ResManager.LoadKDString("当前负结余卡片对应的预警方案快捷过滤设置异常.", "004023000038310", 5, new object[0]));
			}
			List<long> allowOrgIds = StockConsoleCommon.GetAllowOrgIds(ctx, args, "STK_StockNegativeRpt", true);
			string dateRange = qfilterData["FDateRange"].ToString();
			DateTime dateTime;
			DateTime orgStartDate = this.GetOrgStartDate(ctx, dateRange, allowOrgIds, out dateTime);
			if (orgStartDate == DateTime.MinValue)
			{
				throw new Exception(ResManager.LoadKDString("未找到当前登录组织的最后关账日期或库存启用日期.", "004023000038312", 5, new object[0]));
			}
			string defaultSchemeByFID = CommonServiceHelper.GetDefaultSchemeByFID(ctx, "STK_StockNegativeRpt");
			moveReportShowParameter.CustomParams.Add("filterschemeid", defaultSchemeByFID);
			moveReportShowParameter.CustomParams.Add("DefaultSelectSchemeID", defaultSchemeByFID);
			moveReportShowParameter.CustomParams.Add("SrcBillFormId", "StockNegativeWarmBGService");
			moveReportShowParameter.CustomParams.Add("OrgId", string.Join<long>(",", allowOrgIds));
			moveReportShowParameter.CustomParams.Add("BeginDate", orgStartDate.ToShortDateString());
			moveReportShowParameter.CustomParams.Add("EndDate", dateTime.ToShortDateString());
			moveReportShowParameter.CustomParams.Add("NegativeWarnId", "");
			moveReportShowParameter.CustomParams.Add("WarnSchemeId", args.WarnSchemeId);
			moveReportShowParameter.CustomParams.Add("QFilterData", (args.QFilterData == null) ? " " : args.QFilterData.ToJSONString());
			moveReportShowParameter.CustomParams.Add("ScheduleId", "");
			moveReportShowParameter.CustomParams.Add("WarnField", args.WarnField);
			moveReportShowParameter.CustomParams.Add("FilterString", args.FilterString);
			moveReportShowParameter.CustomParams.Add("DateOrderType", qfilterData["FDateOrderType"].ToString());
			moveReportShowParameter.CustomParams.Add("ShowForbidMaterial", qfilterData["FShowForbidMaterial"].ToString());
			moveReportShowParameter.CustomParams.Add("SplitPageByOrg", qfilterData["FSplitPageByOrg"].ToString());
			moveReportShowParameter.CustomParams.Add("SplitPageByOwner", qfilterData["FSplitPageByOwner"].ToString());
			view.ShowForm(moveReportShowParameter);
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0001C500 File Offset: 0x0001A700
		public DateTime GetOrgStartDate(Context ctx, string dateRange, List<long> stockOrgIds, out DateTime endTime)
		{
			endTime = TimeServiceHelper.GetSystemDateTime(ctx);
			DateTime result = DateTime.MinValue;
			if (dateRange != null)
			{
				if (!(dateRange == "1"))
				{
					if (!(dateRange == "2"))
					{
						if (!(dateRange == "3"))
						{
							if (dateRange == "4")
							{
								result = StockWarnServiceHelper.GetStockOrgAcctLastCloseDate(ctx, stockOrgIds);
							}
						}
						else
						{
							result = Convert.ToDateTime(endTime.AddDays((double)(1 - endTime.Day)).ToString("yyyy-MM-dd"));
						}
					}
					else
					{
						result = Convert.ToDateTime(endTime.AddDays((double)(-(double)Convert.ToInt32(endTime.DayOfWeek.ToString("d")))).ToString("yyyy-MM-dd"));
					}
				}
				else
				{
					result = endTime;
				}
			}
			return result;
		}

		// Token: 0x040000CB RID: 203
		private const string SrcBillFormId = "StockNegativeWarmBGService";

		// Token: 0x040000CC RID: 204
		private const string FormId = "STK_StockNegativeRpt";
	}
}
