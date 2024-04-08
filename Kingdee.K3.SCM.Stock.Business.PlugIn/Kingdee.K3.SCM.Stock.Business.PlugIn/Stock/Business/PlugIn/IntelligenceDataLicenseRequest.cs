using System;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000022 RID: 34
	public class IntelligenceDataLicenseRequest
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000150 RID: 336 RVA: 0x00012436 File Offset: 0x00010636
		// (set) Token: 0x06000151 RID: 337 RVA: 0x0001243E File Offset: 0x0001063E
		public string userToken { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000152 RID: 338 RVA: 0x00012447 File Offset: 0x00010647
		// (set) Token: 0x06000153 RID: 339 RVA: 0x0001244F File Offset: 0x0001064F
		public string serverUrl { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000154 RID: 340 RVA: 0x00012458 File Offset: 0x00010658
		// (set) Token: 0x06000155 RID: 341 RVA: 0x00012460 File Offset: 0x00010660
		public string appIds { get; set; }

		// Token: 0x06000156 RID: 342 RVA: 0x00012469 File Offset: 0x00010669
		public IntelligenceDataLicenseRequest()
		{
			this.appIds = string.Format("{0}", AppIdType.AppIdSaftStock);
		}

		// Token: 0x04000076 RID: 118
		public const string PATH_SEGMENT = "/ids/k3cloud/license/info";
	}
}
