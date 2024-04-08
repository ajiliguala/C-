using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000009 RID: 9
	internal abstract class BsonToken
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000049 RID: 73
		public abstract BsonType Type { get; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00003995 File Offset: 0x00001B95
		// (set) Token: 0x0600004B RID: 75 RVA: 0x0000399D File Offset: 0x00001B9D
		public BsonToken Parent { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600004C RID: 76 RVA: 0x000039A6 File Offset: 0x00001BA6
		// (set) Token: 0x0600004D RID: 77 RVA: 0x000039AE File Offset: 0x00001BAE
		public int CalculatedSize { get; set; }
	}
}
