using System;

namespace Newtonsoft.Json.Converters
{
	// Token: 0x0200001C RID: 28
	internal interface IEntityKeyMember
	{
		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000FD RID: 253
		// (set) Token: 0x060000FE RID: 254
		string Key { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000FF RID: 255
		// (set) Token: 0x06000100 RID: 256
		object Value { get; set; }
	}
}
