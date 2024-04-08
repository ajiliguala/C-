using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.JSON;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200007F RID: 127
	[Description("安全库存方案列表插件")]
	public class SafetyStockSolutionEditPlugIn : AbstractWarnSolutionEditPlugIn
	{
		// Token: 0x060005B9 RID: 1465 RVA: 0x00046004 File Offset: 0x00044204
		public override void BeforeSetFilterFields(BeforeSetFilterFieldsEventArgs e)
		{
			for (int i = e.FilterFields.Count - 1; i >= 0; i--)
			{
				JSONObject jsonobject = e.FilterFields[i] as JSONObject;
				if (jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERGOOD" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXSTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINSTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERGOODSTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXSTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINSTUNITQTY")
				{
					e.FilterFields.RemoveAt(i);
				}
			}
			base.BeforeSetFilterFields(e);
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x00046264 File Offset: 0x00044464
		public override void BeforeSetVeriableList(BeforeSetVeriableListEventArgs e)
		{
			for (int i = e.Veriablelist.Count - 1; i >= 0; i--)
			{
				WarnMessageVeriable warnMessageVeriable = e.Veriablelist[i];
				if (warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERGOOD" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXSTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINSTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERGOODSTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXSTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINSTUNITQTY")
				{
					e.Veriablelist.RemoveAt(i);
				}
			}
			base.BeforeSetVeriableList(e);
		}
	}
}
