using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x0200000E RID: 14
	internal class BsonRegex : BsonToken
	{
		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00003AC8 File Offset: 0x00001CC8
		// (set) Token: 0x06000062 RID: 98 RVA: 0x00003AD0 File Offset: 0x00001CD0
		public BsonString Pattern { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000063 RID: 99 RVA: 0x00003AD9 File Offset: 0x00001CD9
		// (set) Token: 0x06000064 RID: 100 RVA: 0x00003AE1 File Offset: 0x00001CE1
		public BsonString Options { get; set; }

		// Token: 0x06000065 RID: 101 RVA: 0x00003AEA File Offset: 0x00001CEA
		public BsonRegex(string pattern, string options)
		{
			this.Pattern = new BsonString(pattern, false);
			this.Options = new BsonString(options, false);
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00003B0C File Offset: 0x00001D0C
		public override BsonType Type
		{
			get
			{
				return BsonType.Regex;
			}
		}
	}
}
