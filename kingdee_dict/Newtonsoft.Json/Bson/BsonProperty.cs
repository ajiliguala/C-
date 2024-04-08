using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x0200000F RID: 15
	internal class BsonProperty
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000067 RID: 103 RVA: 0x00003B10 File Offset: 0x00001D10
		// (set) Token: 0x06000068 RID: 104 RVA: 0x00003B18 File Offset: 0x00001D18
		public BsonString Name { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00003B21 File Offset: 0x00001D21
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00003B29 File Offset: 0x00001D29
		public BsonToken Value { get; set; }
	}
}
