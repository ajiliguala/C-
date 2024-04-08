using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.JSON;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x020000AA RID: 170
	[Description("保质期预警方案列表插件")]
	public class WarnSelfLifeAlarmSolutionEditPlugIn : AbstractWarnSolutionEditPlugIn
	{
		// Token: 0x06000A73 RID: 2675 RVA: 0x0008F364 File Offset: 0x0008D564
		public override void BeforeSetVeriableList(BeforeSetVeriableListEventArgs e)
		{
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x0008F368 File Offset: 0x0008D568
		public override void BeforeSetFilterFields(BeforeSetFilterFieldsEventArgs e)
		{
			e.AddFilterField("FBaseQty");
			for (int i = e.FilterFields.Count - 1; i >= 0; i--)
			{
				JSONObject jsonobject = e.FilterFields[i] as JSONObject;
				if (jsonobject["FieldName"].ToString().ToUpperInvariant() == "FQTY")
				{
					e.FilterFields.RemoveAt(i);
				}
			}
			base.BeforeSetFilterFields(e);
		}
	}
}
