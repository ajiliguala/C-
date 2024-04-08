using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core.Business;
using Kingdee.K3.SCM.Core.Business.Args;
using Kingdee.K3.SCM.ServiceHelper;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.StockConsole
{
	// Token: 0x02000035 RID: 53
	public class MinusInvWarnPlugIn : AbstractWarnPlugIn
	{
		// Token: 0x06000221 RID: 545 RVA: 0x0001AE0C File Offset: 0x0001900C
		public override WarnResult GetData(Context ctx, WarnSchemeArgs args)
		{
			long minusInvWarnData = StockWarnServiceHelper.GetMinusInvWarnData(ctx, args);
			WarnResult warnResult = new WarnResult();
			warnResult.LightType = "0";
			if (minusInvWarnData > args.RedLine)
			{
				warnResult.LightType = "1";
			}
			else if (minusInvWarnData > args.YellowLine)
			{
				warnResult.LightType = "2";
			}
			else if (minusInvWarnData >= 0L)
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

		// Token: 0x06000222 RID: 546 RVA: 0x0001AEBC File Offset: 0x000190BC
		public override void DoShowForm(Context ctx, IDynamicFormView view, WarnSchemeArgs args)
		{
			if (args == null || args.QFilterData == null)
			{
				return;
			}
			List<long> allowOrgIds = StockConsoleCommon.GetAllowOrgIds(ctx, args, MinusInvWarnPlugIn.INVFORMID, false);
			if (allowOrgIds.Count < 1)
			{
				return;
			}
			string text = string.Join<long>(",", allowOrgIds);
			string invQueryFilter = this.GetInvQueryFilter(args);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = MinusInvWarnPlugIn.INVFORMID;
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.Caption = ResManager.LoadKDString("即时库存明细", "004023000038718", 5, new object[0]);
			listShowParameter.ParentPageId = view.PageId;
			string pageId = SequentialGuid.NewGuid().ToString();
			listShowParameter.PageId = pageId;
			listShowParameter.IsUseDefaultScheme = true;
			listShowParameter.CustomParams.Add("UseDefaultScheme", "True");
			listShowParameter.CustomParams.Add("IsFromStockQuery", "True");
			listShowParameter.CustomParams.Add("IsFromDetailQuery", "False");
			listShowParameter.CustomParams.Add("IsShowExit", "True");
			listShowParameter.CustomParams.Add("QueryFilter", invQueryFilter);
			listShowParameter.CustomParams.Add("StockOrgIds", text);
			listShowParameter.MutilListUseOrgId = text;
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsLookUp = false;
			view.ShowForm(listShowParameter);
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0001B000 File Offset: 0x00019200
		private string GetInvQueryFilter(WarnSchemeArgs args)
		{
			string text = "";
			if (args.QFilterData["FWarnDataRange"] != null)
			{
				if (args.QFilterData["FWarnDataRange"].ToString() == "2")
				{
					text = string.Format(" FSECQTY < {0} ", Convert.ToDecimal(args.QFilterData["FSecQty"]));
				}
				else
				{
					text = string.Format(" FBASEQTY < {0} ", Convert.ToDecimal(args.QFilterData["FBaseQty"]));
				}
			}
			if (!string.IsNullOrWhiteSpace(args.FilterString))
			{
				text = string.Format(" ({0}) AND ( {1} )", text, args.FilterString);
			}
			return text;
		}

		// Token: 0x040000BC RID: 188
		private static readonly string INVFORMID = "STK_Inventory";
	}
}
