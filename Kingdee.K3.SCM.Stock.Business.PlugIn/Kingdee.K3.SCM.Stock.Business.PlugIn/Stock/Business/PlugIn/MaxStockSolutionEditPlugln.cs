using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.JSON;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000063 RID: 99
	[Description("最大库存方案列表插件")]
	public class MaxStockSolutionEditPlugln : AbstractWarnSolutionEditPlugIn
	{
		// Token: 0x06000453 RID: 1107 RVA: 0x0003395C File Offset: 0x00031B5C
		public override void BeforeSetFilterFields(BeforeSetFilterFieldsEventArgs e)
		{
			for (int i = e.FilterFields.Count - 1; i >= 0; i--)
			{
				JSONObject jsonobject = e.FilterFields[i] as JSONObject;
				if (jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECEQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECERATE" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERGOOD" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAFESTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINSTOCK" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAVEDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERDIFFERECESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FREORDERGOODSTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FSAFESTUNITQTY" || jsonobject["FieldName"].ToString().ToUpperInvariant() == "FMINSTUNITQTY")
				{
					e.FilterFields.RemoveAt(i);
				}
			}
			base.BeforeSetFilterFields(e);
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x00033BBC File Offset: 0x00031DBC
		public override void BeforeSetVeriableList(BeforeSetVeriableListEventArgs e)
		{
			for (int i = e.Veriablelist.Count - 1; i >= 0; i--)
			{
				WarnMessageVeriable warnMessageVeriable = e.Veriablelist[i];
				if (warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERGOOD" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAFESTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINSTOCK" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECEQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECERATE" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAVEDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERDIFFERECESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FREORDERGOODSTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FSAFESTUNITQTY" || warnMessageVeriable.FiledName.ToUpperInvariant() == "FMINSTUNITQTY")
				{
					e.Veriablelist.RemoveAt(i);
				}
			}
			base.BeforeSetVeriableList(e);
		}
	}
}
