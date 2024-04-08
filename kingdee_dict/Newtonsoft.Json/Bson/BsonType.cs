using System;

namespace Newtonsoft.Json.Bson
{
	// Token: 0x02000010 RID: 16
	internal enum BsonType : sbyte
	{
		// Token: 0x0400004C RID: 76
		Number = 1,
		// Token: 0x0400004D RID: 77
		String,
		// Token: 0x0400004E RID: 78
		Object,
		// Token: 0x0400004F RID: 79
		Array,
		// Token: 0x04000050 RID: 80
		Binary,
		// Token: 0x04000051 RID: 81
		Undefined,
		// Token: 0x04000052 RID: 82
		Oid,
		// Token: 0x04000053 RID: 83
		Boolean,
		// Token: 0x04000054 RID: 84
		Date,
		// Token: 0x04000055 RID: 85
		Null,
		// Token: 0x04000056 RID: 86
		Regex,
		// Token: 0x04000057 RID: 87
		Reference,
		// Token: 0x04000058 RID: 88
		Code,
		// Token: 0x04000059 RID: 89
		Symbol,
		// Token: 0x0400005A RID: 90
		CodeWScope,
		// Token: 0x0400005B RID: 91
		Integer,
		// Token: 0x0400005C RID: 92
		TimeStamp,
		// Token: 0x0400005D RID: 93
		Long,
		// Token: 0x0400005E RID: 94
		MinKey = -1,
		// Token: 0x0400005F RID: 95
		MaxKey = 127
	}
}
