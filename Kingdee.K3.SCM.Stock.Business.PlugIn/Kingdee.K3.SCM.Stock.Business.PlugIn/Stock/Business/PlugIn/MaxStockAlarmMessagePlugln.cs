using System;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Warn.Message;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.Resource;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000062 RID: 98
	[Description("最大库存预警客户端插件")]
	public class MaxStockAlarmMessagePlugln : AbstractWarnMessagePlugIn
	{
		// Token: 0x06000451 RID: 1105 RVA: 0x000338FC File Offset: 0x00031AFC
		public override void ProcessWarnMessage(ProcessWarnMessageEventArgs e)
		{
			if (e.MsgDataKeyValueList != null && e.MsgDataKeyValueList.Count<WarnMessageDataKeyValue>() > 0)
			{
				base.ParentView.ShowMessage(string.Format(ResManager.LoadKDString("无相关业务需要处理!", "004023030005984", 5, new object[0]), new object[0]), 0);
			}
			base.ProcessWarnMessage(e);
		}
	}
}
