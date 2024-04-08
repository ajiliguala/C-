using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000002 RID: 2
	internal enum BsonBinaryType : byte
	{
		// Token: 0x04000002 RID: 2
		Binary,
		// Token: 0x04000003 RID: 3
		Function,
		// Token: 0x04000004 RID: 4
		[Obsolete("This type has been deprecated in the BSON specification. Use Binary instead.")]
		Data,
		// Token: 0x04000005 RID: 5
		Uuid,
		// Token: 0x04000006 RID: 6
		Md5 = 5,
		// Token: 0x04000007 RID: 7
		UserDefined = 128
	}
}
