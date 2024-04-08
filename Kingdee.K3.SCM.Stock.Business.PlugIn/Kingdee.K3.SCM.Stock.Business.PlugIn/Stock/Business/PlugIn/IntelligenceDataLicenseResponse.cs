using System;
using System.Collections.Generic;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000024 RID: 36
	public class IntelligenceDataLicenseResponse
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000159 RID: 345 RVA: 0x000124E3 File Offset: 0x000106E3
		// (set) Token: 0x0600015A RID: 346 RVA: 0x000124EB File Offset: 0x000106EB
		public int errcode { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600015B RID: 347 RVA: 0x000124F4 File Offset: 0x000106F4
		// (set) Token: 0x0600015C RID: 348 RVA: 0x000124FC File Offset: 0x000106FC
		public string description { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600015D RID: 349 RVA: 0x00012505 File Offset: 0x00010705
		// (set) Token: 0x0600015E RID: 350 RVA: 0x0001250D File Offset: 0x0001070D
		public string descriptionCn { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600015F RID: 351 RVA: 0x00012516 File Offset: 0x00010716
		// (set) Token: 0x06000160 RID: 352 RVA: 0x0001251E File Offset: 0x0001071E
		public List<IntelligenceDataLicenseResponsetInnerData> data { get; set; }

		// Token: 0x06000161 RID: 353 RVA: 0x00012527 File Offset: 0x00010727
		public IntelligenceDataLicenseResponse()
		{
			this.data = new List<IntelligenceDataLicenseResponsetInnerData>();
		}
	}
}
