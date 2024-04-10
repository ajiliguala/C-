using System;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000017 RID: 23
	public class ResponseInfo
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00009275 File Offset: 0x00007475
		// (set) Token: 0x060000AA RID: 170 RVA: 0x0000927D File Offset: 0x0000747D
		public string status { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00009286 File Offset: 0x00007486
		// (set) Token: 0x060000AC RID: 172 RVA: 0x0000928E File Offset: 0x0000748E
		public string message { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00009297 File Offset: 0x00007497
		// (set) Token: 0x060000AE RID: 174 RVA: 0x0000929F File Offset: 0x0000749F
		public string data { get; set; }
	}
}
