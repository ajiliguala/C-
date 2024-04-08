using System;

namespace Newtonsoft.Json.Schema
{
	// Token: 0x02000090 RID: 144
	[Flags]
	public enum JsonSchemaType
	{
		// Token: 0x04000214 RID: 532
		None = 0,
		// Token: 0x04000215 RID: 533
		String = 1,
		// Token: 0x04000216 RID: 534
		Float = 2,
		// Token: 0x04000217 RID: 535
		Integer = 4,
		// Token: 0x04000218 RID: 536
		Boolean = 8,
		// Token: 0x04000219 RID: 537
		Object = 16,
		// Token: 0x0400021A RID: 538
		Array = 32,
		// Token: 0x0400021B RID: 539
		Null = 64,
		// Token: 0x0400021C RID: 540
		Any = 127
	}
}
