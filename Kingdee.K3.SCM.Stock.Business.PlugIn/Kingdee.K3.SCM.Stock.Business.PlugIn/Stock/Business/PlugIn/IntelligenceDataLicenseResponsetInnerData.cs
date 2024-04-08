using System;

namespace Kingdee.K3.SCM.Stock.Business.PlugIn
{
	// Token: 0x02000025 RID: 37
	public class IntelligenceDataLicenseResponsetInnerData
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000162 RID: 354 RVA: 0x0001253A File Offset: 0x0001073A
		// (set) Token: 0x06000163 RID: 355 RVA: 0x00012542 File Offset: 0x00010742
		public string appId { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000164 RID: 356 RVA: 0x0001254B File Offset: 0x0001074B
		// (set) Token: 0x06000165 RID: 357 RVA: 0x00012553 File Offset: 0x00010753
		public int trialStatus { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000166 RID: 358 RVA: 0x0001255C File Offset: 0x0001075C
		// (set) Token: 0x06000167 RID: 359 RVA: 0x00012564 File Offset: 0x00010764
		public string trialExpireDate { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000168 RID: 360 RVA: 0x0001256D File Offset: 0x0001076D
		// (set) Token: 0x06000169 RID: 361 RVA: 0x00012575 File Offset: 0x00010775
		public int trialRemainingDays { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600016A RID: 362 RVA: 0x0001257E File Offset: 0x0001077E
		// (set) Token: 0x0600016B RID: 363 RVA: 0x00012586 File Offset: 0x00010786
		public int status { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600016C RID: 364 RVA: 0x0001258F File Offset: 0x0001078F
		// (set) Token: 0x0600016D RID: 365 RVA: 0x00012597 File Offset: 0x00010797
		public bool isbought { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600016E RID: 366 RVA: 0x000125A0 File Offset: 0x000107A0
		// (set) Token: 0x0600016F RID: 367 RVA: 0x000125A8 File Offset: 0x000107A8
		public int remainingLicenseDays { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000170 RID: 368 RVA: 0x000125B1 File Offset: 0x000107B1
		// (set) Token: 0x06000171 RID: 369 RVA: 0x000125B9 File Offset: 0x000107B9
		public string expireDate { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000172 RID: 370 RVA: 0x000125C2 File Offset: 0x000107C2
		// (set) Token: 0x06000173 RID: 371 RVA: 0x000125CA File Offset: 0x000107CA
		public int enterpriseType { get; set; }

		// Token: 0x04000085 RID: 133
		public const string DateFormat = "yyyy-MM-dd";
	}
}
