using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000014 RID: 20
	public class BsonObjectId
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00004E3F File Offset: 0x0000303F
		// (set) Token: 0x060000DD RID: 221 RVA: 0x00004E47 File Offset: 0x00003047
		public byte[] Value { get; private set; }

		// Token: 0x060000DE RID: 222 RVA: 0x00004E50 File Offset: 0x00003050
		public BsonObjectId(byte[] value)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value.Length != 12)
			{
				throw new Exception("An ObjectId must be 12 bytes");
			}
			this.Value = value;
		}
	}
}
