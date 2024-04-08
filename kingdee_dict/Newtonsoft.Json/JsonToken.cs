using System;

namespace Newtonsoft.Json
{
	// Token: 0x020000C1 RID: 193
	public enum JsonToken
	{
		// Token: 0x04000287 RID: 647
		None,
		// Token: 0x04000288 RID: 648
		StartObject,
		// Token: 0x04000289 RID: 649
		StartArray,
		// Token: 0x0400028A RID: 650
		StartConstructor,
		// Token: 0x0400028B RID: 651
		PropertyName,
		// Token: 0x0400028C RID: 652
		Comment,
		// Token: 0x0400028D RID: 653
		Raw,
		// Token: 0x0400028E RID: 654
		Integer,
		// Token: 0x0400028F RID: 655
		Float,
		// Token: 0x04000290 RID: 656
		String,
		// Token: 0x04000291 RID: 657
		Boolean,
		// Token: 0x04000292 RID: 658
		Null,
		// Token: 0x04000293 RID: 659
		Undefined,
		// Token: 0x04000294 RID: 660
		EndObject,
		// Token: 0x04000295 RID: 661
		EndArray,
		// Token: 0x04000296 RID: 662
		EndConstructor,
		// Token: 0x04000297 RID: 663
		Date,
		// Token: 0x04000298 RID: 664
		Bytes
	}
}
