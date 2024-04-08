using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.JSON;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x0200007D RID: 125
	[Description("再订货点方案列表插件")]
	public class ReorderPointSolutionEditPlugln : AbstractWarnSolutionEditPlugIn
	{
		// Token: 0x060005B1 RID: 1457 RVA: 0x00045AD8 File Offset: 0x00043CD8
		public override void BeforeSetFilterFields(BeforeSetFilterFieldsEventArgs e)
		{
			for (int i = e.FilterFields.Count - 1; i >= 0; i--)
			{
				JSONObject jsonobject = e.FilterFields[i] as JSONObject;
				if (jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAFESTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXSTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINSTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAFESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMAXSTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINSTUNITQTY")
				{
					e.FilterFields.RemoveAt(i);
				}
			}
			base.BeforeSetFilterFields(e);
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x00045D38 File Offset: 0x00043F38
		public override void BeforeSetVeriableList(BeforeSetVeriableListEventArgs e)
		{
			for (int i = e.Veriablelist.Count - 1; i >= 0; i--)
			{
				WarnMessageVeriable warnMessageVeriable = e.Veriablelist[i];
				if (warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAFESTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXSTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINSTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAFESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMAXSTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINSTUNITQTY")
				{
					e.Veriablelist.RemoveAt(i);
				}
			}
		}
	}
}
