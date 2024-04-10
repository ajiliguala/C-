using System;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000005 RID: 5
	public class OperationMessage
	{
		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00003D8A File Offset: 0x00001F8A
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00003D92 File Offset: 0x00001F92
		public object BillPKValue { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00003D9B File Offset: 0x00001F9B
		// (set) Token: 0x06000058 RID: 88 RVA: 0x00003DA3 File Offset: 0x00001FA3
		public string BillNumber { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00003DAC File Offset: 0x00001FAC
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00003DB4 File Offset: 0x00001FB4
		public string Title { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00003DBD File Offset: 0x00001FBD
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00003DC5 File Offset: 0x00001FC5
		public bool State { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003DCE File Offset: 0x00001FCE
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00003DD6 File Offset: 0x00001FD6
		public string Message { get; set; }
	}
}
