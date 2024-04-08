using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x0200000D RID: 13
	internal class BsonString : BsonValue
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00003A95 File Offset: 0x00001C95
		// (set) Token: 0x0600005D RID: 93 RVA: 0x00003A9D File Offset: 0x00001C9D
		public int ByteCount { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600005E RID: 94 RVA: 0x00003AA6 File Offset: 0x00001CA6
		// (set) Token: 0x0600005F RID: 95 RVA: 0x00003AAE File Offset: 0x00001CAE
		public bool IncludeLength { get; set; }

		// Token: 0x06000060 RID: 96 RVA: 0x00003AB7 File Offset: 0x00001CB7
		public BsonString(object value, bool includeLength) : base(value, BsonType.String)
		{
			this.IncludeLength = includeLength;
		}
	}
}
