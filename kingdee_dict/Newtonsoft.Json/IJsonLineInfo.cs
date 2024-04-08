using System;

namespace Newtonsoft.Json
{
	// Token: 0x02000027 RID: 39
	public interface IJsonLineInfo
	{
		// Token: 0x06000134 RID: 308
		bool HasLineInfo();

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000135 RID: 309
		int LineNumber { get; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000136 RID: 310
		int LinePosition { get; }
	}
}
