using System;
using System.Collections;

namespace Newtonsoft.Json.Utilities
{
	// Token: 0x020000C6 RID: 198
	internal interface IWrappedList : IList, ICollection, IEnumerable
	{
		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000882 RID: 2178
		object UnderlyingList { get; }
	}
}
