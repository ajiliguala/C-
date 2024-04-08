using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.JSON;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000065 RID: 101
	[Description("最小库存方案列表插件")]
	public class MinStockSolutionEditPlugln : AbstractWarnSolutionEditPlugIn
	{
		// Token: 0x0600045B RID: 1115 RVA: 0x00033E8C File Offset: 0x0003208C
		public override void BeforeSetFilterFields(BeforeSetFilterFieldsEventArgs e)
		{
			for (int i = e.FilterFields.Count - 1; i >= 0; i--)
			{
				JSONObject jsonobject = e.FilterFields[i] as JSONObject;
				if (jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERGOOD" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXSTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAFESTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERGOODSTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXSTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAFESTUNITQTY")
				{
					e.FilterFields.RemoveAt(i);
				}
			}
			base.BeforeSetFilterFields(e);
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x000340EC File Offset: 0x000322EC
		public override void BeforeSetVeriableList(BeforeSetVeriableListEventArgs e)
		{
			for (int i = e.Veriablelist.Count - 1; i >= 0; i--)
			{
				WarnMessageVeriable warnMessageVeriable = e.Veriablelist[i];
				if (warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERGOOD" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXSTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAFESTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERGOODSTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXSTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAFESTUNITQTY")
				{
					e.Veriablelist.RemoveAt(i);
				}
			}
			base.BeforeSetVeriableList(e);
		}
	}
}
