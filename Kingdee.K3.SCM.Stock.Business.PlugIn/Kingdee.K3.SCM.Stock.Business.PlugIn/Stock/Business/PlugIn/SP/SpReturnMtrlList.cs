using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.SCM.Business;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn.SP
{
	// Token: 0x02000084 RID: 132
	public class SpReturnMtrlList : AbstractListPlugIn
	{
		// Token: 0x0600065C RID: 1628 RVA: 0x0004D938 File Offset: 0x0004BB38
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			List<ConvertBillElement> list;
			if (e.Bills != null)
			{
				list = (e.Bills as List<ConvertBillElement>);
			}
			else
			{
				list = new List<ConvertBillElement>();
			}
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation == 26 && list.Count > 0)
			{
				ConvertBillElement convertBillElement = list.FirstOrDefault((ConvertBillElement o) => StringUtils.EqualsIgnoreCase(o.FormID, "ENG_BomExpandBill"));
				if (convertBillElement != null)
				{
					list.Remove(convertBillElement);
				}
			}
			e.Bills = list;
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0004D9B8 File Offset: 0x0004BBB8
		public override void PrepareFilterParameter(FilterArgs e)
		{
			string text = string.Empty;
			string text2 = Convert.ToString(e.CustomFilter["OrgList"]);
			text = SCMCommon.GetfilterGroupDataIsolation(this, text2, new BusinessGroupDataIsolationArgs
			{
				OrgIdKey = "FStockOrgID",
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
	}
}
